// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="BizTalkApplicationParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Parses application information out of the bindings file.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
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
    /// Parses application information out of the bindings file.
    /// </summary>
    public sealed class BizTalkApplicationParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a property ket of the application display name.
        /// </summary>
        private const string ApplicationDisplayNameProperty = "DisplayName";

        /// <summary>
        /// Defines a property key of the application description.
        /// </summary>
        public const string ApplicationDescriptionProperty = "ApplicationDescription";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of this parser.
        /// </summary>
        public const string ParserName = ModelConstants.ApplicationPrefix + "bizTalkapplicationparser";


        /// <summary>
        /// Creates a new instance of the <see cref="BizTalkApplicationParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public BizTalkApplicationParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(ParserName, model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Parses the name of the BizTalk application.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The migration context.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "By design as the exception messages are collated in an error object.")]
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(BizTalkApplicationParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(BizTalkApplicationParser));

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

                        _logger.LogDebug(TraceMessages.ParsingBizTalkApplicationFromResourceContainer, application.Application.ApplicationDefinition.ResourceContainerKey);

                        var adf = from resourceContainer in model.MigrationSource.ResourceContainers
                                    from resourceDefinition in resourceContainer.ResourceDefinitions
                                    where resourceContainer.Key == application.ResourceContainerKey &&
                                    application.Application.ApplicationDefinition.ResourceContainerKey == resourceContainer.Key &&
                                    application.Application.ApplicationDefinition.ResourceDefinitionKey == resourceDefinition.Key &&
                                    resourceDefinition.Type == ModelConstants.ResourceDefinitionApplicationDefinition
                                    select resourceDefinition;

                        var applicationResourceDefinition = adf.SingleOrDefault();
                        if (applicationResourceDefinition == null)
                        {
                            _logger.LogWarning(WarningMessages.ApplicationDefinitionNotFound, application.Application.Name);
                            continue;
                        }

                        var applicationDefinition = application.Application.ApplicationDefinition.ApplicationDefinition ?? ApplicationDefinition.FromXml((string)applicationResourceDefinition.ResourceContent); // Only parse if not already deserialized.
                        var applicationName = applicationDefinition.Properties.Where(p => p.Name == BizTalkApplicationParser.ApplicationDisplayNameProperty).SingleOrDefault();

                        if (applicationName != null && !string.IsNullOrWhiteSpace(applicationName.Value))
                        {
                            // Check to see if there is already an application in the source with this name (duplicate names can occur is passing multiple unrelated
                            // MSI files).
                            var duplicateApplication = model.FindResourcesByType(ModelConstants.ResourceApplication).Any(a => a.Name == applicationName.Value);

                            // Set application name
                            application.Application.Name = applicationName.Value;
                            if (duplicateApplication)
                            {
                                application.Application.Name = $"{application.Application.Name} {ResourceItemProperties.Duplicate}";
                            }

                            // Define resource key for application.
                            var resourceKey = string.Concat(applicationResourceDefinition.Key, ":", applicationName);
                            application.Application.ApplicationDefinition.ResourceKey = resourceKey;

                            // Create the application resource under the application definition resource.
                            var applicationResource = new ResourceItem
                            {
                                Name = applicationName.Value,
                                Description = applicationDefinition.Properties.Where(p => p.Name == BizTalkApplicationParser.ApplicationDescriptionProperty).SingleOrDefault()?.Value,
                                Key = resourceKey,
                                Type = ModelConstants.ResourceApplication,
                                ParentRefId = applicationResourceDefinition.RefId,
                                Rating = ConversionRating.NotSupported
                            };

                            application.Application.ApplicationDefinition.Resource = applicationResource; // Maintain pointer to the resource.
                            applicationResource.SourceObject = applicationDefinition; // Maintain backward pointer.
                            applicationResourceDefinition.Resources.Add(applicationResource);

                            _logger.LogTrace(TraceMessages.ResourceCreated, nameof(BizTalkApplicationParser), applicationResource.Key, applicationResource.Name, applicationResource.Type, applicationResource.Key);

                            // If this does not exist then update on the source model.
                            if (application.Application.ApplicationDefinition.ApplicationDefinition == null)
                            {
                                application.Application.ApplicationDefinition.ApplicationDefinition = applicationDefinition;
                            }

                            _logger.LogDebug(TraceMessages.ParsedTheBizTalkApplicationWithName, application.Application.Name);

                            // Raise an error if this was a duplicate application
                            if (duplicateApplication)
                            {
                                // Raise an error that there is already an application with this name
                                var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.DuplicateApplicationFound, applicationName.Value);
                                context.Errors.Add(new ErrorMessage(message));
                                _logger.LogError(message);
                            }
                        }
                        else
                        {
                            var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ApplicationNameNotFound, applicationResourceDefinition.Key);
                            context.Errors.Add(new ErrorMessage(message));
                            _logger.LogError(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorReadingApplicationFromAdf, application.Application.ApplicationDefinition.ResourceDefinitionKey, ex.Message);
                        context.Errors.Add(new ErrorMessage(message));
                        _logger.LogError(message);
                    }
                }

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(BizTalkApplicationParser));
            }
        }
    }
}
