// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
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
    /// Stage runner for running the rules for analyzing the convertability of resources.
    /// </summary>
    public class ConversionRulesAnalyzer : StageRunnerBase, IStageAnalyzer
    {
        /// <summary>
        /// Defines the name of this stage runner.
        /// </summary>
        public const string PluginName = ModelConstants.ApplicationPrefix + "conversionrulesanalyzer";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversionRulesAnalyzer"/> class with a generic logger.
        /// </summary>
        /// <param name="logger">A logger.</param>
        public ConversionRulesAnalyzer(ILogger logger)
            : base(PluginName, Stages.Analyze, 30, logger)
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
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.MB001MessageBusAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.AP001ApplicationAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.AP002SystemApplicationAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.MB002MessageBoxAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.AP003ReceivePortScenarioAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.AP004SendPortScenarioAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.AP005OrchestrationScenarioAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.SC001SchemaAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.SC002PropertySchemaAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.RP001FtpReceivePortAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.RP002FileReceivePortAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.RP003HttpReceivePortAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.RP004SftpReceivePortAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.RP005MqReceivePortAnalyzer>(),
                //Container.GetRequiredService<BizTalk.Analyze.ConversionRules.RP006SapReceivePortAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.SP001FtpSendPortAnalyzer>(),                
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.SP002FileSendPortAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.SP003HttpSendPortAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.SP004SftpSendPortAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.SP005MqSendPortAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.SP006SapSendPortAnalyzer>(),
                Container.GetRequiredService<BizTalk.Analyze.ConversionRules.MA001TransformAnalyzer>()
            };

            _logger.LogTrace(TraceMessages.RunningDependencyRulesAnalyzer, analyzers.Length);

            // Analyze the dependencies.
            foreach (var analyzer in analyzers)
            {
                await analyzer.AnalyzeAsync(token).ConfigureAwait(false);
            }
        }
    }
}
