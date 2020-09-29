// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse.Tests
{
    /// <summary>
    /// Tests for the <see cref="TransformParser"/> class.
    /// </summary>
    public class TransformParserFeature
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
                .x(() => e = Record.Exception(() => new TransformParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new TransformParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new TransformParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => parser = new TransformParser(model, context, logger)));

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
        public void ParseIsSkippedIfModelIsMissing(TransformParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
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
                .x(() => parser = new TransformParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests the happy path when a transform is parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseTransformWithSuccess(TransformParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with a transform"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = "TestAssembly.Key", Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var transformResourceDefinition = new ResourceDefinition() 
                    { 
                        Key = "transformDefinitionKey", 
                        Name = "transformName", 
                        Type = ModelConstants.ResourceDefinitionMap                         
                    };

                    asmContainer.ResourceDefinitions.Add(transformResourceDefinition);

                    var application = new BizTalkApplication() { Name = "ApplicationOne" };

                    var transform = new Types.Entities.Transform
                    {
                        Name = "transformName",
                        ResourceContainerKey = asmContainer.Key,
                        ResourceDefinitionKey = transformResourceDefinition.Key                        
                    };

                    application.Transforms.Add(transform);
                
                    group.Applications.Add(
                        new ParsedBizTalkApplication
                        {
                            Application = application
                        });
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new TransformParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the resources should be set."
                .x(() =>
                {
                    var appModel = (AzureIntegrationServicesModel)model;

                    // Check the transform resource has been created.
                    appModel.MigrationSource.ResourceContainers[0].ResourceContainers[0].ResourceContainers[0].ResourceDefinitions[0].Resources.Should().NotBeNullOrEmpty();
                    appModel.MigrationSource.ResourceContainers[0].ResourceContainers[0].ResourceContainers[0].ResourceDefinitions[0].Resources.Should().HaveCount(1);
                    var transformResourceDefinition = appModel.MigrationSource.ResourceContainers[0].ResourceContainers[0].ResourceContainers[0].ResourceDefinitions[0];
                    var transformResource = transformResourceDefinition.Resources[0];

                    // Get the transform.
                    var transform = group.Applications[0].Application.Transforms[0];

                    // Validate the transform resource.
                    transformResource.Should().NotBeNull();
                    transformResource.Key.Should().Be(transformResourceDefinition.Key + ":map");
                    transformResource.Name.Should().Be(transform.Name);
                    transformResource.Type.Should().Be(ModelConstants.ResourceMap);
                    transformResource.Description.Should().Be(transform.FullName);

                    transform.Resource.Should().Be(transformResource); // The pointer to the resource should be set.
                    transformResource.ParentRefId.Should().Be(transformResourceDefinition.RefId); // The parent ref ID should be set.
                    transformResource.SourceObject.Should().Be(transform); // The resource should have a pointer to the source object.
                });
        }

        /// <summary>
        /// Scenario tests the transform fails if the definition is missing.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailsWhenTransformDefinitionIsMissing(TransformParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var missingResourceDefinitionKey = "missingResourceDefinitionKey";
            
            "Given a model with a transform and missing its definition"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = "TestAssembly.Key", Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var transformResourceDefinition = new ResourceDefinition()
                    {
                        Key = "transformDefinitionKey",
                        Name = "transformName",
                        Type = ModelConstants.ResourceDefinitionMap
                    };

                    asmContainer.ResourceDefinitions.Add(transformResourceDefinition);

                    var application = new BizTalkApplication() { Name = "ApplicationOne" };

                    var transform = new Types.Entities.Transform
                    {
                        Name = "transformName",
                        ResourceContainerKey = asmContainer.Key,
                        ResourceDefinitionKey = missingResourceDefinitionKey
                    };

                    application.Transforms.Add(transform);

                    group.Applications.Add(
                        new ParsedBizTalkApplication
                        {
                            Application = application
                        });
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new TransformParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be an error logged"
                .x(() => 
                {
                    context.Errors.Should().NotBeNull();
                    context.Errors.Should().HaveCount(1);
                    context.Errors[0].Message.Should().Contain(missingResourceDefinitionKey);
                    context.Errors[0].Message.Should().Contain(ModelConstants.ResourceDefinitionMap);
                });
        }

        #endregion
    }
}
