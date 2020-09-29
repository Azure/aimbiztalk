// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.DependencyRules;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Tests.DependencyRules
{
    /// <summary>
    /// Tests for the <see cref="DP003OrchestrationDependencyAnalyzer"/> class.
    /// </summary>
    public class DP003OrchestrationDependencyAnalyzerFeature
    {
        /// <summary>
        /// Defines a mocked logger.
        /// </summary>
        private Mock<ILogger> _mockLogger;

        #region Before Each Scenario

        /// <summary>
        /// Sets up state before each scenario.
        /// </summary>
        [Background]
        public void Setup()
        {
            "Given a new mock logger"
                .x(() => _mockLogger = new Mock<ILogger>());
        }

        #endregion

        #region Constructor Scenarios

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null model is passed.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullModel(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And null model"
                .x(() => model.Should().BeNull());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null model"
                .x(() => e = Record.Exception(() => new DP003OrchestrationDependencyAnalyzer(model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null context is passed.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullContext(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And null context"
                .x(() => context.Should().BeNull());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null context"
                .x(() => e = Record.Exception(() => new DP003OrchestrationDependencyAnalyzer(model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("context"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null logger is passed.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullLogger(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And null logger"
                .x(() => logger.Should().BeNull());

            "When constructing with a null logger"
                .x(() => e = Record.Exception(() => new DP003OrchestrationDependencyAnalyzer(model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction works when params ok.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithSuccess(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing"
                .x(() => e = Record.Exception(() => new DP003OrchestrationDependencyAnalyzer(model, context, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region Source model structure failure tests

        /// <summary>
        /// Scenario tests when the source model is empty.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003EmptySourceModel(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given a source model with an orchestration"
              .x(() =>
              {
                  model = new AzureIntegrationServicesModel();
                  model.MigrationSource.MigrationSourceModel = null;
              });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be one context error"
                .x(() =>
                {
                    _mockLogger.Invocations.Count.Should().Be(2); // Should just be the skip message and the run message
                    _mockLogger.Invocations[0].Arguments[2].ToString().Should().Contain("Running BizTalk analyzer");
                    _mockLogger.Invocations[1].Arguments[2].ToString().Should().Contain("Skipping the rule");

                });
        }

        #endregion

        #region Message declaration dependency tests

        /// <summary>
        /// Scenario tests resolving message declaration dependencies.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003ResolveMessageDeclarationDependenciesWithSuccess(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var schemaFullName = "Test.Schemas.DocumentSchema1";
            var schemaResourceKey = "schemaResourceKey";
            var messageDeclarationName = "messageDeclarationName";

            "Given a source model with an orchestration, a service declaration, message declaration and schema"
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
                                            new ElementProperty { Name = "Name", Value = "serviceDeclarationName" }
                                        },
                                        Element1 = new Element[]
                                        {
                                            new Element
                                            {
                                                Type = "MessageDeclaration",
                                                Property = new ElementProperty[]
                                                {
                                                    new ElementProperty{ Name = "Name", Value = messageDeclarationName },
                                                    new ElementProperty{ Name = "Type", Value = schemaFullName }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                        }
                  };

                  var documentSchema = new Schema()
                  {
                      Name = "DocumentSchema1",
                      Namespace = "Test.Schemas",
                      FullName = schemaFullName,
                      XmlNamespace = "http://schemas.test.com/DocumentSchema1",
                      RootNodeName = "Root",
                      ResourceDefinitionKey = "document-schema-1",
                      ResourceKey = schemaResourceKey,
                      SchemaType = BizTalkSchemaType.Document
                  };
                  documentSchema.MessageDefinitions.Add(new MessageDefinition(documentSchema.RootNodeName, documentSchema.XmlNamespace, documentSchema.FullName, documentSchema.Name, documentSchema.ResourceKey));

                  var orchestration = new Orchestration
                  {
                      Name = "orchestrationDefinitionName",
                      ResourceDefinitionKey = "orchestrationDefinitionKey",
                      Model = odxModel
                  };

                  var parsedApplication = new ParsedBizTalkApplication
                  {
                      Application = new BizTalkApplication()
                  };

                  parsedApplication.Application.Orchestrations.Add(orchestration);
                  parsedApplication.Application.Schemas.Add(documentSchema);

                  var aisModel = new AzureIntegrationServicesModel();
                  var group = new ParsedBizTalkApplicationGroup();
                  aisModel.MigrationSource.MigrationSourceModel = group;
                  group.Applications.Add(parsedApplication);

                  model = aisModel;

                  var msiResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerMsi,
                      Key = "msiContainerKey",
                      Name = "msiContainerName"
                  };

                  var cabResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerCab,
                      Key = "cabContainerKey",
                      Name = "cabContainerName"
                  };

                  var assemblyResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerAssembly,
                      Key = "assemblyContainerKey",
                      Name = "assemblyContainerName"
                  };

                  var orchestrationResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionOrchestration,
                      Key = "orchestrationDefinitionKey",
                      Name = "orchestrationDefinitionName"
                  };

                  var serviceDeclarationResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceServiceDeclaration,
                      Key = "serviceDeclarationKey",
                      Name = "serviceDeclarationName",
                      SourceObject = orchestration.FindServiceDeclaration()
                  };

                  var messageDeclarationResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceMessageDeclaration,
                      Key = "messageDeclarationKey",
                      Name = messageDeclarationName,
                      SourceObject = orchestration.FindMessageDeclarations().First()
                  };

                  var schemaResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionSchema,
                      Key = "schemaDefinitionKey",
                      Name = "schemaDefinitionName"
                  };

                  var schemaResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceDocumentSchema,
                      Key = schemaResourceKey,
                      Name = messageDeclarationName,
                      SourceObject = documentSchema,
                      ParentRefId = schemaResourceDefinition.RefId
                  };

                  var messageTypeResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceMessageType,
                      Key = schemaResourceKey,
                      Name = messageDeclarationName,
                      SourceObject = documentSchema.MessageDefinitions[0],
                      ParentRefId = schemaResource.RefId
                  };

                  schemaResource.Resources.Add(messageTypeResource);
                  schemaResourceDefinition.Resources.Add(schemaResource);
                  serviceDeclarationResource.Resources.Add(messageDeclarationResource);
                  orchestrationResourceDefinition.Resources.Add(serviceDeclarationResource);
                  assemblyResourceContainer.ResourceDefinitions.Add(orchestrationResourceDefinition);
                  assemblyResourceContainer.ResourceDefinitions.Add(schemaResourceDefinition);
                  cabResourceContainer.ResourceContainers.Add(assemblyResourceContainer);
                  msiResourceContainer.ResourceContainers.Add(cabResourceContainer);
                  aisModel.MigrationSource.ResourceContainers.Add(msiResourceContainer);
              });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resource node should have the expected relationships created"
                .x(() =>
                {
                    // Get the resources from the model.
                    var resources = model.FindAllResources();

                    // Test the message declaration resource.
                    var messageDeclaration = resources.Where(r => r.Type == ModelConstants.ResourceMessageDeclaration).FirstOrDefault();
                    messageDeclaration.Should().NotBeNull();
                    messageDeclaration.Name.Should().Be(messageDeclarationName);
                    messageDeclaration.ReportMessages.Should().HaveCount(0);
                    messageDeclaration.ResourceRelationships.Should().HaveCount(2);

                    // Test the relationships
                    var schema = resources.Where(r => r.Type == ModelConstants.ResourceDocumentSchema).FirstOrDefault();
                    schema.Should().NotBeNull();
                    schema.ResourceRelationships.Should().HaveCount(1);

                    messageDeclaration.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencesTo);
                    messageDeclaration.ResourceRelationships[0].ResourceRefId.Should().Be(schema.Resources[0].RefId);

                    messageDeclaration.ResourceRelationships[1].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencesTo);
                    messageDeclaration.ResourceRelationships[1].ResourceRefId.Should().Be(schema.RefId);

                    schema.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencedBy);
                    schema.ResourceRelationships[0].ResourceRefId.Should().Be(messageDeclaration.RefId);

                    schema.Resources[0].ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencedBy);
                    schema.Resources[0].ResourceRelationships[0].ResourceRefId.Should().Be(messageDeclaration.RefId);
                });
        }

        /// <summary>
        /// Scenario tests resolving message declaration dependencies with a message type of System.String, results in no dependencies being set.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003ResolveMessageDeclarationDependenciesWithInbuiltType(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var messageDeclarationName = "messageDeclarationName";

            "Given a source model with an orchestration which has a message of type System.String"
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
                                            new ElementProperty { Name = "Name", Value = "serviceDeclarationName" }
                                        },
                                        Element1 = new Element[]
                                        {
                                            new Element
                                            {
                                                Type = "MessageDeclaration",
                                                Property = new ElementProperty[]
                                                {
                                                    new ElementProperty{ Name = "Name", Value = messageDeclarationName },
                                                    new ElementProperty{ Name = "Type", Value = "System.String" }
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
                      Name = "orchestrationDefinitionName",
                      ResourceDefinitionKey = "orchestrationDefinitionKey",
                      Model = odxModel
                  };

                  var parsedApplication = new ParsedBizTalkApplication
                  {
                      Application = new BizTalkApplication()
                  };

                  parsedApplication.Application.Orchestrations.Add(orchestration);

                  var aisModel = new AzureIntegrationServicesModel();
                  var group = new ParsedBizTalkApplicationGroup();
                  aisModel.MigrationSource.MigrationSourceModel = group;
                  group.Applications.Add(parsedApplication);

                  model = aisModel;

                    var msiResourceContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerMsi,
                        Key = "msiContainerKey",
                        Name = "msiContainerName"
                    };

                    var cabResourceContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerCab,
                        Key = "cabContainerKey",
                        Name = "cabContainerName"
                    };

                    var assemblyResourceContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerAssembly,
                        Key = "assemblyContainerKey",
                        Name = "assemblyContainerName"
                    };

                    var orchestrationResourceDefinition = new ResourceDefinition
                    {
                        Type = ModelConstants.ResourceDefinitionOrchestration,
                        Key = "orchestrationDefinitionKey",
                        Name = "orchestrationDefinitionName"
                    };

                    var serviceDeclarationResource = new ResourceItem
                    {
                        Type = ModelConstants.ResourceServiceDeclaration,
                        Key = "serviceDeclarationKey",
                        Name = "serviceDeclarationName",
                        SourceObject = orchestration.FindServiceDeclaration()
                    };

                    var messageDeclarationResource = new ResourceItem
                    {
                        Type = ModelConstants.ResourceMessageDeclaration,
                        Key = "messageDeclarationKey",
                        Name = messageDeclarationName,
                        SourceObject = orchestration.FindMessageDeclarations().First()
                    };

                    serviceDeclarationResource.Resources.Add(messageDeclarationResource);
                    orchestrationResourceDefinition.Resources.Add(serviceDeclarationResource);
                    assemblyResourceContainer.ResourceDefinitions.Add(orchestrationResourceDefinition);
                    cabResourceContainer.ResourceContainers.Add(assemblyResourceContainer);
                    msiResourceContainer.ResourceContainers.Add(cabResourceContainer);
                    aisModel.MigrationSource.ResourceContainers.Add(msiResourceContainer);
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resource node should have no relationships created"
                .x(() =>
                {
                    // Get the resources from the model.
                    var resources = model.FindAllResources();

                    // Test the message declaration resource.
                    var messageDeclaration = resources.Where(r => r.Type == ModelConstants.ResourceMessageDeclaration).FirstOrDefault();
                    messageDeclaration.Should().NotBeNull();
                    messageDeclaration.Name.Should().Be(messageDeclarationName);
                    messageDeclaration.ReportMessages.Should().HaveCount(1);
                    messageDeclaration.ResourceRelationships.Should().BeNullOrEmpty();
                });
        }

        /// <summary>
        /// Scenario tests resolving message declaration dependencies.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003ResolveMessageDeclarationDependenciesWithMissingSchemaResource(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var schemaFullName = "Test.Schemas.DocumentSchema1";
            var schemaResourceKey = "schemaResourceKey";
            var messageDeclarationName = "messageDeclarationName";

            "Given a source model with an orchestration, a service declaration, a message declaration and schema"
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
                                            new ElementProperty { Name = "Name", Value = "serviceDeclarationName" }
                                        },
                                        Element1 = new Element[]
                                        {
                                            new Element
                                            {
                                                Type = "MessageDeclaration",
                                                Property = new ElementProperty[]
                                                {
                                                    new ElementProperty{ Name = "Name", Value = messageDeclarationName },
                                                    new ElementProperty{ Name = "Type", Value = schemaFullName }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                        }
                  };

                  var documentSchema = new Schema()
                  {
                      Name = "DocumentSchema1",
                      Namespace = "Test.Schemas",
                      FullName = schemaFullName,
                      XmlNamespace = "http://schemas.test.com/DocumentSchema1",
                      RootNodeName = "Root",
                      ResourceDefinitionKey = "document-schema-1",
                      ResourceKey = schemaResourceKey,
                      SchemaType = BizTalkSchemaType.Document
                  };
                  documentSchema.MessageDefinitions.Add(new MessageDefinition(documentSchema.RootNodeName, documentSchema.XmlNamespace, documentSchema.FullName, documentSchema.Name, documentSchema.ResourceKey));

                  var orchestration = new Orchestration
                  {
                      Name = "orchestrationDefinitionName",
                      ResourceDefinitionKey = "orchestrationDefinitionKey",
                      Model = odxModel
                  };

                  var parsedApplication = new ParsedBizTalkApplication
                  {
                      Application = new BizTalkApplication()
                  };

                  parsedApplication.Application.Orchestrations.Add(orchestration);
                  parsedApplication.Application.Schemas.Add(documentSchema);

                  var aisModel = new AzureIntegrationServicesModel();
                  var group = new ParsedBizTalkApplicationGroup();
                  aisModel.MigrationSource.MigrationSourceModel = group;
                  group.Applications.Add(parsedApplication);

                  model = aisModel;

                    var msiResourceContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerMsi,
                        Key = "msiContainerKey",
                        Name = "msiContainerName"
                    };

                    var cabResourceContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerCab,
                        Key = "cabContainerKey",
                        Name = "cabContainerName"
                    };

                    var assemblyResourceContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerAssembly,
                        Key = "assemblyContainerKey",
                        Name = "assemblyContainerName"
                    };

                    var orchestrationResourceDefinition = new ResourceDefinition
                    {
                        Type = ModelConstants.ResourceDefinitionOrchestration,
                        Key = "orchestrationDefinitionKey",
                        Name = "orchestrationDefinitionName"
                    };

                    var serviceDeclarationResource = new ResourceItem
                    {
                        Type = ModelConstants.ResourceServiceDeclaration,
                        Key = "serviceDeclarationKey",
                        Name = "serviceDeclarationName",
                        SourceObject = orchestration.FindServiceDeclaration()
                    };

                    var messageDeclarationResource = new ResourceItem
                    {
                        Type = ModelConstants.ResourceMessageDeclaration,
                        Key = "messageDeclarationKey",
                        Name = messageDeclarationName,
                        SourceObject = orchestration.FindMessageDeclarations().First()
                    };

                    serviceDeclarationResource.Resources.Add(messageDeclarationResource);
                    orchestrationResourceDefinition.Resources.Add(serviceDeclarationResource);
                    assemblyResourceContainer.ResourceDefinitions.Add(orchestrationResourceDefinition);
                    cabResourceContainer.ResourceContainers.Add(assemblyResourceContainer);
                    msiResourceContainer.ResourceContainers.Add(cabResourceContainer);
                    aisModel.MigrationSource.ResourceContainers.Add(msiResourceContainer);
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be one context error"
                .x(() =>
                {
                    // Get the resources from the model.
                    var resources = model.FindAllResources();

                    // Test the message declaration resource.
                    var messageDeclaration = resources.Where(r => r.Type == ModelConstants.ResourceMessageDeclaration).FirstOrDefault();
                    messageDeclaration.Should().NotBeNull();
                    messageDeclaration.Name.Should().Be(messageDeclarationName);
                    messageDeclaration.ReportMessages.Should().HaveCount(1);
                    messageDeclaration.ReportMessages[0].Severity.Should().Be(MessageSeverity.Warning);
                    messageDeclaration.ReportMessages[0].Message.Should().Contain(schemaFullName);
                    messageDeclaration.ResourceRelationships.Should().HaveCount(0);
                });
        }

        /// <summary>
        /// Scenario tests resolving message declaration dependencies when multiple schemas match (e.g. when 2 apps use the same schema).
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003ResolveMessageDeclarationDependenciesMultipleSchemaMatches(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var schemaFullName = "Test.Schemas.DocumentSchema1";
            var schemaResourceKey = "schemaResourceKey";
            var messageDeclarationName = "messageDeclarationName";

            "Given a source model with an orchestration, a service declaration, message declaration and schema"
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
                                            new ElementProperty { Name = "Name", Value = "serviceDeclarationName" }
                                        },
                                        Element1 = new Element[]
                                        {
                                            new Element
                                            {
                                                Type = "MessageDeclaration",
                                                Property = new ElementProperty[]
                                                {
                                                    new ElementProperty{ Name = "Name", Value = messageDeclarationName },
                                                    new ElementProperty{ Name = "Type", Value = schemaFullName }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                        }
                  };

                  var documentSchema = new Schema()
                  {
                      Name = "DocumentSchema1",
                      Namespace = "Test.Schemas",
                      FullName = schemaFullName,
                      XmlNamespace = "http://schemas.test.com/DocumentSchema1",
                      RootNodeName = "Root",
                      ResourceDefinitionKey = "document-schema-1",
                      ResourceKey = schemaResourceKey,
                      SchemaType = BizTalkSchemaType.Document
                  };
                  documentSchema.MessageDefinitions.Add(new MessageDefinition(documentSchema.RootNodeName, documentSchema.XmlNamespace, documentSchema.FullName, documentSchema.Name, documentSchema.ResourceKey));

                  var documentSchema2 = new Schema()
                  {
                      Name = "DocumentSchema2",
                      Namespace = "Test.Schemas",
                      FullName = schemaFullName,
                      XmlNamespace = "http://schemas.test.com/DocumentSchema1",
                      RootNodeName = "Root",
                      ResourceDefinitionKey = "document-schema-2",
                      ResourceKey = string.Concat(schemaResourceKey, "2"),
                      SchemaType = BizTalkSchemaType.Document
                  };
                  documentSchema2.MessageDefinitions.Add(new MessageDefinition(documentSchema2.RootNodeName, documentSchema2.XmlNamespace, documentSchema2.FullName, documentSchema2.Name, documentSchema2.ResourceKey));

                  var orchestration = new Orchestration
                  {
                      Name = "orchestrationDefinitionName",
                      ResourceDefinitionKey = "orchestrationDefinitionKey",
                      Model = odxModel
                  };

                  var parsedApplication = new ParsedBizTalkApplication
                  {
                      Application = new BizTalkApplication()
                  };

                  parsedApplication.Application.Orchestrations.Add(orchestration);
                  parsedApplication.Application.Schemas.Add(documentSchema);

                  var aisModel = new AzureIntegrationServicesModel();
                  var group = new ParsedBizTalkApplicationGroup();
                  aisModel.MigrationSource.MigrationSourceModel = group;
                  group.Applications.Add(parsedApplication);

                  model = aisModel;

                  var msiResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerMsi,
                      Key = "msiContainerKey",
                      Name = "msiContainerName"
                  };

                  var cabResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerCab,
                      Key = "cabContainerKey",
                      Name = "cabContainerName"
                  };

                  var assemblyResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerAssembly,
                      Key = "assemblyContainerKey",
                      Name = "assemblyContainerName"
                  };

                  var orchestrationResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionOrchestration,
                      Key = "orchestrationDefinitionKey",
                      Name = "orchestrationDefinitionName"
                  };

                  var serviceDeclarationResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceServiceDeclaration,
                      Key = "serviceDeclarationKey",
                      Name = "serviceDeclarationName",
                      SourceObject = orchestration.FindServiceDeclaration()
                  };

                  var messageDeclarationResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceMessageDeclaration,
                      Key = "messageDeclarationKey",
                      Name = messageDeclarationName,
                      SourceObject = orchestration.FindMessageDeclarations().First()
                  };

                  var schemaResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionSchema,
                      Key = "schemaDefinitionKey",
                      Name = "schemaDefinitionName"
                  };

                  var schemaResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceDocumentSchema,
                      Key = schemaResourceKey,
                      Name = messageDeclarationName,
                      SourceObject = documentSchema,
                      ParentRefId = schemaResourceDefinition.RefId
                  };

                  var messageTypeResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceMessageType,
                      Key = schemaResourceKey,
                      Name = messageDeclarationName,
                      SourceObject = documentSchema.MessageDefinitions[0],
                      ParentRefId = schemaResource.RefId
                  };

                  var schemaResource2 = new ResourceItem
                  {
                      Type = ModelConstants.ResourceDocumentSchema,
                      Key = documentSchema2.ResourceKey,
                      Name = messageDeclarationName,
                      SourceObject = documentSchema2,
                      ParentRefId = schemaResourceDefinition.RefId
                  };

                  var messageTypeResource2 = new ResourceItem
                  {
                      Type = ModelConstants.ResourceMessageType,
                      Key = documentSchema2.ResourceKey,
                      Name = messageDeclarationName,
                      SourceObject = documentSchema2.MessageDefinitions[0],
                      ParentRefId = schemaResource2.RefId
                  };

                  schemaResource.Resources.Add(messageTypeResource);
                  schemaResourceDefinition.Resources.Add(schemaResource);
                  schemaResource2.Resources.Add(messageTypeResource2);
                  schemaResourceDefinition.Resources.Add(schemaResource2); // Adds the duplicate schema.
                  serviceDeclarationResource.Resources.Add(messageDeclarationResource);
                  orchestrationResourceDefinition.Resources.Add(serviceDeclarationResource);
                  assemblyResourceContainer.ResourceDefinitions.Add(orchestrationResourceDefinition);
                  assemblyResourceContainer.ResourceDefinitions.Add(schemaResourceDefinition);
                  cabResourceContainer.ResourceContainers.Add(assemblyResourceContainer);
                  msiResourceContainer.ResourceContainers.Add(cabResourceContainer);
                  aisModel.MigrationSource.ResourceContainers.Add(msiResourceContainer);
              });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resource node should have the expected relationships created"
                .x(() =>
                {
                    // Get the resources from the model.
                    var resources = model.FindAllResources();

                    // Test the message declaration resource.
                    var messageDeclaration = resources.Where(r => r.Type == ModelConstants.ResourceMessageDeclaration).FirstOrDefault();
                    messageDeclaration.Should().NotBeNull();
                    messageDeclaration.Name.Should().Be(messageDeclarationName);
                    messageDeclaration.ReportMessages.Should().HaveCount(1);
                    messageDeclaration.ReportMessages[0].Message.Should().Contain("Dependency cannot be accurately resolved");
                    messageDeclaration.ResourceRelationships.Should().HaveCount(0);

                    // Test the relationships
                    var schema = resources.Where(r => r.Type == ModelConstants.ResourceDocumentSchema).FirstOrDefault();
                    schema.Should().NotBeNull();
                    schema.ResourceRelationships.Should().HaveCount(0);
                });
        }

        /// <summary>
        /// Scenario tests resolving message declaration dependencies when the source object is missing.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003ResolveMessageDeclarationDependenciesMissingSourceObject(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var schemaFullName = "Test.Schemas.DocumentSchema1";
            var schemaResourceKey = "schemaResourceKey";
            var messageDeclarationName = "messageDeclarationName";

            "Given a source model with an orchestration, a service declaration, message declaration and schema"
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
                                            new ElementProperty { Name = "Name", Value = "serviceDeclarationName" }
                                        },
                                        Element1 = new Element[]
                                        {
                                            new Element
                                            {
                                                Type = "MessageDeclaration",
                                                Property = new ElementProperty[]
                                                {
                                                    new ElementProperty{ Name = "Name", Value = messageDeclarationName },
                                                    new ElementProperty{ Name = "Type", Value = schemaFullName }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                        }
                  };

                  var documentSchema = new Schema()
                  {
                      Name = "DocumentSchema1",
                      Namespace = "Test.Schemas",
                      FullName = schemaFullName,
                      XmlNamespace = "http://schemas.test.com/DocumentSchema1",
                      RootNodeName = "Root",
                      ResourceDefinitionKey = "document-schema-1",
                      ResourceKey = schemaResourceKey,
                      SchemaType = BizTalkSchemaType.Document
                  };
                  documentSchema.MessageDefinitions.Add(new MessageDefinition(documentSchema.RootNodeName, documentSchema.XmlNamespace, documentSchema.FullName, documentSchema.Name, documentSchema.ResourceKey));

                  var orchestration = new Orchestration
                  {
                      Name = "orchestrationDefinitionName",
                      ResourceDefinitionKey = "orchestrationDefinitionKey",
                      Model = odxModel
                  };

                  var parsedApplication = new ParsedBizTalkApplication
                  {
                      Application = new BizTalkApplication()
                  };

                  parsedApplication.Application.Orchestrations.Add(orchestration);
                  parsedApplication.Application.Schemas.Add(documentSchema);

                  var aisModel = new AzureIntegrationServicesModel();
                  var group = new ParsedBizTalkApplicationGroup();
                  aisModel.MigrationSource.MigrationSourceModel = group;
                  group.Applications.Add(parsedApplication);

                  model = aisModel;

                  var msiResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerMsi,
                      Key = "msiContainerKey",
                      Name = "msiContainerName"
                  };

                  var cabResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerCab,
                      Key = "cabContainerKey",
                      Name = "cabContainerName"
                  };

                  var assemblyResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerAssembly,
                      Key = "assemblyContainerKey",
                      Name = "assemblyContainerName"
                  };

                  var orchestrationResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionOrchestration,
                      Key = "orchestrationDefinitionKey",
                      Name = "orchestrationDefinitionName"
                  };

                  var serviceDeclarationResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceServiceDeclaration,
                      Key = "serviceDeclarationKey",
                      Name = "serviceDeclarationName",
                      SourceObject = orchestration.FindServiceDeclaration()
                  };

                  var messageDeclarationResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceMessageDeclaration,
                      Key = "messageDeclarationKey",
                      Name = messageDeclarationName,
                      SourceObject = null // This is the missing object that drives the error
                  };

                  var schemaResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionSchema,
                      Key = "schemaDefinitionKey",
                      Name = "schemaDefinitionName"
                  };

                  var schemaResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceDocumentSchema,
                      Key = schemaResourceKey,
                      Name = messageDeclarationName,
                      SourceObject = documentSchema,
                      ParentRefId = schemaResourceDefinition.RefId
                  };

                  var messageTypeResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceMessageType,
                      Key = schemaResourceKey,
                      Name = messageDeclarationName,
                      SourceObject = documentSchema.MessageDefinitions[0], 
                      ParentRefId = schemaResource.RefId
                  };

                  schemaResource.Resources.Add(messageTypeResource);
                  schemaResourceDefinition.Resources.Add(schemaResource);
                  serviceDeclarationResource.Resources.Add(messageDeclarationResource);
                  orchestrationResourceDefinition.Resources.Add(serviceDeclarationResource);
                  assemblyResourceContainer.ResourceDefinitions.Add(orchestrationResourceDefinition);
                  assemblyResourceContainer.ResourceDefinitions.Add(schemaResourceDefinition);
                  cabResourceContainer.ResourceContainers.Add(assemblyResourceContainer);
                  msiResourceContainer.ResourceContainers.Add(cabResourceContainer);
                  aisModel.MigrationSource.ResourceContainers.Add(msiResourceContainer);
              });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(1));

            "And resource node should have the expected relationships created"
                .x(() =>
                {
                    // Get the resources from the model.
                    var resources = model.FindAllResources();

                    // Test the message declaration resource.
                    var messageDeclaration = resources.Where(r => r.Type == ModelConstants.ResourceMessageDeclaration).FirstOrDefault();
                    messageDeclaration.Should().NotBeNull();
                    messageDeclaration.Name.Should().Be(messageDeclarationName);
                    messageDeclaration.ReportMessages.Should().HaveCount(1);
                    messageDeclaration.ReportMessages[0].Message.Should().Contain("The source object associated");
                    messageDeclaration.ResourceRelationships.Should().HaveCount(0);

                    // Test the relationships
                    var schema = resources.Where(r => r.Type == ModelConstants.ResourceDocumentSchema).FirstOrDefault();
                    schema.Should().NotBeNull();
                    schema.ResourceRelationships.Should().HaveCount(0);
                });
        }

        #endregion

        #region Empty model tests

        /// <summary>
        /// Scenario tests the rule is skipped if the source model is empty.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003RuleSkippedIfModelIsEmpty(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given an source model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));
        }

        #endregion

        #region Transform dependency tests

        /// <summary>
        /// Scenario tests resolving transform declaration dependencies.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003ResolveTransformDependenciesWithSuccess(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var transformFullName = "Test.Maps.Map1";
            var transformResourceKey = "transformResourceKey";
            var serviceDeclarationName = "serviceDeclarationName";

            "Given a source model with an orchestration, a service declaration and transform"
              .x(() =>
              {
                  var aisModel = new AzureIntegrationServicesModel();

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
                                                Type = "ServiceBody",
                                                Element1 = new Element[]
                                                {
                                                    new Element
                                                    {
                                                        Type = "Transform",
                                                        Property = new ElementProperty[]
                                                        {
                                                            new ElementProperty { Name = "ClassName", Value = transformFullName }
                                                        }
                                                    }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                        }
                  };

                  var transform = new Transform()
                  {
                      Name = "Map1",
                      Namespace = "Test.Maps",
                      ResourceDefinitionKey = transformResourceKey,
                      FullName = transformFullName
                  };

                  var orchestration = new Orchestration
                  {
                      Name = "orchestrationDefinitionName",
                      ResourceDefinitionKey = "orchestrationDefinitionKey",
                      Model = odxModel
                  };

                  var parsedApplication = new ParsedBizTalkApplication
                  {
                      Application = new BizTalkApplication()
                  };

                  parsedApplication.Application.Orchestrations.Add(orchestration);
                  parsedApplication.Application.Transforms.Add(transform);

                  var group = new ParsedBizTalkApplicationGroup();
                  aisModel.MigrationSource.MigrationSourceModel = group;
                  group.Applications.Add(parsedApplication);

                  var msiResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerMsi,
                      Key = "msiContainerKey",
                      Name = "msiContainerName"
                  };

                  var cabResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerCab,
                      Key = "cabContainerKey",
                      Name = "cabContainerName"
                  };

                  var assemblyResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerAssembly,
                      Key = "assemblyContainerKey",
                      Name = "assemblyContainerName"
                  };

                  var orchestrationResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionOrchestration,
                      Key = "orchestrationDefinitionKey",
                      Name = "orchestrationDefinitionName"
                  };

                  var serviceDeclarationResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceServiceDeclaration,
                      Key = "serviceDeclarationKey",
                      Name = serviceDeclarationName,
                      SourceObject = orchestration.FindServiceDeclaration()
                  };

                  var transformResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionMap,
                      Key = transformResourceKey,
                      Name = "transformDefinitionName",
                      SourceObject = orchestration.FindTransforms().First()
                  };

                  var transformResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceMap,
                      Key = transformResourceKey,
                      Name = transformFullName,
                      SourceObject = transform
                  };

                  transformResourceDefinition.Resources.Add(transformResource);
                  orchestrationResourceDefinition.Resources.Add(serviceDeclarationResource);
                  assemblyResourceContainer.ResourceDefinitions.Add(orchestrationResourceDefinition);
                  assemblyResourceContainer.ResourceDefinitions.Add(transformResourceDefinition);
                  cabResourceContainer.ResourceContainers.Add(assemblyResourceContainer);
                  msiResourceContainer.ResourceContainers.Add(cabResourceContainer);
                  aisModel.MigrationSource.ResourceContainers.Add(msiResourceContainer);

                  model = aisModel;
              });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resource node should have the expected relationships created"
                .x(() =>
                {
                    // Get the resources from the model.
                    var resources = model.FindAllResources();

                    // Test the service declaration resource.
                    var serviceDeclaration = resources.Where(r => r.Type == ModelConstants.ResourceServiceDeclaration).FirstOrDefault();
                    serviceDeclaration.Should().NotBeNull();
                    serviceDeclaration.Name.Should().Be(serviceDeclarationName);
                    serviceDeclaration.ReportMessages.Should().HaveCount(0);
                    serviceDeclaration.ResourceRelationships.Should().HaveCount(1);

                    // Test the relationships
                    var map = resources.Where(r => r.Type == ModelConstants.ResourceMap).FirstOrDefault();
                    map.Should().NotBeNull();
                    map.ResourceRelationships.Should().HaveCount(1);

                    serviceDeclaration.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencesTo);
                    serviceDeclaration.ResourceRelationships[0].ResourceRefId.Should().Be(map.RefId);

                    map.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencedBy);
                    map.ResourceRelationships[0].ResourceRefId.Should().Be(serviceDeclaration.RefId);
                });
        }

        /// <summary>
        /// Scenario tests resolving message declaration when the transform definition is missing.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003ResolveTransformDependenciesWithMissingTransformDefinition(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var transformFullName = "Test.Maps.Map1";
            var transformResourceKey = "transformResourceKey";
            var serviceDeclarationName = "serviceDeclarationName";

            "Given a source model with an orchestration, a service declaration and transform"
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
                                                Type = "ServiceBody",
                                                Element1 = new Element[]
                                                {
                                                    new Element
                                                    {
                                                        Type = "Transform",
                                                        Property = new ElementProperty[]
                                                        {
                                                            new ElementProperty { Name = "ClassName", Value = transformFullName }
                                                        }
                                                    }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                      }
                  };

                  var transform = new Transform()
                  {
                      Name = "Map1",
                      Namespace = "Test.Maps",
                      ResourceDefinitionKey = transformResourceKey,
                      FullName = transformFullName
                  };

                  var orchestration = new Orchestration
                  {
                      Name = "orchestrationDefinitionName",
                      ResourceDefinitionKey = "orchestrationDefinitionKey",
                      Model = odxModel
                  };

                  var parsedApplication = new ParsedBizTalkApplication
                  {
                      Application = new BizTalkApplication()
                  };

                  parsedApplication.Application.Orchestrations.Add(orchestration);
                  parsedApplication.Application.Transforms.Add(transform);

                  var aisModel = new AzureIntegrationServicesModel();
                  var group = new ParsedBizTalkApplicationGroup();
                  aisModel.MigrationSource.MigrationSourceModel = group;
                  group.Applications.Add(parsedApplication);

                  model = aisModel;

                  var msiResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerMsi,
                      Key = "msiContainerKey",
                      Name = "msiContainerName"
                  };

                  var cabResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerCab,
                      Key = "cabContainerKey",
                      Name = "cabContainerName"
                  };

                  var assemblyResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerAssembly,
                      Key = "assemblyContainerKey",
                      Name = "assemblyContainerName"
                  };

                  var orchestrationResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionOrchestration,
                      Key = "orchestrationDefinitionKey",
                      Name = "orchestrationDefinitionName"
                  };

                  var serviceDeclarationResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceServiceDeclaration,
                      Key = "serviceDeclarationKey",
                      Name = serviceDeclarationName,
                      SourceObject = orchestration.FindServiceDeclaration()
                  };

                  orchestrationResourceDefinition.Resources.Add(serviceDeclarationResource);
                  assemblyResourceContainer.ResourceDefinitions.Add(orchestrationResourceDefinition);
                  cabResourceContainer.ResourceContainers.Add(assemblyResourceContainer);
                  msiResourceContainer.ResourceContainers.Add(cabResourceContainer);
                  aisModel.MigrationSource.ResourceContainers.Add(msiResourceContainer);
              });
            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be one context error"
                .x(() =>
                {
                    var resources = model.FindAllResources();
                    var serviceDeclaration = resources.Where(r => r.Type == ModelConstants.ResourceServiceDeclaration).Single();
                    serviceDeclaration.ReportMessages.Count.Should().Be(1);
                    serviceDeclaration.ReportMessages[0].Severity.Should().Be(MessageSeverity.Warning);
                    serviceDeclaration.ReportMessages[0].Message.Should().Contain(transformFullName);
                });
        }

        /// <summary>
        /// Scenario tests resolving transform declaration dependencies with a missing transform in the application
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003ResolveTransformDependenciesWithTransform(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var transformFullName = "Test.Maps.Map1";
            var transformResourceKey = "transformResourceKey";
            var serviceDeclarationName = "serviceDeclarationName";

            "Given a source model with an orchestration, a service declaration and a missing transform"
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
                                                Type = "ServiceBody",
                                                Element1 = new Element[]
                                                {
                                                    new Element
                                                    {
                                                        Type = "Transform",
                                                        Property = new ElementProperty[]
                                                        {
                                                            new ElementProperty { Name = "ClassName", Value = transformFullName }
                                                        }
                                                    }
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
                      Name = "orchestrationDefinitionName",
                      ResourceDefinitionKey = "orchestrationDefinitionKey",
                      Model = odxModel
                  };

                  var parsedApplication = new ParsedBizTalkApplication
                  {
                      Application = new BizTalkApplication()
                  };

                  parsedApplication.Application.Orchestrations.Add(orchestration);

                  var aisModel = new AzureIntegrationServicesModel();
                  var group = new ParsedBizTalkApplicationGroup();
                  aisModel.MigrationSource.MigrationSourceModel = group;
                  group.Applications.Add(parsedApplication);

                  model = aisModel;

                  var msiResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerMsi,
                      Key = "msiContainerKey",
                      Name = "msiContainerName"
                  };

                  var cabResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerCab,
                      Key = "cabContainerKey",
                      Name = "cabContainerName"
                  };

                  var assemblyResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerAssembly,
                      Key = "assemblyContainerKey",
                      Name = "assemblyContainerName"
                  };

                  var orchestrationResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionOrchestration,
                      Key = "orchestrationDefinitionKey",
                      Name = "orchestrationDefinitionName"
                  };

                  var serviceDeclarationResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceServiceDeclaration,
                      Key = "serviceDeclarationKey",
                      Name = serviceDeclarationName,
                      SourceObject = orchestration.FindServiceDeclaration()
                  };

                  var transformResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionMap,
                      Key = transformResourceKey,
                      Name = "transformDefinitionName"
                  };

                  var transformResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceMap,
                      Key = transformResourceKey,
                      Name = transformFullName,
                      SourceObject = null
                  };

                  transformResourceDefinition.Resources.Add(transformResource);
                  orchestrationResourceDefinition.Resources.Add(serviceDeclarationResource);
                  assemblyResourceContainer.ResourceDefinitions.Add(orchestrationResourceDefinition);
                  assemblyResourceContainer.ResourceDefinitions.Add(transformResourceDefinition);
                  cabResourceContainer.ResourceContainers.Add(assemblyResourceContainer);
                  msiResourceContainer.ResourceContainers.Add(cabResourceContainer);
                  aisModel.MigrationSource.ResourceContainers.Add(msiResourceContainer);
              });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resource node should have no relationships created and one warning"
                .x(() =>
                {
                    // Get the resources from the model.
                    var resources = model.FindAllResources();

                    // Test the service declaration resource.
                    var serviceDeclaration = resources.Where(r => r.Type == ModelConstants.ResourceServiceDeclaration).FirstOrDefault();
                    serviceDeclaration.Should().NotBeNull();
                    serviceDeclaration.Name.Should().Be(serviceDeclarationName);
                    serviceDeclaration.ReportMessages.Should().HaveCount(1);
                    serviceDeclaration.ReportMessages[0].Severity.Should().Be(MessageSeverity.Warning);
                    serviceDeclaration.ReportMessages[0].Message.Should().Contain(transformFullName);
                    serviceDeclaration.ResourceRelationships.Should().HaveCount(0);
                });
        }

        /// <summary>
        /// Scenario tests resolving transform declaration dependencies with a missing transform resource.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003ResolveTransformDependenciesWithMissingTransformResource(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var transformFullName = "Test.Maps.Map1";
            var transformResourceKey = "transformResourceKey";
            var serviceDeclarationName = "serviceDeclarationName";

            "Given a source model with an orchestration, a service declaration and transform"
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
                                                Type = "ServiceBody",
                                                Element1 = new Element[]
                                                {
                                                    new Element
                                                    {
                                                        Type = "Transform",
                                                        Property = new ElementProperty[]
                                                        {
                                                            new ElementProperty { Name = "ClassName", Value = transformFullName }
                                                        }
                                                    }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                        }
                  };

                  var transform = new Transform()
                  {
                      Name = "Map1",
                      Namespace = "Test.Maps",
                      ResourceDefinitionKey = transformResourceKey,
                      FullName = transformFullName
                  };

                  var orchestration = new Orchestration
                  {
                      Name = "orchestrationDefinitionName",
                      ResourceDefinitionKey = "orchestrationDefinitionKey",
                      Model = odxModel
                  };

                  var parsedApplication = new ParsedBizTalkApplication
                  {
                      Application = new BizTalkApplication()
                  };

                  parsedApplication.Application.Orchestrations.Add(orchestration);
                  parsedApplication.Application.Transforms.Add(transform);

                  var aisModel = new AzureIntegrationServicesModel();
                  var group = new ParsedBizTalkApplicationGroup();
                  aisModel.MigrationSource.MigrationSourceModel = group;
                  group.Applications.Add(parsedApplication);

                  model = aisModel;

                    var msiResourceContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerMsi,
                        Key = "msiContainerKey",
                        Name = "msiContainerName"
                    };

                    var cabResourceContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerCab,
                        Key = "cabContainerKey",
                        Name = "cabContainerName"
                    };

                    var assemblyResourceContainer = new ResourceContainer
                    {
                        Type = ModelConstants.ResourceContainerAssembly,
                        Key = "assemblyContainerKey",
                        Name = "assemblyContainerName"
                    };

                    var orchestrationResourceDefinition = new ResourceDefinition
                    {
                        Type = ModelConstants.ResourceDefinitionOrchestration,
                        Key = "orchestrationDefinitionKey",
                        Name = "orchestrationDefinitionName"
                    };

                    var serviceDeclarationResource = new ResourceItem
                    {
                        Type = ModelConstants.ResourceServiceDeclaration,
                        Key = "serviceDeclarationKey",
                        Name = serviceDeclarationName,
                        SourceObject = orchestration.FindServiceDeclaration()
                    };

                    var transformResourceDefinition = new ResourceDefinition
                    {
                        Type = ModelConstants.ResourceDefinitionMap,
                        Key = transformResourceKey,
                        Name = "transformDefinitionName"
                    };

                    orchestrationResourceDefinition.Resources.Add(serviceDeclarationResource);
                    assemblyResourceContainer.ResourceDefinitions.Add(orchestrationResourceDefinition);
                    assemblyResourceContainer.ResourceDefinitions.Add(transformResourceDefinition);
                    cabResourceContainer.ResourceContainers.Add(assemblyResourceContainer);
                    msiResourceContainer.ResourceContainers.Add(cabResourceContainer);
                    aisModel.MigrationSource.ResourceContainers.Add(msiResourceContainer);
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be one context error"
                .x(() =>
                {
                    // Get the resources from the model.
                    var resources = model.FindAllResources();

                    // Test the service declaration resource.
                    var serviceDeclaration = resources.Where(r => r.Type == ModelConstants.ResourceServiceDeclaration).FirstOrDefault();
                    serviceDeclaration.Should().NotBeNull();
                    serviceDeclaration.Name.Should().Be(serviceDeclarationName);
                    serviceDeclaration.ReportMessages.Should().HaveCount(1);
                    serviceDeclaration.ReportMessages[0].Severity.Should().Be(MessageSeverity.Warning);
                    serviceDeclaration.ReportMessages[0].Message.Should().Contain(transformFullName);
                    serviceDeclaration.ResourceRelationships.Should().HaveCount(0);
                });
        }

        /// <summary>
        /// Scenario tests resolving transform declaration dependencies when there are multiple matches, e.g. when 2 applications have used the same map.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003ResolveTransformDependenciesWithMultipleMatches(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var transformFullName = "Test.Maps.Map1";
            var transformResourceKey = "transformResourceKey";
            var serviceDeclarationName = "serviceDeclarationName";

            "Given a source model with an orchestration, a service declaration and transform"
              .x(() =>
              {
                  var aisModel = new AzureIntegrationServicesModel();

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
                                                Type = "ServiceBody",
                                                Element1 = new Element[]
                                                {
                                                    new Element
                                                    {
                                                        Type = "Transform",
                                                        Property = new ElementProperty[]
                                                        {
                                                            new ElementProperty { Name = "ClassName", Value = transformFullName }
                                                        }
                                                    }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                        }
                  };

                  var transform = new Transform()
                  {
                      Name = "Map1",
                      Namespace = "Test.Maps",
                      ResourceDefinitionKey = transformResourceKey,
                      FullName = transformFullName
                  };

                  var transform2 = new Transform()
                  {
                      Name = "Map2",
                      Namespace = "Test.Maps",
                      ResourceDefinitionKey = transformResourceKey,
                      FullName = transformFullName
                  };

                  var orchestration = new Orchestration
                  {
                      Name = "orchestrationDefinitionName",
                      ResourceDefinitionKey = "orchestrationDefinitionKey",
                      Model = odxModel
                  };

                  var parsedApplication = new ParsedBizTalkApplication
                  {
                      Application = new BizTalkApplication()
                  };

                  parsedApplication.Application.Orchestrations.Add(orchestration);
                  parsedApplication.Application.Transforms.Add(transform);

                  var group = new ParsedBizTalkApplicationGroup();
                  aisModel.MigrationSource.MigrationSourceModel = group;
                  group.Applications.Add(parsedApplication);

                  var msiResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerMsi,
                      Key = "msiContainerKey",
                      Name = "msiContainerName"
                  };

                  var cabResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerCab,
                      Key = "cabContainerKey",
                      Name = "cabContainerName"
                  };

                  var assemblyResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerAssembly,
                      Key = "assemblyContainerKey",
                      Name = "assemblyContainerName"
                  };

                  var orchestrationResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionOrchestration,
                      Key = "orchestrationDefinitionKey",
                      Name = "orchestrationDefinitionName"
                  };

                  var serviceDeclarationResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceServiceDeclaration,
                      Key = "serviceDeclarationKey",
                      Name = serviceDeclarationName,
                      SourceObject = orchestration.FindServiceDeclaration()
                  };

                  var transformResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionMap,
                      Key = transformResourceKey,
                      Name = "transformDefinitionName",
                      SourceObject = orchestration.FindTransforms().First()
                  };

                  var transformResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceMap,
                      Key = transformResourceKey,
                      Name = transformFullName,
                      SourceObject = transform
                  };

                  var transformResource2 = new ResourceItem
                  {
                      Type = ModelConstants.ResourceMap,
                      Key = string.Concat(transformResourceKey, "2"),
                      Name = transformFullName,
                      SourceObject = transform2
                  };

                  transformResourceDefinition.Resources.Add(transformResource);
                  transformResourceDefinition.Resources.Add(transformResource2);
                  orchestrationResourceDefinition.Resources.Add(serviceDeclarationResource);
                  assemblyResourceContainer.ResourceDefinitions.Add(orchestrationResourceDefinition);
                  assemblyResourceContainer.ResourceDefinitions.Add(transformResourceDefinition);
                  cabResourceContainer.ResourceContainers.Add(assemblyResourceContainer);
                  msiResourceContainer.ResourceContainers.Add(cabResourceContainer);
                  aisModel.MigrationSource.ResourceContainers.Add(msiResourceContainer);

                  model = aisModel;
              });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resource node should have the expected relationships created"
                .x(() =>
                {
                    // Get the resources from the model.
                    var resources = model.FindAllResources();

                    // Test the service declaration resource.
                    var serviceDeclaration = resources.Where(r => r.Type == ModelConstants.ResourceServiceDeclaration).FirstOrDefault();
                    serviceDeclaration.Should().NotBeNull();
                    serviceDeclaration.Name.Should().Be(serviceDeclarationName);
                    serviceDeclaration.ReportMessages.Should().HaveCount(1);
                    serviceDeclaration.ReportMessages[0].Message.Should().Contain("Dependency cannot be accurately resolved");
                    serviceDeclaration.ResourceRelationships.Should().HaveCount(0);

                    // Test the relationships
                    var map = resources.Where(r => r.Type == ModelConstants.ResourceMap).FirstOrDefault();
                    map.Should().NotBeNull();
                    map.ResourceRelationships.Should().HaveCount(0);
                });
        }

        /// <summary>
        /// Scenario tests resolving transform declaration dependencies.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP003ResolveTransformDependenciesSourceObjectMissing(DP003OrchestrationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var transformFullName = "Test.Maps.Map1";
            var transformResourceKey = "transformResourceKey";
            var serviceDeclarationName = "serviceDeclarationName";

            "Given a source model with an orchestration, a service declaration and transform"
              .x(() =>
              {
                  var aisModel = new AzureIntegrationServicesModel();

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
                                                Type = "ServiceBody",
                                                Element1 = new Element[]
                                                {
                                                    new Element
                                                    {
                                                        Type = "Transform",
                                                        Property = new ElementProperty[]
                                                        {
                                                            new ElementProperty { Name = "ClassName", Value = transformFullName }
                                                        }
                                                    }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                        }
                  };

                  var transform = new Transform()
                  {
                      Name = "Map1",
                      Namespace = "Test.Maps",
                      ResourceDefinitionKey = transformResourceKey,
                      FullName = transformFullName
                  };

                  var orchestration = new Orchestration
                  {
                      Name = "orchestrationDefinitionName",
                      ResourceDefinitionKey = "orchestrationDefinitionKey",
                      Model = odxModel
                  };

                  var parsedApplication = new ParsedBizTalkApplication
                  {
                      Application = new BizTalkApplication()
                  };

                  parsedApplication.Application.Orchestrations.Add(orchestration);
                  parsedApplication.Application.Transforms.Add(transform);

                  var group = new ParsedBizTalkApplicationGroup();
                  aisModel.MigrationSource.MigrationSourceModel = group;
                  group.Applications.Add(parsedApplication);

                  var msiResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerMsi,
                      Key = "msiContainerKey",
                      Name = "msiContainerName"
                  };

                  var cabResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerCab,
                      Key = "cabContainerKey",
                      Name = "cabContainerName"
                  };

                  var assemblyResourceContainer = new ResourceContainer
                  {
                      Type = ModelConstants.ResourceContainerAssembly,
                      Key = "assemblyContainerKey",
                      Name = "assemblyContainerName"
                  };

                  var orchestrationResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionOrchestration,
                      Key = "orchestrationDefinitionKey",
                      Name = "orchestrationDefinitionName"
                  };

                  var serviceDeclarationResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceServiceDeclaration,
                      Key = "serviceDeclarationKey",
                      Name = serviceDeclarationName,
                      SourceObject = null // This is the missing object that drives the failure
                  };

                  var transformResourceDefinition = new ResourceDefinition
                  {
                      Type = ModelConstants.ResourceDefinitionMap,
                      Key = transformResourceKey,
                      Name = "transformDefinitionName",
                      SourceObject = orchestration.FindTransforms().First()
                  };

                  var transformResource = new ResourceItem
                  {
                      Type = ModelConstants.ResourceMap,
                      Key = transformResourceKey,
                      Name = transformFullName,
                      SourceObject = transform
                  };

                  transformResourceDefinition.Resources.Add(transformResource);
                  orchestrationResourceDefinition.Resources.Add(serviceDeclarationResource);
                  assemblyResourceContainer.ResourceDefinitions.Add(orchestrationResourceDefinition);
                  assemblyResourceContainer.ResourceDefinitions.Add(transformResourceDefinition);
                  cabResourceContainer.ResourceContainers.Add(assemblyResourceContainer);
                  msiResourceContainer.ResourceContainers.Add(cabResourceContainer);
                  aisModel.MigrationSource.ResourceContainers.Add(msiResourceContainer);

                  model = aisModel;
              });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP003OrchestrationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(1));

            "And resource node should have the expected relationships created"
                .x(() =>
                {
                    // Get the resources from the model.
                    var resources = model.FindAllResources();

                    // Test the service declaration resource.
                    var serviceDeclaration = resources.Where(r => r.Type == ModelConstants.ResourceServiceDeclaration).FirstOrDefault();
                    serviceDeclaration.Should().NotBeNull();
                    serviceDeclaration.Name.Should().Be(serviceDeclarationName);
                    serviceDeclaration.ReportMessages.Should().HaveCount(1);
                    serviceDeclaration.ReportMessages[0].Message.Should().Contain("The source object associated");
                    serviceDeclaration.ResourceRelationships.Should().HaveCount(0);

                    // Test the relationships
                    var map = resources.Where(r => r.Type == ModelConstants.ResourceMap).FirstOrDefault();
                    map.Should().NotBeNull();
                    map.ResourceRelationships.Should().HaveCount(0);
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
