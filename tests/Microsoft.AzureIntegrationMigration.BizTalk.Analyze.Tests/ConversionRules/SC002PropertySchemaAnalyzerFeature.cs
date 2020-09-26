using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.DependencyRules;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Tests.ConversionRules
{
    /// <summary>
    /// Tests for the <see cref="SC002PropertySchemaAnalyzer"/> class.
    /// </summary>
    public class SC002PropertySchemaAnalyzerFeature
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
        public void ConstructWithNullModel(SC002PropertySchemaAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new SC002PropertySchemaAnalyzer(model, context, logger)));

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
        public void ConstructWithNullContext(SC002PropertySchemaAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new SC002PropertySchemaAnalyzer(model, context, logger)));

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
        public void ConstructWithNullLogger(SC002PropertySchemaAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new SC002PropertySchemaAnalyzer(model, context, logger)));

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
        public void ConstructWithSuccess(SC002PropertySchemaAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new SC002PropertySchemaAnalyzer(model, context, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region SC002 Tests

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
        public void SC002RuleSkippedIfModelIsEmpty(SC002PropertySchemaAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given an source model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new SC002PropertySchemaAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));
        }


        /// <summary>
        /// Scenario tests the rule is run correctly.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SC002RuleRuleWithSuccess(SC002PropertySchemaAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given an source model"
                .x(() => model = TestHelper.CreateDefaultModelForAnalyzing());

            "And the model has a migration target "
                .x(() =>
                {
                    TestHelper.CopySourceToTarget(model);
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new SC002PropertySchemaAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And the routing properties should be set"
                .x(() =>
                {
                    model.Should().NotBeNull();
                    model.MigrationTarget.Should().NotBeNull();
                    model.MigrationTarget.MessageBus.Should().NotBeNull();
                    model.MigrationTarget.MessageBus.Applications.Should().HaveCount(3);

                    model.MigrationTarget.MessageBus.Applications[0].Messages.Should().HaveCount(3);
                    model.MigrationTarget.MessageBus.Applications[0].Messages[0].Name = "Test.Schemas.DocumentSchema1";
                    model.MigrationTarget.MessageBus.Applications[0].Messages[0].RoutingProperties.Should().HaveCount(1);
                    model.MigrationTarget.MessageBus.Applications[0].Messages[0].RoutingProperties.First().Key.Should().Be("Test.Schemas.PropertySchema1.Property1");
                    model.MigrationTarget.MessageBus.Applications[0].Messages[0].RoutingProperties.First().Value.Should().Be("some xpath 1");

                    model.MigrationTarget.MessageBus.Applications[0].Messages[1].Name = "Test.Schemas.DocumentSchema2";
                    model.MigrationTarget.MessageBus.Applications[0].Messages[1].RoutingProperties.Should().HaveCount(0);
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
