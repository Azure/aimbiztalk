// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert.GeneratorRules
{
    /// <summary>
    /// Defines a class that implements a converter to generate send routing slips.
    /// </summary>
    public sealed class AP002SendRoutingSlipGenerator : BizTalkConverterBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "AP002";

        /// <summary>
        /// Defines a file repository.
        /// </summary>
        private readonly IFileRepository _fileRepository;

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// Defines a scenario route walker.
        /// </summary>
        private readonly IScenarioRouteWalker _routeWalker;

        /// <summary>
        /// Creates a new instance of a <see cref="AP002SendRoutingSlipGenerator"/> class.
        /// </summary>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="routeWalker">The scenario route walker.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public AP002SendRoutingSlipGenerator(IFileRepository fileRepository, IScenarioRouteWalker routeWalker, IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(AP002SendRoutingSlipGenerator), model, context, logger)
        {
            // Validate and set the member.
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            _routeWalker = routeWalker ?? throw new ArgumentNullException(nameof(routeWalker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate routing slip configuration files for each activator intermediary.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "The routing slip configuration file name is expected to be lowercase")]
        protected override async Task ConvertInternalAsync(CancellationToken token)
        {
            if (Model.MigrationTarget.MessageBus?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsMigrationTargetMessageBusMissing, RuleName, nameof(AP002SendRoutingSlipGenerator));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningGenerator, RuleName, nameof(AP002SendRoutingSlipGenerator));

                // Get all of the intermediaries and endpoints from the migration target.
                var intermediaries = Model.MigrationTarget.MessageBus.Applications.SelectMany(a => a.Intermediaries);
                var channels = Model.MigrationTarget.MessageBus.Applications.SelectMany(a => a.Channels);
                var endpoints = Model.MigrationTarget.MessageBus.Applications.SelectMany(a => a.Endpoints);

                foreach (var targetApplication in Model.MigrationTarget.MessageBus.Applications)
                {
                    // Loop through all of the activating intermediaries (that are topic subscribers).
                    foreach (var activatingIntermediary in targetApplication.Intermediaries.Where(i => i.Activator == true && i is MessageSubscriber && !string.IsNullOrEmpty(i.ResourceMapKey)))
                    {
                        // Get scenario name
                        var scenarioName = activatingIntermediary.Properties[ModelConstants.ScenarioName].ToString();

                        // Walk the routing objects starting at the activating intermediary.
                        var routingObjects = _routeWalker.WalkSendRoute(RuleName, scenarioName, activatingIntermediary, intermediaries, channels, endpoints);

                        // Get the information from the endpoints and intermediaries into one list, filtering out any intermediaries or endpoints which dont have a scenario step.
                        var configurationRoutingObjects = routingObjects.Where(r => r.RoutingObject is Intermediary)
                            .Select(r => r.RoutingObject as Intermediary)
                            .Where(i => i.Properties.ContainsKey(ModelConstants.ScenarioStepName) && i.Activator == false)
                            .Select(i => new
                            {
                                ScenarioStepName = i.Properties[ModelConstants.ScenarioStepName].ToString(),
                                Resources = i.Resources,
                            })
                            .Union(
                            routingObjects.Where(r => r.RoutingObject is Endpoint)
                            .Select(r => r.RoutingObject as Endpoint)
                            .Where(ep => ep.Properties.ContainsKey(ModelConstants.ScenarioStepName))
                            .Select(ep => new
                            {
                                ScenarioStepName = ep.Properties[ModelConstants.ScenarioStepName].ToString(),
                                Resources = ep.Resources,
                            }));

                        // Initialise the JSON routing slip config.
                        var routes = new JArray();
                        var routingSlipConfig = new JObject
                        {
                            ["routes"] = routes
                        };

                        var buildRoutingSlipConfig = true;

                        // Get all template resources in the route.
                        var routeResources = routingObjects.SelectMany(i => i.RoutingObject.Resources);

                        // Build the routes.
                        foreach (var configurationRoutingObject in configurationRoutingObjects)
                        {
                            // Find the logic app resource under the routing object.
                            var logicAppResource = FindLogicAppResource(routeResources, scenarioName, configurationRoutingObject.ScenarioStepName);

                            if (logicAppResource == null)
                            {
                                // Find the logic app resource under the application.
                                var applicationResources = targetApplication.Intermediaries.SelectMany(i => i.Resources).Union(targetApplication.Endpoints.SelectMany(e => e.Resources));
                                logicAppResource = FindLogicAppResource(applicationResources, targetApplication.Name, configurationRoutingObject.ScenarioStepName);

                                if (logicAppResource == null)
                                {
                                    // Find the logic app resource at the global level as this is a common resource.
                                    var messageBusResources = Model.FindAllTargetResourceTemplates();
                                    logicAppResource = FindLogicAppResource(messageBusResources, Model.MigrationTarget.MessageBus.Name, configurationRoutingObject.ScenarioStepName);
                                }
                            }

                            if (logicAppResource != null)
                            {
                                // Generate the routing config.
                                routes.Add(BuildRoutingSlipConfig(configurationRoutingObject.ScenarioStepName, logicAppResource));
                            }
                            else
                            {
                                // Log failure to parse the route.
                                _logger.LogDebug(TraceMessages.UnableToFindResourceWithTypeInTargetModelForScenarioStepName, RuleName, GetLogicAppResourceType(), configurationRoutingObject.ScenarioStepName);
                                buildRoutingSlipConfig = false;
                            }
                        }

                        if (buildRoutingSlipConfig)
                        {
                            var conversionPath = Context.ConversionFolder;

                            var appConfigResource = activatingIntermediary.Resources.SingleOrDefault(r => r.ResourceType == ModelConstants.ResourceTypeRoutingSlip);

                            if (appConfigResource != null)
                            {
                                var fileName = $"{scenarioName}".ToLowerInvariant().Replace(" ", string.Empty); ;
                                var outputPath = new FileInfo(Path.Combine(conversionPath, Path.Combine(appConfigResource.OutputPath, $"{fileName}.routingslip.json")));

                                _fileRepository.WriteJsonFile(outputPath.FullName, routingSlipConfig);
                            }
                            else
                            {
                                _logger.LogError(ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypeRoutingSlip, activatingIntermediary.Name);
                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypeRoutingSlip, activatingIntermediary.Name)));
                            }
                        }
                        else
                        {
                            // Remove the resources to generate the routing slip as there is no valid route.
                            var routingSlipResource = activatingIntermediary.Resources.SingleOrDefault(r => r.ResourceType == ModelConstants.ResourceTypeRoutingSlip);
                            if (routingSlipResource != null)
                            {
                                var resourceName = routingSlipResource.ResourceName;

                                var powerShellResource = activatingIntermediary.Resources.SingleOrDefault(r => r.ResourceType == ModelConstants.ResourceTypePowerShell && r.ResourceName == resourceName);
                                if (powerShellResource != null)
                                {
                                    activatingIntermediary.Resources.Remove(routingSlipResource);
                                    activatingIntermediary.Resources.Remove(powerShellResource);

                                    // Log.
                                    _logger.LogDebug(TraceMessages.RoutingSlipNotGeneratedForScenario, RuleName, activatingIntermediary.Properties[ModelConstants.ScenarioName]);

                                    // Add to report.
                                    activatingIntermediary.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = WarningMessages.RoutingSlipNotGeneratedForScenario });
                                }
                                else
                                {
                                    _logger.LogError(ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypePowerShell, activatingIntermediary.Name);
                                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypePowerShell, activatingIntermediary.Name)));
                                }
                            }
                            else
                            {
                                _logger.LogError(ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypeRoutingSlip, activatingIntermediary.Name);
                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypeRoutingSlip, activatingIntermediary.Name)));
                            }
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.GeneratorCompleted, RuleName, nameof(AP002SendRoutingSlipGenerator));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Build the routing slip config in JSON.
        /// </summary>
        /// <param name="scenarioStepName">The name of the scenario step.</param>
        /// <param name="resource">The resource containing the routing slip information.</param>
        /// <returns>The routing slip config.</returns>
        private static JObject BuildRoutingSlipConfig(string scenarioStepName, TargetResourceTemplate resource)
        {
            switch (resource.ResourceType)
            {
                case ModelConstants.ResourceTypeAzureLogicAppConsumption:
                    {
                        return JObject.FromObject(new
                        {
                            name = scenarioStepName,
                            routingSteps = new
                            {
                                channelType = ModelConstants.TriggerChannelAzureApim
                            },
                            routingParameters = new
                            {
                                messageReceiverType = resource.ResourceType,
                                parameters = new
                                {
                                    resourceId = $"/{resource?.Parameters[ModelConstants.ResourceTemplateParameterAzureResourceGroupName]}/{resource?.ResourceName}"
                                }
                            }
                        });
                    }

                case ModelConstants.ResourceTypeAzureLogicAppStandard:
                    {
                        return JObject.FromObject(new
                        {
                            name = scenarioStepName,
                            routingSteps = new
                            {
                                channelType = ModelConstants.TriggerChannelAzureApim
                            },
                            routingParameters = new
                            {
                                messageReceiverType = resource.ResourceType,
                                parameters = new
                                {
                                    resourceId = $"/{resource?.Parameters[ModelConstants.ResourceTemplateParameterAzureResourceGroupName]}/{resource?.Parameters[ModelConstants.ResourceTemplateParameterAzureLogicAppName]}/{resource?.ResourceName}"
                                }
                            }
                        });
                    }

                default:
                    {
                        return new JObject();
                    }
            }
        }

        /// <summary>
        /// Find the logic app resource, with the matching scenario step name.
        /// </summary>
        /// <param name="resources">The resources to search.</param>
        /// <param name="scope">The scope in which to find the template associated with the step.</param>
        /// <param name="scenarioStepName">The scenario step to look for.</param>
        /// <returns>The matching resource if one is found, otherwise null.</returns>
        private TargetResourceTemplate FindLogicAppResource(IEnumerable<TargetResourceTemplate> resources, string scope, string scenarioStepName)
        {
            
            _logger.LogTrace(TraceMessages.FindingResourceTemplateByScenarioStepName, RuleName, GetLogicAppResourceType(), scenarioStepName, scope);

            var templates = resources.Where(r =>
                r.ResourceType == GetLogicAppResourceType() &&
                r.Parameters.ContainsKey(ModelConstants.ResourceTemplateParameterScenarioStepName) &&
                r.Parameters[ModelConstants.ResourceTemplateParameterScenarioStepName].ToString() == scenarioStepName);

            if (templates != null && templates.Any())
            {
                if (templates.Count() == 1)
                {
                    return templates.Single();
                }
                else
                {
                    _logger.LogDebug(TraceMessages.FoundTooManyResourceTemplatesByScenarioStepName, RuleName, templates.Count(), GetLogicAppResourceType(), scenarioStepName, scope);
                }
            }
            else
            {
                _logger.LogDebug(TraceMessages.FoundNoResourceTemplateByScenarioStepName, RuleName, GetLogicAppResourceType(), scenarioStepName, scope);
            }

            return null;
        }

        /// <summary>
        /// Gets the ResourceType for a LogicApp, depending on the TargetEnvironment.
        /// </summary>
        /// <returns>ResourceType for a LogicApp</returns>
        private string GetLogicAppResourceType()
        {
            var resourceType = "(unknown)";
            if (Model.MigrationTarget.TargetEnvironment == AzureIntegrationServicesTargetEnvironment.Consumption || Model.MigrationTarget.TargetEnvironment == AzureIntegrationServicesTargetEnvironment.ConsumptionLite)
            {
                resourceType = ModelConstants.ResourceTypeAzureLogicAppConsumption;
            }
            else if (Model.MigrationTarget.TargetEnvironment == AzureIntegrationServicesTargetEnvironment.Standard || Model.MigrationTarget.TargetEnvironment == AzureIntegrationServicesTargetEnvironment.StandardLite)
            {
                resourceType = ModelConstants.ResourceTypeAzureLogicAppStandard;
            }

            return resourceType;
        }
    }
}
