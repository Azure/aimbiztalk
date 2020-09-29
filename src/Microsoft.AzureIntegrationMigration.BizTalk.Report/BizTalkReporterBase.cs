// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.Report.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report
{
    /// <summary>
    /// Base functionality for a BizTalk reporter.
    /// </summary>
    public abstract class BizTalkReporterBase : IBizTalkReporter
    {
        /// <summary>
        /// Default constructor for dependency injection.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="state">The application's run state.</param>
        /// <param name="writer">An instance of a <see cref="IReportWriter"/> to be used for writing the report.</param>
        /// <param name="logger">An instance of a <see cref="ILogger"/> to be used for logging within the class.</param>
        protected BizTalkReporterBase(IApplicationModel model, MigrationContext context, IRunState state, IReportWriter writer, ILogger logger)
        {
            Model = (AzureIntegrationServicesModel)model ?? throw new ArgumentNullException(nameof(model));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            State = state ?? throw new ArgumentNullException(nameof(state));
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets or sets the <see cref="ILogger"/> that is logging for the application.
        /// </summary>
        protected ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IReportWriter"/> that is writing the report.
        /// </summary>
        protected IReportWriter Writer { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IRunState"/> that is being analyzed.
        /// </summary>
        protected IRunState State { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MigrationContext"/> that is being analyzed.
        /// </summary>
        protected MigrationContext Context { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AzureIntegrationServicesModel"/> being analyzed.
        /// </summary>
        protected AzureIntegrationServicesModel Model { get; set; }

        /// <summary>
        /// Generates the report output.
        /// </summary>
        public void Report()
        {
            Logger.LogInformation(InformationMessages.StartReport, GetType().Name);

            ReportInternal();

            Logger.LogInformation(InformationMessages.EndReport, GetType().Name);
        }

        /// <summary>
        /// Performs the work of reporting.
        /// </summary>
        protected abstract void ReportInternal();
    }
}
