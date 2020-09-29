// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
    /// Implements rule RP002, which analyzes the model and creates endpoints for each file receive location.
    /// </summary>
    public class RP002FileReceivePortAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "RP002";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="RP002FileReceivePortAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public RP002FileReceivePortAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(RP002FileReceivePortAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the file endpoints in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get parsed BizTalk model from the application model
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();
            if (parsedApplicationGroup?.Applications != null)
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(RP002FileReceivePortAnalyzer));

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
                                    if (receiveLocation.ReceiveLocationTransportType.Name == "FILE")
                                    {
                                        // Find adapter in target model
                                        var adapterKey = $"{ModelConstants.MessageBusLeafKey}:{application.Application.Name.FormatKey()}:{receivePort.Name.FormatKey()}:{receiveLocation.Name.FormatKey()}:{ModelConstants.AdapterEndpointLeafKey}";
                                        var adapter = Model.FindMessagingObject(adapterKey);
                                        if (adapter.messagingObject != null)
                                        {
                                            var fileAdapter = (AdapterEndpoint)adapter.messagingObject;

                                            // Set conversion rating
                                            fileAdapter.Rating = ConversionRating.FullConversionWithFidelityLoss;

                                            // Change to the accept message exchange pattern as no ack can be delivered for file receives.
                                            fileAdapter.MessageExchangePattern = MessageExchangePattern.Accept;

                                            // Set resource map key to hook into the configuration process
                                            var messagingObject = Model.FindMessagingObject(fileAdapter.Key);
                                            var appName = $"{messagingObject.application.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-")}";
                                            var adapterName = fileAdapter.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-");
                                            fileAdapter.ResourceMapKey = $"fileReceiveAdapterEndpoint{appName}{adapterName}";

                                            // Handle adapter properties
                                            if (!string.IsNullOrEmpty(receiveLocation.ReceiveLocationTransportTypeData))
                                            {
                                                var configItems = MapTransportTypeData(receiveLocation.ReceiveLocationTransportTypeData);

                                                // Add the address to the config items.
                                                var address = receiveLocation.Address;
                                                if (!string.IsNullOrEmpty(address))
                                                {
                                                    if (configItems.TryGetValue("FileMask", out var fileMask))
                                                    {
                                                        address = address.Replace(fileMask, string.Empty);
                                                    }
                                                    // Replace the path separator to ensure its can be processed by the Azure CLI.
                                                    address = address.Replace("\\", "/");
                                                }
                                                configItems.Add("Address", address);

                                                MapAdapterProperties(configItems, fileAdapter);

                                                // Map the config.
                                                var configProperties = fileAdapter.Properties[ModelConstants.ConfigurationEntry] as Dictionary<string, object>;
                                                configProperties["failedMessageRouting"] = receivePort.RouteFailedMessage;
                                                configProperties["fileMask"] = fileAdapter.Properties["fileMask"];
                                                
                                                foreach (var item in configItems)
                                                {
                                                    fileAdapter.ReportMessages.Add(new ReportMessage()
                                                    {
                                                        Severity = MessageSeverity.Warning,                                                        
                                                        Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkFileAdapterPropertyNotSupported, item.Key, item.Value)
                                                    });
                                                }

                                                fileAdapter.ReportLinks.Add(AnalysisResources.FileAdapterHelpLink);
                                            }
                                            else
                                            {
                                                _logger.LogDebug(WarningMessages.ReceiveLocationTransportTypeDataNotFound, fileAdapter.Name);

                                                fileAdapter.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.ReceiveLocationTransportTypeDataNotFound, fileAdapter.Name) });
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
                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(RP002FileReceivePortAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(RP002FileReceivePortAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Map the fields that are to be used by the azure file connector, expected fields that are not found are added with a default value or a place holder text.
        /// </summary>
        /// <param name="configItems"></param>
        /// <param name="endpoint"></param>
        private void MapAdapterProperties(Dictionary<string, string> configItems, AdapterEndpoint endpoint)
        {
            // Set supported property names and defaults
            var supportedProperties = new Dictionary<string, (string, object)>()
            {
                { "Address", ("rootFolder", "")},
                { "BatchSize", ("maxFileCount", 10)},
                { "FileMask", ("fileMask", "*.*")},
                { "PollingInterval", ("recurrence", 60000)},
                { "Username", ("userName", "")}
            };

            // Search through the BizTalk adapter properties and map them over to their AIS equivalents.
            foreach (var supportedProperty in supportedProperties)
            {
                (string mappedName, object mappedValue) mappedProperty = supportedProperty.Value;

                if (configItems.ContainsKey(supportedProperty.Key) && !string.IsNullOrEmpty(configItems[supportedProperty.Key]))
                {
                    var convertedValue = ConvertAdapterProperty(supportedProperty.Key, configItems[supportedProperty.Key]);

                    // Set the value on the endpoint
                    endpoint.Properties.Add(mappedProperty.mappedName, convertedValue);

                    _logger.LogDebug(TraceMessages.BizTalkFileReceiveAdapterBindingPropertyFound, RuleName, supportedProperty.Key, endpoint.Name, convertedValue);

                    // Remove the property from the BizTalk adapter property list.
                    configItems.Remove(supportedProperty.Key);
                }
                else
                {
                    // Set default value
                    endpoint.Properties.Add(mappedProperty.mappedName, mappedProperty.mappedValue);

                    _logger.LogDebug(TraceMessages.BizTalkFileReceiveAdapterBindingPropertyNotFound, RuleName, supportedProperty.Key, endpoint.Name, mappedProperty.mappedValue);
                }
            }

            // Passwords are a special case password and will be removed as they contain sensitive information.
            endpoint.Properties.Add("password", "");
            endpoint.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Information, Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkFileAdapterSensitivePropertyMustBeSpecifiedLater, "password") });
            configItems.Remove("Password");

            _logger.LogDebug(TraceMessages.BizTalkFileReceiveAdapterBindingPropertySensitive, RuleName, "Password", endpoint.Name);
        }

        /// <summary>
        /// Map certain property values to new values for AIS.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyValue">The value of the property.</param>
        /// <returns>The new converted value.</returns>
        private static object ConvertAdapterProperty(string propertyName, object propertyValue)
        {
            if (propertyName == "PollingInterval")
            {
                return int.Parse((string)propertyValue, CultureInfo.InvariantCulture) / 1000;
            }
            else
            {
                return propertyValue;
            }
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
