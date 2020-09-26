using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert;
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
    /// Tests for the <see cref="AP005SendRoutingPropertyGenerator"/> class.
    /// </summary>
    public class AP005SendRoutingPropertyGeneratorFeature
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
        public void ConstructWithNullFileRepository(AP005SendRoutingPropertyGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => routeWalker = new ScenarioRouteWalker(TestHelper.BuildContext(), _mockLogger.Object));

            "When constructing with a null file repository"
                .x(() => e = Record.Exception(() => new AP005SendRoutingPropertyGenerator(fileRepository, routeWalker, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("fileRepository"));
        }

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
        public void ConstructWithNullRouteWalker(AP005SendRoutingPropertyGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new AP005SendRoutingPropertyGenerator(fileRepository, routeWalker, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("routeWalker"));
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
        public void ConstructWithNullModel(AP005SendRoutingPropertyGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => routeWalker = new ScenarioRouteWalker(TestHelper.BuildContext(), _mockLogger.Object));

            "When constructing with a null model"
                .x(() => e = Record.Exception(() => new AP005SendRoutingPropertyGenerator(fileRepository, routeWalker, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
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
        public void ConstructWithNullContext(AP005SendRoutingPropertyGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => e = Record.Exception(() => new AP005SendRoutingPropertyGenerator(fileRepository, routeWalker, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("context"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null logger is passed.
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
        public void ConstructWithNullLogger(AP005SendRoutingPropertyGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => routeWalker = new ScenarioRouteWalker(TestHelper.BuildContext(), _mockLogger.Object));

            "When constructing with a null logger"
                .x(() => e = Record.Exception(() => new AP005SendRoutingPropertyGenerator(fileRepository, routeWalker, model, context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction works when params ok.
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
        public void ConstructWithSuccess(AP005SendRoutingPropertyGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
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
                .x(() => routeWalker = new ScenarioRouteWalker(TestHelper.BuildContext(), _mockLogger.Object));

            "When constructing"
                .x(() => e = Record.Exception(() => new AP005SendRoutingPropertyGenerator(fileRepository, routeWalker, model, context, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region AP005 Tests

        /// <summary>
        /// Scenario tests send routing properties are generated.
        /// </summary>
        /// <param name="generator">A generator.</param>
        /// <param name="fileRepository">A file repository.</param>
        /// <param name="routeWalker">A scenario route walker.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="model">The model.</param>
        /// <param name="application">An application.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GenerateWithSuccess(AP005SendRoutingPropertyGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, AzureIntegrationServicesModel model, Application application, MigrationContext context, Exception e)
        {
            var generatedFileName = string.Empty;
            JObject generatedJson = null;
            var resourcemapkey = "resourcemapkey";
            var scenarioName = "scenarioName";

            var routingProperties = new List<(string PropertyName, string PropertyValue)>
            {
                ("propertyOneName", "propertyOneValue"),
                ("propertyTwoName", "propertyTwoValue")
            };

            "Given an generator"
                .x(() => generator.Should().BeNull());

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

            "And an application"
                .x(() =>
                {
                    var activatingIntermediary = new MessageSubscriber
                    {
                        Activator = true,
                        ResourceMapKey = resourcemapkey,
                        Name = "ActivatingIntermediary"
                    };

                    var routingProperties1 = new Dictionary<string, object>();
                    var intermediaryRoutingProperties = routingProperties[0];
                    routingProperties1[intermediaryRoutingProperties.PropertyName] = intermediaryRoutingProperties.PropertyValue;

                    activatingIntermediary.Properties[ModelConstants.ScenarioName] = scenarioName;
                    activatingIntermediary.Properties[ModelConstants.RoutingProperties] = routingProperties1;

                    activatingIntermediary.Resources.Add(
                        new TargetResourceTemplate
                        {
                            ResourceType = ModelConstants.ResourceTypeRoutingProperties,
                            OutputPath = "outputpath"
                        });

                    var secondIntermediary = new GenericFilter
                    {
                        Name = "SecondIntermediary"
                    };

                    var endpoint = new AdapterEndpoint
                    {
                        Name = "Endpoint"
                    };

                    var routingProperties2 = new Dictionary<string, object>();
                    var endpointRoutingProperties = routingProperties[1];
                    routingProperties2[endpointRoutingProperties.PropertyName] = endpointRoutingProperties.PropertyValue;
                    endpoint.Properties[ModelConstants.RoutingProperties] = routingProperties2;

                    application = new Application();
                    application.Intermediaries.Add(activatingIntermediary);
                    application.Intermediaries.Add(secondIntermediary);
                    application.Endpoints.Add(endpoint);
                });

            "And a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    model.MigrationTarget.MessageBus = new MessageBus();
                    model.MigrationTarget.MessageBus.Applications.Add(application);
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a route walker"
                .x(() =>
                {
                    var route = new List<(MessagingObject RoutingObject, Channel InputChannel)>();

                    foreach (var intermediary in application.Intermediaries)
                    {
                        route.Add((intermediary, null));
                    }

                    foreach (var endpoint in application.Endpoints)
                    {
                        route.Add((endpoint, null));
                    }

                    var mockRouteWalker = new Mock<IScenarioRouteWalker>();

                    mockRouteWalker.Setup(w => w.WalkSendRoute(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<Intermediary>(),
                        It.IsAny<IEnumerable<Intermediary>>(),
                        It.IsAny<IEnumerable<Channel>>(),
                        It.IsAny<IEnumerable<Endpoint>>()
                        )).Returns(route);

                    routeWalker = mockRouteWalker.Object;
                });

            "And a generator"
               .x(() => generator = new AP005SendRoutingPropertyGenerator(fileRepository, routeWalker, model, context, logger));

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

                    generatedJson.Should().NotBeNull();
                    generatedJson["routingProperties"].Should().NotBeNull();
                    generatedJson["routingProperties"][0].Should().NotBeNull();
                    generatedJson["routingProperties"][0]["propertyName"].Should().NotBeNull();
                    var propertyOneName = (string)generatedJson.SelectToken("routingProperties[0].propertyName");
                    propertyOneName.Should().NotBeNullOrWhiteSpace();
                    propertyOneName.Should().Be(routingProperties[0].PropertyName);
                    var propertyOneValue = (string)generatedJson.SelectToken("routingProperties[0].propertyValue");
                    propertyOneValue.Should().NotBeNullOrWhiteSpace();
                    propertyOneValue.Should().Be(routingProperties[0].PropertyValue);
                    var propertyOneType = (string)generatedJson.SelectToken("routingProperties[0].propertyType");
                    propertyOneType.Should().NotBeNullOrWhiteSpace();
                    propertyOneType.Should().Be("property");

                    generatedJson["routingProperties"][1].Should().NotBeNull();
                    generatedJson["routingProperties"][1]["propertyName"].Should().NotBeNull();
                    var propertyTwoName = (string)generatedJson.SelectToken("routingProperties[1].propertyName");
                    propertyTwoName.Should().NotBeNullOrWhiteSpace();
                    propertyTwoName.Should().Be(routingProperties[1].PropertyName);
                    var propertyTwoValue = (string)generatedJson.SelectToken("routingProperties[1].propertyValue");
                    propertyTwoValue.Should().NotBeNullOrWhiteSpace();
                    propertyTwoValue.Should().Be(routingProperties[1].PropertyValue);
                    var propertyTwoType = (string)generatedJson.SelectToken("routingProperties[1].propertyType");
                    propertyTwoType.Should().NotBeNullOrWhiteSpace();
                    propertyTwoType.Should().Be("property");
                });
        }


        /// <summary>
        /// Scenario tests send routing properties are not generated when applications are not supplied.
        /// </summary>
        /// <param name="generator">A generator.</param>
        /// <param name="fileRepository">A file repository.</param>
        /// <param name="routeWalker">A scenario route walker.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GenerateWithNoApplications(AP005SendRoutingPropertyGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, AzureIntegrationServicesModel model, MigrationContext context, Exception e)
        {
            "Given an generator"
                .x(() => generator.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRepository.Object);

            "And a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    model.MigrationTarget.MessageBus = new MessageBus();
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a route walker"
                .x(() => routeWalker = new Mock<IScenarioRouteWalker>().Object);

            "And a generator"
               .x(() => generator = new AP005SendRoutingPropertyGenerator(fileRepository, routeWalker, model, context, logger));

            "When converting"
                .x(async () => e = await Record.ExceptionAsync(async () => await generator.ConvertAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be no context errors"
                .x(() => context.Errors.Should().BeNullOrEmpty());
        }

        /// <summary>
        /// Scenario tests generation fails if the config is missing.
        /// </summary>
        /// <param name="generator">A generator.</param>
        /// <param name="fileRepository">A file repository.</param>
        /// <param name="routeWalker">A scenario route walker.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="model">The model.</param>
        /// <param name="application">An application.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GenerateFailsWhenMissingConfig(AP005SendRoutingPropertyGenerator generator, IFileRepository fileRepository, IScenarioRouteWalker routeWalker, ILogger logger, AzureIntegrationServicesModel model, Application application, MigrationContext context, Exception e)
        {
            var generatedFileName = string.Empty;
            JObject generatedJson = null;
            var resourcemapkey = "resourcemapkey";
            var scenarioName = "scenarioName";
            var activatingIntermediaryName = "activatingIntermediaryName";

            var routingProperties = new List<(string PropertyName, string PropertyValue)>
            {
                ("propertyOneName", "propertyOneValue"),
                ("propertyTwoName", "propertyTwoValue")
            };

            "Given an generator"
                .x(() => generator.Should().BeNull());

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

            "And an application"
                .x(() =>
                {
                    var activatingIntermediary = new MessageSubscriber
                    {
                        Activator = true,
                        ResourceMapKey = resourcemapkey,
                        Name = activatingIntermediaryName
                    };

                    var routingProperties1 = new Dictionary<string, object>();
                    var intermediaryRoutingProperties = routingProperties[0];
                    routingProperties1[intermediaryRoutingProperties.PropertyName] = intermediaryRoutingProperties.PropertyValue;

                    activatingIntermediary.Properties[ModelConstants.ScenarioName] = scenarioName;
                    activatingIntermediary.Properties[ModelConstants.RoutingProperties] = routingProperties1;

                    activatingIntermediary.Resources.Add(
                        new TargetResourceTemplate
                        {
                            ResourceType = "WrongResourceType",
                            OutputPath = "outputpath"
                        });

                    var secondIntermediary = new GenericFilter
                    {
                        Name = "SecondIntermediary"
                    };

                    var endpoint = new AdapterEndpoint
                    {
                        Name = "Endpoint"
                    };

                    var routingProperties2 = new Dictionary<string, object>();
                    var endpointRoutingProperties = routingProperties[1];
                    routingProperties2[endpointRoutingProperties.PropertyName] = endpointRoutingProperties.PropertyValue;
                    endpoint.Properties[ModelConstants.RoutingProperties] = routingProperties2;

                    application = new Application();
                    application.Intermediaries.Add(activatingIntermediary);
                    application.Intermediaries.Add(secondIntermediary);
                    application.Endpoints.Add(endpoint);
                });

            "And a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    model.MigrationTarget.MessageBus = new MessageBus();
                    model.MigrationTarget.MessageBus.Applications.Add(application);
                });

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a route walker"
                .x(() =>
                {
                    var route = new List<(MessagingObject RoutingObject, Channel InputChannel)>();

                    foreach (var intermediary in application.Intermediaries)
                    {
                        route.Add((intermediary, null));
                    }

                    foreach (var endpoint in application.Endpoints)
                    {
                        route.Add((endpoint, null));
                    }

                    var mockRouteWalker = new Mock<IScenarioRouteWalker>();

                    mockRouteWalker.Setup(w => w.WalkSendRoute(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<Intermediary>(),
                        It.IsAny<IEnumerable<Intermediary>>(),
                        It.IsAny<IEnumerable<Channel>>(),
                        It.IsAny<IEnumerable<Endpoint>>()
                        )).Returns(route);

                    routeWalker = mockRouteWalker.Object;
                });

            "And a generator"
               .x(() => generator = new AP005SendRoutingPropertyGenerator(fileRepository, routeWalker, model, context, logger));

            "When converting"
                .x(async () => e = await Record.ExceptionAsync(async () => await generator.ConvertAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be one context error"
                .x(() =>
                {
                    context.Errors.Should().NotBeNullOrEmpty();
                    context.Errors.Should().HaveCount(1);
                    context.Errors[0].Message.Should().Contain(ModelConstants.ResourceTypeRoutingProperties);
                    context.Errors[0].Message.Should().Contain(activatingIntermediaryName);
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
