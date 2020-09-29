// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.BizTalk.Report.TargetReport;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report
{
    /// <summary>
    /// Represents the file structure of the report on disk.
    /// </summary>
    public class ReportFileStructure
    {
        /// <summary>
        /// Gets or sets the summary section (home).
        /// </summary>
        public ReportFile<ReportSummary> Summary { get; set; }

        /// <summary>
        /// Gets the source applications for the report.
        /// </summary>
        public IList<ReportFile<ResourceContainer>> SourceApplications { get; } = new List<ReportFile<ResourceContainer>>();

        /// <summary>
        /// Gets or sets the target message bus section for the report.
        /// </summary>
        public ReportFile<MessageBus> TargetMessageBus { get; set; }

        /// <summary>
        /// Gets the target applications for the report.
        /// </summary>
        public IList<ReportFile<TargetApplication>> TargetApplications { get; } = new List<ReportFile<TargetApplication>>();
    }
}
