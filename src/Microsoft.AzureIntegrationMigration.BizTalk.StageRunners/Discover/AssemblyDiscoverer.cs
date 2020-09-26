using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Discover
{
    /// <summary>
    /// Defines a discover stage runner that unpacks resources from BizTalk assembly files.
    /// </summary>
    public class AssemblyDiscoverer : StageRunnerBase, IStageDiscoverer
    {
        /// <summary>
        /// Defines the name of this stage runner.
        /// </summary>
        public const string PluginName = ModelConstants.ApplicationPrefix + "assemblydiscoverer";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyDiscoverer"/> class with a generic logger.
        /// </summary>
        /// <param name="logger">A logger.</param>
        public AssemblyDiscoverer(ILogger logger)
            : base(PluginName, Stages.Discover, 10, logger)
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

            // Discover what's in the assemblies (should be async)
            var assemblyDiscoverer = Container.GetRequiredService<BizTalk.Discover.AssemblyDiscoverer>();
            assemblyDiscoverer.Discover();

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
