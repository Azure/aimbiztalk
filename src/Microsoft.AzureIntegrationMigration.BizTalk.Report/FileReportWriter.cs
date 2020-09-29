// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report
{
    /// <summary>
    /// Writes report text to the file system.
    /// </summary>
    public class FileReportWriter : IReportWriter
    {
        /// <summary>
        /// Writes the report to text given the location specified.
        /// </summary>
        /// <param name="location">The path to write the file.</param>
        /// <param name="content">The contents of the file to write.</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void WriteText(string location, string content)
        {
            // Output the results
            using (var stream = new StreamWriter(location))
            {
                stream.Write(content);
            }
        }
    }
}
