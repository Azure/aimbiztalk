// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="BizTalkPipelineParserFeature.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Defines the test spec for the BizTalkPipelineParser class.</summary>
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
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
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
    /// Defines the test spec for the <see cref="BizTalkPipelineParser"/> class.
    /// </summary>
    public class BizTalkPipelineParserFeature
    {
        /// <summary>
        /// Helper method to serialize a <see cref="Document"/> to a string.
        /// </summary>
        /// <param name="document">The document to serialize.</param>
        /// <returns>The serialized document.</returns>
        private static string SerializeToString(Document document)
        {
            var serializer = new XmlSerializer(typeof(Document));

            using var writer = new StringWriter();
            serializer.Serialize(writer, document);
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
                .x(() => e = Record.Exception(() => new BizTalkPipelineParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new BizTalkPipelineParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new BizTalkPipelineParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => parser = new BizTalkPipelineParser(model, context, logger)));

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
        public void ParseIsSkippedIfModelIsMissing(BizTalkPipelineParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
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
                .x(() => parser = new BizTalkPipelineParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests when the Pipeline XML contains invalid XML, an error is added to the collection.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The parser exception.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureInvalidXml(BizTalkPipelineParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with invalid XML for an Pipeline"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(new ParsedBizTalkApplication()
                    {
                        Application = new BizTalkApplication
                        {
                            ApplicationDefinition = new ApplicationDefinitionFile {  ResourceKey = "ResourceKey" }
                        }
                    });

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = "TestAssembly.Key", Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var pipelineResourceDefinition = new ResourceDefinition() { Key = "TestPipeline.Key", Name = "TestPipeline", Type = ModelConstants.ResourceDefinitionReceivePipeline, ResourceContent = @"Not valid XML" };
                    asmContainer.ResourceDefinitions.Add(pipelineResourceDefinition);

                    group.Applications[0].Application.Pipelines.Add(new Pipeline(asmContainer.Key, pipelineResourceDefinition.Key)
                    {
                        Name = "Test Pipeline Name",
                        Direction = PipelineDirection.Receive,                        
                    });
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);
            
            "And a context"
                .x(() => context = new MigrationContext());

            "And a BizTalk Pipeline Parser"
                .x(() => parser = new BizTalkPipelineParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the error count should be 1"
                .x(() =>
                {
                    context.Errors.Should().NotBeNull().And.HaveCount(1);
                });

            "And the error should indicate a failure parsing the XML"
                .x(() =>
                {
                    context.Errors[0].Message.Should().Contain("error in XML document");
                });
        }

        /// <summary>
        /// Scenario tests when an application is parsed with one application and no pipelines.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The parser exception.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseWithOneApplicationAndNoPipelines(BizTalkPipelineParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one application and no pipelines"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(new ParsedBizTalkApplication() 
                    { 
                        Application = new BizTalkApplication
                        {
                            ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" }
                        }
                    });
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);
            
            "And a context"
                .x(() => context = new MigrationContext());

            "And a BizTalk Pipeline Parser"
               .x(() => parser = new BizTalkPipelineParser(model, context, logger));

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
        /// Scenario tests when an application is parsed with one application and one pipeline.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="document">The document to parse.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The parser exception.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseWithOneApplicationAndOnePipeline(BizTalkPipelineParser parser, ILogger logger, MigrationContext context, Document document, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one application"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(new ParsedBizTalkApplication()
                    {
                        Application = new BizTalkApplication
                        {
                            ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" }
                        }
                    });
                    
                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer());
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition());
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });

                });

            "And a logger"
               .x(() => logger = new Mock<ILogger>().Object);
            
            "And a context"
                .x(() => context = new MigrationContext());

            "And one pipeline with valid pipeline XML document"
                .x(() =>
                {
                    document = new Document
                    {
                        FriendlyName = "Friendly Name",
                        Description = "Description",
                        Stages = new DocumentStage[1]
                        {
                            new DocumentStage
                            {
                                Components = new DocumentStageComponent[1]
                                {
                                    new DocumentStageComponent
                                    {
                                        Name = "Staged Component",
                                        ComponentName = "Component Name"
                                    }
                                }
                            }
                        }
                    };

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = "TestAssembly.Key", Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var pipelineResourceDefinition = new ResourceDefinition() { Key = "TestPipeline.Key", Name = "TestPipeline", Type = ModelConstants.ResourceDefinitionSendPipeline, ResourceContent = SerializeToString(document) };
                    asmContainer.ResourceDefinitions.Add(pipelineResourceDefinition);

                    group.Applications[0].Application.Pipelines.Add(new Pipeline(asmContainer.Key, pipelineResourceDefinition.Key)
                    {
                        Name = "Test Pipeline",
                        Direction = PipelineDirection.Send,
                        Description = "Test Pipeline Description 1."
                    });
                });

            "And a BizTalk Pipeline Parser"
               .x(() => parser = new BizTalkPipelineParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the error count should be 0"
                .x(() =>
                {
                    context.Errors.Should().BeNullOrEmpty();
                });

            "And the document on the pipeline should be equivalent to the XML"
                .x(() =>
                {
                    // Validate the structure of the application.
                    group.Applications[0].Application.Pipelines.Should().NotBeNullOrEmpty().And.HaveCount(1);
                    var pipeline = group.Applications[0].Application.Pipelines[0];
                    group.Applications[0].Application.Pipelines[0].Document.Should().NotBeNull();

                    // Validate the contents of the document.
                    pipeline.Document.FriendlyName.Should().Be(document.FriendlyName);
                    pipeline.Document.Description.Should().Be(document.Description);
                    pipeline.Document.Stages.Should().NotBeNullOrEmpty();
                    pipeline.Document.Stages.Should().HaveCount(document.Stages.Length);
                    pipeline.Document.Stages[0].Components.Should().NotBeNull();
                    pipeline.Document.Stages[0].Components.Should().NotBeNull();
                    pipeline.Document.Stages[0].Components[0].ComponentName.Should().Be(document.Stages[0].Components[0].ComponentName);
                    pipeline.Document.Stages[0].Components[0].Name.Should().Be(document.Stages[0].Components[0].Name);
                });

            "And the pipeline resource should be created"
                .x(() =>
                {
                    var pipeline = group.Applications[0].Application.Pipelines[0];
                    var pipelineResourceDefinition = model.MigrationSource.ResourceContainers[1].ResourceContainers[0].ResourceContainers[0].ResourceDefinitions[0];
                    pipelineResourceDefinition.Should().NotBeNull();
                    pipelineResourceDefinition.Resources.Should().NotBeNullOrEmpty();
                    pipelineResourceDefinition.Resources.Should().HaveCount(1);

                    var pipelineResource = pipelineResourceDefinition.Resources[0];
                    pipelineResource.Name.Should().Be(pipeline.Name);
                    pipelineResource.Type.Should().Be(ModelConstants.ResourceSendPipeline);
                    pipelineResource.Key.Should().NotBeNullOrEmpty();
                    pipelineResource.Key.Should().Be(pipeline.ResourceKey);
                    pipelineResource.Description.Should().Be(pipeline.Description);

                    pipeline.Resource.Should().Be(pipelineResource); // The pointer to the resource should be set.
                    pipelineResource.ParentRefId.Should().Be(pipelineResourceDefinition.RefId); // The parent ref ID should be set.
                    pipelineResource.SourceObject.Should().Be(pipeline); // The resource should have a pointer to the source object.

                });
        }

        /// <summary>
        /// Scenario tests when an application is parsed with two applications and only one has a pipeline.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="applicationOne">The first application to parse.</param>
        /// <param name="applicationTwo">The second application to parse.</param>
        /// <param name="document">The document which will be parsed.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The parser exception.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseWithTwoApplicationsAndOnePipeline(BizTalkPipelineParser parser, ILogger logger, MigrationContext context, BizTalkApplication applicationOne, BizTalkApplication applicationTwo, Document document, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one application"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
               .x(() => logger = new Mock<ILogger>().Object);
            
            "And an application with one pipeline with valid XML"
                .x(() =>
                {
                    document = new Document
                    {
                        FriendlyName = "Friendly Name",
                        Description = "Description",
                        Stages = new DocumentStage[1]
                        {
                            new DocumentStage
                            {
                                Components = new DocumentStageComponent[]
                                {
                                    new DocumentStageComponent { Name = "Staged Component" }
                                }
                            }
                        }
                    };

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    msiContainer.ResourceContainers.Add(cabContainer);

                    var asmContainer = new ResourceContainer() { Key = "TestAssembly.Key", Name = "TestAssembly", Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = @"C:\Test\Test.dll" };
                    cabContainer.ResourceContainers.Add(asmContainer);

                    var pipeline = new ResourceDefinition() { Key = "TestPipeline.Key", Name = "TestPipeline", Type = ModelConstants.ResourceDefinitionReceivePipeline, ResourceContent = SerializeToString(document) };
                    asmContainer.ResourceDefinitions.Add(pipeline);

                    applicationOne = new BizTalkApplication() { Name = "ApplicationOne" };
                    applicationOne.Pipelines.Add(new Pipeline(asmContainer.Key, pipeline.Key)
                    {
                        Name = "Pipeline One",
                        Direction = PipelineDirection.Receive
                    });

                    group.Applications.Add(new ParsedBizTalkApplication
                    {
                        Application = applicationOne
                    });

                    applicationOne.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer());
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition());
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });

                });

            "And an application with no pipelines"
                 .x(() =>
                 {
                     applicationTwo = new BizTalkApplication() { Name = "ApplicationTwo" };

                     group.Applications.Add(
                        new ParsedBizTalkApplication
                        {
                            Application = applicationTwo
                        });
                 });

            "And a context"
                .x(() => context = new MigrationContext());

            "And a BizTalk Pipeline Parser"
               .x(() => parser = new BizTalkPipelineParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the error count should be 0"
                .x(() =>
                {
                    context.Errors.Should().BeNullOrEmpty();
                });

            "And the document on the pipeline in the first application should be equivalent to the pipeline XML"
                .x(() =>
                {
                    group.Applications.Should().HaveCount(2);
                    group.Applications[0].Application.Pipelines.Should().NotBeNullOrEmpty().And.HaveCount(1);
                    var pipeline = group.Applications[0].Application.Pipelines[0];
                    group.Applications[0].Application.Pipelines[0].Document.Should().NotBeNull();

                    // Validate the contents of the pipeline.
                    pipeline.Document.FriendlyName.Should().Be(document.FriendlyName);
                    pipeline.Document.Description.Should().Be(document.Description);
                    pipeline.Document.Stages.Should().NotBeNullOrEmpty();
                    pipeline.Document.Stages.Should().HaveCount(document.Stages.Length);
                    pipeline.Document.Stages[0].Components.Should().NotBeNull();
                    pipeline.Document.Stages[0].Components[0].Should().NotBeNull();
                    pipeline.Document.Stages[0].Components[0].Name.Should().Be(document.Stages[0].Components[0].Name);

                    
                });

            "And the pipeline resource should be created"
                .x(() =>
                {
                    var pipeline = group.Applications[0].Application.Pipelines[0];
                    var pipelineResourceDefinition = model.MigrationSource.ResourceContainers[0].ResourceContainers[0].ResourceContainers[0].ResourceDefinitions[0];
                    pipelineResourceDefinition.Should().NotBeNull();
                    pipelineResourceDefinition.Resources.Should().NotBeNullOrEmpty();
                    pipelineResourceDefinition.Resources.Should().HaveCount(1);

                    var pipelineResource = pipelineResourceDefinition.Resources[0];
                    pipelineResource.Name.Should().Be(pipeline.Name);
                    pipelineResource.Type.Should().Be(ModelConstants.ResourceReceivePipeline);
                    pipelineResource.Key.Should().NotBeNullOrEmpty();
                    pipelineResource.Key.Should().Be(pipeline.ResourceKey);
                    pipelineResource.Description.Should().Be(pipeline.Description);
                    pipelineResource.ResourceRelationships.Should().HaveCount(1);

                });

            "And the the pipeline collection in the second applcation should be empty"
                .x(() =>
                {
                    group.Applications.Should().HaveCount(2);
                    group.Applications[1].Application.Pipelines.Should().BeNullOrEmpty();
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
