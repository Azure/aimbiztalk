// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules
{
    /// <summary>
    /// Implements rule RP003, which analyzes the model and creates EndPoints for each application receive http location.
    /// </summary>
    public sealed class RP003HttpReceivePortAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "RP003";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="RP003HttpReceivePortAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public RP003HttpReceivePortAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(RP003HttpReceivePortAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the http EndPoint for applications in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get the parsed BizTalk model from the application model
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();
            if (parsedApplicationGroup?.Applications != null)
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(RP003HttpReceivePortAnalyzer));

                foreach (var application in parsedApplicationGroup.Applications)
                {
                    var receivePorts = application.Application?.Bindings?.BindingInfo?.ReceivePortCollection;
                    if (receivePorts != null && receivePorts.Any())
                    {
                        foreach (var receivePort in receivePorts)
                        {
                            if (receivePort.ReceiveLocations != null && receivePort.ReceiveLocations.Any())
                            {
                                foreach (var receiveLocation in receivePort.ReceiveLocations)
                                {
                                    if (receiveLocation.ReceiveLocationTransportType.Name == "HTTP")
                                    {
                                        // Find the adapter in target model
                                        var adapterKey = $"{ModelConstants.MessageBusLeafKey}:{application.Application.Name.FormatKey()}:{receivePort.Name.FormatKey()}:{receiveLocation.Name.FormatKey()}:{ModelConstants.AdapterEndpointLeafKey}";
                                        var adapter = Model.FindMessagingObject(adapterKey);
                                        if (adapter.messagingObject != null)
                                        {
                                            var httpAdapter = (AdapterEndpoint)adapter.messagingObject;

                                            // Set conversion rating
                                            httpAdapter.Rating = ConversionRating.FullConversionWithFidelityLoss;

                                            if (!receivePort.IsTwoWay)
                                            {
                                                // Override the message exchange pattern.
                                                httpAdapter.MessageExchangePattern = MessageExchangePattern.Receive;
                                            }

                                            // Set resource map key to hook into the configuration process
                                            var messagingObject = Model.FindMessagingObject(httpAdapter.Key);
                                            var appName = $"{messagingObject.application.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-")}";
                                            var adapterName = httpAdapter.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-");
                                            httpAdapter.ResourceMapKey = $"httpReceiveAdapterEndpoint{appName}{adapterName}";
                                        }
                                        else
                                        {
                                            _logger.LogError(ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Endpoint, adapterKey);
                                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Endpoint, adapterKey)));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(RP003HttpReceivePortAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(RP003HttpReceivePortAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
