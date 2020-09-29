// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.DependencyRules;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Tests.DependencyRules
{
    /// <summary>
    /// Tests for the <see cref="DP004ApplicationDependencyAnalyzer"/> class.
    /// </summary>
    public class DP004ApplicationDependencyAnalyzerFeature
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
        public void ConstructWithNullModel(DP004ApplicationDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP004ApplicationDependencyAnalyzer(model, context, logger)));

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
        public void ConstructWithNullContext(DP004ApplicationDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP004ApplicationDependencyAnalyzer(model, context, logger)));

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
        public void ConstructWithNullLogger(DP004ApplicationDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP004ApplicationDependencyAnalyzer(model, context, logger)));

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
        public void ConstructWithSuccess(DP004ApplicationDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP004ApplicationDependencyAnalyzer(model, context, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region DP004 Rule Tests

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
        public void DP004RuleSkippedIfModelIsEmpty(DP004ApplicationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given an source model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP004ApplicationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));
        }

        /// <summary>
        /// Scenario tests that the rule succeeds in matching references between applications.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP004ResolveApplicationDependenciesWithSuccess(DP004ApplicationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
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
                .x(() => analyzer = new DP004ApplicationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And report resource node should have the expected relationships created"
                .x(() =>
                {
                    // Test the applications
                    var applications = model.GetSourceModel<ParsedBizTalkApplicationGroup>().Applications;
                    applications.Should().NotBeNull().And.HaveCount(3);

                    applications[0].Application.ApplicationDefinition.Resource.ReportMessages.Should().HaveCount(0);
                    applications[0].Application.ApplicationDefinition.Resource.ResourceRelationships.Should().HaveCount(0);

                    applications[1].Application.ApplicationDefinition.Resource.ReportMessages.Should().HaveCount(0);
                    applications[1].Application.ApplicationDefinition.Resource.ResourceRelationships.Should().HaveCount(1);
                    applications[1].Application.ApplicationDefinition.Resource.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencesTo);
                    applications[1].Application.ApplicationDefinition.Resource.ResourceRelationships[0].ResourceRefId.Should().Be(applications[2].Application.ApplicationDefinition.Resource.RefId);

                    applications[2].Application.ApplicationDefinition.Resource.ReportMessages.Should().HaveCount(0);
                    applications[2].Application.ApplicationDefinition.Resource.ResourceRelationships.Should().HaveCount(1);
                    applications[2].Application.ApplicationDefinition.Resource.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.ReferencedBy);
                    applications[2].Application.ApplicationDefinition.Resource.ResourceRelationships[0].ResourceRefId.Should().Be(applications[1].Application.ApplicationDefinition.Resource.RefId);
                });
        }

        /// <summary>
        /// Scenario tests that the rule produces warning messages when there are missing references.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP004ResolveApplicationDependenciesWithWarnings(DP004ApplicationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given a source model"
                .x(() =>
                {
                    model = TestHelper.CreateDefaultModelForAnalyzing();

                    foreach (var application in model.GetSourceModel<ParsedBizTalkApplicationGroup>().Applications)
                    {
                        var appDef = application.Application.ApplicationDefinition.ApplicationDefinition;
                        if (appDef.References != null && appDef.References.Length > 0)
                        {
                            application.Application.ApplicationDefinition.ApplicationDefinition.References[0].Name = "unknown app";
                        }
                    }
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP004ApplicationDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And report resource node should have warning messages"
                .x(() =>
                {
                    // Test the applications
                    var applications = model.GetSourceModel<ParsedBizTalkApplicationGroup>().Applications;
                    applications.Should().NotBeNull().And.HaveCount(3);

                    applications[0].Application.ApplicationDefinition.Resource.ReportMessages.Should().HaveCount(0);
                    applications[0].Application.ApplicationDefinition.Resource.ResourceRelationships.Should().HaveCount(0);

                    applications[1].Application.ApplicationDefinition.Resource.ReportMessages.Should().HaveCount(1);
                    applications[1].Application.ApplicationDefinition.Resource.ResourceRelationships.Should().HaveCount(0);

                    applications[2].Application.ApplicationDefinition.Resource.ReportMessages.Should().HaveCount(0);
                    applications[2].Application.ApplicationDefinition.Resource.ResourceRelationships.Should().HaveCount(0);
                });
        }

        /// <summary>
        /// Scenario tests that the rule raises an error if the application resource for the referencing application doesn't exist.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP004ResolveApplicationDependenciesWithNoReferencingApplicationResourceWithErrors(DP004ApplicationDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given a source model with an application resource with the wrong key"
                .x(() =>
                {
                    model = TestHelper.CreateDefaultModelForAnalyzing();
                    var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();
                    group.Applications[1].Application.ApplicationDefinition.Resource = null;  // Lose pointer to the resource.
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP004ApplicationDependencyAnalyzer(model, context, logger));
            
            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be an error"
                .x(() => context.Errors.Should().HaveCount(1));

            "And report resource node should not have the expected relationships created"
                .x(() =>
                {
                    // Test the errors
                    context.Errors.Count.Should().Be(1);
                    context.Errors[0].Message.Should().Contain("app-resource-2");
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
