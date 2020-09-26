using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Messages;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Deployment.Resources;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules
{
    /// <summary>
    /// Implements rule SC002, which analyzes the model and creates the property schemas on the target source.
    /// </summary>    
    public sealed class SC002PropertySchemaAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "SC002";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="SC002PropertySchemaAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public SC002PropertySchemaAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(SC002PropertySchemaAnalyzer), model, context, logger)
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
            // Get the source model applications.
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (parsedApplicationGroup?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(SC002PropertySchemaAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(SC002PropertySchemaAnalyzer));

                // Find the messages in the target application.
                var applicationMessages = from application in Model.MigrationTarget.MessageBus.Applications
                                          from message in application.Messages
                                          select new { Application = application, Message = message };

                foreach (var applicationMessage in applicationMessages)
                {
                    // Get the message definition from the source application.
                    var messageDefinitionResource = Model.FindResourceByKey(applicationMessage.Message.MessageSchema?.ResourceKeyRef);

                    if (messageDefinitionResource != null)
                    {
                        // Get the raw message definition.
                        var messageDefinition = (MessageDefinition)messageDefinitionResource.SourceObject;

                        foreach (var promotedProperty in messageDefinition.PromotedProperties)
                        {
                            // Add the routing property to the message. 
                            applicationMessage.Message.RoutingProperties.Add(promotedProperty.PropertyType, promotedProperty.XPath);
                        }
                    }
                    else
                    {
                        var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceDefinitionSchema, applicationMessage.Message.MessageSchema?.ResourceKeyRef);
                        _logger.LogError(error);
                        Context.Errors.Add(new ErrorMessage(error));
                    }
                }
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
