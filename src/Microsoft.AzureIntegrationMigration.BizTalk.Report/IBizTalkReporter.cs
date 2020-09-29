// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report
{
    /// <summary>
    /// Defines standard behavior for a reporter.
    /// </summary>
    public interface IBizTalkReporter
    {
        /// <summary>
        /// Generates a report to the specified output location.
        /// </summary>
        void Report();
    }
}
