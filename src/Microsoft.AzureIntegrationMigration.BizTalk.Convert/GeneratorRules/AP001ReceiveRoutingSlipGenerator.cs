// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
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
    /// Defines a class that implements a converter to generate routing slips for receive endpoints.
    /// </summary>
    public sealed class AP001ReceiveRoutingSlipGenerator : BizTalkConverterBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "AP001";

        /// <summary>
        /// Defines a file repository.
        /// </summary>
        private readonly IFileRepository _fileRepository;

        /// <summary>
        /// Defines a scenario route walker.
        /// </summary>
        private readonly IScenarioRouteWalker _routeWalker;

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="AP001ReceiveRoutingSlipGenerator"/> class.
        /// </summary>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="routeWalker">The scenario route walker.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public AP001ReceiveRoutingSlipGenerator(IFileRepository fileRepository, IScenarioRouteWalker routeWalker, IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(AP001ReceiveRoutingSlipGenerator), model, context, logger)
        {
            // Validate and set the member.
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            _routeWalker = routeWalker ?? throw new ArgumentNullException(nameof(routeWalker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate routing slip configuration files for each receive scenario.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "The routing slip configuration file name is expected to be lowercase")]
        protected override async Task ConvertInternalAsync(CancellationToken token)
        {
            if (Model.MigrationTarget.MessageBus?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsMigrationTargetMessageBusMissing, RuleName, nameof(AP001ReceiveRoutingSlipGenerator));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningGenerator, RuleName, nameof(AP001ReceiveRoutingSlipGenerator));

                // Get all of the intermediaries and channels from the migration target.
                var intermediaries = Model.MigrationTarget.MessageBus.Applications.SelectMany(a => a.Intermediaries);
                var channels = Model.MigrationTarget.MessageBus.Applications.SelectMany(a => a.Channels);

                foreach (var targetApplication in Model.MigrationTarget.MessageBus.Applications)
                {
                    // Loop through all of the receive endpoints and request/reply send points, which have config.
                    foreach (var initiatingEndpoint in targetApplication.Endpoints.Where(
                        ep => (ep.Activator || ep.MessageExchangePattern == ApplicationModel.Target.Endpoints.MessageExchangePattern.RequestReply)
                        && !string.IsNullOrEmpty(ep.ResourceMapKey))
                        )
                    {
                        // Get scenario name
                        var scenarioName = initiatingEndpoint.Properties[ModelConstants.ScenarioName].ToString();

                        // Walk the intermediaries starting at the receive endpoint.
                        var routingObjects = _routeWalker.WalkReceiveRoute(RuleName, scenarioName, initiatingEndpoint, intermediaries, channels);

                        // Get the messaging objects in the route which are intermediaries.
                        var routingIntermediaries = 
                            from routingObject in routingObjects
                            where routingObject.RoutingObject is Intermediary
                            select new
                            {
                                Intermediary = (Intermediary)routingObject.RoutingObject,
                                Channel = routingObject.InputChannel
                            };

                        // Filter out the intermediaries which don't have a scenario step.
                        var configurationIntermediaries = routingIntermediaries.Where(i => i.Intermediary.Properties.ContainsKey(ModelConstants.ScenarioStepName));

                        // Initialise the JSON routing slip config.
                        var routes = new JArray();
                        var routingSlipConfig = new JObject
                        {
                            ["routes"] = routes
                        };

                        // Get all template resources in the route.
                        var routeResources = routingIntermediaries.Select(i => i.Intermediary).SelectMany(i => i.Resources);

                        // Build the routes.
                        foreach (var configurationIntermediary in configurationIntermediaries)
                        {
                            var scenarioStepName = configurationIntermediary.Intermediary.Properties[ModelConstants.ScenarioStepName].ToString();

                            // Find the logic app resource in the route.
                            var logicAppResource = FindLogicAppResource(routeResources, scenarioName, scenarioStepName);

                            if (logicAppResource == null)
                            {
                                // Find the logic app resource under the application.
                                var applicationResources = targetApplication.Intermediaries.SelectMany(i => i.Resources);
                                logicAppResource = FindLogicAppResource(applicationResources, targetApplication.Name, scenarioStepName);

                                if (logicAppResource == null)
                                {
                                    // Find the logic app resource at the global level as this is a common resource.
                                    var messageBusResources = Model.FindAllTargetResourceTemplates();
                                    logicAppResource = FindLogicAppResource(messageBusResources, Model.MigrationTarget.MessageBus.Name, scenarioStepName);
                                }
                            }

                            if (logicAppResource != null)
                            {
                                // Generate the routing conig.
                                routes.Add(BuildRoutingSlipConfig(scenarioStepName, logicAppResource));
                            }
                            else
                            {
                                _logger.LogError(ErrorMessages.UnableToFindResourceWithTypeInTargetModelForScenarioStepName, ModelConstants.ResourceTypeAzureLogicApp, scenarioStepName);
                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceWithTypeInTargetModelForScenarioStepName, ModelConstants.ResourceTypeAzureLogicApp, scenarioStepName)));
                            }
                        }

                        var conversionPath = Context.ConversionFolder;

                        var appConfigResource = initiatingEndpoint.Resources.SingleOrDefault(r => r.ResourceType == ModelConstants.ResourceTypeRoutingSlip);

                        if (appConfigResource != null)
                        {
                            var fileName = $"{scenarioName}".ToLowerInvariant().Replace(" ", string.Empty); ;
                            var outputPath = new FileInfo(Path.Combine(conversionPath, Path.Combine(appConfigResource.OutputPath, $"{fileName}.routingslip.json")));

                            _fileRepository.WriteJsonFile(outputPath.FullName, routingSlipConfig);
                        }
                        else
                        {
                            _logger.LogError(ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypeRoutingSlip, initiatingEndpoint.Name);
                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypeRoutingSlip, initiatingEndpoint.Name)));
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.GeneratorCompleted, RuleName, nameof(AP001ReceiveRoutingSlipGenerator));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Discover the intermediaries recursively.
        /// </summary>
        /// <param name="intermediaries">The intermediaries to search.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="inputChannelKey">The current input channel key ref to find.</param>
        /// <returns>The discovered intermediary with their channels.</returns>
        private IList<(Intermediary Intermediary, Channel OutputChannel)> DiscoverIntermediariesRecursively(IEnumerable<Intermediary> intermediaries, IEnumerable<Channel> channels, string inputChannelKey)
        {
            var discoveredIntermediaries = new List<(Intermediary, Channel)>();

            var discoveredIntermediary = intermediaries.FirstOrDefault(i => i.InputChannelKeyRefs.Contains(inputChannelKey));

            // Get the channel for the intermediary.
            var discoveredChannel = channels.FirstOrDefault(c => c.Key == inputChannelKey);

            if (discoveredIntermediary != null)
            {
                // A topic channel indicates the end of the receive configuration, only continue if this is not a topic channel.
                if (!(discoveredChannel is TopicChannel))
                {
                    discoveredIntermediaries.Add((discoveredIntermediary, discoveredChannel));

                    // Loop through all of the output channels.
                    foreach (var outputChannelRef in discoveredIntermediary.OutputChannelKeyRefs)
                    {
                        discoveredIntermediaries.AddRange(DiscoverIntermediariesRecursively(intermediaries, channels, outputChannelRef));
                    }
                }
            }
            else
            {
                _logger.LogError(ErrorMessages.UnableToFindAChannelWithTheKeyInTargetModel, inputChannelKey);
                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAChannelWithTheKeyInTargetModel, inputChannelKey)));
            }

            return discoveredIntermediaries;
        }

        /// <summary>
        /// Build the routing slip config in JSON.
        /// </summary>
        /// <param name="scenarioStepName">The name of the scenario step.</param>
        /// <param name="resource">The resource containing the routing slip information.</param>
        /// <returns>The routing slip config.</returns>
        private static JObject BuildRoutingSlipConfig(string scenarioStepName, TargetResourceTemplate resource)
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

        /// <summary>
        /// Find the logic app resource, with the matching scenario step name.
        /// </summary>
        /// <param name="resources">The resources to search.</param>
        /// <param name="scope">The scope in which to find the template associated with the step.</param>
        /// <param name="scenarioStepName">The scenario step to look for.</param>
        /// <returns>The matching resource if one is found, otherwise null.</returns>
        private TargetResourceTemplate FindLogicAppResource(IEnumerable<TargetResourceTemplate> resources, string scope, string scenarioStepName)
        {
            _logger.LogTrace(TraceMessages.FindingResourceTemplateByScenarioStepName, RuleName, ModelConstants.ResourceTypeAzureLogicApp, scenarioStepName, scope);

            var templates = resources.Where(r =>
                r.ResourceType == ModelConstants.ResourceTypeAzureLogicApp &&
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
                    _logger.LogDebug(TraceMessages.FoundTooManyResourceTemplatesByScenarioStepName, RuleName, templates.Count(), ModelConstants.ResourceTypeAzureLogicApp, scenarioStepName, scope);
                }
            }
            else
            {
                _logger.LogDebug(TraceMessages.FoundNoResourceTemplateByScenarioStepName, RuleName, ModelConstants.ResourceTypeAzureLogicApp, scenarioStepName, scope);
            }

            return null;
        }
    }
}
