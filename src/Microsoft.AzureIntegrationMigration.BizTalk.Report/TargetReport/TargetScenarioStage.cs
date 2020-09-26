using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report.TargetReport
{
    /// <summary>
    /// Represents a discrete stage in a target scenario.
    /// </summary>
    public class TargetScenarioStage
    {
        /// <summary>
        /// Gets or sets the type of stage, e.g. Endpoint or Message.
        /// </summary>
        public string StageType { get; set; }

        /// <summary>
        /// Gets or sets the name of the stage.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the underlying messaging object associated with the stage.
        /// </summary>
        public MessagingObject MessagingObject { get; set; }

        /// <summary>
        /// The subsequent stages following this one.
        /// </summary>
        public IList<TargetScenarioStage> FollowingStages { get; } = new List<TargetScenarioStage>();
    }
}
