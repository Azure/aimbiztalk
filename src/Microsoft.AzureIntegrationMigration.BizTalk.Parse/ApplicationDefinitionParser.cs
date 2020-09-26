//-----------------------------------------------------------------------
// <copyright file="ApplicationDefinitionParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Parses the application definition file.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.ApplicationDefinitions;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Parses the application definition file.
    /// </summary>
    public sealed class ApplicationDefinitionParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of this parser.
        /// </summary>
        public const string ParserName = ModelConstants.ApplicationPrefix + "applicationdefinitionparser";

        /// <summary>
        /// Creates a new instance of the <see cref="ApplicationDefinitionParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public ApplicationDefinitionParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(ParserName, model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Discovers the application definition file contents from the application definition XML.
        /// </summary>
        /// <param name="context">The migration context.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "By design as the exception messages are collated in an error object.")]
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(ApplicationDefinitionParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(ApplicationDefinitionParser));

                foreach (var application in group.Applications)
                {
                    try
                    {
                        // Defensive check
                        if (application.Application.ApplicationDefinition == null)
                        {
                            _logger.LogWarning(WarningMessages.ApplicationDefinitionNotFound, application.Application.Name);
                            continue;
                        }

                        _logger.LogDebug(TraceMessages.ParsingBizTalkApplicationDefinitionFileFromResourceContainer, application.Application.ApplicationDefinition.ResourceContainerKey);

                        var adf = from resourceContainer in model.MigrationSource.ResourceContainers
                                    from resourceDefinition in resourceContainer.ResourceDefinitions
                                    where resourceContainer.Key == application.ResourceContainerKey &&
                                    application.Application.ApplicationDefinition.ResourceContainerKey == resourceContainer.Key &&
                                    application.Application.ApplicationDefinition.ResourceDefinitionKey == resourceDefinition.Key &&
                                    resourceDefinition.Type == ModelConstants.ResourceDefinitionApplicationDefinition
                                    select resourceDefinition;

                        var adfResourceDefinition = adf.Single();
                        var applicationDefinition = ApplicationDefinition.FromXml((string)adfResourceDefinition.ResourceContent);
                        application.Application.ApplicationDefinition.ApplicationDefinition = applicationDefinition;
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorReadingApplicationDefinition, application.Application.Name, ex.Message);
                        context.Errors.Add(new ErrorMessage(message));
                        _logger.LogError(message);
                    }
                }
                
                _logger.LogDebug(TraceMessages.CompletedParser, nameof(ApplicationDefinitionParser));
            }
        }
    }
}
