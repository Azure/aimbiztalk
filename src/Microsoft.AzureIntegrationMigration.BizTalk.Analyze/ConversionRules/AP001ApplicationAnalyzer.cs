// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules
{
    /// <summary>
    /// Implements rule AP001, which analyzes the model and creates the application on the target source.
    /// </summary>
    public sealed class AP001ApplicationAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "AP001";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="AP001ApplicationAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public AP001ApplicationAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(AP001ApplicationAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the artifacts in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get parsed BizTalk model from the application model
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (parsedApplicationGroup?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(AP001ApplicationAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(AP001ApplicationAnalyzer));

                foreach (var application in parsedApplicationGroup.Applications)
                {
                    var resourceMapKey = $"application{application.Application.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-")}";

                    var targetApplication = new ApplicationModel.Target.Application
                    {
                        Name = application.Application.Name,
                        Key = $"{Model.MigrationTarget.MessageBus.Key}:{application.Application.Name.FormatKey()}",
                        ResourceMapKey = resourceMapKey,
                        Description = MigrationTargetResources.ApplicationDescription
                    };

                    targetApplication.Properties.Add(ModelConstants.SourceApplicationResourceKey, application.Application.ApplicationDefinition.Resource.Key);

                    Model.MigrationTarget.MessageBus.Applications.Add(targetApplication);
                }

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(AP001ApplicationAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
