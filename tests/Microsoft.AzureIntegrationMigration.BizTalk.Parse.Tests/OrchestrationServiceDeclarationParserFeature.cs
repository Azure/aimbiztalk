// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Linq;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse.Tests
{
    /// <summary>
    /// Tests for the <see cref="OrchestrationServiceDeclarationParser"/> class.
    /// </summary>
    public class OrchestrationServiceDeclarationParserFeature
    {
        #region Constructor Tests

        /// <summary>
        /// Scenario tests that the object construction throws an exception when null model is passed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="e">The runner exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullModel(IBizTalkParser parser, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a parser"
                .x(() => parser.Should().BeNull());

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And null model"
                .x(() => model.Should().BeNull());

            "And a context"
                .x(() => context = new MigrationContext());

            "When constructing with null model"
                .x(() => e = Record.Exception(() => new OrchestrationServiceDeclarationParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when null context is passed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="e">The runner exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullContext(IBizTalkParser parser, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a parser"
                .x(() => parser.Should().BeNull());


            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a null context"
                .x(() => context.Should().BeNull());

            "When constructing with null context"
                .x(() => e = Record.Exception(() => new OrchestrationServiceDeclarationParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("context"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when null logger is passed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="e">The runner exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullLogger(IBizTalkParser parser, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a parser"
                .x(() => parser.Should().BeNull());

            "And null logger"
                .x(() => logger.Should().BeNull());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = new MigrationContext());

            "When constructing with null logger"
                .x(() => e = Record.Exception(() => new OrchestrationServiceDeclarationParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction succeeds.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="e">The runner exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithSuccess(IBizTalkParser parser, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a parser"
                .x(() => parser.Should().BeNull());

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = new MigrationContext());

            "When constructing"
                .x(() => e = Record.Exception(() => parser = new OrchestrationServiceDeclarationParser(model, context, logger)));

            "Then the parser constructor should succeed"
                .x(() =>
                {
                    e.Should().BeNull();
                    parser.Should().NotBeNull();
                });
        }

        #endregion

        #region Parse Tests

        /// <summary>
        /// Scenario tests parser skips if the source model is missing.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseIsSkippedIfModelIsMissing(OrchestrationServiceDeclarationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new OrchestrationServiceDeclarationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests the happy path when a service declaration message is parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseServiceDeclarationWithSuccess(OrchestrationServiceDeclarationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            var orchestrationDefinitionName = "orchestrationDefinitionName";
            var orchestrationDefinitionKey = "orchestrationDefinitionKey";
            var serviceDeclarationName = "serviceDeclarationName";
            var asmContainerKey = "asmContainerKey";

            "Given a source model with an orchestration and a service declaration"
               .x(() =>
               {
                   var odxModel = new MetaModel
                   {
                       Element = new Element[]
                       {
                            new Element
                            {
                                Type = "Module",
                                Element1 = new Element[]
                                {
                                    new Element
                                    {
                                        Type = "ServiceDeclaration",
                                        Property = new ElementProperty[]
                                        {
                                            new ElementProperty { Name = "Name", Value = serviceDeclarationName }
                                        }
                                    }
                                }
                            }
                         }
                   };

                   var orchestration = new Orchestration
                   {
                       Name = orchestrationDefinitionName,
                       ResourceContainerKey = asmContainerKey,
                       ResourceDefinitionKey = orchestrationDefinitionKey,
                       Model = odxModel
                   };

                   var parsedApplication = new ParsedBizTalkApplication
                   {
                       Application = new BizTalkApplication()
                   };

                   parsedApplication.Application.Orchestrations.Add(orchestration);

                   model = new AzureIntegrationServicesModel();
                   var group = new ParsedBizTalkApplicationGroup();
                   model.MigrationSource.MigrationSourceModel = group;
                   group.Applications.Add(parsedApplication);

                   parsedApplication.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                   var container = new ResourceContainer();
                   container.ResourceDefinitions.Add(new ResourceDefinition());
                   container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                   model.MigrationSource.ResourceContainers.Add(container);
               });

            "And one orchestration in the source model"
                .x(() =>
                {
                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = asmContainerKey, Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var orchestrationDefinition = new ResourceDefinition()
                    {
                        Key = orchestrationDefinitionKey,
                        Name = orchestrationDefinitionName,
                        Type = ModelConstants.ResourceDefinitionOrchestration
                    };
                    asmContainer.ResourceDefinitions.Add(orchestrationDefinition);

                    var metaModel = ((ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel).Applications[0].Application.Orchestrations[0].Model;
                    var metaModelResource = new ResourceItem()
                    {
                        Key = string.Concat(orchestrationDefinitionKey, ":", MetaModelConstants.MetaModelRootElement),
                        Name = MetaModelConstants.MetaModelRootElement,
                        Type = ModelConstants.ResourceMetaModel
                    };
                    metaModel.Resource = metaModelResource;
                    metaModelResource.SourceObject = metaModel;
                    orchestrationDefinition.Resources.Add(metaModelResource);

                    var moduleResource = new ResourceItem()
                    {
                        Key = string.Concat(metaModelResource.Key, ":", MetaModelConstants.ElementTypeModule),
                        Name = MetaModelConstants.ElementTypeModule,
                        Type = ModelConstants.ResourceModule
                    };
                    metaModelResource.Resources.Add(moduleResource);
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new OrchestrationServiceDeclarationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be no errors logged"
                 .x(() => context.Errors.Should().BeNullOrEmpty());

            "And the resources should be set."
                .x(() =>
                {
                    // Check the service declaration resource has been created.
                    var moduleResource = model.FindResourcesByType(ModelConstants.ResourceModule).SingleOrDefault();
                    moduleResource.Should().NotBeNull();
                    var serviceDeclarationResource = model.FindResourcesByType(ModelConstants.ResourceServiceDeclaration).SingleOrDefault();
                    serviceDeclarationResource.Should().NotBeNull();
                    var serviceDeclaration = ((ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel).Applications[0].Application.Orchestrations[0].FindServiceDeclaration();

                    // Validate the service declaration resource.
                    serviceDeclarationResource.Should().NotBeNull();
                    serviceDeclarationResource.Key.Should().Be(string.Concat(moduleResource.Key, ":", serviceDeclarationName));
                    serviceDeclarationResource.Name.Should().Be(serviceDeclarationName);
                    serviceDeclarationResource.Type.Should().Be(ModelConstants.ResourceServiceDeclaration);

                    serviceDeclaration.Resource.Should().Be(serviceDeclarationResource); // The pointer to the resource should be set.
                    serviceDeclarationResource.ParentRefId.Should().Be(moduleResource.RefId); // The parent ref ID should be set.
                    serviceDeclarationResource.SourceObject.Should().Be(serviceDeclaration); // The resource should have a pointer to the source object. 
                });
        }

        /// <summary>
        /// Scenario tests the happy path when a service declaration message is parsed with a message declaration.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseServiceDeclarationWithAMessageDeclarationSuccess(OrchestrationServiceDeclarationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            var orchestrationDefinitionName = "orchestrationDefinitionName";
            var orchestrationDefinitionKey = "orchestrationDefinitionKey";
            var serviceDeclarationName = "serviceDeclarationName";
            var messageDeclarationName = "messageDeclarationName";

            var asmContainerKey = "asmContainerKey";

            "Given a source model with an orchestration and a service declaration, with a message declaration"
               .x(() =>
               {
                   var odxModel = new MetaModel
                   {
                       Element = new Element[]
                       {
                            new Element
                            {
                                Type = "Module",
                                Element1 = new Element[]
                                {
                                    new Element
                                    {
                                        Type = "ServiceDeclaration",
                                        Property = new ElementProperty[]
                                        {
                                            new ElementProperty { Name = "Name", Value = serviceDeclarationName }
                                        },
                                        Element1 = new Element[]
                                        {
                                            new Element
                                            {
                                                Type = "MessageDeclaration",
                                                Property = new ElementProperty[]
                                                {
                                                    new ElementProperty{ Name = "Name", Value = messageDeclarationName }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                       }
                   };

                   var orchestration = new Orchestration
                   {
                       Name = orchestrationDefinitionName,
                       ResourceContainerKey = asmContainerKey,
                       ResourceDefinitionKey = orchestrationDefinitionKey,
                       Model = odxModel
                   };

                   var parsedApplication = new ParsedBizTalkApplication
                   {
                       Application = new BizTalkApplication()
                   };

                   parsedApplication.Application.Orchestrations.Add(orchestration);

                   model = new AzureIntegrationServicesModel();
                   var group = new ParsedBizTalkApplicationGroup();
                   model.MigrationSource.MigrationSourceModel = group;
                   group.Applications.Add(parsedApplication);

                   parsedApplication.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                   var container = new ResourceContainer();
                   container.ResourceDefinitions.Add(new ResourceDefinition());
                   container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                   model.MigrationSource.ResourceContainers.Add(container);
               });

            "And one orchestration in the source report model"
                .x(() =>
                {
                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = asmContainerKey, Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var orchestrationDefinition = new ResourceDefinition()
                    {
                        Key = orchestrationDefinitionKey,
                        Name = orchestrationDefinitionName,
                        Type = ModelConstants.ResourceDefinitionOrchestration
                    };
                    asmContainer.ResourceDefinitions.Add(orchestrationDefinition);

                    var metaModel = ((ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel).Applications[0].Application.Orchestrations[0].Model;
                    var metaModelResource = new ResourceItem()
                    {
                        Key = string.Concat(orchestrationDefinitionKey, ":", MetaModelConstants.MetaModelRootElement),
                        Name = MetaModelConstants.MetaModelRootElement,
                        Type = ModelConstants.ResourceMetaModel
                    };
                    metaModel.Resource = metaModelResource;
                    metaModelResource.SourceObject = metaModel;
                    orchestrationDefinition.Resources.Add(metaModelResource);

                    var moduleResource = new ResourceItem()
                    {
                        Key = string.Concat(metaModelResource.Key, ":", MetaModelConstants.ElementTypeModule),
                        Name = MetaModelConstants.ElementTypeModule,
                        Type = ModelConstants.ResourceModule
                    };
                    metaModelResource.Resources.Add(moduleResource);
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new OrchestrationServiceDeclarationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be no errors logged"
                 .x(() => context.Errors.Should().BeNullOrEmpty());

            "And the resources should be set."
                .x(() =>
                {
                    // Check the service declaration resource has been created.
                    var moduleResource = model.FindResourcesByType(ModelConstants.ResourceModule).SingleOrDefault();
                    moduleResource.Should().NotBeNull();
                    var serviceDeclarationResource = model.FindResourcesByType(ModelConstants.ResourceServiceDeclaration).SingleOrDefault();
                    serviceDeclarationResource.Should().NotBeNull();
                    var serviceDeclaration = ((ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel).Applications[0].Application.Orchestrations[0].FindServiceDeclaration();

                    // Validate the service declaration resource.
                    serviceDeclarationResource.Should().NotBeNull();
                    serviceDeclarationResource.Key.Should().Be(string.Concat(moduleResource.Key, ":", serviceDeclarationName));
                    serviceDeclarationResource.Name.Should().Be(serviceDeclarationName);
                    serviceDeclarationResource.Type.Should().Be(ModelConstants.ResourceServiceDeclaration);

                    // Validate the message declaration resource.
                    var messageDeclaration = serviceDeclarationResource.Resources[0];
                    messageDeclaration.Name.Should().Be(messageDeclarationName);
                    messageDeclaration.Type.Should().Be(ModelConstants.ResourceMessageDeclaration);
                    messageDeclaration.Key.Should().StartWith(serviceDeclarationResource.Key);
                });
        }

        /// <summary>
        /// Scenario tests the happy path when a service declaration message is parsed with a correlation declaration.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseServiceDeclarationWithACorrelationDeclarationSuccess(OrchestrationServiceDeclarationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            var orchestrationDefinitionName = "orchestrationDefinitionName";
            var orchestrationDefinitionKey = "orchestrationDefinitionKey";
            var serviceDeclarationName = "serviceDeclarationName";
            var correlationDeclarationName = "correlationDeclarationName";

            var asmContainerKey = "asmContainerKey";

            "Given a source model with an orchestration and a service declaration, with a correlation declaration"
               .x(() =>
               {
                   var odxModel = new MetaModel
                   {
                       Element = new Element[]
                       {
                            new Element
                            {
                                Type = "Module",
                                Element1 = new Element[]
                                {
                                    new Element
                                    {
                                        Type = "ServiceDeclaration",
                                        Property = new ElementProperty[]
                                        {
                                            new ElementProperty { Name = "Name", Value = serviceDeclarationName }
                                        },
                                        Element1 = new Element[]
                                        {
                                            new Element
                                            {
                                                Type = "CorrelationDeclaration",
                                                Property = new ElementProperty[]
                                                {
                                                    new ElementProperty{ Name = "Name", Value = correlationDeclarationName }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                         }
                   };

                   var orchestration = new Orchestration
                   {
                       Name = orchestrationDefinitionName,
                       ResourceContainerKey = asmContainerKey,
                       ResourceDefinitionKey = orchestrationDefinitionKey,
                       Model = odxModel
                   };

                   var parsedApplication = new ParsedBizTalkApplication
                   {
                       Application = new BizTalkApplication()
                   };

                   parsedApplication.Application.Orchestrations.Add(orchestration);

                   model = new AzureIntegrationServicesModel();
                   var group = new ParsedBizTalkApplicationGroup();
                   model.MigrationSource.MigrationSourceModel = group;
                   group.Applications.Add(parsedApplication);

                   parsedApplication.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                   var container = new ResourceContainer();
                   container.ResourceDefinitions.Add(new ResourceDefinition());
                   container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                   model.MigrationSource.ResourceContainers.Add(container); 
               });

            "And one orchestration in the source report model"
                .x(() =>
                {
                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = asmContainerKey, Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var orchestrationDefinition = new ResourceDefinition()
                    {
                        Key = orchestrationDefinitionKey,
                        Name = orchestrationDefinitionName,
                        Type = ModelConstants.ResourceDefinitionOrchestration
                    };
                    asmContainer.ResourceDefinitions.Add(orchestrationDefinition);

                    var metaModel = ((ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel).Applications[0].Application.Orchestrations[0].Model;
                    var metaModelResource = new ResourceItem()
                    {
                        Key = string.Concat(orchestrationDefinitionKey, ":", MetaModelConstants.MetaModelRootElement),
                        Name = MetaModelConstants.MetaModelRootElement,
                        Type = ModelConstants.ResourceMetaModel
                    };
                    metaModel.Resource = metaModelResource;
                    metaModelResource.SourceObject = metaModel;
                    orchestrationDefinition.Resources.Add(metaModelResource);

                    var moduleResource = new ResourceItem()
                    {
                        Key = string.Concat(metaModelResource.Key, ":", MetaModelConstants.ElementTypeModule),
                        Name = MetaModelConstants.ElementTypeModule,
                        Type = ModelConstants.ResourceModule
                    };
                    metaModelResource.Resources.Add(moduleResource);
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new OrchestrationServiceDeclarationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be no errors logged"
                 .x(() => context.Errors.Should().BeNullOrEmpty());

            "And the resources should be set."
                .x(() =>
                {
                    // Check the service declaration resource has been created.
                    var moduleResource = model.FindResourcesByType(ModelConstants.ResourceModule).SingleOrDefault();
                    moduleResource.Should().NotBeNull();
                    var serviceDeclarationResource = model.FindResourcesByType(ModelConstants.ResourceServiceDeclaration).SingleOrDefault();
                    serviceDeclarationResource.Should().NotBeNull();
                    var serviceDeclaration = ((ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel).Applications[0].Application.Orchestrations[0].FindServiceDeclaration();

                    // Validate the service declaration resource.
                    serviceDeclarationResource.Should().NotBeNull();
                    serviceDeclarationResource.Key.Should().Be(string.Concat(moduleResource.Key, ":", serviceDeclarationName));
                    serviceDeclarationResource.Name.Should().Be(serviceDeclarationName);
                    serviceDeclarationResource.Type.Should().Be(ModelConstants.ResourceServiceDeclaration);

                    // Validate the correlation declaration resource.
                    var correlationDeclarationNameResource = serviceDeclarationResource.Resources[0];
                    correlationDeclarationNameResource.Name.Should().Be(correlationDeclarationName);
                    correlationDeclarationNameResource.Type.Should().Be(ModelConstants.ResourceCorrelationDeclaration);
                    correlationDeclarationNameResource.Key.Should().StartWith(serviceDeclarationResource.Key);
                });
        }

        /// <summary>
        /// Scenario tests the happy path when a service declaration message is parsed with a port declaration.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseServiceDeclarationWithAPortDeclarationSuccess(OrchestrationServiceDeclarationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            var orchestrationDefinitionName = "orchestrationDefinitionName";
            var orchestrationDefinitionKey = "orchestrationDefinitionKey";
            var serviceDeclarationName = "serviceDeclarationName";
            var portDeclarationName = "portDeclarationName";

            var asmContainerKey = "asmContainerKey";

            "Given a source model with an orchestration and a service declaration, with a port declaration"
               .x(() =>
               {
                   var odxModel = new MetaModel
                   {
                       Element = new Element[]
                       {
                           new Element
                           {
                               Type = "Module",
                               Element1 = new Element[]
                               {
                                   new Element
                                   {
                                       Type = "ServiceDeclaration",
                                       Property = new ElementProperty[]
                                       {
                                           new ElementProperty { Name = "Name", Value = serviceDeclarationName }
                                       },
                                       Element1 = new Element[]
                                       {
                                           new Element
                                           {
                                               Type = "PortDeclaration",
                                               Property = new ElementProperty[]
                                               {
                                                   new ElementProperty{ Name = "Name", Value = portDeclarationName }
                                               }
                                           }
                                       }
                                   }
                               }
                           }
                       }
                   };
               
                   var orchestration = new Orchestration
                   {
                       Name = orchestrationDefinitionName,
                       ResourceContainerKey = asmContainerKey,
                       ResourceDefinitionKey = orchestrationDefinitionKey,
                       Model = odxModel
                   };

                   var parsedApplication = new ParsedBizTalkApplication
                   {
                       Application = new BizTalkApplication()
                   };

                   parsedApplication.Application.Orchestrations.Add(orchestration);

                   model = new AzureIntegrationServicesModel();
                   var group = new ParsedBizTalkApplicationGroup();
                   model.MigrationSource.MigrationSourceModel = group;
                   group.Applications.Add(parsedApplication);

                   parsedApplication.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                   var container = new ResourceContainer();
                   container.ResourceDefinitions.Add(new ResourceDefinition());
                   container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                   model.MigrationSource.ResourceContainers.Add(container);
               });

            "And one orchestration in the source report model"
                .x(() =>
                {
                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = asmContainerKey, Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var orchestrationDefinition = new ResourceDefinition()
                    {
                        Key = orchestrationDefinitionKey,
                        Name = orchestrationDefinitionName,
                        Type = ModelConstants.ResourceDefinitionOrchestration
                    };
                    asmContainer.ResourceDefinitions.Add(orchestrationDefinition);

                    var metaModel = ((ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel).Applications[0].Application.Orchestrations[0].Model;
                    var metaModelResource = new ResourceItem()
                    {
                        Key = string.Concat(orchestrationDefinitionKey, ":", MetaModelConstants.MetaModelRootElement),
                        Name = MetaModelConstants.MetaModelRootElement,
                        Type = ModelConstants.ResourceMetaModel
                    };
                    metaModel.Resource = metaModelResource;
                    metaModelResource.SourceObject = metaModel;
                    orchestrationDefinition.Resources.Add(metaModelResource);

                    var moduleResource = new ResourceItem()
                    {
                        Key = string.Concat(metaModelResource.Key, ":", MetaModelConstants.ElementTypeModule),
                        Name = MetaModelConstants.ElementTypeModule,
                        Type = ModelConstants.ResourceModule
                    };
                    metaModelResource.Resources.Add(moduleResource);
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new OrchestrationServiceDeclarationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be no errors logged"
                 .x(() => context.Errors.Should().BeNullOrEmpty());

            "And the resources should be set."
                .x(() =>
                {
                    // Check the service declaration resource has been created.
                    var moduleResource = model.FindResourcesByType(ModelConstants.ResourceModule).SingleOrDefault();
                    moduleResource.Should().NotBeNull();
                    var serviceDeclarationResource = model.FindResourcesByType(ModelConstants.ResourceServiceDeclaration).SingleOrDefault();
                    serviceDeclarationResource.Should().NotBeNull();
                    var serviceDeclaration = ((ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel).Applications[0].Application.Orchestrations[0].FindServiceDeclaration();

                    // Validate the service declaration resource.
                    serviceDeclarationResource.Should().NotBeNull();
                    serviceDeclarationResource.Key.Should().Be(string.Concat(moduleResource.Key, ":", serviceDeclarationName));
                    serviceDeclarationResource.Name.Should().Be(serviceDeclarationName);
                    serviceDeclarationResource.Type.Should().Be(ModelConstants.ResourceServiceDeclaration);

                    // Validate the port declaration resource.
                    var portDeclarationNameResource = serviceDeclarationResource.Resources[0];
                    portDeclarationNameResource.Name.Should().Be(portDeclarationName);
                    portDeclarationNameResource.Type.Should().Be(ModelConstants.ResourcePortDeclaration);
                    portDeclarationNameResource.Key.Should().StartWith(serviceDeclarationResource.Key);
                });
        }

        /// <summary>
        /// Scenario tests code fails if the orchestration module is missing.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParsePortTypeWithMissingModule(OrchestrationServiceDeclarationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            var orchestrationDefinitionName = "orchestrationDefinitionName";
            var orchestrationDefinitionKey = "orchestrationDefinitionKey";
            var asmContainerKey = "asmContainerKey";
            var wrongKey = "wrongKey";

            "Given a source model with an orchestration with a missing module"
               .x(() =>
               {
                   var orchestration = new Orchestration
                   {
                       Name = orchestrationDefinitionName,
                       ResourceContainerKey = asmContainerKey,
                       ResourceDefinitionKey = wrongKey
                   };

                   var parsedApplication = new ParsedBizTalkApplication
                   {
                       Application = new BizTalkApplication()
                   };

                   parsedApplication.Application.Orchestrations.Add(orchestration);

                   model = new AzureIntegrationServicesModel();
                   var group = new ParsedBizTalkApplicationGroup();
                   model.MigrationSource.MigrationSourceModel = group;
                   group.Applications.Add(parsedApplication);

               });

            "And one orchestration in the source report model"
                .x(() =>
                {
                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = asmContainerKey, Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var orchestrationDefinition = new ResourceDefinition()
                    {
                        Key = orchestrationDefinitionKey,
                        Name = orchestrationDefinitionName,
                        Type = ModelConstants.ResourceDefinitionOrchestration
                    };
                    asmContainer.ResourceDefinitions.Add(orchestrationDefinition);

                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new OrchestrationServiceDeclarationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be an error logged"
                 .x(() =>
                 {
                     context.Errors.Should().NotBeNull();
                     context.Errors.Should().HaveCount(1);
                     context.Errors[0].Message.Should().Contain(wrongKey);
                     context.Errors[0].Message.Should().Contain(ModelConstants.ResourceModule);
                 });
        }

        #endregion
    }
}
