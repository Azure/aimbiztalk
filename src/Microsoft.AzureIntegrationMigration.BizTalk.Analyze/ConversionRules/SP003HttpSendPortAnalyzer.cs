// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules
{
    /// <summary>
    /// Implements rule SP003, which analyzes the model and creates endpoints for each send http location.
    /// </summary>
    public sealed class SP003HttpSendPortAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "SP003";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="SP003HttpSendPortAnalyzer "/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public SP003HttpSendPortAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(SP003HttpSendPortAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the http send endpoints in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get parsed BizTalk model from the application model
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();
            if (parsedApplicationGroup?.Applications != null)
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(SP003HttpSendPortAnalyzer));

                foreach (var application in parsedApplicationGroup.Applications)
                {
                    var sendPorts = application.Application?.Bindings?.BindingInfo?.SendPortCollection;
                    if (sendPorts != null && sendPorts.Any())
                    {
                        foreach(var sendPort in sendPorts)
                        {
                            if (sendPort?.PrimaryTransport?.TransportType?.Name == "HTTP")
                            {
                                // Find adapter in target model
                                var adapterKey = $"{ModelConstants.MessageBusLeafKey}:{application.Application.Name.FormatKey()}:{sendPort.Name.FormatKey()}:{ModelConstants.AdapterEndpointLeafKey}";
                                var adapter = Model.FindMessagingObject(adapterKey);
                                if (adapter.messagingObject != null)
                                {
                                    var httpAdapter = (AdapterEndpoint)adapter.messagingObject;

                                    // Set conversion rating
                                    httpAdapter.Rating = ConversionRating.FullConversionWithFidelityLoss;

                                    // Set the message exchange pattern.
                                    httpAdapter.MessageExchangePattern = MessageExchangePattern.Send;

                                    // Set resource map key to hook into the configuration process
                                    var messagingObject = Model.FindMessagingObject(httpAdapter.Key);
                                    var appName = $"{messagingObject.application.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-")}";
                                    var adapterName = httpAdapter.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-");
                                    httpAdapter.ResourceMapKey = $"httpSendAdapterEndpoint{appName}{adapterName}";

                                    if (!string.IsNullOrEmpty(sendPort.PrimaryTransport.TransportTypeData))
                                    {
                                        var configItems = MapTransportTypeData(sendPort.PrimaryTransport.TransportTypeData);

                                        MapAdapterProperties(configItems, httpAdapter);

                                        foreach (var item in configItems)
                                        {
                                            httpAdapter.ReportMessages.Add(new ReportMessage()
                                            {
                                                Severity = MessageSeverity.Warning,
                                                Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkHttpAdapterPropertyNotSupported, item.Key, item.Value)
                                            });
                                        }

                                        // Map the config.
                                        var configProperties = httpAdapter.Properties[ModelConstants.ConfigurationEntry] as Dictionary<string, object>;

                                        httpAdapter.ReportLinks.Add(AnalysisResources.HttpAdapterHelpLink);
                                    }
                                    else
                                    {
                                        _logger.LogDebug(WarningMessages.SendPortTransportTypeDataNotFound, httpAdapter.Name);
                                        httpAdapter.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.SendPortTransportTypeDataNotFound, httpAdapter.Name) });
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
                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(SP003HttpSendPortAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(SP003HttpSendPortAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Map the fields that used by the azure http connector, any expected fields that are not found are added with the default values.
        /// </summary>
        /// <param name="configItems">The adapter configuration properties.</param>
        /// <param name="endpoint">The http endpoint.</param>
        private void MapAdapterProperties(Dictionary<string, string> configItems, AdapterEndpoint endpoint)
        {
            // Set supported property names and defaults
            var supportedProperties = new Dictionary<string, (string, object)>()
            {
                { "Address", ("address", "")}
            };

            // Search through the BizTalk adapter properties and match properties
            foreach (var supportedProperty in supportedProperties)
            {
                (string mappedName, object mappedValue) mappedProperty = supportedProperty.Value;

                if (configItems.ContainsKey(supportedProperty.Key) && !string.IsNullOrEmpty(configItems[supportedProperty.Key]))
                {
                    // Set value on endpoint
                    endpoint.Properties.Add(mappedProperty.mappedName, configItems[supportedProperty.Key]);

                    _logger.LogDebug(TraceMessages.BizTalkHttpSendAdapterBindingPropertyFound, RuleName, supportedProperty.Key, endpoint.Name, mappedProperty.mappedValue);

                    // Remove handled property from BizTalk adapter property list
                    configItems.Remove(supportedProperty.Key);
                }
                else
                {
                    // Set default value
                    endpoint.Properties.Add(mappedProperty.mappedName, mappedProperty.mappedValue);

                    _logger.LogDebug(TraceMessages.BizTalkHttpSendAdapterBindingPropertyFound, RuleName, supportedProperty.Key, endpoint.Name, mappedProperty.mappedValue);
                }
            }

            _logger.LogDebug(TraceMessages.BizTalkHttpSendAdapterBindingPropertySensitive, RuleName, "password", endpoint.Name);
        }

        /// <summary>
        /// Convert the BizTalk adapter TransportTypeData to a dictionary.
        /// </summary>
        /// <param name="transportTypeData">The XML representation of the transport type.</param>
        /// <returns>A dictionary containing the transport type data.</returns>
        private static Dictionary<string, string> MapTransportTypeData(string transportTypeData)
        {
            var decodedCustomProps = HttpUtility.HtmlDecode(transportTypeData);
            var docCustomProps = new XmlDocument() { XmlResolver = null };
#pragma warning disable CA3075 // Insecure DTD processing in XML
            docCustomProps.LoadXml(decodedCustomProps);
#pragma warning restore CA3075 // Insecure DTD processing in XML
            var node = docCustomProps.SelectSingleNode("/CustomProps");

            var items = new Dictionary<string, string>();

            foreach (var cnode in node.ChildNodes.OfType<XmlElement>())
            {
                items.Add(cnode.Name, cnode.InnerText);
            }

            return items;
        }
    }
}
