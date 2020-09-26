using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze;
using Microsoft.AzureIntegrationMigration.BizTalk.Discover.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Analyze
{
    /// <summary>
    /// Stage runner for analyzing the dependencies in the source application and creating
    /// relationships between the dependencies.
    /// </summary>
    public class DependencyRulesAnalyzer : StageRunnerBase, IStageAnalyzer
    {
        /// <summary>
        /// Defines the name of this stage runner.
        /// </summary>
        public const string PluginName = ModelConstants.ApplicationPrefix + "dependencyrulesanalyzer";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyRulesAnalyzer"/> class with a generic logger.
        /// </summary>
        /// <param name="logger">A logger.</param>
        public DependencyRulesAnalyzer(ILogger logger)
            : base(PluginName, Stages.Analyze, 20, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Runs the stage runner.
        /// </summary>
        /// <param name="state">The execution state.</param>
        /// <param name="token">A cancellation token used to cancel this operation.</param>
        /// <returns>A task used to await the operation.</returns>
        protected override async Task InvokeRunAsync(IRunState state, CancellationToken token)
        {
            _ = state ?? throw new ArgumentNullException(nameof(state));

            var analyzers = new IBizTalkAnalyzer[]
            {
                Container.GetRequiredService<BizTalk.Analyze.DependencyRules.DP001SchemaDependencyAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.DependencyRules.DP002TransformDependencyAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.DependencyRules.DP003OrchestrationDependencyAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.DependencyRules.DP004ApplicationDependencyAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.DependencyRules.DP005DistributionListDependencyAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.DependencyRules.DP006ParentChildDependencyAnalyzer>(),                
            };

            _logger.LogTrace(TraceMessages.RunningDependencyRulesAnalyzer, analyzers.Length);

            // Analyze the dependencies (should be async)
            foreach (var analyzer in analyzers)
            {
                await analyzer.AnalyzeAsync(token).ConfigureAwait(false);
            }
        }
    }
}
