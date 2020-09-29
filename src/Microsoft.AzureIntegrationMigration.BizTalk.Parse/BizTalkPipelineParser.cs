// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="BizTalkPipelineParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Parses the pipeline which is stored in the raw XML.</summary>
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
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Parses the pipeline which is stored in the raw XML.
    /// </summary>
    public sealed class BizTalkPipelineParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of this parser.
        /// </summary>
        public const string ParserName = ModelConstants.ApplicationPrefix + "biztalkpipelineparser";

        /// <summary>
        /// Creates a new instance of the <see cref="BizTalkPipelineParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public BizTalkPipelineParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(ParserName, model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Parses the raw XML Pipeline definition, into a typed document representing the Pipeline.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The migration context.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "By design as the exception messages are collated in an error object.")]
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(BizTalkPipelineParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(BizTalkPipelineParser));

                foreach (var application in group.Applications)
                {
                    _logger.LogDebug(TraceMessages.ParsingBizTalkPipelinesInApplication, application.Application.Name);

                    var applicationResource = model.FindResourceByKey(application.Application?.ApplicationDefinition?.ResourceKey);

                    // Loop through all of the pipelines.
                    foreach (var pipeline in application.Application.Pipelines)
                    {
                        try
                        {
                            var pipelineResourceDefinition = model.FindResourceDefinitionByKey(pipeline.ResourceDefinitionKey, pipeline.Direction == PipelineDirection.Receive ? ModelConstants.ResourceDefinitionReceivePipeline : ModelConstants.ResourceDefinitionSendPipeline);

                            pipeline.Document = Document.FromXml((string)pipelineResourceDefinition.ResourceContent);

                            pipeline.ResourceKey = string.Concat(pipelineResourceDefinition.Key, ":", "pipelineresource");

                            // Create the resource.
                            var pipelineResource = new ResourceItem
                            {
                                Name = pipeline.Name,
                                Key = pipeline.ResourceKey,
                                Type = pipeline.Direction == PipelineDirection.Send ? ModelConstants.ResourceSendPipeline : ModelConstants.ResourceReceivePipeline,
                                Description = pipeline.Description,
                                ParentRefId = pipelineResourceDefinition.RefId,
                                Rating = ConversionRating.NotSupported
                            };

                            pipeline.Resource = pipelineResource;  // Maintain reference to the resource.
                            pipelineResource.SourceObject = pipeline; // Maintain backward pointer.

                            pipelineResourceDefinition.Resources.Add(pipelineResource);

                            if (applicationResource != null)
                            {
                                applicationResource.AddRelationship(new ResourceRelationship(pipelineResource.RefId, ResourceRelationshipType.Child));
                                pipelineResource.AddRelationship(new ResourceRelationship(applicationResource.RefId, ResourceRelationshipType.Parent));
                            }
                            else
                            {
                                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceApplication, application.Application?.ApplicationDefinition?.ResourceKey);
                                _logger.LogError(error);
                                context.Errors.Add(new ErrorMessage(error));
                            }

                            _logger.LogDebug(TraceMessages.ParsedBizTalkPipeline, pipeline.FullName);
                        }
                        catch (Exception ex)
                        {
                            var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorParsingPipelineDocument, pipeline.Name, application.Application.Name, ex.Message);
                            context.Errors.Add(new ErrorMessage(message));
                            _logger.LogError(message);
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(BizTalkPipelineParser));
            }
        }
    }
}
