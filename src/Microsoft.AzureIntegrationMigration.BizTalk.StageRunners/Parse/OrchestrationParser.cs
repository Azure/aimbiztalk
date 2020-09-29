// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse;
using Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Parse
{
    /// <summary>
    /// Defines a parser stage runner that parses BizTalk orchestration definition.
    /// </summary>
    public class OrchestrationParser : StageRunnerBase, IStageParser
    {
        /// <summary>
        /// Defines the name of this stage runner.
        /// </summary>
        public const string PluginName = ModelConstants.ApplicationPrefix + "orchestrationparser";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestrationParser"/> class with a generic logger.
        /// </summary>
        /// <param name="logger">A logger.</param>
        public OrchestrationParser(ILogger logger)
            : base(PluginName, Stages.Parse, 40, logger)
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

            var parsers = new IBizTalkParser[]
            {
                Container.GetRequiredService<BizTalkOrchestrationParser>(),
                Container.GetRequiredService<OrchestrationCorrelationTypeParser>(),
                Container.GetRequiredService<OrchestrationMultiPartMessageTypeParser>(),
                Container.GetRequiredService<OrchestrationPortTypeParser>(),
                Container.GetRequiredService<OrchestrationServiceLinkTypeParser>(),
                Container.GetRequiredService<OrchestrationServiceDeclarationParser>(),
            };

            _logger.LogTrace(TraceMessages.RunningDependencyRulesAnalyzer, parsers.Length);

            // Parse definition (should be async)
            foreach (var parser in parsers)
            {
                parser.Parse();
            }
            
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
