// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="BizTalkAnalyzerBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Default behaviour for an analyzer.</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze
{
    /// <summary>
    /// Default behaviour for an analyzer.
    /// </summary>
    public abstract class BizTalkAnalyzerBase : IBizTalkAnalyzer
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of the analyzer.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Default constructor for dependency injection.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="logger">An instance of a <see cref="ILogger"/> to be used for logging within the class.</param>
        protected BizTalkAnalyzerBase(string analyzerName, IApplicationModel model, MigrationContext context, ILogger logger)
        {
            // Validate and set the members.
            _name = analyzerName ?? throw new ArgumentNullException(nameof(analyzerName));
            Model = model as AzureIntegrationServicesModel ?? throw new ArgumentNullException(nameof(model));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets or sets the <see cref="MigrationContext"/> that is being analyzed.
        /// </summary>
        protected MigrationContext Context { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AzureIntegrationServicesModel"/> being analyzed.
        /// </summary>
        protected AzureIntegrationServicesModel Model { get; set; }

        /// <summary>
        /// Analyzes the contents of the parsed source and builds the output report.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        public async Task AnalyzeAsync(CancellationToken token)
        {
            _logger.LogInformation(InformationMessages.RunningBizTalkAnalyzer, Name);

            await AnalyzeInternalAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the name of the BizTalk discoverers.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Performs the work of analyzing.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected abstract Task AnalyzeInternalAsync(CancellationToken token);


        /// <summary>
        /// Maps well known BizTalk filter property names to suitable new values for AIS, otherwise
        /// returns the input filter property name formatted for AIS.
        /// </summary>
        /// <param name="filterPropertyName">The filter property to map.</param>
        /// <returns>A mapped and formatted filter property name.</returns>
        protected string MapSubscriptionFilterProperty(string filterPropertyName)
        {
            var mappedFilterPropertyName = filterPropertyName;

            switch (filterPropertyName)
            {
                case "BTS.AckFailureCategory":

                    mappedFilterPropertyName = ModelConstants.BizTalkAckFailureCategory;
                    break;

                case "BTS.AckFailureCode":

                    mappedFilterPropertyName = ModelConstants.BizTalkAckFailureCode;
                    break;

                case "BTS.AckID":

                    mappedFilterPropertyName = ModelConstants.BizTalkAckId;
                    break;

                case "BTS.AckInboundTransportLocation":

                    mappedFilterPropertyName = ModelConstants.BizTalkAckInboundTransportLocation;
                    break;

                case "BTS.AckOutboundTransportLocation":

                    mappedFilterPropertyName = ModelConstants.BizTalkAckOutboundTransportLocation;
                    break;

                case "BTS.AckOwnerID":

                    mappedFilterPropertyName = ModelConstants.BizTalkAckOwnerId;
                    break;

                case "BTS.AckReceivePortID":

                    mappedFilterPropertyName = ModelConstants.BizTalkAckReceivePortId;
                    break;

                case "BTS.AckReceivePortName":

                    mappedFilterPropertyName = ModelConstants.BizTalkAckReceivePortName;
                    break;

                case "BTS.AckSendPortID":

                    mappedFilterPropertyName = ModelConstants.BizTalkAckSendPortId;
                    break;

                case "BTS.AckSendPortName":

                    mappedFilterPropertyName = ModelConstants.BizTalkAckSendPortName;
                    break;

                case "BTS.AckType":

                    mappedFilterPropertyName = ModelConstants.BizTalkAckType;
                    break;

                case "BTS.ActionOnFailure":

                    mappedFilterPropertyName = ModelConstants.BizTalkActionOnFailure;
                    break;

                case "BTS.CorrelationToken":

                    mappedFilterPropertyName = ModelConstants.BizTalkCorrelationToken;
                    break;

                case "BTS.InboundTransportLocation":

                    mappedFilterPropertyName = ModelConstants.BizTalkInboundTransportLocation;
                    break;

                case "BTS.InboundTransportType":

                    mappedFilterPropertyName = ModelConstants.BizTalkInboundTransportType;
                    break;

                case "BTS.InterchangeSequenceNumber":

                    mappedFilterPropertyName = ModelConstants.BizTalkInterchangeSequenceNumber;
                    break;

                case "BTS.IsDynamicSend":

                    mappedFilterPropertyName = ModelConstants.BizTalkIsDynamicSend;
                    break;

                case "BTS.MessageDestination":

                    mappedFilterPropertyName = ModelConstants.BizTalkMessageDestination;
                    break;

                case "BTS.MessageType":

                    mappedFilterPropertyName = ModelConstants.BizTalkMessageType;
                    break;

                case "BTS.OutboundTransportLocation":

                    mappedFilterPropertyName = ModelConstants.BizTalkOutboundTransportLocation;
                    break;

                case "BTS.OutboundTransportType":

                    mappedFilterPropertyName = ModelConstants.BizTalkOutboundTransportType;
                    break;

                case "BTS.PropertiesToUpdate":

                    mappedFilterPropertyName = ModelConstants.BizTalkPropertiesToUpdate;
                    break;

                case "BTS.ReceivePipelineID":

                    mappedFilterPropertyName = ModelConstants.BizTalkReceivePipelineId;
                    break;

                case "BTS.ReceivePortID":

                    mappedFilterPropertyName = ModelConstants.BizTalkReceivePortId;
                    break;

                case "BTS.ReceivePortName":

                    mappedFilterPropertyName = ModelConstants.BizTalkReceivePortName;
                    break;

                case "BTS.SPGroupID":

                    mappedFilterPropertyName = ModelConstants.BizTalkSpGroupId;
                    break;

                case "BTS.SPID":

                    mappedFilterPropertyName = ModelConstants.BizTalkSpId;
                    break;

                case "BTS.SPName":

                    mappedFilterPropertyName = ModelConstants.BizTalkSpName;
                    break;

                case "BTS.SPTransportBackupID":

                    mappedFilterPropertyName = ModelConstants.BizTalkSpTransportBackupId;
                    break;

                case "BTS.SPTransportID":

                    mappedFilterPropertyName = ModelConstants.BizTalkSpTransportId;
                    break;

                case "BTS.SuspendAsNonResumable":

                    mappedFilterPropertyName = ModelConstants.BizTalkSuspendAsNonResumable;
                    break;

                case "BTS.SuspendMessageOnRoutingFailure":

                    mappedFilterPropertyName = ModelConstants.BizTalkSuspendMessageOnRoutingFailure;
                    break;

                case "BTS.AckDescription":

                    mappedFilterPropertyName = ModelConstants.BizTalkAckDescription;
                    break;

                case "BTS.InterchangeID":

                    mappedFilterPropertyName = ModelConstants.BizTalkInterchangeId;
                    break;

                case "BTS.Loopback":

                    mappedFilterPropertyName = ModelConstants.BizTalkLoopback;
                    break;

                case "BTS.SignatureCertificate":

                    mappedFilterPropertyName = ModelConstants.BizTalkSignatureCertificate;
                    break;

                case "BTS.SourcePartyID":

                    mappedFilterPropertyName = ModelConstants.BizTalkSourcePartyId;
                    break;

                case "BTS.SSOTicket":

                    mappedFilterPropertyName = ModelConstants.BizTalkSsoTicket;
                    break;

                case "BTS.WindowsUser":

                    mappedFilterPropertyName = ModelConstants.BizTalkWindowsUser;
                    break;

                case "BTS.Operation":

                    mappedFilterPropertyName = ModelConstants.BizTalkOperation;
                    break;
            }

            // Format property name
            mappedFilterPropertyName = mappedFilterPropertyName.Replace(".", string.Empty);

            _logger.LogTrace(TraceMessages.MappedSubscriptionFilterPropertyName, _name, filterPropertyName, mappedFilterPropertyName);

            return mappedFilterPropertyName;
        }
    }
}
