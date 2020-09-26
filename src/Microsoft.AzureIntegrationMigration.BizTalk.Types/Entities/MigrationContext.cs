//-----------------------------------------------------------------------
// <copyright file="MigrationContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Root object passed through the migration process.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities
{
    /// <summary>
    /// Root object passed through the migration process.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class MigrationContext
    {
        /// <summary>
        /// Gets or sets the working folder.
        /// </summary>
        public string WorkingFolder { get; set; }

        /// <summary>
        /// Gets or sets the file path to the report file.
        /// </summary>
        public string ReportFilePath { get; set; }

        /// <summary>
        /// Gets or sets the directory path for the converted output.
        /// </summary>
        public string ConversionFolder { get; set; }

        /// <summary>
        /// Gets or sets the directory path for the generated output.
        /// </summary>
        public string GenerationFolder { get; set; }

        /// <summary>
        /// Gets or sets the directory path to the template configuration files.
        /// </summary>
        public string TemplateConfigFolder { get; set; }

        /// <summary>
        /// Gets or sets the paths to the template files.
        /// </summary>
        public IList<string> TemplateFolders { get; } = new List<string>();

        /// <summary>
        /// Gets or sets a list of errors.
        /// </summary>
        public List<IErrorMessage> Errors { get; } = new List<IErrorMessage>();
    }
}
