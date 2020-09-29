// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="ReceivePortPipelineDataParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a class used to parse the custom pipeline data configuation found in the bindings, for receive ports.</summary>
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
    /// Represents a class used to parse the custom pipeline data configuation found in the bindings, for receive ports.
    /// </summary>
    public sealed class ReceivePortPipelineDataParser : PipelineDataParser
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of this parser.
        /// </summary>
        public const string ParserName = ModelConstants.ApplicationPrefix + "receiveportpipelinedataparser";

        /// <summary>
        /// Creates a new instance of the <see cref="ReceivePortPipelineDataParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public ReceivePortPipelineDataParser(IApplicationModel model, MigrationContext context, ILogger logger)
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
            return string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorParsingReceivePortPipelineData, resourceIdentifier, direction, applicationName, message);
        }

        /// <summary>
        /// Parses the receive pipeline data stored as XML in the bindings.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The migration context.</param>        
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(ReceivePortPipelineDataParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(ReceivePortPipelineDataParser));

                foreach (var application in group.Applications)
                {
                    // Defensive check
                    if (application.Application.Bindings == null)
                    {
                        _logger.LogWarning(WarningMessages.BindingInfoNotFound, application.Application.Name);
                        continue;
                    }

                    if (application.Application.Bindings.BindingInfo.ReceivePortCollection != null)
                    {
                        foreach (var receivePort in application.Application.Bindings.BindingInfo.ReceivePortCollection)
                        {
                            _logger.LogDebug(TraceMessages.ParsingBizTalkSendPipelineCustomConfigurationForReceivePort, receivePort.Name);

                            receivePort.SendPipelineCustomConfiguration = ParsePipelineData(application, receivePort.Name, receivePort.SendPipelineData, PipelineDirection.Send, context.Errors);
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(ReceivePortPipelineDataParser));
            }
        }
    }
}
