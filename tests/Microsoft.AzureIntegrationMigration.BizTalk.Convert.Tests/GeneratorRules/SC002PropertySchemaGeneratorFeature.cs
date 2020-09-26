using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Messages;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.GeneratorRules;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Tests;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert.Tests.GeneratorRules
{
    /// <summary>
    /// Tests for the <see cref="SC002PropertySchemaGenerator"/> class.
    /// </summary>
    public class SC002PropertySchemaGeneratorFeature
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
        public void ConstructWithNullFileRepository(SC002PropertySchemaGenerator generator, IFileRepository fileRepository, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new SC002PropertySchemaGenerator(fileRepository, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("fileRepository"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null model is passed.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullModel(SC002PropertySchemaGenerator generator, IFileRepository fileRepository, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new SC002PropertySchemaGenerator(fileRepository, model, context, logger)));

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
        public void ConstructWithNullContext(SC002PropertySchemaGenerator generator, IFileRepository fileRepository, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new SC002PropertySchemaGenerator(fileRepository, model, context, logger)));

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
        public void ConstructWithNullLogger(SC002PropertySchemaGenerator generator, IFileRepository fileRepository, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new SC002PropertySchemaGenerator(fileRepository, model, context, logger)));

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
        public void ConstructWithSuccess(SC002PropertySchemaGenerator generator, IFileRepository fileRepository, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new SC002PropertySchemaGenerator(fileRepository, model, context, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region SC002 Tests

        /// <summary>
        /// Scenario tests if the message bus is missing the convertion is skipped.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConvertSkippedWithMissingMessageBus(SC002PropertySchemaGenerator generator, IFileRepository fileRepository, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            "Given a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a generator"
                .x(() => generator = new SC002PropertySchemaGenerator(fileRepository, model, context, logger));

            "When converting"
                .x(async () => e = await Record.ExceptionAsync(async () => await generator.ConvertAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be no context error"
                .x(() =>
                {
                    context.Errors.Should().BeNullOrEmpty();
                });
        }

        /// <summary>
        /// Scenario tests if the config is missing an error is logged.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConvertWithMissingAppConfig(SC002PropertySchemaGenerator generator, IFileRepository fileRepository, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            var messageName = "testMessageName";

            "Given a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a message with no appConfig"
                .x(() =>
                {
                    var documentMessage = new DocumentMessage
                    {
                        Name = messageName
                    };

                    documentMessage.RoutingProperties.Add("routingPropertyKey1", "routingPropertyValue1");

                    var application = new Application();
                    application.Messages.Add(documentMessage);

                    model.MigrationTarget.MessageBus = new MessageBus();
                    model.MigrationTarget.MessageBus.Applications.Add(application);
                });

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a generator"
                .x(() => generator = new SC002PropertySchemaGenerator(fileRepository, model, context, logger));

            "When converting"
                .x(async () => e = await Record.ExceptionAsync(async () => await generator.ConvertAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be a context error"
                .x(() =>
                {
                    context.Errors.Should().NotBeEmpty();
                    context.Errors.Should().HaveCount(1);
                    context.Errors.First().Message.Should().Contain(ModelConstants.ResourceTypeRoutingProperties);
                    context.Errors.First().Message.Should().Contain(messageName);
                });
        }

        /// <summary>
        /// Scenario tests a successful conversion.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConvertWithSuccess(SC002PropertySchemaGenerator generator, IFileRepository fileRepository, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            var messageName = "testMessageName";
            var conversionFolder = @"c:\conversionFolder";
            var templateOutputPath = "templateOutputPath";
            var messageSchemaName = "testMessageSchemaName";

            var generatedFileName = string.Empty;
            JObject generatedJson = null;

            "Given a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a message with no appConfig"
                .x(() =>
                {
                    var documentMessage = new DocumentMessage
                    {
                        Name = messageName,
                        MessageSchema = new MessageSchema
                        {
                            Name = messageSchemaName
                        }
                    };

                    documentMessage.RoutingProperties.Add("routingPropertyKey1", "routingPropertyValue1");

                    documentMessage.Resources.Add(new TargetResourceTemplate
                    {
                        OutputPath = templateOutputPath,
                        ResourceType = ModelConstants.ResourceTypeRoutingProperties
                    });

                    var application = new Application();
                    application.Messages.Add(documentMessage);

                    model.MigrationTarget.MessageBus = new MessageBus();
                    model.MigrationTarget.MessageBus.Applications.Add(application);
                });

            "And a file repository"
                .x(() =>
                {

                    _mockFileRepository.Setup(f => f.WriteJsonFile(
                        It.IsAny<string>(),
                        It.IsAny<JObject>()
                        ))
                    .Callback<string, JObject>(
                        (p1, p2) =>
                        {
                            generatedFileName = p1;
                            generatedJson = p2;
                        });

                    fileRepository = _mockFileRepository.Object;
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() =>
                {
                    context = new MigrationContext
                    {
                        ConversionFolder = conversionFolder
                    };
                });

            "And a generator"
                .x(() => generator = new SC002PropertySchemaGenerator(fileRepository, model, context, logger));

            "When converting"
                .x(async () => e = await Record.ExceptionAsync(async () => await generator.ConvertAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().BeNullOrEmpty());

            "And the file should be generated"
                .x(() =>
                {                    
                    _mockFileRepository.Verify(f => f.WriteJsonFile(
                        It.IsAny<string>(),
                        It.IsAny<JObject>()), Times.Once);

                    var expectedPath = Path.Combine(conversionFolder, templateOutputPath);

                    generatedFileName.Should().StartWith(expectedPath);
                    generatedJson.Should().NotBeNull();                    
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
