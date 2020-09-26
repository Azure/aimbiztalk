using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.DependencyRules
{
    /// <summary>
    /// Defines an Analyzer to resolve dependencies to schemas and ports in the source model.
    /// </summary>
    public sealed class DP002TransformDependencyAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "DP002";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="DP002TransformDependencyAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public DP002TransformDependencyAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(DP002TransformDependencyAnalyzer), model, context, logger)
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
            // Get parsed BizTalk model from the application model
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (parsedApplicationGroup?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(DP002TransformDependencyAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(DP002TransformDependencyAnalyzer));

                // Find all transforms in source model
                var transforms = parsedApplicationGroup.Applications.SelectMany(a => a.Application.Transforms);
                if (transforms != null && transforms.Any())
                {
                    foreach (var transform in transforms)
                    {
                        // Defensive check
                        if (transform.Resource == null)
                        {
                            _logger.LogError(ErrorMessages.UnableToFindAssociatedResource, transform.GetType(), transform.Name, transform.ResourceKey);
                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAssociatedResource, transform.GetType(), transform.Name, transform.ResourceKey)));
                            continue;
                        }

                        // Resolve source schemas
                        _logger.LogTrace(TraceMessages.ResolvingSourceSchemasForMap, RuleName, transform.FullName);

                        ResolveSchemaDependencies(parsedApplicationGroup, transform, true);

                        // Resolve target schemas
                        _logger.LogTrace(TraceMessages.ResolvingTargetSchemasForMap, RuleName, transform.FullName);

                        ResolveSchemaDependencies(parsedApplicationGroup, transform, false);

                        // Resolve receive and send ports
                        _logger.LogTrace(TraceMessages.ResolvingPortsForMap, RuleName, transform.FullName);

                        ResolvePortDependencies(parsedApplicationGroup, transform);

                    }
                }

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(DP002TransformDependencyAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Resolves dependencies between schemas in transform and schemas in a BizTalk application.
        /// </summary>
        /// <param name="group">The group of BizTalk applications.</param>
        /// <param name="transform">The transform.</param>
        /// <param name="isSourceSchema">Is source (true) or target (false) schema list.</param>
        private void ResolveSchemaDependencies(ParsedBizTalkApplicationGroup group, Transform transform, bool isSourceSchema)
        {
            // Determine which schema list based on the isSource flag.
            var transformSchemas = isSourceSchema ? transform.SourceSchemaTypeNames : transform.TargetSchemaTypeNames;

            foreach (var transformSchema in transformSchemas)
            {
                // Find related schema in source model
                var schema = group.Applications.SelectMany(a => a.Application.Schemas).Where(s => transformSchema == s.FullName).FirstOrDefault();
                if (schema != null)
                {
                    // Defensive check
                    if (schema.Resource == null)
                    {
                        _logger.LogError(ErrorMessages.UnableToFindAssociatedResource, schema.GetType(), schema.Name, schema.ResourceKey);
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAssociatedResource, schema.GetType(), schema.Name, schema.ResourceKey)));
                        continue;
                    }

                    // Add relationships between transform resource and schema resource
                    var transformDirection = isSourceSchema ? ResourceRelationshipType.ReferencedBy : ResourceRelationshipType.ReferencesTo;
                    transform.Resource.AddRelationship(new ResourceRelationship(schema.Resource.RefId, transformDirection));

                    _logger.LogDebug(TraceMessages.RelationshipCreated, RuleName, transform.ResourceKey, schema.ResourceKey, transformDirection);

                    // Add reverse relationship between schema resource and transform resource
                    var schemaDirection = isSourceSchema ? ResourceRelationshipType.ReferencesTo : ResourceRelationshipType.ReferencedBy;
                    schema.Resource.AddRelationship(new ResourceRelationship(transform.Resource.RefId, schemaDirection));

                    _logger.LogDebug(TraceMessages.RelationshipCreated, RuleName, schema.ResourceKey, transform.ResourceKey, schemaDirection);
                }
                else
                {
                    // Add unresolved dependency message to transform resource
                    var warning = string.Format(CultureInfo.CurrentCulture, WarningMessages.SchemaReferencedByTransformIsMissing, transformSchema, transform.ResourceKey);
                    transform.Resource.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = warning });
                }
            }
        }

        /// <summary>
        /// Resolves dependencies between receive and send port transforms and transforms in a BizTalk application.
        /// </summary>
        /// <param name="group">The group of BizTalk applications.</param>
        /// <param name="transform">The transform.</param>
        private void ResolvePortDependencies(ParsedBizTalkApplicationGroup group, Transform transform)
        {
            // Find all receive ports
            var receivePorts = group.Applications
                .Where(a => a?.Application?.Bindings?.BindingInfo?.ReceivePortCollection != null && a.Application.Bindings.BindingInfo.ReceivePortCollection.Length > 0)
                .SelectMany(a => a.Application.Bindings.BindingInfo.ReceivePortCollection)
                .Where(p => (p.Transforms != null && p.Transforms.Where(t => t.FullName == transform.FullName).Any()) || (p.OutboundTransforms != null && p.OutboundTransforms.Where(t => t.FullName == transform.FullName).Any()));

            if (receivePorts != null && receivePorts.Any())
            {
                foreach (var receivePort in receivePorts)
                {
                    // Defensive check
                    if (receivePort.Resource == null)
                    {
                        _logger.LogError(ErrorMessages.UnableToFindAssociatedResource, receivePort.GetType(), receivePort.Name, receivePort.ResourceKey);
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAssociatedResource, receivePort.GetType(), receivePort.Name, receivePort.ResourceKey)));
                        continue;
                    }

                    // Add relationships between transform resource and port resource
                    transform.Resource.AddRelationship(new ResourceRelationship(receivePort.Resource.RefId, ResourceRelationshipType.ReferencedBy));

                    _logger.LogDebug(TraceMessages.RelationshipCreated, RuleName, transform.ResourceKey, receivePort.ResourceKey, ResourceRelationshipType.ReferencedBy);

                    // Add reverse relationship between port resource and transform resource
                    receivePort.Resource.AddRelationship(new ResourceRelationship(transform.Resource.RefId, ResourceRelationshipType.ReferencesTo));

                    _logger.LogDebug(TraceMessages.RelationshipCreated, RuleName, receivePort.ResourceKey, transform.ResourceKey, ResourceRelationshipType.ReferencesTo);
                }
            }
            else
            {
                _logger.LogDebug(TraceMessages.MapNotUsedByReceivePorts, RuleName, transform.FullName);
            }

            // Find all send ports
            var sendPorts = group.Applications
                .Where(a => a?.Application?.Bindings?.BindingInfo?.SendPortCollection != null && a.Application.Bindings.BindingInfo.SendPortCollection.Length > 0)
                .SelectMany(a => a.Application.Bindings.BindingInfo.SendPortCollection)
                .Where(p => (p.Transforms != null && p.Transforms.Where(t => t.FullName == transform.FullName).Any()) || (p.InboundTransforms != null && p.InboundTransforms.Where(t => t.FullName == transform.FullName).Any()));

            if (sendPorts != null && sendPorts.Any())
            {
                foreach (var sendPort in sendPorts)
                {
                    // Defensive check
                    if (sendPort.Resource == null)
                    {
                        _logger.LogError(ErrorMessages.UnableToFindAssociatedResource, sendPort.GetType(), sendPort.Name, sendPort.ResourceKey);
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAssociatedResource, sendPort.GetType(), sendPort.Name, sendPort.ResourceKey)));
                        continue;
                    }

                    // Add relationships between transform resource and port resource
                    transform.Resource.AddRelationship(new ResourceRelationship(sendPort.Resource.RefId, ResourceRelationshipType.ReferencedBy));

                    _logger.LogDebug(TraceMessages.RelationshipCreated, RuleName, transform.ResourceKey, sendPort.ResourceKey, ResourceRelationshipType.ReferencedBy);

                    // Add reverse relationship between port resource and transform resource
                    sendPort.Resource.AddRelationship(new ResourceRelationship(transform.Resource.RefId, ResourceRelationshipType.ReferencesTo));

                    _logger.LogDebug(TraceMessages.RelationshipCreated, RuleName, sendPort.ResourceKey, transform.ResourceKey, ResourceRelationshipType.ReferencesTo);
                }
            }
            else
            {
                _logger.LogDebug(TraceMessages.MapNotUsedBySendPorts, RuleName, transform.FullName);
            }
        }
    }
}
