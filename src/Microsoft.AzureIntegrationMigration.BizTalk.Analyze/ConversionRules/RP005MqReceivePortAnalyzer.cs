// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Messages;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules
{
    /// <summary>
    /// Implements rule RP005, which analyzes the model and creates EndPoints for each application receive mq location.
    /// </summary>
    public sealed class RP005MqReceivePortAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "RP005";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="RP005MqReceivePortAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public RP005MqReceivePortAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(RP005MqReceivePortAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the mq EndPoints for applications in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get parsed BizTalk model from the application model
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();
            if (parsedApplicationGroup?.Applications != null)
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(RP005MqReceivePortAnalyzer));

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
                                    if (receiveLocation.ReceiveLocationTransportType.Name == "MQS")
                                    {
                                        // Find adapter in target model
                                        var adapterKey = $"{ModelConstants.MessageBusLeafKey}:{application.Application.Name.FormatKey()}:{receivePort.Name.FormatKey()}:{receiveLocation.Name.FormatKey()}:{ModelConstants.AdapterEndpointLeafKey}";
                                        var adapter = Model.FindMessagingObject(adapterKey);
                                        if (adapter.messagingObject != null)
                                        {
                                            var mqAdapter = (AdapterEndpoint)adapter.messagingObject;

                                            // Set conversion rating
                                            mqAdapter.Rating = ConversionRating.FullConversionWithFidelityLoss;

                                            // Change to Accept message exchange pattern as no ack can be delivered for MQ
                                            mqAdapter.MessageExchangePattern = MessageExchangePattern.Accept;

                                            // Set resource map key to hook into the configuration process
                                            var messagingObject = Model.FindMessagingObject(mqAdapter.Key);
                                            var appName = $"{messagingObject.application.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-")}";
                                            var adapterName = mqAdapter.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-");
                                            mqAdapter.ResourceMapKey = $"mqReceiveAdapterEndpoint{appName}{adapterName}";

                                            // Handle adapter properties
                                            if (!string.IsNullOrEmpty(receiveLocation.ReceiveLocationTransportTypeData))
                                            {
                                                var configItems = MapTransportTypeData(receiveLocation.ReceiveLocationTransportTypeData);

                                                MapAdapterProperties(configItems, mqAdapter);

                                                mqAdapter.ReportLinks.Add(AnalysisResources.MqAdapterHelpLink);
                                            }
                                            else
                                            {
                                                _logger.LogDebug(WarningMessages.ReceiveLocationTransportTypeDataNotFound, mqAdapter.Name);

                                                mqAdapter.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.ReceiveLocationTransportTypeDataNotFound, mqAdapter.Name) });
                                            }
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

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(RP005MqReceivePortAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(RP005MqReceivePortAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Map the fields that are to be used by the azure mq connector, expected fields that are not found are added with default value or a place holder text.
        /// </summary>
        /// <param name="configItems"></param>
        /// <param name="endpoint"></param>
        private void MapAdapterProperties(Dictionary<string, string> configItems, AdapterEndpoint endpoint)
        {
            // Set supported property names and defaults
            var supportedProperties = new Dictionary<string, (string, object)>()
            {
                { "uri", ("uri", "") },
                { "queueDetails", ("queueDetails", "") },
                { "userName", ("userName", "") },
                { "password", ("password", "") },
                { "transactionSupported", ("transactionSupported", false) },
                { "suspendAsNonResumable", ("suspendAsNonResumable", false) },
                { "dataOffsetForHeaders", ("dataOffsetForHeaders", false) },
                { "waitInterval", ("waitInterval", "3") },
                { "pollingInterval", ("pollingInterval", "3") },
                { "pollingUnit", ("pollingUnit", "seconds") },
                { "maximumBatchSize", ("maximumBatchSize", "500000") },
                { "maximumNumberOfMessages", ("maximumNumberOfMessages", "100") },
                { "batchWaitInterval", ("batchWaitInterval", "0") },
                { "threadCount", ("threadCount", "1") },
                { "fragmentationSize", ("fragmentationSize", "500000") },
                { "characterSet", ("characterSet", "utf8") },
                { "errorThreshold", ("errorThreshold", "10") },
                { "segmentation", ("segmentation", "none") },
                { "ordered", ("ordered", false) }
            };

            // Search through BizTalk adapter properties and match properties
            foreach (var supportedProperty in supportedProperties)
            {
                (string mappedName, object mappedValue) mappedProperty = supportedProperty.Value;

                if (configItems.ContainsKey(supportedProperty.Key) && !string.IsNullOrEmpty(configItems[supportedProperty.Key]))
                {
                    // Convert values
                    var convertedValue = ConvertAdapterProperty(supportedProperty.Key, configItems[supportedProperty.Key]);

                    // Set value on endpoint
                    endpoint.Properties.Add(mappedProperty.mappedName, convertedValue);

                    _logger.LogDebug(TraceMessages.BizTalkMqReceiveAdapterBindingPropertyFound, RuleName, supportedProperty.Key, endpoint.Name, convertedValue);

                    // If this is queuedetails, split into server, port, queuemaneger and queue
                    if (supportedProperty.Key == "queueDetails" && !string.IsNullOrWhiteSpace(configItems[supportedProperty.Key]))
                    {
                        string[] queueDetailsParts = configItems[supportedProperty.Key].Split('/');
                        if (queueDetailsParts.Length == 3)
                        {
                            // Check if server contains a port
                            if (queueDetailsParts[0].Contains(":"))
                            {
                                string[] serverNameParts = queueDetailsParts[0].Split(':');
                                endpoint.Properties.Add("serverAddress", serverNameParts[0]);
                                endpoint.Properties.Add("serverPort", serverNameParts[1]);
                            }
                            else
                            {
                                endpoint.Properties.Add("serverAddress", queueDetailsParts[0]);
                                endpoint.Properties.Add("serverPort", "");
                            }
                            endpoint.Properties.Add("queueManager", queueDetailsParts[1]);
                            endpoint.Properties.Add("queue", queueDetailsParts[2]);
                        }
                    }

                    // Remove handled property from BizTalk adapter property list
                    configItems.Remove(supportedProperty.Key);
                }
                else
                {
                    // Set default value
                    endpoint.Properties.Add(mappedProperty.mappedName, mappedProperty.mappedValue);

                    _logger.LogDebug(TraceMessages.BizTalkMqReceiveAdapterBindingPropertyNotFound, RuleName, supportedProperty.Key, endpoint.Name, mappedProperty.mappedValue);
                }
            }

            // Special case password because it is a sensitive value
            endpoint.Properties.Add("password", "");
            endpoint.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Information, Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkMqAdapterSensitivePropertyMustBeSpecifiedLater, "password") });
            configItems.Remove("password");
            _logger.LogDebug(TraceMessages.BizTalkMqReceiveAdapterBindingPropertySensitive, RuleName, "password", endpoint.Name);
        }

        /// <summary>
        /// Map certain property values to new values for AIS.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyValue">The value of the property.</param>
        /// <returns>The new converted value.</returns>
        private static object ConvertAdapterProperty(string propertyName, object propertyValue)
        {
            switch (propertyName)
            {
                case "transactionSupported":
                case "suspendAsNonResumable":
                case "dataOffsetForHeaders":
                case "ordered":

                    return propertyValue.ToString() == "yes" ? true : false;
            }

            return propertyValue;
        }

        /// <summary>
        /// Convert the a BizTalk adapter TransportTypeData to a dictionary.
        /// </summary>
        /// <param name="transportTypeData">The xml of the transport type</param>
        /// <returns>A dictionary with the key value types.</returns>
        private static Dictionary<string, string> MapTransportTypeData(string transportTypeData)
        {
            var decodedCustomProps = HttpUtility.HtmlDecode(transportTypeData);
            var docCustomProps = new XmlDocument() { XmlResolver = null };
#pragma warning disable CA3075 // Insecure DTD processing in XML
            docCustomProps.LoadXml(decodedCustomProps);
#pragma warning restore CA3075 // Insecure DTD processing in XML
            var node = docCustomProps.SelectSingleNode("/CustomProps/AdapterConfig/Config");

            var items = new Dictionary<string, string>();

            foreach (var cnode in node.ChildNodes.OfType<XmlElement>())
            {
                items.Add(cnode.Name, cnode.InnerText);
            }

            return items;
        }
    }
}
