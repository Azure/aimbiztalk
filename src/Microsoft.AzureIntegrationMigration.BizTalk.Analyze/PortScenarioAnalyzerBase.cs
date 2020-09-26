using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze
{
    /// <summary>
    /// Implements a base class for receive and send port message scenario analyzers.
    /// </summary>
    public abstract class PortScenarioAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of the analyzer.
        /// </summary>
        private readonly string _analyzerName;

        /// <summary>
        /// Defines the name of the rule.
        /// </summary>
        private readonly string _ruleName;

        /// <summary>
        /// Creates a new instance of a <see cref="PortScenarioAnalyzer"/> class.
        /// </summary>
        /// <param name="analyzerName">The name of the analyzer.</param>
        /// <param name="analyzerRuleName">The name of the analyzer rule.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        protected PortScenarioAnalyzer(string analyzerName, string analyzerRuleName, IApplicationModel model, MigrationContext context, ILogger logger)
            : base(analyzerName, model, context, logger)
        {
            // Validate and set the members
            _analyzerName = analyzerName ?? throw new ArgumentNullException(nameof(analyzerName));
            _ruleName = analyzerRuleName ?? throw new ArgumentNullException(nameof(analyzerRuleName));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the artifacts in the migration target.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Find all application resources
            var applications = Model.FindResourcesByType(ModelConstants.ResourceApplication);
            if (applications != null && applications.Any())
            {
                _logger.LogDebug(TraceMessages.RunningRule, _ruleName, _analyzerName);

                foreach (var application in applications)
                {
                    // Find target application (compare against resource key in the properties of the target application rather than the name
                    // because sometimes a duplicate application could exist if disparate unrelated MSIs are provided to the tool).
                    var targetApplication = Model.MigrationTarget.MessageBus.Applications.Where(a => a.Properties.ContainsKey(ModelConstants.SourceApplicationResourceKey) && a.Properties[ModelConstants.SourceApplicationResourceKey].ToString() == application.Key).FirstOrDefault();
                    if (targetApplication != null)
                    {
                        // Find all receive ports and build messaging scenarios
                        await AnalyzePortsAsync(application, targetApplication, token).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.LogError(ErrorMessages.UnableToFindMessagingObjectWithNameInTargetModel, MessagingObjectType.Application, application.Name);
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithNameInTargetModel, MessagingObjectType.Application, application.Name)));
                    }
                }

                _logger.LogDebug(TraceMessages.RuleCompleted, _ruleName, _analyzerName);
            }
            else
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, _ruleName, _analyzerName);
            }
        }

        /// <summary>
        /// Override to analyze the receive or send ports.
        /// </summary>
        /// <param name="sourceApplication">The application object in the source model.</param>
        /// <param name="targetApplication">The application object in the target model.</param>
        protected abstract Task AnalyzePortsAsync(ResourceItem sourceApplication, Application targetApplication, CancellationToken token);

        /// <summary>
        /// Creates the default pipeline intermediaries, if they are in the model.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="pipelineRef">The pipeline reference from the source application.</param>
        /// <param name="intermediaries">The list to add the pipeline intermediary to.</param>
        protected void CreateDefaultPipelineIntermediaries(string intermediaryKeyPrefix, Types.Entities.Bindings.PipelineRef pipelineRef, List<MessagingObject> intermediaries)
        {
            _ = pipelineRef ?? throw new ArgumentNullException(nameof(pipelineRef));
            _ = intermediaries ?? throw new ArgumentNullException(nameof(intermediaries));

            switch (pipelineRef.Name)
            {
                case "Microsoft.BizTalk.DefaultPipelines.PassThruReceive":

                    _logger.LogDebug(TraceMessages.PassthruPipelineDoesNotRequireIntermediaries, _ruleName, pipelineRef.Name);

                    break;

                case "Microsoft.BizTalk.DefaultPipelines.PassThruTransmit":

                    _logger.LogDebug(TraceMessages.PassthruPipelineDoesNotRequireIntermediaries, _ruleName, pipelineRef.Name);

                    break;

                case "Microsoft.BizTalk.DefaultPipelines.XMLReceive":

                    _logger.LogDebug(TraceMessages.CreatingDefaultPipelineComponentIntermediaries, _ruleName, pipelineRef.Name);

                    // Mimic a DocumentStageComponent for the default XML Disassembler
                    var xmlDisassemblerComponent = new DocumentStageComponent()
                    {
                        Name = "Microsoft.BizTalk.Component.XmlDasmComp",
                        ComponentName = MigrationTargetResources.XmlDisassemblerName,
                        Properties = new DocumentStageComponentProperty[7]
                        {
                                new DocumentStageComponentProperty() { Name = "EnvelopeSpecNames", Value = null },
                                new DocumentStageComponentProperty() { Name = "EnvelopeSpecTargetNamespaces", Value = null },
                                new DocumentStageComponentProperty() { Name = "DocumentSpecNames", Value = null },
                                new DocumentStageComponentProperty() { Name = "DocumentSpecTargetNamespaces", Value = null },
                                new DocumentStageComponentProperty() { Name = "AllowUnrecognizedMessage", Value = false.ToString() },
                                new DocumentStageComponentProperty() { Name = "ValidateDocument", Value = false.ToString() },
                                new DocumentStageComponentProperty() { Name = "RecoverableInterchangeProcessing", Value = false.ToString() }
                        }
                    };

                    // Create intermediaries representing the XML Disassembler
                    intermediaries.AddRange(CreatePipelineIntermediaries(intermediaryKeyPrefix, xmlDisassemblerComponent, null));

                    // TODO: Add the Party Resolver although currently that isn't supported in an Azure Logic App template

                    break;

                case "Microsoft.BizTalk.DefaultPipelines.XMLTransmit":

                    _logger.LogDebug(TraceMessages.CreatingDefaultPipelineComponentIntermediaries, _ruleName, pipelineRef.Name);

                    // Mimic a DocumentStageComponent for the default XML Assembler
                    var xmlAssemblerComponent = new DocumentStageComponent()
                    {
                        Name = "Microsoft.BizTalk.Component.XmlAsmComp",
                        ComponentName = MigrationTargetResources.XmlAssemblerName,
                        Properties = new DocumentStageComponentProperty[11]
                        {
                                new DocumentStageComponentProperty() { Name = "EnvelopeDocSpecNames", Value = null },
                                new DocumentStageComponentProperty() { Name = "EnvelopeSpecTargetNamespaces", Value = null },
                                new DocumentStageComponentProperty() { Name = "DocumentSpecNames", Value = null },
                                new DocumentStageComponentProperty() { Name = "DocumentSpecTargetNamespaces", Value = null },
                                new DocumentStageComponentProperty() { Name = "XmlAsmProcessingInstructions", Value = null },
                                new DocumentStageComponentProperty() { Name = "ProcessingInstructionsOptions", Value = "0" },
                                new DocumentStageComponentProperty() { Name = "ProcessingInstructionsScope", Value = "0" },
                                new DocumentStageComponentProperty() { Name = "AddXmlDeclaration", Value = true.ToString() },
                                new DocumentStageComponentProperty() { Name = "TargetCharset", Value = null },
                                new DocumentStageComponentProperty() { Name = "TargetCodePage", Value = "0" },
                                new DocumentStageComponentProperty() { Name = "PreserveBom", Value = true.ToString() }
                        }
                    };

                    // Create intermediaries representing the XML Assembler
                    intermediaries.AddRange(CreatePipelineIntermediaries(intermediaryKeyPrefix, xmlAssemblerComponent, null));

                    break;

                default:

                    _logger.LogWarning(WarningMessages.SkippingUnknownDefaultPipeline, pipelineRef.Name);

                    break;
            }
        }

        /// <summary>
        /// Creates a default pair of intermediaries for content promotion and topic publishing (message box).
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="topicChannelKey">The key for the topic channel the intermediary will publish to.</param>
        /// <param name="useSessions">True if the message publisher should set a session ID, otherwise False.</param>
        /// <param name="sessionProperty">The name of the session property from the message header to use as the session ID.</param>
        /// <returns>Returns list of intermediaries.</returns>
        protected IList<MessagingObject> CreateMessageAgentIntermediaries(string intermediaryKeyPrefix, string topicChannelKey, bool useSessions, string sessionProperty)
        {
            var intermediaries = new List<MessagingObject>();

            _logger.LogDebug(TraceMessages.CreatingContentPromoterIntermediary, _ruleName, MigrationTargetResources.ContentPromoterName);

            // Find topic channel
            var messagingObject = Model.FindMessagingObject(topicChannelKey);
            if (messagingObject.messagingObject != null)
            {
                var topicChannel = (TopicChannel)messagingObject.messagingObject;

                // Create an intermediary based on the content promoter (resources pulled in via message publisher resource map key)
                var contentPromoterIntermediary = new ContentPromoter(MigrationTargetResources.ContentPromoterName)
                {
                    Description = MigrationTargetResources.ContentPromoterDescription,
                    Key = $"{intermediaryKeyPrefix}:{ModelConstants.ContentPromoterLeafKey}",
                    Rating = ConversionRating.FullConversion
                };

                contentPromoterIntermediary.Properties.Add(ModelConstants.ScenarioStepName, "contentPromoter");

                // Add configuration properties
                contentPromoterIntermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());

                intermediaries.Add(contentPromoterIntermediary);

                _logger.LogDebug(TraceMessages.CreatingMessagePublisherIntermediary, _ruleName, MigrationTargetResources.MessagePublisherName);

                // Create an intermediary based on the message publisher
                var topicPublisherIntermediary = new MessagePublisher(MigrationTargetResources.MessagePublisherName)
                {
                    Description = MigrationTargetResources.MessagePublisherDescription,
                    Key = $"{intermediaryKeyPrefix}:{ModelConstants.MessagePublisherLeafKey}",
                    Rating = ConversionRating.FullConversion,
                    ResourceMapKey = "messageAgent"
                };

                topicPublisherIntermediary.Properties.Add(ModelConstants.ScenarioStepName, "topicPublisher");

                // Add configuration properties
                var config = new Dictionary<string, object>()
                {
                    { ModelConstants.TopicName, topicChannel.TopicName },
                    { ModelConstants.UseSessions, useSessions }
                };

                if (useSessions)
                {
                    config.Add(ModelConstants.SessionPropertyName, sessionProperty);
                }

                topicPublisherIntermediary.Properties.Add(ModelConstants.ConfigurationEntry, config);

                // Route to topic channel
                topicPublisherIntermediary.OutputChannelKeyRefs.Add(topicChannel.Key);

                intermediaries.Add(topicPublisherIntermediary);
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, topicChannelKey);
                _logger.LogError(error);
                Context.Errors.Add(new ErrorMessage(error));
            }

            return intermediaries;
        }

        /// <summary>
        /// Creates a list of intermediaries representing the receive pipeline.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="sourceApplication">The application to parse for receive pipelines.</param>
        /// <param name="pipelineRef">The pipeline reference from the receive handler.</param>
        /// <param name="pipelineStages">The stages of the pipeline.</param>
        /// <returns>Returns list of intermediaries.</returns>
        protected IList<MessagingObject> CreateReceivePipelineIntermediaries(string intermediaryKeyPrefix, ResourceItem sourceApplication, Types.Entities.Bindings.PipelineRef pipelineRef, Root pipelineStages)
        {
            _ = pipelineRef ?? throw new ArgumentNullException(nameof(pipelineRef));

            var intermediaries = new List<MessagingObject>();

            // Find pipeline in the application (which contains the default property values).
            // If the pipeline data in the binding file is null then the properties of the pipeline components haven't been overridden.
            var pipeline = sourceApplication
                .FindRelatedResourcesByType(Model, ResourceRelationshipType.Child, ModelConstants.ResourceReceivePipeline)
                .Where(r => r.SourceObject != null)
                .Select(r => (Pipeline)r.SourceObject)
                .Where(p => p.FullName == pipelineRef.Name)
                .SingleOrDefault();

            if (pipeline != null)
            {
                if (pipeline.Document?.Stages != null && pipeline.Document.Stages.Any())
                {
                    _logger.LogDebug(TraceMessages.CreatingPipelineComponentIntermediaries, _ruleName, pipelineRef.Name);

                    var pipelineCount = 0;

                    foreach (var stage in pipeline.Document.Stages)
                    {
                        if (stage.Components != null && stage.Components.Any())
                        {
                            foreach (var component in stage.Components)
                            {
                                _logger.LogDebug(TraceMessages.CreatingPipelineComponentIntermediary, _ruleName, component.ComponentName, pipelineRef.Name);

                                // Create intermediaries representing pipeline component
                                intermediaries.AddRange(CreatePipelineIntermediaries(intermediaryKeyPrefix, component, pipelineStages));

                                pipelineCount++;
                            }
                        }
                    }

                    _logger.LogDebug(TraceMessages.FoundPipelineComponentsInPipeline, _ruleName, pipelineCount, pipelineRef.Name);
                }
                else
                {
                    _logger.LogError(ErrorMessages.PipelineHasNoStages, pipelineRef.Name);
                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.PipelineHasNoStages, pipelineRef.Name)));
                }
            }
            else
            {
                _logger.LogTrace(TraceMessages.CheckingForDefaultPipeline, _ruleName);

                CreateDefaultPipelineIntermediaries(intermediaryKeyPrefix, pipelineRef, intermediaries);
            }

            return intermediaries;
        }

        /// <summary>
        /// Creates a list of intermediaries representing the pipeline.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="sourceApplication">The application to parse for send pipelines.</param>
        /// <param name="pipelineRef">The pipeline reference from the send handler.</param>
        /// <param name="pipelineStages">The stages of the pipeline.</param>
        /// <returns>Returns list of intermediaries.</returns>
        protected IList<MessagingObject> CreateSendPipelineIntermediaries(string intermediaryKeyPrefix, ResourceItem sourceApplication, Types.Entities.Bindings.PipelineRef pipelineRef, Root pipelineStages)
        {
            _ = pipelineRef ?? throw new ArgumentNullException(nameof(pipelineRef));

            var intermediaries = new List<MessagingObject>();

            // Find pipeline in the application (which contains the default property values).
            // If the pipeline data in the binding file is null then the properties of the pipeline components haven't been overridden.
            var pipeline = sourceApplication
                .FindRelatedResourcesByType(Model, ResourceRelationshipType.Child, ModelConstants.ResourceSendPipeline)
                .Where(r => r.SourceObject != null)
                .Select(r => (Pipeline)r.SourceObject)
                .Where(p => p.FullName == pipelineRef.Name)
                .SingleOrDefault();

            if (pipeline != null)
            {
                if (pipeline.Document?.Stages != null && pipeline.Document.Stages.Any())
                {
                    _logger.LogDebug(TraceMessages.CreatingPipelineComponentIntermediaries, _ruleName, pipelineRef.Name);

                    var pipelineCount = 0;

                    foreach (var stage in pipeline.Document.Stages)
                    {
                        if (stage.Components != null && stage.Components.Any())
                        {
                            foreach (var component in stage.Components)
                            {
                                _logger.LogDebug(TraceMessages.CreatingPipelineComponentIntermediary, _ruleName, component.ComponentName, pipelineRef.Name);

                                // Create intermediaries representing pipeline component
                                intermediaries.AddRange(CreatePipelineIntermediaries(intermediaryKeyPrefix, component, pipelineStages));

                                pipelineCount++;
                            }
                        }
                    }

                    _logger.LogDebug(TraceMessages.FoundPipelineComponentsInPipeline, _ruleName, pipelineCount, pipelineRef.Name);
                }
                else
                {
                    _logger.LogError(ErrorMessages.PipelineHasNoStages, pipelineRef.Name);
                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.PipelineHasNoStages, pipelineRef.Name)));
                }
            }
            else
            {
                _logger.LogTrace(TraceMessages.CheckingForDefaultPipeline, _ruleName);

                CreateDefaultPipelineIntermediaries(intermediaryKeyPrefix, pipelineRef, intermediaries);
            }

            return intermediaries;
        }

        /// <summary>
        /// Creates one or more intermediaries representing a pipeline component.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="component">The pipeline component.</param>
        /// <returns>Returns list of intermediaries.</returns>
        protected IList<Intermediary> CreatePipelineIntermediaries(string intermediaryKeyPrefix, DocumentStageComponent component, Root pipelineStages)
        {
            _ = intermediaryKeyPrefix ?? throw new ArgumentNullException(nameof(intermediaryKeyPrefix));
            _ = component ?? throw new ArgumentNullException(nameof(component));

            var intermediaries = new List<Intermediary>(1);

            if (component.ComponentName == MigrationTargetResources.XmlDisassemblerName)
            {
                intermediaries.AddRange(CreateXmlDisassemblerPipelineIntermediaries(intermediaryKeyPrefix, component, pipelineStages));
            }
            else if (component.ComponentName == MigrationTargetResources.XmlAssemblerName)
            {
                intermediaries.AddRange(CreateXmlAssemblerPipelineIntermediaries(intermediaryKeyPrefix, component, pipelineStages));
            }
            else if (component.ComponentName == MigrationTargetResources.FlatFileDisassemblerName)
            {
                var intermediary = new GenericFilter(component.ComponentName)
                {
                    Component = "Microsoft.BizTalk.Component.FFDasmComp",
                    Description = MigrationTargetResources.FlatFileDisassemblerDescription,
                    Key = $"{intermediaryKeyPrefix}:FFDasmComp",
                    Rating = ConversionRating.NoAutomaticConversion
                };
                intermediary.Properties.Add(ModelConstants.ScenarioStepName, "flatFileMessageProcessor");
                intermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());
                intermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());
                intermediary.ComponentProperties.Merge<string, object>(GetPipelineProperties(component, pipelineStages));
                intermediaries.Add(intermediary);
            }
            else if (component.ComponentName == MigrationTargetResources.FlatFileAssemblerName)
            {
                var intermediary = new GenericFilter(component.ComponentName)
                {
                    Component = "Microsoft.BizTalk.Component.BTFAsmComp",
                    Description = MigrationTargetResources.FlatFileAssemblerDescription,
                    Key = $"{intermediaryKeyPrefix}:BTFAsmComp",
                    Rating = ConversionRating.NoAutomaticConversion
                };
                intermediary.Properties.Add(ModelConstants.ScenarioStepName, "flatFileAssembler");
                intermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());
                intermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());
                intermediary.ComponentProperties.Merge<string, object>(GetPipelineProperties(component, pipelineStages));
                intermediaries.Add(intermediary);
            }
            else if (component.ComponentName == MigrationTargetResources.BizTalkFrameworkDisassemblerName)
            {
                var intermediary = new GenericFilter(component.ComponentName)
                {
                    Component = "Microsoft.BizTalk.Component.BtfDisassembler",
                    Description = MigrationTargetResources.BizTalkFrameworkDisassemblerDescription,
                    Key = $"{intermediaryKeyPrefix}:BtfDisassembler",
                    Rating = ConversionRating.NoAutomaticConversion
                };
                intermediary.Properties.Add(ModelConstants.ScenarioStepName, "biztalkFrameworkDisassembler");
                intermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());
                intermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());
                intermediary.ComponentProperties.Merge<string, object>(GetPipelineProperties(component, pipelineStages));
                intermediaries.Add(intermediary);
            }
            else if (component.ComponentName == MigrationTargetResources.BizTalkFrameworkAssemblerName)
            {
                var intermediary = new GenericFilter(component.ComponentName)
                {
                    Component = "Microsoft.BizTalk.Component.BtfAssembler",
                    Description = MigrationTargetResources.BizTalkFrameworkAssemblerDescription,
                    Key = $"{intermediaryKeyPrefix}:BtfAssembler",
                    Rating = ConversionRating.NoAutomaticConversion
                };
                intermediary.Properties.Add(ModelConstants.ScenarioStepName, "biztalkFrameworkAssembler");
                intermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());
                intermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());
                intermediary.ComponentProperties.Merge<string, object>(GetPipelineProperties(component, pipelineStages));
                intermediaries.Add(intermediary);
            }
            else if (component.ComponentName == MigrationTargetResources.JsonDecoderName)
            {
                intermediaries.Add(CreateJsonDecoderIntermediary(intermediaryKeyPrefix, component, pipelineStages));
            }
            else if (component.ComponentName == MigrationTargetResources.JsonEncoderName)
            {
                intermediaries.Add(CreateJsonEncoderIntermediary(intermediaryKeyPrefix, component, pipelineStages));
            }
            else if (component.ComponentName == MigrationTargetResources.MimeDecoderName)
            {
                var intermediary = new GenericFilter(component.ComponentName)
                {
                    Component = "Microsoft.BizTalk.Component.Mime",
                    Description = MigrationTargetResources.MimeDecoderDescription,
                    Key = $"{intermediaryKeyPrefix}:MimeDecoder",
                    Rating = ConversionRating.NoAutomaticConversion
                };
                intermediary.Properties.Add(ModelConstants.ScenarioStepName, "mimeDecoder");
                intermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());
                intermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());
                intermediary.ComponentProperties.Merge<string, object>(GetPipelineProperties(component, pipelineStages));
                intermediaries.Add(intermediary);
            }
            else if (component.ComponentName == MigrationTargetResources.MimeEncoderName)
            {
                var intermediary = new GenericFilter(component.ComponentName)
                {
                    Component = "Microsoft.BizTalk.Component.Mime",
                    Description = MigrationTargetResources.MimeEncoderDescription,
                    Key = $"{intermediaryKeyPrefix}:MimeEncoder",
                    Rating = ConversionRating.NoAutomaticConversion
                };
                intermediary.Properties.Add(ModelConstants.ScenarioStepName, "mimeEncoder");
                intermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());
                intermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());
                intermediary.ComponentProperties.Merge<string, object>(GetPipelineProperties(component, pipelineStages));
                intermediaries.Add(intermediary);
            }
            else if (component.ComponentName == MigrationTargetResources.PartyResolverName)
            {
                var intermediary = new GenericFilter(component.ComponentName)
                {
                    Component = "Microsoft.BizTalk.Component.PartyRes",
                    Description = MigrationTargetResources.PartyResolverDescription,
                    Key = $"{intermediaryKeyPrefix}:PartyRes",
                    Rating = ConversionRating.NoAutomaticConversion
                };
                intermediary.Properties.Add(ModelConstants.ScenarioStepName, "partyResolution");
                intermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());
                intermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());
                intermediary.ComponentProperties.Merge<string, object>(GetPipelineProperties(component, pipelineStages));
                intermediaries.Add(intermediary);
            }
            else if (component.ComponentName == MigrationTargetResources.XmlValidatorName)
            {
                intermediaries.Add(CreateXmlValidatorIntermediary(intermediaryKeyPrefix, component, pipelineStages));
            }
            else
            {
                _logger.LogTrace(TraceMessages.PipelineComponentNotRecognisedAsDefaultComponent, _ruleName, component.ComponentName);

                // Not a default component, must be custom
                var intermediary = new GenericFilter(component.ComponentName)
                {
                    Component = component.ComponentName,
                    Description = component.Description,
                    Key = $"{intermediaryKeyPrefix}:{component.ComponentName.FormatKey()}",
                    Rating = ConversionRating.NoAutomaticConversion
                };
                intermediary.Properties.Add(ModelConstants.ScenarioStepName, component.ComponentName);
                intermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());
                intermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());
                intermediary.ComponentProperties.Merge<string, object>(GetPipelineProperties(component, pipelineStages));
                intermediaries.Add(intermediary);
            }

            return intermediaries;
        }

        /// <summary>
        /// Creates an intermediary representing a message translator.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="targetApplication">The application object in the target model.</param>
        /// <param name="maps">The maps.</param>
        /// <returns>Returns a message translator.</returns>
        protected MessagingObject CreateMapIntermediary(string intermediaryKeyPrefix, Application targetApplication, IList<ResourceItem> maps)
        {
            _ = intermediaryKeyPrefix ?? throw new ArgumentNullException(nameof(intermediaryKeyPrefix));
            _ = targetApplication ?? throw new ArgumentNullException(nameof(targetApplication));
            _ = maps ?? throw new ArgumentNullException(nameof(maps));

            _logger.LogDebug(TraceMessages.CreatingMapIntermediary, _ruleName, maps.Count);

            MessageTranslator intermediary = null;

            // Find suspend queue channel
            var suspendQueueChannelKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}:{ModelConstants.SuspendQueueLeafKey}";

            var messagingObject = Model.FindMessagingObject(suspendQueueChannelKey);
            if (messagingObject.messagingObject != null)
            {
                // Create an intermediary based on the message translator
                intermediary = new MessageTranslator(MigrationTargetResources.XmlMessageTranslatorName)
                {
                    Description = MigrationTargetResources.XmlMessageTranslatorDescription,
                    Key = $"{intermediaryKeyPrefix}:{ModelConstants.XmlMessageTranslatorLeafKey}",
                    Rating = ConversionRating.FullConversion
                };

                // Create the configuration object for maps.
                var mapConfigurations = new List<object>();

                var configEntry = new Dictionary<string, object>
                {
                    ["maps"] = mapConfigurations,
                    [ModelConstants.AllowUnrecognizedMessages] = true
                };

                // Add map references from source application and build the configuration.
                foreach (var map in maps)
                {
                    intermediary.MapKeyRefs.Add(map.Key);

                    var sourceSchemaResources = map.FindRelatedResourcesByType(Model, ResourceRelationshipType.ReferencedBy, ModelConstants.ResourceDocumentSchema);

                    // Check there is one source schema.
                    if (sourceSchemaResources == null || sourceSchemaResources.Count == 0)
                    {
                        _logger.LogError(ErrorMessages.UnableToFindFindRelatedResourceByType, map.Key, ResourceRelationshipType.ReferencedBy, ModelConstants.ResourceDocumentSchema);
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceDocumentSchema, ResourceRelationshipType.ReferencedBy, map.Key)));
                    }
                    else if (sourceSchemaResources.Count != 1)
                    {
                        _logger.LogError(ErrorMessages.MapHasTooManySourceSchemas, map.Key, 1, sourceSchemaResources.Count);
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.MapHasTooManySourceSchemas, map.Key)));
                    }
                    else
                    {
                        var sourceSchema = sourceSchemaResources.First();
                        var schema = sourceSchema.SourceObject as Schema;

                        var messageDefinition = schema.MessageDefinitions.FirstOrDefault();

                        mapConfigurations.Add(new { messageType = messageDefinition.MessageType, mapName = $"{targetApplication.Name.FormatKey()}.{map.Name.FormatKey()}" });
                    }
                }

                // Set step name for routing slip configuration
                intermediary.Properties.Add(ModelConstants.ScenarioStepName, "xmlMessageTranslator");

                // Add configuration properties
                intermediary.Properties.Add(ModelConstants.ConfigurationEntry, configEntry);

                // Add routing properties
                intermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());

                // Hook up suspend queue
                intermediary.OutputChannelKeyRefs.Add(suspendQueueChannelKey);
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, suspendQueueChannelKey);
                _logger.LogError(error);
                Context.Errors.Add(new ErrorMessage(error));
            }

            return intermediary;
        }

        /// <summary>
        /// Creates an intermediary representing a routing slip router, which is used to route
        /// from an endpoint or intermediary to the next intermediary in the route.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="fromStep">The step this router will route from.</param>
        /// <param name="toStep">The step this router will route to.</param>
        /// <returns>A routing slip router intermediary.</returns>
        protected Intermediary CreateRoutingSlipRouterIntermediary(string intermediaryKeyPrefix, string fromStep, string toStep)
        {
            _ = intermediaryKeyPrefix ?? throw new ArgumentNullException(nameof(intermediaryKeyPrefix));
            _ = fromStep ?? throw new ArgumentNullException(nameof(fromStep));
            _ = toStep ?? throw new ArgumentNullException(nameof(toStep));

            _logger.LogTrace(TraceMessages.CreatingRoutingSlipRouterIntermediary, _ruleName, fromStep, toStep);

            // Create an intermediary based on the routing slip router
            var intermediary = new RoutingSlipRouter(MigrationTargetResources.RoutingSlipRouterName)
            {
                Description = string.Format(CultureInfo.CurrentCulture, MigrationTargetResources.RoutingSlipRouterDescription, toStep),
                Key = $"{intermediaryKeyPrefix}:{ModelConstants.RoutingSlipRouterLeafKey}:{fromStep.FormatKey()}-{toStep.FormatKey()}",
                Rating = ConversionRating.FullConversion
            };

            return intermediary;
        }

        /// <summary>
        /// Gets properties for the pipeline component for the intermediary object.
        /// </summary>
        /// <remarks>
        /// It copies the properties from the pipeline component unless they have been overridden in the pipeline, in
        /// which case those property values are used instead.
        /// </remarks>
        /// <param name="component">The pipeline component.</param>
        /// <param name="pipelineStages">The pipeline.</param>
        /// <returns>A dictionary of pipeline properties.</returns>
        private IDictionary<string, object> GetPipelineProperties(DocumentStageComponent component, Root pipelineStages)
        {
            var properties = new Dictionary<string, object>();

            // See if there are overridden properties
            var overriddenComponent = pipelineStages?.Stages?.SelectMany(s => s.Components).Where(c => component.Name.Contains(c.Name)).SingleOrDefault();

            // Add properties
            if (component.Properties != null && component.Properties.Any())
            {
                foreach (var componentProperty in component.Properties)
                {
                    var overriddenProperty = overriddenComponent?.Properties?.Where(p => p.Name == componentProperty.Name).SingleOrDefault();
                    if (overriddenProperty != null)
                    {
                        _logger.LogTrace(TraceMessages.UsingOverriddenPropertyValueForPipelineComponent, _ruleName, overriddenProperty.Value, overriddenProperty.Name, component.ComponentName);

                        // Use overridden property in receive location
                        properties.Add(overriddenProperty.Name, overriddenProperty.Value);
                    }
                    else
                    {
                        _logger.LogTrace(TraceMessages.UsingOverriddenPropertyValueForPipelineComponent, _ruleName, componentProperty.Value, componentProperty.Name, component.ComponentName);

                        // Use default pipeline property
                        properties.Add(componentProperty.Name, componentProperty.Value);
                    }
                }
            }

            return properties;
        }

        /// <summary>
        /// Creates one or more intermediaries representing a XML Disassembler pipeline component.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="component">The pipeline component.</param>
        /// <returns>Returns list of intermediaries.</returns>
        private IList<Intermediary> CreateXmlDisassemblerPipelineIntermediaries(string intermediaryKeyPrefix, DocumentStageComponent component, Root pipelineStages)
        {
            var intermediaries = new List<Intermediary>();

            // Find suspend queue channel
            var suspendQueueChannelKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}:{ModelConstants.SuspendQueueLeafKey}";

            var messagingObject = Model.FindMessagingObject(suspendQueueChannelKey);
            if (messagingObject.messagingObject != null)
            {
                _logger.LogDebug(TraceMessages.CreatingXmlMessageProcessorIntermediary, _ruleName, component.ComponentName);

                // Create XML message processor intermediary
                var xmlMessageProcessor = new GenericFilter(component.ComponentName)
                {
                    Component = "Microsoft.BizTalk.Component.XmlDasmComp",
                    Description = MigrationTargetResources.XmlDisassemblerDescription,
                    Key = $"{intermediaryKeyPrefix}:{ModelConstants.XmlMessageProcessorFilterLeafKey}",
                    Rating = ConversionRating.PartialConversion
                };
                var properties = GetPipelineProperties(component, pipelineStages);
                xmlMessageProcessor.ComponentProperties.Merge<string, object>(properties);
                xmlMessageProcessor.Properties.Add(ModelConstants.ScenarioStepName, "xmlMessageProcessor");
                xmlMessageProcessor.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());

                // Support batch handling, which is only for envelope de-batching, but if the XML disassembler is
                // set up to dynamically work out the schema at runtime, we won't know at design time whether a
                // batch will be received.  In this instance, just always add batch handling to the model in the
                // case envelopes are sent.  Of course, the envelope could be explicitly specified and we could
                // work it out in some scenarios, but for the purposes of setting this up in Azure, may as well
                // output the relevant artifacts in conversion to support it in case needed in the future for
                // this scenario.

                // TODO: Change this back to true when envelope debatching is supported by the solution.
                xmlMessageProcessor.Properties.Add(ModelConstants.HandleBatches, false);

                // Get properties
                var allowUnrecognizedMessage = properties.ContainsKey("AllowUnrecognizedMessage") ?
                    Convert.ToBoolean(properties["AllowUnrecognizedMessage"], CultureInfo.CurrentCulture) :
                    false;

                var validateDocument = properties.ContainsKey("ValidateDocument") ?
                    Convert.ToBoolean(properties["ValidateDocument"], CultureInfo.CurrentCulture) :
                    false;

                var recoverableInterchangeProcessing = properties.ContainsKey("RecoverableInterchangeProcessing") ?
                    Convert.ToBoolean(properties["RecoverableInterchangeProcessing"], CultureInfo.CurrentCulture) :
                    false;

                // Build configuration
                var config = new Dictionary<string, object>()
                {
                    { ModelConstants.AllowUnrecognizedMessages, allowUnrecognizedMessage },
                    { ModelConstants.ValidateDocument, validateDocument },
                    { ModelConstants.RecoverableInterchangeProcessing, recoverableInterchangeProcessing }
                };

                // See if document schemas have been explicitly defined
                if (properties.ContainsKey("DocumentSpecNames"))
                {
                    var messageTypes = new List<string>();
                    var docSpecNames = ParseDocSpecNames(properties["DocumentSpecNames"], component);
                    foreach (var docSpecName in docSpecNames)
                    {
                        messageTypes.Add(docSpecName.Value);
                    }

                    if (messageTypes.Any())
                    {
                        config.Add(ModelConstants.MessageTypes, messageTypes);
                    }
                }

                // See if envelope schemas have been explicitly defined
                if (properties.ContainsKey("EnvelopeDocSpecNames"))
                {
                    var messageTypes = new List<string>();
                    var envSpecNames = ParseDocSpecNames(properties["EnvelopeDocSpecNames"], component);
                    foreach (var envSpecName in envSpecNames)
                    {
                        messageTypes.Add(envSpecName.Value);
                    }

                    if (messageTypes.Any())
                    {
                        config.Add(ModelConstants.EnvelopeMessageTypes, messageTypes);
                    }
                }

                // Set configuration
                xmlMessageProcessor.Properties.Add(ModelConstants.ConfigurationEntry, config);

                // Hook up suspend queue
                xmlMessageProcessor.OutputChannelKeyRefs.Add(suspendQueueChannelKey);

                intermediaries.Add(xmlMessageProcessor);

                // If allow unrecognized message is true, route via a XML message filter
                if (allowUnrecognizedMessage)
                {
                    _logger.LogDebug(TraceMessages.CreatingXmlMessageFilterIntermediary, _ruleName, MigrationTargetResources.XmlMessageFilterName);

                    // Create message filter intermediary
                    var xmlMessageFilter = new MessageFilter(MigrationTargetResources.XmlMessageFilterName)
                    {
                        Description = MigrationTargetResources.XmlMessageFilterDescription,
                        Key = $"{intermediaryKeyPrefix}:{ModelConstants.XmlMessageFilterLeafKey}",
                        Rating = ConversionRating.NoAutomaticConversion
                    };

                    xmlMessageFilter.Properties.Add(ModelConstants.ScenarioStepName, "xmlMessageFilter");
                    xmlMessageFilter.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());
                    xmlMessageFilter.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());

                    // Hook up suspend queue
                    xmlMessageFilter.OutputChannelKeyRefs.Add(suspendQueueChannelKey);

                    intermediaries.Add(xmlMessageFilter);
                }

                // If validate document is true, route via a XML validator
                if (validateDocument)
                {
                    intermediaries.Add(CreateXmlValidatorIntermediary(intermediaryKeyPrefix, component, pipelineStages));
                }
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, suspendQueueChannelKey);
                _logger.LogError(error);
                Context.Errors.Add(new ErrorMessage(error));
            }

            return intermediaries;
        }

        /// <summary>
        /// Creates one or more intermediaries representing a XML Assembler pipeline component.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="component">The pipeline component.</param>
        /// <returns>Returns list of intermediaries.</returns>
        private IList<Intermediary> CreateXmlAssemblerPipelineIntermediaries(string intermediaryKeyPrefix, DocumentStageComponent component, Root pipelineStages)
        {
            var intermediaries = new List<Intermediary>();

            // Find suspend queue channel
            var suspendQueueChannelKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}:{ModelConstants.SuspendQueueLeafKey}";

            var messagingObject = Model.FindMessagingObject(suspendQueueChannelKey);
            if (messagingObject.messagingObject != null)
            {
                _logger.LogDebug(TraceMessages.CreatingXmlEnvelopeWrapperIntermediary, _ruleName, component.ComponentName);

                // Create XML envelope wrapper intermediary
                var xmlEnvelopeWrapper = new EnvelopeWrapper(component.ComponentName)
                {
                    Description = MigrationTargetResources.XmlAssemblerDescription,
                    Key = $"{intermediaryKeyPrefix}:{ModelConstants.XmlEnvelopeWrapperFilterLeafKey}",
                    Rating = ConversionRating.NoAutomaticConversion
                };
                xmlEnvelopeWrapper.Properties.Add(ModelConstants.ScenarioStepName, "xmlEnvelopeWrapper");
                xmlEnvelopeWrapper.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());

                // Get properties
                var properties = GetPipelineProperties(component, pipelineStages);

                var addXmlDeclaration = properties.ContainsKey("AddXmlDeclaration") ?
                    Convert.ToBoolean(properties["AddXmlDeclaration"], CultureInfo.CurrentCulture) :
                    true;

                var preserveBom = properties.ContainsKey("PreserveBom") ?
                    Convert.ToBoolean(properties["PreserveBom"], CultureInfo.CurrentCulture) :
                    true;

                var xmlAssemblerProcessingInstructions = properties.ContainsKey("XmlAsmProcessingInstructions") ?
                    properties["XmlAsmProcessingInstructions"] :
                    null;

                var processingInstructionsOptions = properties.ContainsKey("ProcessingInstructionsOptions") ?
                    Convert.ToInt32(properties["ProcessingInstructionsOptions"], CultureInfo.CurrentCulture) :
                    0;

                var processingInstructionsScope = properties.ContainsKey("ProcessingInstructionsScope") ?
                    Convert.ToInt32(properties["ProcessingInstructionsScope"], CultureInfo.CurrentCulture) :
                    0;

                var targetCharset = properties.ContainsKey("TargetCharset") ?
                    properties["TargetCharset"] :
                    null;

                var targetCodePage = properties.ContainsKey("TargetCodePage") ?
                    Convert.ToInt32(properties["TargetCodePage"], CultureInfo.CurrentCulture) :
                    0;

                // This is by default, but can be set using context property XMLNORM.AllowUnrecognizedMessage.  We
                // currently don't parse this from the orchestration, so default to false.
                var allowUnrecognizedMessage = false;

                // Only do if envelope specified and batching is potentially required
                // TODO: Change this back to true when envelope batching is supported by the solution.
                xmlEnvelopeWrapper.Properties.Add(ModelConstants.HandleBatches, false);

                // Build configuration
                var config = new Dictionary<string, object>()
                {
                    { ModelConstants.AddXmlDeclaration, addXmlDeclaration },
                    { ModelConstants.PreserveBom, preserveBom },
                    { ModelConstants.XmlAsmProcessingInstructions, xmlAssemblerProcessingInstructions },
                    { ModelConstants.ProcessingInstructionsOptions, processingInstructionsOptions },
                    { ModelConstants.ProcessingInstructionsScope, processingInstructionsScope },
                    { ModelConstants.TargetCharset, targetCharset },
                    { ModelConstants.TargetCodePage, targetCodePage },
                    { ModelConstants.AllowUnrecognizedMessages, allowUnrecognizedMessage }
                };

                // See if document schemas have been explicitly defined
                if (properties.ContainsKey("DocumentSpecNames"))
                {
                    var messageTypes = new List<string>();
                    var docSpecNames = ParseDocSpecNames(properties["DocumentSpecNames"], component);
                    foreach (var docSpecName in docSpecNames)
                    {
                        messageTypes.Add(docSpecName.Value);
                    }

                    if (messageTypes.Any())
                    {
                        config.Add(ModelConstants.MessageTypes, messageTypes);
                    }
                }

                // See if envelope schemas have been explicitly defined
                if (properties.ContainsKey("EnvelopeDocSpecNames"))
                {
                    var messageTypes = new List<string>();
                    var envSpecNames = ParseDocSpecNames(properties["EnvelopeDocSpecNames"], component);
                    foreach (var envSpecName in envSpecNames)
                    {
                        messageTypes.Add(envSpecName.Value);
                    }

                    if (messageTypes.Any())
                    {
                        config.Add(ModelConstants.EnvelopeMessageTypes, messageTypes);
                    }
                }

                // Set configuration
                xmlEnvelopeWrapper.Properties.Add(ModelConstants.ConfigurationEntry, config);

                // Hook up suspend queue
                xmlEnvelopeWrapper.OutputChannelKeyRefs.Add(suspendQueueChannelKey);

                intermediaries.Add(xmlEnvelopeWrapper);

                // If allow unrecognized message is true, route via a XML message filter
                if (allowUnrecognizedMessage)
                {
                    _logger.LogDebug(TraceMessages.CreatingXmlMessageFilterIntermediary, _ruleName, MigrationTargetResources.XmlMessageFilterName);

                    // Create message filter intermediary
                    var xmlMessageFilter = new MessageFilter(MigrationTargetResources.XmlMessageFilterName)
                    {
                        Description = MigrationTargetResources.XmlMessageFilterDescription,
                        Key = $"{intermediaryKeyPrefix}:{ModelConstants.XmlMessageFilterLeafKey}",
                        Rating = ConversionRating.NoAutomaticConversion
                    };

                    xmlMessageFilter.Properties.Add(ModelConstants.ScenarioStepName, "xmlMessageFilter");
                    xmlMessageFilter.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());
                    xmlMessageFilter.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());

                    // Hook up suspend queue
                    xmlMessageFilter.OutputChannelKeyRefs.Add(suspendQueueChannelKey);

                    intermediaries.Add(xmlMessageFilter);
                }

                _logger.LogDebug(TraceMessages.CreatingContentDemoterIntermediary, _ruleName, MigrationTargetResources.ContentDemoterName);

                // Create content demoter intermediary
                var contentDemoter = new ContentDemoter(MigrationTargetResources.ContentDemoterName)
                {
                    Description = MigrationTargetResources.ContentDemoterDescription,
                    Key = $"{intermediaryKeyPrefix}:{ModelConstants.ContentDemoterLeafKey}",
                    Rating = ConversionRating.NoAutomaticConversion
                };

                contentDemoter.Properties.Add(ModelConstants.ScenarioStepName, "contentDemoter");
                contentDemoter.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());
                contentDemoter.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());

                // Hook up suspend queue
                contentDemoter.OutputChannelKeyRefs.Add(suspendQueueChannelKey);

                intermediaries.Add(contentDemoter);
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, suspendQueueChannelKey);
                _logger.LogError(error);
                Context.Errors.Add(new ErrorMessage(error));
            }

            return intermediaries;
        }

        /// <summary>
        /// Creates an intermediary representing a XML Validator pipeline component.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="component">The pipeline component.</param>
        /// <returns>Returns list of intermediaries.</returns>
        private Intermediary CreateXmlValidatorIntermediary(string intermediaryKeyPrefix, DocumentStageComponent component, Root pipelineStages)
        {
            _logger.LogDebug(TraceMessages.CreatingXmlMessageValidatorIntermediary, _ruleName, MigrationTargetResources.XmlValidatorName);

            GenericFilter xmlValidator = null;

            // Find suspend queue channel
            var suspendQueueChannelKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}:{ModelConstants.SuspendQueueLeafKey}";

            var messagingObject = Model.FindMessagingObject(suspendQueueChannelKey);
            if (messagingObject.messagingObject != null)
            {
                // Create XML validator intermediary
                xmlValidator = new GenericFilter(MigrationTargetResources.XmlValidatorName)
                {
                    Description = MigrationTargetResources.XmlValidatorDescription,
                    Key = $"{intermediaryKeyPrefix}:{ModelConstants.XmlValidatorFilterLeafKey}",
                    Rating = ConversionRating.FullConversion
                };

                xmlValidator.Properties.Add(ModelConstants.ScenarioStepName, "xmlValidator");
                xmlValidator.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());

                // Get pipeline properties and see if document schemas have been explicitly defined
                var config = new Dictionary<string, object>()
                {
                    [ModelConstants.AllowUnrecognizedMessages] = true
                };

                var properties = GetPipelineProperties(component, pipelineStages);
                if (properties.ContainsKey("DocumentSpecName"))
                {
                    var messageTypes = new List<string>();
                    var docSpecNames = ParseDocSpecNames(properties["DocumentSpecName"], component);
                    foreach (var docSpecName in docSpecNames)
                    {
                        messageTypes.Add(docSpecName.Value);
                    }

                    if (messageTypes.Any())
                    {
                        config.Add(ModelConstants.MessageTypes, messageTypes);
                    }
                }

                // Set configuration
                xmlValidator.Properties.Add(ModelConstants.ConfigurationEntry, config);

                // Hook up suspend queue
                xmlValidator.OutputChannelKeyRefs.Add(suspendQueueChannelKey);
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, suspendQueueChannelKey);
                _logger.LogError(error);
                Context.Errors.Add(new ErrorMessage(error));
            }

            return xmlValidator;
        }

        /// <summary>
        /// Parses the provide document spec names which may be pipe delimited into a dictionary of
        /// doc spec names which are fully qualified type names and message types.
        /// </summary>
        /// <param name="docSpecNames">The document spec names which may be pipe delimited.</param>
        /// <param name="component">The pipeline component.</param>
        /// <returns>A dictionary of doc spec names to message types.</returns>
        private IDictionary<string, string> ParseDocSpecNames(object docSpecNames, DocumentStageComponent component)
        {
            var docSpecNameMapping = new Dictionary<string, string>();

            if (docSpecNames != null)
            {
                // Split by pipe delimiter in case there are multiple
                var docSpecNameValues = docSpecNames.ToString().Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (docSpecNameValues != null && docSpecNameValues.Any())
                {
                    var parsedApplicationGroup = Model.GetSourceModel<ParsedBizTalkApplicationGroup>();
                    var schemas = parsedApplicationGroup?.Applications.SelectMany(a => a.Application.Schemas);

                    foreach (var docSpecName in docSpecNameValues)
                    {
                        // Find schema in source
                        var schema = schemas?.Where(s => s.FullName == docSpecName).FirstOrDefault();
                        if (schema != null)
                        {
                            docSpecNameMapping.Add(docSpecName, string.Concat(schema.XmlNamespace, "#", schema.RootNodeName));
                        }
                        else
                        {
                            _logger.LogWarning(WarningMessages.SchemaReferencedByPipelineComponentIsMissing, docSpecName, component.ComponentName);
                        }
                    }
                }
            }

            return docSpecNameMapping;
        }

        /// <summary>
        /// Creates an intermediary representing a JSON decoder pipeline component.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="component">The pipeline component.</param>
        /// <returns>Returns a JSON decoder intermediary.</returns>
        private Intermediary CreateJsonDecoderIntermediary(string intermediaryKeyPrefix, DocumentStageComponent component, Root pipelineStages)
        {
            _logger.LogDebug(TraceMessages.CreatingJsonDecoderIntermediary, _ruleName, component.ComponentName);

            // Create JSON decoder intermediary
            var intermediary = new GenericFilter(component.ComponentName)
            {
                Component = "Microsoft.BizTalk.Component.JsonDecoder",
                Description = MigrationTargetResources.JsonDecoderDescription,
                Key = $"{intermediaryKeyPrefix}:{ModelConstants.JsonDecoderLeafKey}",
                Rating = ConversionRating.FullConversion
            };
            intermediary.Properties.Add(ModelConstants.ScenarioStepName, "jsonDecoder");
            intermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());

            // Get properties
            var properties = GetPipelineProperties(component, pipelineStages);

            var rootNode = properties.ContainsKey("RootNode") ?
                properties["RootNode"] :
                null;

            var rootNodeNamespace = properties.ContainsKey("RootNodeNamespace") ?
                properties["RootNodeNamespace"] :
                null;

            // This property if overridden with true has a value of -1 in the pipeline data field in the binding
            // file but a value of false in the pipeline document XML.
            var addMessageBodyForEmptyMessage = false;
            if (properties.ContainsKey("AddMessageBodyForEmptyMessage"))
            {
                if (bool.TryParse(properties["AddMessageBodyForEmptyMessage"].ToString(), out var boolResult))
                {
                    addMessageBodyForEmptyMessage = boolResult;
                }
                else
                {
                    if (int.TryParse(properties["AddMessageBodyForEmptyMessage"].ToString(), out var numberResult))
                    {
                        addMessageBodyForEmptyMessage = numberResult == -1 ? true : false;
                    }
                }
            }

            // Build configuration
            var config = new Dictionary<string, object>()
            {
                { "rootNode", rootNode },
                { "rootNodeNamespace", rootNodeNamespace },
                { "addMessageBodyForEmptyMessage", addMessageBodyForEmptyMessage },
                { "writeArrayAttribute", true },
                { "encodeSpecialCharacters", false }
            };

            // Set configuration
            intermediary.Properties.Add(ModelConstants.ConfigurationEntry, config);

            return intermediary;
        }

        /// <summary>
        /// Creates an intermediary representing a JSON encoder pipeline component.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="component">The pipeline component.</param>
        /// <returns>Returns a JSON encoder intermediary.</returns>
        private Intermediary CreateJsonEncoderIntermediary(string intermediaryKeyPrefix, DocumentStageComponent component, Root pipelineStages)
        {
            _logger.LogDebug(TraceMessages.CreatingJsonEncoderIntermediary, _ruleName, component.ComponentName);

            // Create JSON encoder intermediary
            var intermediary = new GenericFilter(component.ComponentName)
            {
                Component = "Microsoft.BizTalk.Component.JsonEncoder",
                Description = MigrationTargetResources.JsonEncoderDescription,
                Key = $"{intermediaryKeyPrefix}:{ModelConstants.JsonEncoderLeafKey}",
                Rating = ConversionRating.FullConversion
            };
            intermediary.Properties.Add(ModelConstants.ScenarioStepName, "jsonEncoder");
            intermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());

            // Get properties
            var properties = GetPipelineProperties(component, pipelineStages);

            // This property if overridden with true has a value of -1 in the pipeline data field in the binding
            // file but a value of false in the pipeline document XML.
            var removeOuterEnvelope = false;
            if (properties.ContainsKey("RemoveOuterEnvelope"))
            {
                if (bool.TryParse(properties["RemoveOuterEnvelope"].ToString(), out var boolResult))
                {
                    removeOuterEnvelope = boolResult;
                }
                else
                {
                    if (int.TryParse(properties["RemoveOuterEnvelope"].ToString(), out var numberResult))
                    {
                        removeOuterEnvelope = numberResult == -1 ? true : false;
                    }
                }
            }

            // Build configuration
            var config = new Dictionary<string, object>()
            {
                { "removeOuterEnvelope", removeOuterEnvelope }
            };

            // Set configuration
            intermediary.Properties.Add(ModelConstants.ConfigurationEntry, config);

            return intermediary;
        }
    }
}
