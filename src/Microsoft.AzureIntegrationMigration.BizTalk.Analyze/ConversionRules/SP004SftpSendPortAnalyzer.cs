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
    /// Implements rule SP004, which analyzes the model and creates EndPoints for each application send sftp location.
    /// </summary>
    public sealed class SP004SftpSendPortAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "SP004";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="SP004SftpSendPortAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public SP004SftpSendPortAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(SP004SftpSendPortAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the SFTP send EndPoints for applications in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get parsed BizTalk model from the application model
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();
            if (parsedApplicationGroup?.Applications != null)
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(SP004SftpSendPortAnalyzer));

                foreach (var application in parsedApplicationGroup.Applications)
                {
                    var sendPorts = application.Application?.Bindings?.BindingInfo?.SendPortCollection;
                    if (sendPorts != null && sendPorts.Any())
                    {
                        foreach(var sendPort in sendPorts)
                        {
                            if (sendPort?.PrimaryTransport?.TransportType?.Name == "SFTP")
                            {
                                // Find adapter in target model
                                var adapterKey = $"{ModelConstants.MessageBusLeafKey}:{application.Application.Name.FormatKey()}:{sendPort.Name.FormatKey()}:{ModelConstants.AdapterEndpointLeafKey}";
                                var adapter = Model.FindMessagingObject(adapterKey);
                                if (adapter.messagingObject != null)
                                {
                                    var sftpAdapter = (AdapterEndpoint)adapter.messagingObject;

                                    // Set conversion rating
                                    sftpAdapter.Rating = ConversionRating.FullConversionWithFidelityLoss;

                                    // We're using a Send pattern
                                    sftpAdapter.MessageExchangePattern = MessageExchangePattern.Send;

                                    // Set resource map key to hook into the configuration process
                                    var messagingObject = Model.FindMessagingObject(sftpAdapter.Key);
                                    var appName = $"{messagingObject.application.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-")}";
                                    var adapterName = sftpAdapter.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-");
                                    sftpAdapter.ResourceMapKey = $"sftpSendAdapterEndpoint{appName}{adapterName}";

                                    if (!string.IsNullOrEmpty(sendPort.PrimaryTransport.TransportTypeData))
                                    {
                                        var configItems = MapTransportTypeData(sendPort.PrimaryTransport.TransportTypeData);

                                        MapAdapterProperties(configItems, sftpAdapter);

                                        foreach (var item in configItems)
                                        {
                                            sftpAdapter.ReportMessages.Add(new ReportMessage()
                                            {
                                                Severity = MessageSeverity.Warning,
                                                Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkSftpAdapterPropertyNotSupported, item.Key, item.Value)
                                            });
                                        }

                                        sftpAdapter.ReportLinks.Add(AnalysisResources.SftpAdapterHelpLink);
                                    }
                                    else
                                    {
                                        _logger.LogDebug(WarningMessages.SendPortTransportTypeDataNotFound, sftpAdapter.Name);
                                        sftpAdapter.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.SendPortTransportTypeDataNotFound, sftpAdapter.Name) });
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

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(SP004SftpSendPortAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(SP004SftpSendPortAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Map the fields that are to be used by the azure sftp connector, expected fields that are not found are added with default value or a place holder text.
        /// </summary>
        /// <param name="configItems">The adapter configuration properties.</param>
        /// <param name="endpoint">The sftp endpoint.</param>
        private void MapAdapterProperties(Dictionary<string, string> configItems, AdapterEndpoint endpoint)
        {
            // Set supported property names and defaults
            var supportedProperties = new Dictionary<string, (string, object)>()
            {
                { "ServerAddress", ("serverAddress", "") },
                { "Port", ("serverPort", 22) },
                { "UserName", ("userName", "") },
                { "PrivateKey", ("sshPrivateKey", "") },
                { "AccessAnySSHServerHostKey", ("acceptAnySshHostKey", "false") },
                { "SSHServerHostKey", ("sshHostKeyFingerprint", "") },
                { "ClientAuthenticationMode", ("clientAuthenticationMode", "Password")},
                { "targetFolder", ("targetFolder", "/") },
                { "targetFileName", ("targetFileName", "%MessageID%.xml") }
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

                    _logger.LogDebug(TraceMessages.BizTalkSftpSendAdapterBindingPropertyFound, RuleName, supportedProperty.Key, endpoint.Name, convertedValue);

                    // Remove handled property from BizTalk adapter property list
                    configItems.Remove(supportedProperty.Key);
                }
                else
                {
                    // Set default value
                    endpoint.Properties.Add(mappedProperty.mappedName, mappedProperty.mappedValue);

                    _logger.LogDebug(TraceMessages.BizTalkSftpSendAdapterBindingPropertyNotFound, RuleName, supportedProperty.Key, endpoint.Name, mappedProperty.mappedValue);
                }
            }

            // Special case for Password or PrivateKeyPassword because they are sensitive values
            // Check the ClientAuthenticationMode
            var authenticationMode = endpoint.Properties["clientAuthenticationMode"] as string;
            if (authenticationMode == "Password")
            {
                endpoint.Properties.Add("password", "<redacted>");
                _logger.LogDebug(TraceMessages.BizTalkSftpReceiveAdapterBindingPropertySensitive, RuleName, "password", endpoint.Name);
                endpoint.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Information, Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkSftpAdapterSensitivePropertyMustBeSpecifiedLater, "password") });
            }
            else if (authenticationMode == "PublicKeyAuthentication")
            {
                endpoint.Properties.Add("sshPrivateKeyPassphrase", "<redacted>");
                _logger.LogDebug(TraceMessages.BizTalkSftpReceiveAdapterBindingPropertySensitive, RuleName, "sshPrivateKeyPassphrase", endpoint.Name);
                endpoint.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Information, Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkSftpAdapterSensitivePropertyMustBeSpecifiedLater, "sshPrivateKeyPassphrase") });
            }
            else
            {
                // Unsupported ClientAuthenticationMode (e.g. MFA)
                endpoint.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.BizTalkSftpAdapterPropertyNotSupported, "clientAuthenticationMode", authenticationMode) });
            }
            configItems.Remove("clientAuthenticationMode");
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
                case "AccessAnySSHServerHostKey":
                    return propertyValue.ToString() == "-1" ? "true" : "false";
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
