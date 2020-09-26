using System;
using System.Globalization;
using System.Linq;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Filters;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Defines a class to parse pipeline components.
    /// </summary>
    public sealed class PipelineComponentParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="PipelineComponentParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public PipelineComponentParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(PipelineComponentParser), model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Implements the internal parsing logic.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The <see cref="MigrationContext"/> being parsed.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "By design as the exception messages are collated in an error object.")]
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(PipelineComponentParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(PipelineComponentParser));

                foreach (var pipeline in group.Applications.SelectMany(a => a.Application.Pipelines).Where(p => p.Components != null))
                {
                    var pipelineType = pipeline.Direction == Types.Enumerations.PipelineDirection.Send ? ModelConstants.ResourceDefinitionSendPipeline : ModelConstants.ResourceReceivePipeline;

                    // Find the resource for the pipeline.                    
                    var pipelineResource = model.FindResourceByKey(pipeline.ResourceKey);

                    if (pipelineResource != null)
                    {
                        // Loop through all of the components.
                        var stageComponents = Document.FindStageComponents(pipeline.Document);
                        foreach (var stageComponent in stageComponents)
                        {
                            var resourceName = stageComponent.ComponentName;
                            var resourceKey = string.Concat(pipelineResource.Key, ":", resourceName);

                            // Create the component resource.
                            var pipelineComponentResource = new ResourceItem
                            {
                                Name = resourceName,
                                Key = resourceKey,
                                Type = ModelConstants.ResourcePipelineComponent,
                                Description = stageComponent.Description,
                                ParentRefId = pipelineResource.RefId,
                                Rating = ConversionRating.NotSupported
                            };

                            stageComponent.Resource = pipelineComponentResource; // Maintain pointer to the resource.
                            pipelineComponentResource.SourceObject = stageComponent; // Maintain backward pointer.
                            stageComponent.ResourceKey = resourceKey;
                            pipelineResource.Resources.Add(pipelineComponentResource);

                            _logger.LogDebug(TraceMessages.ResourceCreated, nameof(PipelineComponentParser), pipelineComponentResource.Key, pipelineComponentResource.Name, pipelineComponentResource.Type, pipelineResource.Key);
                        }                        
                    }
                    else 
                    {
                        var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResource, pipelineType, pipeline.ResourceKey);
                        _logger.LogError(error);
                        context.Errors.Add(new ErrorMessage(error));
                    }
                }
                _logger.LogDebug(TraceMessages.CompletedParser, nameof(PipelineComponentParser));
            }
        }
    }
}
