// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="BizTalkOrchestrationParserFeature.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Defines the test spec for the BizTalkOrchestrationParser class.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
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

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse.Tests
{
#pragma warning disable CA1303 // Do not pass literals as localized parameters

    /// <summary>
    /// Defines the test spec for the <see cref="BizTalkOrchestrationParser"/> class.
    /// </summary>
    public class BizTalkOrchestrationParserFeature
    {
        /// <summary>
        /// Helper method to serialize a <see cref="MetaModel"/> to a string.
        /// </summary>
        /// <param name="metaModel">The model to serialize.</param>
        /// <returns>The serialized model.</returns>
        private static string SerializeToString(MetaModel model)
        {
            var serializer = new XmlSerializer(typeof(MetaModel));

            using var writer = new StringWriter();
            serializer.Serialize(writer, model);
            return writer.ToString();
        }

        #region Constructor Tests

        /// <summary>
        /// Scenario tests that the object construction throws an exception when null model is passed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
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
                .x(() => e = Record.Exception(() => new BizTalkOrchestrationParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when null context is passed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
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
                .x(() => e = Record.Exception(() => new BizTalkOrchestrationParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new BizTalkOrchestrationParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction succeeds.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
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
                .x(() => e = Record.Exception(() => parser = new BizTalkOrchestrationParser(model, context, logger)));

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
        public void ParseIsSkippedIfModelIsMissing(BizTalkOrchestrationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
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
                .x(() => parser = new BizTalkOrchestrationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests when the ODX contains invalid XML, an error is added to the collection.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The parser exception.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureInvalidXml(BizTalkOrchestrationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with invalid XML"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(new ParsedBizTalkApplication()
                    {
                        Application = new BizTalkApplication()
                    });

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = "TestAssembly.Key", Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var orchestration = new ResourceDefinition() { Key = "TestOrchestration.Key", Name = "TestOrchestration", Type = ModelConstants.ResourceDefinitionOrchestration, ResourceContent = @"Not valid XML" };
                    asmContainer.ResourceDefinitions.Add(orchestration);

                    group.Applications[0].Application.Orchestrations.Add(new Orchestration(asmContainer.Key, orchestration.Key) { Name = "Test Name" });
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a BizTalk Orchestration Parser"
                .x(() => parser = new BizTalkOrchestrationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the error count should be 1"
                .x(() =>
                {
                    context.Errors.Should().NotBeNull().And.HaveCount(1);
                });

            "And the error should indicate a failure with parsing the XML"
                .x(() =>
                {
                    context.Errors[0].Message.Should().Contain("error in XML document");
                });
        }

        /// <summary>
        /// Scenario tests when an application is parsed with one application and no orchestrations.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The parser exception.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseWithOneApplicationAndNoOrchestrations(BizTalkOrchestrationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one application and no orchestrations"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(new ParsedBizTalkApplication { Application = new BizTalkApplication() });
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a BizTalk Orchestration Parser"
               .x(() => parser = new BizTalkOrchestrationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the error count should be 0"
                .x(() =>
                {
                    context.Errors.Should().BeNullOrEmpty();
                });
        }

        /// <summary>
        /// Scenario tests when an application is parsed with one application and one orchestration.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="metaModel">The model to parse.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The parser exception.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseWithOneApplicationAndOneOrchestration(BizTalkOrchestrationParser parser, ILogger logger, MigrationContext context, MetaModel metaModel, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one application and an orchestration with valid ODX"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;

                    var parsedApplication = new ParsedBizTalkApplication
                    {
                        Application = new BizTalkApplication()
                    };
                    parsedApplication.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                    group.Applications.Add(parsedApplication);

                    var container = new ResourceContainer();
                    container.ResourceDefinitions.Add(new ResourceDefinition());
                    container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                    model.MigrationSource.ResourceContainers.Add(container);
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object); 
            
            "And a context"
                .x(() => context = new MigrationContext());

            "And one orchestration with a valid ODX"
                .x(() =>
                {
                    metaModel = new MetaModel
                    {
                        Core = "Core",
                        Element = new Element[]
                        {
                            new Element { Type = MetaModelConstants.ElementTypeModule }
                        },
                        MajorVersion = "MajorVersion",
                        MinorVersion = "MinorVersion"
                    };

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = "TestAssembly.Key", Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var orchestration = new ResourceDefinition() { Key = "TestOrchestration.Key", Name = "TestOrchestration", Type = ModelConstants.ResourceDefinitionOrchestration, ResourceContent = SerializeToString(metaModel) };
                    asmContainer.ResourceDefinitions.Add(orchestration);

                    group.Applications[0].Application.Orchestrations.Add(new Orchestration(asmContainer.Key, orchestration.Key)
                    {
                        Name = "Test Orchestration"
                    });
                });

            "And a BizTalk Orchestration Parser"
               .x(() => parser = new BizTalkOrchestrationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the error count should be 0"
                .x(() =>
                {
                    context.Errors.Should().BeNullOrEmpty();
                });

            "And the model on the orchestration should be equivalent to the ODX"
                .x(() =>
                {
                    // Validate the structure of the model.
                    group.Applications[0].Application.Orchestrations.Should().NotBeNullOrEmpty().And.HaveCount(1);
                    var orchestration = group.Applications[0].Application.Orchestrations[0];
                    group.Applications[0].Application.Orchestrations[0].Model.Should().NotBeNull();

                    // Validate the contents of the model.
                    orchestration.Model.MajorVersion.Should().Be(metaModel.MajorVersion);
                    orchestration.Model.MinorVersion.Should().Be(metaModel.MinorVersion);
                    orchestration.Model.Core.Should().Be(metaModel.Core);
                    orchestration.Model.Element.Should().NotBeNullOrEmpty();
                    orchestration.Model.Element.Should().HaveCount(metaModel.Element.Length);
                });
        }

        /// <summary>
        /// Scenario tests when an application is parsed with two applications and only one has an orchestration.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="applicationOne">The first application to parse.</param>
        /// <param name="applicationTwo">The second application to parse.</param>
        /// <param name="metaModel">The model which will be parsed.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The parser exception.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseWithTwoApplicationsAndOneOrchestration(BizTalkOrchestrationParser parser, ILogger logger, MigrationContext context, BizTalkApplication applicationOne, BizTalkApplication applicationTwo, MetaModel metaModel, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object); 
            
            "And a context"
                .x(() => context = new MigrationContext());

            "And an application with one orchestration with a valid ODX"
                .x(() =>
                {
                    metaModel = new MetaModel
                    {
                        Core = "Core",
                        Element = new Element[]
                        {
                            new Element { Type = MetaModelConstants.ElementTypeModule }
                        },
                        MajorVersion = "MajorVersion",
                        MinorVersion = "MinorVersion"
                    };

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = "TestAssembly.Key", Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var orchestration = new ResourceDefinition() { Key = "TestOrchestration.Key", Name = "TestOrchestration", Type = ModelConstants.ResourceDefinitionOrchestration, ResourceContent = SerializeToString(metaModel) };
                    asmContainer.ResourceDefinitions.Add(orchestration);

                    applicationOne = new BizTalkApplication() { Name = "ApplicationOne" };
                    applicationOne.Orchestrations.Add(new Orchestration(asmContainer.Key, orchestration.Key)
                    {
                        Name = "OrchestrationOne"
                    });
                    applicationOne.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };

                    var parsedApplication = new ParsedBizTalkApplication
                    {
                        Application = applicationOne
                    };
                    group.Applications.Add(parsedApplication);

                    var container = new ResourceContainer();
                    container.ResourceDefinitions.Add(new ResourceDefinition());
                    container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                    model.MigrationSource.ResourceContainers.Add(container);
                });

            "And an application with no orchestrations"
                 .x(() =>
                 {
                     applicationTwo = new BizTalkApplication() { Name = "ApplicationTwo" };

                     group.Applications.Add(
                        new ParsedBizTalkApplication
                        {
                            Application = applicationTwo
                        });
                 });

            "And a BizTalk Orchestration Parser"
               .x(() => parser = new BizTalkOrchestrationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the error count should be 0"
                .x(() =>
                {
                    context.Errors.Should().BeNullOrEmpty();
                });

            "And the model on the orchestration should be equivalent to the ODX for the first application"
                .x(() =>
                {
                    // Validate the structure of the model.
                    group.Applications[0].Application.Orchestrations.Should().NotBeNullOrEmpty().And.HaveCount(1);
                    var orchestration = group.Applications[0].Application.Orchestrations[0];
                    group.Applications[0].Application.Orchestrations[0].Model.Should().NotBeNull();

                    // Validate the contents of the model.
                    orchestration.Model.MajorVersion.Should().Be(metaModel.MajorVersion);
                    orchestration.Model.MinorVersion.Should().Be(metaModel.MinorVersion);
                    orchestration.Model.Core.Should().Be(metaModel.Core);
                    orchestration.Model.Element.Should().NotBeNullOrEmpty();
                    orchestration.Model.Element.Should().HaveCount(metaModel.Element.Length);
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
