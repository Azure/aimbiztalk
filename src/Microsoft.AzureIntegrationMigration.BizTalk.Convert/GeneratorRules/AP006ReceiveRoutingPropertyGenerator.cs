// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    /// Defines a class that implements a converter to generate routing properties for receive scenarios.
    /// </summary>
    public class AP006ReceiveRoutingPropertyGenerator : BizTalkConverterBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "AP006";

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
        /// Creates a new instance of a <see cref="AP006ReceiveRoutingPropertyGenerator"/> class.
        /// </summary>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="routeWalker">The scenario route walker.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public AP006ReceiveRoutingPropertyGenerator(IFileRepository fileRepository, IScenarioRouteWalker routeWalker, IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(AP006ReceiveRoutingPropertyGenerator), model, context, logger)
        {
            // Validate and set the members.
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            _routeWalker = routeWalker ?? throw new ArgumentNullException(nameof(routeWalker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate routing properties for each scenario.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "The routing properties file name is expected to be lowercase")]
        protected override async Task ConvertInternalAsync(CancellationToken token)
        {
            if (Model.MigrationTarget.MessageBus?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsMigrationTargetMessageBusMissing, RuleName, nameof(AP006ReceiveRoutingPropertyGenerator));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningGenerator, RuleName, nameof(AP006ReceiveRoutingPropertyGenerator));

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
                        // Get the scenario name.
                        var scenarioName = initiatingEndpoint.Properties[ModelConstants.ScenarioName].ToString();

                        // Walk the routing objects starting at the initiating endpoint.
                        var routingObjects = _routeWalker.WalkReceiveRoute(RuleName, scenarioName, initiatingEndpoint, intermediaries, channels);

                        // Get the information from the endpoints and intermediaries into one list, filtering out any intermediaries or endpoints which don't have routing properties.
                        var scenarioStepRoutes = routingObjects.Where(r => r.RoutingObject is Endpoint)
                            .Select(r => r.RoutingObject as Endpoint)
                            .Where(ep => ep.Properties.ContainsKey(ModelConstants.RoutingProperties))
                            .Select(ep => new
                            {
                                RoutingProperties = ep.Properties[ModelConstants.RoutingProperties] as Dictionary<string, object>
                            })
                            .Union(
                            routingObjects.Where(r => r.RoutingObject is Intermediary)
                            .Select(r => r.RoutingObject as Intermediary)
                            .Where(i => i.Properties.ContainsKey(ModelConstants.RoutingProperties))
                            .Select(i => new
                            {
                                RoutingProperties = i.Properties[ModelConstants.RoutingProperties] as Dictionary<string, object>
                            }));

                        // Generate the routing property config as JSON.
                        var routingConfig =
                            new JObject(
                                new JProperty("routingProperties",
                                new JArray(
                                    from scenarioStepRoute in scenarioStepRoutes
                                    from routingProperty in scenarioStepRoute.RoutingProperties
                                    select new JObject(
                                        new JProperty("propertyName", routingProperty.Key),
                                        new JProperty("propertyType", "property"),
                                        new JProperty("propertyValue", routingProperty.Value)
                                        )
                                    )
                                ));

                        var conversionPath = Context.ConversionFolder;

                        var appConfigResource = initiatingEndpoint.Resources.SingleOrDefault(r => r.ResourceType == ModelConstants.ResourceTypeRoutingProperties);

                        if (appConfigResource != null)
                        {
                            var fileName = $"{scenarioName}".ToLowerInvariant().Replace(" ", string.Empty);
                            var outputPath = new FileInfo(Path.Combine(conversionPath, Path.Combine(appConfigResource.OutputPath, $"{fileName}.routingproperties.json")));

                            _fileRepository.WriteJsonFile(outputPath.FullName, routingConfig);
                        }
                        else
                        {
                            _logger.LogError(ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.RoutingProperties, initiatingEndpoint.Name);
                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypeRoutingProperties, initiatingEndpoint.Name)));
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.GeneratorCompleted, RuleName, nameof(AP006ReceiveRoutingPropertyGenerator));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
