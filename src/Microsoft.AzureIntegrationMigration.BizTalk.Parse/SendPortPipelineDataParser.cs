// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="SendPortPipelineDataParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a class used to parse the custom pipeline data configuation found in the bindings, for send ports.</summary>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Linq;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Represents a class used to parse the custom pipeline data configuation found in the bindings, for send ports.
    /// </summary>
    public sealed class SendPortPipelineDataParser : PipelineDataParser
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of this parser.
        /// </summary>
        public const string ParserName = ModelConstants.ApplicationPrefix + "sendportpipelinedataparser";

        /// <summary>
        /// Creates a new instance of the <see cref="SendPortPipelineDataParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public SendPortPipelineDataParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(ParserName, model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Formats the error message.
        /// </summary>
        /// <param name="direction">The direction of the pipeline.</param>
        /// <param name="resourceIdentifier">The identified of the resource, i.e. port name, receive location name etc.</param>
        /// <param name="applicationName">The name of the application.</param>
        /// <param name="message">The exception message.</param>
        /// <returns>The formated error message</returns>
        protected override string FormatErrorMessage(PipelineDirection direction, string resourceIdentifier, string applicationName, string message)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorParsingSendPortPipelineData, resourceIdentifier, direction, applicationName, message);
        }

        /// <summary>
        /// Parses the send pipeline data stored as XML in the bindings.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The migration context.</param>        
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(SendPortPipelineDataParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(SendPortPipelineDataParser));

                foreach (var application in group.Applications)
                {
                    // Defensive check
                    if (application.Application.Bindings == null)
                    {
                        _logger.LogWarning(WarningMessages.BindingInfoNotFound, application.Application.Name);
                        continue;
                    }

                    _logger.LogDebug(TraceMessages.ParsingBizTalkSendPipelineDataInApplication, application.Application.Name);
                    if (application.Application.Bindings.BindingInfo.SendPortCollection != null)
                    {
                        foreach (var sendPort in application.Application.Bindings.BindingInfo.SendPortCollection)
                        {
                            _logger.LogDebug(TraceMessages.ParsingBizTalkSendPortPipelineDataForSendPort, sendPort.Name);

                            sendPort.SendPipelineCustomConfiguration = ParsePipelineData(application, sendPort.Name, sendPort.SendPipelineData, PipelineDirection.Send, context.Errors);

                            sendPort.ReceivePipelineCustomConfiguration = ParsePipelineData(application, sendPort.Name, sendPort.ReceivePipelineData, PipelineDirection.Receive, context.Errors);
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(SendPortPipelineDataParser));
            }
        }
    }
}
