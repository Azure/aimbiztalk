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
    /// Implements rule AP002, which analyzes the model and creates the system application on the target source.
    /// </summary>
    public sealed class AP002SystemApplicationAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "AP002";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="AP002SystemApplicationAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public AP002SystemApplicationAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(AP002SystemApplicationAnalyzer), model, context, logger)
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
            _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(AP002SystemApplicationAnalyzer));

            Model.MigrationTarget.MessageBus.Applications.Add(new ApplicationModel.Target.Application
            {
                Name = MigrationTargetResources.SystemApplicationName,
                Key = $"{Model.MigrationTarget.MessageBus.Key}:{ModelConstants.SystemApplicationLeafKey}",
                ResourceMapKey = "systemApplication",
                Description = MigrationTargetResources.SystemApplicationDescription,
                Rating = ApplicationModel.Report.ConversionRating.FullConversion
            });

            _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(AP002SystemApplicationAnalyzer));

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
