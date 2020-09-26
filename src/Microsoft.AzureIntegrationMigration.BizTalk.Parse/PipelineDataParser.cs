//-----------------------------------------------------------------------
// <copyright file="PipelineDataParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents an abstract class used to parse the custom configuation found in the bindings.</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Represents an abstract class used to parse the custom pipeline data configuation found in the bindings.
    /// </summary>
    public abstract class PipelineDataParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="PipelineDataParser" class./>
        /// </summary>
        /// <param name="parserName">The name of the BizTalk parser.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public PipelineDataParser(string parserName, IApplicationModel model, MigrationContext context, ILogger logger)
            : base(parserName, model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Parses the pipeline data.
        /// </summary>
        /// <param name="application">The BizTalk application</param>
        /// <param name="sendPortName">The name of the send port.</param>
        /// <param name="pipelineData">The data to parse.</param>
        /// <param name="direction">The direction of the pipeline.</param>
        /// <param name="errors">An error object used to record if an error occured in the parsing.</param>
        /// <returns>The formatted error message</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "By design as the exception messages are collated in an error object.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "This is validated in the public method")]
        protected Root ParsePipelineData(ParsedBizTalkApplication application, string sendPortName, string pipelineData, PipelineDirection direction, List<IErrorMessage> errors)
        {
            Root pipelineConfiguration = null;

            try
            {
                // Only parse if there is data.
                if (!string.IsNullOrEmpty(pipelineData))
                {
                    _logger.LogDebug(TraceMessages.ParsingBizTalkSendPortPipelineData, sendPortName);

                    pipelineConfiguration = Root.FromXml(pipelineData);

                    _logger.LogDebug(TraceMessages.ParsedBizTalkSendPortPipelineData, sendPortName);
                }
            }
            catch (Exception ex)
            {
                var message = FormatErrorMessage(direction, sendPortName, application.Application.Name, ex.Message);
                errors.Add(new ErrorMessage(message));
            }

            return pipelineConfiguration;
        }

        /// <summary>
        /// Formats the error message.
        /// </summary>
        /// <param name="direction">The direction of the pipeline.</param>
        /// <param name="resourceIdentifier">The identified of the resource, i.e. port name, receive location name etc.</param>
        /// <param name="applicationName">The name of the application.</param>
        /// <param name="message">The exception message.</param>
        /// <returns>The formated error message</returns>
        protected abstract string FormatErrorMessage(PipelineDirection direction, string resourceIdentifier, string applicationName, string message);
    }
}
