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
    /// Tests for the <see cref="DP005DistributionListDependencyAnalyzer"/> class.
    /// </summary>
    public class DP005DistributionListDependencyAnalyzerFeature
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
        public void ConstructWithNullModel(DP005DistributionListDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP005DistributionListDependencyAnalyzer(model, context, logger)));

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
        public void ConstructWithNullContext(DP005DistributionListDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP005DistributionListDependencyAnalyzer(model, context, logger)));

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
        public void ConstructWithNullLogger(DP005DistributionListDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP005DistributionListDependencyAnalyzer(model, context, logger)));

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
        public void ConstructWithSuccess(DP005DistributionListDependencyAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new DP005DistributionListDependencyAnalyzer(model, context, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region DP005 Scenarios

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
        public void DP005RuleSkippedIfModelIsEmpty(DP005DistributionListDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given an source model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP005DistributionListDependencyAnalyzer(model, context, logger));

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
        /// Scenario tests that the rule succeeds in matching references between applications.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DP005ResolveDistributionListDependenciesWithSuccess(DP005DistributionListDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
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
                .x(() => analyzer = new DP005DistributionListDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resources should have the expected relationships created"
                .x(() =>
                {
                    // Test the applications
                    var applications = model.GetSourceModel<ParsedBizTalkApplicationGroup>().Applications;
                    applications.Should().NotBeNull().And.HaveCount(3);

                    var distributionList = model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceDistributionList).Single();
                    var sendPort = model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceSendPort).Single();

                    // Check that the DL has a relationship to the send port
                    distributionList.ResourceRelationships.Count.Should().Be(1);
                    distributionList.ResourceRelationships[0].ResourceRefId.Should().Be(sendPort.RefId);
                    distributionList.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.CallsTo);

                    // Check that the SP has a relationship to the DL
                    sendPort.ResourceRelationships.Count.Should().Be(1);
                    sendPort.ResourceRelationships[0].ResourceRefId.Should().Be(distributionList.RefId);
                    sendPort.ResourceRelationships[0].ResourceRelationshipType.Should().Be(ResourceRelationshipType.CalledBy);
                });
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
        public void DP005ResolveDistributionListDependenciesNullSourceObject(DP005DistributionListDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given a source model"
                .x(() =>
                {
                    model = TestHelper.CreateDefaultModelForAnalyzing();
                    // Null the source object to make the resource unfindable
                    model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceDistributionList).Single().SourceObject = null;
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP005DistributionListDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resources should have the expected relationships created"
                .x(() =>
                {
                    var distributionList = model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceDistributionList).Single();
                    var sendPort = model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceSendPort).Single();

                    // Check that the DL has the correct info
                    distributionList.ResourceRelationships.Count.Should().Be(0);
                    distributionList.ReportMessages.Count.Should().Be(1);
                    distributionList.ReportMessages[0].Severity.Should().Be(MessageSeverity.Error);

                    // Check that the SP has the correct info
                    sendPort.ResourceRelationships.Count.Should().Be(0);
                    sendPort.ReportMessages.Count.Should().Be(0);
                });
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
        public void DP005ResolveDistributionListDependenciesInvalidSourceObject(DP005DistributionListDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given a source model"
                .x(() =>
                {
                    model = TestHelper.CreateDefaultModelForAnalyzing();
                    model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceDistributionList).Single().SourceObject = "A different Type";
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP005DistributionListDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resources should have the expected relationships created"
                .x(() =>
                {
                    var distributionList = model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceDistributionList).Single();
                    var sendPort = model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceSendPort).Single();

                    // Check that the DL has the correct info
                    distributionList.ResourceRelationships.Count.Should().Be(0);
                    distributionList.ReportMessages.Count.Should().Be(1);
                    distributionList.ReportMessages[0].Severity.Should().Be(MessageSeverity.Error);

                    // Check that the SP has the correct info
                    sendPort.ResourceRelationships.Count.Should().Be(0);
                    sendPort.ReportMessages.Count.Should().Be(0);
                });
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
        public void DP005ResolveDistributionListDependenciesSendPortNotFound(DP005DistributionListDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given a source model"
                .x(() =>
                {
                    model = TestHelper.CreateDefaultModelForAnalyzing();
                    var sendPort = model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceSendPort).Single();
                    sendPort.Name = "This name will not match";
                    ((SendPort)sendPort.SourceObject).Name = sendPort.Name;
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP005DistributionListDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resources should have the expected relationships created"
                .x(() =>
                {
                    var distributionList = model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceDistributionList).Single();
                    var sendPort = model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceSendPort).Single();

                    // Check that the DL has the correct info
                    distributionList.ResourceRelationships.Count.Should().Be(0);
                    distributionList.ReportMessages.Count.Should().Be(1);
                    distributionList.ReportMessages[0].Severity.Should().Be(MessageSeverity.Warning);

                    // Check that the SP has the correct info
                    sendPort.ResourceRelationships.Count.Should().Be(0);
                    sendPort.ReportMessages.Count.Should().Be(0);
                });
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
        public void DP005ResolveDistributionListDependenciesMultipleMatches(DP005DistributionListDependencyAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given a source model"
                .x(() =>
                {
                    model = TestHelper.CreateDefaultModelForAnalyzing();
                    var sendPort = model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceSendPort).Single();
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[7].Resources.Add(new ResourceItem()
                    {
                        Type = ModelConstants.ResourceSendPort,
                        ParentRefId = sendPort.ParentRefId,
                        SourceObject = sendPort.SourceObject,
                        Name = sendPort.Name
                    });
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new DP005DistributionListDependencyAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().HaveCount(0));

            "And resources should have the expected relationships created"
                .x(() =>
                {
                    var distributionList = model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceDistributionList).Single();
                    var sendPort = model.FindAllResources().Where(r => r.Type == ModelConstants.ResourceSendPort).First();

                    // Check that the DL has the correct info
                    distributionList.ResourceRelationships.Count.Should().Be(0);
                    distributionList.ReportMessages.Count.Should().Be(1);
                    distributionList.ReportMessages[0].Severity.Should().Be(MessageSeverity.Warning);
                    distributionList.ReportMessages[0].Message.Should().Contain("Dependency cannot be accurately resolved.");

                    // Check that the SP has the correct info
                    sendPort.ResourceRelationships.Count.Should().Be(0);
                    sendPort.ReportMessages.Count.Should().Be(0);
                });
        }

        #endregion
    }
}
