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
    /// Implements rule SP002, which analyzes the model and creates endpoints for each send file location.
    /// </summary>
    public sealed class SP002FileSendPortAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "SP002";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="SP002FileSendPortAnalyzer "/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public SP002FileSendPortAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(SP002FileSendPortAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the file send endpoints in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get parsed BizTalk model from the application model
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();
            if (parsedApplicationGroup?.Applications != null)
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(SP002FileSendPortAnalyzer));

                foreach (var application in parsedApplicationGroup.Applications)
                {
                    var sendPorts = application.Application?.Bindings?.BindingInfo?.SendPortCollection;
                    if (sendPorts != null && sendPorts.Any())
                    {
                        foreach(var sendPort in sendPorts)
                        {
                            if (sendPort?.PrimaryTransport?.TransportType?.Name == "FILE")
                            {
                                // Find adapter in target model
                                var adapterKey = $"{ModelConstants.MessageBusLeafKey}:{application.Application.Name.FormatKey()}:{sendPort.Name.FormatKey()}:{ModelConstants.AdapterEndpointLeafKey}";
                                var adapter = Model.FindMessagingObject(adapterKey);
                                if (adapter.messagingObject != null)
                                {
                                    var fileAdapter = (AdapterEndpoint)adapter.messagingObject;

                                    // Set conversion rating
                                    fileAdapter.Rating = ConversionRating.FullConversionWithFidelityLoss;

                                    // Set the message exchange pattern.
                                    fileAdapter.MessageExchangePattern = MessageExchangePattern.Send;

                                    // Set resource map key to hook into the configuration process
                                    var messagingObject = Model.FindMessagingObject(fileAdapter.Key);
                                    var appName = $"{messagingObject.application.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-")}";
                                    var adapterName = fileAdapter.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-");
                                    fileAdapter.ResourceMapKey = $"fileSendAdapterEndpoint{appName}{adapterName}";

                                    if (!string.IsNullOrEmpty(sendPort.PrimaryTransport.TransportTypeData))
                                    {
                                        var configItems = MapTransportTypeData(sendPort.PrimaryTransport.TransportTypeData);

                                        // Add the address to the config items.
                                        var address = sendPort.PrimaryTransport.Address;
                                        if (!string.IsNullOrEmpty(address))
                                        {
                                            if (configItems.TryGetValue("FileName", out var fileName))
                                            {
                                                address = address.Replace(fileName, string.Empty);
                                            }
                                            // Replace the path separator to ensure its can be processed by the Azure CLI.
                                            address = address.Replace("\\", "/");
                                        }
                                        configItems.Add("Address", address);

                                        MapAdapterProperties(configItems, fileAdapter);

                                        foreach (var item in configItems)
                                        {
                                            fileAdapter.ReportMessages.Add(new ReportMessage()
                                            {
                                                Severity = MessageSeverity.Warning,
                                                Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkFileAdapterPropertyNotSupported, item.Key, item.Value)
                                            });
                                        }

                                        // Map the config.
                                        var configProperties = fileAdapter.Properties[ModelConstants.ConfigurationEntry] as Dictionary<string, object>;

                                        configProperties["copyMode"] = MapCopyMode(fileAdapter.Properties["copyMode"]);
                                        configProperties["destinationFolder"] = fileAdapter.Properties["rootFolder"];
                                        configProperties["fileName"] = fileAdapter.Properties["fileName"];

                                        fileAdapter.ReportLinks.Add(AnalysisResources.FileAdapterHelpLink);
                                    }
                                    else
                                    {
                                        _logger.LogDebug(WarningMessages.SendPortTransportTypeDataNotFound, fileAdapter.Name);
                                        fileAdapter.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.SendPortTransportTypeDataNotFound, fileAdapter.Name) });
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
                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(SP002FileSendPortAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(SP002FileSendPortAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Map the fields that used by the azure file connector, any expected fields that are not found are added with the default values.
        /// </summary>
        /// <param name="configItems">The adapter configuration properties.</param>
        /// <param name="endpoint">The file endpoint.</param>
        private void MapAdapterProperties(Dictionary<string, string> configItems, AdapterEndpoint endpoint)
        {
            // Set supported property names and defaults
            var supportedProperties = new Dictionary<string, (string, object)>()
            {
                { "Address", ("rootFolder", "")},
                { "FileName", ("fileName", "%MessageID%.xml") },
                { "Username", ("userName", "")},
                { "CopyMode", ("copyMode", 1) }
            };

            // Search through the BizTalk adapter properties and match properties
            foreach (var supportedProperty in supportedProperties)
            {
                (string mappedName, object mappedValue) mappedProperty = supportedProperty.Value;

                if (configItems.ContainsKey(supportedProperty.Key) && !string.IsNullOrEmpty(configItems[supportedProperty.Key]))
                {
                    // Set value on endpoint
                    endpoint.Properties.Add(mappedProperty.mappedName, configItems[supportedProperty.Key]);

                    _logger.LogDebug(TraceMessages.BizTalkFileSendAdapterBindingPropertyFound, RuleName, supportedProperty.Key, endpoint.Name, mappedProperty.mappedValue);

                    // Remove handled property from BizTalk adapter property list
                    configItems.Remove(supportedProperty.Key);
                }
                else
                {
                    // Set default value
                    endpoint.Properties.Add(mappedProperty.mappedName, mappedProperty.mappedValue);

                    _logger.LogDebug(TraceMessages.BizTalkFileSendAdapterBindingPropertyFound, RuleName, supportedProperty.Key, endpoint.Name, mappedProperty.mappedValue);
                }
            }

            // Special case password because it is a sensitive value
            endpoint.Properties.Add("password", "");
            endpoint.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Information, Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkFileAdapterSensitivePropertyMustBeSpecifiedLater, "password") });
            configItems.Remove("Password");

            _logger.LogDebug(TraceMessages.BizTalkFileSendAdapterBindingPropertySensitive, RuleName, "password", endpoint.Name);
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

        /// <summary>
        /// Maps the copy mode value from its BizTalk to AIS value.
        /// </summary>
        /// <param name="copyMode">The copy mode to map from.</param>
        /// <returns>The mapped copy mode.</returns>
        private static string MapCopyMode(object copyMode)
        {  
            switch (int.Parse((string)copyMode, CultureInfo.InvariantCulture))
            {
                case 0:
                    return "Append";

                case 2:
                    return "Update";

                default:
                    return "Create";                    
            }
        }
    }
}
