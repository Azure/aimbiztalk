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
    /// Defines a list of strings for the workflow model.
    /// </summary>
    public static class WorkflowModelConstants
    {
        #region Workflow Activity Types

        /// <summary>
        /// Defines the type of activity group.
        /// </summary>
        public const string ActivityTypeActivityGroup = "ActivityGroup";

        /// <summary>
        /// Defines the type of code expression.
        /// </summary>
        public const string ActivityTypeCodeExpression = "CodeExpression";

        /// <summary>
        /// Defines the type of decision.
        /// </summary>
        public const string ActivityTypeDecision = "Decision";

        /// <summary>
        /// Defines the type of decision branch.
        /// </summary>
        public const string ActivityTypeDecisionBranch = "DecisionBranch";

        /// <summary>
        /// Defines the type of invoke workflow.
        /// </summary>
        public const string ActivityTypeInvokeWorkflow = "InvokeWorkflow";

        /// <summary>
        /// Defines the type of message construction.
        /// </summary>
        public const string ActivityTypeMessageConstruction = "MessageConstruction";

        /// <summary>
        /// Defines the type of message transform.
        /// </summary>
        public const string ActivityTypeMessageTransform = "MessageTransform";

        /// <summary>
        /// Defines the type of receive.
        /// </summary>
        public const string ActivityTypeReceive = "Receive";

        /// <summary>
        /// Defines the type of send.
        /// </summary>
        public const string ActivityTypeSend = "Send";

        /// <summary>
        /// Defines the type of suspend.
        /// </summary>
        public const string ActivityTypeSuspend = "Suspend";

        /// <summary>
        /// Defines the type of terminate.
        /// </summary>
        public const string ActivityTypeTerminate = "Terminate";

        /// <summary>
        /// Defines the type of workflow.
        /// </summary>
        public const string ActivityTypeWorkflow = "Workflow";

        #endregion

        #region Workflow Channel Types

        /// <summary>
        /// Defines the type of publish-subscribe.
        /// </summary>
        public const string ChannelTypePublishSubscribe = "PublishSubscribe";

        /// <summary>
        /// Defines the type of trigger.
        /// </summary>
        public const string ChannelTypeTrigger = "Trigger";

        #endregion

        #region Workflow Object Properties

        /// <summary>
        /// Defines the property name of activate.
        /// </summary>
        public const string PropertyActivate = "Activate";

        /// <summary>
        /// Defines the property name of activity references.
        /// </summary>
        public const string PropertyActivityReferences = "ActivityReferences";

        /// <summary>
        /// Defines the property name of binding.
        /// </summary>
        public const string PropertyBinding = "Binding";

        /// <summary>
        /// Defines the property name of direct binding type.
        /// </summary>
        public const string PropertyDirectBindingType = "DirectBindingType";

        /// <summary>
        /// Defines the property name of constructed messages.
        /// </summary>
        public const string PropertyConstructedMessages = "ConstructedMessages";

        /// <summary>
        /// Defines the property name of consumer workflow channel.
        /// </summary>
        public const string PropertyConsumerChannelReference = "ConsumerChannelReference";

        /// <summary>
        /// Defines the property name of expression.
        /// </summary>
        public const string PropertyExpression = "Expression";

        /// <summary>
        /// Defines the property name of expression language.
        /// </summary>
        public const string PropertyExpressionLanguage = "ExpressionLanguage";

        /// <summary>
        /// Defines the property name of follows correlation.
        /// </summary>
        public const string PropertyFollowsCorrelation = "FollowsCorrelation";

        /// <summary>
        /// Defines the property name of initializes correlation.
        /// </summary>
        public const string PropertyInitializesCorrelation = "InitializesCorrelation";

        /// <summary>
        /// Defines the property name of invokee.
        /// </summary>
        public const string PropertyInvokee = "Invokee";

        /// <summary>
        /// Defines the property name of is async.
        /// </summary>
        public const string PropertyIsAsync = "IsAsync";

        /// <summary>
        /// Defines the property name of is multi-source.
        /// </summary>
        public const string PropertyIsMultiSource = "IsMultiSource";

        /// <summary>
        /// Defines the property name of is multi-target.
        /// </summary>
        public const string PropertyIsMultiTarget = "IsMultiTarget";

        /// <summary>
        /// Defines the property name of is service link.
        /// </summary>
        public const string PropertyIsServiceLink = "IsServiceLink";

        /// <summary>
        /// Defines the property name of logical binding port.
        /// </summary>
        public const string PropertyLogicalBindingPort = "LogicalBindingPort";

        /// <summary>
        /// Defines the property name of map.
        /// </summary>
        public const string PropertyMap = "Map";

        /// <summary>
        /// Defines the property name of message name.
        /// </summary>
        public const string PropertyMessageName = "MessageName";

        /// <summary>
        /// Defines the property name of object ID.
        /// </summary>
        public const string PropertyObjectId = "ObjectId";

        /// <summary>
        /// Defines the property name of operation name.
        /// </summary>
        public const string PropertyOperationName = "OperationName";

        /// <summary>
        /// Defines the property name of operation message name.
        /// </summary>
        public const string PropertyOperationMessageName = "OperationMessageName";

        /// <summary>
        /// Defines the property name of port name.
        /// </summary>
        public const string PropertyPortName = "PortName";

        /// <summary>
        /// Defines the property name of provider workflow channel.
        /// </summary>
        public const string PropertyProviderChannelReference = "ProviderChannelReference";

        /// <summary>
        /// Defines the property name of role name.
        /// </summary>
        public const string PropertyRoleName = "RoleName";

        /// <summary>
        /// Defines the property name of service link name.
        /// </summary>
        public const string PropertyServiceLinkName = "ServiceLinkName";

        /// <summary>
        /// Defines the property name of service link role name.
        /// </summary>
        public const string PropertyServiceLinkRoleName = "ServiceLinkRoleName";

        /// <summary>
        /// Defines the property name of service link port type name.
        /// </summary>
        public const string PropertyServiceLinkPortTypeName = "ServiceLinkPortTypeName";

        /// <summary>
        /// Defines the property name of source message references.
        /// </summary>
        public const string PropertySourceMessageReferences = "SourceMessageReferences";

        /// <summary>
        /// Defines the property name of subscription filter.
        /// </summary>
        public const string PropertySubscriptionFilter = "SubscriptionFilter";

        /// <summary>
        /// Defines the property name of target message references.
        /// </summary>
        public const string PropertyTargetMessageReferences = "TargetMessageReferences";

        /// <summary>
        /// Defines the property name of unique ID.
        /// </summary>
        public const string PropertyUniqueId = "UniqueId";

        /// <summary>
        /// Defines the property name of workflow channel.
        /// </summary>
        public const string PropertyWorkflowChannel = "WorkflowChannel";

        /// <summary>
        /// Defines the property name of workflow parameters.
        /// </summary>
        public const string PropertyWorkflowParameters = "WorkflowParameters";

        #endregion

        #region Workflow Object Values

        /// <summary>
        /// Defines the property value for a direct binding.
        /// </summary>
        public const string PropertyValueDirectBinding = "Direct";

        /// <summary>
        /// Defines the property value for a logical binding.
        /// </summary>
        public const string PropertyValueLogicalBinding = "Logical";

        /// <summary>
        /// Defines the property value for a physical binding.
        /// </summary>
        public const string PropertyValuePhysicalBinding = "Physical";

        /// <summary>
        /// Defines the property value for a decision branch else.
        /// </summary>
        public const string PropertyValueElse = "Else";

        /// <summary>
        /// Defines the property value for a language of C#.
        /// </summary>
        public const string PropertyValueLanguageCSharp = "C#";

        #endregion
    }
}
