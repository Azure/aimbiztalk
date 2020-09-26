using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.ApplicationDefinitions;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse.Tests
{
    /// <summary>
    /// Contains reusable helper methods for testing such as creation of standard object models.
    /// </summary>
    public static class TestHelper
    {        
        /// <summary>
        /// Builds a migration context.
        /// </summary>
        /// <returns>A migration context.</returns>
        public static MigrationContext BuildContext()
        {
            return new MigrationContext
            {
                WorkingFolder = @"c:\temp\workingfolder"
            };
        }

        /// <summary>
        /// Creates a default object model for parsing and building a report.
        /// </summary>
        /// <returns></returns>
        public static AzureIntegrationServicesModel CreateDefaultModelForParsing()
        {
            var aisModel = new AzureIntegrationServicesModel();

            // Create a report node with a resource container and resource definitions
            var resourceContainer = new ResourceContainer()
            {
                Name = "TestApp1.msi",
                Description = "This is the description of the MSI.",
                Type = ModelConstants.ResourceContainerMsi,
                Key = "test-app-1-container-key"
            };
            aisModel.MigrationSource.ResourceContainers.Add(resourceContainer);

            var appResourceDefinition1 = new ResourceDefinition()
            {
                Name = "App 1 Resource Definition",
                Key = $"{resourceContainer.Key}:app-1",
                Description = "App 1 Description",
                Type = ModelConstants.ResourceDefinitionApplicationDefinition
            };
            resourceContainer.ResourceDefinitions.Add(appResourceDefinition1);

            var appResource1 = new ResourceItem
            {
                Name = "App 1 Resource Definition Application",
                Key = $"{appResourceDefinition1.Key}:app-resource-1",
                Description = "App 1 Resource Description",
                Type = ModelConstants.ResourceApplication
            };
            appResourceDefinition1.Resources.Add(appResource1);

            var appResourceDefinition2 = new ResourceDefinition()
            {
                Name = "App 2 Resource Definition",
                Key = $"{resourceContainer.Key}:app-2",
                Description = "App 2 Description",
                Type = ModelConstants.ResourceDefinitionApplicationDefinition
            };
            resourceContainer.ResourceDefinitions.Add(appResourceDefinition2);

            var appResource2 = new ResourceItem
            {
                Name = "App 2 Resource Definition Application",
                Key = $"{appResourceDefinition2.Key}:app-resource-2",
                Description = "App 1 Resource Description",
                Type = ModelConstants.ResourceApplication
            };
            appResourceDefinition2.Resources.Add(appResource2);

            var appResourceDefinition3 = new ResourceDefinition()
            {
                Name = "App 3 Resource Definition",
                Key = $"{resourceContainer.Key}:app-3",
                Description = "App 3 Description",
                Type = ModelConstants.ResourceDefinitionApplicationDefinition
            };
            resourceContainer.ResourceDefinitions.Add(appResourceDefinition3);

            var appResource3 = new ResourceItem
            {
                Name = "App 3 Resource Definition Application",
                Key = $"{appResourceDefinition3.Key}:pp-resource-3",
                Description = "App 3 Resource Description",
                Type = ModelConstants.ResourceApplication
            };
            appResourceDefinition3.Resources.Add(appResource3);

            var schemaResourceDefinition1 = new ResourceDefinition()
            {
                Name = "DocumentSchema1",
                Description = "This is document schema 1.",
                Type = ModelConstants.ResourceDefinitionSchema,
                Key = $"{resourceContainer.Key}:document-schema-1"
            };
            resourceContainer.ResourceDefinitions.Add(schemaResourceDefinition1);

            var schemaResource1 = new ResourceItem()
            {
                Name = "DocumentSchema1",
                Description = "This is document schema 1.",
                Type = ModelConstants.ResourceDocumentSchema,
                Key = $"{schemaResourceDefinition1.Key}:document-schema-1:schema"
            };
            schemaResourceDefinition1.Resources.Add(schemaResource1);

            var schemaResourceDefinition2 = new ResourceDefinition()
            {
                Name = "DocumentSchema2",
                Description = "This is document schema 2.",
                Type = ModelConstants.ResourceDefinitionSchema,
                Key = $"{resourceContainer.Key}:document-schema-2"
            };
            resourceContainer.ResourceDefinitions.Add(schemaResourceDefinition2);

            var schemaResource2 = new ResourceItem()
            {
                Name = "DocumentSchema2",
                Description = "This is document schema 2.",
                Type = ModelConstants.ResourceDocumentSchema,
                Key = $"{schemaResourceDefinition2.Key}:document-schema-2:schema"
            };
            schemaResourceDefinition2.Resources.Add(schemaResource2);

            var schemaResourceDefinition3 = new ResourceDefinition()
            {
                Name = "PropertySchema1",
                Description = "This is property schema 1.",
                Type = ModelConstants.ResourceDefinitionSchema,
                Key = $"{resourceContainer.Key}:property-schema-1"
            };
            resourceContainer.ResourceDefinitions.Add(schemaResourceDefinition3);

            var schemaResource3 = new ResourceItem()
            {
                Name = "PropertySchema1",
                Description = "This is property schema 2.",
                Type = ModelConstants.ResourceDocumentSchema,
                Key = $"{schemaResourceDefinition3.Key}:property-schema-1:schema"
            }; 
            schemaResourceDefinition3.Resources.Add(schemaResource3);

            var schemaResource3Property1 = new ResourceItem()
            {
                Name = "Property1",
                Description = "This is property 2",
                Type = ModelConstants.ResourcePropertySchema,
                Key = $"{schemaResourceDefinition3.Key}:property-schema-1:schema:Property1"
            };
        
            schemaResourceDefinition3.Resources.Add(schemaResource3Property1);

            var schemaResource3Property2 = new ResourceItem()
            {
                Name = "Property2",
                Description = "This is property 2",
                Type = ModelConstants.ResourcePropertySchema,
                Key = $"{schemaResourceDefinition3.Key}:property-schema-1:schema:Property2"
            };
            schemaResourceDefinition3.Resources.Add(schemaResource3Property2);

            var transformResourceDefinition1 = new ResourceDefinition()
            {
                Name = "Transform1",
                Description = "This is transform 1.",
                Type = ModelConstants.ResourceDefinitionMap,
                Key = $"{resourceContainer.Key}:transform-1"
            };
            resourceContainer.ResourceDefinitions.Add(transformResourceDefinition1);

            var transformResource1 = new ResourceItem()
            {
                Name = "Transform1",
                Description = "This is the transform 1, resource",
                Type = ModelConstants.ResourceMap,
                Key = $"{transformResourceDefinition1.Key}:transform-1-resource"
            };
            transformResourceDefinition1.Resources.Add(transformResource1);

            var bindingResourceDefinition1 = new ResourceDefinition()
            {
                Name = "Binding1",
                Description = "This is binding 1.",
                Type = ModelConstants.ResourceDefinitionBindings,
                Key = $"{resourceContainer.Key}:binding-1"
            };
            resourceContainer.ResourceDefinitions.Add(bindingResourceDefinition1);

            var sendPortResource1 = new ResourceItem()
            {
                Name = "SendPort1",
                Description = "This is sendport 1.",
                Type = ModelConstants.ResourceSendPort,
                Key = $"{bindingResourceDefinition1.Key}:sendport-1"
            };
            bindingResourceDefinition1.Resources.Add(sendPortResource1);

            var sendPortFilterResource1 = new ResourceItem()
            {
                Name = "SendPort1-Filter",
                Description = "This is sendport 1, filter expression",
                Type = ModelConstants.ResourceFilterExpression,
                Key = $"{sendPortResource1.Key}:sendport-1-filter"
            };
            sendPortResource1.Resources.Add(sendPortFilterResource1);

            var receivePortResource1 = new ResourceItem()
            {
                Name = "ReceivePort1",
                Description = "This is receive port 1.",
                Type = ModelConstants.ResourceReceivePort,
                Key = $"{bindingResourceDefinition1.Key}:receiveport-1"
            };
            bindingResourceDefinition1.Resources.Add(receivePortResource1);

            var receiveLocation1 = new ResourceItem()
            {
                Name = "ReceiveLocation1",
                Description = "This is receive location 1.",
                Type = ModelConstants.ResourceReceiveLocation,
                Key = $"{receivePortResource1.Key}:receivelocation-1"
            };
            receivePortResource1.Resources.Add(receiveLocation1);

            var distributionListResource1 = new ResourceItem
            {
                Name = "DistributionList1",
                Description = "This is distributionlist 1.",
                Type = ModelConstants.ResourceDistributionList,
                Key = $"{bindingResourceDefinition1.Key}:distributionlist-1"
            };
            bindingResourceDefinition1.Resources.Add(distributionListResource1);

            var distributionListFilterResource1 = new ResourceItem
            { 
                Name = "DistributionListFilter1",
                Description = "This is distribution list filer 1.",
                Type = ModelConstants.ResourceFilterExpression,
                Key = $"{distributionListResource1.Key}:distributionlistfilter-1"
            };
            distributionListResource1.Resources.Add(distributionListFilterResource1);

            // Create a parsed BizTalk Application Group
            var applicationGroup = new ParsedBizTalkApplicationGroup();
            aisModel.MigrationSource.MigrationSourceModel = applicationGroup;

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

            var container = new ResourceContainer();
            container.ResourceDefinitions.Add(new ResourceDefinition());
            container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = appResource1.Key });
            aisModel.MigrationSource.ResourceContainers.Add(container);

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
                ResourceContainerKey = "app-1-assembly-resource-key",
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
                ResourceContainerKey = "app-1-assembly-resource-key",
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

            var app1Msi = new ResourceContainer()
            {
                Name = "App 1 MSI",
                Description = "App 1 MSI Description",
                Key = "app-1-msi-resource-key",
                Type = ModelConstants.ResourceContainerMsi
            };
            aisModel.MigrationSource.ResourceContainers.Add(app1Msi);
            var app1Cab = new ResourceContainer()
            {
                Name = "App 1 CAB",
                Description = "App 1 CAB Description",
                Key = $"{app1Msi.Key}:app-1-cab-resource-key",
                Type = ModelConstants.ResourceContainerCab
            };
            app1Msi.ResourceContainers.Add(app1Cab);
            var app1Assembly = new ResourceContainer()
            {
                Name = "App 1 Assembly",
                Description = "App 1 Assembly Description",
                Key = $"{app1Cab.Key}:app-1-assembly-resource-key",
                Type = ModelConstants.ResourceContainerAssembly
            };
            app1Cab.ResourceContainers.Add(app1Assembly);
            var app1Schema = new ResourceDefinition()
            {
                Name = "Document Schema 1",
                Description = "Document Schema 1 Description",
                Key = $"{app1Assembly.Key}:document-schema-1",
                Type = ModelConstants.ResourceDefinitionSchema
            };
            app1Assembly.ResourceDefinitions.Add(app1Schema);
            var app1DocumentSchema = new ResourceItem()
            {
                Name = "DocumentSchema1",
                Description = "DocumentSchema1Description",
                Key = $"{app1Schema.Key}:app-1-schema-resource-key"
            };
            app1Schema.Resources.Add(app1DocumentSchema);
            var app1Schema2 = new ResourceDefinition()
            {
                Name = "Document Schema 2",
                Description = "Document Schema 2 Description",
                Key = $"{app1Assembly.Key}:document-schema-2",
                Type = ModelConstants.ResourceDefinitionSchema
            };
            app1Assembly.ResourceDefinitions.Add(app1Schema2);
            var app1DocumentSchema2 = new ResourceItem()
            {
                Name = "DocumentSchema2",
                Description = "DocumentSchema2Description",
                Key = $"{app1Schema.Key}:app-1-schema-2-resource-key"
            };
            app1Schema.Resources.Add(app1DocumentSchema2);

            return aisModel;
        }
    }
}
