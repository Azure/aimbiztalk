using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.DependencyRules
{
    /// <summary>
    /// Defines an Analyzer to resolve dependency to an orchestration in the source model.  If missing
    /// dependencies are found, it warns in the report.
    /// </summary>
    public sealed class DP003OrchestrationDependencyAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "DP003";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="DP003OrchestrationDependencyAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public DP003OrchestrationDependencyAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(DP003OrchestrationDependencyAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Performs the internal work of analyzing the model.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get the parsed BizTalk model from the application model.
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (Model == null || Model.MigrationSource == null || Model.MigrationSource.MigrationSourceModel == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(DP003OrchestrationDependencyAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(DP003OrchestrationDependencyAnalyzer));

                var resources = Model.FindAllResources();  // Gets all the resources in the model as a list

                ResolveMessageSchemaDependencies(resources);
                ResolveTransformDependencies(resources);

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(DP003OrchestrationDependencyAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Resolves the dependencies from messages to schemas.
        /// </summary>
        /// <param name="resources">The list of resources being analyzed.</param>
        private void ResolveMessageSchemaDependencies(IList<ResourceItem> resources)
        {
            var messageDeclarations = resources.Where(r => r.Type == ModelConstants.ResourceMessageDeclaration);
            var messageTypes = resources.Where(r => r.Type == ModelConstants.ResourceMessageType && r.SourceObject != null);

            // Iterate through the message declarations and see if there is an associated schema.
            foreach (var messageDeclaration in messageDeclarations)
            {
                // Check for bad references.
                if (messageDeclaration.SourceObject == null)
                {
                    _logger.LogError(ErrorMessages.SourceObjectNotFound, messageDeclaration.Name, messageDeclaration.Type);
                    var errorMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.SourceObjectNotFound, messageDeclaration.Name, messageDeclaration.Type);
                    messageDeclaration.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Error, Message = errorMessage });
                    Context.Errors.Add(new ErrorMessage(errorMessage));
                    continue;
                }

                // Check for an associated message type matching the message declaration.
                var sourceObject = (Element)messageDeclaration.SourceObject;
                var schemaType = sourceObject.FindPropertyValue(MetaModelConstants.PropertyKeyType);

                if (schemaType.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
                {
                    // Log as we can't lookup System types in the application.
                    _logger.LogDebug(TraceMessages.SchemaDependencyNotResolved, RuleName, schemaType);
                    messageDeclaration.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Information, Message = string.Format(CultureInfo.CurrentCulture, InformationMessages.SystemSchemaDependencyFound, messageDeclaration.Key, schemaType) });
                }
                else
                {
                    var matchingMessageTypes = messageTypes.Where(m => ((MessageDefinition)m.SourceObject).FullName == schemaType);
                    if (matchingMessageTypes == null || !matchingMessageTypes.Any())
                    {
                        // Log as we can't find a matching schema.
                        _logger.LogDebug(TraceMessages.SchemaDependencyNotResolved, RuleName, schemaType);
                        messageDeclaration.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.SchemaReferencedByMessageDeclarationIsMissing, schemaType, messageDeclaration.Key) });
                    }
                    else if (matchingMessageTypes.Count() > 1)
                    {
                        messageDeclaration.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.SchemaReferenceMultipleMatches, schemaType, messageDeclaration.Key, matchingMessageTypes.Count()) });
                    }
                    else
                    {
                        // We have a matching reference
                        var messageType = matchingMessageTypes.Single();
                        messageType.AddRelationship(new ResourceRelationship(messageDeclaration.RefId, ResourceRelationshipType.ReferencedBy));
                        messageDeclaration.AddRelationship(new ResourceRelationship(messageType.RefId, ResourceRelationshipType.ReferencesTo)); // Reference to the message declaration
                        messageDeclaration.AddRelationship(new ResourceRelationship(messageType.ParentRefId, ResourceRelationshipType.ReferencesTo)); // Reference to the schema

                        // Add a link from the schema to the message declaration.
                        var schema = resources.Where(r => r.RefId == messageType.ParentRefId).SingleOrDefault();
                        if (schema != null)
                        {
                            schema.AddRelationship(new ResourceRelationship(messageDeclaration.RefId, ResourceRelationshipType.ReferencedBy));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the dependencies from transforms to schemas.
        /// </summary>
        /// <param name="resources">The list of resources being analyzed.</param>
        private void ResolveTransformDependencies(IList<ResourceItem> resources)
        {
            var serviceDeclarations = resources.Where(s => s.Type == ModelConstants.ResourceServiceDeclaration);
            var transforms = resources.Where(t => t.Type == ModelConstants.ResourceMap && t.SourceObject != null);

            // Iterate through the service declarations (orchestrations) and look for transforms used within the service declaration.
            foreach (var serviceDeclaration in serviceDeclarations)
            {
                // Check for bad references.
                if (serviceDeclaration.SourceObject == null)
                {
                    _logger.LogError(ErrorMessages.SourceObjectNotFound, serviceDeclaration.Name, serviceDeclaration.Type);
                    var errorMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.SourceObjectNotFound, serviceDeclaration.Name, serviceDeclaration.Type);
                    Context.Errors.Add(new ErrorMessage(errorMessage));
                    serviceDeclaration.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Error, Message = errorMessage });
                    continue;
                }

                // Find all the transform shapes within the orchestration.
                var sourceObject = (Element)serviceDeclaration.SourceObject;
                var transformShapes = sourceObject.FindTransforms();

                if (transformShapes != null && transformShapes.Any())
                {
                    foreach (var transformShape in transformShapes)
                    {
                        var transformType = transformShape.FindPropertyValue(MetaModelConstants.PropertyKeyClassName);

                        // Search for a matching resource.
                        var matchingTransforms = transforms.Where(t => ((Transform)t.SourceObject).FullName == transformType);

                        if (matchingTransforms == null || !matchingTransforms.Any())
                        {
                            // Log as we can't find a matching schema.
                            _logger.LogDebug(TraceMessages.TransformDependencyNotResolved, RuleName, transformType);
                            serviceDeclaration.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.TransformReferencedByServiceDeclarationIsMissing, transformType, serviceDeclaration.Key) });
                        }
                        else if (matchingTransforms.Count() > 1)
                        {
                            serviceDeclaration.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.TransformReferenceMultipleMatches, transformType, serviceDeclaration.Key, matchingTransforms.Count()) });
                        }
                        else
                        {
                            // We have a matching reference
                            var transform = matchingTransforms.Single();
                            transform.AddRelationship(new ResourceRelationship(serviceDeclaration.RefId, ResourceRelationshipType.ReferencedBy));
                            serviceDeclaration.AddRelationship(new ResourceRelationship(transform.RefId, ResourceRelationshipType.ReferencesTo));
                        }
                    }
                }
            }
        }
    }
}
