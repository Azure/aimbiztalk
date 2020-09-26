using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules
{
    /// <summary>
    /// Implements rule MB001, which analyzes the model and creates the messagebus on the target source.
    /// </summary>
    public sealed class MB001MessageBusAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "MB001";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="MB001MessageBusAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public MB001MessageBusAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(MB001MessageBusAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the message bus in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(MB001MessageBusAnalyzer));

            Model.MigrationTarget.MessageBus = new ApplicationModel.Target.MessageBus
            {
                Name = MigrationTargetResources.MessageBusName,
                Key = ModelConstants.MessageBusLeafKey,
                ResourceMapKey = "messageBus",
                Description = MigrationTargetResources.MessageBusDescription,
                Rating = ConversionRating.FullConversion
            };

            _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(MB001MessageBusAnalyzer));

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
