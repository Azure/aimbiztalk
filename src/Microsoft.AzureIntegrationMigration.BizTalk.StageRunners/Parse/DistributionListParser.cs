using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Parse
{
    /// <summary>
    /// Defines a parser stage runner that parses BizTalk distribution lists.
    /// </summary>
    public class DistributionListParser : StageRunnerBase, IStageParser
    {
        /// <summary>
        /// Defines the name of this stage runner.
        /// </summary>
        public const string PluginName = ModelConstants.ApplicationPrefix + "distributionlistparser";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributionListParser"/> class with a generic logger.
        /// </summary>
        /// <param name="logger">A logger.</param>
        public DistributionListParser(ILogger logger)
            : base(PluginName, Stages.Parse, 30, logger)
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

            // Parse definition (should be async)
            var parser = Container.GetRequiredService<BizTalk.Parse.DistributionListParser>();
            parser.Parse();

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
