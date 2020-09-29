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
using Microsoft.AzureIntegrationMigration.BizTalk.Discover.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Report
{
    /// <summary>
    /// Stage runner for generating a BizTalk migration report in HTML format.
    /// </summary>
    public sealed class HtmlReporter : StageRunnerBase, IStageReporter
    {
        /// <summary>
        /// Defines the name of this stage runner.
        /// </summary>
        public const string PluginName = ModelConstants.ApplicationPrefix + "htmlreporter";

        /// <summary>
        /// Defines an argument name representing the report file path.
        /// </summary>
        public const string ArgReportFile = PluginName + ".reportfile";

        /// <summary>
        /// Defines the default name for the report.
        /// </summary>
        public const string DefaultReportFileName = "BizTalkMigrationReport.html";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the report file path.
        /// </summary>
        private string _reportFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlReporter"/> class with a generic logger.
        /// </summary>
        /// <param name="logger">A logger.</param>
        public HtmlReporter(ILogger logger)
            : base(PluginName, Stages.Report, 0, logger)
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
            // Default file path if not specified
            if (string.IsNullOrWhiteSpace(_reportFilePath))
            {
                _reportFilePath = Path.Combine(Environment.CurrentDirectory, DefaultReportFileName);
            }

            _logger.LogInformation(InformationMessages.ReportFilePath, _reportFilePath);

            // Try and create directories if they doesn't exist
            var fileInfo = new FileInfo(_reportFilePath);
            if (!fileInfo.Directory.Exists)
            {
                _logger.LogDebug(TraceMessages.CreatingDirectoriesForReportFilePath, _reportFilePath);

                // Ensure directories exist
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            }

            // Get context (initialize the context - means this runner MUST run first)
            var context = Container.GetRequiredService<MigrationContext>();
            context.ReportFilePath = _reportFilePath;

            // Build report (should be async)
            var reporter = Container.GetRequiredService<BizTalk.Report.HtmlReportFormatter>();
            reporter.Report();

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Gets arguments for this stage runner.
        /// </summary>
        protected override void GetArgs()
        {
            // Report File
            var argReportFile = GetArg<string>(ArgReportFile);
            if (!string.IsNullOrWhiteSpace(argReportFile))
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArgReportFile, argReportFile);

                _reportFilePath = new FileInfo(argReportFile).FullName;
            }
        }
    }
}
