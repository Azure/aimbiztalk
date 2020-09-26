using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations
{
    /// <summary>
    /// Defines constants used within the Orchestration Meta Model.
    /// </summary>
    public static class MetaModelConstants
    {
        /// <summary>
        /// Defines the name of the root element.
        /// </summary>
        public const string MetaModelRootElement = "MetaModel";

        #region Element Types

        /// <summary>
        /// Defines a call element type.
        /// </summary>
        public const string ElementTypeCall = "Call";

        /// <summary>
        /// Defines a construct element type.
        /// </summary>
        public const string ElementTypeConstruct = "Construct";

        /// <summary>
        /// Defines a correlation declaration element type.
        /// </summary>
        public const string ElementTypeCorrelationDeclaration = "CorrelationDeclaration";

        /// <summary>
        /// Defines a correlation type element type.
        /// </summary>
        public const string ElementTypeCorrelationType = "CorrelationType";

        /// <summary>
        /// Defines a direct binding attribute element type.
        /// </summary>
        public const string ElementTypeDirectBindingAttribute = "DirectBindingAttribute";

        /// <summary>
        /// Defines a DNF predicate element type.
        /// </summary>
        public const string ElementTypeDnfPredicate = "DNFPredicate";

        /// <summary>
        /// Defines an exec element type.
        /// </summary>
        public const string ElementTypeExec = "Exec";

        /// <summary>
        /// Defines a message assignment element type.
        /// </summary>
        public const string ElementTypeMessageAssignment = "MessageAssignment";

        /// <summary>
        /// Defines a message declaration element type.
        /// </summary>
        public const string ElementTypeMessageDeclaration = "MessageDeclaration";

        /// <summary>
        /// Defines a message part ref element type.
        /// </summary>
        public const string ElementTypeMessagePartRef = "MessagePartRef";

        /// <summary>
        /// Defines a message ref element type.
        /// </summary>
        public const string ElementTypeMessageRef = "MessageRef";

        /// <summary>
        /// Defines an module element type.
        /// </summary>
        public const string ElementTypeModule = "Module";

        /// <summary>
        /// Defines a multipart message type element.
        /// </summary>
        public const string ElementTypeMultipartMessageType = "MultipartMessageType";

        /// <summary>
        /// Defines an operation declaration element.
        /// </summary>
        public const string ElementTypeOperationDeclaration = "OperationDeclaration";

        /// <summary>
        /// Defines an operation type element.
        /// </summary>
        public const string ElementTypeOperationType = "OperationType";

        /// <summary>
        /// Defines a parameter element.
        /// </summary>
        public const string ElementTypeParameter = "Parameter";

        /// <summary>
        /// Defines a part declaration type element.
        /// </summary>
        public const string ElementTypePartDeclaration = "PartDeclaration";

        /// <summary>
        /// Defines a physical binding attribute element type.
        /// </summary>
        public const string ElementTypePhysicalBindingAttribute = "PhysicalBindingAttribute";

        /// <summary>
        /// Defines a port declaration type element.
        /// </summary>
        public const string ElementTypePortDeclaration = "PortDeclaration";

        /// <summary>
        /// Defines a port type element.
        /// </summary>
        public const string ElementTypePortType = "PortType";

        /// <summary>
        /// Defines a port type ref element.
        /// </summary>
        public const string ElementTypePortTypeRef = "PortTypeRef";

        /// <summary>
        /// Defines a property ref element.
        /// </summary>
        public const string ElementTypePropertyRef = "PropertyRef";

        /// <summary>
        /// Defines a receive element.
        /// </summary>
        public const string ElementTypeReceive = "Receive";

        /// <summary>
        /// Defines a role declaration element.
        /// </summary>
        public const string ElementTypeRoleDeclaration = "RoleDeclaration";

        /// <summary>
        /// Defines a service body element.
        /// </summary>
        public const string ElementTypeServiceBody = "ServiceBody";

        /// <summary>
        /// Defines a service declaration element.
        /// </summary>
        public const string ElementTypeServiceDeclaration = "ServiceDeclaration";

        /// <summary>
        /// Defines a service link declaration element.
        /// </summary>
        public const string ElementTypeServiceLinkDeclaration = "ServiceLinkDeclaration";

        /// <summary>
        /// Defines a service link type element.
        /// </summary>
        public const string ElementTypeServiceLinkType = "ServiceLinkType";

        /// <summary>
        /// Defines a statement ref element.
        /// </summary>
        public const string ElementTypeStatementRef = "StatementRef";

        /// <summary>
        /// Defines task declaration element.
        /// </summary>
        public const string ElementTypeTask = "Task";

        /// <summary>
        /// Defines transform declaration element.
        /// </summary>
        public const string ElementTypeTransform = "Transform";

        /// <summary>
        /// Defines a variable assignment element.
        /// </summary>
        public const string ElementTypeVariableAssignment = "VariableAssignment";

        /// <summary>
        /// Defines a variable declaration element.
        /// </summary>
        public const string ElementTypeVariableDeclaration = "VariableDeclaration";

        #endregion

        #region Element Property Keys

        /// <summary>
        /// Defines a property key of class name.
        /// </summary>
        public const string PropertyKeyClassName = "ClassName";

        /// <summary>
        /// Defines a property key of direct binding type.
        /// </summary>
        public const string PropertyKeyDirectBindingType = "DirectBindingType";

        /// <summary>
        /// Defines a property key of expression.
        /// </summary>
        public const string PropertyKeyExpression = "Expression";

        /// <summary>
        /// Defines a property key of grouping.
        /// </summary>
        public const string PropertyKeyGrouping = "Grouping";

        /// <summary>
        /// Defines a property key of initializes.
        /// </summary>
        public const string PropertyKeyInitializes = "Initializes";

        /// <summary>
        /// Defines a property key of initial value.
        /// </summary>
        public const string PropertyKeyInitialValue = "InitialValue";

        /// <summary>
        /// Defines a property key of is ghost branch.
        /// </summary>
        public const string PropertyKeyIsGhostBranch = "IsGhostBranch";

        /// <summary>
        /// Defines a property key of is web port.
        /// </summary>
        public const string PropertyKeyIsWebPort = "IsWebPort";

        /// <summary>
        /// Defines a property key of LHS.
        /// </summary>
        public const string PropertyKeyLhs = "LHS";

        /// <summary>
        /// Defines a property key of message ref.
        /// </summary>
        public const string PropertyKeyMessageRef = "MessageRef";

        /// <summary>
        /// Defines a property key of name.
        /// </summary>
        public const string PropertyKeyName = "Name";

        /// <summary>
        /// Defines a property key of operator.
        /// </summary>
        public const string PropertyKeyOperator = "Operator";

        /// <summary>
        /// Defines a property key of orientation.
        /// </summary>
        public const string PropertyKeyOrientation = "Orientation";

        /// <summary>
        /// Defines a property key of part ref.
        /// </summary>
        public const string PropertyKeyPartRef = "PartRef";

        /// <summary>
        /// Defines a property key of port index.
        /// </summary>
        public const string PropertyKeyPortIndex = "PortIndex";

        /// <summary>
        /// Defines a property key of port modifier.
        /// </summary>
        public const string PropertyKeyPortModifier = "PortModifier";

        /// <summary>
        /// Defines a property key of param direction.
        /// </summary>
        public const string PropertyKeyParamDirection = "ParamDirection";

        /// <summary>
        /// Defines a property key of ref.
        /// </summary>
        public const string PropertyKeyRef = "Ref";

        /// <summary>
        /// Defines a property key of report to analyst.
        /// </summary>
        public const string PropertyKeyReportToAnalyst = "ReportToAnalyst";

        /// <summary>
        /// Defines a property key of RHS.
        /// </summary>
        public const string PropertyKeyRhs = "RHS";

        /// <summary>
        /// Defines the property name of role name.
        /// </summary>
        public const string PropertyKeyRoleName = "RoleName";

        /// <summary>
        /// Defines a property key of signal.
        /// </summary>
        public const string PropertyKeySignal = "Signal";

        /// <summary>
        /// Defines a property key of type.
        /// </summary>
        public const string PropertyKeyType = "Type";

        /// <summary>
        /// Defines a property key of use default constructor.
        /// </summary>
        public const string PropertyKeyUseDefaultConstructor = "UseDefaultConstructor";

        #endregion

        #region ParentLink Values

        /// <summary>
        /// Defines a parent link value for a fault message to an operation in a port type.
        /// </summary>
        public const string ParentLinkOperationFaultMessage = "OperationDeclaration_FaultMessageRef";

        /// <summary>
        /// Defines a parent link value for a request message to an operation in a port type.
        /// </summary>
        public const string ParentLinkOperationRequestMessage = "OperationDeclaration_RequestMessageRef";

        /// <summary>
        /// Defines a parent link value for a response message to an operation in a port type.
        /// </summary>
        public const string ParentLinkOperationResponseMessage = "OperationDeclaration_ResponseMessageRef";

        /// <summary>
        /// Defines a parent link value for an input message to a transform.
        /// </summary>
        public const string ParentLinkTransformInputMessage = "Transform_InputMessagePartRef";

        /// <summary>
        /// Defines a parent link value for an output message to a transform.
        /// </summary>
        public const string ParentLinkTransformOutputMessage = "Transform_OutputMessagePartRef";

        #endregion
    }
}
