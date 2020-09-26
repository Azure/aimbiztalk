using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Messages;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.ApplicationDefinitions;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert.Tests
{
    /// <summary>
    /// Contains reusable helper methods for testing such as creation of standard object models.
    /// </summary>
    public static class TestHelper
    {        /// <summary>
        /// Builds a migration context.
        /// </summary>
        /// <returns>A migration context.</returns>
        public static MigrationContext BuildContext()
        {
            var context = new MigrationContext
            {
                WorkingFolder = @"c:\temp\workingfolder",
                ConversionFolder = Path.GetTempPath()
            };

            context.TemplateFolders.Add(Path.GetTempPath());

            return context;
        }

        /// <summary>
        /// Creates a default object model for converting.
        /// </summary>
        /// <returns></returns>
        public static AzureIntegrationServicesModel CreateDefaultModelForConverting()
        {
            var model = new AzureIntegrationServicesModel();

            // Create a report node with a resource container and resource definitions
            var resourceContainer = new ResourceContainer()
            {
                Name = "TestApp1.msi",
                Description = "This is the description of the MSI.",
                Type = ModelConstants.ResourceContainerMsi,
                Key = "test-app-1-container-key"
            };
            model.MigrationSource.ResourceContainers.Add(resourceContainer);

            var appResourceDefinition1 = new ResourceDefinition()
            {
                Name = "App 1 Resource Definition",
                Key = "app-1",
                Description = "App 1 Description",
                Type = ModelConstants.ResourceDefinitionApplicationDefinition
            };
            resourceContainer.ResourceDefinitions.Add(appResourceDefinition1);

            var appResource1 = new ResourceItem
            {
                Name = "App 1 Resource Definition Application",
                Key = "app-resource-1",
                Description = "App 1 Resource Description",
                Type = ModelConstants.ResourceApplication
            };
            appResourceDefinition1.Resources.Add(appResource1);

            var appResourceDefinition2 = new ResourceDefinition()
            {
                Name = "App 2 Resource Definition",
                Key = "app-2",
                Description = "App 2 Description",
                Type = ModelConstants.ResourceDefinitionApplicationDefinition
            };
            resourceContainer.ResourceDefinitions.Add(appResourceDefinition2);

            var appResource2 = new ResourceItem
            {
                Name = "App 2 Resource Definition Application",
                Key = "app-resource-2",
                Description = "App 1 Resource Description",
                Type = ModelConstants.ResourceApplication
            };
            appResourceDefinition2.Resources.Add(appResource2);

            var appResourceDefinition3 = new ResourceDefinition()
            {
                Name = "App 3 Resource Definition",
                Key = "app-3",
                Description = "App 3 Description",
                Type = ModelConstants.ResourceDefinitionApplicationDefinition
            };
            resourceContainer.ResourceDefinitions.Add(appResourceDefinition3);

            var appResource3 = new ResourceItem
            {
                Name = "App 3 Resource Definition Application",
                Key = "app-resource-3",
                Description = "App 3 Resource Description",
                Type = ModelConstants.ResourceApplication
            };
            appResourceDefinition3.Resources.Add(appResource3);

            var schemaResourceDefinition1 = new ResourceDefinition()
            {
                Name = "DocumentSchema1",
                Description = "This is document schema 1.",
                Type = ModelConstants.ResourceDefinitionSchema,
                Key = "document-schema-1",
                ResourceContent = "<some xml>"
            };
            resourceContainer.ResourceDefinitions.Add(schemaResourceDefinition1);

            var schemaResource1 = new ResourceItem()
            {
                Name = "DocumentSchema1",
                Description = "This is document schema 1.",
                Type = ModelConstants.ResourceDocumentSchema,
                Key = "document-schema-1:schema1",
                ParentRefId = schemaResourceDefinition1.RefId
            };
            schemaResourceDefinition1.Resources.Add(schemaResource1);
           
            var messageResource1 = new ResourceItem()
            {
                Name = "Message1",
                Description = "This is message 1.",
                Type = ModelConstants.ResourceMessageType,
                Key = "document-schema-1:schema1:message1",
                ParentRefId = schemaResource1.RefId
            };
            schemaResource1.Resources.Add(messageResource1);

            var schemaResourceDefinition2 = new ResourceDefinition()
            {
                Name = "DocumentSchema2",
                Description = "This is document schema 2.",
                Type = ModelConstants.ResourceDefinitionSchema,
                Key = "document-schema-2",
                ParentRefId = resourceContainer.RefId
            };
            resourceContainer.ResourceDefinitions.Add(schemaResourceDefinition2);

            var schemaResource2 = new ResourceItem()
            {
                Name = "DocumentSchema2",
                Description = "This is document schema 2.",
                Type = ModelConstants.ResourceDocumentSchema,
                Key = "document-schema-2:schema2",
                ParentRefId = schemaResourceDefinition2.RefId
            };
            schemaResourceDefinition2.Resources.Add(schemaResource2);

            var messageResource2 = new ResourceItem()
            {
                Name = "Message2",
                Description = "This is message 2.",
                Type = ModelConstants.ResourceMessageType,
                Key = "document-schema-2:schema2:message2",
                ParentRefId = schemaResource2.RefId
            };
            schemaResource2.Resources.Add(messageResource2);

            var schemaResourceDefinition3 = new ResourceDefinition()
            {
                Name = "PropertySchema1",
                Description = "This is property schema 1.",
                Type = ModelConstants.ResourceDefinitionSchema,
                Key = "property-schema-1",
                ParentRefId = resourceContainer.RefId
            };
            resourceContainer.ResourceDefinitions.Add(schemaResourceDefinition3);

            var schemaResource3 = new ResourceItem()
            {
                Name = "PropertySchema1",
                Description = "This is property schema 2.",
                Type = ModelConstants.ResourceDocumentSchema,
                Key = "property-schema-1:schema",
                ParentRefId = schemaResourceDefinition3.RefId
            }; 
            schemaResourceDefinition3.Resources.Add(schemaResource3);

            var messageResource3 = new ResourceItem()
            {
                Name = "Message3",
                Description = "This is message 3.",
                Type = ModelConstants.ResourceMessageType,
                Key = "document-schema-3:schema3:message3",
                ParentRefId = schemaResource3.RefId
            };
            schemaResource3.Resources.Add(messageResource3);

            var schemaResource3Property1 = new ResourceItem()
            {
                Name = "Property1",
                Description = "This is property 2",
                Key = "property-schema-1:schema:Property1",
                Type = ModelConstants.ResourcePropertySchema,
                ParentRefId = schemaResourceDefinition3.RefId
            };        
            schemaResourceDefinition3.Resources.Add(schemaResource3Property1);

            var schemaResource3Property2 = new ResourceItem()
            {
                Name = "Property2",
                Description = "This is property 2",
                Key = "property-schema-1:schema:Property2",
                Type = ModelConstants.ResourcePropertySchema,
                ParentRefId = schemaResourceDefinition3.RefId
            };
            schemaResourceDefinition3.Resources.Add(schemaResource3Property2);

            var transformResourceDefinition1 = new ResourceDefinition()
            {
                Name = "Transform1",
                Description = "This is transform 1.",
                Type = ModelConstants.ResourceDefinitionMap,
                Key = "transform-1",
                ParentRefId = resourceContainer.RefId,
                ResourceContent = "<some xml>"
            };
            resourceContainer.ResourceDefinitions.Add(transformResourceDefinition1);

            var transformResource1 = new ResourceItem()
            {
                Name = "Transform1",
                Description = "This is the transform 1, resource",
                Type = ModelConstants.ResourceMap,
                Key = "transform-1-resource",
                ParentRefId = transformResourceDefinition1.RefId
            };
            transformResourceDefinition1.Resources.Add(transformResource1);

            var bindingResourceDefinition1 = new ResourceDefinition()
            {
                Name = "Binding1",
                Description = "This is binding 1.",
                Type = ModelConstants.ResourceDefinitionBindings,
                Key = "binding-1",
                ParentRefId = resourceContainer.RefId
            };
            resourceContainer.ResourceDefinitions.Add(bindingResourceDefinition1);

            var sendPortResource1 = new ResourceItem()
            {
                Name = "SendPort1",
                Description = "This is sendport 1.",
                Type = ModelConstants.ResourceSendPort,
                Key = "sendport-1",
                ParentRefId = bindingResourceDefinition1.RefId
            };
            bindingResourceDefinition1.Resources.Add(sendPortResource1);

            var sendPortFilterResource1 = new ResourceItem()
            {
                Name = "SendPort1-Filter",
                Description = "This is sendport 1, filter expression",
                Type = ModelConstants.ResourceFilterExpression,
                Key = "sendport-1-filter",
                ParentRefId = sendPortResource1.RefId
            };
            sendPortResource1.Resources.Add(sendPortFilterResource1);

            var receivePortResource1 = new ResourceItem()
            {
                Name = "ReceivePort1",
                Description = "This is receive port 1.",
                Type = ModelConstants.ResourceReceivePort,
                Key = "receiveport-1",
                ParentRefId = bindingResourceDefinition1.RefId
            };
            bindingResourceDefinition1.Resources.Add(receivePortResource1);

            var receiveLocation1 = new ResourceItem()
            {
                Name = "ReceiveLocation1",
                Description = "This is receive location 1.",
                Type = ModelConstants.ResourceReceiveLocation,
                Key = "receivelocation-1",
                ParentRefId = receivePortResource1.RefId
            };
            receivePortResource1.Resources.Add(receiveLocation1);

            var distributionListResource1 = new ResourceItem
            {
                Name = "DistributionList1",
                Description = "This is distributionlist 1.",
                Type = ModelConstants.ResourceDistributionList,
                Key = "distributionlist-1",
                ParentRefId = bindingResourceDefinition1.RefId
            };
            bindingResourceDefinition1.Resources.Add(distributionListResource1);

            var distributionListFilterResource1 = new ResourceItem
            { 
                Name = "DistributionListFilter1",
                Description = "This is distribution list filer 1.",
                Type = ModelConstants.ResourceFilterExpression,
                Key = "distributionlistfilter-1",
                ParentRefId = distributionListResource1.RefId
            };
            distributionListResource1.Resources.Add(distributionListFilterResource1);

            // Create a parsed BizTalk Application Group
            var applicationGroup = new ParsedBizTalkApplicationGroup();
            model.MigrationSource.MigrationSourceModel = applicationGroup;

            // Create applications
            var application1 = new ParsedBizTalkApplication();
            application1.Application.Name = "Test App 1";
            applicationGroup.Applications.Add(application1);

            var application2 = new ParsedBizTalkApplication();
            application2.Application.Name = "Test App 2";
            application2.Application.Bindings = new BindingFile { BindingInfo = new BindingInfo() };

            applicationGroup.Applications.Add(application2);

            var application3 = new ParsedBizTalkApplication();
            application3.Application.Name = "Test App 3";
            applicationGroup.Applications.Add(application3);
            application3.Application.Bindings = new BindingFile { BindingInfo = new BindingInfo() };

            // Create application definitions
            application1.Application.ApplicationDefinition = new ApplicationDefinitionFile()
            {
                ResourceContainerKey = resourceContainer.Key,
                ResourceDefinitionKey = appResourceDefinition1.Key,
                ResourceKey = appResource1.Key,
                ApplicationDefinition = new ApplicationDefinition()
                {
                    Properties = new List<ApplicationDefinitionProperty>()
                    {
                        new ApplicationDefinitionProperty() { Name = "DisplayName", Value = application1.Application.Name },
                        new ApplicationDefinitionProperty() { Name = "ApplicationDescription", Value = application1.Application.Name + " Description" }
                    }.ToArray()
                }
            };

            application2.Application.ApplicationDefinition = new ApplicationDefinitionFile()
            {
                ResourceContainerKey = resourceContainer.Key,
                ResourceDefinitionKey = appResourceDefinition2.Key,
                ResourceKey = appResource2.Key,
                ApplicationDefinition = new ApplicationDefinition()
                {
                    References = new List<ApplicationDefinitionReference>()
                    {
                        new ApplicationDefinitionReference() { Name = application3.Application.Name }
                    }.ToArray(),
                    Properties = new List<ApplicationDefinitionProperty>()
                    {
                        new ApplicationDefinitionProperty() { Name = "DisplayName", Value = application2.Application.Name },
                        new ApplicationDefinitionProperty() { Name = "ApplicationDescription", Value = application2.Application.Name + " Description" }
                    }.ToArray()
                }
            };

            application3.Application.ApplicationDefinition = new ApplicationDefinitionFile()
            {
                ResourceContainerKey = resourceContainer.Key,
                ResourceDefinitionKey = appResourceDefinition3.Key,
                ResourceKey = appResource3.Key,
                ApplicationDefinition = new ApplicationDefinition()
                {
                    Properties = new List<ApplicationDefinitionProperty>()
                    {
                        new ApplicationDefinitionProperty() { Name = "DisplayName", Value = application3.Application.Name },
                        new ApplicationDefinitionProperty() { Name = "ApplicationDescription", Value = application3.Application.Name + " Description" }
                    }.ToArray()
                }
            };

            // Create schemas
            var documentSchema1 = new Types.Entities.Schema()
            {
                Name = "DocumentSchema1",
                Namespace = "Test.Schemas",
                FullName = "Test.Schemas.DocumentSchema1",
                XmlNamespace = "http://schemas.test.com/DocumentSchema1",
                RootNodeName = "Root",
                ResourceDefinitionKey = schemaResourceDefinition1.Key,
                ResourceKey = schemaResource1.Key,
                SchemaType = BizTalkSchemaType.Document
            };
            documentSchema1.MessageDefinitions.Add(new MessageDefinition(documentSchema1.RootNodeName, documentSchema1.XmlNamespace, "Test.Schemas.DocumentSchema1", "DocumentSchema1", "document-schema-1:schema:Root"));
            documentSchema1.PromotedProperties.Add(new PromotedProperty() { PropertyType = "Test.Schemas.PropertySchema1.Property1", XPath = "some xpath" });
            application1.Application.Schemas.Add(documentSchema1);

            var documentSchema2 = new Types.Entities.Schema()
            {
                Name = "DocumentSchema2",
                Namespace = "Test.Schemas",
                FullName = "Test.Schemas.DocumentSchema2",
                XmlNamespace = "http://schemas.test.com/DocumentSchema2",
                RootNodeName = "Root",
                ResourceDefinitionKey = schemaResourceDefinition2.Key,
                ResourceKey = schemaResource2.Key,
                SchemaType = BizTalkSchemaType.Document
            };
            documentSchema2.MessageDefinitions.Add(new MessageDefinition(documentSchema2.RootNodeName, documentSchema2.XmlNamespace, "Test.Schemas.DocumentSchema2", "DocumentSchema2", "document-schema-2:schema:Root"));
            application1.Application.Schemas.Add(documentSchema2);

            var propertySchema = new Types.Entities.Schema()
            {
                Name = "PropertySchema1",
                Namespace = "Test.Schemas",
                FullName = "Test.Schemas.PropertySchema1",
                XmlNamespace = "http://schemas.test.com/PropertySchema1",
                RootNodeName = "Root",
                ResourceDefinitionKey = schemaResourceDefinition3.Key,
                ResourceKey = schemaResource3.Key,
                SchemaType = BizTalkSchemaType.Property
            };
            propertySchema.ContextProperties.Add(new ContextProperty() { DataType = "xs:string", FullyQualifiedName = "Test.Schemas.PropertySchema1.Property1", PropertyName = schemaResource3Property1.Name, Namespace = "Test.Schemas.PropertySchema1", ResourceKey = schemaResource3Property1.Key });
            propertySchema.ContextProperties.Add(new ContextProperty() { DataType = "xs:int", FullyQualifiedName = "Test.Schemas.PropertySchema1.Property2", PropertyName = schemaResource3Property2.Name, Namespace = "Test.Schemas.PropertySchema1", ResourceKey = schemaResource3Property2.Key });
            application1.Application.Schemas.Add(propertySchema);

            // Create transforms
            var map = new Types.Entities.Transform()
            {
                Name = "Transform1",
                FullName = "Test.Maps.Transform1",
                ModuleName = "Test.Maps, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                Namespace = "Test.Maps",
                ResourceContainerKey = resourceContainer.Key,
                ResourceDefinitionKey = transformResourceDefinition1.Key
            };
            map.SourceSchemaTypeNames.Add("Test.Schemas.DocumentSchema1");
            map.TargetSchemaTypeNames.Add("Test.Schemas.DocumentSchema2");
            application1.Application.Transforms.Add(map);

            // Create send ports.
            var sendPort = new SendPort
            {
                ResourceKey = sendPortResource1.Key,
                Description = "This is a send port description.",
                Name = "Test.SendPorts.SendPort1",
                FilterExpression = new Types.Entities.Filters.Filter
                {
                    Group = new Types.Entities.Filters.Group[]
                    {
                        new Types.Entities.Filters.Group 
                        { 
                            Statement = new Types.Entities.Filters.Statement[]
                            {
                                new Types.Entities.Filters.Statement()
                            }
                        }
                    }
                }
            };
            
            /// Create receive ports.
            var receivePort = new ReceivePort
            {
                ResourceKey = receivePortResource1.Key,
                Description = receivePortResource1.Description,
                Name = receivePortResource1.Name,
                ReceiveLocations = new ReceiveLocation[]
                {
                    new ReceiveLocation
                    {
                        ResourceKey = receiveLocation1.Key,
                        Name = receiveLocation1.Name,
                        Description = receiveLocation1.Name
                    }
                }
            };

            // Create distribution lists.
            var distributionList = new DistributionList
            {
                ResourceKey = distributionListResource1.Key,
                Description = distributionListResource1.Description,
                Name = distributionListResource1.Name,
                FilterExpression = new Types.Entities.Filters.Filter
                {
                    ResourceKey = distributionListFilterResource1.Key,
                    Group = new Types.Entities.Filters.Group[]
                    {
                        new Types.Entities.Filters.Group()
                    }
                }
            };

            application1.Application.Bindings = new BindingFile
            {
                BindingInfo = new BindingInfo
                {                    
                    SendPortCollection = new SendPort[] { sendPort },
                    ReceivePortCollection = new ReceivePort[] { receivePort },
                    DistributionListCollection = new DistributionList[] { distributionList }
                }
            };

            // Target model
            model.MigrationTarget.TargetEnvironment = AzureIntegrationServicesTargetEnvironment.Consumption;
            model.MigrationTarget.DeploymentEnvironment = "dev";
            model.MigrationTarget.AzureSubscriptionId = "azure-subs-id";
            model.MigrationTarget.AzurePrimaryRegion = "UK South";
            model.MigrationTarget.AzureSecondaryRegion = "UK West";

            // Add a message bus
            model.MigrationTarget.MessageBus = new MessageBus()
            {
                Name = "ContosoMessageBus",
                Key = "ContosoMessageBus",
                ResourceMapKey = "messageBus",
            };
            var messageBusResourceTemplate = new TargetResourceTemplate() { OutputPath = "output" };
            messageBusResourceTemplate.ResourceTemplateFiles.Add("path/to/file.json.liquid");
            messageBusResourceTemplate.ResourceTemplateFiles.Add("path/to/file2.json");
            model.MigrationTarget.MessageBus.Resources.Add(messageBusResourceTemplate);

            // Add an application
            var systemApp = new Application()
            {
                Name = "System",
                Key = "ContosoMessageBus:System"
            };
            model.MigrationTarget.MessageBus.Applications.Add(systemApp);

            var app = new Application()
            {
                Name = "AppA",
                Key = "ContosoMessageBus:AppA",
                ResourceMapKey = "application"
            };
            model.MigrationTarget.MessageBus.Applications.Add(app);

            // Add an application message
            var appMessage = new DocumentMessage()
            {
                Name = "PurchaseOrderFlatFile",
                MessageSchema = new MessageSchema
                {
                    ResourceKeyRef = messageResource1.Key,
                    Name = messageResource2.Name
                },
                Key = "ContosoMessageBus:AppA:PurchaseOrderFlatFile",
                ContentType = MessageContentType.Xml,
                ResourceMapKey = "applicationMessage"
            };
            app.Messages.Add(appMessage);

            var appMessageResource1 = new TargetResourceTemplate()
            {
                OutputPath = "OutputPath",
                ResourceType = ModelConstants.ResourceTypeXml
            };
            appMessage.Resources.Add(appMessageResource1);

            var transform = new MessageTransform
            {
                Name = "MessageTransform",
                ResourceKeyRef = "transform-1-resource",
            };
            appMessage.MessageTransforms.Add(transform);

            var appMessageResource2 = new TargetResourceTemplate()
            {
                OutputPath = "OutputPath",
                ResourceType = ModelConstants.ResourceTypeXslt
            };
            appMessage.Resources.Add(appMessageResource2);

            // Add a message box
            var messageBox = new TopicChannel()
            {
                Name = "MessageBox",
                Key = "ContosoMessageBus:System:MessageBox",
                ResourceMapKey = "messageBox"
            };
            systemApp.Channels.Add(messageBox);

            // Add a message agent
            var messageAgent = new ContentBasedRouter()
            {
                Name = "MessageAgent",
                Key = "ContosoMessageBus:System:MessageAgent",
                ResourceMapKey = "messageAgent"
            };
            systemApp.Intermediaries.Add(messageAgent);

            // Add an FTP endpoint
            var ftpReceive = new AdapterEndpoint()
            {
                Name = "FtpReceive",
                Key = "ContosoMessageBus:System:FtpReceive",
                ResourceMapKey = "ftpReceive"
            };
            systemApp.Endpoints.Add(ftpReceive);

            return model;
        }
    }
}
