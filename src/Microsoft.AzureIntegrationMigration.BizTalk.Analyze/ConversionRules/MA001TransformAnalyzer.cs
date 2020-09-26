using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Messages;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules
{
    /// <summary>
    /// Implements rule MA001, which analyzes the model and creates the a reference to maps of each message in application on the target source.
    /// </summary>
    public sealed class MA001TransformAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "MA001";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="MA001TransformAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public MA001TransformAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(MA001TransformAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the schema for applications in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get parsed BizTalk model from the application model
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (parsedApplicationGroup?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(MA001TransformAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(MA001TransformAnalyzer));

                foreach (var targetApplication in Model.MigrationTarget.MessageBus.Applications)
                {
                    foreach (var message in targetApplication.Messages)
                    {
                        if (message.MessageSchema != null)
                        {
                            var messageResource = Model.FindResourceByKey(message.MessageSchema.ResourceKeyRef);
                            if (messageResource != null)
                            {
                                var schemaResource = Model.FindResourceByRefId(messageResource.ParentRefId);
                                if (schemaResource != null)
                                {
                                    var mapResources = schemaResource.FindRelatedResourcesByType(Model, ResourceRelationshipType.ReferencedBy, ModelConstants.ResourceMap);

                                    foreach (var mapResource in mapResources)
                                    {
                                        var transform = new MessageTransform
                                        {
                                            Name = mapResource.Name,
                                            ResourceKeyRef = mapResource.Key
                                        };

                                        var transformSourceObject = (Transform)mapResource.SourceObject;
                                        transform.Properties.Add(ModelConstants.TypeName, transformSourceObject.FullName);

                                        // Source schemas.
                                        var sourceSchemaResources = mapResource.FindRelatedResourcesByType(Model, ResourceRelationshipType.ReferencedBy, ModelConstants.ResourceDocumentSchema);
                                        var sourceMessageTypes = new List<string>();
                                        foreach (var sourceSchema in sourceSchemaResources)
                                        {
                                            transform.SourceSchemaKeyRefs.Add(sourceSchema.Key);
                                            var schema = sourceSchema.SourceObject as Schema;
                                            sourceMessageTypes.AddRange(schema.MessageDefinitions.Select(m => m.MessageType));
                                        }
                                        transform.Properties.Add(ModelConstants.SourceSchemaMessageTypes, sourceMessageTypes);

                                        // Target schemas.
                                        var targetSchemaResources = mapResource.FindRelatedResourcesByType(Model, ResourceRelationshipType.ReferencesTo, ModelConstants.ResourceDocumentSchema);
                                        var targetMessageTypes = new List<string>();
                                        foreach (var targetSchema in targetSchemaResources)
                                        {
                                            transform.TargetSchemaKeyRefs.Add(targetSchema.Key);
                                            var schema = targetSchema.SourceObject as Schema;
                                            targetMessageTypes.AddRange(schema.MessageDefinitions.Select(m => m.MessageType));
                                        }
                                        transform.Properties.Add(ModelConstants.TargetSchemaMessageTypes, targetMessageTypes);

                                        // Add the transform to the list.
                                        message.MessageTransforms.Add(transform);
                                    }
                                }
                                else
                                {
                                    _logger.LogError(ErrorMessages.UnableToFindResourceReference, nameof(MA001TransformAnalyzer), messageResource.ParentRefId);
                                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceReference, nameof(MA001TransformAnalyzer), messageResource.ParentRefId)));
                                }
                            }
                            else
                            {
                                _logger.LogError(ErrorMessages.UnableToFindResourceReference, nameof(MA001TransformAnalyzer), message.MessageSchema.ResourceKeyRef);
                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceReference, nameof(MA001TransformAnalyzer), message.MessageSchema.ResourceKeyRef)));
                            }
                        }
                        else
                        {
                            _logger.LogError(ErrorMessages.TransformMessageSchemaMissing, RuleName, message.Name, nameof(MA001TransformAnalyzer));
                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.TransformMessageSchemaMissing, RuleName, message.Name, nameof(MA001TransformAnalyzer))));
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(MA001TransformAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
