using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AzureIntegrationMigration.BizTalk.Report.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report
{
    /// <summary>
    /// Class containing formatting and name translation.
    /// </summary>
    public static class ResourceFormatter
    {

        #region Resource Friendly Name Translation

        /// <summary>
        /// Returns a friendly name for the type of resource container.
        /// </summary>
        /// <param name="key">The key for the type of container.</param>
        /// <returns>A friendly name string.</returns>
        public static string GetResourceContainerFriendlyName(string key)
        {
            switch (key)
            {
                case ModelConstants.ResourceContainerMsi:
                    return HtmlResources.ResourceContainerMsi;
                case ModelConstants.ResourceContainerCab:
                    return HtmlResources.ResourceContainerCab;
                case ModelConstants.ResourceContainerAssembly:
                    return HtmlResources.ResourceContainerAssembly;
                default:
                    return HtmlResources.ResourceContainerDefault;
            }
        }

        /// <summary>
        /// Returns an icon for the type of resource container.
        /// </summary>
        /// <param name="key">The key for the type of definition.</param>
        /// <returns>A friendly name string.</returns>
        public static string GetResourceContainerIconFromType(string key)
        {
            switch (key)
            {
                case ModelConstants.ResourceContainerMsi:
                    return HtmlResources.ResourceContainerIconMsi;
                case ModelConstants.ResourceContainerCab:
                    return HtmlResources.ResourceContainerIconCab;
                case ModelConstants.ResourceContainerAssembly:
                    return HtmlResources.ResourceContainerIconAssembly;
                default:
                    return HtmlResources.ResourceContainerIconDefault;
            }
        }

        /// <summary>
        /// Returns a friendly name for the type of resource definition.
        /// </summary>
        /// <param name="key">The key for the type of definition.</param>
        /// <returns>A friendly name string.</returns>
        public static string GetResourceDefinitionFriendlyName(string key)
        {
            switch (key)
            {
                case ModelConstants.ResourceDefinitionSchema:
                    return HtmlResources.ResourceDefinitionNameSchema;
                case ModelConstants.ResourceDefinitionMap:
                    return HtmlResources.ResourceDefinitionNameMap;
                case ModelConstants.ResourceDefinitionReceivePipeline:
                    return HtmlResources.ResourceDefinitionNameReceivePipeline;
                case ModelConstants.ResourceDefinitionSendPipeline:
                    return HtmlResources.ResourceDefinitionNameSendPipeline;
                case ModelConstants.ResourceDefinitionOrchestration:
                    return HtmlResources.ResourceDefinitionNameOrchestration;
                case ModelConstants.ResourceDefinitionFile:
                    return HtmlResources.ResourceDefinitionNameFile;
                case ModelConstants.ResourceDefinitionBindings:
                    return HtmlResources.ResourceDefinitionNameBindings;
                case ModelConstants.ResourceDefinitionApplicationDefinition:
                    return HtmlResources.ResourceDefinitionNameApplicationDefinition;
                default:
                    return HtmlResources.ResourceDefinitionNameDefault;
            }
        }

        /// <summary>
        /// Returns a friendly name for the type of resource definition.
        /// </summary>
        /// <param name="key">The key for the type of definition.</param>
        /// <returns>A friendly name string.</returns>
        public static string GetResourceDefinitionIconFromType(string key)
        {
            switch (key)
            {
                case ModelConstants.ResourceDefinitionSchema:
                    return HtmlResources.ResourceDefinitionIconSchema;
                case ModelConstants.ResourceDefinitionMap:
                    return HtmlResources.ResourceDefinitionIconMap;
                case ModelConstants.ResourceDefinitionReceivePipeline:
                    return HtmlResources.ResourceDefinitionIconReceivePipeline;
                case ModelConstants.ResourceDefinitionSendPipeline:
                    return HtmlResources.ResourceDefinitionIconSendPipeline;
                case ModelConstants.ResourceDefinitionOrchestration:
                    return HtmlResources.ResourceDefinitionIconOrchestration;
                case ModelConstants.ResourceDefinitionFile:
                    return HtmlResources.ResourceDefinitionIconFile;
                case ModelConstants.ResourceDefinitionBindings:
                    return HtmlResources.ResourceDefinitionIconBindings;
                case ModelConstants.ResourceDefinitionApplicationDefinition:
                    return HtmlResources.ResourceDefinitionIconApplicationDefinition;
                default:
                    return HtmlResources.ResourceDefinitionIconDefault;
            }
        }

        /// <summary>
        /// Returns a friendly name for the type of resource.
        /// </summary>
        /// <param name="key">The key for the type of container.</param>
        /// <returns>A friendly name string.</returns>
        public static string GetResourceFriendlyName(string key)
        {
            switch (key)
            {
                case ModelConstants.ResourceApplication:
                    return HtmlResources.ResourceNameApplication;
                case ModelConstants.ResourceReceiveLocation:
                    return HtmlResources.ResourceNameReceiveLocation;
                case ModelConstants.ResourceReceivePort:
                    return HtmlResources.ResourceNameReceivePort;
                case ModelConstants.ResourceSendPort:
                    return HtmlResources.ResourceNameSendPort;
                case ModelConstants.ResourceDistributionList:
                    return HtmlResources.ResourceNameDistributionList;
                case ModelConstants.ResourceFilterExpression:
                    return HtmlResources.ResourceNameFilter;
                case ModelConstants.ResourceServiceBinding:
                    return HtmlResources.ResourceNameService;
                case ModelConstants.ResourceDocumentSchema:
                    return HtmlResources.ResourceNameDocumentSchema;
                case ModelConstants.ResourcePropertySchema:
                    return HtmlResources.ResourceNamePropertySchema;
                case ModelConstants.ResourceContextProperty:
                    return HtmlResources.ResourceNameContextProperty;
                case ModelConstants.ResourceMessageType:
                    return HtmlResources.ResourceNameMessageType;
                case ModelConstants.ResourceMetaModel:
                    return HtmlResources.ResourceNameMetaModel;
                case ModelConstants.ResourceModule:
                    return HtmlResources.ResourceNameModule;
                case ModelConstants.ResourceCorrelationType:
                    return HtmlResources.ResourceNameCorrelationType;
                case ModelConstants.ResourcePortType:
                    return HtmlResources.ResourceNamePortType;
                case ModelConstants.ResourceServiceLinkType:
                    return HtmlResources.ResourceNameServiceLinkType;
                case ModelConstants.ResourceMultipartMessageType:
                    return HtmlResources.ResourceNameMultipartMessageType;
                case ModelConstants.ResourceCorrelationDeclaration:
                    return HtmlResources.ResourceNameCorrelationSet;
                case ModelConstants.ResourceMessageDeclaration:
                    return HtmlResources.ResourceNameMessage;
                case ModelConstants.ResourcePortDeclaration:
                    return HtmlResources.ResourceNameLogicalPort;
                case ModelConstants.ResourceServiceDeclaration:
                    return HtmlResources.ResourceNameServiceDeclaration;
                case ModelConstants.ResourceMap:
                    return HtmlResources.ResourceNameMap;
                case ModelConstants.ResourceReceivePipeline:
                    return HtmlResources.ResourceNameReceivePipeline;
                case ModelConstants.ResourceSendPipeline:
                    return HtmlResources.ResourceNameSendPipeline;
                case ModelConstants.ResourcePipelineComponent:
                    return HtmlResources.ResourceNamePipelineComponent;
                default:
                    return HtmlResources.ResourceNameDefault;
            }
        }

        /// <summary>
        /// Returns an icon snippet for the type of resource.
        /// </summary>
        /// <param name="key">The key for the type of resource.</param>
        /// <returns>A friendly name string.</returns>
        public static string GetResourceIconFromType(string key)
        {
            switch (key)
            {
                case ModelConstants.ResourceApplication:
                    return HtmlResources.ResourceIconApplication;
                case ModelConstants.ResourceReceiveLocation:
                    return HtmlResources.ResourceIconReceiveLocation;
                case ModelConstants.ResourceReceivePort:
                    return HtmlResources.ResourceIconReceivePort;
                case ModelConstants.ResourceSendPort:
                    return HtmlResources.ResourceIconSendPort;
                case ModelConstants.ResourceDistributionList:
                    return HtmlResources.ResourceIconDistributionList;
                case ModelConstants.ResourceFilterExpression:
                    return HtmlResources.ResourceIconFilter;
                case ModelConstants.ResourceServiceBinding:
                    return HtmlResources.ResourceIconService;
                case ModelConstants.ResourceDocumentSchema:
                    return HtmlResources.ResourceIconDocumentSchema;
                case ModelConstants.ResourcePropertySchema:
                    return HtmlResources.ResourceIconPropertySchema;
                case ModelConstants.ResourceContextProperty:
                    return HtmlResources.ResourceIconContextProperty;
                case ModelConstants.ResourceMessageType:
                    return HtmlResources.ResourceIconMessageType;
                case ModelConstants.ResourceMetaModel:
                    return HtmlResources.ResourceIconMetaModel;
                case ModelConstants.ResourceModule:
                    return HtmlResources.ResourceIconModule;
                case ModelConstants.ResourceCorrelationType:
                    return HtmlResources.ResourceIconCorrelationType;
                case ModelConstants.ResourcePortType:
                    return HtmlResources.ResourceIconPortType;
                case ModelConstants.ResourceServiceLinkType:
                    return HtmlResources.ResourceIconServiceLinkType;
                case ModelConstants.ResourceMultipartMessageType:
                    return HtmlResources.ResourceIconMultipartMessageType;
                case ModelConstants.ResourceCorrelationDeclaration:
                    return HtmlResources.ResourceIconCorrelationSet;
                case ModelConstants.ResourceMessageDeclaration:
                    return HtmlResources.ResourceIconMessage;
                case ModelConstants.ResourcePortDeclaration:
                    return HtmlResources.ResourceIconLogicalPort;
                case ModelConstants.ResourceServiceDeclaration:
                    return HtmlResources.ResourceIconOrchestration;
                case ModelConstants.ResourceMap:
                    return HtmlResources.ResourceIconMap;
                case ModelConstants.ResourceReceivePipeline:
                    return HtmlResources.ResourceIconReceivePipeline;
                case ModelConstants.ResourceSendPipeline:
                    return HtmlResources.ResourceIconSendPipeline;
                case ModelConstants.ResourcePipelineComponent:
                    return HtmlResources.ResourceIconPipelineComponent;
                default:
                    return HtmlResources.ResourceIconDefault;
            }
        }

        #endregion

        #region Target Resource Name Translation

        /// <summary>
        /// Returns a friendly name for the type of resource.
        /// </summary>
        /// <param name="key">The resource type for the target resource.</param>
        /// <returns>A friendly name string.</returns>
        public static string GetTargetResourceFriendlyName(string key)
        {
            switch (key)
            {
                case "microsoft.scripts.powershell":
                    return HtmlResources.TargetResourceNameScript;
                case "microsoft.schemas.xml":
                    return HtmlResources.TargetResourceNameXmlSchema;
                case "microsoft.groups.azureresourcegroup":
                    return HtmlResources.TargetResourceNameResourceGroup;
                case "microsoft.configuration.routingproperties":
                    return HtmlResources.TargetResourceNameRoutingProperties;
                case "microsoft.messaging.azureservicebus":
                    return HtmlResources.TargetResourceNameServiceBus;
                case "microsoft.security.azurerole":
                    return HtmlResources.TargetResourceNameSecurityRole;
                case "microsoft.services.azureapimanagement":
                    return HtmlResources.TargetResourceNameApiManagement;
                case "microsoft.services.azureappservice":
                    return HtmlResources.TargetResourceNameAppServicePlan;
                case "microsoft.services.azurestorage":
                    return HtmlResources.TargetResourceNameStorageAccount;
                case "microsoft.services.azureappinsights":
                    return HtmlResources.TargetResourceNameApplicationInsights;
                case "microsoft.stores.azureintegrationaccount":
                    return HtmlResources.TargetResourceNameIntegrationAccount;
                case "microsoft.stores.azureappconfiguration":
                    return HtmlResources.TargetResourceNameAppConfiguration;
                case "microsoft.stores.azurekeyvault":
                    return HtmlResources.TargetResourceNameKeyVault;
                case "microsoft.services.azurefunction":
                    return HtmlResources.TargetResourceNameAzureFunction;
                case "microsoft.workflows.azurelogicapp":
                    return HtmlResources.TargetResourceNameLogicApp;
                case "microsoft.web.connections":
                    return HtmlResources.TargetResourceNameWebConnections;
                case "microsoft.configuration.routingslip":
                    return HtmlResources.TargetResourceNameRoutingSlip;
                case "microsoft.configuration.configurationentry":
                    return HtmlResources.TargetResourceNameConfigurationEntry;
                case "microsoft.messaging.azureservicebustopicsubscription":
                    return HtmlResources.TargetResourceNameServiceBusTopicSubscription;
                case "microsoft.messaging.azureservicebustopic":
                    return HtmlResources.TargetResourceNameServiceBusTopic;
                case "microsoft.web.connectiongateway":
                    return HtmlResources.TargetResourceNameConnectionGateway;
                default:
                    return key;
            }
        }

        /// <summary>
        /// Returns an icon snippet for the type of resource.
        /// </summary>
        /// <param name="key">The resource type for the target resource.</param>
        /// <returns>A friendly name string.</returns>
        public static string GetTargetResourceIconFromType(string key)
        {
            switch (key)
            {
                case "microsoft.scripts.powershell":
                    return HtmlResources.TargetResourceIconScript;
                case "microsoft.schemas.xml":
                    return HtmlResources.TargetResourceIconXmlSchema;
                case "microsoft.groups.azureresourcegroup":
                    return HtmlResources.TargetResourceIconResourceGroup;
                case "microsoft.configuration.routingproperties":
                    return HtmlResources.TargetResourceIconRoutingProperties;
                case "microsoft.messaging.azureservicebus":
                    return HtmlResources.TargetResourceIconServiceBus;
                case "microsoft.security.azurerole":
                    return HtmlResources.TargetResourceIconSecurityRole;
                case "microsoft.services.azureapimanagement":
                    return HtmlResources.TargetResourceIconApiManagement;
                case "microsoft.services.azureappservice":
                    return HtmlResources.TargetResourceIconAppServicePlan;
                case "microsoft.services.azurestorage":
                    return HtmlResources.TargetResourceIconStorageAccount;
                case "microsoft.services.azureappinsights":
                    return HtmlResources.TargetResourceIconApplicationInsights;
                case "microsoft.stores.azureintegrationaccount":
                    return HtmlResources.TargetResourceIconIntegrationAccount;
                case "microsoft.stores.azureappconfiguration":
                    return HtmlResources.TargetResourceIconAppConfiguration;
                case "microsoft.stores.azurekeyvault":
                    return HtmlResources.TargetResourceIconKeyVault;
                case "microsoft.services.azurefunction":
                    return HtmlResources.TargetResourceIconAzureFunction;
                case "microsoft.workflows.azurelogicapp":
                    return HtmlResources.TargetResourceIconLogicApp;
                case "microsoft.web.connections":
                    return HtmlResources.TargetResourceIconWebConnections;
                case "microsoft.configuration.routingslip":
                    return HtmlResources.TargetResourceIconRoutingSlip;
                case "microsoft.configuration.configurationentry":
                    return HtmlResources.TargetResourceIconConfigurationEntry;
                case "microsoft.messaging.azureservicebustopicsubscription":
                    return HtmlResources.TargetResourceIconServiceBusTopicSubscription;
                case "microsoft.messaging.azureservicebustopic":
                    return HtmlResources.TargetResourceIconServiceBusTopic;
                case "microsoft.web.connectiongateway":
                    return HtmlResources.TargetResourceIconConnectionGateway;
                default:
                    return HtmlResources.TargetResourceIconDefault;
            }
        }

        #endregion
    }
}
