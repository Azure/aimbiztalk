﻿// Copyright (c) Microsoft Corporation.
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
    /// Implements rule RP001, which analyzes the model and creates EndPoints for each application receive ftp location.
    /// </summary>
    public sealed class RP001FtpReceivePortAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "RP001";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="RP001FtpReceivePortAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public RP001FtpReceivePortAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(RP001FtpReceivePortAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the ftp EndPoints for applications in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get parsed BizTalk model from the application model
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();
            if (parsedApplicationGroup?.Applications != null)
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(RP001FtpReceivePortAnalyzer));

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
                                    if (receiveLocation.ReceiveLocationTransportType.Name == "FTP")
                                    {
                                        // Find adapter in target model
                                        var adapterKey = $"{ModelConstants.MessageBusLeafKey}:{application.Application.Name.FormatKey()}:{receivePort.Name.FormatKey()}:{receiveLocation.Name.FormatKey()}:{ModelConstants.AdapterEndpointLeafKey}";
                                        var adapter = Model.FindMessagingObject(adapterKey);
                                        if (adapter.messagingObject != null)
                                        {
                                            var ftpAdapter = (AdapterEndpoint)adapter.messagingObject;

                                            // Set conversion rating
                                            ftpAdapter.Rating = ConversionRating.FullConversionWithFidelityLoss;

                                            // Change to Accept message exchange pattern as no ack can be delivered for FTP
                                            ftpAdapter.MessageExchangePattern = MessageExchangePattern.Accept;

                                            // Set resource map key to hook into the configuration process
                                            var messagingObject = Model.FindMessagingObject(ftpAdapter.Key);
                                            var appName = $"{messagingObject.application.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-")}";
                                            var adapterName = ftpAdapter.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-");
                                            ftpAdapter.ResourceMapKey = $"ftpReceiveAdapterEndpoint{appName}{adapterName}";

                                            // Handle adapter properties
                                            if (!string.IsNullOrEmpty(receiveLocation.ReceiveLocationTransportTypeData))
                                            {
                                                var configItems = MapTransportTypeData(receiveLocation.ReceiveLocationTransportTypeData);

                                                MapAdapterProperties(configItems, ftpAdapter);

                                                foreach (var item in configItems)
                                                {
                                                    ftpAdapter.ReportMessages.Add(new ReportMessage()
                                                    {
                                                        Severity = MessageSeverity.Warning,
                                                        Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkFtpAdapterPropertyNotSupported, item.Key, item.Value)
                                                    });
                                                }

                                                ftpAdapter.ReportLinks.Add(AnalysisResources.FtpAdapterHelpLink);
                                            }
                                            else
                                            {
                                                _logger.LogDebug(WarningMessages.ReceiveLocationTransportTypeDataNotFound, ftpAdapter.Name);

                                                ftpAdapter.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.ReceiveLocationTransportTypeDataNotFound, ftpAdapter.Name) });
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

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(RP001FtpReceivePortAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(RP001FtpReceivePortAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Map the fields that are to be used by the azure ftp connector, expected fields that are not found are added with default value or a place holder text.
        /// </summary>
        /// <param name="configItems"></param>
        /// <param name="endpoint"></param>
        private void MapAdapterProperties(Dictionary<string, string> configItems, AdapterEndpoint endpoint)
        {
            // Set supported property names and defaults
            var supportedProperties = new Dictionary<string, (string, object)>()
            {
                { "serverAddress", ("serverAddress", "localhost") },
                { "serverPort", ("serverPort", 21) },
                { "userName", ("userName", "temp.user") },
                { "representationType", ("isBinaryTransport", true) },
                { "useSsl", ("isSSL", false) },
                { "targetFolder", ("targetFolder", "/") },
                { "fileMask", ("fileMask", "*.*") },
                { "pollingUnitOfMeasure", ("frequency", "Seconds") },
                { "pollingInterval", ("recurrence", 60) },
                { "maximumNumberOfFiles", ("maxFileCount", 10) },
                { "maxFileSize", ("maxFileSize", 100) }
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

                    _logger.LogDebug(TraceMessages.BizTalkFtpReceiveAdapterBindingPropertyFound, RuleName, supportedProperty.Key, endpoint.Name, convertedValue);

                    // Remove handled property from BizTalk adapter property list
                    configItems.Remove(supportedProperty.Key);
                }
                else
                {
                    // Set default value
                    endpoint.Properties.Add(mappedProperty.mappedName, mappedProperty.mappedValue);

                    _logger.LogDebug(TraceMessages.BizTalkFtpReceiveAdapterBindingPropertyNotFound, RuleName, supportedProperty.Key, endpoint.Name, mappedProperty.mappedValue);
                }
            }

            // Special case password because it is a sensitive value
            endpoint.Properties.Add("password", "");
            endpoint.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Information, Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkFtpAdapterSensitivePropertyMustBeSpecifiedLater, "password") });
            configItems.Remove("password");

            _logger.LogDebug(TraceMessages.BizTalkFtpReceiveAdapterBindingPropertySensitive, RuleName, "password", endpoint.Name);
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
                case "representationType":

                    return propertyValue.ToString() == "binary" ? true : false;

                case "useSsl":

                    return propertyValue.ToString() == "False" ? false : true;

                case "maximumNumberOfFiles":

                    if ((propertyValue is int maxFileCount) && maxFileCount > 0)
                    {
                        return maxFileCount;
                    }
                    else
                    {
                        return 10;
                    }

                case "targetFolder":

                    return propertyValue.ToString().StartsWith("/") ? propertyValue : "/" + propertyValue.ToString();

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
