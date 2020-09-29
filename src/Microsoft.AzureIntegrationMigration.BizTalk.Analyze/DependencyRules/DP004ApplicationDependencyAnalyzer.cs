// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
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
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.ApplicationDefinitions;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.DependencyRules
{
    /// <summary>
    /// Defines an Analyzer to resolve dependency between applications in the source model.  If no
    /// dependency is found, it warns in the report.
    /// </summary>
    public sealed class DP004ApplicationDependencyAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "DP004";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="DP004ApplicationDependencyAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public DP004ApplicationDependencyAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(DP004ApplicationDependencyAnalyzer), model, context, logger)
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
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(DP004ApplicationDependencyAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(DP004ApplicationDependencyAnalyzer));

                // Find all application definitions in source model
                var applicationDefinitionFiles = parsedApplicationGroup.Applications.Select(a => a.Application.ApplicationDefinition).Where(ad => ad != null);
                if (applicationDefinitionFiles != null && applicationDefinitionFiles.Any())
                {
                    foreach (var applicationDefinitionFile in applicationDefinitionFiles)
                    {
                        if (applicationDefinitionFile.ApplicationDefinition != null)
                        {
                            // Get application name
                            var applicationName = applicationDefinitionFile.ApplicationDefinition.GetPropertyValue(ApplicationDefinitionConstants.PropertyKeyDisplayName);

                            // Defensive check
                            if (applicationDefinitionFile.Resource == null)
                            {
                                _logger.LogError(ErrorMessages.UnableToFindAssociatedResource, applicationDefinitionFile.GetType(), applicationName, applicationDefinitionFile.ResourceKey);
                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAssociatedResource, applicationDefinitionFile.GetType(), applicationName, applicationDefinitionFile.ResourceKey)));
                                continue;
                            }
                            
                            // Get application references
                            var applicationRefs = applicationDefinitionFile?.ApplicationDefinition?.References?.Where(r => r.Name != "BizTalk.System");
                            if (applicationRefs != null && applicationRefs.Any())
                            {
                                foreach (var applicationRef in applicationRefs)
                                {
                                    var foundReference = false;

                                    // Search all application definitions in source model for related applications
                                    var relatedApplicationDefinitionFiles = parsedApplicationGroup.Applications.Select(a => a.Application.ApplicationDefinition).Where(a => a != null && a.ApplicationDefinition != null);
                                    if (relatedApplicationDefinitionFiles != null && relatedApplicationDefinitionFiles.Any())
                                    {
                                        foreach (var relatedApplicationDefinitionFile in applicationDefinitionFiles)
                                        {
                                            // Get application name
                                            var relatedApplicationName = relatedApplicationDefinitionFile.ApplicationDefinition.GetPropertyValue(ApplicationDefinitionConstants.PropertyKeyDisplayName);

                                            // Defensive check
                                            if (relatedApplicationDefinitionFile.Resource == null)
                                            {
                                                _logger.LogError(ErrorMessages.UnableToFindAssociatedResource, relatedApplicationDefinitionFile.GetType(), relatedApplicationName, relatedApplicationDefinitionFile.ResourceKey);
                                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAssociatedResource, relatedApplicationDefinitionFile.GetType(), relatedApplicationName, relatedApplicationDefinitionFile.ResourceKey)));
                                                continue;
                                            }

                                            if (relatedApplicationName == applicationRef.Name)
                                            {
                                                // Add relationships between application resource and related application resource
                                                applicationDefinitionFile.Resource.ResourceRelationships.Add(new ResourceRelationship(relatedApplicationDefinitionFile.Resource.RefId, ResourceRelationshipType.ReferencesTo));

                                                _logger.LogDebug(TraceMessages.RelationshipCreated, RuleName, applicationDefinitionFile.ResourceKey, relatedApplicationDefinitionFile.ResourceKey, ResourceRelationshipType.ReferencesTo);

                                                // Add reverse relationship between schema resource and transform resource
                                                relatedApplicationDefinitionFile.Resource.ResourceRelationships.Add(new ResourceRelationship(applicationDefinitionFile.Resource.RefId, ResourceRelationshipType.ReferencedBy));

                                                _logger.LogDebug(TraceMessages.RelationshipCreated, RuleName, relatedApplicationDefinitionFile.ResourceKey, applicationDefinitionFile.ResourceKey, ResourceRelationshipType.ReferencedBy);

                                                foundReference = true;
                                            }
                                        }
                                    }

                                    if (!foundReference)
                                    {
                                        // Add unresolved dependency message to application resource
                                        _logger.LogWarning(WarningMessages.ApplicationReferencedByApplicationIsMissing, applicationRef.Name, applicationName);
                                        var warning = string.Format(CultureInfo.CurrentCulture, WarningMessages.ApplicationReferencedByApplicationIsMissing, applicationRef.Name, applicationName);
                                        applicationDefinitionFile.Resource.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = warning });
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogError(ErrorMessages.NoApplicationDefinition, applicationDefinitionFile.ResourceDefinitionKey);
                            var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.NoApplicationDefinition, applicationDefinitionFile.ResourceDefinitionKey);
                            Context.Errors.Add(new ErrorMessage(error));
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(DP004ApplicationDependencyAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
