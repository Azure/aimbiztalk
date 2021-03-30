// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
using Microsoft.AzureIntegrationMigration.BizTalk.Report.TargetReport;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report.TargetReport
{
    /// <summary>
    /// Decodes the target scenarios from the model in the application.
    /// </summary>
    public static class TargetScenarioModeller
    {
        /// <summary>
        /// Decodes the scenarios from the target model
        /// </summary>
        /// <param name="target">The target model.</param>
        /// <returns>A list of decoded applications with scenarios.</returns>
        public static IList<TargetApplication> DecodeTargetModel(AzureIntegrationServicesMigrationTarget target)
        {
            _ = target ?? throw new ArgumentNullException(nameof(target));

            var applications = new List<TargetApplication>();

            // Loop through each application on the message bus to initialize the applications.
            foreach (var application in target.MessageBus.Applications)
            {
                var targetApplication = new TargetApplication()
                {
                    Application = application
                };
                DecodeScenarios(targetApplication, target);
                RollupConversionRatings(targetApplication);
                RollupResources(targetApplication);
                applications.Add(targetApplication);
            }

            return applications;
        }

        /// <summary>
        /// Decodes the scenarios for the application.
        /// </summary>
        /// <param name="application">The application being decoded.</param>
        /// <param name="target">The whole target model, should references to other applications be required.</param>
        private static void DecodeScenarios(TargetApplication application, AzureIntegrationServicesMigrationTarget target)
        {
            // Create a list of scenarios based on the activator flag
            var scenarios = new List<TargetScenario>();
            application.Application.Endpoints.Where(e => e.Activator && e.Properties.ContainsKey(ModelConstants.ScenarioName)).ToList().ForEach(e => scenarios.Add(e.CreateScenario()));
            application.Application.Endpoints.Where(e => e.MessageExchangePattern == MessageExchangePattern.RequestReply && e.Properties.ContainsKey(ModelConstants.ScenarioName)).ToList().ForEach(e => scenarios.Add(e.CreateScenario()));
            application.Application.Intermediaries.Where(e => e.Activator).ToList().ForEach(i => scenarios.Add(i.CreateScenario()));

            // Recurse through the scenarios to find subsequent stages.
            scenarios.ForEach(s =>
            {
                var allRefs = new List<string>();
                RecurseScenarioStage(s.Activator, target, allRefs);
            });

            // Append to output.
            scenarios.ForEach(s => application.Scenarios.Add(s));
        }

        /// <summary>
        /// Recursively checks the output refs for the stage and creates links to following intermediaries and stages.
        /// </summary>
        /// <param name="stage">The current stage being walked.</param>
        /// <param name="target">The entire target model.</param>
        /// <param name="allRefs">The list of references already processed, to prevent circular scenario walks.</param>
        private static void RecurseScenarioStage(TargetScenarioStage stage, AzureIntegrationServicesMigrationTarget target, List<string> allRefs)
        {
            // The list of subsequent stages referenced by the current stage.
            var outputRefs = new List<string>();

            switch (stage.MessagingObject.Type)
            {
                case MessagingObjectType.Endpoint:
                    // The endpoint has a single output.
                    outputRefs.Add(((Endpoint)stage.MessagingObject).OutputChannelKeyRef);
                    break;
                case MessagingObjectType.Channel:
                    var channel = (Channel)stage.MessagingObject;
                    // Only add the channel if its a routing channel.
                    if (channel.Properties.ContainsKey(ModelConstants.RouteLabel))
                    {
                        // The next stage is keyed off the channel.
                        outputRefs.Add(((Channel)stage.MessagingObject).Key);
                    }
                    break;
                case MessagingObjectType.Intermediary:
                    // An intermediary can have multiple outputs.
                    outputRefs.AddRange(((Intermediary)stage.MessagingObject).OutputChannelKeyRefs);
                    break;
            }

            outputRefs.ForEach((o) =>
            {
                if (!allRefs.Contains(o))
                {
                    var mo = target.FindMessagingObject(o, stage.MessagingObject.Type);
                    if (mo != null) stage.FollowingStages.Add(mo.CreateScenarioStage());
                    allRefs.Add(o);
                }
            });

            stage.FollowingStages.ToList().ForEach(s => RecurseScenarioStage(s, target, allRefs));
        }

        /// <summary>
        /// Aggregates the Messaging Objects that have been given a rating.
        /// </summary>
        /// <param name="application">The target application.</param>
        private static void RollupConversionRatings(TargetApplication application)
        {
            var validResources = 0;
            var conversionTotal = 0;

            application.Application.Channels.Where(c => (int)c.Rating > 0).ToList().ForEach(c => 
            {
                validResources++;
                conversionTotal += (int)c.Rating;
            });

            application.Application.Intermediaries.Where(c => (int)c.Rating > 0).ToList().ForEach(c =>
            {
                validResources++;
                conversionTotal += (int)c.Rating;
            });

            application.Application.Endpoints.Where(c => (int)c.Rating > 0).ToList().ForEach(c =>
            {
                validResources++;
                conversionTotal += (int)c.Rating;
            });

            application.Application.Messages.Where(c => (int)c.Rating > 0).ToList().ForEach(c =>
            {
                validResources++;
                conversionTotal += (int)c.Rating;
            });

            // Aggregate - note the enum scores out of 5 so multiply by 20 to get a percentage.
            if (validResources > 0) application.AverageConversionRating =  20.0M * conversionTotal / validResources;
        }

        /// <summary>
        /// Rolls up the resources in the target application.
        /// </summary>
        /// <param name="application">The target application.</param>
        private static void RollupResources(TargetApplication application)
        {
            application.Application.Resources.ToList().ForEach(r => application.Resources.Add(r));
            application.Application.Channels.ToList().ForEach(m => m.Resources.ToList().ForEach(r => application.Resources.Add(r)));
            application.Application.Endpoints.ToList().ForEach(m => m.Resources.ToList().ForEach(r => application.Resources.Add(r)));
            application.Application.Intermediaries.ToList().ForEach(m => m.Resources.ToList().ForEach(r => application.Resources.Add(r)));
            application.Application.Messages.ToList().ForEach(m => m.Resources.ToList().ForEach(r => application.Resources.Add(r)));
        }

        #region Extension Methods

        /// <summary>
        /// Initializes a scenario from a messaging object.
        /// </summary>
        /// <param name="mo">The messaging object.</param>
        /// <returns>A new scenario.</returns>
        private static TargetScenario CreateScenario(this MessagingObject mo)
        {
            var scenario = new TargetScenario
            {
                Name = mo.Properties[ModelConstants.ScenarioName].ToString(),
                Activator = mo.CreateScenarioStage()
            };
            return scenario;
        }

        /// <summary>
        /// Creates a scenario stage from the messaging object,
        /// </summary>
        /// <param name="mo">The messaging object.</param>
        /// <returns>A new scenario stage.</returns>
        private static TargetScenarioStage CreateScenarioStage(this MessagingObject mo)
        {
            return new TargetScenarioStage
            {
                Name = mo.Name,
                StageType = mo.GetType().Name,
                MessagingObject = mo
            };
        }

        /// <summary>
        /// Walks the target model and finds the messaging object with the matching key.
        /// </summary>
        /// <param name="target">The target model.</param>
        /// <param name="key">The messaging object's key.</param>
        /// <param name="stageType">The current stage object's type.</param>
        /// <returns>A messaging object matching the key.</returns>
        private static MessagingObject FindMessagingObject(this AzureIntegrationServicesMigrationTarget target, string key, MessagingObjectType stageType)
        {
            var mos = new List<MessagingObject>();
            foreach (var application in target.MessageBus.Applications)
            {
                // Alternate between endpoints and channels.
                if (stageType == MessagingObjectType.Channel)
                {
                    mos.AddRange(application.Intermediaries.Where(c => c.InputChannelKeyRefs != null && c.InputChannelKeyRefs.Contains(key) && !c.Activator));
                    mos.AddRange(application.Endpoints.Where(c => c.InputChannelKeyRef == key));
                } 
                else
                {
                    mos.AddRange(application.Channels.Where(c => c.Key == key));
                }
            }
            return mos.FirstOrDefault();
        }

        #endregion
    }
}
