using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Messages;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules
{
    /// <summary>
    /// Implements rule SC001, which analyzes the model and creates the schema of each application on the target source.
    /// </summary>
    public sealed class SC001SchemaAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "SC001";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="SC001SchemaAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public SC001SchemaAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(SC001SchemaAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the schema for applications in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get parsed BizTalk model from the application model
            var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (parsedApplicationGroup?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(SC001SchemaAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(SC001SchemaAnalyzer));

                foreach (var targetApplication in Model.MigrationTarget.MessageBus.Applications)
                {
                    var sourceApplication = parsedApplicationGroup.Applications.SingleOrDefault(a => a.Application.Name == targetApplication.Name);
                    if (sourceApplication?.Application?.Schemas != null)
                    {
                        var schemaMessages = from schema in sourceApplication.Application.Schemas
                                             from messageDefinition in schema.MessageDefinitions
                                             where schema.SchemaType == Types.Enumerations.BizTalkSchemaType.Document
                                             select new { Schema = schema, MessageDefinition = messageDefinition };

                        foreach (var schemaMessage in schemaMessages)
                        {
                            var appName = $"{sourceApplication.Application.Name.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-")}";
                            var messageName = $"{schemaMessage.MessageDefinition.LocalName.Replace(".", "-").Replace(" ", string.Empty).Replace("/", "-").Replace(":", "-")}";

                            var resourceMapKey = $"applicationMessage{appName}{messageName}";

                            // Check if the schema is an envelope or document schema?
                            if (schemaMessage.Schema.IsEnvelope)
                            {
                                // Create an envelope message
                                var envelopeMessage = new EnvelopeMessage
                                {
                                    Name = schemaMessage.MessageDefinition.LocalName,
                                    Key = $"{targetApplication.Key}:{schemaMessage.MessageDefinition.LocalName}",
                                    Description = MigrationTargetResources.SchemaDescription,
                                    ResourceMapKey = resourceMapKey,
                                    ContentType = MessageContentType.Xml,
                                    MessageSchema = new MessageSchema
                                    {
                                        ResourceKeyRef = schemaMessage.MessageDefinition.ResourceKey,
                                        Name = schemaMessage.Schema.Name
                                    }
                                };

                                envelopeMessage.Properties[ModelConstants.MessageType] = schemaMessage.MessageDefinition.MessageType;
                                envelopeMessage.Properties[ModelConstants.TypeName] = schemaMessage.Schema.FullName;

                                targetApplication.Messages.Add(envelopeMessage);

                                _logger.LogDebug(TraceMessages.CreatedEnvelopeMessage, RuleName, schemaMessage.Schema.Name);
                            }
                            else
                            {
                                // Create a document message
                                var documentMessage = new DocumentMessage
                                {
                                    Name = schemaMessage.MessageDefinition.LocalName,
                                    Key = $"{targetApplication.Key}:{schemaMessage.MessageDefinition.LocalName}",
                                    Description = MigrationTargetResources.SchemaDescription,
                                    ResourceMapKey = resourceMapKey,
                                    ContentType = MessageContentType.Xml,
                                    MessageSchema = new MessageSchema
                                    {
                                        ResourceKeyRef = schemaMessage.MessageDefinition.ResourceKey,
                                        Name = schemaMessage.Schema.Name
                                    }
                                };
                                
                                documentMessage.Properties[ModelConstants.MessageType] = schemaMessage.MessageDefinition.MessageType;
                                documentMessage.Properties[ModelConstants.TypeName] = schemaMessage.Schema.FullName;

                                targetApplication.Messages.Add(documentMessage);

                                _logger.LogDebug(TraceMessages.CreatedDocumentMessage, RuleName, schemaMessage.Schema.Name);
                            }
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(SC001SchemaAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
