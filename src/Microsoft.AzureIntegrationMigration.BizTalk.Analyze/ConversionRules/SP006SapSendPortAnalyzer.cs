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
    /// Implements rule SP006, which analyzes the model and creates EndPoints for each application send sap location.
    /// </summary>
    public sealed class SP006SapSendPortAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "SP006";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="SP006SapSendPortAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public SP006SapSendPortAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(SP006SapSendPortAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the SAP send EndPoints for applications in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get parsed BizTalk model from the application model
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();
            if (parsedApplicationGroup?.Applications != null)
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(SP006SapSendPortAnalyzer));

                foreach (var application in parsedApplicationGroup.Applications)
                {
                    var sendPorts = application.Application?.Bindings?.BindingInfo?.SendPortCollection;
                    if (sendPorts != null && sendPorts.Any())
                    {
                        foreach(var sendPort in sendPorts)
                        {
                            // We're looking either for the WCF-SAP adapter, or WCF-Custom with a binding type of "sapBinding"
                            if (sendPort?.PrimaryTransport?.TransportType?.Name == "WCF-SAP" || (sendPort?.PrimaryTransport?.TransportType?.Name == "WCF-Custom" && GetBindingType(sendPort?.PrimaryTransport?.TransportTypeData) == "sapBinding"))
                            {
                                // Find adapter in target model
                                var adapterKey = $"{ModelConstants.MessageBusLeafKey}:{application.Application.Name.FormatKey()}:{sendPort.Name.FormatKey()}:{ModelConstants.AdapterEndpointLeafKey}";
                                var adapter = Model.FindMessagingObject(adapterKey);
                                if (adapter.messagingObject != null)
                                {
                                    var sapAdapter = (AdapterEndpoint)adapter.messagingObject;

                                    // Set conversion rating
                                    sapAdapter.Rating = ConversionRating.FullConversionWithFidelityLoss;

                                    // We're using a Send pattern
                                    sapAdapter.MessageExchangePattern = MessageExchangePattern.Send;

                                    // Set resource map key to hook into the configuration process
                                    var messagingObject = Model.FindMessagingObject(sapAdapter.Key);
                                    var appName = $"{messagingObject.application.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-")}";
                                    var adapterName = sapAdapter.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-");
                                    sapAdapter.ResourceMapKey = $"sapSendAdapterEndpoint{appName}{adapterName}";

                                    // SAP Adapter settings are held in both the TransportTypeData and the PrimaryEndpoint Address field
                                    // Note that we don't currently support secondary endpoints.

                                    // Process TransportTypeData settings
                                    if (!string.IsNullOrEmpty(sendPort.PrimaryTransport.TransportTypeData))
                                    {
                                        var configItems = MapTransportTypeData(sendPort.PrimaryTransport.TransportTypeData);

                                        MapAdapterProperties(configItems, sapAdapter);

                                        foreach (var item in configItems)
                                        {
                                            sapAdapter.ReportMessages.Add(new ReportMessage()
                                            {
                                                Severity = MessageSeverity.Warning,
                                                Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkSapAdapterPropertyNotSupported, item.Key, item.Value)
                                            });
                                        }

                                        sapAdapter.ReportLinks.Add(AnalysisResources.SapAdapterHelpLink);
                                    }
                                    else
                                    {
                                        _logger.LogDebug(WarningMessages.SendPortTransportTypeDataNotFound, sapAdapter.Name);
                                        sapAdapter.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.SendPortTransportTypeDataNotFound, sapAdapter.Name) });
                                    }

                                    // Process PrimaryEndpointAddress settings
                                    if (!string.IsNullOrEmpty(sendPort.PrimaryTransport.Address))
                                    {
                                        var configItems = MapAddressData(sendPort.PrimaryTransport.Address);

                                        MapAdapterAddressProperties(configItems, sapAdapter);

                                        foreach (var item in configItems)
                                        {
                                            sapAdapter.ReportMessages.Add(new ReportMessage()
                                            {
                                                Severity = MessageSeverity.Warning,
                                                Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkSapAdapterPropertyNotSupported, item.Key, item.Value)
                                            });
                                        }

                                        sapAdapter.ReportLinks.Add(AnalysisResources.SapAdapterHelpLink);
                                    }
                                    else
                                    {
                                        _logger.LogDebug(WarningMessages.SendPortAddressDataNotFound, sapAdapter.Name);
                                        sapAdapter.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.SendPortAddressDataNotFound, sapAdapter.Name) });
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

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(SP006SapSendPortAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(SP006SapSendPortAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Map the fields that are to be used by the azure sap connector and which come from the address; 
        /// expected fields that are not found are added with default value or a place holder text.
        /// </summary>
        /// <param name="configItems">The adapter configuration properties.</param>
        /// <param name="endpoint">The sap endpoint.</param>
        private void MapAdapterAddressProperties(Dictionary<string, string> configItems, AdapterEndpoint endpoint)
        {
            // Set supported property names and defaults
            var supportedProperties = new Dictionary<string, (string, object)>()
            {
                { "ConnectionType", ("connectionType", string.Empty) },
                { "ApplicationServerHost", ("applicationServerHost", string.Empty) },
                { "SystemNumber", ("applicationServerSystemNumber", string.Empty) },
                { "MessageServerHost", ("messageServerHost", string.Empty) },
                { "R3SystemName", ("messageServerSystemName", string.Empty) },
                { "DestinationName", ("sapRfcIniDestinationName", string.Empty) },
                { "CLIENT", ("clientNumber", "800") },
                { "LANG", ("language", "EN") },
                { "GWHOST", ("applicationServerGatewayHost", string.Empty) },
                { "GWSERV", ("applicationServerGatewayService", string.Empty) },
                { "MSSERV", ("messageServerService", string.Empty) },
                { "GROUP", ("messageServerApplicationServerGroupName", string.Empty) },
                { "ListenerDest", ("rfcServerSapRfcIniDestinationName", string.Empty) },
                { "ListenerGwServ", ("rfcServerGatewayService", string.Empty) },
                { "ListenerGwHost", ("rfcServerGatewayHost", string.Empty) },
                { "ListenerProgramId", ("rfcServerProgramId", string.Empty) },
                { "RfcSdkTrace", ("rfcSdkTraceEnabled", false) },
                { "AbapDebug", ("abapDebugEnabled", false) },
                { "UseSnc", ("useSnc", false) }
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

                    _logger.LogDebug(TraceMessages.BizTalkSapSendAdapterBindingPropertyFound, RuleName, supportedProperty.Key, endpoint.Name, convertedValue);

                    // Remove handled property from BizTalk adapter property list
                    configItems.Remove(supportedProperty.Key);
                }
                else
                {
                    // Set default value
                    endpoint.Properties.Add(mappedProperty.mappedName, mappedProperty.mappedValue);

                    _logger.LogDebug(TraceMessages.BizTalkSapSendAdapterBindingPropertyNotFound, RuleName, supportedProperty.Key, endpoint.Name, mappedProperty.mappedValue);
                }
            }
        }

        /// <summary>
        /// Map the fields that are to be used by the azure sap connector, expected fields that are not found are added with default value or a place holder text.
        /// </summary>
        /// <param name="configItems">The adapter configuration properties.</param>
        /// <param name="endpoint">The sap endpoint.</param>
        private void MapAdapterProperties(Dictionary<string, string> configItems, AdapterEndpoint endpoint)
        {
            // Set supported property names and defaults
            var supportedProperties = new Dictionary<string, (string, object)>()
            {
                { "UserName", ("userName", "") },
                { "Password", ("password", "") },
                { "EnableTransaction", ("enableTransaction", false) }
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

                    _logger.LogDebug(TraceMessages.BizTalkSapSendAdapterBindingPropertyFound, RuleName, supportedProperty.Key, endpoint.Name, convertedValue);

                    // Remove handled property from BizTalk adapter property list
                    configItems.Remove(supportedProperty.Key);
                }
                else
                {
                    // Set default value
                    endpoint.Properties.Add(mappedProperty.mappedName, mappedProperty.mappedValue);

                    _logger.LogDebug(TraceMessages.BizTalkSapSendAdapterBindingPropertyNotFound, RuleName, supportedProperty.Key, endpoint.Name, mappedProperty.mappedValue);
                }
            }

            // Special case password because it is a sensitive value
            endpoint.Properties.Add("password", "");
            endpoint.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Information, Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkSapAdapterSensitivePropertyMustBeSpecifiedLater, "password") });
            configItems.Remove("password");
            _logger.LogDebug(TraceMessages.BizTalkSapSendAdapterBindingPropertySensitive, RuleName, "password", endpoint.Name);
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
                case "EnableTransaction":
                    return propertyValue.ToString() == "-1" ? false : true;
            }

            return propertyValue;
        }

        /// <summary>
        /// Map certain property values to new values for AIS.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyValue">The value of the property.</param>
        /// <returns>The new converted value.</returns>
        private static object ConvertSapAddressProperty(string propertyName, object propertyValue)
        {
            switch (propertyName)
            {
                case "UseSnc":
                case "RfcSdkTrace":
                case "AbapDebug":
                    return propertyValue.ToString().ToLower() == "true" ? true : false;
            }

            return propertyValue;
        }

        /// <summary>
        /// Convert the a BizTalk adapter Address to a dictionary.
        /// </summary>
        /// <param name="address">The address value.</param>
        /// <returns>A dictionary with the key value types.</returns>
        private static Dictionary<string, string> MapAddressData(string address)
        {
            var items = new Dictionary<string, string>();

            // The address should always start with "sap://" - if it doesn't, then the send port isn't configured as a SAP adapter.
            if (address.StartsWith("sap://", StringComparison.OrdinalIgnoreCase))
            {
                address = address.Substring(6);

                var addressParts = address.Split(';');
                foreach (var addressPart in addressParts)
                {
                    if (addressPart.StartsWith("@"))
                    {
                        var uriParts = addressPart.Split('?');
                        if (uriParts.Length == 2)
                        {
                            // First part is a path, containing the connection type and the server details
                            // Second part is a query string, containing a set of KeyValue pairs, separated by "&"

                            // Process First Part
                            var pathParts = uriParts[0].Split('/');
                            if (pathParts.Length > 1)
                            {
                                var connectionTypeCode = pathParts[0].Substring(1);
                                switch (connectionTypeCode.ToLower())
                                {
                                    case "a":
                                        {
                                            items.Add("ConnectionType", "ApplicationServer");
                                            items.Add("ApplicationServerHost", pathParts[1]);
                                            if (pathParts.Length == 3)
                                            {
                                                items.Add("SystemNumber", pathParts[2]);
                                            }

                                            // Add the other ConnectionType values so we don't get a warning about them
                                            items.Add("MessageServerHost", string.Empty);
                                            items.Add("R3SystemName", string.Empty);
                                            items.Add("DestinationName", string.Empty);
                                            break;
                                        }
                                    case "b":
                                        {
                                            items.Add("ConnectionType", "MessageServer");
                                            items.Add("MessageServerHost", pathParts[1]);
                                            if (pathParts.Length == 3)
                                            {
                                                items.Add("R3SystemName", pathParts[2]);
                                            }

                                            // Add the other ConnectionType values so we don't get a warning about them
                                            items.Add("ApplicationServerHost", string.Empty);
                                            items.Add("SystemNumber", string.Empty);
                                            items.Add("DestinationName", string.Empty);
                                            break;
                                        }
                                    case "d":
                                        {
                                            items.Add("ConnectionType", "MessageServer");
                                            items.Add("DestinationName", pathParts[1]);

                                            // Add the other ConnectionType values so we don't get a warning about them
                                            items.Add("ApplicationServerHost", string.Empty);
                                            items.Add("SystemNumber", string.Empty);
                                            items.Add("MessageServerHost", string.Empty);
                                            items.Add("R3SystemName", string.Empty);
                                            break;
                                        }
                                    default:
                                        {
                                            // Unsupported connection type
                                            break;
                                        }
                                }
                            }

                            // Process second part
                            var queryParts = uriParts[1].Split('&');
                            foreach (var queryPart in queryParts)
                            {
                                // This should be a KeyValue pair
                                ProcessKeyValuePair(queryPart, items);
                            }
                        }
                    }
                    else
                    {
                        // This should be a KeyValue pair
                        ProcessKeyValuePair(addressPart, items);
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// Processes a string containing a KeyValue pair, in the form "key=value" and adds the pair to the supplied dictionary.
        /// </summary>
        /// <param name="keyValuePair">String containing KeyValue pair.</param>
        /// <param name="items"><see cref="IDictionary{string, string}"/> containing config items.</param>
        private static void ProcessKeyValuePair(string keyValuePair, IDictionary<string, string> items)
        {
            if (!string.IsNullOrWhiteSpace(keyValuePair) && keyValuePair.Contains("="))
            {
                var keyValueParts = keyValuePair.Split('=');
                if (keyValueParts.Length == 2)
                {
                    items.Add(keyValueParts[0], keyValueParts[1]);
                }
            }
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
            var node = docCustomProps.SelectSingleNode("/CustomProps");

            var items = new Dictionary<string, string>();

            if (node != null)
            {
                foreach (var cnode in node.ChildNodes.OfType<XmlElement>())
                {
                    items.Add(cnode.Name, cnode.InnerText);
                }
            }

            // Now add binding info
            var bindingNode = docCustomProps.SelectSingleNode("/CustomProps/BindingConfiguration/binding");
            if (bindingNode != null)
            {
                foreach (var cnode in bindingNode.ChildNodes.OfType<XmlElement>())
                {
                    items.Add(cnode.Name, cnode.InnerText);
                }
            }

            return items;
        }

        /// <summary>
        /// Gets the binding type for this send port.
        /// </summary>
        /// <param name="transportTypeData">The xml of the transport type</param>
        /// <returns>The bindingType, or null if none found.</returns>
        private static string GetBindingType(string transportTypeData)
        {
            if (string.IsNullOrWhiteSpace(transportTypeData)) return null;

            var decodedCustomProps = HttpUtility.HtmlDecode(transportTypeData);
            var docCustomProps = new XmlDocument() { XmlResolver = null };
#pragma warning disable CA3075 // Insecure DTD processing in XML
            docCustomProps.LoadXml(decodedCustomProps);
#pragma warning restore CA3075 // Insecure DTD processing in XML
            var node = docCustomProps.SelectSingleNode("/CustomProps/BindingType");
            if (node != null)
            {
                return node.Value;
            }

            return null;
        }
    }
}
