// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules
{
    /// <summary>
    /// Implements rule AP005, which analyzes the orchestration source model and creates the message scenarios in the target model.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The message scenarios represent the receive and send ports that messages flow through to and from the
    /// message box, both one way and two way.  This also includes orchestrations that subscribe to messages
    /// from the message box and then send messages back to the message box, to be routed onwards.
    /// </para>
    /// <para>
    /// The flow is represented in the target model as a set of connected messaging building blocks of endpoints,
    /// intermediaries and channels.  By default, this analyzer will assume that the flow uses routing slips and
    /// therefore will take into account that the channels between intermediaries will be represented by the
    /// channels used by the routing slip router pattern implementation in the associated AIM Azure repository.
    /// </para>
    /// </remarks>    
    public sealed class AP005OrchestrationScenarioAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "AP005";

        /// <summary>
        /// Defines a list of orchestration metamodel properties that are ignored when parsing.
        /// </summary>
        private readonly static IList<string> s_propertyIgnoreList = new List<string>()
        {
            MetaModelConstants.PropertyKeyIsGhostBranch,
            MetaModelConstants.PropertyKeyIsWebPort,
            MetaModelConstants.PropertyKeyPortModifier,
            MetaModelConstants.PropertyKeyName,
            MetaModelConstants.PropertyKeyOrientation,
            MetaModelConstants.PropertyKeyParamDirection,
            MetaModelConstants.PropertyKeyPortIndex,
            MetaModelConstants.PropertyKeyReportToAnalyst,
            MetaModelConstants.PropertyKeySignal,
            MetaModelConstants.PropertyKeyUseDefaultConstructor,
            MetaModelConstants.PropertyKeyInitialValue
        };

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines a set of bind handlers for binding channels.
        /// </summary>
        private readonly Dictionary<string, BindActivityHandler> _bindChannelHandlers;

        /// <summary>
        /// Creates a new instance of a <see cref="AP005OrchestrationScenarioAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public AP005OrchestrationScenarioAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(AP005OrchestrationScenarioAnalyzer), model, context, logger)
        {
            // Validate and set the member
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Set up bind channel handlers
            _bindChannelHandlers = new Dictionary<string, BindActivityHandler>()
            {
                { WorkflowModelConstants.ActivityTypeReceive, HandleReceiveActivity },
                { WorkflowModelConstants.ActivityTypeSend, HandleSendActivity },
                { WorkflowModelConstants.ActivityTypeInvokeWorkflow, HandleInvokeWorkflowActivity },
                { WorkflowModelConstants.ActivityTypeSuspend, HandleSuspendActivity },
                { WorkflowModelConstants.ActivityTypeTerminate, HandleTerminateActivity }
            };
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
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(AP005OrchestrationScenarioAnalyzer));

                foreach (var application in applications)
                {
                    // Find target application
                    var targetApplication = Model.MigrationTarget.MessageBus.Applications.Where(a => a.Name == application.Name).SingleOrDefault();
                    if (targetApplication != null)
                    {
                        // Find all activatable orchestrations and build messaging scenarios
                        AnalyzeOrchestrations(application, targetApplication);
                    }
                    else
                    {
                        _logger.LogError(ErrorMessages.UnableToFindMessagingObjectWithNameInTargetModel, MessagingObjectType.Application, application.Name);
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithNameInTargetModel, MessagingObjectType.Application, application.Name)));
                    }
                }

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(AP005OrchestrationScenarioAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(AP005OrchestrationScenarioAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Analyze the orchestrations and build the scenario in the target model.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Orchestrations can be activated from the message box using a subscription, which are reflected in the type of
        /// port binding that the orchestration port receive messages on.  This is used to create a routing scenario.
        /// </para>
        /// <para>
        /// Called and started orchestrations are treated as part of the same scenario as an 'activatable' orchestration,
        /// that is, they are not considered the start point of a scenario.
        /// </para>
        /// <param name="sourceApplication">The application to parse for receive ports.</param>
        /// <param name="targetApplication">The application in the target.</param>
        private void AnalyzeOrchestrations(ResourceItem sourceApplication, Application targetApplication)
        {
            _logger.LogDebug(TraceMessages.AnalyzingOrchestrationScenarios, RuleName, sourceApplication.Name);

            var scenarios = 0;

            // Find orchestrations in application
            var orchestrations = sourceApplication.FindRelatedResourcesByType(Model, ResourceRelationshipType.Child, ModelConstants.ResourceMetaModel);
            foreach (var orchestration in orchestrations)
            {
                if (orchestration.SourceObject != null)
                {
                    // Build workflow model
                    var workflowModel = BuildWorkflowModel(orchestration);
                    if (workflowModel != null)
                    {
                        // Bind workflow model
                        BindWorkflowModel(sourceApplication, targetApplication, orchestration, workflowModel);

                        // Create process manager
                        var processManager = CreateProcessManager(targetApplication, workflowModel);
                        targetApplication.Intermediaries.Add(processManager);

                        scenarios++;
                    }
                }
                else
                {
                    _logger.LogError(ErrorMessages.SourceObjectNotFound, orchestration.Key, ModelConstants.ResourceMetaModel);
                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.SourceObjectNotFound, orchestration.Key, ModelConstants.ResourceMetaModel)));
                }
            }

            if (scenarios > 0)
            {
                _logger.LogDebug(TraceMessages.FoundOrchestrationScenariosInApplication, RuleName, scenarios, sourceApplication.Name);
            }
            else
            {
                _logger.LogDebug(TraceMessages.NoOrchestrationsFoundInApplication, RuleName, sourceApplication.Name);
            }

            // Build routes
            var activatableProcessManagers = targetApplication.Intermediaries.Where(i => i is ProcessManager p && p.Activator).Select(i => (ProcessManager)i).ToList();
            if (activatableProcessManagers != null && activatableProcessManagers.Any())
            {
                // Bind invoked workflows, which may add workflow trigger channels to invoked workflows
                foreach (var activatableProcessManager in activatableProcessManagers)
                {
                    BindInvokedWorkflows(activatableProcessManager.WorkflowModel, activatableProcessManager.WorkflowModel);
                }

                // Build the route as a separate pass because some of the workflow trigger channels may not exist if the
                // previous step is done in the same foreach loop as building the route.
                foreach (var activatableProcessManager in activatableProcessManagers)
                {
                    BuildRoute(targetApplication, activatableProcessManager);
                }
            }
            else
            {
                _logger.LogDebug(TraceMessages.NoActivatableProcessManagersInApplication, RuleName, targetApplication.Name);
            }
        }

        /// <summary>
        /// Builds a workflow model to attach to a process manager intermediary.
        /// </summary>
        /// <param name="metaModel">The orchestration metamodel.</param>
        /// <returns>A workflow model.</returns>
        private WorkflowDefinition BuildWorkflowModel(ResourceItem orchestration)
        {
            _logger.LogDebug(TraceMessages.BuildingWorkflowModel, RuleName, orchestration.Name);

            var model = new WorkflowDefinition();
            var metaModel = (MetaModel)orchestration.SourceObject;

            if (metaModel.Element != null && metaModel.Element.Any())
            {
                for (var i = 0; i < metaModel.Element.Length; i++)
                {
                    ParseMetaModelElement(metaModel, model, model, null, metaModel.Element[i], i);
                }
            }

            return model;
        }

        /// <summary>
        /// Parses a metamodel element.
        /// </summary>
        /// <param name="metaModel">The metamodel.</param>
        /// <param name="model">The workflow model.</param>
        /// <param name="parentObject">The parent object in the workflow model of the current element.</param>
        /// <param name="parentElement">The parent object in the metamodel of the current element.</param>
        /// <param name="element">The current element.</param>
        /// <param name="elementIndex">The incrementing index used for generating unique keys.</param>
        private void ParseMetaModelElement(MetaModel metaModel, WorkflowDefinition model, WorkflowObject parentObject, Element parentElement, Element element, int elementIndex)
        {
            WorkflowObject currentObject;
            var childElements = element.Element1?.ToArray();

            // Handle element based on its type
            switch (element.Type)
            {
                case MetaModelConstants.ElementTypeModule:
                    currentObject = HandleModule(parentObject);
                    break;

                case MetaModelConstants.ElementTypeServiceBody:
                    currentObject = HandleServiceBody(model, parentObject, element);
                    break;

                case MetaModelConstants.ElementTypeServiceDeclaration:
                    currentObject = HandleServiceDeclaration(model, parentObject, parentElement, element);
                    break;

                case MetaModelConstants.ElementTypeMessageDeclaration:
                    currentObject = HandleMessageDeclaration(model, parentObject, element);
                    childElements = null;
                    break;

                case MetaModelConstants.ElementTypeVariableDeclaration:
                    currentObject = HandleVariableDeclaration(model, parentObject, element);
                    childElements = null;
                    break;

                case MetaModelConstants.ElementTypePortDeclaration:
                    currentObject = HandlePortDeclaration(model, parentObject, element);
                    childElements = null;
                    break;

                case MetaModelConstants.ElementTypeServiceLinkDeclaration:
                    currentObject = HandleServiceLinkDeclaration(model, parentObject, element);
                    childElements = null;
                    break;

                case MetaModelConstants.ElementTypeCorrelationDeclaration:
                    currentObject = HandleCorrelationDeclaration(model, parentObject, element, elementIndex);
                    childElements = null;
                    break;

                case MetaModelConstants.ElementTypeCall:
                    currentObject = HandleCall(parentObject, element, elementIndex);
                    childElements = null;
                    break;

                case MetaModelConstants.ElementTypeExec:
                    currentObject = HandleExec(parentObject, element, elementIndex);
                    childElements = null;
                    break;

                case MetaModelConstants.ElementTypeConstruct:
                    currentObject = HandleConstruct(parentObject, element, elementIndex);
                    childElements = null;
                    break;

                case MetaModelConstants.ElementTypeTask:
                    currentObject = HandleTask(parentObject, element, elementIndex);
                    break;

                case MetaModelConstants.ElementTypeVariableAssignment:
                    currentObject = HandleVariableAssignment(parentObject, element, elementIndex);
                    break;

                case MetaModelConstants.ElementTypeReceive:
                    currentObject = HandleReceive(parentObject, element, elementIndex);
                    childElements = null;
                    break;

                case MetaModelConstants.ElementTypeCorrelationType:
                case MetaModelConstants.ElementTypeMultipartMessageType:
                case MetaModelConstants.ElementTypePortType:
                case MetaModelConstants.ElementTypeServiceLinkType:

                    // Ignore the 'types' as these will be referenced when binding the model and relevant properties
                    // added to existing workflow objects.  Also, no need to recurse into the child elements.
                    currentObject = parentObject;
                    childElements = null;
                    break;

                default:
                    currentObject = HandleElement(parentObject, element, elementIndex);
                    break;
            }

            // Recurse
            if (childElements != null && childElements.Any())
            {
                for (var i = 0; i < childElements.Length; i++)
                {
                    ParseMetaModelElement(metaModel, model, currentObject, element, childElements[i], i);
                }
            }
        }

        /// <summary>
        /// Handles the Module element.
        /// </summary>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <returns>The current workflow object.</returns>
        private static WorkflowObject HandleModule(WorkflowObject parentObject)
        {
            return parentObject;
        }

        /// <summary>
        /// Handles the ServiceBody element.
        /// </summary>
        /// <param name="model">The workflow model.</param>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <returns>The current workflow object.</returns>
        private static WorkflowObject HandleServiceBody(WorkflowDefinition model, WorkflowObject parentObject, Element element)
        {
            AddProperties(model, element);
            return parentObject;
        }

        /// <summary>
        /// Handles the ServiceDeclaration element.
        /// </summary>
        /// <param name="model">The workflow model.</param>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="parentElement">The parent metamodel element.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <returns>The current workflow object.</returns>
        private static WorkflowObject HandleServiceDeclaration(WorkflowDefinition model, WorkflowObject parentObject, Element parentElement, Element element)
        {
            model.Name = string.Concat(parentElement.FindPropertyValue(MetaModelConstants.PropertyKeyName), ".", element.FindPropertyValue(MetaModelConstants.PropertyKeyName));
            model.Key = model.Name.FormatKey();
            model.Type = WorkflowModelConstants.ActivityTypeWorkflow;
            return parentObject;
        }

        /// <summary>
        /// Handles the MessageDeclaration element.
        /// </summary>
        /// <param name="model">The workflow model.</param>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <returns>The current workflow object.</returns>
        private WorkflowObject HandleMessageDeclaration(WorkflowDefinition model, WorkflowObject parentObject, Element element)
        {
            // Define message
            WorkflowMessage message = null;

            // Get message name and type
            var messageName = element.FindPropertyValue(MetaModelConstants.PropertyKeyName);
            var messageType = element.FindPropertyValue(MetaModelConstants.PropertyKeyType);

            // Find all multipart message types
            var multiPartMessageTypes = Model.FindResourcesByType(ModelConstants.ResourceMultipartMessageType);
            if (multiPartMessageTypes != null && multiPartMessageTypes.Any())
            {
                foreach (var multiPartMessageType in multiPartMessageTypes)
                {
                    // Combine name with module to get a fully qualified type name
                    var parentModule = multiPartMessageType.FindRelatedResourcesByType(Model, ResourceRelationshipType.Parent, ModelConstants.ResourceModule).SingleOrDefault();
                    if (parentModule != null)
                    {
                        var typeName = string.Concat(parentModule.Name, ".", multiPartMessageType.Name);
                        if (typeName == messageType)
                        {
                            // Found multipart message, create a composite message
                            message = new WorkflowCompositeMessage()
                            {
                                Name = messageName,
                                Key = string.Concat(parentObject.Key, ".", messageName).FormatKey(),
                                Type = messageType
                            };

                            _logger.LogTrace(TraceMessages.CreatedWorkflowCompositeMessage, RuleName, messageName, messageType);

                            // Create parts
                            var metaModelObject = multiPartMessageType.SourceObject as Element;
                            if (metaModelObject?.Element1 != null && metaModelObject.Element1.Any(e => e.Type == MetaModelConstants.ElementTypePartDeclaration))
                            {
                                var parts = metaModelObject.Element1.Where(e => e.Type == MetaModelConstants.ElementTypePartDeclaration);
                                foreach (var part in parts)
                                {
                                    var partName = part.FindPropertyValue(MetaModelConstants.PropertyKeyName);
                                    var partType = part.FindPropertyValue(MetaModelConstants.PropertyKeyClassName);

                                    // Create part message
                                    var partMessage = new WorkflowMessage()
                                    {
                                        Name = partName,
                                        Key = string.Concat(message.Key, ".", partName).FormatKey(),
                                        Type = partType
                                    };

                                    ((WorkflowCompositeMessage)message).MessageParts.Add(partMessage);

                                    AddProperties(partMessage, part);

                                    _logger.LogTrace(TraceMessages.CreatedWorkflowCompositeMessagePart, RuleName, partName, partType);
                                }
                            }
                            else
                            {
                                _logger.LogError(ErrorMessages.MultipartMessageTypeMustHaveParts, multiPartMessageType.Name);
                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.MultipartMessageTypeMustHaveParts, multiPartMessageType.Name)));
                            }

                            break;
                        }
                    }
                    else
                    {
                        _logger.LogError(ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceModule, ResourceRelationshipType.Parent, multiPartMessageType.Key);
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceModule, ResourceRelationshipType.Parent, multiPartMessageType.Key)));
                    }
                }
            }

            // Was it a multipart message?
            if (message == null)
            {
                // No, create a normal message
                message = new WorkflowMessage()
                {
                    Name = messageName,
                    Key = string.Concat(parentObject.Key, ".", messageName).FormatKey(),
                    Type = messageType
                };

                _logger.LogTrace(TraceMessages.CreatedWorkflowMessage, RuleName, messageName, messageType);
            }

            AddProperties(message, element);

            // Set message type on each message from the schema associated with the message (if there is one,
            // message may be a System.Xml.XmlDocument - any message - or System.String, for example).
            var messageDefinitions = Model.FindResourcesByType(ModelConstants.ResourceMessageType);
            if (messageDefinitions != null && messageDefinitions.Any())
            {
                var workflowMessages = message is WorkflowCompositeMessage m ? m.MessageParts.ToArray() : new WorkflowMessage[1] { message };
                foreach (var workflowMessage in workflowMessages)
                {
                    foreach (var messageDefinition in messageDefinitions)
                    {
                        // Get source object
                        if (messageDefinition.SourceObject is MessageDefinition source)
                        {
                            if (workflowMessage.Type == source.FullName)
                            {
                                // Found a match, get the message type
                                workflowMessage.MessageType = source.MessageType;
                            }
                        }
                    }
                }
            }

            if (parentObject is WorkflowActivityContainer container)
            {
                // Add to scope of activity container
                container.Messages.Add(message);
            }
            else
            {
                // Add to top level scope
                model.Messages.Add(message);
            }

            return message;
        }

        /// <summary>
        /// Handles the VariableDeclaration element.
        /// </summary>
        /// <param name="model">The workflow model.</param>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <returns>The current workflow object.</returns>
        private WorkflowObject HandleVariableDeclaration(WorkflowDefinition model, WorkflowObject parentObject, Element element)
        {
            var variableName = element.FindPropertyValue(MetaModelConstants.PropertyKeyName);
            var variableType = element.FindPropertyValue(MetaModelConstants.PropertyKeyType);

            var variable = new WorkflowVariable()
            {
                Name = variableName,
                Key = string.Concat(parentObject.Key, ".", variableName).FormatKey(),
                Type = variableType,
                InitialValue = element.FindPropertyValue(MetaModelConstants.PropertyKeyInitialValue),
                UseDefaultConstructor = Convert.ToBoolean(element.FindPropertyValue(MetaModelConstants.PropertyKeyUseDefaultConstructor), CultureInfo.InvariantCulture)
            };

            AddProperties(variable, element);

            _logger.LogTrace(TraceMessages.CreatedWorkflowVariable, RuleName, variableName, variableType);

            if (parentObject is WorkflowActivityContainer container)
            {
                // Add to scope of activity container
                container.Variables.Add(variable);
            }
            else
            {
                // Add to top level scope
                model.Variables.Add(variable);
            }

            return variable;
        }

        /// <summary>
        /// Handles the PortDeclaration element.
        /// </summary>
        /// <param name="model">The workflow model.</param>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <returns>The current workflow object.</returns>
        private WorkflowObject HandlePortDeclaration(WorkflowDefinition model, WorkflowObject parentObject, Element element)
        {
            // Get port name and type
            var portName = element.FindPropertyValue(MetaModelConstants.PropertyKeyName);
            var portTypeName = element.FindPropertyValue(MetaModelConstants.PropertyKeyType);

            // Find port types to find operations
            var portTypes = Model.FindResourcesByType(ModelConstants.ResourcePortType);
            if (portTypes != null && portTypes.Any())
            {
                foreach (var portType in portTypes)
                {
                    // Combine name with module to get a fully qualified type name
                    var parentModule = portType.FindRelatedResourcesByType(Model, ResourceRelationshipType.Parent, ModelConstants.ResourceModule).SingleOrDefault();
                    if (parentModule != null)
                    {
                        var typeName = string.Concat(parentModule.Name, ".", portType.Name);
                        if (typeName == portTypeName)
                        {
                            // Get operations on port type
                            var metaModelObject = portType.SourceObject as Element;
                            if (metaModelObject?.Element1 != null && metaModelObject.Element1.Any(e => e.Type == MetaModelConstants.ElementTypeOperationDeclaration))
                            {
                                var operations = metaModelObject.Element1.Where(e => e.Type == MetaModelConstants.ElementTypeOperationDeclaration);
                                foreach (var operation in operations)
                                {
                                    // Create channel
                                    var channel = new WorkflowChannel()
                                    {
                                        Name = portName,
                                        OperationName = operation.FindPropertyValue(MetaModelConstants.PropertyKeyName),
                                        Key = string.Concat(parentObject.Key, ".", portName, ".", operation.FindPropertyValue(MetaModelConstants.PropertyKeyName)).FormatKey(),
                                        Type = portTypeName
                                    };

                                    AddProperties(channel, element);
                                    channel.Properties.Add(WorkflowModelConstants.PropertyIsServiceLink, false);

                                    _logger.LogTrace(TraceMessages.CreatedWorkflowChannel, RuleName, portName, portTypeName);

                                    model.Channels.Add(channel);

                                    var directBindingAttribute = element.Element1.Where(e => e.Type == MetaModelConstants.ElementTypeDirectBindingAttribute).SingleOrDefault();
                                    if (directBindingAttribute != null)
                                    {
                                        // When a Receive or Send activity is bound to a WorkflowChannel, the subscription or
                                        // promotion of properties is determined by the type of message associated with the
                                        // orchestration logical Port and any additional filters on the activatable Receive.
                                        // Additional promoted properties can be added to a message when sending in a Construct
                                        // with Message Assignment shape.
                                        // Non-activatable Receive may be associated with a convoy through a correlation set.
                                        // This will impact on the subscription to create.
                                        channel.Properties.Add(WorkflowModelConstants.PropertyBinding, WorkflowModelConstants.PropertyValueDirectBinding);
                                        channel.Properties.Add(WorkflowModelConstants.PropertyDirectBindingType, directBindingAttribute.FindPropertyValue(MetaModelConstants.PropertyKeyDirectBindingType));
                                    }
                                    else
                                    {
                                        var physicalBindingAttribute = element.Element1.Where(e => e.Type == MetaModelConstants.ElementTypePhysicalBindingAttribute).SingleOrDefault();
                                        if (physicalBindingAttribute != null)
                                        {
                                            // Although a physical binding (specify now), we will still look up the bound messaging port in the binding file
                                            // for the orchestration logical port when binding the workflow channel.
                                            channel.Properties.Add(WorkflowModelConstants.PropertyBinding, WorkflowModelConstants.PropertyValuePhysicalBinding);
                                        }
                                        else
                                        {
                                            // Assume it is a Logical binding (specify later).
                                            // When a Receive or Send activity is bound to a WorkflowChannel, look up in the BizTalk Binding
                                            // file what messaging port is associated with the orchestration logical port.  This determines
                                            // either the subscription (for a Receive) or what to promote (for a Send).
                                            channel.Properties.Add(WorkflowModelConstants.PropertyBinding, WorkflowModelConstants.PropertyValueLogicalBinding);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _logger.LogError(ErrorMessages.PortTypeMustHaveOperations, portTypeName);
                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.PortTypeMustHaveOperations, portTypeName)));
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError(ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceModule, ResourceRelationshipType.Parent, portType.Key);
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceModule, ResourceRelationshipType.Parent, portType.Key)));
                    }
                }
            }

            // Return parent, the calling method won't recurse because this method has handled the port declaration and any child elements.
            return parentObject;
        }

        /// <summary>
        /// Handles the ServiceLinkDeclaration element.
        /// </summary>
        /// <param name="model">The workflow model.</param>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <returns>The current workflow object.</returns>
        private WorkflowObject HandleServiceLinkDeclaration(WorkflowDefinition model, WorkflowObject parentObject, Element element)
        {
            // Define channels
            WorkflowChannel providerChannel = null;
            WorkflowChannel consumerChannel = null;

            // Get service link name and type
            var serviceLinkName = element.FindPropertyValue(MetaModelConstants.PropertyKeyName);
            var serviceLinkTypeName = element.FindPropertyValue(MetaModelConstants.PropertyKeyType);

            // Get role and port modifier - implements (receive) or uses (send)
            var serviceLinkRoleName = element.FindPropertyValue(MetaModelConstants.PropertyKeyRoleName);
            var serviceLinkPortModifier = element.FindPropertyValue(MetaModelConstants.PropertyKeyPortModifier);
            var serviceLinkRoleProvider = serviceLinkPortModifier == "Implements";

            // Find service link types to find roles and associated port types to create channels
            var serviceLinkTypes = Model.FindResourcesByType(ModelConstants.ResourceServiceLinkType);
            if (serviceLinkTypes != null && serviceLinkTypes.Any())
            {
                foreach (var serviceLinkType in serviceLinkTypes)
                {
                    // Combine name with module to get a fully qualified type name
                    var parentModule = serviceLinkType.FindRelatedResourcesByType(Model, ResourceRelationshipType.Parent, ModelConstants.ResourceModule).SingleOrDefault();
                    if (parentModule != null)
                    {
                        var typeName = string.Concat(parentModule.Name, ".", serviceLinkType.Name);
                        if (typeName == serviceLinkTypeName)
                        {
                            // Found service link type, now get roles
                            var metaModelObject = serviceLinkType.SourceObject as Element;
                            if (metaModelObject?.Element1 != null && metaModelObject.Element1.Any(e => e.Type == MetaModelConstants.ElementTypeRoleDeclaration))
                            {
                                var roles = metaModelObject.Element1.Where(e => e.Type == MetaModelConstants.ElementTypeRoleDeclaration);
                                foreach (var role in roles)
                                {
                                    // Get role name to determine if this is a Provider or Consumer role
                                    var roleName = role.FindPropertyValue(MetaModelConstants.PropertyKeyName);
                                    var channelName = string.Concat(serviceLinkName, ".", roleName);

                                    // Get port type from port reference
                                    var portTypeReference = role.Element1?.Where(e => e.Type == MetaModelConstants.ElementTypePortTypeRef).SingleOrDefault();
                                    if (portTypeReference != null)
                                    {
                                        // Get port type value
                                        var channelType = portTypeReference.FindPropertyValue(MetaModelConstants.PropertyKeyRef);

                                        // Create channel based on role and whether it's a provider or consumer
                                        var roleProvider = roleName == serviceLinkRoleName ? serviceLinkRoleProvider : !serviceLinkRoleProvider;
                                        if (roleProvider)
                                        {
                                            // The Provider role is used to receive messages
                                            providerChannel = new WorkflowChannel()
                                            {
                                                Name = channelName,
                                                Key = string.Concat(parentObject.Key, ".", channelName).FormatKey(),
                                                Type = channelType
                                            };

                                            AddProperties(providerChannel, element);
                                            providerChannel.Properties.Add(WorkflowModelConstants.PropertyIsServiceLink, true);
                                            providerChannel.Properties[WorkflowModelConstants.PropertyRoleName] = roleName;

                                            _logger.LogTrace(TraceMessages.CreatedWorkflowChannel, RuleName, channelName, channelType);

                                            model.Channels.Add(providerChannel);
                                        }
                                        else
                                        {
                                            // The Consumer role is used to send messages
                                            consumerChannel = new WorkflowChannel()
                                            {
                                                Name = channelName,
                                                Key = string.Concat(parentObject.Key, ".", channelName).FormatKey(),
                                                Type = channelType
                                            };

                                            AddProperties(consumerChannel, element);
                                            consumerChannel.Properties.Add(WorkflowModelConstants.PropertyIsServiceLink, true);
                                            consumerChannel.Properties[WorkflowModelConstants.PropertyRoleName] = roleName;

                                            _logger.LogTrace(TraceMessages.CreatedWorkflowChannel, RuleName, channelName, channelType);

                                            model.Channels.Add(consumerChannel);
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogError(ErrorMessages.ServiceLinkTypeRoleMustHavePortTypeReference, serviceLinkTypeName, roleName);
                                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.ServiceLinkTypeRoleMustHavePortTypeReference, serviceLinkTypeName, roleName)));
                                    }
                                }
                            }
                            else
                            {
                                _logger.LogError(ErrorMessages.ServiceLinkTypeMustHaveRoles, serviceLinkTypeName);
                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.ServiceLinkTypeMustHaveRoles, serviceLinkTypeName)));
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError(ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceModule, ResourceRelationshipType.Parent, serviceLinkType.Key);
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceModule, ResourceRelationshipType.Parent, serviceLinkType.Key)));
                    }
                }
            }

            // If both provider and consumer channels created, add a reference to each other
            if (providerChannel != null && consumerChannel != null)
            {
                providerChannel.Properties.Add(WorkflowModelConstants.PropertyConsumerChannelReference, consumerChannel.Key);
                consumerChannel.Properties.Add(WorkflowModelConstants.PropertyProviderChannelReference, providerChannel.Key);
            }

            // Return parent, the calling method won't recurse because this method has handled the port declaration and any child elements.
            return parentObject;
        }

        /// <summary>
        /// Handles the CorrelationDeclaration element.
        /// </summary>
        /// <param name="model">The workflow model.</param>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <param name="elementIndex">The index used to provide key uniqueness.</param>
        /// <returns>The current workflow object.</returns>
        private WorkflowObject HandleCorrelationDeclaration(WorkflowDefinition model, WorkflowObject parentObject, Element element, int elementIndex)
        {
            // Get correlation name and type
            var correlationName = element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type;
            var correlationTypeName = element.FindPropertyValue(MetaModelConstants.PropertyKeyType);

            // Create correlation variable
            var correlationVariable = new WorkflowCorrelationVariable()
            {
                Name = correlationName,
                Key = string.Concat(parentObject.Key, ".", correlationName, elementIndex.ToString(CultureInfo.InvariantCulture)).FormatKey(),
                Type = correlationTypeName
            };

            AddProperties(correlationVariable, element);

            _logger.LogTrace(TraceMessages.CreatedWorkflowCorrelationVariable, RuleName, correlationName, correlationTypeName);

            // Find all correlation types
            var correlationTypes = Model.FindResourcesByType(ModelConstants.ResourceCorrelationType);
            if (correlationTypes != null && correlationTypes.Any())
            {
                foreach (var correlationType in correlationTypes)
                {
                    // Combine name with module to get a fully qualified type name
                    var parentModule = correlationType.FindRelatedResourcesByType(Model, ResourceRelationshipType.Parent, ModelConstants.ResourceModule).SingleOrDefault();
                    if (parentModule != null)
                    {
                        var typeName = string.Concat(parentModule.Name, ".", correlationType.Name);
                        if (typeName == correlationTypeName)
                        {
                            // Found correlation type, get properties
                            var metaModelObject = correlationType.SourceObject as Element;
                            if (metaModelObject?.Element1 != null && metaModelObject.Element1.Any(e => e.Type == MetaModelConstants.ElementTypePropertyRef))
                            {
                                var properties = metaModelObject.Element1.Where(e => e.Type == MetaModelConstants.ElementTypePropertyRef);
                                foreach (var property in properties)
                                {
                                    var correlationProperty = new WorkflowVariable()
                                    {
                                        Name = property.FindPropertyValue(MetaModelConstants.PropertyKeyName),
                                        Key = string.Concat(correlationVariable.Key, ".", property.FindPropertyValue(MetaModelConstants.PropertyKeyName)),
                                        Type = property.FindPropertyValue(MetaModelConstants.PropertyKeyRef)
                                    };

                                    correlationVariable.CorrelationProperties.Add(correlationProperty);
                                }
                            }
                            else
                            {
                                _logger.LogError(ErrorMessages.CorrelationTypeMustHaveProperties, correlationType.Name);
                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.CorrelationTypeMustHaveProperties, correlationType.Name)));
                            }

                            break;
                        }
                    }
                    else
                    {
                        _logger.LogError(ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceModule, ResourceRelationshipType.Parent, correlationType.Key);
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceModule, ResourceRelationshipType.Parent, correlationType.Key)));
                    }
                }
            }

            if (parentObject is WorkflowActivityContainer container)
            {
                // Add to scope of activity container
                container.Variables.Add(correlationVariable);
            }
            else
            {
                // Add to top level scope
                model.Variables.Add(correlationVariable);
            }

            // Add statement references (to receive/send activities)
            var statements = element.Element1?.Where(e => e.Type == MetaModelConstants.ElementTypeStatementRef).ToArray();
            if (statements != null && statements.Any())
            {
                var relatedActivities = new Dictionary<string, bool>(statements.Length);
                foreach (var statement in statements)
                {
                    relatedActivities.Add(statement.FindPropertyValue(MetaModelConstants.PropertyKeyRef), Convert.ToBoolean(statement.FindPropertyValue(MetaModelConstants.PropertyKeyInitializes), CultureInfo.InvariantCulture));
                }

                correlationVariable.Properties.Add(WorkflowModelConstants.PropertyActivityReferences, relatedActivities);
            }

            return correlationVariable;
        }

        /// <summary>
        /// Handles the Call element.
        /// </summary>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <param name="elementIndex">The index used to provide key uniqueness.</param>
        /// <returns>The current workflow object.</returns>
        private static WorkflowObject HandleCall(WorkflowObject parentObject, Element element, int elementIndex)
        {
            // Create activity
            var callActivity = new WorkflowActivity()
            {
                Name = element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type,
                Key = string.Concat(parentObject.Key, ".", element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type, elementIndex.ToString(CultureInfo.InvariantCulture)).FormatKey(),
                Type = WorkflowModelConstants.ActivityTypeInvokeWorkflow
            };

            AddProperties(callActivity, element);
            callActivity.Properties.Add(WorkflowModelConstants.PropertyIsAsync, false);

            ((WorkflowActivityContainer)parentObject).Activities.Add(callActivity);

            var callParameters = element.Element1?.Where(e => e.Type == MetaModelConstants.ElementTypeParameter).ToArray();
            if (callParameters != null && callParameters.Any())
            {
                var callParameterValues = new Dictionary<string, string>(callParameters.Length);
                foreach (var callParameter in callParameters)
                {
                    callParameterValues.Add(callParameter.FindPropertyValue(MetaModelConstants.PropertyKeyName), callParameter.FindPropertyValue(MetaModelConstants.PropertyKeyType));
                }

                callActivity.Properties.Add(WorkflowModelConstants.PropertyWorkflowParameters, callParameterValues);
            }

            return callActivity;
        }

        /// <summary>
        /// Handles the Exec element.
        /// </summary>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <param name="elementIndex">The index used to provide key uniqueness.</param>
        /// <returns>The current workflow object.</returns>
        private static WorkflowObject HandleExec(WorkflowObject parentObject, Element element, int elementIndex)
        {
            // Create activity
            var execActivity = new WorkflowActivity()
            {
                Name = element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type,
                Key = string.Concat(parentObject.Key, ".", element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type, elementIndex.ToString(CultureInfo.InvariantCulture)).FormatKey(),
                Type = WorkflowModelConstants.ActivityTypeInvokeWorkflow
            };

            AddProperties(execActivity, element);
            execActivity.Properties.Add(WorkflowModelConstants.PropertyIsAsync, true);

            ((WorkflowActivityContainer)parentObject).Activities.Add(execActivity);

            var execParameters = element.Element1?.Where(e => e.Type == MetaModelConstants.ElementTypeParameter).ToArray();
            if (execParameters != null && execParameters.Any())
            {
                var execParameterValues = new Dictionary<string, string>(execParameters.Length);
                foreach (var execParameter in execParameters)
                {
                    execParameterValues.Add(execParameter.FindPropertyValue(MetaModelConstants.PropertyKeyName), execParameter.FindPropertyValue(MetaModelConstants.PropertyKeyType));
                }

                execActivity.Properties.Add(WorkflowModelConstants.PropertyWorkflowParameters, execParameterValues);
            }

            return execActivity;
        }

        /// <summary>
        /// Handles the Construct element.
        /// </summary>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <param name="elementIndex">The index used to provide key uniqueness.</param>
        /// <returns>The current workflow object.</returns>
        private static WorkflowObject HandleConstruct(WorkflowObject parentObject, Element element, int elementIndex)
        {
            // Set up a group for message assignment and message transform activities
            var groupContainer = new WorkflowActivityContainer()
            {
                Name = element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type,
                Key = string.Concat(parentObject.Key, ".", element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type, elementIndex.ToString(CultureInfo.InvariantCulture)).FormatKey(),
                Type = WorkflowModelConstants.ActivityTypeActivityGroup
            };

            AddProperties(groupContainer, element);

            ((WorkflowActivityContainer)parentObject).Activities.Add(groupContainer);

            // Check each child shape in the construct and build the activities in the correct order
            var childElements = element.Element1.Where(e => e.Type == MetaModelConstants.ElementTypeMessageAssignment || e.Type == MetaModelConstants.ElementTypeTransform).ToArray();
            if (childElements != null && childElements.Any())
            {
                foreach (var childElement in childElements)
                {
                    var constructActivity = new WorkflowActivity()
                    {
                        Name = childElement.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? childElement.Type,
                        Key = string.Concat(groupContainer.Key, ".", childElement.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? childElement.Type, elementIndex.ToString(CultureInfo.InvariantCulture)).FormatKey()
                    };

                    AddProperties(constructActivity, childElement);

                    groupContainer.Activities.Add(constructActivity);

                    // Is it a message assignment shape?
                    if (childElement.Type == MetaModelConstants.ElementTypeMessageAssignment)
                    {
                        constructActivity.Type = WorkflowModelConstants.ActivityTypeMessageConstruction;
                    }

                    // Is it a transform shape?
                    if (childElement.Type == MetaModelConstants.ElementTypeTransform)
                    {
                        constructActivity.Type = WorkflowModelConstants.ActivityTypeMessageTransform;
                        constructActivity.Properties.Add(WorkflowModelConstants.PropertyMap, childElement.FindPropertyValue(MetaModelConstants.PropertyKeyClassName));

                        var parts = childElement.Element1.Where(e => e.Type == MetaModelConstants.ElementTypeMessagePartRef).ToArray();
                        if (parts != null && parts.Any())
                        {
                            // Get input and output messages separately
                            var sourceParts = parts.Where(p => p.ParentLink == MetaModelConstants.ParentLinkTransformInputMessage).ToArray();
                            var targetParts = parts.Where(p => p.ParentLink == MetaModelConstants.ParentLinkTransformOutputMessage).ToArray();

                            // Add source (input) messages to activity
                            if (sourceParts != null && sourceParts.Any())
                            {
                                var sourceMessageRefs = new List<string>(sourceParts.Length);

                                for (var i = 0; i < sourceParts.Length; i++)
                                {
                                    var messageRef = sourceParts[i].FindPropertyValue(MetaModelConstants.PropertyKeyMessageRef);
                                    var partRef = sourceParts[i].FindPropertyValue(MetaModelConstants.PropertyKeyPartRef);
                                    var formattedMessageRef = partRef != null ? $"{messageRef}.{partRef}" : messageRef;
                                    sourceMessageRefs.Add(formattedMessageRef);
                                }

                                constructActivity.Properties.Add(WorkflowModelConstants.PropertySourceMessageReferences, sourceMessageRefs);
                                constructActivity.Properties.Add(WorkflowModelConstants.PropertyIsMultiSource, sourceParts.Length > 1 ? true : false);
                            }

                            // Add target (output) messages to activity
                            if (targetParts != null && targetParts.Any())
                            {
                                var targetMessageRefs = new List<string>(targetParts.Length);

                                for (var i = 0; i < targetParts.Length; i++)
                                {
                                    var messageRef = targetParts[i].FindPropertyValue(MetaModelConstants.PropertyKeyMessageRef);
                                    var partRef = targetParts[i].FindPropertyValue(MetaModelConstants.PropertyKeyPartRef);
                                    var formattedMessageRef = partRef != null ? $"{messageRef}.{partRef}" : messageRef;
                                    targetMessageRefs.Add(formattedMessageRef);
                                }

                                constructActivity.Properties.Add(WorkflowModelConstants.PropertyTargetMessageReferences, targetMessageRefs);
                                constructActivity.Properties.Add(WorkflowModelConstants.PropertyIsMultiTarget, targetParts.Length > 1 ? true : false);
                            }
                        }
                    }
                }
            }

            var constructedMessagesRef = element.Element1.Where(e => e.Type == MetaModelConstants.ElementTypeMessageRef).ToArray();
            if (constructedMessagesRef != null && constructedMessagesRef.Any())
            {
                var constructedMessages = new List<string>(constructedMessagesRef.Length);

                foreach (var constructedMessageRef in constructedMessagesRef)
                {
                    var constructedMessage = constructedMessageRef.FindPropertyValue(MetaModelConstants.PropertyKeyRef);
                    constructedMessages.Add(constructedMessage);
                }

                groupContainer.Properties.Add(WorkflowModelConstants.PropertyConstructedMessages, constructedMessages);
            }

            return groupContainer;
        }

        /// <summary>
        /// Handles the Task element.
        /// </summary>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <param name="elementIndex">The index used to provide key uniqueness.</param>
        /// <returns>The current workflow object.</returns>
        private static WorkflowObject HandleTask(WorkflowObject parentObject, Element element, int elementIndex)
        {
            // Used in BizTalk as a grouping shape
            var groupContainer = new WorkflowActivityContainer()
            {
                Name = element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type,
                Key = string.Concat(parentObject.Key, ".", element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type, elementIndex.ToString(CultureInfo.InvariantCulture)).FormatKey(),
                Type = WorkflowModelConstants.ActivityTypeActivityGroup
            };

            AddProperties(groupContainer, element);

            ((WorkflowActivityContainer)parentObject).Activities.Add(groupContainer);

            return groupContainer;
        }

        /// <summary>
        /// Handles the VariableAssignment element.
        /// </summary>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <param name="elementIndex">The index used to provide key uniqueness.</param>
        /// <returns>The current workflow object.</returns>
        private static WorkflowObject HandleVariableAssignment(WorkflowObject parentObject, Element element, int elementIndex)
        {
            var codeExpression = new WorkflowActivity()
            {
                Name = element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type,
                Key = string.Concat(parentObject.Key, ".", element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type, elementIndex.ToString(CultureInfo.InvariantCulture)).FormatKey(),
                Type = WorkflowModelConstants.ActivityTypeCodeExpression
            };

            AddProperties(codeExpression, element);

            codeExpression.Properties.Add(WorkflowModelConstants.PropertyExpressionLanguage, WorkflowModelConstants.PropertyValueLanguageCSharp);

            ((WorkflowActivityContainer)parentObject).Activities.Add(codeExpression);

            return codeExpression;
        }

        /// <summary>
        /// Handles the Receive element.
        /// </summary>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <param name="elementIndex">The index used to provide key uniqueness.</param>
        /// <returns>The current workflow object.</returns>
        private WorkflowObject HandleReceive(WorkflowObject parentObject, Element element, int elementIndex)
        {
            // Create activity
            var receiveActivity = new WorkflowActivity()
            {
                Name = element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type,
                Key = string.Concat(parentObject.Key, ".", element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type, elementIndex.ToString(CultureInfo.InvariantCulture)).FormatKey(),
                Type = element.Type
            };

            AddProperties(receiveActivity, element);

            ((WorkflowActivityContainer)parentObject).Activities.Add(receiveActivity);

            // Parse the DNFPredicate and attach predicate groups containing expressions to the receive activity
            if (element.Element1 != null)
            {
                var predicates = element.Element1.Where(e => e.Type == MetaModelConstants.ElementTypeDnfPredicate).ToArray();
                if (predicates != null && predicates.Any())
                {
                    var predicateGroups = new List<List<string>>();
                    var predicateGroup = new List<string>();

                    for (var i = 0; i < predicates.Length; i++)
                    {
                        // Parse expression
                        var leftExpression = MapSubscriptionFilterProperty(predicates[i].FindPropertyValue(MetaModelConstants.PropertyKeyLhs));
                        var rightExpression = predicates[i].FindPropertyValue(MetaModelConstants.PropertyKeyRhs).Replace("\"", string.Empty);
                        var logicalOperator = predicates[i].FindPropertyValue(MetaModelConstants.PropertyKeyOperator);

                        switch (logicalOperator)
                        {
                            case "Equals":
                                logicalOperator = "=";
                                break;
                            case "LessThan":
                                logicalOperator = "<";
                                break;
                            case "LessThanEqualTo":
                                logicalOperator = "<=";
                                break;
                            case "GreaterThan":
                                logicalOperator = ">";
                                break;
                            case "GreaterThanEqualTo":
                                logicalOperator = ">=";
                                break;
                            case "NotEquals":
                                logicalOperator = "!=";
                                break;
                            case "Exists":
                                logicalOperator = "EXISTS";
                                break;
                        }

                        // Build expression (format for EXISTS is different to other operators)
                        var expression = $"{leftExpression} {logicalOperator} '{rightExpression}'";
                        if (logicalOperator == "EXISTS")
                        {
                            expression = $"EXISTS ( {leftExpression} )";
                        }

                        // Add to group
                        predicateGroup.Add(expression);

                        // Create new group if grouping is an OR
                        var grouping = predicates[i].FindPropertyValue(MetaModelConstants.PropertyKeyGrouping);
                        if (grouping == "OR")
                        {
                            predicateGroups.Add(predicateGroup);
                            predicateGroup = new List<string>();
                        }
                    }

                    predicateGroups.Add(predicateGroup);
                    receiveActivity.Properties.Add(WorkflowModelConstants.PropertySubscriptionFilter, predicateGroups);
                }
            }

            return receiveActivity;
        }

        /// <summary>
        /// Handles any element.
        /// </summary>
        /// <param name="parentObject">The parent workflow object.</param>
        /// <param name="element">The current metamodel element.</param>
        /// <param name="elementIndex">The index used to provide key uniqueness.</param>
        /// <returns>The current workflow object.</returns>
        private static WorkflowObject HandleElement(WorkflowObject parentObject, Element element, int elementIndex)
        {
            WorkflowActivity activity = null;

            if (element.Element1 != null && element.Element1.Any())
            {
                activity = new WorkflowActivityContainer()
                {
                    Name = element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type,
                    Key = string.Concat(parentObject.Key, ".", element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type, elementIndex.ToString(CultureInfo.InvariantCulture)).FormatKey(),
                    Type = element.Type
                };

                AddProperties(activity, element);

                // This check stops it from recursing into declaration and type elements and
                // ensure we only add activities within the service body.
                if (parentObject != null && parentObject is WorkflowActivityContainer container)
                {
                    container.Activities.Add(activity);
                }
            }
            else
            {
                activity = new WorkflowActivity()
                {
                    Name = element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type,
                    Key = string.Concat(parentObject.Key, ".", element.FindPropertyValue(MetaModelConstants.PropertyKeyName) ?? element.Type, elementIndex.ToString(CultureInfo.InvariantCulture)).FormatKey(),
                    Type = element.Type
                };

                AddProperties(activity, element);

                // This check stops it from recursing into declaration and type elements and
                // ensure we only add activities within the service body.
                if (parentObject != null && parentObject is WorkflowActivityContainer container)
                {
                    container.Activities.Add(activity);
                }
            }

            return activity;
        }

        /// <summary>
        /// Adds properties from the metamodel element to the workflow object.
        /// </summary>
        /// <param name="workflowObject">The workflow object.</param>
        /// <param name="metaModelObject">The metamodel element.</param>
        private static void AddProperties(WorkflowObject workflowObject, Element metaModelObject)
        {
            workflowObject.Properties.Add(WorkflowModelConstants.PropertyObjectId, metaModelObject.OID);
            if (metaModelObject.Property != null && metaModelObject.Property.Any())
            {
                foreach (var property in metaModelObject.Property)
                {
                    // Ignore certain properties
                    if (s_propertyIgnoreList.Contains(property.Name))
                    {
                        continue;
                    }

                    workflowObject.Properties.Add(property.Name, property.Value);
                }
            }
        }

        /// <summary>
        /// Binds the workflow model by matching channels to ports and correlation sets to receive and
        /// send shapes.
        /// </summary>
        /// <param name="sourceApplication">The source application.</param>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="orchestration">The orchestration.</param>
        /// <param name="model">The workflow model.</param>
        /// <returns></returns>
        private void BindWorkflowModel(ResourceItem sourceApplication, Application targetApplication, ResourceItem orchestration, WorkflowDefinition model)
        {
            _logger.LogTrace(TraceMessages.BindingWorkflowModel, RuleName, orchestration.Name);

            if (model.Activities.Any())
            {
                BindChannels(sourceApplication, targetApplication, orchestration, model, model);
                BindCorrelationVariables(orchestration, model, model);
            }
        }

        /// <summary>
        /// Binds the workflow channels to messaging channels in the target model.
        /// </summary>
        /// <param name="sourceApplication">The source application.</param>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="orchestration">The orchestration.</param>
        /// <param name="model">The workflow model.</param>
        /// <param name="container">The workflow activity container in which to find bindable activities.</param>
        private void BindChannels(ResourceItem sourceApplication, Application targetApplication, ResourceItem orchestration, WorkflowDefinition model, WorkflowActivityContainer container)
        {
            _logger.LogTrace(TraceMessages.BindingWorkflowChannels, RuleName, model.Name, orchestration.Name, container.Name);

            if (container.Activities.Any())
            {
                foreach (var activity in container.Activities)
                {
                    // Handle activity based on its type
                    if (_bindChannelHandlers.ContainsKey(activity.Type))
                    {
                        _bindChannelHandlers[activity.Type](sourceApplication, targetApplication, model, activity);
                    }
                }
            }

            // Recurse
            var childContainers = container.Activities.Where(a => a is WorkflowActivityContainer).Select(a => (WorkflowActivityContainer)a);
            if (childContainers.Any())
            {
                foreach (var childContainer in childContainers)
                {
                    BindChannels(sourceApplication, targetApplication, orchestration, model, childContainer);
                }
            }
        }

        /// <summary>
        /// Handles an activity when binding channels.
        /// </summary>
        /// <param name="sourceApplication">The source application.</param>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="model">The workflow model.</param>
        /// <param name="activity">The workflow activity to bind the channel to.</param>
        private delegate void BindActivityHandler(ResourceItem sourceApplication, Application targetApplication, WorkflowDefinition model, WorkflowActivity activity);

        /// <summary>
        /// Handles binding the receive activity with the workflow channel.
        /// </summary>
        /// <param name="sourceApplication">The source application.</param>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="model">The workflow model.</param>
        /// <param name="activity">The workflow activity to bind the channel to.</param>
        private void HandleReceiveActivity(ResourceItem sourceApplication, Application targetApplication, WorkflowDefinition model, WorkflowActivity activity)
        {
            var messageType = WorkflowMessageType.Request;
            var activatableReceive = Convert.ToBoolean(activity.Properties[WorkflowModelConstants.PropertyActivate], CultureInfo.InvariantCulture);

            // Get channel name from activity
            string channelName = null;
            string operationName = null;
            var isRoleLink = activity.Properties.ContainsKey(WorkflowModelConstants.PropertyServiceLinkName);
            if (isRoleLink)
            {
                channelName = string.Concat((string)activity.Properties[WorkflowModelConstants.PropertyServiceLinkName], ".", (string)activity.Properties[WorkflowModelConstants.PropertyServiceLinkRoleName]);
            }
            else
            {
                channelName = (string)activity.Properties[WorkflowModelConstants.PropertyPortName];
                operationName = (string)activity.Properties[WorkflowModelConstants.PropertyOperationName];
            }

            // Find channel
            var channel = model.Channels.Where(c => c.Name == channelName && c.OperationName == operationName).SingleOrDefault();
            if (channel != null)
            {
                _logger.LogTrace(TraceMessages.BindingReceiveActivity, RuleName, channel.Name, activity.Name);

                // Is it an activatable workflow?
                if (!channel.Activator)
                {
                    channel.Activator = activatableReceive;
                }

                // If it's a service linked receive activity, get port type from the activity properties
                var portTypeName = channel.Type;
                if (isRoleLink)
                {
                    portTypeName = (string)activity.Properties[WorkflowModelConstants.PropertyServiceLinkPortTypeName];
                }

                // Find port types to find operations and determine message exchange pattern for channel
                var portTypes = Model.FindResourcesByType(ModelConstants.ResourcePortType);
                if (portTypes != null && portTypes.Any())
                {
                    foreach (var portType in portTypes)
                    {
                        // Combine name with module to get a fully qualified type name
                        var parentModule = portType.FindRelatedResourcesByType(Model, ResourceRelationshipType.Parent, ModelConstants.ResourceModule).SingleOrDefault();
                        if (parentModule != null)
                        {
                            var typeName = string.Concat(parentModule.Name, ".", portType.Name);
                            if (typeName == portTypeName)
                            {
                                // Found port type, now match up operations and messages
                                var operationMessageName = (string)activity.Properties[WorkflowModelConstants.PropertyOperationMessageName];

                                // Get operations on port type
                                var metaModelObject = portType.SourceObject as Element;
                                if (metaModelObject?.Element1 != null && metaModelObject.Element1.Any(e => e.Type == MetaModelConstants.ElementTypeOperationDeclaration))
                                {
                                    var operations = metaModelObject.Element1.Where(e => e.Type == MetaModelConstants.ElementTypeOperationDeclaration);
                                    foreach (var operation in operations)
                                    {
                                        if (operationName == operation.FindPropertyValue(MetaModelConstants.PropertyKeyName))
                                        {
                                            // Get operation type (OneWay or RequestResponse)
                                            var operationType = operation.FindPropertyValue(MetaModelConstants.ElementTypeOperationType);

                                            // Get message references
                                            var messageRefs = operation.Element1?.Where(e => e.Type == MetaModelConstants.ElementTypeMessageRef).ToArray();
                                            if (messageRefs != null && messageRefs.Any())
                                            {
                                                var matched = false;
                                                foreach (var messageRef in messageRefs)
                                                {
                                                    if (operationMessageName == messageRef.FindPropertyValue(MetaModelConstants.PropertyKeyName))
                                                    {
                                                        // What type of message is it?
                                                        switch (messageRef.ParentLink)
                                                        {
                                                            case MetaModelConstants.ParentLinkOperationRequestMessage:

                                                                if (operationType == "OneWay")
                                                                {
                                                                    // The receive activity is the request of a one-way port
                                                                    messageType = WorkflowMessageType.Request;
                                                                    channel.Direction = MessageExchangePattern.Receive;
                                                                }
                                                                else
                                                                {
                                                                    // The receive activity is the request (receive request) of a request-response port
                                                                    messageType = WorkflowMessageType.Request;
                                                                    channel.Direction = MessageExchangePattern.ReceiveResponse;
                                                                }

                                                                matched = true;
                                                                break;

                                                            case MetaModelConstants.ParentLinkOperationResponseMessage:

                                                                // The receive activity is the response (to receive reply) of a request-response port
                                                                messageType = WorkflowMessageType.Response;
                                                                channel.Direction = MessageExchangePattern.RequestReply;

                                                                matched = true;
                                                                break;

                                                            case MetaModelConstants.ParentLinkOperationFaultMessage:

                                                                // The receive activity is the fault response (to receive reply) of a request-response port
                                                                messageType = WorkflowMessageType.Fault;
                                                                channel.Direction = MessageExchangePattern.RequestReply;

                                                                matched = true;
                                                                break;
                                                        }
                                                    }
                                                }

                                                if (!matched)
                                                {
                                                    // Should never get here, but if we do, something wrong with metamodel
                                                    _logger.LogError(ErrorMessages.PortTypeOperationMustHaveMatchingMessage, portTypeName, operationName, operationMessageName, activity.Name);
                                                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.PortTypeOperationMustHaveMatchingMessage, portTypeName, operationName, operationMessageName, activity.Name)));
                                                }
                                            }
                                            else
                                            {
                                                _logger.LogError(ErrorMessages.PortTypeOperationMustHaveMessageReferences, portTypeName, operationName);
                                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.PortTypeOperationMustHaveMessageReferences, portTypeName, operationName)));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    _logger.LogError(ErrorMessages.PortTypeMustHaveOperations, portTypeName);
                                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.PortTypeMustHaveOperations, portTypeName)));
                                }
                            }
                        }
                        else
                        {
                            _logger.LogError(ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceModule, ResourceRelationshipType.Parent, portType.Key);
                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceModule, ResourceRelationshipType.Parent, portType.Key)));
                        }
                    }
                }

                // Find message to build subscription and set the message as the input for the channel
                var message = FindMessage(model, activity.Properties[WorkflowModelConstants.PropertyMessageName].ToString());
                if (message != null)
                {
                    // Set type
                    message.WorkflowMessageType = messageType;

                    // Is it a composite message?
                    if (message is WorkflowCompositeMessage compositeMessage)
                    {
                        foreach (var messagePart in compositeMessage.MessageParts)
                        {
                            messagePart.WorkflowMessageType = messageType;
                        }
                    }

                    // Set activate property on message
                    if (!message.Properties.ContainsKey(WorkflowModelConstants.PropertyActivate))
                    {
                        message.Properties.Add(WorkflowModelConstants.PropertyActivate, activatableReceive);
                    }

                    // Assign to channel 'In' message (if it doesn't already exist)
                    if (!channel.MessagesIn.Any(m => m.Name == message.Name))
                    {
                        channel.MessagesIn.Add(message);
                    }

                    // Build subscription
                    if (activatableReceive)
                    {
                        channel.Subscription = BuildChannelSubscription(model, activity, channel, message);
                    }
                }

                // Set relationship
                activity.Properties.Add(WorkflowModelConstants.PropertyWorkflowChannel, channel.Key);
            }
            else
            {
                if (isRoleLink)
                {
                    _logger.LogError(ErrorMessages.UnableToFindWorkflowChannel, activity.Properties[WorkflowModelConstants.PropertyServiceLinkName], activity.Name);
                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindWorkflowChannel, activity.Properties[WorkflowModelConstants.PropertyServiceLinkName], activity.Name)));
                }
                else
                {
                    _logger.LogError(ErrorMessages.UnableToFindWorkflowChannel, activity.Properties[WorkflowModelConstants.PropertyPortName], activity.Name);
                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindWorkflowChannel, activity.Properties[WorkflowModelConstants.PropertyPortName], activity.Name)));
                }
            }
        }

        /// <summary>
        /// Builds the subscription for the specified receive activity and attaches it to the workflow channel.
        /// </summary>
        /// <param name="model">The workflow model.</param>
        /// <param name="activity">The receive activity.</param>
        /// <param name="channel">The channel on which the message is received.</param>
        /// <param name="message">The message being received.</param>
        private Subscription BuildChannelSubscription(WorkflowDefinition model, WorkflowActivity activity, WorkflowChannel channel, WorkflowMessage message)
        {
            // Define subscription
            Subscription subscription = null;

            // Build key to find topic channel in target model
            var topicChannelKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}:{ModelConstants.MessageBoxLeafKey}";

            // Find topic channel
            var topicChannelObject = Model.FindMessagingObject(topicChannelKey);
            if (topicChannelObject.application != null && topicChannelObject.messagingObject != null)
            {
                var topicChannel = (TopicChannel)topicChannelObject.messagingObject;

                // Determine if a subscription is needed for a following correlation set
                if (activity.Properties.ContainsKey(WorkflowModelConstants.PropertyFollowsCorrelation))
                {
                    // TODO: Create a subscription for the correlation type (if we don't know)
                    // the values until runtime, how do we create a subscription filter?  Same
                    // as BizTalk I guess, create a subscription filter on Send.
                }

                // Create filter group in which rules are created
                var filterGroup = new AndFilterGroup();

                // Check to see if the receive activity has a logical or physical binding
                if (channel.Properties.Any(p => p.Key == WorkflowModelConstants.PropertyBinding && ((string)p.Value == WorkflowModelConstants.PropertyValueLogicalBinding || (string)p.Value == WorkflowModelConstants.PropertyValuePhysicalBinding)))
                {
                    // Need to find the equivalent 'messaging port' to this 'logical port' in the BizTalk bindings
                    var serviceBinding = Model.FindResourcesByType(ModelConstants.ResourceServiceBinding).Where(s => s.Name == model.Name).SingleOrDefault();
                    if (serviceBinding != null)
                    {
                        if (serviceBinding.SourceObject is ServiceRef source)
                        {
                            // Check direction of channel
                            if (channel.Direction == MessageExchangePattern.Receive || channel.Direction == MessageExchangePattern.Accept || channel.Direction == MessageExchangePattern.ReceiveResponse)
                            {
                                // It's a Request receive activity
                                var receivePort = source.Ports?.Where(p => p.Name == channel.Name && p.ReceivePortRef != null).Select(p => p.ReceivePortRef.Name).SingleOrDefault();
                                if (receivePort != null)
                                {
                                    _logger.LogTrace(TraceMessages.FoundPortForReceiveActivityLogicalBinding, RuleName, receivePort, channel.Name, activity.Name, model.Name);

                                    // Add the receive port name as an expression to the subscription
                                    filterGroup.Filters.Add(new Filter() { FilterExpression = $"{ModelConstants.BizTalkReceivePortName} = '{receivePort}'" });

                                    // Add the receive port name to the channel properties (if it doesn't already exist)
                                    if (!channel.Properties.ContainsKey(WorkflowModelConstants.PropertyLogicalBindingPort))
                                    {
                                        channel.Properties.Add(WorkflowModelConstants.PropertyLogicalBindingPort, receivePort);
                                    }
                                }
                            }
                            else
                            {
                                // It's a Response receive activity
                                var sendPort = source.Ports?.Where(p => p.Name == channel.Name && p.SendPortRef != null).Select(p => p.SendPortRef.Name).SingleOrDefault();
                                if (sendPort != null)
                                {
                                    _logger.LogTrace(TraceMessages.FoundPortForReceiveActivityLogicalBinding, RuleName, sendPort, channel.Name, activity.Name, model.Name);

                                    // TODO: Need to be subscribing to response messages from send port here, potentially in a following
                                    // correlation set scenario.

                                    // Add the send port name to the channel properties (if it doesn't already exist)
                                    if (!channel.Properties.ContainsKey(WorkflowModelConstants.PropertyLogicalBindingPort))
                                    {
                                        channel.Properties.Add(WorkflowModelConstants.PropertyLogicalBindingPort, sendPort);
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogError(string.Format(CultureInfo.CurrentCulture, ErrorMessages.SourceObjectNotFound, serviceBinding.Key, ModelConstants.ResourceServiceBinding));
                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.SourceObjectNotFound, serviceBinding.Key, ModelConstants.ResourceServiceBinding)));
                        }
                    }
                    else
                    {
                        _logger.LogError(string.Format(CultureInfo.CurrentCulture, ErrorMessages.ServiceMissingInBindingFile, model.Name));
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.ServiceMissingInBindingFile, model.Name)));
                    }
                }

                // There may also be a filter on the receive shape
                if (activity.Properties.ContainsKey(WorkflowModelConstants.PropertySubscriptionFilter))
                {
                    // Create an overall OR filter group to represent the OR operator between each
                    // predicate groups in the receive activity.  The expressions in each predicate group
                    // are represented by the AND operator.  A predicate group may only contain a single
                    // expression.
                    foreach (var predicateGroup in (List<List<string>>)activity.Properties[WorkflowModelConstants.PropertySubscriptionFilter])
                    {
                        foreach (var expression in predicateGroup)
                        {
                            filterGroup.Filters.Add(new Filter() { FilterExpression = expression });
                        }
                    }
                }
                else
                {
                    // Set up a simple MessageType subscription filter for the incoming message (if set).
                    // If the message doesn't contain a message type, allow any message in.
                    if (message.MessageType != null)
                    {
                        filterGroup.Filters.Add(new Filter() { FilterExpression = $"{ModelConstants.BizTalkMessageType} = '{message.MessageType}'" });
                    }
                }

                // TODO: Check correlation sets to determine whether filters are needed for convoys.

                // If there are any filters, create the subscription
                if (filterGroup.Filters.Any())
                {
                    // Create subscription
                    subscription = new Subscription(model.Name, topicChannel.TopicName)
                    {
                        IsDurable = true
                    };

                    subscription.Filters.Add(new SubscriptionFilter(filterGroup));

                    // Add subscription to messaging channel
                    topicChannel.Subscriptions.Add(subscription);
                }
            }
            else
            {
                _logger.LogError(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, topicChannelKey));
                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, topicChannelKey)));
            }

            return subscription;
        }

        /// <summary>
        /// Handles binding the send activity with the workflow channel.
        /// </summary>
        /// <param name="sourceApplication">The source application.</param>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="model">The workflow model.</param>
        /// <param name="activity">The workflow activity to bind the channel to.</param>
        private void HandleSendActivity(ResourceItem sourceApplication, Application targetApplication, WorkflowDefinition model, WorkflowActivity activity)
        {
            var messageType = WorkflowMessageType.Request;

            // Get channel name from activity
            string channelName = null;
            string operationName = null;
            var isRoleLink = activity.Properties.ContainsKey(WorkflowModelConstants.PropertyServiceLinkName);
            if (isRoleLink)
            {
                channelName = string.Concat((string)activity.Properties[WorkflowModelConstants.PropertyServiceLinkName], ".", (string)activity.Properties[WorkflowModelConstants.PropertyServiceLinkRoleName]);
            }
            else
            {
                channelName = (string)activity.Properties[WorkflowModelConstants.PropertyPortName];
                operationName = (string)activity.Properties[WorkflowModelConstants.PropertyOperationName];
            }

            // Find channel
            var channel = model.Channels.Where(c => c.Name == channelName && c.OperationName == operationName).SingleOrDefault();
            if (channel != null)
            {
                _logger.LogTrace(TraceMessages.BindingSendActivity, RuleName, channel.Name, activity.Name);

                // If it's a service linked send activity, get port type from the activity properties
                var portTypeName = channel.Type;
                if (isRoleLink)
                {
                    portTypeName = (string)activity.Properties[WorkflowModelConstants.PropertyServiceLinkPortTypeName];
                }

                // Find port types to find operations and determine message exchange pattern for channel
                var portTypes = Model.FindResourcesByType(ModelConstants.ResourcePortType);
                if (portTypes != null && portTypes.Any())
                {
                    foreach (var portType in portTypes)
                    {
                        // Combine name with module to get a fully qualified type name
                        var parentModule = portType.FindRelatedResourcesByType(Model, ResourceRelationshipType.Parent, ModelConstants.ResourceModule).SingleOrDefault();
                        if (parentModule != null)
                        {
                            var typeName = string.Concat(parentModule.Name, ".", portType.Name);
                            if (typeName == portTypeName)
                            {
                                // Found port type, now match up operations and messages
                                var operationMessageName = (string)activity.Properties[WorkflowModelConstants.PropertyOperationMessageName];

                                // Get operations on port type
                                var metaModelObject = portType.SourceObject as Element;
                                if (metaModelObject?.Element1 != null && metaModelObject.Element1.Any(e => e.Type == MetaModelConstants.ElementTypeOperationDeclaration))
                                {
                                    var operations = metaModelObject.Element1.Where(e => e.Type == MetaModelConstants.ElementTypeOperationDeclaration);
                                    foreach (var operation in operations)
                                    {
                                        if (operationName == operation.FindPropertyValue(MetaModelConstants.PropertyKeyName))
                                        {
                                            // Get operation type (OneWay or RequestResponse)
                                            var operationType = operation.FindPropertyValue(MetaModelConstants.ElementTypeOperationType);

                                            // Get message references
                                            var messageRefs = operation.Element1?.Where(e => e.Type == MetaModelConstants.ElementTypeMessageRef).ToArray();
                                            if (messageRefs != null && messageRefs.Any())
                                            {
                                                var matched = false;
                                                foreach (var messageRef in messageRefs)
                                                {
                                                    if (operationMessageName == messageRef.FindPropertyValue(MetaModelConstants.PropertyKeyName))
                                                    {
                                                        // What type of message is it?
                                                        switch (messageRef.ParentLink)
                                                        {
                                                            case MetaModelConstants.ParentLinkOperationRequestMessage:

                                                                if (operationType == "OneWay")
                                                                {
                                                                    // The send activity is the request (to send request) of a one-way port
                                                                    messageType = WorkflowMessageType.Request;
                                                                    channel.Direction = MessageExchangePattern.Send;
                                                                }
                                                                else
                                                                {
                                                                    // The send activity is the request (to initiate request) of a request-response port
                                                                    messageType = WorkflowMessageType.Request;
                                                                    channel.Direction = MessageExchangePattern.RequestReply;
                                                                }

                                                                matched = true;
                                                                break;

                                                            case MetaModelConstants.ParentLinkOperationResponseMessage:

                                                                // The send activity is the response (to reply with response) of a request-response port
                                                                messageType = WorkflowMessageType.Response;
                                                                channel.Direction = MessageExchangePattern.ReceiveResponse;

                                                                matched = true;
                                                                break;

                                                            case MetaModelConstants.ParentLinkOperationFaultMessage:

                                                                // The send activity is the fault response (to reply with response) of a request-response port
                                                                messageType = WorkflowMessageType.Fault;
                                                                channel.Direction = MessageExchangePattern.ReceiveResponse;

                                                                matched = true;
                                                                break;
                                                        }
                                                    }
                                                }

                                                if (!matched)
                                                {
                                                    // Should never get here, but if we do, something wrong with metamodel
                                                    _logger.LogError(ErrorMessages.PortTypeOperationMustHaveMatchingMessage, portTypeName, operationName, operationMessageName, activity.Name);
                                                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.PortTypeOperationMustHaveMatchingMessage, portTypeName, operationName, operationMessageName, activity.Name)));
                                                }
                                            }
                                            else
                                            {
                                                _logger.LogError(ErrorMessages.PortTypeOperationMustHaveMessageReferences, portTypeName, operationName);
                                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.PortTypeOperationMustHaveMessageReferences, portTypeName, operationName)));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    _logger.LogError(ErrorMessages.PortTypeMustHaveOperations, operationName);
                                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.PortTypeMustHaveOperations, operationName)));
                                }
                            }
                        }
                        else
                        {
                            _logger.LogError(ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceModule, ResourceRelationshipType.Parent, portType.Key);
                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindFindRelatedResourceByType, ModelConstants.ResourceModule, ResourceRelationshipType.Parent, portType.Key)));
                        }
                    }
                }

                // Find message
                var message = FindMessage(model, activity.Properties[WorkflowModelConstants.PropertyMessageName].ToString());
                if (message != null)
                {
                    // Set type
                    message.WorkflowMessageType = messageType;

                    // Assign to channel 'Out' message (if it doesn't already exist)
                    if (!channel.MessagesOut.Any(m => m.Name == message.Name))
                    {
                        channel.MessagesOut.Add(message);
                    }

                    // Build property promotions for activity
                    BuildPropertyPromotions(sourceApplication, targetApplication, model, activity, channel, message);
                }

                // Set relationship
                activity.Properties.Add(WorkflowModelConstants.PropertyWorkflowChannel, channel.Key);
            }
            else
            {
                if (isRoleLink)
                {
                    _logger.LogError(ErrorMessages.UnableToFindWorkflowChannel, activity.Properties[WorkflowModelConstants.PropertyServiceLinkName], activity.Name);
                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindWorkflowChannel, activity.Properties[WorkflowModelConstants.PropertyServiceLinkName], activity.Name)));
                }
                else
                {
                    _logger.LogError(ErrorMessages.UnableToFindWorkflowChannel, activity.Properties[WorkflowModelConstants.PropertyPortName], activity.Name);
                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindWorkflowChannel, activity.Properties[WorkflowModelConstants.PropertyPortName], activity.Name)));
                }
            }
        }

        /// <summary>
        /// Builds the promoted properties for the specified send activity and attaches them to the
        /// activity (message properties) and the workflow channel (routing properties).
        /// </summary>
        /// <param name="sourceApplication">The source application.</param>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="model">The workflow model.</param>
        /// <param name="activity">The receive activity.</param>
        /// <param name="channel">The channel on which the message is received.</param>
        /// <param name="message">The message being received.</param>
        private void BuildPropertyPromotions(ResourceItem sourceApplication, Application targetApplication, WorkflowDefinition model, WorkflowActivity activity, WorkflowChannel channel, WorkflowMessage message)
        {
            // Determine if properties are needed to be promoted for a correlation set
            if (activity.Properties.ContainsKey(WorkflowModelConstants.PropertyInitializesCorrelation) || activity.Properties.ContainsKey(WorkflowModelConstants.PropertyFollowsCorrelation))
            {
                // TODO: Set properties on the message header to be promoted based on the properties
                // that make up the correlation type.
            }

            // Create dictionaries for message properties and routing properties
            var messageProperties = new Dictionary<string, object>();
            var requestMessageProperties = new Dictionary<string, object>();
            var routingProperties = new Dictionary<string, object>();

            // Check to see if the send activity has a logical or physical binding
            if (channel.Properties.Any(p => p.Key == WorkflowModelConstants.PropertyBinding && ((string)p.Value == WorkflowModelConstants.PropertyValueLogicalBinding || (string)p.Value == WorkflowModelConstants.PropertyValuePhysicalBinding)))
            {
                // Need to find the equivalent 'messaging port' to this 'logical port' in the BizTalk bindings
                var serviceBinding = Model.FindResourcesByType(ModelConstants.ResourceServiceBinding).Where(s => s.Name == model.Name).SingleOrDefault();
                if (serviceBinding != null)
                {
                    if (serviceBinding.SourceObject is ServiceRef source)
                    {
                        // Check direction of channel
                        if (channel.Direction == MessageExchangePattern.Send || channel.Direction == MessageExchangePattern.FireForget || channel.Direction == MessageExchangePattern.RequestReply)
                        {
                            // It's a Request send activity
                            var sendPort = source.Ports?.Where(p => p.Name == channel.Name && p.SendPortRef != null).Select(p => p.SendPortRef.Name).SingleOrDefault();
                            if (sendPort != null)
                            {
                                _logger.LogTrace(TraceMessages.FoundPortForSendActivityLogicalBinding, RuleName, sendPort, channel.Name, activity.Name, model.Name);

                                // Find send port
                                var sendPortObject = Model.FindResourcesByType(ModelConstants.ResourceSendPort).Where(s => s.Name == sendPort).SingleOrDefault();
                                if (sendPortObject != null)
                                {
                                    var sendPortSource = (SendPort)sendPortObject.SourceObject;

                                    // Add message and routing properties
                                    if (!sendPortSource.IsStatic)
                                    {
                                        messageProperties.Add(ModelConstants.BizTalkSpTransportId, $"{targetApplication.Name.FormatKey()}.{sendPort.FormatKey()}");
                                        routingProperties.Add(ModelConstants.BizTalkSpTransportId, ModelConstants.BizTalkSpTransportId);
                                    }

                                    if (sendPortSource.PrimaryTransport != null && !string.IsNullOrEmpty(sendPortSource.PrimaryTransport.TransportType?.Name))
                                    {
                                        messageProperties.Add(ModelConstants.BizTalkSpTransportId, $"{targetApplication.Name.FormatKey()}.{sendPort.FormatKey()}.{sendPortSource.PrimaryTransport.TransportType.Name}");
                                        messageProperties.Add(ModelConstants.BizTalkOutboundTransportLocation, sendPortSource.PrimaryTransport.Address);
                                        messageProperties.Add(ModelConstants.BizTalkOutboundTransportType, sendPortSource.PrimaryTransport.TransportType.Name);

                                        routingProperties.Add(ModelConstants.BizTalkSpTransportId, ModelConstants.BizTalkSpTransportId);
                                        routingProperties.Add(ModelConstants.BizTalkOutboundTransportLocation, ModelConstants.BizTalkOutboundTransportLocation);
                                        routingProperties.Add(ModelConstants.BizTalkOutboundTransportType, ModelConstants.BizTalkOutboundTransportType);
                                    }

                                    if (sendPortSource.SecondaryTransport != null && !string.IsNullOrEmpty(sendPortSource.SecondaryTransport.TransportType?.Name))
                                    {
                                        messageProperties.Add(ModelConstants.BizTalkSpTransportBackupId, $"{targetApplication.Name.FormatKey()}.{sendPort.FormatKey()}.{sendPortSource.SecondaryTransport.TransportType.Name}");
                                        routingProperties.Add(ModelConstants.BizTalkSpTransportBackupId, ModelConstants.BizTalkSpTransportBackupId);
                                    }

                                    messageProperties.Add(ModelConstants.BizTalkSpId, $"{targetApplication.Name.FormatKey()}.{sendPort.FormatKey()}");
                                    routingProperties.Add(ModelConstants.BizTalkSpId, ModelConstants.BizTalkSpId);

                                    // Add the send port name to the channel properties (if it doesn't already exist)
                                    if (!channel.Properties.ContainsKey(WorkflowModelConstants.PropertyLogicalBindingPort))
                                    {
                                        channel.Properties.Add(WorkflowModelConstants.PropertyLogicalBindingPort, sendPort);
                                    }
                                }
                                else
                                {
                                    _logger.LogError(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindRelatedResourceByTypeAndName, ModelConstants.ResourceSendPort, sendPort, ResourceRelationshipType.Child, sourceApplication.Key));
                                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindRelatedResourceByTypeAndName, ModelConstants.ResourceSendPort, sendPort, ResourceRelationshipType.Child, sourceApplication.Key)));
                                }
                            }
                            else
                            {
                                var sendPortGroup = source.Ports?.Where(p => p.Name == channel.Name && p.DistributionListRef != null).Select(p => p.DistributionListRef.Name).SingleOrDefault();
                                if (sendPortGroup != null)
                                {
                                    _logger.LogTrace(TraceMessages.FoundPortForSendActivityLogicalBinding, RuleName, sendPortGroup, channel.Name, activity.Name, model.Name);

                                    // Add message properties to activity
                                    messageProperties.Add(ModelConstants.BizTalkSpGroupId, $"{targetApplication.Name.FormatKey()}.{sendPortGroup.FormatKey()}");

                                    // Add routing properties to channel
                                    routingProperties.Add(ModelConstants.BizTalkSpGroupId, ModelConstants.BizTalkSpGroupId);

                                    // Add the send port group name to the channel properties (if it doesn't already exist)
                                    if (!channel.Properties.ContainsKey(WorkflowModelConstants.PropertyLogicalBindingPort))
                                    {
                                        channel.Properties.Add(WorkflowModelConstants.PropertyLogicalBindingPort, sendPortGroup);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // It's a Response send activity
                            var receivePort = source.Ports?.Where(p => p.Name == channel.Name && p.ReceivePortRef != null).Select(p => p.ReceivePortRef.Name).SingleOrDefault();
                            if (receivePort != null)
                            {
                                _logger.LogTrace(TraceMessages.FoundPortForReceiveActivityLogicalBinding, RuleName, receivePort, channel.Name, activity.Name, model.Name);

                                // Add message properties to activity
                                messageProperties.Add(ModelConstants.BizTalkAckReceivePortId, $"{targetApplication.Name.FormatKey()}.{receivePort.FormatKey()}");

                                // Add request message properties to activity (properties to copy from request)
                                requestMessageProperties.Add(ModelConstants.CorrelationId, ModelConstants.MessageId);

                                // Add routing properties to channel
                                routingProperties.Add(ModelConstants.BizTalkAckReceivePortId, ModelConstants.BizTalkAckReceivePortId);

                                // Add the receive port name to the channel properties (if it doesn't already exist)
                                if (!channel.Properties.ContainsKey(WorkflowModelConstants.PropertyLogicalBindingPort))
                                {
                                    channel.Properties.Add(WorkflowModelConstants.PropertyLogicalBindingPort, receivePort);
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError(string.Format(CultureInfo.CurrentCulture, ErrorMessages.SourceObjectNotFound, serviceBinding.Key, ModelConstants.ResourceServiceBinding));
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.SourceObjectNotFound, serviceBinding.Key, ModelConstants.ResourceServiceBinding)));
                    }
                }
                else
                {
                    _logger.LogError(string.Format(CultureInfo.CurrentCulture, ErrorMessages.ServiceMissingInBindingFile, model.Name));
                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.ServiceMissingInBindingFile, model.Name)));
                }
            }

            // If the message being sent has a message type, add that to the properties
            if (message.MessageType != null)
            {
                // Add the literal value as a message property, then promote it to the routing properties from the message properties
                messageProperties.Add(ModelConstants.BizTalkMessageType, message.MessageType);
                routingProperties.Add(ModelConstants.BizTalkMessageType, ModelConstants.BizTalkMessageType);
            }

            // Add message and routing properties
            activity.Properties.Add(ModelConstants.MessageProperties, messageProperties);
            activity.Properties.Add(ModelConstants.RequestMessageProperties, requestMessageProperties);

            if (!channel.Properties.ContainsKey(ModelConstants.RoutingProperties))
            {
                channel.Properties.Add(ModelConstants.RoutingProperties, routingProperties);
            }
        }

        /// <summary>
        /// Handles binding the invoke workflow activity with the workflow channel.
        /// </summary>
        /// <param name="sourceApplication">The source application.</param>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="model">The workflow model.</param>
        /// <param name="activity">The workflow activity to bind the channel to.</param>
        private void HandleInvokeWorkflowActivity(ResourceItem sourceApplication, Application targetApplication, WorkflowDefinition model, WorkflowActivity activity)
        {
            // Find channel that routes to the invoked workflow
            var channel = model.Channels.Where(c => c.Name == activity.Properties[WorkflowModelConstants.PropertyInvokee].ToString()).SingleOrDefault();
            if (channel == null)
            {
                // Doesn't exist yet, create channel (to be used to bind to messaging trigger channel for routing to workflow).
                // This channel always represents an outgoing operation but no messages (in reality, we would have to wrap
                // parameters inside a MessageBus message).
                channel = new WorkflowChannel()
                {
                    Name = activity.Properties[WorkflowModelConstants.PropertyInvokee].ToString(),
                    Key = string.Concat(model.Key, ".", activity.Properties[WorkflowModelConstants.PropertyInvokee].ToString()).FormatKey(),
                    Type = WorkflowModelConstants.ChannelTypeTrigger,
                    Direction = Convert.ToBoolean(activity.Properties[WorkflowModelConstants.PropertyIsAsync], CultureInfo.InvariantCulture) ? MessageExchangePattern.FireForget : MessageExchangePattern.RequestReply
                };

                _logger.LogTrace(TraceMessages.CreatedWorkflowChannel, RuleName, channel.Name, channel.Type);

                model.Channels.Add(channel);
            }

            _logger.LogTrace(TraceMessages.BindingInvokeWorkflowActivity, RuleName, channel.Name, activity.Name);

            // Set relationship
            activity.Properties.Add(WorkflowModelConstants.PropertyWorkflowChannel, channel.Key);

            // Add parameters to the channel
            if (activity.Properties.ContainsKey(WorkflowModelConstants.PropertyWorkflowParameters))
            {
                // Only add if not present.
                if (!channel.Properties.ContainsKey(WorkflowModelConstants.PropertyWorkflowParameters))
                {
                    channel.Properties.Add(WorkflowModelConstants.PropertyWorkflowParameters, activity.Properties[WorkflowModelConstants.PropertyWorkflowParameters]);
                }
            }
        }

        /// <summary>
        /// Handles binding the suspend activity with the workflow channel.
        /// </summary>
        /// <param name="sourceApplication">The source application.</param>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="model">The workflow model.</param>
        /// <param name="activity">The workflow activity to bind the channel to.</param>
        private void HandleSuspendActivity(ResourceItem sourceApplication, Application targetApplication, WorkflowDefinition model, WorkflowActivity activity)
        {
            // Find channel that routes to the suspend queue
            var channel = model.Channels.Where(c => c.Name == MigrationTargetResources.SuspendQueueName).SingleOrDefault();
            if (channel == null)
            {
                // Doesn't exist yet, create channel (to be used to bind to messaging trigger channel for routing to workflow).
                // This channel always represents an outgoing operation but no messages (in reality, we would have to wrap
                // parameters inside a MessageBus message).
                channel = new WorkflowChannel()
                {
                    Name = MigrationTargetResources.SuspendQueueName,
                    Key = string.Concat(model.Key, ".", MigrationTargetResources.SuspendQueueName).FormatKey(),
                    Type = WorkflowModelConstants.ChannelTypePublishSubscribe,
                    Direction = MessageExchangePattern.FireForget
                };

                _logger.LogTrace(TraceMessages.CreatedWorkflowChannel, RuleName, channel.Name, channel.Type);

                model.Channels.Add(channel);
            }

            _logger.LogTrace(TraceMessages.BindingSuspendActivity, RuleName, channel.Name, activity.Name);

            // Set relationship
            activity.Properties.Add(WorkflowModelConstants.PropertyWorkflowChannel, channel.Key);
        }

        /// <summary>
        /// Handles binding the terminate activity with the workflow channel.
        /// </summary>
        /// <param name="sourceApplication">The source application.</param>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="model">The workflow model.</param>
        /// <param name="activity">The workflow activity to bind the channel to.</param>
        private void HandleTerminateActivity(ResourceItem sourceApplication, Application targetApplication, WorkflowDefinition model, WorkflowActivity activity)
        {
            // Find channel
            var channel = model.Channels.Where(c => c.Name == MigrationTargetResources.SuspendQueueName).SingleOrDefault();
            if (channel == null)
            {
                // Doesn't exist yet, create channel (to be used to bind to messaging trigger channel for routing to workflow).
                // This channel always represents an outgoing operation with an outgoing message.
                channel = new WorkflowChannel()
                {
                    Name = MigrationTargetResources.SuspendQueueName,
                    Key = string.Concat(model.Key, ".", MigrationTargetResources.SuspendQueueName).FormatKey(),
                    Type = WorkflowModelConstants.ChannelTypePublishSubscribe,
                    Direction = MessageExchangePattern.FireForget
                };

                _logger.LogTrace(TraceMessages.CreatedWorkflowChannel, RuleName, channel.Name, channel.Type);

                model.Channels.Add(channel);
            }

            _logger.LogTrace(TraceMessages.BindingTerminateActivity, RuleName, channel.Name, activity.Name);

            // Set relationship
            activity.Properties.Add(WorkflowModelConstants.PropertyWorkflowChannel, channel.Key);
        }

        /// <summary>
        /// Binds correlation variables to the activities that are related.
        /// </summary>
        /// <param name="orchestration">The orchestration.</param>
        /// <param name="model">The workflow model.</param>
        /// <param name="container">The workflow activity container in which to find bindable activities.</param>
        private void BindCorrelationVariables(ResourceItem orchestration, WorkflowDefinition model, WorkflowActivityContainer container)
        {
            var correlationVariables = container.Variables.Where(v => v is WorkflowCorrelationVariable).Select(v => (WorkflowCorrelationVariable)v);
            if (correlationVariables != null && correlationVariables.Any())
            {
                foreach (var correlationVariable in correlationVariables)
                {
                    _logger.LogTrace(TraceMessages.BindingCorrelationVariable, RuleName, correlationVariable.Name, orchestration.Name);

                    if (correlationVariable.Properties.ContainsKey(WorkflowModelConstants.PropertyActivityReferences))
                    {
                        // Find related receive or send activities for the correlation
                        foreach (var kvp in (Dictionary<string, bool>)correlationVariable.Properties[WorkflowModelConstants.PropertyActivityReferences])
                        {
                            var relatedActivityObject = FindActivityByObjectId(model, kvp.Key);
                            if (relatedActivityObject != null)
                            {
                                // Set whether the related activity initializes the correlation properties for other activities
                                // to correlate messages from.
                                if (kvp.Value)
                                {
                                    // Initializes correlation
                                    relatedActivityObject.Properties.Add(WorkflowModelConstants.PropertyInitializesCorrelation, correlationVariable.Key);
                                    correlationVariable.InitializingActivity = relatedActivityObject;
                                }
                                else
                                {
                                    // Follows correlation
                                    relatedActivityObject.Properties.Add(WorkflowModelConstants.PropertyFollowsCorrelation, correlationVariable.Key);
                                    correlationVariable.FollowingActivities.Add(relatedActivityObject);
                                }

                                // Find message
                                var relatedMessage = FindMessage(model, relatedActivityObject.Properties[WorkflowModelConstants.PropertyMessageName].ToString());
                                if (relatedMessage != null)
                                {
                                    if (correlationVariable.CorrelationProperties.Any())
                                    {
                                        foreach (var correlationProperty in correlationVariable.CorrelationProperties)
                                        {
                                            if (!relatedMessage.CorrelationProperties.Contains(correlationProperty.Type))
                                            {
                                                relatedMessage.CorrelationProperties.Add(correlationProperty.Type);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogDebug(TraceMessages.CorrelationVariableNotBound, RuleName, correlationVariable.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Finds an activity in the workflow model with the specified object ID.
        /// </summary>
        /// <param name="activity">The workflow activity to check.</param>
        /// <param name="objectId">The object ID.</param>
        /// <returns>The found workflow activity or null if not found.</returns>
        private static WorkflowActivity FindActivityByObjectId(WorkflowActivity activity, string objectId)
        {
            if (activity.Properties.ContainsKey(WorkflowModelConstants.PropertyObjectId) && activity.Properties[WorkflowModelConstants.PropertyObjectId].ToString() == objectId)
            {
                return activity;
            }
            else
            {
                if (activity is WorkflowActivityContainer activityContainer)
                {
                    foreach (var childActivity in activityContainer.Activities)
                    {
                        var foundObject = FindActivityByObjectId(childActivity, objectId);
                        if (foundObject != null)
                        {
                            return foundObject;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks an activity container for the specified message.
        /// </summary>
        /// <param name="container">The container to check for the message.</param>
        /// <param name="messageName">The message name.</param>
        /// <returns>The found message or null if not found.</returns>
        private static WorkflowMessage FindMessage(WorkflowActivityContainer container, string messageName)
        {
            var message = container.Messages.Where(m => m.Name == messageName).SingleOrDefault();
            if (message == null)
            {
                // Recurse into activities looking for more containers
                var childContainers = container.Activities.Where(a => a is WorkflowActivityContainer).Select(a => (WorkflowActivityContainer)a);
                if (childContainers.Any())
                {
                    foreach (var childContainer in childContainers)
                    {
                        message = FindMessage(childContainer, messageName);
                        if (message != null)
                        {
                            break;
                        }
                    }
                }
            }

            return message;
        }

        /// <summary>
        /// Creates a process manager.
        /// </summary>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="workflowModel">The workflow model.</param>
        /// <returns>An instance of a process manager.</returns>
        private ProcessManager CreateProcessManager(Application targetApplication, WorkflowDefinition workflowModel)
        {
            // Build key parts
            var applicationName = targetApplication.Name.FormatKey();
            var processManagerName = workflowModel.Name.FormatKey();

            // Create process manager intermediary
            var processManager = new ProcessManager(workflowModel.Name)
            {
                Activator = workflowModel.Channels.Any(c => c.Activator),
                Description = workflowModel.Name,
                Key = $"{Model.MigrationTarget.MessageBus.Key}:{applicationName}:{processManagerName}",
                WorkflowModel = workflowModel
            };

            processManager.Properties.Add(ModelConstants.TypeName, workflowModel.Name);

            // Set resource map key
            processManager.ResourceMapKey = processManager.Activator ?
                $"activatableProcessManager{applicationName.Replace(".", "-").Replace("/", "-").Replace(":", "-")}{processManagerName.Replace(".", "-").Replace("/", "-").Replace(":", "-")}" :
                $"invokableProcessManager{applicationName.Replace(".", "-").Replace("/", "-").Replace(":", "-")}{processManagerName.Replace(".", "-").Replace("/", "-").Replace(":", "-")}";

            // We will do our best in conversion to output a workflow, but it may contain placeholders
            processManager.Rating = ConversionRating.PartialConversion;

            // Set scenario on the process manager if it is an activatable process manager
            if (processManager.Activator)
            {
                var scenarioName = $"{targetApplication.Name.FormatKey()}.{workflowModel.Name.FormatKey()}";
                processManager.Properties.Add(ModelConstants.ScenarioName, scenarioName);
            }

            // Set scenario step name
            processManager.Properties.Add(ModelConstants.ScenarioStepName, workflowModel.Name);

            // Add configuration properties
            var config = new Dictionary<string, object>()
            {
                { ModelConstants.FailedMessageRouting, true }
            };

            processManager.Properties.Add(ModelConstants.ConfigurationEntry, config);

            // Add routing properties
            processManager.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());

            return processManager;
        }

        /// <summary>
        /// Binds the invoking and invoked workflows.
        /// </summary>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="model">The workflow model.</param>
        /// <param name="container">The workflow activity container in which to find bindable activities.</param>
        private void BindInvokedWorkflows(WorkflowDefinition model, WorkflowActivityContainer container)
        {
            if (container.Activities.Any())
            {
                // Get process managers across all target applications
                var processManagers = Model.MigrationTarget.MessageBus?.Applications.SelectMany(a => a.Intermediaries.Where(i => i is ProcessManager)).Select(i => (ProcessManager)i);
                if (processManagers != null && processManagers.Any())
                {
                    foreach (var activity in container.Activities)
                    {
                        // Is it an InvokeWorkflow activity?
                        if (activity.Type == WorkflowModelConstants.ActivityTypeInvokeWorkflow)
                        {
                            // Get invoked workflow name
                            var invokedWorkflowName = (string)activity.Properties[WorkflowModelConstants.PropertyInvokee];

                            _logger.LogTrace(TraceMessages.BindingInvokedWorkflow, RuleName, invokedWorkflowName, model.Name);

                            // Find invoked workflow
                            var invokedWorkflow = processManagers.Where(p => p.WorkflowModel?.Name == invokedWorkflowName).SingleOrDefault();
                            if (invokedWorkflow != null)
                            {
                                // Find channel that routes to the invoked workflow
                                var channel = model.Channels.Where(c => c.Name == invokedWorkflowName).SingleOrDefault();
                                if (channel != null)
                                {
                                    // Only add if it doesn't already exist
                                    if (!invokedWorkflow.WorkflowModel.Channels.Any(c => c.Name == invokedWorkflowName))
                                    {
                                        // Create channel and attach to the invoked workflow.
                                        // We could create the channel by checking the IsInvokable (True) property on the ServiceDeclaration
                                        // in the HandleServiceDeclaration method, but following the link from the invoker allows us to set
                                        // direction correctly and also set the parameters passed by the invoker.  Parameters in the invoked
                                        // workflow could be worked out by looking at the VariableDeclaration elements in the ServiceBody, but
                                        // this guarantees we can build a route by following the invoker/invoked links.
                                        channel = new WorkflowChannel()
                                        {
                                            Activator = true,
                                            Name = invokedWorkflowName,
                                            Key = string.Concat(model.Key, ".", invokedWorkflowName).FormatKey(),
                                            Type = WorkflowModelConstants.ChannelTypeTrigger,
                                            Direction = Convert.ToBoolean(activity.Properties[WorkflowModelConstants.PropertyIsAsync], CultureInfo.InvariantCulture) ? MessageExchangePattern.Accept : MessageExchangePattern.ReceiveResponse
                                        };

                                        _logger.LogTrace(TraceMessages.CreatedWorkflowChannel, RuleName, channel.Name, channel.Type, invokedWorkflowName);

                                        invokedWorkflow.WorkflowModel.Channels.Add(channel);

                                        // Add parameters from the invoking workflow to the invoked workflow channel
                                        if (activity.Properties.ContainsKey(WorkflowModelConstants.PropertyWorkflowParameters))
                                        {
                                            channel.Properties.Add(WorkflowModelConstants.PropertyWorkflowParameters, activity.Properties[WorkflowModelConstants.PropertyWorkflowParameters]);
                                        }
                                    }
                                }
                                else
                                {
                                    _logger.LogError(ErrorMessages.UnableToFindWorkflowChannel, invokedWorkflowName, activity.Name);
                                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindWorkflowChannel, invokedWorkflowName, activity.Name)));
                                }
                            }
                            else
                            {
                                _logger.LogError(ErrorMessages.UnableToFindWorkflow, invokedWorkflowName, model.Name);
                                Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindWorkflow, invokedWorkflowName, model.Name)));
                            }
                        }
                    }
                }
            }

            // Recurse
            var childContainers = container.Activities.Where(a => a is WorkflowActivityContainer).Select(a => (WorkflowActivityContainer)a);
            if (childContainers.Any())
            {
                foreach (var childContainer in childContainers)
                {
                    BindInvokedWorkflows(model, childContainer);
                }
            }
        }

        /// <summary>
        /// Analyze the process manager and build the scenario in the target model.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A process manager intermediary in the target model is a representation of a BizTalk orchestration.  This
        /// object has an associated workflow model that has previously been parsed from an orchestration metamodel.
        /// </para>
        /// <para>
        /// If a workflow receives messages or sends messages for a particular operation, these are represented as
        /// workflow channels that have a direction based on whether it's a one-way or two-way operation and whether
        /// the message is received first or sent first.
        /// </para>
        /// <para>
        /// The basic rules are relatively simple.  In a BizTalk orchestration, messages received and sent by an
        /// orchestration traverse through the Message Box.  Therefore, this analyzer rule will connect the workflow
        /// channels with the topic channel representing the Message Box.
        /// </para>
        /// <para>
        /// For suspend and terminate activities, these are represented as workflow channel that are to be connected
        /// with the Suspend Queue topic channel, to allow subscribing as part of a failure handling process.
        /// </para>
        /// <para>
        /// For called (sync) and started (async) workflows, these are part of the route.  These are also
        /// represented as workflow channels where an invoke workflow activity exists in the workflow model, with an
        /// equivalent channel to act as the triggered input for an invoked workflow.
        /// </para>
        /// </remarks>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="processManager">The activatable process manager.</param>
        private void BuildRoute(Application targetApplication, ProcessManager processManager)
        {
            _logger.LogDebug(TraceMessages.BuildingWorkflowRoute, RuleName, processManager.Name, targetApplication.Name);

            // Build keys to find topic channels in target model
            var messageBoxTopicChannelKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}:{ModelConstants.MessageBoxLeafKey}";
            var suspendQueueTopicChannelKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}:{ModelConstants.SuspendQueueLeafKey}";

            // Bind all of the channels in the workflow, according to their type
            if (processManager.WorkflowModel != null && processManager.WorkflowModel.Channels.Any())
            {
                // Find topic channels
                var messageBoxTopicChannelObject = Model.FindMessagingObject(messageBoxTopicChannelKey);
                if (messageBoxTopicChannelObject.application != null && messageBoxTopicChannelObject.messagingObject != null)
                {
                    var suspendQueueTopicChannelObject = Model.FindMessagingObject(suspendQueueTopicChannelKey);
                    if (suspendQueueTopicChannelObject.application != null && suspendQueueTopicChannelObject.messagingObject != null)
                    {
                        // Set activatable process manager as the start of the route bound to the message box
                        var route = new List<MessagingObject>
                        {
                            messageBoxTopicChannelObject.messagingObject,
                            processManager
                        };

                        // Set input messaging channel to message box channel
                        processManager.InputChannelKeyRefs.Add(messageBoxTopicChannelObject.messagingObject.Key);

                        // Start binding channels from activatable process manager and add each invoked process manager to the route
                        BindProcessManagerChannels(targetApplication, processManager, (Channel)messageBoxTopicChannelObject.messagingObject, (Channel)suspendQueueTopicChannelObject.messagingObject, null, route);

                        var keyPrefix = $"{Model.MigrationTarget.MessageBus.Key}:{targetApplication.Name}:{processManager.Name}";
                        var lastIntermediary = (Intermediary)route.Last();

                        // End the route with the routing slip router
                        var routingSlipRouter = CreateRoutingSlipRouterIntermediary(keyPrefix, lastIntermediary.Name, MigrationTargetResources.EndRoute);

                        var toRouterChannel = new TriggerChannel(MigrationTargetResources.TriggerChannelName)
                        {
                            Description = string.Format(CultureInfo.CurrentCulture, MigrationTargetResources.TriggerChannelDescription, routingSlipRouter.Name),
                            Key = $"{keyPrefix}:{ModelConstants.TriggerChannelLeafKey}:{lastIntermediary.Name}-{routingSlipRouter.Name}".FormatKey(),
                            Rating = ConversionRating.FullConversion
                        };

                        // Label the channel appropriately
                        toRouterChannel.Properties.Add(ModelConstants.RouteLabel, MigrationTargetResources.RouteFromChannelLabel);

                        // Set output messaging channel of last intermediary in route to trigger channel
                        if (!lastIntermediary.OutputChannelKeyRefs.Contains(toRouterChannel.Key))
                        {
                            lastIntermediary.OutputChannelKeyRefs.Add(toRouterChannel.Key);
                        }

                        // Set input messaging channel for routing slip router to trigger channel
                        if (!routingSlipRouter.InputChannelKeyRefs.Contains(toRouterChannel.Key))
                        {
                            routingSlipRouter.InputChannelKeyRefs.Add(toRouterChannel.Key);
                        }

                        // Add to application and route
                        targetApplication.Channels.Add(toRouterChannel);
                        targetApplication.Intermediaries.Add(routingSlipRouter);

                        route.Add(toRouterChannel);
                        route.Add(routingSlipRouter);

                        _logger.LogDebug(TraceMessages.ProcessManagerRouteBound, RuleName, processManager.Name, route.Count);
                    }
                    else
                    {
                        _logger.LogError(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, suspendQueueTopicChannelKey));
                        Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, suspendQueueTopicChannelKey)));
                    }
                }
                else
                {
                    _logger.LogError(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, messageBoxTopicChannelKey));
                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, messageBoxTopicChannelKey)));
                }
            }
        }

        /// <summary>
        /// Binds workflow channels to messaging channels for process manager.
        /// </summary>
        /// <remarks>
        /// For every invoked workflow, this method is recursed into in order to bind channels in
        /// invoked workflow.
        /// </remarks>
        /// <param name="targetApplication">The target application.</param>
        /// <param name="processManager">The process manager.</param>
        /// <param name="messageBoxChannel">The message box channel.</param>
        /// <param name="suspendQueueChannel">The suspend queue channel.</param>
        /// <param name="invokingTriggerChannel">The invoker of the <paramref name="processManager"/>, if applicable.</param>
        /// <param name="route">The route to add messaging objects to.</param>
        private void BindProcessManagerChannels(Application targetApplication, ProcessManager processManager, Channel messageBoxChannel, Channel suspendQueueChannel, Channel invokingTriggerChannel, IList<MessagingObject> route)
        {
            var keyPrefix = $"{Model.MigrationTarget.MessageBus.Key}:{targetApplication.Name}:{processManager.Name}";

            // Bind all of the channels in the workflow, according to their type
            if (processManager.WorkflowModel != null && processManager.WorkflowModel.Channels.Any())
            {
                // Determine type of channel, then bind messaging channel key
                foreach (var channel in processManager.WorkflowModel.Channels)
                {
                    switch (channel.Type)
                    {
                        case WorkflowModelConstants.ChannelTypeTrigger:

                            // Used for invoking workflows / being invoked by a workflow
                            if (channel.Direction == MessageExchangePattern.Accept ||
                                channel.Direction == MessageExchangePattern.Receive ||
                                channel.Direction == MessageExchangePattern.ReceiveResponse)
                            {
                                // Receive (invoked)
                                if (!channel.ChannelKeyRefIn.Contains(invokingTriggerChannel.Key))
                                {
                                    channel.ChannelKeyRefIn.Add(invokingTriggerChannel.Key);
                                }

                                // Set input messaging channel of invoked process manager to trigger channel
                                if (!processManager.InputChannelKeyRefs.Contains(invokingTriggerChannel.Key))
                                {
                                    processManager.InputChannelKeyRefs.Add(invokingTriggerChannel.Key);
                                }

                                _logger.LogTrace(TraceMessages.BoundWorkflowChannel, RuleName, channel.Name, channel.Type, processManager.Name, invokingTriggerChannel.Name);
                            }
                            else
                            {
                                // Send (invoker), this is part of the route as we are invoking a workflow, so add a trigger channel,
                                // routing slip router, another trigger channel to invoke the child workflow.
                                var invokedProcessManager = Model.MigrationTarget.MessageBus?.Applications.SelectMany(a => a.Intermediaries.Where(i => i is ProcessManager p && p.WorkflowModel?.Name == channel.Name)).Select(i => (ProcessManager)i).SingleOrDefault();
                                if (invokedProcessManager != null)
                                {
                                    var routingSlipRouter = CreateRoutingSlipRouterIntermediary(keyPrefix, processManager.Name, invokedProcessManager.Name);

                                    var toRouterChannel = new TriggerChannel(MigrationTargetResources.TriggerChannelName)
                                    {
                                        Description = string.Format(CultureInfo.CurrentCulture, MigrationTargetResources.TriggerChannelDescription, routingSlipRouter.Name),
                                        Key = $"{keyPrefix}:{ModelConstants.TriggerChannelLeafKey}:{processManager.Name}-{routingSlipRouter.Name}".FormatKey(),
                                        Rating = ConversionRating.FullConversion
                                    };

                                    // Label the channel appropriately
                                    toRouterChannel.Properties.Add(ModelConstants.RouteLabel, MigrationTargetResources.RouteFromChannelLabel);

                                    var fromRouterChannel = new TriggerChannel(MigrationTargetResources.TriggerChannelName)
                                    {
                                        Description = string.Format(CultureInfo.CurrentCulture, MigrationTargetResources.TriggerChannelDescription, processManager.Name),
                                        Key = $"{keyPrefix}:{ModelConstants.TriggerChannelLeafKey}:{routingSlipRouter.Name}-{invokedProcessManager.Name}".FormatKey(),
                                        TriggerUrl = $"/routingManager/route/{invokedProcessManager.Properties[ModelConstants.ScenarioStepName]}",
                                        Rating = ConversionRating.FullConversion
                                    };

                                    // Label the channel appropriately
                                    fromRouterChannel.Properties.Add(ModelConstants.RouteLabel, MigrationTargetResources.RouteToChannelLabel);

                                    // Set output messaging channel of this process manager to trigger channel
                                    if (!processManager.OutputChannelKeyRefs.Contains(toRouterChannel.Key))
                                    {
                                        processManager.OutputChannelKeyRefs.Add(toRouterChannel.Key);
                                    }

                                    // Set messaging channels for routing slip router
                                    if (!routingSlipRouter.InputChannelKeyRefs.Contains(toRouterChannel.Key))
                                    {
                                        routingSlipRouter.InputChannelKeyRefs.Add(toRouterChannel.Key);
                                    }

                                    if (!routingSlipRouter.OutputChannelKeyRefs.Contains(fromRouterChannel.Key))
                                    {
                                        routingSlipRouter.OutputChannelKeyRefs.Add(fromRouterChannel.Key);
                                    }

                                    // Send (invoker)
                                    if (!channel.ChannelKeyRefOut.Contains(toRouterChannel.Key))
                                    {
                                        channel.ChannelKeyRefOut.Add(toRouterChannel.Key);
                                    }

                                    // Add to application and route
                                    targetApplication.Channels.Add(toRouterChannel);
                                    targetApplication.Channels.Add(fromRouterChannel);
                                    targetApplication.Intermediaries.Add(routingSlipRouter);

                                    route.Add(toRouterChannel);
                                    route.Add(routingSlipRouter);
                                    route.Add(fromRouterChannel);
                                    route.Add(invokedProcessManager);

                                    // Recurse and bind invoked workflow's channels
                                    BindProcessManagerChannels(targetApplication, invokedProcessManager, messageBoxChannel, suspendQueueChannel, fromRouterChannel, route);

                                    _logger.LogTrace(TraceMessages.BoundWorkflowChannel, RuleName, channel.Name, channel.Type, processManager.Name, fromRouterChannel.Name);
                                }
                                else
                                {
                                    _logger.LogError(ErrorMessages.UnableToFindWorkflow, channel.Name, processManager.Name);
                                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindWorkflow, channel.Name, processManager.Name)));
                                }
                            }

                            break;

                        case WorkflowModelConstants.ChannelTypePublishSubscribe:

                            // Used for suspend and terminate activities
                            if (!channel.ChannelKeyRefOut.Contains(suspendQueueChannel.Key))
                            {
                                channel.ChannelKeyRefOut.Add(suspendQueueChannel.Key);
                            }

                            if (!processManager.OutputChannelKeyRefs.Contains(suspendQueueChannel.Key))
                            {
                                processManager.OutputChannelKeyRefs.Add(suspendQueueChannel.Key);
                            }

                            _logger.LogTrace(TraceMessages.BoundWorkflowChannel, RuleName, channel.Name, channel.Type, processManager.Name, suspendQueueChannel.Name);

                            break;

                        default:

                            // Assume all others are channels attached to the Message Box topic channel
                            if (channel.Direction == MessageExchangePattern.Accept || channel.Direction == MessageExchangePattern.Receive)
                            {
                                // Receive
                                if (!channel.ChannelKeyRefIn.Contains(messageBoxChannel.Key))
                                {
                                    channel.ChannelKeyRefIn.Add(messageBoxChannel.Key);
                                }

                                if (!processManager.InputChannelKeyRefs.Contains(messageBoxChannel.Key))
                                {
                                    processManager.InputChannelKeyRefs.Add(messageBoxChannel.Key);
                                }
                            }
                            else if (channel.Direction == MessageExchangePattern.FireForget || channel.Direction == MessageExchangePattern.Send)
                            {
                                // Send
                                if (!channel.ChannelKeyRefOut.Contains(messageBoxChannel.Key))
                                {
                                    channel.ChannelKeyRefOut.Add(messageBoxChannel.Key);
                                }

                                if (!processManager.OutputChannelKeyRefs.Contains(messageBoxChannel.Key))
                                {
                                    processManager.OutputChannelKeyRefs.Add(messageBoxChannel.Key);
                                }
                            }
                            else
                            {
                                // Two-Way
                                if (!channel.ChannelKeyRefIn.Contains(messageBoxChannel.Key))
                                {
                                    channel.ChannelKeyRefIn.Add(messageBoxChannel.Key);
                                }

                                if (!processManager.InputChannelKeyRefs.Contains(messageBoxChannel.Key))
                                {
                                    processManager.InputChannelKeyRefs.Add(messageBoxChannel.Key);
                                }

                                if (!channel.ChannelKeyRefOut.Contains(messageBoxChannel.Key))
                                {
                                    channel.ChannelKeyRefOut.Add(messageBoxChannel.Key);
                                }

                                if (!processManager.OutputChannelKeyRefs.Contains(messageBoxChannel.Key))
                                {
                                    processManager.OutputChannelKeyRefs.Add(messageBoxChannel.Key);
                                }
                            }

                            _logger.LogTrace(TraceMessages.BoundWorkflowChannel, RuleName, channel.Name, channel.Type, processManager.Name, messageBoxChannel.Name);

                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Creates an intermediary representing a routing slip router, which is used to route
        /// to the next process manager intermediary in the route.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="fromStep">The step this router will route from.</param>
        /// <param name="toStep">The step this router will route to.</param>
        /// <returns>A routing slip router intermediary.</returns>
        private Intermediary CreateRoutingSlipRouterIntermediary(string intermediaryKeyPrefix, string fromStep, string toStep)
        {
            _logger.LogTrace(TraceMessages.CreatingRoutingSlipRouterIntermediary, RuleName, fromStep, toStep);

            // Create an intermediary based on the routing slip router
            var intermediary = new RoutingSlipRouter(MigrationTargetResources.RoutingSlipRouterName)
            {
                Description = string.Format(CultureInfo.CurrentCulture, MigrationTargetResources.RoutingSlipRouterDescription, toStep),
                Key = $"{intermediaryKeyPrefix}:{ModelConstants.RoutingSlipRouterLeafKey}:{fromStep.FormatKey()}-{toStep.FormatKey()}",
                Rating = ConversionRating.FullConversion
            };

            return intermediary;
        }
    }
}
