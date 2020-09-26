using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Messages;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.ApplicationDefinitions;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Tests
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
            return new MigrationContext
            {
                WorkingFolder = @"c:\temp\workingfolder"
            };
        }

        /// <summary>
        /// Copies the source model to the target model.
        /// </summary>
        /// <param name="model">The model containing the source and target.</param>
        public static void CopySourceToTarget(AzureIntegrationServicesModel model, bool includeFtpReceive = false, bool includeFtpSend = false)
        {
            model = model ?? throw new ArgumentNullException(nameof(model));

            model.MigrationTarget.MessageBus = new ApplicationModel.Target.MessageBus
            {
                Name = "nameMessageBus",
                Key = ModelConstants.MessageBusLeafKey,
                ResourceMapKey = "resourceMapKeyMessageBus",
            };

            foreach (var application in model.GetSourceModel<ParsedBizTalkApplicationGroup>().Applications)
            {
                var targetApplication = new ApplicationModel.Target.Application
                {
                    Name = application.Application.Name,
                    Key = $"{model.MigrationTarget.MessageBus.Key}:{application.Application.Name.FormatKey()}",
                    ResourceMapKey = "application",
                };

                var schemaMessages = from schema in application.Application.Schemas
                                     from messageDefintion in schema.MessageDefinitions
                                     where schema.SchemaType == BizTalkSchemaType.Document
                                     select new { Schema = schema, MessageDefinition = messageDefintion };

                foreach (var schemaMessage in schemaMessages)
                {
                    var resourceMapKey = $"applicationMessage{application.Application.Name}{schemaMessage.MessageDefinition.LocalName}";

                    targetApplication.Messages.Add(new DocumentMessage
                    {
                        Name = schemaMessage.MessageDefinition.LocalName,
                        MessageSchema = new MessageSchema
                        {
                            ResourceKeyRef = schemaMessage.MessageDefinition.ResourceKey,
                            Name = schemaMessage.Schema.Name
                        },
                        Key = $"{targetApplication.Key}:{schemaMessage.MessageDefinition.LocalName}",
                        ContentType = MessageContentType.Xml,
                        ResourceMapKey = resourceMapKey,
                        Description = "Schema description"
                    });
                }

                if (includeFtpReceive)
                {
                    var ftpReceiveAdapter = new AdapterEndpoint()
                    {
                        Name = "FTP Receive Adapter",
                        Description = "Test FTP receive adapter",
                        Key = $"{targetApplication.Key}:ReceivePort1:ReceiveLocation1:{ModelConstants.AdapterEndpointLeafKey}",
                        Activator = true,
                        AdapterProtocol = "FTP",
                        MessageExchangePattern = MessageExchangePattern.Accept
                    };

                    ftpReceiveAdapter.Properties.Add(ModelConstants.ScenarioName, $"{targetApplication.Name}:ReceivePort1:ReceiveLocation1");
                    targetApplication.Endpoints.Add(ftpReceiveAdapter);
                }

                if (includeFtpSend)
                {
                    var ftpSendAdapter = new AdapterEndpoint()
                    {
                        Name = "FTP Send Adapter",
                        Description = "Test Send receive adapter",
                        Key = $"{targetApplication.Key}:Test.SendPorts.SendPort1:{ModelConstants.AdapterEndpointLeafKey}",
                        Activator = true,
                        AdapterProtocol = "FTP",
                        MessageExchangePattern = MessageExchangePattern.Send
                    };

                    ftpSendAdapter.Properties.Add(ModelConstants.ScenarioName, $"{targetApplication.Name}:Test.SendPorts.SendPort1");
                    targetApplication.Endpoints.Add(ftpSendAdapter);
                }

                model.MigrationTarget.MessageBus.Applications.Add(targetApplication);
            }
        }

        /// <summary>
        /// Creates a default object model for analyzing and building a report.
        /// </summary>
        /// <returns></returns>
        public static AzureIntegrationServicesModel CreateDefaultModelForAnalyzing()
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
                Key = "document-schema-1"
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
                Description = "This is message schema 1.",
                Type = ModelConstants.ResourceMessageType,
                Key = "document-schema-1:schema:Root",
                ParentRefId = schemaResource1.RefId
            };
            schemaResource1.Resources.Add(messageResource1);

            var schemaResourceDefinition2 = new ResourceDefinition()
            {
                Name = "DocumentSchema2",
                Description = "This is document schema 2.",
                Type = ModelConstants.ResourceDefinitionSchema,
                Key = "document-schema-2"
            };
            resourceContainer.ResourceDefinitions.Add(schemaResourceDefinition2);

            var schemaResource2 = new ResourceItem()
            {
                Name = "DocumentSchema2",
                Description = "This is document schema 2.",
                Type = ModelConstants.ResourceDocumentSchema,
                Key = "document-schema-2:schema",
                ParentRefId = schemaResourceDefinition2.RefId
            };
            schemaResourceDefinition2.Resources.Add(schemaResource2);

            var messageResource2 = new ResourceItem()
            {
                Name = "Message2",
                Description = "This is message schema 2.",
                Type = ModelConstants.ResourceMessageType,
                Key = "document-schema-2:schema:Root",
                ParentRefId = schemaResource2.RefId
            };
            schemaResource2.Resources.Add(messageResource2);

            var schemaResourceDefinition3 = new ResourceDefinition()
            {
                Name = "PropertySchema1",
                Description = "This is property schema 1.",
                Type = ModelConstants.ResourceDefinitionSchema,
                Key = "property-schema-1"
            };
            resourceContainer.ResourceDefinitions.Add(schemaResourceDefinition3);

            var schemaResource3 = new ResourceItem()
            {
                Name = "PropertySchema1",
                Description = "This is property schema 1 resource.",
                Type = ModelConstants.ResourceDocumentSchema,
                Key = "property-schema-1:schema",
                ParentRefId = schemaResourceDefinition3.RefId
            }; 
            schemaResourceDefinition3.Resources.Add(schemaResource3);

            var messageResource3 = new ResourceItem()
            {
                Name = "Message3",
                Description = "This is message schema 3.",
                Type = ModelConstants.ResourceMessageType,
                Key = "document-schema-3:schema:Root",
                ParentRefId = schemaResource3.RefId
            };
            schemaResource3.Resources.Add(messageResource3);

            var schemaResourceDefinition4 = new ResourceDefinition()
            {
                Name = "EnvelopeSchema1",
                Description = "This is envelope schema 1.",
                Type = ModelConstants.ResourceDefinitionSchema,
                Key = "envelope-schema-1"
            };
            resourceContainer.ResourceDefinitions.Add(schemaResourceDefinition4);

            var schemaResource4 = new ResourceItem()
            {
                Name = "EnvelopeSchema1",
                Description = "This is envelope schema 1.",
                Type = ModelConstants.ResourceDocumentSchema,
                Key = "envelope-schema-1:schema",
                ParentRefId = schemaResourceDefinition1.RefId
            };
            schemaResourceDefinition4.Resources.Add(schemaResource4);

            var messageResource4 = new ResourceItem()
            {
                Name = "Envelope1",
                Description = "This is envelope schema 1.",
                Type = ModelConstants.ResourceMessageType,
                Key = "envelope-schema-1:schema:Envelope",
                ParentRefId = schemaResource4.RefId
            };
            schemaResource4.Resources.Add(messageResource4);

            var contextProperty1 = new ResourceItem()
            {
                Name = "Property1",
                Description = "This is property 2",
                Key = "property-schema-1:schema:Property1",
                Type = ModelConstants.ResourcePropertySchema
            };

            schemaResource3.Resources.Add(contextProperty1);

            var contextProperty2 = new ResourceItem()
            {
                Name = "Property2",
                Description = "This is property 2",
                Key = "property-schema-1:schema:Property2",
                Type = ModelConstants.ResourcePropertySchema
            };
            schemaResource3.Resources.Add(contextProperty2);

            var transformResourceDefinition1 = new ResourceDefinition()
            {
                Name = "Transform1",
                Description = "This is transform 1.",
                Type = ModelConstants.ResourceDefinitionMap,
                Key = "transform-1"
            };
            resourceContainer.ResourceDefinitions.Add(transformResourceDefinition1);

            var transformResourceDefinition2 = new ResourceDefinition()
            {
                Name = "Transform2",
                Description = "This is transform 2.",
                Type = ModelConstants.ResourceDefinitionMap,
                Key = "transform-2"
            };
            resourceContainer.ResourceDefinitions.Add(transformResourceDefinition2);

            var transformResource1 = new ResourceItem()
            {
                Name = "Transform1",
                Description = "This is the transform 1, resource",
                Type = ModelConstants.ResourceMap,
                Key = "transform-1-resource"
            };
            transformResourceDefinition1.Resources.Add(transformResource1);

            schemaResource1.ResourceRelationships.Add(new ResourceRelationship
            {
                ResourceRelationshipType = ResourceRelationshipType.ReferencedBy,
                ResourceRefId = transformResource1.RefId,
            });

            var transformResource2 = new ResourceItem()
            {
                Name = "Transform2",
                Description = "This is the transform 2, resource",
                Type = ModelConstants.ResourceMap,
                Key = "transform-2-resource"
            };
            transformResourceDefinition2.Resources.Add(transformResource2);

            var bindingResourceDefinition1 = new ResourceDefinition()
            {
                Name = "Binding1",
                Description = "This is binding 1.",
                Type = ModelConstants.ResourceDefinitionBindings,
                Key = "binding-1"
            };
            resourceContainer.ResourceDefinitions.Add(bindingResourceDefinition1);

            var sendPortResource1 = new ResourceItem()
            {
                Name = "Test.SendPorts.SendPort1",
                Description = "This is sendport 1.",
                Type = ModelConstants.ResourceSendPort,
                Key = "sendport-1"
            };
            bindingResourceDefinition1.Resources.Add(sendPortResource1);

            var sendPortFilterResource1 = new ResourceItem()
            {
                Name = "SendPort1-Filter",
                Description = "This is sendport 1, filter expression",
                Type = ModelConstants.ResourceFilterExpression,
                Key = "sendport-1-filter"
            };
            sendPortResource1.Resources.Add(sendPortFilterResource1);

            var receivePortResource1 = new ResourceItem()
            {
                Name = "ReceivePort1",
                Description = "This is receive port 1.",
                Type = ModelConstants.ResourceReceivePort,
                Key = "receiveport-1"
            };
            bindingResourceDefinition1.Resources.Add(receivePortResource1);

            var receiveLocation1 = new ResourceItem()
            {
                Name = "ReceiveLocation1",
                Description = "This is receive location 1.",
                Type = ModelConstants.ResourceReceiveLocation,
                Key = "receivelocation-1"
            };
            receivePortResource1.Resources.Add(receiveLocation1);

            var distributionListResource1 = new ResourceItem
            {
                Name = "DistributionList1",
                Description = "This is distributionlist 1.",
                Type = ModelConstants.ResourceDistributionList,
                Key = "distributionlist-1"
            };
            bindingResourceDefinition1.Resources.Add(distributionListResource1);

            var distributionListFilterResource1 = new ResourceItem
            { 
                Name = "DistributionListFilter1",
                Description = "This is distribution list filer 1.",
                Type = ModelConstants.ResourceFilterExpression,
                Key = "distributionlistfilter-1"
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

            // Create application definitions
            application1.Application.ApplicationDefinition = new ApplicationDefinitionFile()
            {
                ResourceContainerKey = resourceContainer.Key,
                ResourceDefinitionKey = appResourceDefinition1.Key,
                ResourceKey = appResource1.Key,
                Resource = appResource1,
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
                Resource = appResource2,
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
                Resource = appResource3,
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
                Resource = schemaResource1,
                SchemaType = BizTalkSchemaType.Document
            };

            var messageDefinition1 = new MessageDefinition(documentSchema1.RootNodeName, documentSchema1.XmlNamespace, "Test.Schemas.DocumentSchema1", "DocumentSchema1", messageResource1.Key);
            messageResource1.SourceObject = messageDefinition1;
            documentSchema1.MessageDefinitions.Add(messageDefinition1);
            documentSchema1.PromotedProperties.Add(new PromotedProperty() { PropertyType = "Test.Schemas.PropertySchema1.Property1", XPath = "some xpath 1" });
            messageDefinition1.PromotedProperties.Add(new PromotedProperty() { PropertyType = "Test.Schemas.PropertySchema1.Property1", XPath = "some xpath 1" });
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
                Resource = schemaResource2,
                SchemaType = BizTalkSchemaType.Document
            };

            var messageDefinition2 = new MessageDefinition(documentSchema2.RootNodeName, documentSchema2.XmlNamespace, "Test.Schemas.DocumentSchema2", "DocumentSchema2", messageResource2.Key);
            messageResource2.SourceObject = messageDefinition2;
            documentSchema2.MessageDefinitions.Add(messageDefinition2);
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
                Resource = schemaResource3,
                SchemaType = BizTalkSchemaType.Property
            };
            propertySchema.ContextProperties.Add(new ContextProperty() 
                { 
                    DataType = "xs:string", 
                    FullyQualifiedName = "Test.Schemas.PropertySchema1.Property1", 
                    PropertyName = contextProperty1.Name, 
                    Namespace = "Test.Schemas.PropertySchema1", 
                    ResourceKey = contextProperty1.Key,
                    Resource = contextProperty1
                });
            propertySchema.ContextProperties.Add(new ContextProperty() 
            { 
                DataType = "xs:int", 
                FullyQualifiedName = "Test.Schemas.PropertySchema1.Property2", 
                PropertyName = contextProperty2.Name, 
                Namespace = "Test.Schemas.PropertySchema1", 
                ResourceKey = contextProperty2.Key,
                Resource = contextProperty2
            });
            application1.Application.Schemas.Add(propertySchema);

            var envelopeSchema1 = new Types.Entities.Schema()
            {
                Name = "EnvelopeSchema1",
                Namespace = "Test.Schemas",
                FullName = "Test.Schemas.EnvelopeSchema1",
                XmlNamespace = "http://schemas.test.com/EnvelopeSchema1",
                RootNodeName = "Envelope",
                IsEnvelope = true,
                BodyXPath = "/*[local-name()='Envelope' and namespace-uri()='http://schemas.test.com/EnvelopeSchema1']/*[local-name()='Body' and namespace-uri()='http://schemas.test.com/EnvelopeSchema1']",
                ResourceDefinitionKey = schemaResourceDefinition4.Key,
                ResourceKey = schemaResource4.Key,
                Resource = schemaResource4,
                SchemaType = BizTalkSchemaType.Document
            };

            var messageDefinition3 = new MessageDefinition(envelopeSchema1.RootNodeName, envelopeSchema1.XmlNamespace, "Test.Schemas.EnvelopeSchema1", "EnvelopeSchema1", messageResource4.Key);
            messageResource4.SourceObject = messageDefinition3;
            envelopeSchema1.MessageDefinitions.Add(messageDefinition2);
            application1.Application.Schemas.Add(envelopeSchema1);

            // Create transforms
            var map1 = new Types.Entities.Transform()
            {
                Name = "Transform1",
                FullName = "Test.Maps.Transform1",
                ModuleName = "Test.Maps, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                Namespace = "Test.Maps",
                ResourceContainerKey = resourceContainer.Key,
                ResourceDefinitionKey = transformResourceDefinition1.Key,
                ResourceKey = transformResource1.Key,
                Resource = transformResource1
            };
            map1.SourceSchemaTypeNames.Add("Test.Schemas.DocumentSchema1");
            map1.TargetSchemaTypeNames.Add("Test.Schemas.DocumentSchema2");
            transformResource1.SourceObject = map1;
            application1.Application.Transforms.Add(map1);

            var map2 = new Types.Entities.Transform()
            {
                Name = "Transform2",
                FullName = "Test.Maps.Transform2",
                ModuleName = "Test.Maps, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                Namespace = "Test.Maps",
                ResourceContainerKey = resourceContainer.Key,
                ResourceDefinitionKey = transformResourceDefinition1.Key,
                ResourceKey = transformResource2.Key,
                Resource = transformResource2
            };
            map2.SourceSchemaTypeNames.Add("Test.Schemas.DocumentSchema2");
            map2.TargetSchemaTypeNames.Add("Test.Schemas.DocumentSchema1");
            transformResource2.SourceObject = map2;
            application1.Application.Transforms.Add(map2);

            // Create send ports.
            var sendPort = new SendPort
            {
                ResourceKey = sendPortResource1.Key,
                Resource = sendPortResource1,
                Description = "This is a send port description.",
                Name = "Test.SendPorts.SendPort1",
                PrimaryTransport = new TransportInfo
                {
                    TransportType = new ProtocolType
                    {
                        Name = "FTP"
                    },
                    TransportTypeData = "<CustomProps><AdapterConfig vt=\"8\">&lt;Config xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"&gt;&lt;uri&gt;ftp://127.0.0.1:22/%MessageID%.xml&lt;/uri&gt;&lt;serverAddress&gt;127.0.0.1&lt;/serverAddress&gt;&lt;serverPort&gt;22&lt;/serverPort&gt;&lt;ftpServerType&gt;Detect&lt;/ftpServerType&gt;&lt;targetFolder /&gt;&lt;targetFileName&gt;%MessageID%.xml&lt;/targetFileName&gt;&lt;representationType&gt;binary&lt;/representationType&gt;&lt;allocateStorage&gt;False&lt;/allocateStorage&gt;&lt;appendIfExists&gt;False&lt;/appendIfExists&gt;&lt;connectionLimit&gt;0&lt;/connectionLimit&gt;&lt;passiveMode&gt;False&lt;/passiveMode&gt;&lt;firewallType&gt;NoFirewall&lt;/firewallType&gt;&lt;firewallPort&gt;22&lt;/firewallPort&gt;&lt;useSsl&gt;False&lt;/useSsl&gt;&lt;useDataProtection&gt;True&lt;/useDataProtection&gt;&lt;ftpsConnMode&gt;Explicit&lt;/ftpsConnMode&gt;&lt;/Config&gt;</AdapterConfig></CustomProps>",
                },
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
                },
                Transforms = new Types.Entities.Bindings.Transform[1] { new Types.Entities.Bindings.Transform() { FullName = map2.FullName, AssemblyQualifiedName = $"{map2.Name}, {map2.ModuleName}" } },
                InboundTransforms = new Types.Entities.Bindings.Transform[1] { new Types.Entities.Bindings.Transform() { FullName = map1.FullName, AssemblyQualifiedName = $"{map1.Name}, {map1.ModuleName}" } }
            };
            sendPortResource1.SourceObject = sendPort;

            /// Create receive ports.
            var receivePort = new ReceivePort
            {
                ResourceKey = receivePortResource1.Key,
                Resource = receivePortResource1,
                Description = receivePortResource1.Description,
                Name = receivePortResource1.Name,
                ReceiveLocations = new ReceiveLocation[]
                {
                    new ReceiveLocation
                    {
                        ResourceKey = receiveLocation1.Key,
                        Name = receiveLocation1.Name,
                        Description = receiveLocation1.Name,
                        ReceiveLocationTransportType = new ProtocolType
                        {
                            Name = "FTP",
                        },
                        ReceiveLocationTransportTypeData = "<CustomProps><AdapterConfig vt=\"8\">&lt;Config xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"&gt;&lt;uri&gt;ftp://127.0.0.1:21/*.*&lt;/uri&gt;&lt;serverAddress&gt;127.0.0.1&lt;/serverAddress&gt;&lt;serverPort&gt;21&lt;/serverPort&gt;&lt;userName&gt;btsuser&lt;/userName&gt;&lt;password&gt;******&lt;/password&gt;&lt;ftpServerType&gt;Detect&lt;/ftpServerType&gt;&lt;fileMask&gt;*.*&lt;/fileMask&gt;&lt;targetFolder /&gt;&lt;representationType&gt;binary&lt;/representationType&gt;&lt;receiveDataTimeout&gt;90000&lt;/receiveDataTimeout&gt;&lt;maximumBatchSize&gt;0&lt;/maximumBatchSize&gt;&lt;maximumNumberOfFiles&gt;0&lt;/maximumNumberOfFiles&gt;&lt;passiveMode&gt;False&lt;/passiveMode&gt;&lt;useNLST&gt;False&lt;/useNLST&gt;&lt;firewallType&gt;NoFirewall&lt;/firewallType&gt;&lt;firewallPort&gt;21&lt;/firewallPort&gt;&lt;pollingUnitOfMeasure&gt;Seconds&lt;/pollingUnitOfMeasure&gt;&lt;pollingInterval&gt;60&lt;/pollingInterval&gt;&lt;redownloadInterval&gt;-1&lt;/redownloadInterval&gt;&lt;errorThreshold&gt;10&lt;/errorThreshold&gt;&lt;maxFileSize&gt;100&lt;/maxFileSize&gt;&lt;useSsl&gt;False&lt;/useSsl&gt;&lt;useDataProtection&gt;True&lt;/useDataProtection&gt;&lt;ftpsConnMode&gt;Explicit&lt;/ftpsConnMode&gt;&lt;deleteAfterDownload&gt;True&lt;/deleteAfterDownload&gt;&lt;enableTimeComparison&gt;False&lt;/enableTimeComparison&gt;&lt;/Config&gt;</AdapterConfig></CustomProps>"
                    }
                },
                Transforms = new Types.Entities.Bindings.Transform[1] { new Types.Entities.Bindings.Transform() { FullName = map1.FullName, AssemblyQualifiedName = $"{map1.Name}, {map1.ModuleName}" } },
                OutboundTransforms = new Types.Entities.Bindings.Transform[1] { new Types.Entities.Bindings.Transform() { FullName = map2.FullName, AssemblyQualifiedName = $"{map2.Name}, {map2.ModuleName}" } }
            };

            // Create distribution lists.
            var distributionList = new DistributionList
            {
                ResourceKey = distributionListResource1.Key,
                Resource = distributionListResource1,
                Description = distributionListResource1.Description,
                Name = distributionListResource1.Name,
                FilterExpression = new Types.Entities.Filters.Filter
                {
                    ResourceKey = distributionListFilterResource1.Key,
                    Group = new Types.Entities.Filters.Group[]
                    {
                        new Types.Entities.Filters.Group()
                    }
                },
                SendPorts = new SendPortRef[]
                {
                    new SendPortRef()
                    {
                        Name = sendPort.Name
                    }
                }
            };
            distributionListResource1.SourceObject = distributionList;

            application1.Application.Bindings = new BindingFile
            {
                BindingInfo = new BindingInfo
                {                    
                    SendPortCollection = new SendPort[] { sendPort },
                    ReceivePortCollection = new ReceivePort[] { receivePort },
                    DistributionListCollection = new DistributionList[] { distributionList }
                }
            };
            
            return aisModel;
        }
    }
}
