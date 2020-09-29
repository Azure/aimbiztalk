// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Messages;
using Microsoft.AzureIntegrationMigration.BizTalk.Report.TargetReport;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Report.Tests
{
    /// <summary>
    /// Class containing tests for the <see cref="TargetScenarioModeller"/>.
    /// </summary>
    public class TargetScenarioModellerFeature
    {
        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DecodeScenariosHappyPath(AzureIntegrationServicesModel model, IList<TargetApplication> result)
        {
            "Given a model"
                .x(() =>
                {
                    model = TestHelper.BuildModel();
                });

            "When comparing x and y"
                .x(() => result = TargetScenarioModeller.DecodeTargetModel(model.MigrationTarget));

            "Expect the correct number of applications"
                .x(() => {
                    var results = result.ToList();
                    results.Count.Should().Be(2);
                });
        }
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
