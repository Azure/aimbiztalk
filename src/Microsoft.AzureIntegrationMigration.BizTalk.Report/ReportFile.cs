// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report
{
    /// <summary>
    /// Represents a file that will be written as part of the report generation.
    /// </summary>
    /// <typeparam name="T">The type of object the report file will be written from.</typeparam>
    public class ReportFile<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportFile"/> class.
        /// </summary>
        /// <param name="sectionName">The name of the report section.</param>
        /// <param name="description">The description of the section.</param>
        /// <param name="reportData">The data to render into the section.</param>
        /// <param name="prefix">The prefix to apply to the file name.</param>
        public ReportFile(string sectionName, string description, T reportData, string prefix)
        {
            SectionName = sectionName ?? throw new ArgumentNullException(nameof(sectionName));
            Description = description;
            ReportData = reportData;
            Filename = string.Concat(prefix, "-", sectionName.Replace(".", "-").Replace(" ", "-"), "-", RefId, ".html");
        }

        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets a unique reference to ensure file name uniqueness when multiple applications have the same name.
        /// </summary>
        public string RefId { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the description of the section.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the underlying data item for the report.
        /// </summary>
        public T ReportData { get; set; }

        /// <summary>
        /// Gets the report HTML document.
        /// </summary>
        public HtmlDocument ReportHtml { get; } = new HtmlDocument();
    }
}
