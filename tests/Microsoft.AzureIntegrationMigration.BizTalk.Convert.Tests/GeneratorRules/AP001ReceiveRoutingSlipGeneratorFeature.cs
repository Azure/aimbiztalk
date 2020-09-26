using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.GeneratorRules;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Tests;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert.Tests.GeneratorRules
{
    /// <summary>
    /// Tests for the <see cref="AP001ReceiveRoutingSlipGenerator"/> class.
    /// </summary>
    public class AP001ReceiveRoutingSlipGeneratorFeature
    {
        /// <summary>
        /// Defines a mocked logger.
        /// </summary>
        private Mock<ILogger> _mockLogger;

        /// <summary>
        /// Defines a file repository.
        /// </summary>
        private Mock<IFileRepository> _mockFileRepository;

        #region Before Each Scenario

        /// <summary>
        /// Sets up state before each scenario.
        /// </summary>
        [Background]
        public void Setup()
        {
            "Given a new mock logger"
                .x(() => _mockLogger = new Mock<ILogger>());

            "Given a mock file repository"
              .x(() =>
              {
                  _mockFileRepository = new Mock<IFileRepository>();
              });
        }

        #endregion

        #region Constructor Scenarios

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null file repository is passed.
        /// </summary>
        /// <param name="generator">The analyzer.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="routeWalker">The scenario router walker.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullFileRepository(AP001ReceiveRoutingSlipGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given an generator"
                .x(() => generator.Should().BeNull());

            "And a null file repository"
              .x(() => fileRepository.Should().BeNull());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a route walker"
                .x(() => routeWalker = new ScenarioRouteWalker(context, logger));

            "When constructing with a null file repository"
                .x(() => e = Record.Exception(() => new AP001ReceiveRoutingSlipGenerator(fileRepository, routeWalker, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("fileRepository"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null model is passed.
        /// </summary>
        /// <param name="generator">The analyzer.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="routeWalker">The scenario router walker.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullModel(AP001ReceiveRoutingSlipGenerator generator, IFileRepository fileRepository, ILogger logger, IScenarioRouteWalker routeWalker, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given an generator"
                .x(() => generator.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And null model"
                .x(() => model.Should().BeNull());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a route walker"
                .x(() => routeWalker = new ScenarioRouteWalker(context, logger));

            "When constructing with a null model"
                .x(() => e = Record.Exception(() => new AP001ReceiveRoutingSlipGenerator(fileRepository, routeWalker, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null route walker is passed.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="routeWalker">The scenario router walker.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullRouteWalker(AP001ReceiveRoutingSlipGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given an generator"
                .x(() => generator.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a null route walker"
                .x(() => routeWalker.Should().BeNull());

            "When constructing with a null route walker"
                .x(() => e = Record.Exception(() => new AP001ReceiveRoutingSlipGenerator(fileRepository, routeWalker, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("routeWalker"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null context is passed.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="routeWalker">The scenario router walker.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullContext(AP001ReceiveRoutingSlipGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given an generator"
                .x(() => generator.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And null context"
                .x(() => context.Should().BeNull());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a route walker"
                .x(() => routeWalker = new ScenarioRouteWalker(TestHelper.BuildContext(), _mockLogger.Object));

            "When constructing with a null context"
                .x(() => e = Record.Exception(() => new AP001ReceiveRoutingSlipGenerator(fileRepository, routeWalker, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("context"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null logger is passed.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="routeWalker">The scenario route walker.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullLogger(AP001ReceiveRoutingSlipGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given an generator"
                .x(() => generator.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And null logger"
                .x(() => logger.Should().BeNull());

            "And a route walker"
                .x(() => routeWalker = new ScenarioRouteWalker(context, new Mock<ILogger>().Object));

            "When constructing with a null logger"
                .x(() => e = Record.Exception(() => new AP001ReceiveRoutingSlipGenerator(fileRepository, routeWalker, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction works when params ok.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="routeWalker">The scenario route walker.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithSuccess(AP001ReceiveRoutingSlipGenerator generator, IScenarioRouteWalker routeWalker, IFileRepository fileRepository, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given an generator"
                .x(() => generator.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a route walker"
            .x(() => routeWalker = new ScenarioRouteWalker(context, logger));

            "When constructing"
                .x(() => e = Record.Exception(() => new AP001ReceiveRoutingSlipGenerator(fileRepository, routeWalker, model, context, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region AP001 Tests

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
