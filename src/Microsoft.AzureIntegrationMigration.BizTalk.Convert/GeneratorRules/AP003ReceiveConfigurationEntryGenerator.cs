// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
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
    /// Defines a class that implements a converter to generate configuration for receive scenarios.
    /// </summary>
    public sealed class AP003ReceiveConfigurationEntryGenerator : BizTalkConverterBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "AP003";

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
        /// Creates a new instance of a <see cref="AP003ReceiveConfigurationEntryGenerator"/> class.
        /// </summary>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="routeWalker">The scenario route walker.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public AP003ReceiveConfigurationEntryGenerator(IFileRepository fileRepository, IScenarioRouteWalker routeWalker, IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(AP003ReceiveConfigurationEntryGenerator), model, context, logger)
        {
            // Validate and set the member.
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            _routeWalker = routeWalker ?? throw new ArgumentNullException(nameof(routeWalker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate configuration.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "The configuration entry file name is expected to be lowercase")]
        protected override async Task ConvertInternalAsync(CancellationToken token)
        {
            if (Model.MigrationTarget.MessageBus?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsMigrationTargetMessageBusMissing, RuleName, nameof(AP003ReceiveConfigurationEntryGenerator));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningGenerator, RuleName, nameof(AP003ReceiveConfigurationEntryGenerator));

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
                        var scenarioName = initiatingEndpoint.Properties[ModelConstants.ScenarioName].ToString();
                        var scenarioStepName = initiatingEndpoint.Properties[ModelConstants.ScenarioStepName].ToString();

                        var appConfigResource = initiatingEndpoint.Resources.SingleOrDefault(r => r.ResourceType == ModelConstants.ResourceTypeConfigurationEntry);

                        if (appConfigResource != null)
                        {
                            // Walk the intermediaries starting at the receive endpoint.
                            var routingObjects = _routeWalker.WalkReceiveRoute(RuleName, scenarioName, initiatingEndpoint, intermediaries, channels);

                            // Get any global config from the resource.
                            var globalConfig = new JObject(
                                new JProperty("globalConfig",
                                new JObject(
                                from globalConfigSetting in appConfigResource.Parameters
                                where globalConfigSetting.Key.StartsWith(ModelConstants.ResourceTemplateParamterGlobalConfigPrefix, StringComparison.OrdinalIgnoreCase)
                                select new JProperty(
                                               globalConfigSetting.Key.Replace(ModelConstants.ResourceTemplateParamterGlobalConfigPrefix, string.Empty).Replace("_", " ").ConvertSnakeCaseToCamelCase(),
                                               globalConfigSetting.Value)
                                )));

                            // Get the configuration object.
                            var configurationObjects = from routingObject in routingObjects
                                                       where routingObject.RoutingObject.Properties.ContainsKey(ModelConstants.ScenarioStepName)
                                                       select new
                                                       {
                                                           ScenarioStepName = routingObject.RoutingObject.Properties[ModelConstants.ScenarioStepName].ToString(),
                                                           Configuration = routingObject.RoutingObject.Properties.TryGetValue(ModelConstants.ConfigurationEntry, out var value) ? value as Dictionary<string, object> : new Dictionary<string, object>()
                                                       };

                            // Generate the JSON configuration.
                            var configurationEntry = new JObject(
                                from configurationObject in configurationObjects
                                where configurationObject.Configuration != null
                                select new JProperty(configurationObject.ScenarioStepName,
                                    new JObject(
                                    from configurationProperty in configurationObject.Configuration.AsEnumerable()
                                    select new JProperty(configurationProperty.Key, configurationProperty.Value != null ? JToken.FromObject(configurationProperty.Value) : null)
                                    ))
                                );

                            // Merge in the global config.
                            configurationEntry.Merge(globalConfig, new JsonMergeSettings
                            {
                                MergeArrayHandling = MergeArrayHandling.Union
                            });

                            var conversionPath = Context.ConversionFolder;

                            var fileName = $"{initiatingEndpoint.Properties[ModelConstants.ScenarioName]}".ToLowerInvariant().Replace(" ", string.Empty); ;
                            var outputPath = new FileInfo(Path.Combine(conversionPath, Path.Combine(appConfigResource.OutputPath, $"{fileName}.configurationentry.json")));

                            _fileRepository.WriteJsonFile(outputPath.FullName, configurationEntry);
                        }
                        else
                        {
                            _logger.LogError(ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypeConfigurationEntry, initiatingEndpoint.Name);
                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypeConfigurationEntry, initiatingEndpoint.Name)));
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.GeneratorCompleted, RuleName, nameof(AP003ReceiveConfigurationEntryGenerator));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
