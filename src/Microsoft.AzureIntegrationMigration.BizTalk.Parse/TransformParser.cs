using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Filters;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    ///  Defines a class to parse Transforms.
    /// </summary>
    public sealed class TransformParser : BizTalkParserBase
    {/// <summary>
     /// Defines a logger.
     /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="SendPortFilterParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public TransformParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(TransformParser), model, context, logger)
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
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(TransformParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(TransformParser));

                // Find all transforms in source model
                foreach (var application in group.Applications)
                {
                    var applicationResource = model.FindResourceByKey(application.Application?.ApplicationDefinition?.ResourceKey);

                    var transforms = application.Application.Transforms;
                    if (transforms != null && transforms.Any())
                    {
                        foreach (var transform in transforms)
                        {
                            // Find transform resource definition
                            var transformResourceDefinition = model.FindResourceDefinitionByKey(transform.ResourceDefinitionKey, ModelConstants.ResourceDefinitionMap);
                            if (transformResourceDefinition != null)
                            {
                                // Add transform resource to source
                                var transformResource = new ResourceItem()
                                {
                                    Name = transform.Name,
                                    Description = transform.FullName,
                                    Key = transformResourceDefinition.Key + ":map",
                                    Type = ModelConstants.ResourceMap,
                                    ParentRefId = transformResourceDefinition.RefId,
                                    Rating = ConversionRating.NotSupported
                                };

                                transform.Resource = transformResource; // Maintain pointer to the resource.
                                transformResource.SourceObject = transform; // Maintain backward pointer.
                                transformResourceDefinition.Resources.Add(transformResource);

                                if (applicationResource != null)
                                {
                                    applicationResource.AddRelationship(new ResourceRelationship(transformResource.RefId, ResourceRelationshipType.Child));
                                    transformResource.AddRelationship(new ResourceRelationship(applicationResource.RefId, ResourceRelationshipType.Parent));
                                }
                                else
                                {
                                    var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceApplication, application.Application?.ApplicationDefinition?.ResourceKey);
                                    _logger.LogError(error);
                                    context.Errors.Add(new ErrorMessage(error));
                                }

                                _logger.LogTrace(TraceMessages.ResourceCreated, nameof(TransformParser), transformResource.Key, transformResource.Name, transformResource.Type, transformResourceDefinition.Key);
                            }
                            else
                            {
                                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceDefinitionMap, transform.ResourceDefinitionKey);
                                _logger.LogError(error);
                                context.Errors.Add(new ErrorMessage(error));
                            }
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(TransformParser));
            }
        }
    }
}
