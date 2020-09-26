using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.DependencyRules;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
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
    /// Tests for the <see cref="DP002TransformDependencyAnalyzer"/> class.
    /// </summary>
    public class DP002TransformDependencyAnalyzerFeature
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
        public void ConstructWithNullModel(DP002TransformDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP002TransformDependencyAnalyzer(model, context, logger)));

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
        public void ConstructWithNullContext(DP002TransformDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP002TransformDependencyAnalyzer(model, context, logger)));

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
        public void ConstructWithNullLogger(DP002TransformDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP002TransformDependencyAnalyzer(model, context, logger)));

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
        public void ConstructWithSuccess(DP002TransformDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP002TransformDependencyAnalyzer(model, context, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region DP002 Rule Tests

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
        public void DP001RuleSkippedIfModelIsEmpty(DP002TransformDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given an source model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP002TransformDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));
        }

        /// <summary>
        /// Scenario tests rule against the standard data model.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP002ResolveSchemaAndTransformDependenciesWithSuccess(DP002TransformDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given a source model"
                .x(() =>
                {
                    model = TestHelper.CreateDefaultModelForAnalyzing();
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP002TransformDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And report resource node should have the expected relationships created"
                .x(() =>
                {
                    // Get the map
                    var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();
                    var map1 = group.Applications[0].Application.Transforms[0];
                    var map2 = group.Applications[0].Application.Transforms[1];

                    // Test the relationships
                    var schemas = group.Applications[0].Application.Schemas.Where(s => s.SchemaType == BizTalkSchemaType.Document).ToList();
                    schemas.Should().NotBeNull();
                    schemas.Should().HaveCountGreaterOrEqualTo(2);

                    map1.Resource.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencedBy);
                    map1.Resource.ResourceRelationships[0].ResourceRefId.Should().Be(schemas[0].Resource.RefId);

                    map1.Resource.ResourceRelationships[1].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencesTo);
                    map1.Resource.ResourceRelationships[1].ResourceRefId.Should().Be(schemas[1].Resource.RefId);

                    map2.Resource.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencedBy);
                    map2.Resource.ResourceRelationships[0].ResourceRefId.Should().Be(schemas[1].Resource.RefId);

                    map2.Resource.ResourceRelationships[1].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencesTo);
                    map2.Resource.ResourceRelationships[1].ResourceRefId.Should().Be(schemas[0].Resource.RefId);

                    var receivePorts = group.Applications[0].Application.Bindings.BindingInfo.ReceivePortCollection.ToList();
                    receivePorts.Should().NotBeNull();
                    receivePorts.Should().HaveCount(1);

                    map1.Resource.ResourceRelationships[2].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencedBy);
                    map1.Resource.ResourceRelationships[2].ResourceRefId.Should().Be(receivePorts[0].Resource.RefId);

                    map2.Resource.ResourceRelationships[2].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencedBy);
                    map2.Resource.ResourceRelationships[2].ResourceRefId.Should().Be(receivePorts[0].Resource.RefId);

                    var sendPorts = group.Applications[0].Application.Bindings.BindingInfo.SendPortCollection.ToList();
                    sendPorts.Should().NotBeNull();
                    sendPorts.Should().HaveCount(1);

                    map1.Resource.ResourceRelationships[3].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencedBy);
                    map1.Resource.ResourceRelationships[3].ResourceRefId.Should().Be(sendPorts[0].Resource.RefId);

                    map2.Resource.ResourceRelationships[3].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencedBy);
                    map2.Resource.ResourceRelationships[3].ResourceRefId.Should().Be(sendPorts[0].Resource.RefId);
                });
        }

        /// <summary>
        /// Scenario tests rule against the standard data model.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP002ResolveSchemaAndNoPortDependenciesWithSuccess(DP002TransformDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given a source model"
                .x(() =>
                {
                    model = TestHelper.CreateDefaultModelForAnalyzing();

                    foreach (var application in model.GetSourceModel<ParsedBizTalkApplicationGroup>().Applications)
                    {
                        if (application.Application.Bindings.BindingInfo.ReceivePortCollection != null)
                        {
                            foreach (var receivePort in application.Application.Bindings.BindingInfo.ReceivePortCollection)
                            {
                                receivePort.Transforms = null;
                                receivePort.OutboundTransforms = null;
                            }
                        }

                        if (application.Application.Bindings.BindingInfo.SendPortCollection != null)
                        {
                            foreach (var sendPort in application.Application.Bindings.BindingInfo.SendPortCollection)
                            {
                                sendPort.Transforms = null;
                                sendPort.InboundTransforms = null;
                            }
                        }
                    }
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP002TransformDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And report resource node should have the expected relationships created"
                .x(() =>
                {
                    // Get the map
                    var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();
                    var map = group.Applications[0].Application.Transforms[0];

                    // Test the relationships
                    var schemas = group.Applications[0].Application.Schemas.Where(s => s.SchemaType == BizTalkSchemaType.Document).ToList();
                    schemas.Should().NotBeNull();
                    schemas.Should().HaveCountGreaterOrEqualTo(2);

                    map.Resource.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencedBy);
                    map.Resource.ResourceRelationships[0].ResourceRefId.Should().Be(schemas[0].Resource.RefId);

                    map.Resource.ResourceRelationships[1].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencesTo);
                    map.Resource.ResourceRelationships[1].ResourceRefId.Should().Be(schemas[1].Resource.RefId);
                });
        }

        /// <summary>
        /// Scenario tests that when there are no referenced schemas that warning messages are created for the report.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP002ResolveSchemaDependenciesWithWarnings(DP002TransformDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given a source model"
                .x(() =>
                {
                    model = TestHelper.CreateDefaultModelForAnalyzing();

                    foreach (var application in model.GetSourceModel<ParsedBizTalkApplicationGroup>().Applications)
                    {
                        application.Application.Schemas.Clear();
                    }
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP002TransformDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And report resource node should have warnings"
                .x(() =>
                {
                    // Get the maps
                    var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

                    var map1 = group.Applications[0].Application.Transforms[0];
                    map1.Should().NotBeNull();
                    map1.Name.Should().Be("Transform1");
                    map1.Resource.ReportMessages.Should().HaveCount(2);
                    map1.Resource.ResourceRelationships.Should().HaveCount(2);

                    var map2 = group.Applications[0].Application.Transforms[1];
                    map2.Should().NotBeNull();
                    map2.Name.Should().Be("Transform2");
                    map2.Resource.ReportMessages.Should().HaveCount(2);
                    map2.Resource.ResourceRelationships.Should().HaveCount(2);
                });
        }

        /// <summary>
        /// Scenario tests that when there are no document schema resources that errors are output.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP002ResolveSchemaDependenciesWithNoSchemaResourcesWithErrors(DP002TransformDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var invalidKey = "invalidkey";

            "Given a source model"
                .x(() =>
                {
                    model = TestHelper.CreateDefaultModelForAnalyzing();

                    // Corrupt the keys for the schemas.
                    var parsedApplicationGroup = model.GetSourceModel<ParsedBizTalkApplicationGroup>();
                    var schemas = parsedApplicationGroup.Applications.Where(a => a.Application.Schemas != null).SelectMany(a => a.Application.Schemas);
                    foreach(var schema in schemas)
                    {
                        schema.ResourceKey = string.Concat(invalidKey, Guid.NewGuid());
                        schema.Resource = null;
                    }
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP002TransformDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be 2 context errors"
                .x(() =>
                {
                    context.Errors.Should().HaveCount(4);
                    context.Errors[0].Message.Should().Contain(invalidKey);
                    context.Errors[1].Message.Should().Contain(invalidKey);
                    context.Errors[2].Message.Should().Contain(invalidKey);
                    context.Errors[3].Message.Should().Contain(invalidKey);
                });
        }

        /// <summary>
        /// Scenario tests rule against the standard data model.  Error when a resource definition cannot be found.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP002ResolveSchemaDependenciesWithNoTransformResourceDefinitionWithErrors(DP002TransformDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var invalidKey = "invalidkey";

            "Given a source model"
                .x(() => model = TestHelper.CreateDefaultModelForAnalyzing());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP002TransformDependencyAnalyzer(model, context, logger));

            "And transform resource definition has incorrect key"
                .x(() => 
                {
                    var transform = model.GetSourceModel<ParsedBizTalkApplicationGroup>().Applications[0].Application.Transforms[0];
                    transform.Resource = null;
                    transform.ResourceKey = invalidKey;
                });

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be an error in the migration context"
                .x(() => context.Errors.Should().HaveCount(1));

            "And report node should not have the expected relationships created"
                .x(() =>
                {
                    context.Errors.Should().HaveCount(1);
                    context.Errors[0].Message.Should().Contain(invalidKey);
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
