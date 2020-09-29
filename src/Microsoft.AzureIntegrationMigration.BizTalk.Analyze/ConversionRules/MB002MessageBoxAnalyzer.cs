// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Messages;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules
{
    /// <summary>
    /// Implements rule MB002, which analyzes the model and creates the message box on the target source.
    /// </summary>
    public sealed class MB002MessageBoxAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "MB002";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="MB002MessageBoxAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public MB002MessageBoxAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(MB002MessageBoxAnalyzer), model, context, logger)
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
            _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(MB002MessageBoxAnalyzer));

            var systemApplicationKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}";

            var messagingObject = Model.FindMessagingObject(systemApplicationKey);
            if (messagingObject.application != null)
            {
                // Create the message box channels
                CreateMessageBoxChannels(messagingObject.application);
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Application, systemApplicationKey);
                _logger.LogError(error);
                Context.Errors.Add(new ErrorMessage(error));
            }

            _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(MB002MessageBoxAnalyzer));

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the message box channels.
        /// </summary>
        /// <param name="systemApplication">The application in the target.</param>
        private void CreateMessageBoxChannels(Application systemApplication)
        {
            _logger.LogDebug(TraceMessages.CreatingTopicChannel, RuleName, MigrationTargetResources.MessageBoxName);

            // Create a topic channel representing the normal queue in the message box
            var topicChannel = new TopicChannel(MigrationTargetResources.MessageBoxName)
            {
                Description = MigrationTargetResources.MessageBoxDescription,
                Key = $"{systemApplication.Key}:{ModelConstants.MessageBoxLeafKey}",
                Rating = ConversionRating.FullConversion,
                ResourceMapKey = "messageBox",
                TopicName = "messagebox"
            };

            // Add to target application
            systemApplication.Channels.Add(topicChannel);

            // Create a topic channel representing the response queue in the message box
            var topicResponseChannel = new TopicChannel(MigrationTargetResources.MessageBoxResponseName)
            {
                Description = MigrationTargetResources.MessageBoxResponseDescription,
                Key = $"{systemApplication.Key}:{ModelConstants.MessageBoxResponseLeafKey}",
                Rating = ConversionRating.FullConversion,
                ResourceMapKey = "messageBoxResponse",
                TopicName = "messageboxresponse"
            };

            // Add to target application
            systemApplication.Channels.Add(topicResponseChannel);

            _logger.LogDebug(TraceMessages.CreatingQueueChannel, RuleName, MigrationTargetResources.SuspendQueueName);

            // Create a topic channel representing the suspend queue in the message box
            var suspendTopicChannel = new TopicChannel(MigrationTargetResources.SuspendQueueName)
            {
                Description = MigrationTargetResources.SuspendQueueDescription,
                Key = $"{systemApplication.Key}:{ModelConstants.SuspendQueueLeafKey}",
                ChannelType = ChannelType.InvalidMessage,
                Rating = ConversionRating.FullConversion,
                ResourceMapKey = "suspendQueue",
                TopicName = "suspendqueue"
            };

            // Add to target application
            systemApplication.Channels.Add(suspendTopicChannel);

            _logger.LogDebug(TraceMessages.CreatingQueueChannel, RuleName, MigrationTargetResources.InterchangeQueueName);

            // Create a correlating channel representing an interchange queue for aggregating interchanges for
            // recoverable interchange processing.
            var interchangeQueueChannel = new CorrelatingQueueChannel(MigrationTargetResources.InterchangeQueueName)
            {
                Description = MigrationTargetResources.InterchangeQueueDescription,
                Key = $"{systemApplication.Key}:{ModelConstants.InterchangeQueueLeafKey}",
                Rating = ConversionRating.NoAutomaticConversion,
                QueueName = "interchangeQueue"
            };

            // Add to target application
            systemApplication.Channels.Add(interchangeQueueChannel);
        }
    }
}
