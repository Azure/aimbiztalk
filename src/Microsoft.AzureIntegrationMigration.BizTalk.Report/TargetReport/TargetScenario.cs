using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report.TargetReport
{
    /// <summary>
    /// Models a scenario in the target report.
    /// </summary>
    public class TargetScenario
    {
        /// <summary>
        /// Gets or sets the name of the scenario.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the entry point of the scenario.
        /// </summary>
        public TargetScenarioStage Activator { get; set; }
    }
}
