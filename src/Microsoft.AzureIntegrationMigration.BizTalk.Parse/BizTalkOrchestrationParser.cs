// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="BizTalkOrchestrationParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Parses the orchestration which is stored in the Odx definition.</summary>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Linq;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Parses the orchestration which is stored in the Odx definition.
    /// </summary>
    public sealed class BizTalkOrchestrationParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of this parser.
        /// </summary>
        public const string ParserName = ModelConstants.ApplicationPrefix + "biztalkorchestrationparser";

        /// <summary>
        /// Creates a new instance of the <see cref="BizTalkOrchestrationParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public BizTalkOrchestrationParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(ParserName, model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));            
        }

        /// <summary>
        /// Parses the raw ODX information from the Binding File, into a typed model representing the Orchestration.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The migration context.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "By design as the exception messages are collated in an error object.")]
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(BizTalkOrchestrationParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(BizTalkOrchestrationParser));

                foreach (var application in group.Applications)
                {
                    _logger.LogDebug(TraceMessages.ParsingBizTalkOrchestrationsInApplication, application.Application.Name);

                    // Loop through all of the orchestrations.
                    foreach (var orchestration in application.Application.Orchestrations)
                    {
                        var applicationResource = model.FindResourceByKey(application.Application?.ApplicationDefinition?.ResourceKey);

                        try
                        {
                            var orchestrationResourceDefinition = model.FindResourceDefinitionByKey(orchestration.ResourceDefinitionKey, ModelConstants.ResourceDefinitionOrchestration);
                            if (orchestrationResourceDefinition != null)
                            {
                                // Load metamodel
                                orchestration.Model = MetaModel.FromXml((string)orchestrationResourceDefinition.ResourceContent);

                                _logger.LogDebug(TraceMessages.ParsedBizTalkOrchestration, orchestration.FullName);

                                // Create resource for metamodel
                                var metaModelResource = new ResourceItem()
                                {
                                    Name = orchestration.Name,
                                    Key = string.Concat(orchestrationResourceDefinition.Key, ":", MetaModelConstants.MetaModelRootElement),
                                    Type = ModelConstants.ResourceMetaModel,
                                    ParentRefId = orchestrationResourceDefinition.RefId,
                                    Rating = ConversionRating.NotSupported
                                };

                                orchestration.Model.Resource = metaModelResource; // Maintain pointer to resource.
                                metaModelResource.SourceObject = orchestration.Model; // Maintain backward pointer.
                                orchestrationResourceDefinition.Resources.Add(metaModelResource);

                                if (applicationResource != null)
                                {
                                    applicationResource.AddRelationship(new ResourceRelationship(metaModelResource.RefId, ResourceRelationshipType.Child));
                                    metaModelResource.AddRelationship(new ResourceRelationship(applicationResource.RefId, ResourceRelationshipType.Parent));
                                }
                                else
                                {
                                    var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceApplication, application.Application?.ApplicationDefinition?.ResourceKey);
                                    _logger.LogError(error);
                                    context.Errors.Add(new ErrorMessage(error));
                                }

                                _logger.LogTrace(TraceMessages.ResourceCreated, nameof(BizTalkOrchestrationParser), metaModelResource.Key, metaModelResource.Name, metaModelResource.Type, orchestrationResourceDefinition.Key);

                                var module = orchestration.FindModule();
                                if (module != null)
                                {
                                    var resourceName = module.FindPropertyValue(MetaModelConstants.PropertyKeyName);
                                    var resourceKey = string.Concat(metaModelResource.Key, ":", resourceName);

                                    // Create resource for module
                                    var moduleResource = new ResourceItem()
                                    {
                                        Name = resourceName,
                                        Key = resourceKey,
                                        Type = ModelConstants.ResourceModule,
                                        ParentRefId = metaModelResource.RefId,
                                        Rating = ConversionRating.NotSupported
                                    };

                                    module.Resource = moduleResource; // Maintain pointer to resource.
                                    moduleResource.SourceObject = module; // Maintain backward pointer.
                                    metaModelResource.Resources.Add(moduleResource);

                                    _logger.LogTrace(TraceMessages.ResourceCreated, nameof(BizTalkOrchestrationParser), moduleResource.Key, moduleResource.Name, moduleResource.Type, metaModelResource.Key);
                                }
                                else
                                {
                                    var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindModuleInOrchestrationModel, orchestration.ResourceContainerKey, orchestration.FullName);
                                    _logger.LogError(error);
                                    context.Errors.Add(new ErrorMessage(error));
                                }
                            }
                            else
                            {
                                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceDefinitionOrchestration, orchestration.ResourceDefinitionKey);
                                _logger.LogError(error);
                                context.Errors.Add(new ErrorMessage(error));
                            }
                        }
                        catch (Exception ex)
                        {
                            var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorParsingMetaModel, orchestration.Name, application.Application.Name, ex.Message);
                            context.Errors.Add(new ErrorMessage(message));
                            _logger.LogError(message);
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(BizTalkOrchestrationParser));
            }
        }
    }
}
