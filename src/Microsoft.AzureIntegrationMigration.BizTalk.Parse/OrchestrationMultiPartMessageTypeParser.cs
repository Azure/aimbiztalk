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
    /// Parses the multi-part message types for orchestrations.
    /// </summary>
    public sealed class OrchestrationMultiPartMessageTypeParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="OrchestrationMultiPartMessageTypeParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public OrchestrationMultiPartMessageTypeParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(OrchestrationMultiPartMessageTypeParser), model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Implements the internal parsing logic.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The migration context.</param>
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(OrchestrationMultiPartMessageTypeParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(OrchestrationMultiPartMessageTypeParser));

                foreach (var application in group.Applications)
                {
                    // Loop through all of the orchestrations.
                    foreach (var orchestration in application.Application.Orchestrations)
                    {
                        // Find the module for the orchestration.
                        var moduleResource = orchestration.Model?.Resource?.FindResourcesByType(ModelConstants.ResourceModule).SingleOrDefault();
                        if (moduleResource != null)
                        {
                            foreach (var multiPartMessageType in orchestration.FindMultiPartMessageTypes())
                            {
                                var resourceName = multiPartMessageType.FindPropertyValue(MetaModelConstants.PropertyKeyName);
                                var resourceKey = string.Concat(moduleResource.Key, ":", resourceName);

                                var multiPartMessageTypeResource = new ResourceItem
                                {
                                    Name = resourceName,
                                    Key = resourceKey,
                                    Type = ModelConstants.ResourceMultipartMessageType,
                                    ParentRefId = moduleResource.RefId,
                                    Rating = ConversionRating.NotSupported
                                };

                                multiPartMessageType.Resource = multiPartMessageTypeResource; // Maintain pointer to resource.
                                multiPartMessageTypeResource.SourceObject = multiPartMessageType; // Maintain backward pointer.
                                moduleResource.Resources.Add(multiPartMessageTypeResource);

                                _logger.LogTrace(TraceMessages.ResourceCreated, nameof(OrchestrationMultiPartMessageTypeParser), multiPartMessageTypeResource.Key, multiPartMessageTypeResource.Name, multiPartMessageTypeResource.Type, moduleResource.Key);
                            }
                        }
                        else
                        {
                            var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResource, ModelConstants.ResourceModule, orchestration.ResourceDefinitionKey);
                            _logger.LogError(error);
                            context.Errors.Add(new ErrorMessage(error));
                        }
                    }
                }
                _logger.LogDebug(TraceMessages.CompletedParser, nameof(OrchestrationMultiPartMessageTypeParser));
            }
        }
    }
}
