#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#endregion

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse.Tests
{
    /// <summary>
    /// Tests for parsing the distribution lists.
    /// </summary>
    public class PipelineComponentParserFeature
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
                .x(() => e = Record.Exception(() => new PipelineComponentParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new PipelineComponentParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new PipelineComponentParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => parser = new PipelineComponentParser(model, context, logger)));

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
        public void ParseIsSkippedIfModelIsMissing(PipelineComponentParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
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
                .x(() => parser = new PipelineComponentParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests the happy path when there is a send pipeline and pipeline component.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulSingleSendPipelineComponent(PipelineComponentParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            var sendPiplineDefinitionName = "sendPiplineDefinitionName ";
            var sendPipelineDefinitionKey = "sendPipelineDefinitionKey";
            var sendPipelineDefinitionDescription = "sendPipelineDescription";
            var sendPipelineResourceKey = "sendPipelineResourceKey";
            var sendPipelineResourceName = "sendPipelineResourceName";
            var sendPipelineComponentName = "sendPipelineComponentName";
            var sendPipelineComponentDescription = "sendPipelineComponentDescription";

            "Given a source model with a send pipeline and component"
               .x(() =>
               {
                   var pipelineComponentDocument = new Document
                   {
                       Stages = new DocumentStage[]
                       {
                            new DocumentStage
                            {
                                Components = new DocumentStageComponent[1]
                                {
                                    new DocumentStageComponent { ComponentName = sendPipelineComponentName, Description = sendPipelineComponentDescription }
                                }
                            }
                       }
                   };

                   var sendPipeline = new Pipeline
                   {
                       Name = sendPiplineDefinitionName,
                       ResourceDefinitionKey = sendPipelineDefinitionKey,
                       Direction = PipelineDirection.Send,
                       Description = sendPipelineDefinitionDescription,
                       Document = pipelineComponentDocument,
                       ResourceKey = sendPipelineResourceKey
                   };

                   var parsedApplication = new ParsedBizTalkApplication
                   {
                       Application = new BizTalkApplication()
                   };

                   parsedApplication.Application.Pipelines.Add(sendPipeline);

                   var aisModel = new AzureIntegrationServicesModel();
                   var group = new ParsedBizTalkApplicationGroup();
                   aisModel.MigrationSource.MigrationSourceModel = group;
                   group.Applications.Add(parsedApplication);

                   model = aisModel;
               });

            "And one send pipeline resource in the source model"
                .x(() =>
                {
                    var msiReportContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerMsi,
                        Key = "msiContainerKey",
                        Name = "msiContainerName"
                    };

                    var cabReportContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerCab,
                        Key = "cabContainerKey",
                        Name = "cabContainerName"
                    };

                    var assemblyReportContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerAssembly,
                        Key = "assemblyContainerKey",
                        Name = "assemblyContainerName"
                    };

                    var sendPipelineReportDefinition = new ResourceDefinition
                    {
                        Type = ModelConstants.ResourceDefinitionOrchestration,
                        Key = sendPipelineDefinitionKey,
                        Name = sendPiplineDefinitionName
                    };

                    var sendPipelineResource = new ResourceItem
                    {
                        Type = ModelConstants.ResourceSendPipeline,
                        Name = sendPipelineResourceName,
                        Key = sendPipelineResourceKey
                    };

                    sendPipelineReportDefinition.Resources.Add(sendPipelineResource);
                    assemblyReportContainer.ResourceDefinitions.Add(sendPipelineReportDefinition);
                    cabReportContainer.ResourceContainers.Add(assemblyReportContainer);
                    msiReportContainer.ResourceContainers.Add(cabReportContainer);

                    model.MigrationSource.ResourceContainers.Add(msiReportContainer);
                 });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new PipelineComponentParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the source model should have a pipeline component under the send pipeline resource in the source report"
                .x(() =>
                {
                    model.Should().NotBeNull();
                    model.MigrationSource.Should().NotBeNull();
                    model.MigrationSource.ResourceContainers[0].Should().NotBeNull();
                    model.MigrationSource.ResourceContainers[0].ResourceContainers[0].Should().NotBeNull();
                    model.MigrationSource.ResourceContainers[0].ResourceContainers[0].ResourceContainers[0].Should().NotBeNull();
                    model.MigrationSource.ResourceContainers[0].ResourceContainers[0].ResourceContainers[0].ResourceDefinitions.Should().NotBeNull();
                    model.MigrationSource.ResourceContainers[0].ResourceContainers[0].ResourceContainers[0].ResourceDefinitions.Should().HaveCount(1);
                    var sendPipeline = ((ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel).Applications[0].Application.Pipelines[0];

                    var sendPipelineDefinition = model.MigrationSource.ResourceContainers[0].ResourceContainers[0].ResourceContainers[0].ResourceDefinitions[0];
                    sendPipelineDefinition.Resources.Should().NotBeNull();
                    sendPipelineDefinition.Resources.Should().HaveCount(1);

                    // Validate the send pipeline.
                    var sendPipelineResource = sendPipelineDefinition.Resources[0];
                    sendPipelineResource.Resources.Should().NotBeNull();
                    sendPipelineResource.Resources.Should().HaveCount(1);

                    // Validate the pipeline components.
                    var pipelineComponentResource = sendPipelineResource.Resources[0];
                    pipelineComponentResource.Type.Should().Be(ModelConstants.ResourcePipelineComponent);
                    pipelineComponentResource.Name.Should().Be(sendPipelineComponentName);
                    pipelineComponentResource.Description.Should().Be(sendPipelineComponentDescription);
                    pipelineComponentResource.Key.Should().Be(string.Concat(sendPipelineResource.Key, ":", pipelineComponentResource.Name));

                    sendPipeline.Document.Stages[0].Components[0].Resource.Should().Be(pipelineComponentResource); // The pointer to the resource should be set.
                    pipelineComponentResource.ParentRefId.Should().Be(sendPipelineResource.RefId); // The parent ref ID should be set.
                    var stageComponents = Document.FindStageComponents(sendPipeline.Document);
                    pipelineComponentResource.SourceObject.Should().Be(stageComponents.First()); // The resource should have a pointer to the source object. 
                });
        }

        /// <summary>
        /// Scenario tests the happy path when there is a receive pipeline and pipeline component.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulSingleReceivePipelineComponent(PipelineComponentParser parser, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var receivePiplineDefinitionName = "receivePiplineDefinitionName ";
            var receivePipelineDefinitionKey = "receivePipelineDefinitionKey";
            var receivePipelineDefinitionDescription = "receivePipelineDescription";
            var receivePipelineResourceKey = "receivePipelineResourceKey";
            var receivePipelineResourceName = "receivePipelineResourceName";
            var receivePipelineComponentName = "receivePipelineComponentName";
            var receivePipelineComponentDescription = "receivePipelineComponentDescription";

            "Given a source model with a receive pipeline and component"
                .x(() =>
                {
                    var pipelineComponentDocument = new Document
                    {
                        Stages = new DocumentStage[]
                        {
                            new DocumentStage
                            {
                                Components = new DocumentStageComponent[1]
                                {
                                    new DocumentStageComponent { ComponentName = receivePipelineComponentName, Description = receivePipelineComponentDescription }
                                }
                            }
                        }
                    };

                    var receivePipeline = new Pipeline
                    {
                        Name = receivePiplineDefinitionName,
                        ResourceDefinitionKey = receivePipelineDefinitionKey,
                        Direction = PipelineDirection.Receive,
                        Description = receivePipelineDefinitionDescription,
                        Document = pipelineComponentDocument,
                        ResourceKey = receivePipelineResourceKey
                    };

                    var parsedApplication = new ParsedBizTalkApplication
                    {
                        Application = new BizTalkApplication()
                    };

                    parsedApplication.Application.Pipelines.Add(receivePipeline);

                    var aisModel = new AzureIntegrationServicesModel();
                    var group = new ParsedBizTalkApplicationGroup();
                    aisModel.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(parsedApplication);


                    model = aisModel;
                });

            "And one receive pipeline resource in the source model"
                .x(() =>
                {
                    var msiReportContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerMsi,
                        Key = "msiContainerKey",
                        Name = "msiContainerName"
                    };

                    var cabReportContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerCab,
                        Key = "cabContainerKey",
                        Name = "cabContainerName"
                    };

                    var assemblyReportContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerAssembly,
                        Key = "assemblyContainerKey",
                        Name = "assemblyContainerName"
                    };

                    var receivePipelineReportDefinition = new ResourceDefinition
                    {
                        Type = ModelConstants.ResourceDefinitionOrchestration,
                        Key = receivePipelineDefinitionKey,
                        Name = receivePiplineDefinitionName
                    };

                    var receivePipelineResource = new ResourceItem
                    {
                        Type = ModelConstants.ResourceReceivePipeline,
                        Name = receivePipelineResourceName,
                        Key = receivePipelineResourceKey
                    };

                    receivePipelineReportDefinition.Resources.Add(receivePipelineResource);
                    assemblyReportContainer.ResourceDefinitions.Add(receivePipelineReportDefinition);
                    cabReportContainer.ResourceContainers.Add(assemblyReportContainer);
                    msiReportContainer.ResourceContainers.Add(cabReportContainer);

                    model.MigrationSource.ResourceContainers.Add(msiReportContainer);
                });

            "And a logger"
                 .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new PipelineComponentParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the source should have a pipeline component under the receive pipeline resource"
                .x(() =>
                {
                    model.MigrationSource.Should().NotBeNull();
                    model.MigrationSource.Should().NotBeNull();
                    model.MigrationSource.ResourceContainers[0].Should().NotBeNull();
                    model.MigrationSource.ResourceContainers[0].ResourceContainers[0].Should().NotBeNull();
                    model.MigrationSource.ResourceContainers[0].ResourceContainers[0].ResourceContainers[0].Should().NotBeNull();
                    model.MigrationSource.ResourceContainers[0].ResourceContainers[0].ResourceContainers[0].ResourceDefinitions.Should().NotBeNull();
                    model.MigrationSource.ResourceContainers[0].ResourceContainers[0].ResourceContainers[0].ResourceDefinitions.Should().HaveCount(1);

                    var receivePipelineDefinition = model.MigrationSource.ResourceContainers[0].ResourceContainers[0].ResourceContainers[0].ResourceDefinitions[0];
                    receivePipelineDefinition.Resources.Should().NotBeNull();
                    receivePipelineDefinition.Resources.Should().HaveCount(1);

                    var receivePipelineResource = receivePipelineDefinition.Resources[0];
                    receivePipelineResource.Resources.Should().NotBeNull();
                    receivePipelineResource.Resources.Should().HaveCount(1);

                    var pipelineComponentResource = receivePipelineResource.Resources[0];
                    pipelineComponentResource.Type.Should().Be(ModelConstants.ResourcePipelineComponent);
                    pipelineComponentResource.Name.Should().Be(receivePipelineComponentName);
                    pipelineComponentResource.Description.Should().Be(receivePipelineComponentDescription);
                    pipelineComponentResource.Key.Should().Be(string.Concat(receivePipelineResource.Key, ":", pipelineComponentResource.Name));
                });
        }

        /// <summary>
        /// Scenario tests an error is logged when the pipeline resource is missing.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailsWhenPipelineResourceIsMissing(PipelineComponentParser parser, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var receivePiplineDefinitionName = "receivePiplineDefinitionName ";
            var receivePipelineDefinitionKey = "receivePipelineDefinitionKey";
            var receivePipelineDefinitionDescription = "receivePipelineDescription";
            var receivePipelineResourceKey = "receivePipelineResourceKey";
            var receivePipelineComponentName = "receivePipelineComponentName";
            var receivePipelineComponentDescription = "receivePipelineComponentDescription";

            "Given a source model with a receive pipeline and component"
                .x(() =>
                {
                    var pipelineComponentDocument = new Document
                    {
                        Stages = new DocumentStage[]
                        {
                            new DocumentStage
                            {
                                Components = new DocumentStageComponent[]
                                {
                                    new DocumentStageComponent { ComponentName = receivePipelineComponentName, Description = receivePipelineComponentDescription }
                                }
                            }
                        }
                    };

                    var receivePipeline = new Pipeline
                    {
                        Name = receivePiplineDefinitionName,
                        ResourceDefinitionKey = receivePipelineDefinitionKey,
                        Direction = PipelineDirection.Receive,
                        Description = receivePipelineDefinitionDescription,
                        Document = pipelineComponentDocument,
                        ResourceKey = receivePipelineResourceKey
                    };

                    var parsedApplication = new ParsedBizTalkApplication
                    {
                        Application = new BizTalkApplication()
                    };

                    parsedApplication.Application.Pipelines.Add(receivePipeline);

                    var aisModel = new AzureIntegrationServicesModel();
                    var group = new ParsedBizTalkApplicationGroup();
                    aisModel.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(parsedApplication);


                    model = aisModel;
                });

            "And no receive pipeline resource in the source model"
                .x(() =>
                {
                    var msiReportContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerMsi,
                        Key = "msiContainerKey",
                        Name = "msiContainerName"
                    };

                    var cabReportContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerCab,
                        Key = "cabContainerKey",
                        Name = "cabContainerName"
                    };

                    var assemblyReportContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerAssembly,
                        Key = "assemblyContainerKey",
                        Name = "assemblyContainerName"
                    };

                    var receivePipelineReportDefinition = new ResourceDefinition
                    {
                        Type = ModelConstants.ResourceDefinitionOrchestration,
                        Key = receivePipelineDefinitionKey,
                        Name = receivePiplineDefinitionName
                    };

                    assemblyReportContainer.ResourceDefinitions.Add(receivePipelineReportDefinition);
                    cabReportContainer.ResourceContainers.Add(assemblyReportContainer);
                    msiReportContainer.ResourceContainers.Add(cabReportContainer);

                    model.MigrationSource.ResourceContainers.Add(msiReportContainer);
                });

            "And a logger"
                 .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new PipelineComponentParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be one context error"
                .x(() =>
                {
                    context.Errors.Should().NotBeNullOrEmpty();
                    context.Errors.Should().HaveCount(1);
                    context.Errors[0].Message.Should().Contain(receivePipelineResourceKey);
                    context.Errors[0].Message.Should().Contain(ModelConstants.ResourceReceivePipeline);
                });
        }

        #endregion

        /// <summary>
        /// Creates a group with the fields for send port parsing.
        /// </summary>
        /// <returns>A populated <see cref="ParsedBizTalkApplicationGroup"/>.</returns>
        private static ParsedBizTalkApplicationGroup CreateGroup()
        {
            var group = new ParsedBizTalkApplicationGroup();
            var application = new ParsedBizTalkApplication();
            var bindingInfo = new BindingInfo();
            var distributionLists = new List<DistributionList>
            {
                new DistributionList() { Name = "Send Port Group 1", Filter = "" }
            };
            bindingInfo.DistributionListCollection = distributionLists.ToArray();
            application.Application.Bindings = new BindingFile("TestContainer", "BindingInfo")
            {
                BindingInfo = bindingInfo
            };
            group.Applications.Add(application);
            return group;
        }
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
