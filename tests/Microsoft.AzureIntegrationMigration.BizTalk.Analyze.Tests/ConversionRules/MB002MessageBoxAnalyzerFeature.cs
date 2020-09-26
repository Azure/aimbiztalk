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
    /// Tests for the <see cref="MB002MessageBoxAnalyzer"/> class.
    /// </summary>
    public class MB002MessageBoxAnalyzerFeature
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
        public void ConstructWithNullModel(MB002MessageBoxAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new MB002MessageBoxAnalyzer(model, context, logger)));

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
        public void ConstructWithNullContext(MB002MessageBoxAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new MB002MessageBoxAnalyzer(model, context, logger)));

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
        public void ConstructWithNullLogger(MB002MessageBoxAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new MB002MessageBoxAnalyzer(model, context, logger)));

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
        public void ConstructWithSuccess(MB002MessageBoxAnalyzer analyzer, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new MB002MessageBoxAnalyzer(model, context, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region MB002 Tests

        /// <summary>
        /// Scenario tests when an attempt is made to create the message box and there is no message bus.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void CreateMessageBoxFailsWithNoMessageBus(MB002MessageBoxAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());
            
            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new MB002MessageBoxAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And and the context contains an error"
                .x(() =>
                {
                    context.Errors.Should().NotBeNullOrEmpty();
                    context.Errors.Should().HaveCount(1);
                    context.Errors[0].Message.Should().NotBeNullOrEmpty();
                    context.Errors[0].Message.Should().Contain("MessageBus:SystemApplication");
                });
        }

        /// <summary>
        /// Scenario tests when an attempt is made to create the message box and the system application is missing.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void CreateMessageBoxFailsWithNoSystemApplication(MB002MessageBoxAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And a model with a message bus"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();

                    model.MigrationTarget.MessageBus = new ApplicationModel.Target.MessageBus
                    {
                        Name = "messageBusName",
                        Key = "MessageBus"
                    };
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new MB002MessageBoxAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And and the context contains an error"
                 .x(() =>
                 {
                     context.Errors.Should().NotBeNullOrEmpty();
                     context.Errors.Should().HaveCount(1);
                     context.Errors[0].Message.Should().NotBeNullOrEmpty();
                     context.Errors[0].Message.Should().Contain("MessageBus:SystemApplication");
                 });
        }

        /// <summary>
        /// Scenario tests that the message box is created on the migration target.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void CreateMessageBoxWithSuccess(MB002MessageBoxAnalyzer analyzer, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            var messageBusKey = "MessageBus";
            var systemApplicationKey = "SystemApplication";

            "Given an analyzer"
                .x(() => analyzer.Should().BeNull());

            "And a model with a message bus and system application"
                .x(() =>
                { 
                    model = new AzureIntegrationServicesModel();

                    model.MigrationTarget.MessageBus = new ApplicationModel.Target.MessageBus
                    {
                        Name = "messageBusName",
                        Key = messageBusKey
                    };

                    model.MigrationTarget.MessageBus.Applications.Add(
                        new ApplicationModel.Target.Application
                        {
                            Name = "systemApplicationName",
                            Key = $"{messageBusKey}:{systemApplicationKey}"
                        }); 
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an analyzer"
                .x(() => analyzer = new MB002MessageBoxAnalyzer(model, context, logger));

            "When analyzing"
                .x(async () => e = await Record.ExceptionAsync(async () => await analyzer.AnalyzeAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then there should be no exception"
                .x(() => e.Should().BeNull());

            "And the message box will have been created"
                .x(() =>
                {
                    model.Should().NotBeNull();
                    model.MigrationTarget.Should().NotBeNull();
                    model.MigrationTarget.MessageBus.Should().NotBeNull();
                    model.MigrationTarget.MessageBus.Applications.Should().NotBeNullOrEmpty();
                    model.MigrationTarget.MessageBus.Applications.Should().HaveCount(1);
                    model.MigrationTarget.MessageBus.Applications[0].Channels.Should().NotBeNullOrEmpty();
                    model.MigrationTarget.MessageBus.Applications[0].Channels.Should().HaveCount(4);

                    var channel = model.MigrationTarget.MessageBus.Applications[0].Channels[0];

                    channel.Should().BeOfType(typeof(TopicChannel));

                    var topicChannel = (TopicChannel)channel;

                    topicChannel.Key.Should().Be($"{messageBusKey}:{systemApplicationKey}:MessageBox");
                    topicChannel.ResourceMapKey.Should().Be("messageBox");
                    topicChannel.Description.Should().NotBeNullOrEmpty();
                    topicChannel.Name.Should().Be("Message Box");
                    topicChannel.Rating.Should().Be(ApplicationModel.Report.ConversionRating.FullConversion);

                    channel = model.MigrationTarget.MessageBus.Applications[0].Channels[1];

                    channel.Should().BeOfType(typeof(TopicChannel));

                    var topicResponseChannel = (TopicChannel)channel;

                    topicResponseChannel.Key.Should().Be($"{messageBusKey}:{systemApplicationKey}:{ModelConstants.MessageBoxResponseLeafKey}");
                    topicResponseChannel.ResourceMapKey.Should().Be("messageBoxResponse");
                    topicResponseChannel.Description.Should().NotBeNullOrEmpty();
                    topicResponseChannel.Name.Should().Be("Message Box Response");
                    topicResponseChannel.Rating.Should().Be(ApplicationModel.Report.ConversionRating.FullConversion);
                    topicResponseChannel.ChannelType.Should().Be(ChannelType.PublishSubscribe);

                    channel = model.MigrationTarget.MessageBus.Applications[0].Channels[2];

                    channel.Should().BeOfType(typeof(TopicChannel));

                    var suspendTopicChannel = (TopicChannel)channel;

                    suspendTopicChannel.Key.Should().Be($"{messageBusKey}:{systemApplicationKey}:{ModelConstants.SuspendQueueLeafKey}");
                    suspendTopicChannel.ResourceMapKey.Should().Be("suspendQueue");
                    suspendTopicChannel.Description.Should().NotBeNullOrEmpty();
                    suspendTopicChannel.Name.Should().Be("Suspend Queue");
                    suspendTopicChannel.Rating.Should().Be(ApplicationModel.Report.ConversionRating.FullConversion);
                    suspendTopicChannel.ChannelType.Should().Be(ChannelType.InvalidMessage);

                    channel = model.MigrationTarget.MessageBus.Applications[0].Channels[3];

                    channel.Should().BeOfType(typeof(CorrelatingQueueChannel));

                    var interchangeQueueChannel = (CorrelatingQueueChannel)channel;

                    interchangeQueueChannel.Key.Should().Be($"{messageBusKey}:{systemApplicationKey}:{ModelConstants.InterchangeQueueLeafKey}");
                    interchangeQueueChannel.ResourceMapKey.Should().BeNullOrEmpty();
                    interchangeQueueChannel.Description.Should().NotBeNullOrEmpty();
                    interchangeQueueChannel.Name.Should().Be("Interchange Queue");
                    interchangeQueueChannel.Rating.Should().Be(ApplicationModel.Report.ConversionRating.NoAutomaticConversion);
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
