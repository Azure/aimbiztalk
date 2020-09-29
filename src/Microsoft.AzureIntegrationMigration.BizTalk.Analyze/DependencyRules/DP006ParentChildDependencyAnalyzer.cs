// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.ApplicationDefinitions;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Deployment.Resources;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.DependencyRules
{
    /// <summary>
    /// Defines an Analyzer to add parent-child relationships to resources if they are missing.
    /// </summary>
    public sealed class DP006ParentChildDependencyAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "DP006";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="DP006ParentChildDependencyAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public DP006ParentChildDependencyAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(DP006ParentChildDependencyAnalyzer), model, context, logger)
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
            var resources = Model.FindAllResources();

            if (resources == null || !resources.Any())
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(DP006ParentChildDependencyAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(DP006ParentChildDependencyAnalyzer));

                foreach (var parent in resources.Where(r => r.Resources != null && r.Resources.Count > 0))
                {
                    // Ensure the parent-child relationship exists for each resource.
                    foreach (var child in parent.Resources)
                    {
                        parent.AddRelationship(new ResourceRelationship(child.RefId, ResourceRelationshipType.Child));
                        child.AddRelationship(new ResourceRelationship(parent.RefId, ResourceRelationshipType.Parent));
                    }
                }

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(DP006ParentChildDependencyAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
