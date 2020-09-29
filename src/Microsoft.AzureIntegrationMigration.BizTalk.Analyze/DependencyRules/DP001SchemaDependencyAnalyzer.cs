// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.DependencyRules
{
    /// <summary>
    /// Implements rule DP001, which analyzes the context properties used by document schemas for promoted properties.
    /// </summary>
    public sealed class DP001SchemaDependencyAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "DP001";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="DP001SchemaDependencyAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public DP001SchemaDependencyAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(DP001SchemaDependencyAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Analyzes the message schemas in the source tree and creates resources for them.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get the applications from the source model.
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (parsedApplicationGroup?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(DP001SchemaDependencyAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(DP001SchemaDependencyAnalyzer));

                // Find all document schemas
                var schemas = parsedApplicationGroup.Applications.SelectMany(a => a.Application.Schemas).Where(s => s.SchemaType == BizTalkSchemaType.Document);
                if (schemas != null && schemas.Any())
                {
                    foreach (var schema in schemas)
                    {
                        _logger.LogDebug(TraceMessages.AnalyzingSchemaDependencies, RuleName, schema.Name);

                        // Defensive check
                        if (schema.Resource == null)
                        {
                            _logger.LogError(ErrorMessages.UnableToFindAssociatedResource, schema.GetType(), schema.Name, schema.ResourceKey);
                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAssociatedResource, schema.GetType(), schema.Name, schema.ResourceKey)));
                            continue;
                        }

                        // Loop through the promoted properties
                        foreach (var promotedProperty in schema.PromotedProperties)
                        {
                            var foundReference = false;

                            // Find all property schemas
                            var propertySchemas = parsedApplicationGroup.Applications.SelectMany(a => a.Application.Schemas).Where(s => s.SchemaType == BizTalkSchemaType.Property);
                            if (propertySchemas != null && propertySchemas.Any())
                            {
                                foreach (var propertySchema in propertySchemas)
                                {
                                    // Defensive check
                                    if (propertySchema.Resource == null)
                                    {
                                        _logger.LogError(ErrorMessages.UnableToFindAssociatedResource, propertySchema.GetType(), propertySchema.Name, propertySchema.ResourceKey);
                                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAssociatedResource, propertySchema.GetType(), propertySchema.Name, propertySchema.ResourceKey)));
                                        continue;
                                    }

                                    var contextProperty = propertySchema.ContextProperties.Where(cp => cp.FullyQualifiedName == promotedProperty.PropertyType).FirstOrDefault();
                                    if (contextProperty != null)
                                    {
                                        // Defensive check
                                        if (contextProperty.Resource == null)
                                        {
                                            _logger.LogError(ErrorMessages.UnableToFindAssociatedResource, contextProperty.GetType(), contextProperty.FullyQualifiedName, contextProperty.ResourceKey);
                                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAssociatedResource, contextProperty.GetType(), contextProperty.FullyQualifiedName, contextProperty.ResourceKey)));
                                            continue;
                                        }

                                        // Add relationships to document schema
                                        schema.Resource.AddRelationship(new ResourceRelationship(propertySchema.Resource.RefId, ResourceRelationshipType.ReferencesTo));

                                        _logger.LogDebug(TraceMessages.RelationshipCreated, RuleName, schema.ResourceKey, propertySchema.ResourceKey, ResourceRelationshipType.ReferencesTo);

                                        schema.Resource.AddRelationship(new ResourceRelationship(contextProperty.Resource.RefId, ResourceRelationshipType.ReferencesTo));

                                        _logger.LogDebug(TraceMessages.RelationshipCreated, RuleName, schema.ResourceKey, contextProperty.ResourceKey, ResourceRelationshipType.ReferencesTo);

                                        // Add relationships to property schema
                                        propertySchema.Resource.AddRelationship(new ResourceRelationship(schema.Resource.RefId, ResourceRelationshipType.ReferencedBy));

                                        _logger.LogDebug(TraceMessages.RelationshipCreated, RuleName, propertySchema.ResourceKey, schema.ResourceKey, ResourceRelationshipType.ReferencedBy);

                                        // Add relationships to context property
                                        contextProperty.Resource.AddRelationship(new ResourceRelationship(schema.Resource.RefId, ResourceRelationshipType.ReferencedBy));

                                        _logger.LogDebug(TraceMessages.RelationshipCreated, RuleName, contextProperty.ResourceKey, schema.ResourceKey, ResourceRelationshipType.ReferencedBy);

                                        foundReference = true;
                                    }
                                }
                            }

                            if (!foundReference)
                            {
                                // Add unresolved dependency message to schema resource
                                var warning = string.Format(CultureInfo.CurrentCulture, WarningMessages.ContextPropertyReferencedBySchemaIsMissing, promotedProperty.PropertyType, schema.FullName);
                                schema.Resource.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = warning });
                            }
                        }
                    }

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(DP001SchemaDependencyAnalyzer));
                }
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
