// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Report.Tests
{
    /// <summary>
    /// Class containing tests for the <see cref="HtmlReportFormatter"/>.
    /// </summary>
    public class HtmlReportFormatterFeature
    {
        /// <summary>
        /// Defines a mocked logger.
        /// </summary>
        private Mock<ILogger> _mockLogger;

        /// <summary>
        /// Defines a mock report writer.
        /// </summary>
        private Mock<IReportWriter> _mockWriter;

        #region Before Each Scenario

        /// <summary>
        /// Sets up state before each scenario.
        /// </summary>
        [Background]
        public void Setup()
        {
            "Given a new mock logger"
                .x(() => _mockLogger = new Mock<ILogger>());

            "And a new mock writer"
                .x(() => _mockWriter = new Mock<IReportWriter>());
        }

        #endregion

        #region Constructor Tests

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null model is passed.
        /// </summary>
        /// <param name="reporter">The reporter.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="state">The run state.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullModel(HtmlReportFormatter reporter, IApplicationModel model, MigrationContext context, IRunState state, IReportWriter writer, ILogger logger, Exception e)
        {
            "Given an reporter"
                .x(() => reporter.Should().BeNull());

            "And a model"
                .x(() => model.Should().BeNull());

            "And run state"
                .x(() => state = TestHelper.BuildRunState(model));

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a writer"
                .x(() => writer = _mockWriter.Object);

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null model"
                .x(() => e = Record.Exception(() => new HtmlReportFormatter(model, context, state, writer, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null context is passed.
        /// </summary>
        /// <param name="reporter">The reporter.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="state">The run state.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullContext(HtmlReportFormatter reporter, IApplicationModel model, MigrationContext context, IRunState state, IReportWriter writer, ILogger logger, Exception e)
        {
            "Given an reporter"
                .x(() => reporter.Should().BeNull());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And run state"
                .x(() => state = TestHelper.BuildRunState(model));

            "And a context"
                .x(() => context.Should().BeNull());

            "And a writer"
                .x(() => writer = _mockWriter.Object);

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null context"
                .x(() => e = Record.Exception(() => new HtmlReportFormatter(model, context, state, writer, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("context"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null context is passed.
        /// </summary>
        /// <param name="reporter">The reporter.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="state">The run state.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullState(HtmlReportFormatter reporter, IApplicationModel model, MigrationContext context, IRunState state, IReportWriter writer, ILogger logger, Exception e)
        {
            "Given an reporter"
                .x(() => reporter.Should().BeNull());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And run state"
                .x(() => state.Should().BeNull());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a writer"
                .x(() => writer = _mockWriter.Object);

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null context"
                .x(() => e = Record.Exception(() => new HtmlReportFormatter(model, context, state, writer, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("state"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null writer is passed.
        /// </summary>
        /// <param name="reporter">The reporter.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="state">The run state.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullWriter(HtmlReportFormatter reporter, IApplicationModel model, MigrationContext context, IRunState state, IReportWriter writer, ILogger logger, Exception e)
        {
            "Given an reporter"
                .x(() => reporter.Should().BeNull());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And run state"
                .x(() => state = TestHelper.BuildRunState(model));

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a writer"
                .x(() => writer.Should().BeNull());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null writer"
                .x(() => e = Record.Exception(() => new HtmlReportFormatter(model, context, state, writer, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("writer"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null logger is passed.
        /// </summary>
        /// <param name="reporter">The reporter.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="state">The run state.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullLogger(HtmlReportFormatter reporter, IApplicationModel model, MigrationContext context, IRunState state, IReportWriter writer, ILogger logger, Exception e)
        {
            "Given an reporter"
                .x(() => reporter.Should().BeNull());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And run state"
                .x(() => state = TestHelper.BuildRunState(model));

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a writer"
                .x(() => writer = _mockWriter.Object);

            "And a logger"
                .x(() => logger.Should().BeNull());

            "When constructing with a null logger"
                .x(() => e = Record.Exception(() => new HtmlReportFormatter(model, context, state, writer, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction works when params ok.
        /// </summary>
        /// <param name="reporter">The reporter.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="state">The run state.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithSuccess(HtmlReportFormatter reporter, IApplicationModel model, MigrationContext context, IRunState state, IReportWriter writer, ILogger logger, Exception e)
        {
            "Given an reporter"
                .x(() => reporter.Should().BeNull());

            "And a model"
                .x(() => model = TestHelper.BuildModel());

            "And run state"
                .x(() => state = TestHelper.BuildRunState(model));

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a writer"
                .x(() => writer = _mockWriter.Object);

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing..."
                .x(() => e = Record.Exception(() => new HtmlReportFormatter(model, context, state, writer, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region Report Generation Tests

        /// <summary>
        /// Scenario tests that a report is generated happy-path.
        /// </summary>
        /// <param name="reporter">The reporter.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="state">The run state.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ReportHtmlGenerationHappyPath(HtmlReportFormatter reporter, AzureIntegrationServicesModel model, MigrationContext context, IRunState state, IReportWriter writer, ILogger logger, Exception e)
        {
            "Given a model"
                .x(() => model = TestHelper.BuildModel());

            "And run state"
                .x(() => state = TestHelper.BuildRunState(model));

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a writer"
                .x(() => writer = _mockWriter.Object);

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an reporter"
                .x(() => reporter = new HtmlReportFormatter(model, context, state, writer, logger));

            "When executing the report formatter"
                .x(() => e = Record.Exception(() => reporter.Report()));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "The report node should have a source application"
                .x(() =>
                {
                    _mockWriter.Invocations.Count.Should().Be(5);
                    _mockWriter.Invocations[0].Arguments.Count.Should().Be(2);
                    _mockWriter.Invocations[0].Arguments[0].Should().Be(context.ReportFilePath);
                    _mockWriter.Invocations[0].Arguments[1].Should().NotBeNull();
                    _mockWriter.Invocations[0].Arguments[1].Should().NotBeEquivalentTo(string.Empty);
                });
        }

        /// <summary>
        /// Scenario tests that a report is generated happy-path.
        /// </summary>
        /// <param name="reporter">The reporter.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="state">The run state.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ReportHtmlGenerationNoContainers(HtmlReportFormatter reporter, AzureIntegrationServicesModel model, MigrationContext context, IRunState state, IReportWriter writer, ILogger logger, Exception e)
        {
            "Given a model"
                .x(() => {
                    model = TestHelper.BuildModel();
                    model.MigrationSource.ResourceContainers.Clear(); //removes any resource containers.
                });

            "And run state"
                .x(() => state = TestHelper.BuildRunState(model));

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a writer"
                .x(() => writer = _mockWriter.Object);

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an reporter"
                .x(() => reporter = new HtmlReportFormatter(model, context, state, writer, logger));

            "When executing the report formatter"
                .x(() => e = Record.Exception(() => reporter.Report()));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "The report node should have a source application"
                .x(() =>
                {
                    _mockWriter.Invocations.Count.Should().Be(4);
                    _mockWriter.Invocations[0].Arguments.Count.Should().Be(2);
                    _mockWriter.Invocations[0].Arguments[0].Should().Be(context.ReportFilePath);
                    _mockWriter.Invocations[0].Arguments[1].Should().NotBeNull();
                    
                    var html = (string)_mockWriter.Invocations[0].Arguments[1];
                    html.Should().Contain("No Input BizTalk Applications");
                });
        }

        /// <summary>
        /// Scenario tests that a report is generated happy-path.
        /// </summary>
        /// <param name="reporter">The reporter.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="state">The run state.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ReportHtmlGenerationNoApplication(HtmlReportFormatter reporter, AzureIntegrationServicesModel model, MigrationContext context, IRunState state, IReportWriter writer, ILogger logger, Exception e)
        {
            "Given a model"
                .x(() => {
                    model = TestHelper.BuildModel();
                    model.MigrationSource.ResourceContainers.ToList().ForEach(rc =>
                    {
                        rc.ResourceContainers.Clear();
                        rc.ResourceDefinitions.Clear();
                    }); //removes any resource containers.
                });

            "And run state"
                .x(() => state = TestHelper.BuildRunState(model));

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a writer"
                .x(() => writer = _mockWriter.Object);

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an reporter"
                .x(() => reporter = new HtmlReportFormatter(model, context, state, writer, logger));

            "When executing the report formatter"
                .x(() => e = Record.Exception(() => reporter.Report()));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "The report node should have a source application"
                .x(() =>
                {
                    _mockWriter.Invocations.Count.Should().Be(4);
                    _mockWriter.Invocations[0].Arguments.Count.Should().Be(2);
                    _mockWriter.Invocations[0].Arguments[0].Should().Be(context.ReportFilePath);
                    _mockWriter.Invocations[0].Arguments[1].Should().NotBeNull();

                    var invocation = _mockLogger.Invocations.Where(i => i.Arguments[0].ToString() == "Warning").FirstOrDefault();
                    invocation.Should().NotBeNull();
                    invocation.Arguments[2].ToString().Should().Contain("no reportable BizTalk application");
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
