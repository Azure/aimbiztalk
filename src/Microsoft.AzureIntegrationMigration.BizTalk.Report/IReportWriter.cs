using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report
{
    /// <summary>
    /// Defines the public behavior of a report writer.
    /// </summary>
    public interface IReportWriter
    {
        /// <summary>
        /// Implements the report writer functionality.
        /// </summary>
        /// <param name="location">The location to write the report to.</param>
        /// <param name="content">The text contents of the report.</param>
        void WriteText(string location, string content);
    }
}
