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
using Microsoft.AzureIntegrationMigration.ApplicationModel.Renderer;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;
using YamlDotNet.RepresentationModel;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert.Tests
{
    /// <summary>
    /// Tests for the <see cref="TemplateRendererConverter"/> class.
    /// </summary>
    public class TemplateRendererConverterFeature
    {
        /// <summary>
        /// Defines a mocked logger.
        /// </summary>
        private Mock<ILogger> _mockLogger;

        /// <summary>
        /// Defines a file repository.
        /// </summary>
        private Mock<IFileRepository> _mockFileRepository;

        /// <summary>
        /// Defines a template repository.
        /// </summary>
        private Mock<ITemplateRepository> _mockRepository;

        /// <summary>
        /// Defines a template renderer.
        /// </summary>
        private Mock<ITemplateRenderer> _mockRenderer;

        #region Before Each Scenario

        /// <summary>
        /// Sets up state before each scenario.
        /// </summary>
        [Background]
        public void Setup()
        {
            "Given a mock logger"
                .x(() => _mockLogger = new Mock<ILogger>());

            "Given a mock file repository"
                .x(() =>
                {
                    _mockFileRepository = new Mock<IFileRepository>();
                    _mockFileRepository.Setup(r => r.DoesDirectoryExist(It.IsAny<string>())).Returns(true);
                    _mockFileRepository.Setup(r => r.DoesFileExist(It.IsAny<string>())).Returns(true);
                });

            "Given a mock repository"
                .x(() => _mockRepository = new Mock<ITemplateRepository>());

            "Given a mock renderer"
                .x(() => _mockRenderer = new Mock<ITemplateRenderer>());
        }

        #endregion

        #region Constructor Scenarios

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null file repository is passed.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="renderer">The renderer.</param>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullFileRepository(TemplateRendererConverter converter, IFileRepository fileRepository, ITemplateRepository repository, ITemplateRenderer renderer, IApplicationModel model, ILogger logger, MigrationContext context, Exception e)
        {
            "Given a converter"
                .x(() => converter.Should().BeNull());

            "And a null file repository"
                .x(() => fileRepository.Should().BeNull());

            "And a repository"
                .x(() => repository = _mockRepository.Object);

            "And a renderer"
                .x(() => renderer = _mockRenderer.Object);

            "And a model"
                .x(() => model = TestHelper.CreateDefaultModelForConverting());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null file repository"
                .x(() => e = Record.Exception(() => new TemplateRendererConverter(fileRepository, repository, renderer, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("fileRepository"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null repository is passed.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="renderer">The renderer.</param>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullRepository(TemplateRendererConverter converter, IFileRepository fileRepository, ITemplateRepository repository, ITemplateRenderer renderer, IApplicationModel model, ILogger logger, MigrationContext context, Exception e)
        {
            "Given a converter"
                .x(() => converter.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a null repository"
                .x(() => repository.Should().BeNull());

            "And a renderer"
                .x(() => renderer = _mockRenderer.Object);

            "And a model"
                .x(() => model = TestHelper.CreateDefaultModelForConverting());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null repository"
                .x(() => e = Record.Exception(() => new TemplateRendererConverter(fileRepository, repository, renderer, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("repository"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null renderer is passed.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="renderer">The renderer.</param>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullRenderer(TemplateRendererConverter converter, IFileRepository fileRepository, ITemplateRepository repository, ITemplateRenderer renderer, IApplicationModel model, ILogger logger, MigrationContext context, Exception e)
        {
            "Given a converter"
                .x(() => converter.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a repository"
                .x(() => repository = _mockRepository.Object);

            "And a null renderer"
                .x(() => renderer.Should().BeNull());

            "And a model"
                .x(() => model = TestHelper.CreateDefaultModelForConverting());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null renderer"
                .x(() => e = Record.Exception(() => new TemplateRendererConverter(fileRepository, repository, renderer, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("renderer"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null model is passed.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="renderer">The renderer.</param>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullModel(TemplateRendererConverter converter, IFileRepository fileRepository, ITemplateRepository repository, ITemplateRenderer renderer, IApplicationModel model, ILogger logger, MigrationContext context, Exception e)
        {
            "Given a converter"
                .x(() => converter.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a repository"
                .x(() => repository = _mockRepository.Object);

            "And a renderer"
                .x(() => renderer = _mockRenderer.Object);

            "And a null model"
                .x(() => model.Should().BeNull());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null model"
                .x(() => e = Record.Exception(() => new TemplateRendererConverter(fileRepository, repository, renderer, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null context is passed.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="renderer">The renderer.</param>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullContext(TemplateRendererConverter converter, IFileRepository fileRepository, ITemplateRepository repository, ITemplateRenderer renderer, IApplicationModel model, ILogger logger, MigrationContext context, Exception e)
        {
            "Given a converter"
                .x(() => converter.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a repository"
                .x(() => repository = _mockRepository.Object);

            "And a renderer"
                .x(() => renderer = _mockRenderer.Object);

            "And a model"
                .x(() => model = TestHelper.CreateDefaultModelForConverting());

            "And a null context"
                .x(() => context.Should().BeNull());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing with a null context"
                .x(() => e = Record.Exception(() => new TemplateRendererConverter(fileRepository, repository, renderer, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("context"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null logger is passed.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="renderer">The renderer.</param>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullLogger(TemplateRendererConverter converter, IFileRepository fileRepository, ITemplateRepository repository, ITemplateRenderer renderer, IApplicationModel model, ILogger logger, MigrationContext context, Exception e)
        {
            "Given a converter"
                .x(() => converter.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a repository"
                .x(() => repository = _mockRepository.Object);

            "And a renderer"
                .x(() => renderer = _mockRenderer.Object);

            "And a model"
                .x(() => model = TestHelper.CreateDefaultModelForConverting());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a null logger"
                .x(() => logger.Should().BeNull());

            "When constructing with a null logger"
                .x(() => e = Record.Exception(() => new TemplateRendererConverter(fileRepository, repository, renderer, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        #endregion

        #region ConvertAsync Scenarios

        /// <summary>
        /// Scenario tests that the convert succeeds.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConvertAsyncWithSuccess(TemplateRendererConverter converter, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a model"
                .x(() => model = TestHelper.CreateDefaultModelForConverting());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a converter"
                .x(() => converter = new TemplateRendererConverter(_mockFileRepository.Object, _mockRepository.Object, _mockRenderer.Object, model, context, _mockLogger.Object));

            "When converting"
                .x(async () => e = await Record.ExceptionAsync(async () => await converter.ConvertAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then the convert should succeed"
                .x(() => e.Should().BeNull());

            "And the resource template files should have been generated using the renderer and repository"
                .x(() =>
                {
                    _mockRepository.Verify(r => r.LoadTemplateAsync(
                        It.IsAny<string>()), Times.Once);

                    _mockRenderer.Verify(r => r.RenderTemplateAsync(
                        It.IsAny<string>(),
                        It.Is<AzureIntegrationServicesModel>(m => m == model),
                        It.IsAny<MessagingObject>(),
                        It.IsAny<TargetResourceTemplate>()), Times.Once);

                    _mockRepository.Verify(r => r.SaveTemplateAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>()), Times.Once);

                    _mockFileRepository.Verify(r => r.CopyFile(
                        It.IsAny<string>(),
                        It.IsAny<string>()), Times.Once);
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
