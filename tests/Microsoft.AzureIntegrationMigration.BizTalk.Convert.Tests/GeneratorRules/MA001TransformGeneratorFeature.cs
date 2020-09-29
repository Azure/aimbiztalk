// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
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
namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Tests.GeneratorRules
{
    /// <summary>
    /// Tests for the <see cref="MA001TransformGenerator"/> class.
    /// </summary>
    public class MA001TransformGeneratorFeature
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
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullFileRepository(MA001TransformGenerator generator, IFileRepository fileRepository, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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

            "When constructing with a null file repository"
                .x(() => e = Record.Exception(() => new MA001TransformGenerator(fileRepository, model , context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("fileRepository"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null model is passed.
        /// </summary>
        /// <param name="generator">The analyzer.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullModel(MA001TransformGenerator generator, IFileRepository fileRepository, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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

            "When constructing with a null model"
                .x(() => e = Record.Exception(() => new MA001TransformGenerator(fileRepository, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null context is passed.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullContext(MA001TransformGenerator generator, IFileRepository fileRepository, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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

            "When constructing with a null context"
                .x(() => e = Record.Exception(() => new MA001TransformGenerator(fileRepository, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("context"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null logger is passed.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullLogger(MA001TransformGenerator generator, IFileRepository fileRepository, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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

            "When constructing with a null logger"
                .x(() => e = Record.Exception(() => new MA001TransformGenerator(fileRepository, model,  context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction works when params ok.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithSuccess(MA001TransformGenerator generator, IFileRepository fileRepository, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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

            "When constructing"
                .x(() => e = Record.Exception(() => new MA001TransformGenerator(fileRepository, model, context, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region MA001 Tests

        /// <summary>
        /// Scenario tests the Xml schema is generated.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GenerateMapper(MA001TransformGenerator generator, IFileRepository fileRepository, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given an generator"
                .x(() => generator.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a model"
                .x(() => model = TestHelper.CreateDefaultModelForConverting());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an generator"
                .x(() => generator = new MA001TransformGenerator(fileRepository, model, context, logger));

            "When generating"
                .x(async () => e = await Record.ExceptionAsync(async () => await generator.ConvertAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());

            "And 1 schema has been created"
                .x(() =>
                {
                    model.Should().NotBeNull();

                    _mockFileRepository.Verify(r => r.WriteXmlFile(
                            It.IsAny<string>(), 
                            It.IsAny<string>()), 
                            Times.Once);
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
