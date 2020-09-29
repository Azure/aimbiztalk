// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Generator;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.DependencyRules;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;
using YamlDotNet.RepresentationModel;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Tests
{
    /// <summary>
    /// Tests for the <see cref="ResourceGeneratorAnalyzer"/> class.
    /// </summary>
    public class ResourceGeneratorAnalyzerFeature
    {
        /// <summary>
        /// Defines a mocked logger.
        /// </summary>
        private Mock<ILogger> _mockLogger;

        /// <summary>
        /// Defines a configuration repository.
        /// </summary>
        private Mock<IConfigurationRepository> _mockRepository;

        /// <summary>
        /// Defines a resource generator.
        /// </summary>
        private Mock<IResourceGenerator> _mockGenerator;

        #region Before Each Scenario

        /// <summary>
        /// Sets up state before each scenario.
        /// </summary>
        [Background]
        public void Setup()
        {
            "Given a mock logger"
                .x(() => _mockLogger = new Mock<ILogger>());

            "Given a mock repository"
                .x(() => _mockRepository = new Mock<IConfigurationRepository>());

            "Given a mock generator"
                .x(() => _mockGenerator = new Mock<IResourceGenerator>());
        }

        #endregion

        #region Constructor Scenarios

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null repository is passed.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullRepository(ResourceGeneratorAnalyzer analyzer, IConfigurationRepository repository, IResourceGenerator generator, IApplicationModel model, ILogger logger, MigrationContext context, Exception e)
        {
            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And a null repository"
                .x(() => repository.Should().BeNull());

            "And a generator"
                .x(() => generator = _mockGenerator.Object);

            "And a model"
                .x(() => model = TestHelper.CreateDefaultModelForAnalyzing());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null repository"
                .x(() => e = Record.Exception(() => new ResourceGeneratorAnalyzer(repository, generator, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("repository"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null generator is passed.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullGenerator(ResourceGeneratorAnalyzer analyzer, IConfigurationRepository repository, IResourceGenerator generator, IApplicationModel model, ILogger logger, MigrationContext context, Exception e)
        {
            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And a repository"
                .x(() => repository = _mockRepository.Object);

            "And a null generator"
                .x(() => generator.Should().BeNull());

            "And a model"
                .x(() => model = TestHelper.CreateDefaultModelForAnalyzing());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null generator"
                .x(() => e = Record.Exception(() => new ResourceGeneratorAnalyzer(repository, generator, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("generator"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null model is passed.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullModel(ResourceGeneratorAnalyzer analyzer, IConfigurationRepository repository, IResourceGenerator generator, IApplicationModel model, ILogger logger, MigrationContext context, Exception e)
        {
            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And a repository"
                .x(() => repository = _mockRepository.Object);

            "And a generator"
                .x(() => generator = _mockGenerator.Object);

            "And null model"
                .x(() => model.Should().BeNull());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null model"
                .x(() => e = Record.Exception(() => new ResourceGeneratorAnalyzer(repository, generator, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null context is passed.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullContext(ResourceGeneratorAnalyzer analyzer, IConfigurationRepository repository, IResourceGenerator generator, IApplicationModel model, ILogger logger, MigrationContext context, Exception e)
        {
            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And a repository"
                .x(() => repository = _mockRepository.Object);

            "And a generator"
                .x(() => generator = _mockGenerator.Object);

            "And a model"
                .x(() => model = TestHelper.CreateDefaultModelForAnalyzing());

            "And a null context"
                .x(() => context.Should().BeNull());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null context"
                .x(() => e = Record.Exception(() => new ResourceGeneratorAnalyzer(repository, generator, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("context"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null logger is passed.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullLogger(ResourceGeneratorAnalyzer analyzer, IConfigurationRepository repository, IResourceGenerator generator, IApplicationModel model, ILogger logger, MigrationContext context, Exception e)
        {
            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And a repository"
                .x(() => repository = _mockRepository.Object);

            "And a generator"
                .x(() => generator = _mockGenerator.Object);

            "And a model"
                .x(() => model = TestHelper.CreateDefaultModelForAnalyzing());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a null logger"
                .x(() => logger.Should().BeNull());

            "When constructing with a null logger"
                .x(() => e = Record.Exception(() => new ResourceGeneratorAnalyzer(repository, generator, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction succeeds.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithSuccess(ResourceGeneratorAnalyzer analyzer, IConfigurationRepository repository, IResourceGenerator generator, IApplicationModel model, ILogger logger, MigrationContext context, Exception e)
        {
            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And a repository"
                .x(() => repository = _mockRepository.Object);

            "And a generator"
                .x(() => generator = _mockGenerator.Object);

            "And a model"
                .x(() => model = TestHelper.CreateDefaultModelForAnalyzing());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing"
                .x(() => e = Record.Exception(() => new ResourceGeneratorAnalyzer(repository, generator, model, context, logger)));

            "Then the constructor should succeed"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region AnalyzeAsync Scenarios

        /// <summary>
        /// Scenario tests that the analyze succeeds.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void AnalyzeAsyncWithSuccess(ResourceGeneratorAnalyzer analyzer, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a model"
                .x(() => model = TestHelper.CreateDefaultModelForAnalyzing());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And an analyzer"
                .x(() => analyzer = new ResourceGeneratorAnalyzer(_mockRepository.Object, _mockGenerator.Object, model, context, _mockLogger.Object));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then the analyze should succeed"
                .x(() => e.Should().BeNull());

            "And the model target resource templates should have been generated using the generator"
                .x(() =>
                {
                    _mockGenerator.Verify(g => g.GenerateResourcesAsync(
                        It.Is<AzureIntegrationServicesModel>(m => m == model),
                        It.IsAny<IList<YamlStream>>(),
                        It.Is<CancellationToken>(t => t == CancellationToken.None)), Times.Once);
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
