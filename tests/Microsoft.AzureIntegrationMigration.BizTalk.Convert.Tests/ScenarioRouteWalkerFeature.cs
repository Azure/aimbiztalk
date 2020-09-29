// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Tests;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert.Tests
{
    /// <summary>
    /// Tests for the <see cref="ScenarioRouteWalker"/> class.
    /// </summary>
    public class ScenarioRouteWalkerFeature
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
        /// Defines a rule name.
        /// </summary>
        private readonly string _ruleName = "TestRule";

        /// <summary>
        /// Defines a scenario name.
        /// </summary>
        private readonly string _scenarioName = "TestScenario";

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
        /// Scenario tests that the object construction throws an exception when a null logger is passed.
        /// </summary>
        /// <param name="routeWalker">The scenario router walker.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullLogger(ScenarioRouteWalker routeWalker, ILogger logger, MigrationContext context, Exception e)
        {
            "Given an route walker"
                .x(() => routeWalker.Should().BeNull());

            "And a null logger"
              .x(() => logger.Should().BeNull());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "When constructing with a null logger"
                .x(() => e = Record.Exception(() => new ScenarioRouteWalker(context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null context is passed.
        /// </summary>
        /// <param name="routeWalker">The scenario router walker.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullContext(ScenarioRouteWalker routeWalker, ILogger logger, MigrationContext context, Exception e)
        {
            "Given an route walker"
                .x(() => routeWalker.Should().BeNull());

            "And a logger"
              .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context.Should().BeNull());

            "When constructing with a null context"
                .x(() => e = Record.Exception(() => new ScenarioRouteWalker(context, logger)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("context"));
        }

        /// <summary>
        /// Scenario tests that the object construction works when params ok.
        /// </summary>
        /// <param name="routeWalker">The scenario router walker.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithSuccess(ScenarioRouteWalker routeWalker, ILogger logger, MigrationContext context, Exception e)
        {
            "Given a route walker"
                .x(() => routeWalker.Should().BeNull());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "When constructing"
                .x(() => e = Record.Exception(() => new ScenarioRouteWalker(context, logger)));

            "Then the constructor should NOT throw an exception"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region Receive Route 

        /// <summary>
        /// Scenario tests walking the receive route.
        /// </summary>
        /// <param name="routeWalker">The scenario router walker.</param>
        /// <param name="initiatingEndpoint">The initiating endpoint.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="endpoints">The endpoints.</param>
        /// <param name="intermediaries">The intermediaries.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="route">The walked route.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]

        public void WalkReceiveRouteWithSuccess(ScenarioRouteWalker routeWalker, ILogger logger, MigrationContext context, Endpoint initiatingEndpoint, List<Intermediary> intermediaries, List<Channel> channels, List<Endpoint> endpoints, IList<(MessagingObject RoutingObject, Channel InputChannel)> route, Exception e)
        {
            var endpointToIntermediaryOneChannelKey = "endpointToIntermediaryOneChannelKey";
            var endpointToIntermediaryOneChannelName = "endpointToIntermediaryOneChannelName";
            var intermediaryOneToIntermediaryTwoChannelKey = "intermediaryOneToIntermediaryTwoChannelKey";
            var intermediaryOneToIntermediaryTwoChannelName = "intermediaryOneToIntermediaryTwoChannelName";
            var activatingEndpointName = "activatingEndpointName";
            var activatingEndpointKey = "activatingEndpointKey";
            var intermediaryOneName = "intermediaryOneName";
            var intermediaryOneKey = "intermediaryOneKey";
            var intermediaryTwoName = "intermediaryTwoName";
            var intermediaryTwoKey = "intermediaryTwoKey";
            var topicChannelName = "topicChannelName";
            var topicChannelKey = "topicChannelKey";

            "Given a route walker"
               .x(() => routeWalker.Should().BeNull());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an generator"
              .x(() => routeWalker = new ScenarioRouteWalker(context, logger));

            "And a route to walk"
                .x(() =>
                {
                    channels = new List<Channel>();
                    endpoints = new List<Endpoint>();
                    intermediaries = new List<Intermediary>();

                    var endpointToIntermediaryOneChannel = new TriggerChannel
                    {
                        Key = endpointToIntermediaryOneChannelKey,
                        Name = endpointToIntermediaryOneChannelName
                    };
                    endpointToIntermediaryOneChannel.Properties.Add(ModelConstants.RouteLabel, "RouteLabel");

                    var intermediaryOneToIntermediaryTwoChannel = new TriggerChannel
                    {
                        Key = intermediaryOneToIntermediaryTwoChannelKey,
                        Name = intermediaryOneToIntermediaryTwoChannelName
                    };
                    intermediaryOneToIntermediaryTwoChannel.Properties.Add(ModelConstants.RouteLabel, "RouteLabel");

                    var topicChannel = new TopicChannel
                    {
                        Name = topicChannelName,
                        Key = topicChannelKey
                    };

                    initiatingEndpoint = new AdapterEndpoint
                    {
                        Name = activatingEndpointName,
                        Key = activatingEndpointKey,
                        Activator = true,
                        InputChannelKeyRef = null,
                        OutputChannelKeyRef = endpointToIntermediaryOneChannel.Key
                    };

                    var intermediaryOne = new GenericFilter
                    {
                        Name = intermediaryOneName,
                        Key = intermediaryOneKey
                    };
                    intermediaryOne.InputChannelKeyRefs.Add(endpointToIntermediaryOneChannel.Key);
                    intermediaryOne.OutputChannelKeyRefs.Add(intermediaryOneToIntermediaryTwoChannel.Key);

                    var intermediaryTwo = new GenericFilter
                    {
                        Name = intermediaryTwoName,
                        Key = intermediaryTwoKey
                    };
                    intermediaryTwo.InputChannelKeyRefs.Add(intermediaryOneToIntermediaryTwoChannel.Key);
                    intermediaryTwo.OutputChannelKeyRefs.Add(topicChannel.Key);

                    endpoints.Add(initiatingEndpoint);
                    intermediaries.Add(intermediaryOne);
                    intermediaries.Add(intermediaryTwo);
                    channels.Add(endpointToIntermediaryOneChannel);
                    channels.Add(intermediaryOneToIntermediaryTwoChannel);
                    channels.Add(topicChannel);
                });

            "When walking the receive route"
                .x(() => e = Record.Exception(() => route = routeWalker.WalkReceiveRoute(_ruleName, _scenarioName, initiatingEndpoint, intermediaries, channels)));

            "Then the walker NOT throw an exception"
                .x(() => e.Should().BeNull());

            "And the route should be walked"
                .x(() =>
                {
                    route.Should().NotBeNull();
                    route.Should().HaveCount(3);

                    route[0].RoutingObject.Should().NotBeNull();
                    route[0].RoutingObject.Name.Should().Be(activatingEndpointName);
                    route[0].RoutingObject.Key.Should().Be(activatingEndpointKey);
                    route[0].InputChannel.Should().BeNull();

                    route[1].RoutingObject.Should().NotBeNull();
                    route[1].RoutingObject.Name.Should().Be(intermediaryOneName);
                    route[1].RoutingObject.Key.Should().Be(intermediaryOneKey);
                    route[1].InputChannel.Should().NotBeNull();
                    route[1].InputChannel.Name.Should().Be(endpointToIntermediaryOneChannelName);
                    route[1].InputChannel.Key.Should().Be(endpointToIntermediaryOneChannelKey);

                    route[2].RoutingObject.Should().NotBeNull();
                    route[2].RoutingObject.Name.Should().Be(intermediaryTwoName);
                    route[2].RoutingObject.Key.Should().Be(intermediaryTwoKey);
                    route[2].InputChannel.Should().NotBeNull();
                    route[2].InputChannel.Name.Should().Be(intermediaryOneToIntermediaryTwoChannelName);
                    route[2].InputChannel.Key.Should().Be(intermediaryOneToIntermediaryTwoChannelKey);

                });
        }

        /// <summary>
        /// Scenario tests walking the receive route when a channel is missing.
        /// </summary>
        /// <param name="routeWalker">The scenario router walker.</param>
        /// <param name="activatingEndpoint">The activating endpoint.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="endpoints">The endpoints.</param>
        /// <param name="intermediaries">The intermediaries.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="route">The walked route.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void WalkReceiveRouteWithMissingChannel(ScenarioRouteWalker routeWalker, ILogger logger, MigrationContext context, Endpoint activatingEndpoint, List<Intermediary> intermediaries, List<Channel> channels, List<Endpoint> endpoints, IList<(MessagingObject RoutingObject, Channel InputChannel)> route, Exception e)
        {
            var wrongChannelKey = "wrongChannelKey";

            "Given a route walker"
               .x(() => routeWalker.Should().BeNull());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an generator"
              .x(() => routeWalker = new ScenarioRouteWalker(context, logger));

            "And a route to walk"
                .x(() =>
                {
                    channels = new List<Channel>();
                    endpoints = new List<Endpoint>();
                    intermediaries = new List<Intermediary>();

                    var endpointToIntermediaryOneChannel = new TriggerChannel
                    {
                        Key = "endpointToIntermediaryOneChannelKey",
                        Name = "endpointToIntermediaryOneChannelName"
                    };
                    endpointToIntermediaryOneChannel.Properties.Add(ModelConstants.RouteLabel, "RouteLabel");

                    var intermediaryOneToIntermediaryTwoChannel = new TriggerChannel
                    {
                        Key = "intermediaryOneToIntermediaryTwoChannelKey",
                        Name = "intermediaryOneToIntermediaryTwoChannelName"
                    };
                    intermediaryOneToIntermediaryTwoChannel.Properties.Add(ModelConstants.RouteLabel, "RouteLabel");

                    var topicChannel = new TopicChannel
                    {
                        Name = "topicChannelName",
                        Key = "topicChannelKey"
                    };

                    activatingEndpoint = new AdapterEndpoint
                    {
                        Name = "activatingEndpointName",
                        Key = "activatingEndpointKey",
                        Activator = true,
                        InputChannelKeyRef = null,
                        OutputChannelKeyRef = endpointToIntermediaryOneChannel.Key
                    };

                    var intermediaryOne = new GenericFilter
                    {
                        Name = "intermediaryOneName",
                        Key = "intermediaryOneKey"
                    };
                    intermediaryOne.InputChannelKeyRefs.Add(endpointToIntermediaryOneChannel.Key);
                    intermediaryOne.OutputChannelKeyRefs.Add(wrongChannelKey);

                    var intermediaryTwo = new GenericFilter
                    {
                        Name = "intermediaryTwoName",
                        Key = "intermediaryTwoKey"
                    };
                    intermediaryTwo.InputChannelKeyRefs.Add(intermediaryOneToIntermediaryTwoChannel.Key);
                    intermediaryTwo.OutputChannelKeyRefs.Add(topicChannel.Key);

                    endpoints.Add(activatingEndpoint);
                    intermediaries.Add(intermediaryOne);
                    intermediaries.Add(intermediaryTwo);
                    channels.Add(endpointToIntermediaryOneChannel);
                    channels.Add(intermediaryOneToIntermediaryTwoChannel);
                    channels.Add(topicChannel);
                });

            "When walking the receive route"
                .x(() => e = Record.Exception(() => route = routeWalker.WalkReceiveRoute(_ruleName, _scenarioName, activatingEndpoint, intermediaries, channels)));

            "Then the walker NOT throw an exception"
                .x(() => e.Should().BeNull());

            "And the context should contain an error"
                .x(() =>
                {
                    context.Should().NotBeNull();
                    context.Errors.Should().NotBeNullOrEmpty();
                    context.Errors.Should().HaveCount(1);
                    context.Errors[0].Message.Should().Contain(wrongChannelKey);
                });
        }

        #endregion

        #region Send Route

        /// <summary>
        /// Scenario tests walking the receive route.
        /// </summary>
        /// <param name="routeWalker">The scenario router walker.</param>
        /// <param name="activatingEndpoint">The activating endpoint.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="endpoints">The endpoints.</param>
        /// <param name="intermediaries">The intermediaries.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="route">The walked route.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]

        public void WalkSendRouteWithSuccess(ScenarioRouteWalker routeWalker, ILogger logger, MigrationContext context, Intermediary activatingIntermediary, List<Intermediary> intermediaries, List<Channel> channels, List<Endpoint> endpoints, IList<(MessagingObject RoutingObject, Channel InputChannel)> route, Exception e)
        {
            var activatingIntermediaryKey = "activatingIntermediaryKey";
            var activatingIntermediaryName = "activatingIntermediaryName";
            var activatingIntermediaryToIntermediaryOneChannelKey = "activatingIntermediaryToIntermediaryOneChannelKey";
            var activatingIntermediaryToIntermediaryOneChannelName = "activatingIntermediaryToIntermediaryOneChannelName";
            var intermediaryOneToIntermediaryTwoChannelKey = "intermediaryOneToIntermediaryTwoChannelKey";
            var intermediaryOneToIntermediaryTwoChannelName = "intermediaryOneToIntermediaryTwoChannelName";
            var finalEndpointName = "finalEndpointName";
            var finalEndpointKey = "finalEndpointKey";
            var intermediaryTwoToFinalEndpointChannelKey = "intermediaryTwoToFinalEndpointChannelKey";
            var intermediaryTwoToFinalEndpointChannelName = "intermediaryTwoToFinalEndpointChannelName";
            var intermediaryOneName = "intermediaryOneName";
            var intermediaryOneKey = "intermediaryOneKey";
            var intermediaryTwoName = "intermediaryTwoName";
            var intermediaryTwoKey = "intermediaryTwoKey";

            "Given a route walker"
               .x(() => routeWalker.Should().BeNull());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an generator"
              .x(() => routeWalker = new ScenarioRouteWalker(context, logger));

            "And a route to walk"
                .x(() =>
                {
                    channels = new List<Channel>();
                    endpoints = new List<Endpoint>();
                    intermediaries = new List<Intermediary>();

                    activatingIntermediary = new GenericFilter
                    {
                        Key = activatingIntermediaryKey,
                        Name = activatingIntermediaryName,
                        Activator = true
                    };

                    var activatingIntermediaryToIntermediaryOneChannel = new TriggerChannel
                    {
                        Key = activatingIntermediaryToIntermediaryOneChannelKey,
                        Name = activatingIntermediaryToIntermediaryOneChannelName
                    };
                    activatingIntermediaryToIntermediaryOneChannel.Properties.Add(ModelConstants.RouteLabel, "RouteLabel");

                    var intermediaryOneToIntermediaryTwoChannel = new TriggerChannel
                    {
                        Key = intermediaryOneToIntermediaryTwoChannelKey,
                        Name = intermediaryOneToIntermediaryTwoChannelName
                    };
                    intermediaryOneToIntermediaryTwoChannel.Properties.Add(ModelConstants.RouteLabel, "RouteLabel");

                    var intermediaryTwoToFinalEndpointChannel = new TriggerChannel
                    {
                        Key = intermediaryTwoToFinalEndpointChannelKey,
                        Name = intermediaryTwoToFinalEndpointChannelName
                    };
                    intermediaryTwoToFinalEndpointChannel.Properties.Add(ModelConstants.RouteLabel, "RouteLabel");

                    var finalEndpoint = new AdapterEndpoint
                    {
                        Name = finalEndpointName,
                        Key = finalEndpointKey,
                        Activator = false,
                        InputChannelKeyRef = intermediaryTwoToFinalEndpointChannel.Key,
                        OutputChannelKeyRef = null
                    };

                    activatingIntermediary.OutputChannelKeyRefs.Add(activatingIntermediaryToIntermediaryOneChannel.Key);

                    var intermediaryOne = new GenericFilter
                    {
                        Name = intermediaryOneName,
                        Key = intermediaryOneKey
                    };
                    intermediaryOne.InputChannelKeyRefs.Add(activatingIntermediaryToIntermediaryOneChannel.Key);
                    intermediaryOne.OutputChannelKeyRefs.Add(intermediaryOneToIntermediaryTwoChannel.Key);

                    var intermediaryTwo = new GenericFilter
                    {
                        Name = intermediaryTwoName,
                        Key = intermediaryTwoKey
                    };
                    intermediaryTwo.InputChannelKeyRefs.Add(intermediaryOneToIntermediaryTwoChannel.Key);
                    intermediaryTwo.OutputChannelKeyRefs.Add(intermediaryTwoToFinalEndpointChannel.Key);

                    endpoints.Add(finalEndpoint);
                    intermediaries.Add(activatingIntermediary);
                    intermediaries.Add(intermediaryOne);
                    intermediaries.Add(intermediaryTwo);
                    channels.Add(activatingIntermediaryToIntermediaryOneChannel);
                    channels.Add(intermediaryOneToIntermediaryTwoChannel);
                    channels.Add(intermediaryTwoToFinalEndpointChannel);
                });

            "When walking the send route"
                .x(() => e = Record.Exception(() => route = routeWalker.WalkSendRoute(_ruleName, _scenarioName, activatingIntermediary, intermediaries, channels, endpoints))); ;

            "Then the walker NOT throw an exception"
                .x(() => e.Should().BeNull());

            "And the route should be walked"
                .x(() =>
                {
                    route.Should().NotBeNullOrEmpty();
                    route.Should().HaveCount(4);

                    route[0].RoutingObject.Should().NotBeNull();
                    route[0].RoutingObject.Name.Should().Be(activatingIntermediaryName);
                    route[0].RoutingObject.Key.Should().Be(activatingIntermediaryKey);
                    route[0].InputChannel.Should().BeNull();

                    route[1].RoutingObject.Should().NotBeNull();
                    route[1].RoutingObject.Name.Should().Be(intermediaryOneName);
                    route[1].RoutingObject.Key.Should().Be(intermediaryOneKey);
                    route[1].InputChannel.Should().NotBeNull();
                    route[1].InputChannel.Name.Should().Be(activatingIntermediaryToIntermediaryOneChannelName);
                    route[1].InputChannel.Key.Should().Be(activatingIntermediaryToIntermediaryOneChannelKey);

                    route[2].RoutingObject.Should().NotBeNull();
                    route[2].RoutingObject.Name.Should().Be(intermediaryTwoName);
                    route[2].RoutingObject.Key.Should().Be(intermediaryTwoKey);
                    route[2].InputChannel.Should().NotBeNull();
                    route[2].InputChannel.Name.Should().Be(intermediaryOneToIntermediaryTwoChannelName);
                    route[2].InputChannel.Key.Should().Be(intermediaryOneToIntermediaryTwoChannelKey);

                    route[3].RoutingObject.Should().NotBeNull();
                    route[3].RoutingObject.Name.Should().Be(finalEndpointName);
                    route[3].RoutingObject.Key.Should().Be(finalEndpointKey);
                    route[3].InputChannel.Should().NotBeNull();
                    route[3].InputChannel.Name.Should().Be(intermediaryTwoToFinalEndpointChannelName);
                    route[3].InputChannel.Key.Should().Be(intermediaryTwoToFinalEndpointChannelKey);
                });
        }

        /// <summary>
        /// Scenario tests walking the receive route when a channel is missing.
        /// </summary>
        /// <param name="routeWalker">The scenario router walker.</param>
        /// <param name="activatingIntermediary">The activating endpoint.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="endpoints">The endpoints.</param>
        /// <param name="intermediaries">The intermediaries.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="route">The walked route.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void WalkSendRouteWithMissingChannel(ScenarioRouteWalker routeWalker, ILogger logger, MigrationContext context, Intermediary activatingIntermediary, List<Intermediary> intermediaries, List<Channel> channels, List<Endpoint> endpoints, IList<(MessagingObject RoutingObject, Channel InputChannel)> route, Exception e)
        {
            var wrongChannelKey = "wrongChannelKey ";

            "Given a route walker"
               .x(() => routeWalker.Should().BeNull());

            "And a context"
                .x(() => context = TestHelper.BuildContext());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And an generator"
              .x(() => routeWalker = new ScenarioRouteWalker(context, logger));

            "And a route to walk"
                .x(() =>
                {
                    channels = new List<Channel>();
                    endpoints = new List<Endpoint>();
                    intermediaries = new List<Intermediary>();

                    activatingIntermediary = new GenericFilter
                    {
                        Key = "activatingIntermediaryKey",
                        Name = "activatingIntermediaryName",
                        Activator = true
                    };

                    var activatingIntermediaryToIntermediaryOneChannel = new TriggerChannel
                    {
                        Key = "activatingIntermediaryToIntermediaryOneChannelKey",
                        Name = "activatingIntermediaryToIntermediaryOneChannelName"
                    };
                    activatingIntermediaryToIntermediaryOneChannel.Properties.Add(ModelConstants.RouteLabel, "RouteLabel");

                    var intermediaryOneToIntermediaryTwoChannel = new TriggerChannel
                    {
                        Key = "intermediaryOneToIntermediaryTwoChannelKey",
                        Name = "intermediaryOneToIntermediaryTwoChannelName"
                    };
                    intermediaryOneToIntermediaryTwoChannel.Properties.Add(ModelConstants.RouteLabel, "RouteLabel");

                    var intermediaryTwoToFinalEndpointChannel = new TriggerChannel
                    {
                        Key = "activatingIntermediaryToIntermediaryOneChannelKey",
                        Name = "activatingIntermediaryToIntermediaryOneChannelName"
                    };
                    intermediaryTwoToFinalEndpointChannel.Properties.Add(ModelConstants.RouteLabel, "RouteLabel");

                    var finalEndpoint = new AdapterEndpoint
                    {
                        Name = "finalEndpointName",
                        Key = "finalEndpointKey",
                        Activator = false,
                        InputChannelKeyRef = "intermediaryTwoToFinalEndpointChannelKey",
                        OutputChannelKeyRef = null
                    };

                    activatingIntermediary.OutputChannelKeyRefs.Add(activatingIntermediaryToIntermediaryOneChannel.Key);

                    var intermediaryOne = new GenericFilter
                    {
                        Name = "intermediaryOneName",
                        Key = "intermediaryOneKey"
                    };
                    intermediaryOne.InputChannelKeyRefs.Add(activatingIntermediaryToIntermediaryOneChannel.Key);
                    intermediaryOne.OutputChannelKeyRefs.Add(wrongChannelKey);

                    var intermediaryTwo = new GenericFilter
                    {
                        Name = "intermediaryTwoName",
                        Key = "intermediaryTwoKey"
                    };
                    intermediaryTwo.InputChannelKeyRefs.Add(intermediaryOneToIntermediaryTwoChannel.Key);
                    intermediaryTwo.OutputChannelKeyRefs.Add(finalEndpoint.Key);

                    endpoints.Add(finalEndpoint);
                    intermediaries.Add(activatingIntermediary);
                    intermediaries.Add(intermediaryOne);
                    intermediaries.Add(intermediaryTwo);
                    channels.Add(activatingIntermediaryToIntermediaryOneChannel);
                    channels.Add(intermediaryOneToIntermediaryTwoChannel);
                    channels.Add(intermediaryTwoToFinalEndpointChannel);
                });

            "When walking the send route"
                .x(() => e = Record.Exception(() => route = routeWalker.WalkSendRoute(_ruleName, _scenarioName, activatingIntermediary, intermediaries, channels, endpoints))); ;

            "Then the walker NOT throw an exception"
                .x(() => e.Should().BeNull());

            "And the context should contain an error"
                .x(() =>
                {
                    context.Should().NotBeNull();
                    context.Errors.Should().NotBeNullOrEmpty();
                    context.Errors.Should().HaveCount(1);
                    context.Errors[0].Message.Should().Contain(wrongChannelKey);
                });
        }
        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
