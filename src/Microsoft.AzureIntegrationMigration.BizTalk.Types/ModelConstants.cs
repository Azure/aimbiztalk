// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types
{
    /// <summary>
    /// Defines a list of type strings for the application model.
    /// </summary>
    public static class ModelConstants
    {
        #region Application Prefix

        /// <summary>
        /// Defines the prefix for Microsoft BizTalk specific types in the model.
        /// </summary>
        public const string ApplicationPrefix = "microsoft.biztalk.";

        #endregion

        #region Arguments

        /// <summary>
        /// Defines the prefix for shared BizTalk arguments.
        /// </summary>
        public const string CoreArgsPrefix = ApplicationPrefix + "args.core.";

        #endregion

        #region Resource Containers

        /// <summary>
        /// Defines the prefix for resource containers.
        /// </summary>
        public const string ResourceContainerPrefix = ApplicationPrefix + "resourcecontainer.";

        /// <summary>
        /// Defines an MSI resource container.
        /// </summary>
        public const string ResourceContainerMsi = ResourceContainerPrefix + "msi";

        /// <summary>
        /// Defines an assembly resource container.
        /// </summary>
        public const string ResourceContainerAssembly = ResourceContainerPrefix + "assembly";

        /// <summary>
        /// Defines a cabinet file (CAB) resource container.
        /// </summary>
        public const string ResourceContainerCab = ResourceContainerPrefix + "cab";

        #endregion

        #region Resource Definitions

        /// <summary>
        /// Defines the prefix for resource definitions.
        /// </summary>
        public const string ResourceDefinitionPrefix = ApplicationPrefix + "resourcedefinition.";

        /// <summary>
        /// Defines a file resource definition.
        /// </summary>
        public const string ResourceDefinitionFile = ResourceDefinitionPrefix + "file";

        /// <summary>
        /// Defines a schema resource definition.
        /// </summary>
        public const string ResourceDefinitionSchema = ResourceDefinitionPrefix + "schema";

        /// <summary>
        /// Defines a map resource definition.
        /// </summary>
        public const string ResourceDefinitionMap = ResourceDefinitionPrefix + "map";

        /// <summary>
        /// Defines a receive pipeline resource definition.
        /// </summary>
        public const string ResourceDefinitionReceivePipeline = ResourceDefinitionPrefix + "receivepipeline";

        /// <summary>
        /// Defines a send pipeline resource definition.
        /// </summary>
        public const string ResourceDefinitionSendPipeline = ResourceDefinitionPrefix + "sendpipeline";

        /// <summary>
        /// Defines a orchestration resource definition.
        /// </summary>
        public const string ResourceDefinitionOrchestration = ResourceDefinitionPrefix + "orchestration";

        /// <summary>
        /// Defines an application definition file resource definition.
        /// </summary>
        public const string ResourceDefinitionApplicationDefinition = ResourceDefinitionPrefix + "applicationdefinition";

        /// <summary>
        /// Defines a binding file resource definition.
        /// </summary>
        public const string ResourceDefinitionBindings = ResourceDefinitionPrefix + "bindings";

        #endregion

        #region Resources

        /// <summary>
        /// Defines the prefix for resources.
        /// </summary>
        public const string ResourcePrefix = ApplicationPrefix + "resource.";

        /// <summary>
        /// Defines a BizTalk application resource.
        /// </summary>
        public const string ResourceApplication = ResourcePrefix + "application";

        /// <summary>
        /// Defines a correlation declaration resource.
        /// </summary>
        public const string ResourceCorrelationDeclaration = ResourcePrefix + "correlationdeclaration";

        /// <summary>
        /// Defines a context property resource.
        /// </summary>
        public const string ResourceContextProperty = ResourcePrefix + "contextproperty";

        /// <summary>
        /// Defines a correlation type resource.
        /// </summary>
        public const string ResourceCorrelationType = ResourcePrefix + "correlationtype";

        /// <summary>
        /// Defines distribution list resource.
        /// </summary>
        public const string ResourceDistributionList = ResourcePrefix + "distributionlist";

        /// <summary>
        /// Defines a document schema resource.
        /// </summary>
        public const string ResourceDocumentSchema = ResourcePrefix + "documentschema";

        /// <summary>
        /// Defines a filter expression send port.
        /// </summary>
        public const string ResourceFilterExpression = ResourcePrefix + "filterexpression";

        /// <summary>
        /// Defines a transform resource.
        /// </summary>
        public const string ResourceMap = ResourcePrefix + "map";

        /// <summary>
        /// Defines a message declaration resource.
        /// </summary>
        public const string ResourceMessageDeclaration = ResourcePrefix + "messagedeclaration";

        /// <summary>
        /// Defines a message type resource.
        /// </summary>
        public const string ResourceMessageType = ResourcePrefix + "messagetype";

        /// <summary>
        /// Defines an orchestration metamodel resource.
        /// </summary>
        public const string ResourceMetaModel = ResourcePrefix + "metamodel";

        /// <summary>
        /// Defines a multipart message type resource.
        /// </summary>
        public const string ResourceMultipartMessageType = ResourcePrefix + "multipartmessagetype";

        /// <summary>
        /// Defines an orchestration module resource.
        /// </summary>
        public const string ResourceModule = ResourcePrefix + "module";

        /// <summary>
        /// Defines a pipeline component resource.
        /// </summary>
        public const string ResourcePipelineComponent = ResourcePrefix + "pipelinecomponent";

        /// <summary>
        /// Defines a port declaration resource.
        /// </summary>
        public const string ResourcePortDeclaration = ResourcePrefix + "portdeclaration";

        /// <summary>
        /// Defines a port type resource.
        /// </summary>
        public const string ResourcePortType = ResourcePrefix + "porttype";

        /// <summary>
        /// Defines a property schema resource.
        /// </summary>
        public const string ResourcePropertySchema = ResourcePrefix + "propertyschema";

        /// <summary>
        /// Defines a receive location resource.
        /// </summary>
        public const string ResourceReceiveLocation = ResourcePrefix + "receivelocation";

        /// <summary>
        /// Defines a receive pipeline resource.
        /// </summary>
        public const string ResourceReceivePipeline = ResourcePrefix + "receivepipeline";

        /// <summary>
        /// Defines a receive port resource.
        /// </summary>
        public const string ResourceReceivePort = ResourcePrefix + "receiveport";

        /// <summary>
        /// Defines a send port resource.
        /// </summary>
        public const string ResourceSendPort = ResourcePrefix + "sendport";

        /// <summary>
        /// Defines a service resource.
        /// </summary>
        public const string ResourceServiceBinding = ResourcePrefix + "servicebinding";

        /// <summary>
        /// Defines a service link type resource.
        /// </summary>
        public const string ResourceServiceLinkType = ResourcePrefix + "servicelinktype";

        /// <summary>
        /// Defines a send pipeline resource.
        /// </summary>
        public const string ResourceSendPipeline = ResourcePrefix + "sendpipeline";

        /// <summary>
        /// Defines a service resource.
        /// </summary>
        public const string ResourceServiceDeclaration = ResourcePrefix + "servicedeclaration";

        #endregion

        #region Reports

        /// <summary>
        /// Defines the prefix for reports.
        /// </summary>
        public const string ReportPrefix = ApplicationPrefix + "report";

        /// <summary>
        /// Defines the prefix for biztalk reports.
        /// </summary>
        public const string ReportBizTalk = ReportPrefix + "biztalk";

        #endregion

        #region Numeric Constants

        /// <summary>
        /// The maximum conversion rating as specified by the <see cref="ConversionRating"/> enum.
        /// </summary>
        public const int ConversionRatingMaximum = 5;

        /// <summary>
        /// The minimum conversion rating as specified by the <see cref="ConversionRating"/> enum.
        /// </summary>
        public const int ConversionRatingMinimum = 1;

        #endregion

        #region Migration Target Leaf Key

        /// <summary>
        /// The leaf key used in the migration target for a content promoter.
        /// </summary>
        public const string ContentPromoterLeafKey = "ContentPromoter";

        /// <summary>
        /// The leaf key used in the migration target for a content demoter.
        /// </summary>
        public const string ContentDemoterLeafKey = "ContentDemoter";

        /// <summary>
        /// The leaf key used in the migration target for a content based router.
        /// </summary>
        public const string ContentBasedRouterLeafKey = "ContentBasedRouter";

        /// <summary>
        /// The leaf key used in the migration target for a message publisher.
        /// </summary>
        public const string MessagePublisherLeafKey = "MessagePublisher";

        /// <summary>
        /// The leaf key used in the migration target for a message subscriber.
        /// </summary>
        public const string MessageSubscriberLeafKey = "MessageSubscriber";

        /// <summary>
        /// The leaf key used in the migration target for a message translator.
        /// </summary>
        public const string XmlMessageTranslatorLeafKey = "XmlMessageTranslator";

        /// <summary>
        /// The leaf key used in the migration target for a routing slip router.
        /// </summary>
        public const string RoutingSlipRouterLeafKey = "RoutingSlipRouter";

        /// <summary>
        /// The leaf key used in the migration target for a generic filter.
        /// </summary>
        public const string GenericFilterLeafKey = "GenericFilter";

        /// <summary>
        /// The leaf key used in the migration target for an adapter endpoint.
        /// </summary>
        public const string AdapterEndpointLeafKey = "AdapterEndpoint";

        /// <summary>
        /// The leaf key used in the migration target for a message box.
        /// </summary>
        public const string MessageBoxLeafKey = "MessageBox";

        /// <summary>
        /// The leaf key used in the migration target for a message box response.
        /// </summary>
        public const string MessageBoxResponseLeafKey = "MessageBoxResponse";

        /// <summary>
        /// The leaf key used in the migration target for a suspend queue.
        /// </summary>
        public const string SuspendQueueLeafKey = "SuspendQueue";

        /// <summary>
        /// The leaf key used in the migration target for an interchange queue.
        /// </summary>
        public const string InterchangeQueueLeafKey = "InterchangeQueue";

        /// <summary>
        /// The leaf used in the migration target for a message bus.
        /// </summary>
        public const string MessageBusLeafKey = "MessageBus";

        /// <summary>
        /// The leaf key used in the migration target for a trigger channel.
        /// </summary>
        public const string TriggerChannelLeafKey = "TriggerChannel";

        /// <summary>
        /// The leaf used in the migration target key for a system application.
        /// </summary>
        public const string SystemApplicationLeafKey = "SystemApplication";

        /// <summary>
        /// The leaf used in the migration target key for a message filter.
        /// </summary>
        public const string XmlMessageFilterLeafKey = "XmlMessageFilter";

        /// <summary>
        /// The leaf used in the migration target key for a message validator.
        /// </summary>
        public const string XmlValidatorFilterLeafKey = "XmlValidator";

        /// <summary>
        /// The leaf used in the migration target key for a message processor.
        /// </summary>
        public const string XmlMessageProcessorFilterLeafKey = "XmlMessageProcessor";

        /// <summary>
        /// The leaf used in the migration target key for an envelope wrapper.
        /// </summary>
        public const string XmlEnvelopeWrapperFilterLeafKey = "XmlEnvelopeWrapper";

        /// <summary>
        /// The leaf key used in the migration target for an interchange aggregator.
        /// </summary>
        public const string InterchangeAggregatorLeafKey = "InterchangeAggregator";

        /// <summary>
        /// The leaf key used in the migration target for an interchange splitter.
        /// </summary>
        public const string InterchangeSplitterLeafKey = "InterchangeSplitter";

        /// <summary>
        /// The leaf key used in the migration target for a JSON decoder.
        /// </summary>
        public const string JsonDecoderLeafKey = "JsonDecoder";

        /// <summary>
        /// The leaf key used in the migration target for a JSON encoder.
        /// </summary>
        public const string JsonEncoderLeafKey = "JsonEncoder";

        #endregion

        #region Messaging Object Properties

        /// <summary>
        /// Defines the name of the scenario for a specific route.
        /// </summary>
        public const string ScenarioName = "scenario";

        /// <summary>
        /// Defines the name of a step in a scenario.
        /// </summary>
        public const string ScenarioStepName = "scenarioStep";

        /// <summary>
        /// Defines the name of the configuration property in a messaging object.
        /// </summary>
        public const string ConfigurationEntry = "configuration";

        /// <summary>
        /// Defines the name of the routing properties property in a messaging object.
        /// </summary>
        public const string RoutingProperties = "routingProperties";

        /// <summary>
        /// Defines the name of the message properties property in a messaging object.
        /// </summary>
        public const string MessageProperties = "messageProperties";

        /// <summary>
        /// Defines the name of the request message properties property in a messaging object.
        /// </summary>
        public const string RequestMessageProperties = "requestMessageProperties";

        /// <summary>
        /// Defines the key of the property which holds the message type in a messaging object.
        /// </summary>
        public const string MessageType = "messageType";

        /// <summary>
        /// Defines the key of the property which holds a collection of message types in a messaging object.
        /// </summary>
        public const string MessageTypes = "messageTypes";

        /// <summary>
        /// Defines the key of the property which holds a collection of message types of envelopes in a messaging object.
        /// </summary>
        public const string EnvelopeMessageTypes = "envelopeMessageTypes";

        /// <summary>
        /// Defines the key of the property which holds a collection of message transforms in a messaging object.
        /// </summary>
        public const string MessageTransforms = "messageTransforms";

        /// <summary>
        /// Defines the name of the handle batches property in a messaging object.
        /// </summary>
        public const string HandleBatches = "handleBatches";

        /// <summary>
        /// Defines the name of the route label property in a messaging object.
        /// </summary>
        public const string RouteLabel = "routeLabel";

        /// <summary>
        /// Defines the name of the source schema message types property in a transform object.
        /// </summary>
        public const string SourceSchemaMessageTypes = "sourceSchemaMessageTypes";

        /// <summary>
        /// Defines the name of the target schema message types property in a transform object.
        /// </summary>
        public const string TargetSchemaMessageTypes = "targetSchemaMessageTypes";

        /// <summary>
        /// Defines the full type name of a source object artifact.
        /// </summary>
        public const string TypeName = "typeName";

        /// <summary>
        /// Defines the key of the property which holds the message ID.
        /// </summary>
        public const string MessageId = "messageId";

        /// <summary>
        /// Defines the key of the property which holds the correlation ID, which is a copy of the request message ID.
        /// </summary>
        public const string CorrelationId = "correlationId";

        /// <summary>
        /// Defines the key of the property which holds a topic name.
        /// </summary>
        public const string TopicName = "topicName";

        /// <summary>
        /// Defines the key of the property which holds a flag indicating whether the endpoint should support failed message routing.
        /// </summary>
        public const string FailedMessageRouting = "failedMessageRouting";

        /// <summary>
        /// Defines the key of the property which holds the response subscription.
        /// </summary>
        public const string ResponseSubscription = "responseSubscription";

        /// <summary>
        /// Defines the key of the property which holds the response timeout name.
        /// </summary>
        public const string ResponseTimeout = "responseTimeoutInMinutes";

        /// <summary>
        /// Defines the key of the property which holds a flag indicating a two way endpoint or not.
        /// </summary>
        public const string IsTwoWay = "isTwoWay";

        /// <summary>
        /// Defines the key of the property which holds a flag indicating whether to use sessions or not.
        /// </summary>
        public const string UseSessions = "useSessions";

        /// <summary>
        /// Defines the key of the property which holds the name of the session property.
        /// </summary>
        public const string SessionPropertyName = "sessionPropertyName";

        /// <summary>
        /// Defines the key of the source application related to the target application.
        /// </summary>
        public const string SourceApplicationResourceKey = "sourceApplicationResourceKey";

        /// <summary>
        /// Defines the key of the property for a flag that allows unrecognized messages.
        /// </summary>
        public const string AllowUnrecognizedMessages = "allowUnrecognizedMessages";

        /// <summary>
        /// Defines the key of the property for a flag that validates document.
        /// </summary>
        public const string ValidateDocument = "validateDocument";

        /// <summary>
        /// Defines the key of the property for a flag that supports recoverable interchange processing.
        /// </summary>
        public const string RecoverableInterchangeProcessing = "recoverableInterchangeProcessing";

        /// <summary>
        /// Defines the key of the property for a flag that adds XML declaration.
        /// </summary>
        public const string AddXmlDeclaration = "addXmlDeclaration";

        /// <summary>
        /// Defines the key of the property for a flag that preserves the BOM.
        /// </summary>
        public const string PreserveBom = "preserveBom";

        /// <summary>
        /// Defines the key of the property for XML assembly processing instructions.
        /// </summary>
        public const string XmlAsmProcessingInstructions = "xmlAsmProcessingInstructions";

        /// <summary>
        /// Defines the key of the property for processing instructions options.
        /// </summary>
        public const string ProcessingInstructionsOptions = "processingInstructionsOptions";

        /// <summary>
        /// Defines the key of the property for processing instructions scope.
        /// </summary>
        public const string ProcessingInstructionsScope = "processingInstructionsScope";

        /// <summary>
        /// Defines the key of the property for target character set.
        /// </summary>
        public const string TargetCharset = "targetCharset";

        /// <summary>
        /// Defines the key of the property for target code page.
        /// </summary>
        public const string TargetCodePage = "targetCodePage";

        #region BizTalk Specific Properties

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkAckFailureCategory = "btsAckFailureCategory";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkAckFailureCode = "btsAckFailureCode";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkAckId = "btsAckId";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkAckInboundTransportLocation = "btsAckInboundTransportLocation";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkAckOutboundTransportLocation = "btsAckOutboundTransportLocation";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkAckOwnerId = "btsAckOwnerId";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkAckReceivePortId = "btsAckReceivePortId";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkAckReceivePortName = "btsAckReceivePortName";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkAckSendPortId = "btsAckSendPortId";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkAckSendPortName = "btsAckSendPortName";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkAckType = "btsAckType";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkActionOnFailure = "btsActionOnFailure";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkCorrelationToken = "btsCorrelationToken";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkInboundTransportLocation = "btsInboundTransportLocation";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkInboundTransportType = "btsInboundTransportType";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkInterchangeSequenceNumber = "btsInterchangeSequenceNumber";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkIsDynamicSend = "btsIsDynamicSend";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkMessageDestination = "btsMessageDestination";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkMessageType = "btsMessageType";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkOutboundTransportLocation = "btsOutboundTransportLocation";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkOutboundTransportType = "btsOutboundTransportType";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkPropertiesToUpdate = "btsPropertiesToUpdate";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkReceivePipelineId = "btsReceivePipelineId";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkReceivePortId = "btsReceivePortId";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkReceivePortName = "btsReceivePortName";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkSpGroupId = "btsSpGroupId";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkSpId = "btsSpId";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkSpName = "btsSpName";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkSpTransportBackupId = "btsSpTransportBackupId";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkSpTransportId = "btsSpTransportId";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkSuspendAsNonResumable = "btsSuspendAsNonResumable";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkSuspendMessageOnRoutingFailure = "btsSuspendMessageOnRoutingFailure";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkAckDescription = "btsAckDescription";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkInterchangeId = "btsInterchangeId";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkLoopback = "btsLoopback";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkSignatureCertificate = "btsSignatureCertificate";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkSourcePartyId = "btsSourcePartyId";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkSsoTicket = "btsSsoTicket";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkWindowsUser = "btsWindowsUser";

        /// <summary>
        /// Defines a BizTalk context property.
        /// </summary>
        public const string BizTalkOperation = "btsOperation";

        #endregion

        /// <summary>
        /// Defines the name for the Azure APIM trigger channel.
        /// </summary>
        public const string TriggerChannelAzureApim = "microsoft.channels.trigger.azure-apim";

        #endregion

        #region Resource Types

        /// <summary>
        /// Defines the prefix for microsoft resources.
        /// </summary>
        public const string ResourceTypeMicrosoftPrefix = "microsoft";

        /// <summary>
        /// Defines an Azure logic app resource.
        /// </summary>
        public const string ResourceTypeAzureLogicApp = ResourceTypeWorkflows + ".azurelogicapp";

        /// <summary>
        /// Defines an Azure consumption logic app resource.
        /// </summary>
        public const string ResourceTypeAzureLogicAppConsumption = ResourceTypeAzureLogicApp + ".consumption";

        /// <summary>
        /// Defines an Azure standard logic app resource.
        /// </summary>
        public const string ResourceTypeAzureLogicAppStandard = ResourceTypeAzureLogicApp + ".standard";

        /// <summary>
        /// Defines a configuration resources.
        /// </summary>
        public const string ResourceTypeConfiguration = ResourceTypeMicrosoftPrefix + ".configuration";

        /// <summary>
        /// Defines a configuration entry resource.
        /// </summary>
        public const string ResourceTypeConfigurationEntry = ResourceTypeConfiguration + ".configurationentry";

        /// <summary>
        /// Defines a powershell resource.
        /// </summary>
        public const string ResourceTypePowerShell = ResourceTypeScripts + ".powershell";

        /// <summary>
        /// Defines a routing properties resource.
        /// </summary>
        public const string ResourceTypeRoutingProperties = ResourceTypeConfiguration + ".routingproperties";

        /// <summary>
        /// Defines a routing slip resource.
        /// </summary>
        public const string ResourceTypeRoutingSlip= ResourceTypeConfiguration + ".routingslip";

        /// <summary>
        /// Defines a schema resource.
        /// </summary>
        public const string ResourceTypeSchemas = ResourceTypeMicrosoftPrefix + ".schemas";

        /// <summary>
        /// Defines a scripts resource.
        /// </summary>
        public const string ResourceTypeScripts = ResourceTypeMicrosoftPrefix + ".scripts";

        /// <summary>
        /// Defines a workflow resource.
        /// </summary>
        public const string ResourceTypeWorkflows = ResourceTypeMicrosoftPrefix + ".workflows";

        /// <summary>
        /// Defines an xml resource.
        /// </summary>
        public const string ResourceTypeXml = ResourceTypeSchemas + ".xml";

        /// <summary>
        /// Defines an xslt resource.
        /// </summary>
        public const string ResourceTypeXslt = ResourceTypeSchemas + ".xslt";

        /// <summary>
        /// Defines a workflow definition resource.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowDefinition = ".workflowdefinition";

        /// <summary>
        /// Defines a workflow parameters definition resource.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowParametersDefinition = ".parametersdefinition";

        /// <summary>
        /// Defines a workflow parameter resource.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowParameter = ".parameter";

        /// <summary>
        /// Defines a workflow property resource.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowProperty = ".property";

        /// <summary>
        /// Defines a workflow channel resource.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowChannel = ".channel";

        /// <summary>
        /// Defines a workflow variable resource.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowVariable = ".variable";

        /// <summary>
        /// Defines a workflow message resource.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowMessage = ".message";

        /// <summary>
        /// Defines a workflow activity container.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowActivityContainer = ".activitycontainer";

        /// <summary>
        /// Defines a workflow activity.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowActivity = ".activity";

        /// <summary>
        /// Defines a workflow pre-built action.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowAction = ".action";

        /// <summary>
        /// Defines a workflow placeholder activity container used when an activity container doesn't have a specific snippet.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowActivityContainerPlaceHolder = ResourceTypeSuffixWorkflowActivityContainer + ".placeholder";

        /// <summary>
        /// Defines a workflow placeholder activity used when an activity doesn't have a specific snippet.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowActivityPlaceHolder = ResourceTypeSuffixWorkflowActivity + ".placeholder";

        /// <summary>
        /// Defines a workflow placeholder variable used for arbitrary variables defined in the workflow model.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowVariablePlaceHolder = ResourceTypeSuffixWorkflowVariable + ".placeholder";

        /// <summary>
        /// Defines a workflow placeholder message used for arbitrary messages defined in the workflow model.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowMessagePlaceHolder = ResourceTypeSuffixWorkflowMessage + ".placeholder";

        /// <summary>
        /// Defines a pre-built action to get configuration from configuration store for the step.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowActionGetConfiguration = ResourceTypeSuffixWorkflowAction + ".getconfiguration";

        /// <summary>
        /// Defines a pre-built action to call the routing slip router when invoking the next step in the route.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowActionCallRoutingSlipRouter = ResourceTypeSuffixWorkflowAction + ".callroutingsliprouter";

        /// <summary>
        /// Defines a trigger channel.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowChannelTrigger = ResourceTypeSuffixWorkflowChannel + ".trigger";

        /// <summary>
        /// Defines a receive channel.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowChannelReceive = ResourceTypeSuffixWorkflowChannel + ".receive";

        /// <summary>
        /// Defines a send channel.
        /// </summary>
        public const string ResourceTypeSuffixWorkflowChannelSend = ResourceTypeSuffixWorkflowChannel + ".send";

        #endregion

        #region Resource Template Params

        /// <summary>
        /// Defines the resource template parameter for an Azure resource group name.
        /// </summary>
        public const string ResourceTemplateParameterAzureResourceGroupName = "azure_resource_group_name";

        /// <summary>
        /// Defines the resource template parameter for an Azure logic app name.
        /// </summary>
        public const string ResourceTemplateParameterAzureLogicAppName = "azure_logic_app_name";

        /// <summary>
        /// Defines the prefix used on resource template parameters for global config.
        /// </summary>
        public const string ResourceTemplateParamterGlobalConfigPrefix = "global_config_";

        /// <summary>
        /// Defines the resource template parameter for a scenario step name.
        /// </summary>
        public const string ResourceTemplateParameterScenarioStepName = "scenario_step_name";

        #endregion
    }
}
