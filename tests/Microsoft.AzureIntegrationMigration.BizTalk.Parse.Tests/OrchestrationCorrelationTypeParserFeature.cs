// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
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
    /// Tests for the <see cref="OrchestrationCorrelationTypeParser"/> class.
    /// </summary>
    public class OrchestrationCorrelationTypeParserFeature
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
                .x(() => e = Record.Exception(() => new OrchestrationCorrelationTypeParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new OrchestrationCorrelationTypeParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new OrchestrationCorrelationTypeParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => parser = new OrchestrationCorrelationTypeParser(model, context, logger)));

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
        public void ParseIsSkippedIfModelIsMissing(OrchestrationCorrelationTypeParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
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
                .x(() => parser = new OrchestrationCorrelationTypeParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests the happy path when a correlation type is parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseCorrelationTypeWithSuccess(OrchestrationCorrelationTypeParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            var orchestrationDefinitionName = "orchestrationDefinitionName";
            var orchestrationDefinitionKey = "orchestrationDefinitionKey";
            var correlationTypeName = "correlationTypeName";
            var asmContainerKey = "asmContainerKey";

            "Given a source model with an orchestration and a correlation type"
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
                                        Type = "CorrelationType",
                                        Property = new ElementProperty[]
                                        {
                                            new ElementProperty { Name = "Name", Value = correlationTypeName }
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
                .x(() => parser = new OrchestrationCorrelationTypeParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And the resources should be set."
                .x(() =>
                {
                    // Check the correlation type resource has been created.
                    var moduleResource = model.FindResourcesByType(ModelConstants.ResourceModule).SingleOrDefault();
                    moduleResource.Should().NotBeNull();
                    var correlationTypeResource = model.FindResourcesByType(ModelConstants.ResourceCorrelationType).SingleOrDefault();
                    correlationTypeResource.Should().NotBeNull();
                    var correlationType = ((ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel).Applications[0].Application.Orchestrations[0].FindCorrelationTypes().Single();

                    // Validate the correlation type resource.
                    correlationTypeResource.Should().NotBeNull();
                    correlationTypeResource.Key.Should().Be(string.Concat(moduleResource.Key, ":", correlationTypeName));
                    correlationTypeResource.Name.Should().Be(correlationTypeName);
                    correlationTypeResource.Type.Should().Be(ModelConstants.ResourceCorrelationType);

                    correlationType.Resource.Should().Be(correlationTypeResource); // The pointer to the resource should be set.
                    correlationTypeResource.ParentRefId.Should().Be(moduleResource.RefId); // The parent ref ID should be set.
                    correlationTypeResource.SourceObject.Should().Be(correlationType); // The resource should have a pointer to the source object.            
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
        public void ParseCorrelationTypeWithMissingModule(OrchestrationCorrelationTypeParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
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
                       ResourceDefinitionKey = wrongKey,
                       Model = new MetaModel()
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
                .x(() => parser = new OrchestrationCorrelationTypeParser(model, context, logger));

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
