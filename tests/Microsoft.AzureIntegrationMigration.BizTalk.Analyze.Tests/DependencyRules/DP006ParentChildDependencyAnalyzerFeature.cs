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
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Tests.DependencyRules
{
    /// <summary>
    /// Tests for the <see cref="DP006ParentChildDependencyAnalyzer"/> class.
    /// </summary>
    public class DP006ParentChildDependencyAnalyzerFeature
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
        public void ConstructWithNullModel(DP006ParentChildDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP006ParentChildDependencyAnalyzer(model, context, logger)));

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
        public void ConstructWithNullContext(DP006ParentChildDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP006ParentChildDependencyAnalyzer(model, context, logger)));

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
        public void ConstructWithNullLogger(DP006ParentChildDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP006ParentChildDependencyAnalyzer(model, context, logger)));

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
        public void ConstructWithSuccess(DP006ParentChildDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP006ParentChildDependencyAnalyzer(model, context, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region DP006 Scenarios

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
        public void DP006RuleSkippedIfModelIsEmpty(DP006ParentChildDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given an source model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP006ParentChildDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And the logger should have been called once"
                .x(() => _mockLogger.Invocations.Count.Should().Be(2));
        }

        /// <summary>
        /// Scenario tests that the rule succeeds in matching dependencies between resources.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP006ResolveDependenciesWithSuccess(DP006ParentChildDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
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
                .x(() => analyzer = new DP006ParentChildDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resources should have the expected relationships created"
                .x(() =>
                {
                    // Test the resources
                    var resources = model.FindAllResources();

                    var receivePort = resources.Where(r => r.Type == ModelConstants.ResourceReceivePort).First();
                    receivePort.Should().NotBeNull();

                    var receiveLocation = resources.Where(r => r.Type == ModelConstants.ResourceReceiveLocation).First();
                    receiveLocation.Should().NotBeNull();

                    // Check that the receive port has a parent-child to receive location
                    receivePort.ResourceRelationships.Count.Should().Be(1);
                    receivePort.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.Child);
                    receivePort.ResourceRelationships[0].ResourceRefId.Should().Be(receiveLocation.RefId);

                    // Check that the receive location has a child-parent to receive port
                    receiveLocation.ResourceRelationships.Count.Should().Be(1);
                    receiveLocation.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.Parent);
                    receiveLocation.ResourceRelationships[0].ResourceRefId.Should().Be(receivePort.RefId);
                });
        }

        /// <summary>
        /// Scenario tests when there are no children.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP006ResolveDependenciesNoChildren(DP006ParentChildDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given a source model"
                .x(() =>
                {
                    model = TestHelper.CreateDefaultModelForAnalyzing();
                    model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceReceivePort).ToList().ForEach(r => r.Resources.Clear());  // Clears the children.
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP006ParentChildDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resources should have the expected relationships created"
                .x(() =>
                {
                    // Test the resources
                    var resources = model.FindAllResources();

                    var receivePort = resources.Where(r => r.Type == ModelConstants.ResourceReceivePort).First();
                    receivePort.Should().NotBeNull();

                    // Check that the receive port has a parent-child to receive location
                    receivePort.ResourceRelationships.Count.Should().Be(0);
                });
        }

        #endregion
    }
}
