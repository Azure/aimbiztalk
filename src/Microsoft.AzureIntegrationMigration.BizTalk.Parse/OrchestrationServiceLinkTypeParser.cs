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
    /// Parses the service link types for orchestrations.
    /// </summary>
    public sealed class OrchestrationServiceLinkTypeParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="OrchestrationServiceLinkTypeParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public OrchestrationServiceLinkTypeParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(OrchestrationServiceLinkTypeParser), model, context, logger)
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
            if (group?.Applications != null)
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(OrchestrationServiceLinkTypeParser));

                foreach (var application in group.Applications)
                {
                    // Loop through all of the orchestrations.
                    foreach (var orchestration in application.Application.Orchestrations)
                    {
                        // Find the module for the orchestration.
                        var moduleResource = orchestration.Model?.Resource?.FindResourcesByType(ModelConstants.ResourceModule).SingleOrDefault();
                        if (moduleResource != null)
                        {
                            foreach (var serviceLinkType in orchestration.FindServiceLinkTypes())
                            {
                                var resourceName = serviceLinkType.FindPropertyValue(MetaModelConstants.PropertyKeyName);
                                var resourceKey = string.Concat(moduleResource.Key, ":", resourceName);

                                var serviceLinkTypeResource = new ResourceItem
                                {
                                    Name = resourceName,
                                    Key = resourceKey,
                                    Type = ModelConstants.ResourceServiceLinkType,
                                    ParentRefId = moduleResource.RefId,
                                    Rating = ConversionRating.NotSupported
                                };

                                serviceLinkType.Resource = serviceLinkTypeResource; // Maintain pointer to resource.
                                serviceLinkTypeResource.SourceObject = serviceLinkType; // Maintain backward pointer.
                                moduleResource.Resources.Add(serviceLinkTypeResource);

                                _logger.LogTrace(TraceMessages.ResourceCreated, nameof(OrchestrationServiceLinkTypeParser), serviceLinkTypeResource.Key, serviceLinkTypeResource.Name, serviceLinkTypeResource.Type, moduleResource.Key);
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

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(OrchestrationServiceLinkTypeParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(OrchestrationServiceLinkTypeParser));
            }
        }
    }
}
