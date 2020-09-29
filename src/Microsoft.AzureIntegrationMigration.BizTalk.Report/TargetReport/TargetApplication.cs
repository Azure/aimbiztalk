// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report.TargetReport
{
    /// <summary>
    /// Facilitates modelling the target application.
    /// </summary>
    public class TargetApplication
    {
        /// <summary>
        /// Gets or sets the associated application.
        /// </summary>
        public Application Application { get; set;}

        /// <summary>
        /// Gets or sets the average conversaion rating.
        /// </summary>
        public decimal AverageConversionRating { get; set; } = 0;

        /// <summary>
        /// Gets or sets the scenarios within the application.
        /// </summary>
        public IList<TargetScenario> Scenarios { get; } = new List<TargetScenario>();

        /// <summary>
        /// Gets a list of resources associated with the target application.
        /// </summary>
        public IList<TargetResourceTemplate> Resources { get; } = new List<TargetResourceTemplate>();
    }
}
