// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules
{
    /// <summary>
    /// Implements rule AP003, which analyzes the receive port source model and creates the message scenarios in the target model.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The message scenarios represent the receive and send ports that messages flow through to and from the
    /// message box, both one way and two way.  This also includes orchestrations that subscribe to messages
    /// from the message box and then send messages back to the message box, to be routed onwards.
    /// </para>
    /// <para>
    /// The flow is represented in the target model as a set of connected messaging building blocks of endpoints,
    /// intermediaries and channels.  By default, this analyzer will assume that the flow uses routing slips and
    /// therefore will take into account that the channels between intermediaries will be represented by the
    /// channels used by the routing slip router pattern implementation in the associated AIM Azure repository.
    /// </para>
    /// </remarks>    
    public sealed class AP003ReceivePortScenarioAnalyzer : PortScenarioAnalyzer
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "AP003";

        /// <summary>
        /// Defines the key in the model for the message box channel key.
        /// </summary>
        private readonly string _messageBoxChannelKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}:{ModelConstants.MessageBoxLeafKey}";

        /// <summary>
        /// Defines the key in the model for the message box response channel key.
        /// </summary>
        private readonly string _messageBoxResponseChannelKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}:{ModelConstants.MessageBoxResponseLeafKey}";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="AP003ReceivePortScenarioAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public AP003ReceivePortScenarioAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(AP003ReceivePortScenarioAnalyzer), RuleName, model, context, logger)
        {
            // Validate and set the member
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Analyze the receive ports and build the scenario in the target model.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Receive ports consist of an adapter in a receive location, pipeline with components and an optional map.
        /// Two way receive ports also have an equivalent reverse path for message responses that are delivered back
        /// to the adapter and onwards to the application.
        /// </para>
        /// <para>
        /// Multiple receive locations in a receive port are treated as separate scenarios.
        /// </para>
        /// <para>
        /// The basic pattern for analyzing the receive port is to create the following in the target model:
        /// <list type="bullet">
        /// <item>
        /// <description>Adapter becomes an <see cref="ApplicationModel.Target.Endpoints.AdapterEndpoint"/> object.</description>
        /// </item>
        /// <item>
        /// <description>Pipeline component becomes a specialised type of <see cref="ApplicationModel.Target.Intermediaries.Intermediary"/>.</description>
        /// </item>
        /// <item>
        /// <description>Map component becomes an intermediary of type <see cref="ApplicationModel.Target.Intermediaries.MessageTranslator"/>.</description>
        /// </item>
        /// <item>
        /// <description>
        /// The publish to the message box becomes both a<see cref="ApplicationModel.Target.Intermediaries.ContentPromoter"/>
        /// for routing property promotion and <see cref="ApplicationModel.Target.Intermediaries.MessagePublisher"/> for
        /// publishing to the message box topic.
        /// </description>
        /// </item>
        /// </para>
        /// <para>
        /// The response path is the same but in reverse:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// Filter on the subscription receives the response from the message box topic, including the potential for a nack message.
        /// </description>
        /// </item>
        /// <item>
        /// <description>Map component becomes an intermediary of type <see cref="ApplicationModel.Target.Intermediaries.MessageTranslator"/>.</description>
        /// </item>
        /// <item>
        /// <description>Pipeline component becomes a specialised type of <see cref="ApplicationModel.Target.Intermediaries.Intermediary"/>.</description>
        /// </item>
        /// </list>
        /// <item>
        /// <description>Adapter becomes an <see cref="ApplicationModel.Target.Endpoints.AdapterEndpoint"/> object.</description>
        /// </item>
        /// </para>
        /// </remarks>
        /// <param name="sourceApplication">The application object in the source model.</param>
        /// <param name="targetApplication">The application object in the target model.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "AddRange can't be added so wouldn't be consistent from a style perspective.")]
        protected override async Task AnalyzePortsAsync(ResourceItem sourceApplication, Application targetApplication, CancellationToken token)
        {
            _logger.LogDebug(TraceMessages.AnalyzingReceivePortScenarios, RuleName, sourceApplication.Name);

            var scenarios = 0;
            var applicationName = targetApplication.Name.FormatKey();

            // Find receive ports in application
            var receivePorts = sourceApplication.FindRelatedResourcesByType(Model, ResourceRelationshipType.Child, ModelConstants.ResourceReceivePort);
            foreach (var receivePort in receivePorts)
            {
                var receivePortSource = (ReceivePort)receivePort.SourceObject;
                var receivePortName = receivePort.Name.FormatKey();

                // Find receive locations in receive port
                var receiveLocations = receivePort.Resources.Where(r => r.Type == ModelConstants.ResourceReceiveLocation);
                if (receiveLocations != null && receiveLocations.Any())
                {
                    foreach (var receiveLocation in receiveLocations)
                    {
                        var receiveLocationSource = (ReceiveLocation)receiveLocation.SourceObject;
                        var receiveLocationName = receiveLocation.Name.FormatKey();

                        // Is there a pipeline?
                        if (receiveLocationSource.ReceivePipeline != null)
                        {
                            var route = new List<MessagingObject>();
                            var keyPrefix = $"{Model.MigrationTarget.MessageBus.Key}:{applicationName}:{receivePortName}:{receiveLocationName}";

                            // Create endpoint adapter
                            route.Add(CreateReceiveEndpoint(keyPrefix, sourceApplication, targetApplication, receivePortSource, receiveLocationSource));

                            // Create the intermediaries for the pipeline
                            route.AddRange(CreateReceivePipelineIntermediaries(keyPrefix, sourceApplication, receiveLocationSource.ReceivePipeline, receiveLocationSource.ReceivePipelineCustomConfiguration));

                            // If there is a map, create the intermediary
                            if (receivePortSource.Transforms != null && receivePortSource.Transforms.Any())
                            {
                                // Find map resource items
                                var transforms = receivePort.FindRelatedResourcesByType(Model, ResourceRelationshipType.ReferencesTo, ModelConstants.ResourceMap);

                                // Add to route
                                var intermediary = CreateMapIntermediary(keyPrefix, targetApplication, transforms);
                                if (intermediary != null)
                                {
                                    route.Add(intermediary);
                                }
                            }
                            else
                            {
                                _logger.LogTrace(TraceMessages.NoReceiveMapSpecifiedOnReceivePort, RuleName, receivePortSource.Name, receiveLocationSource.Name);
                            }

                            // Check the route for interchange batch handling
                            var handleBatches = route.Any(s => s.Properties.ContainsKey(ModelConstants.HandleBatches) && (bool)s.Properties[ModelConstants.HandleBatches]);
                            if (handleBatches)
                            {
                                // Need to add intermediaries for batch handling (regardless of recoverable interchange processing, all routes will
                                // go through the content based router, but if batch failures must be handled as a unit, then messages in an
                                // interchange will go via a queue used to aggregate messages back into a batch to see if the batch failed, before
                                // being split again and sent to the message box topic.
                                route.AddRange(CreateInterchangeHandlingIntermediaries(keyPrefix));
                            }

                            // Create the message agent intermediaries
                            route.AddRange(CreateMessageAgentIntermediaries(keyPrefix, _messageBoxChannelKey, false, null));

                            // Binds the route by adding routing slip router intermediaries between the intermediaries in the receive port
                            var boundRoute = BindRoute(keyPrefix, targetApplication, receivePortSource, route);

                            // Binds the channels between the endpoint and intermediaries up to the message box (topic channel)
                            BindChannels(keyPrefix, targetApplication, receivePortSource, boundRoute);

                            // Add a new route for interchange batch completion handling, if required
                            if (handleBatches)
                            {
                                // Need to add interchange aggregation for batch failure handling as an atomic unit (recoverable interchange
                                // processing = false).  If batch succeeds and the messages in the batch can be published, then it goes back through
                                // a splitter for the messages to be published individually to the message box, otherwise the aggregated message is
                                // sent to the suspend queue channel.  The aggregator is an Activator intermediary as it triggers off the interchange
                                // channel, so this is treated as its own route with routing slip.  In this instance, the content based router won't
                                // route to the content promoter and the route ends with the main scenario route before it gets to the content promoter.
                                BuildInterchangeAggregationRoute(keyPrefix, targetApplication, receivePortSource, receiveLocationSource);
                            }

                            // If port is two way, check for reverse path and add a new route if required.
                            if (receivePortSource.IsTwoWay)
                            {
                                AnalyzeReceivePortResponse(keyPrefix, sourceApplication, targetApplication, receivePortSource, receiveLocation);
                            }

                            scenarios++;
                        }
                        else
                        {
                            _logger.LogError(ErrorMessages.ReceivePipelineNotSetInReceiveLocation, receiveLocation.Name);
                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.ReceivePipelineNotSetInReceiveLocation, receiveLocation.Name)));
                        }
                    }
                }
            }

            if (scenarios > 0)
            {
                _logger.LogDebug(TraceMessages.FoundReceivePortScenariosInApplication, RuleName, scenarios, sourceApplication.Name);
            }
            else
            {
                _logger.LogDebug(TraceMessages.NoReceivePortsFoundInApplication, RuleName, sourceApplication.Name);
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Analyzes the response path of a two way receive port to build the route back to the endpoint.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="sourceApplication">The application object in the source model.</param>
        /// <param name="targetApplication">The application object in the target model.</param>
        /// <param name="receivePort">The receive port.</param>
        /// <param name="receiveLocation">The receive location.</param>
        private void AnalyzeReceivePortResponse(string intermediaryKeyPrefix, ResourceItem sourceApplication, Application targetApplication, ReceivePort receivePort, ResourceItem receiveLocation)
        {
            _logger.LogDebug(TraceMessages.ReceivePortIsTwoWay, RuleName, receivePort.Name);

            var receiveLocationSource = (ReceiveLocation)receiveLocation.SourceObject;

            // Create the message subscriber.
            var route = new List<MessagingObject>
            {
                CreateMessageSubscriberIntermediary(intermediaryKeyPrefix, targetApplication, receivePort, receiveLocation)
            };

            // If there is a pipeline
            if (receiveLocationSource.SendPipeline != null)
            {
                route.AddRange(CreateSendPipelineIntermediaries(intermediaryKeyPrefix, sourceApplication, receiveLocationSource.SendPipeline, receiveLocationSource.SendPipelineCustomConfiguration));
            }

            // Create the message agent intermediaries
            route.AddRange(CreateMessageAgentIntermediaries(intermediaryKeyPrefix, _messageBoxResponseChannelKey, receivePort.IsTwoWay, ModelConstants.CorrelationId));

            // Binds the route by adding routing slip router intermediaries between the intermediaries in the receive port
            var boundRoute = BindRoute(intermediaryKeyPrefix, targetApplication, receivePort, route);

            // Binds the channels between the endpoint and intermediaries up to the message box (topic channel)
            BindResponseChannels(intermediaryKeyPrefix, targetApplication, receivePort, boundRoute);
        }

        /// <summary>
        /// Builds a new route for interchange handling where recoverable interchange processing is set to false, meaning
        /// the batch must atomically succeed or fail as a unit, rather than an individual messages.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="receivePort">The receive port.</param>
        /// <param name="receiveLocation">The receive location.</param>
        private void BuildInterchangeAggregationRoute(string intermediaryKeyPrefix, Application targetApplication, ReceivePort receivePort, ReceiveLocation receiveLocation)
        {
            _logger.LogDebug(TraceMessages.BuildingNewRouteForInterchangeHandling, RuleName);

            // Find interchange queue channel
            var interchangeQueueChannelKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}:{ModelConstants.InterchangeQueueLeafKey}";

            var messagingObject = Model.FindMessagingObject(interchangeQueueChannelKey);
            if (messagingObject.messagingObject != null)
            {
                _logger.LogDebug(TraceMessages.CreatingInterchangeAggregatorIntermediary, RuleName, MigrationTargetResources.InterchangeAggregatorName);

                var route = new List<MessagingObject>();

                // Create an intermediary based on the aggregator
                var aggregatorIntermediary = new Aggregator(MigrationTargetResources.InterchangeAggregatorName)
                {
                    Activator = true,
                    Description = MigrationTargetResources.InterchangeAggregatorDescription,
                    Key = $"{intermediaryKeyPrefix}:{ModelConstants.InterchangeAggregatorLeafKey}",
                    Rating = ConversionRating.NoAutomaticConversion
                };

                var scenarioName = $"{targetApplication.Name.FormatKey()}.{receivePort.Name.FormatKey()}.{receiveLocation.Name.FormatKey()}.{ModelConstants.InterchangeAggregatorLeafKey}";
                aggregatorIntermediary.Properties.Add(ModelConstants.ScenarioName, scenarioName);
                aggregatorIntermediary.Properties.Add(ModelConstants.ScenarioStepName, "interchangeAggregator");
                aggregatorIntermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());
                aggregatorIntermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());

                // Hook aggregator up to interchange queue
                aggregatorIntermediary.InputChannelKeyRefs.Add(interchangeQueueChannelKey);

                route.Add(aggregatorIntermediary);

                _logger.LogDebug(TraceMessages.CreatingInterchangeSplitterIntermediary, RuleName, MigrationTargetResources.InterchangeSplitterName);

                // Create an intermediary based on the splitter
                var splitterIntermediary = new Splitter(MigrationTargetResources.InterchangeSplitterName)
                {
                    Description = MigrationTargetResources.InterchangeSplitterDescription,
                    Key = $"{intermediaryKeyPrefix}:{ModelConstants.InterchangeSplitterLeafKey}",
                    Rating = ConversionRating.NoAutomaticConversion
                };

                splitterIntermediary.Properties.Add(ModelConstants.ScenarioStepName, "interchangeSplitter");
                splitterIntermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());
                splitterIntermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());

                route.Add(splitterIntermediary);

                // Create the message agent intermediaries
                route.AddRange(CreateMessageAgentIntermediaries(intermediaryKeyPrefix, _messageBoxChannelKey, false, null));

                // Binds the route by adding routing slip router intermediaries between the intermediaries in the receive port
                var boundRoute = BindRoute(intermediaryKeyPrefix, targetApplication, receivePort, route);

                // Binds the channels between the endpoint and intermediaries up to the message box (topic channel)
                BindChannels(intermediaryKeyPrefix, targetApplication, receivePort, boundRoute);
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, interchangeQueueChannelKey);
                _logger.LogError(error);
                Context.Errors.Add(new ErrorMessage(error));
            }
        }

        /// <summary>
        /// Creates an endpoint adapter from a receive location.
        /// </summary>
        /// <param name="endpointKeyPrefix">The prefix for the endpoint key.</param>
        /// <param name="sourceApplication">The application to parse for receive ports.</param>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="receivePort">The receive port.</param>
        /// <param name="receiveLocation">The receive location.</param>
        /// <returns>The created endpoint adapter.</returns>
#pragma warning disable CA1801
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Used as routing slip config key.")]
        private MessagingObject CreateReceiveEndpoint(string endpointKeyPrefix, ResourceItem sourceApplication, Application targetApplication, ReceivePort receivePort, ReceiveLocation receiveLocation)
#pragma warning restore CA1801
        {
            _logger.LogDebug(TraceMessages.CreatingReceiveEndpointAdapter, RuleName, receiveLocation.ReceiveLocationTransportType.Name, receiveLocation.Name);

            // Create an endpoint adapter (assume receive-response or receive, but some adapters may be accept which will be changed later by a specific rule)
            var endpointAdapter = new AdapterEndpoint(receiveLocation.Name, receiveLocation.ReceiveLocationTransportType.Name)
            {
                Activator = true,
                Description = receiveLocation.Description,
                Key = $"{endpointKeyPrefix}:{ModelConstants.AdapterEndpointLeafKey}",
                MessageDeliveryGuarantee = MessageDeliveryGuarantee.AtLeastOnce,
                MessageExchangePattern = receivePort.IsTwoWay ? MessageExchangePattern.ReceiveResponse : MessageExchangePattern.Receive
            };

            // TODO: Add schema references from source application

            // By default, this isn't convertible unless overridden by a specific rule
            endpointAdapter.Rating = ConversionRating.NoAutomaticConversion;

            // Set scenario and step name as properties on the endpoint
            var scenarioName = $"{targetApplication.Name.FormatKey()}.{receivePort.Name.FormatKey()}.{receiveLocation.Name.FormatKey()}";
            endpointAdapter.Properties.Add(ModelConstants.ScenarioName, scenarioName);
            endpointAdapter.Properties.Add(ModelConstants.ScenarioStepName, $"{receiveLocation.ReceiveLocationTransportType.Name.ToLowerInvariant()}ReceiveAdapter");

            // Set BizTalk specific configuration properties
            var configurationEntries = new Dictionary<string, object>()
            {
                { ModelConstants.IsTwoWay, receivePort.IsTwoWay },
                { ModelConstants.BizTalkReceivePortName, receivePort.Name },
                { ModelConstants.BizTalkReceivePortId, $"{targetApplication.Name.FormatKey()}.{receivePort.Name.FormatKey()}" },
                { ModelConstants.BizTalkInboundTransportType, receiveLocation.ReceiveLocationTransportType.Name },
                { ModelConstants.BizTalkInboundTransportLocation, receiveLocation.Address },
                { ModelConstants.FailedMessageRouting, true }
            };

            // Is it a two-way port?
            if (receivePort.IsTwoWay)
            {
                configurationEntries.Add(ModelConstants.ResponseTimeout, 20);
            }

            // Add configuration properties
            endpointAdapter.Properties.Add(ModelConstants.ConfigurationEntry, configurationEntries);

            // Set BizTalk specific routing properties
            var routingProperties = new Dictionary<string, object>()
            {
                { ModelConstants.BizTalkReceivePortName, ModelConstants.BizTalkReceivePortName },
                { ModelConstants.BizTalkReceivePortId, ModelConstants.BizTalkReceivePortId },
                { ModelConstants.BizTalkInboundTransportType, ModelConstants.BizTalkInboundTransportType },
                { ModelConstants.BizTalkInboundTransportLocation, ModelConstants.BizTalkInboundTransportLocation }
            };

            // Add routing properties
            endpointAdapter.Properties.Add(ModelConstants.RoutingProperties, routingProperties);

            // Add response topic subscription
            if (receivePort.IsTwoWay)
            {
                CreateEndpointFilter(endpointAdapter, receivePort);
            }

            return endpointAdapter;
        }

        /// <summary>
        /// Creates a subscription and filter on the endpoint for the response topic of the response path of the receive port.
        /// </summary>
        /// <param name="endpoint">The endpointsubscriber.</param>
        /// <param name="receivePort">The receive port.</param>
        private void CreateEndpointFilter(Endpoint endpoint, ReceivePort receivePort)
        {
            var topicChannelKey = _messageBoxResponseChannelKey;
            var receivePortSubscriptionName = receivePort.Name.FormatKey();

            // Find topic channel to add subscriptions to
            var topicChannelObject = Model.FindMessagingObject(topicChannelKey);
            if (topicChannelObject.application != null && topicChannelObject.messagingObject != null)
            {
                var topicChannel = (TopicChannel)topicChannelObject.messagingObject;

                // Create subscription and name it after the receive port
                var subscription = new Subscription(receivePortSubscriptionName, topicChannel.TopicName)
                {
                    IsDurable = true,
                    IsOrdered = true
                };

                _logger.LogDebug(TraceMessages.CreatingReceivePortResponseSubscription, RuleName, receivePort.Name);

                // Create filter expression against the receive port ID
                var targetFilter = new Filter()
                {
                    FilterExpression = $"{ModelConstants.BizTalkAckReceivePortId} = '{((Dictionary<string, object>)endpoint.Properties[ModelConstants.ConfigurationEntry])[ModelConstants.BizTalkReceivePortId]}'"
                };

                // Create a group, doesn't matter too much what the operation is as there is only one filter
                var targetFilterGroup = new OrFilterGroup();
                targetFilterGroup.Filters.Add(targetFilter);

                _logger.LogTrace(TraceMessages.CreatedSubscriptionFilterStatement, RuleName, targetFilter.FilterExpression);

                // Create filter
                subscription.Filters.Add(new SubscriptionFilter(targetFilterGroup));

                _logger.LogTrace(TraceMessages.CreatedSubscriptionFilterGroupForReceivePort, RuleName, targetFilterGroup.Operation, receivePort.Name);

                // Add subscription to channel
                topicChannel.Subscriptions.Add(subscription);

                // Relate subscription on channel to the endpoint
                endpoint.Properties.Add(ModelConstants.ResponseSubscription, subscription.Name);
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, topicChannelKey);
                _logger.LogError(error);
                Context.Errors.Add(new ErrorMessage(error));
            }
        }

        /// <summary>
        /// Creates an intermediary for content based router, which decides based on recoverable interchange
        /// processing, whether to route to content promoter or send message to interchange channel for
        /// message aggregation.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <returns>Returns list of intermediaries.</returns>
        private IList<MessagingObject> CreateInterchangeHandlingIntermediaries(string intermediaryKeyPrefix)
        {
            var intermediaries = new List<MessagingObject>();

            _logger.LogDebug(TraceMessages.CreatingContentBasedRouterIntermediary, RuleName, MigrationTargetResources.ContentBasedRouterName);

            // Find interchange queue channel
            var interchangeQueueChannelKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}:{ModelConstants.InterchangeQueueLeafKey}";

            var messagingObject = Model.FindMessagingObject(interchangeQueueChannelKey);
            if (messagingObject.messagingObject != null)
            {
                // Create an intermediary based on the content based router
                var contentBasedRouterIntermediary = new ContentBasedRouter(MigrationTargetResources.ContentBasedRouterName)
                {
                    Description = MigrationTargetResources.ContentBasedRouterDescription,
                    Key = $"{intermediaryKeyPrefix}:{ModelConstants.ContentBasedRouterLeafKey}",
                    Rating = ConversionRating.NoAutomaticConversion
                };

                contentBasedRouterIntermediary.Properties.Add(ModelConstants.ScenarioStepName, "contentBasedRouter");
                contentBasedRouterIntermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());
                contentBasedRouterIntermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());

                // Hook content based router up to interchange queue
                contentBasedRouterIntermediary.OutputChannelKeyRefs.Add(interchangeQueueChannelKey);

                intermediaries.Add(contentBasedRouterIntermediary);
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, interchangeQueueChannelKey);
                _logger.LogError(error);
                Context.Errors.Add(new ErrorMessage(error));
            }

            return intermediaries;
        }

        /// <summary>
        /// Creates a message subscriber for a receive port, receive location response.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the endpoint key.</param>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="receivePort">The receive port.</param>
        /// <param name="receiveLocation">The receive location.</param>
        /// <returns>The created message subscriber.</returns>
        private MessageSubscriber CreateMessageSubscriberIntermediary(string intermediaryKeyPrefix, Application targetApplication, ReceivePort receivePort, ResourceItem receiveLocation)
        {
            _logger.LogDebug(TraceMessages.CreatingMessageSubscriberIntermediaryReceivePort, RuleName, MigrationTargetResources.MessageSubscriberName, receivePort.Name);

            // Format names
            var applicationName = targetApplication.Name.FormatKey();
            var receivePortName = receivePort.Name.FormatKey();
            var receiveLocationName = receiveLocation.Name.FormatKey();

            var scenarioName = $"{targetApplication.Name.FormatKey()}.{receivePort.Name.FormatKey()}.{receiveLocationName}.Response";

            // Create an intermediary based on the message subscriber
            var messageSubscriberIntermediary = new MessageSubscriber(receivePort.Name)
            {
                Description = MigrationTargetResources.MessageSubscriberDescription,
                Key = $"{intermediaryKeyPrefix}:{ModelConstants.MessageSubscriberLeafKey}",
                ResourceMapKey = $"topicSubscriber{applicationName.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-")}{receivePortName.Replace(".", "-").Replace("/", "-").Replace(":", "-")}",
                Rating = ConversionRating.FullConversion,
                Activator = true
            };

            // Set scenario and step name as properties on the endpoint
            messageSubscriberIntermediary.Properties.Add(ModelConstants.ScenarioName, scenarioName);
            messageSubscriberIntermediary.Properties.Add(ModelConstants.ScenarioStepName, "messageSubscriber");

            // Set BizTalk specific configuration properties
            var configurationEntries = new Dictionary<string, object>()
            {
                { ModelConstants.BizTalkAckReceivePortId, $"{applicationName}.{receivePortName}" }
            };

            // Add configuration properties
            messageSubscriberIntermediary.Properties.Add(ModelConstants.ConfigurationEntry, configurationEntries);

            // Set BizTalk specific routing properties
            var routingProperties = new Dictionary<string, object>()
            {
                { ModelConstants.BizTalkAckReceivePortId, ModelConstants.BizTalkAckReceivePortId }
            };

            // Add routing properties
            messageSubscriberIntermediary.Properties.Add(ModelConstants.RoutingProperties, routingProperties);

            // Add subscription filter
            CreateMessageSubscriberFilter(messageSubscriberIntermediary, receivePort);

            return messageSubscriberIntermediary;
        }

        /// <summary>
        /// Creates a subscription and filter on the message subscriber for the receive port response handler.
        /// </summary>
        /// <param name="messageSubscriber">The message subscriber.</param>
        /// <param name="receivePort">The receive port.</param>
        private void CreateMessageSubscriberFilter(MessageSubscriber messageSubscriber, ReceivePort receivePort)
        {
            var topicChannelKey = _messageBoxChannelKey;
            var receivePortSubscriptionName = receivePort.Name.FormatKey();

            // Find topic channel to add subscriptions to
            var topicChannelObject = Model.FindMessagingObject(topicChannelKey);
            if (topicChannelObject.application != null && topicChannelObject.messagingObject != null)
            {
                var topicChannel = (TopicChannel)topicChannelObject.messagingObject;

                // Subscriptions will be created independent of subscribers so that messages can be delivered to
                // the subscriptions, meaning the subscription lifetime is not tied to the consumer.  This is a durable
                // subscription and the only ones we intend to create.
                messageSubscriber.IsDurable = true;

                // Create subscription and name it after the receive port
                var subscription = new Subscription(receivePortSubscriptionName, topicChannel.TopicName)
                {
                    IsDurable = messageSubscriber.IsDurable
                };

                _logger.LogDebug(TraceMessages.CreatingReceivePortSubscription, RuleName, receivePort.Name);

                // Create filter expression against the receive port ID
                var targetFilter = new Filter()
                {
                    FilterExpression = $"{ModelConstants.BizTalkAckReceivePortId} = '{((Dictionary<string, object>)messageSubscriber.Properties[ModelConstants.ConfigurationEntry])[ModelConstants.BizTalkAckReceivePortId]}'"
                };

                // Create a group, doesn't matter too much what the operation is as there is only one filter
                var targetFilterGroup = new OrFilterGroup();
                targetFilterGroup.Filters.Add(targetFilter);

                _logger.LogTrace(TraceMessages.CreatedSubscriptionFilterStatement, RuleName, targetFilter.FilterExpression);

                // Create filter
                subscription.Filters.Add(new SubscriptionFilter(targetFilterGroup));

                _logger.LogTrace(TraceMessages.CreatedSubscriptionFilterGroupForReceivePort, RuleName, targetFilterGroup.Operation, receivePort.Name);

                // Add subscription to channel
                topicChannel.Subscriptions.Add(subscription);

                // Relate subscription on channel to the subscriber
                messageSubscriber.TopicSubscriptions.Add(topicChannel.TopicName, subscription.Name);
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, topicChannelKey);
                _logger.LogError(error);
                Context.Errors.Add(new ErrorMessage(error));
            }
        }

        /// <summary>
        /// Binds the route with the routing slip router between the intermediaries and endpoint (if present).
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="receivePort">The receive port.</param>
        /// <param name="route">The chain of messaging objects that need to be bound by routing slip router.</param>
        /// <returns>Returns the new route bound with routing slip router intermediaries.</returns>
        private IList<MessagingObject> BindRoute(string intermediaryKeyPrefix, Application targetApplication, ReceivePort receivePort, IList<MessagingObject> route)
        {
            _logger.LogDebug(TraceMessages.BindingRouteForReceivePort, RuleName, receivePort.Name);

            var boundRoute = new List<MessagingObject>();

            for (var i = 0; i < route.Count; i++)
            {
                // Get messaging object
                var step = route[i];

                // Determine from and to step names (if last step, end the route)
                var fromStep = route[i].Name;
                var toStep = i == (route.Count - 1) ? MigrationTargetResources.EndRoute : route[i + 1].Name;

                // Add step and then follow with a routing slip router intermediary
                boundRoute.Add(step);
                boundRoute.Add(CreateRoutingSlipRouterIntermediary(intermediaryKeyPrefix, fromStep, toStep));
            }

            // Add to target application
            foreach (var step in boundRoute)
            {
                if (step is Endpoint)
                {
                    targetApplication.Endpoints.Add((Endpoint)step);
                }
                else
                {
                    targetApplication.Intermediaries.Add((Intermediary)step);
                }
            }

            return boundRoute;
        }

        /// <summary>
        /// Binds the intermediaries and endpoint (if present) together.
        /// </summary>
        /// <param name="channelKeyPrefix">The prefix for the channel key.</param>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="receivePort">The receive port.</param>
        /// <param name="route">The chain of messaging objects that need to be bound by channels.</param>
        private void BindChannels(string channelKeyPrefix, Application targetApplication, ReceivePort receivePort, IList<MessagingObject> route)
        {
            _logger.LogDebug(TraceMessages.BindingChannelsForReceivePort, RuleName, receivePort.Name);

            // Assume route is in order, that is, first entry is the start of the route.  As the route is built
            // in this class, not expecting anything other than intermediaries and optionally an endpoint.
            for (var i = 0; i < route.Count - 1; i++)
            {
                // Determine from and to steps
                var fromStep = route[i];
                var toStep = route[i + 1];

                // Create channel
                var channel = new TriggerChannel(MigrationTargetResources.TriggerChannelName)
                {
                    Description = string.Format(CultureInfo.CurrentCulture, MigrationTargetResources.TriggerChannelDescription, toStep),
                    Key = $"{channelKeyPrefix}:{ModelConstants.TriggerChannelLeafKey}:{fromStep.Name.FormatKey()}-{toStep.Name.FormatKey()}",
                    Rating = ConversionRating.FullConversion
                };

                // Are we going from a routing slip router?
                if (fromStep is RoutingSlipRouter)
                {
                    // Set URL
                    var scenarioStep = toStep.Properties[ModelConstants.ScenarioStepName];
                    channel.TriggerUrl = $"/routingManager/route/{scenarioStep}";

                    // Label the channel appropriately
                    channel.Properties.Add(ModelConstants.RouteLabel, MigrationTargetResources.RouteToChannelLabel);
                }
                else
                {
                    // Label the channel appropriately
                    channel.Properties.Add(ModelConstants.RouteLabel, MigrationTargetResources.RouteFromChannelLabel);
                }

                // Add channel to application
                targetApplication.Channels.Add(channel);

                // Bind channel with endpoint and intermediaries
                if (fromStep is Endpoint endpoint)
                {
                    endpoint.OutputChannelKeyRef = channel.Key;
                }

                if (fromStep is Intermediary intermediary)
                {
                    intermediary.OutputChannelKeyRefs.Add(channel.Key);
                }

                ((Intermediary)toStep).InputChannelKeyRefs.Add(channel.Key);
            }
        }

        /// <summary>
        /// Binds the intermediaries and endpoint together.
        /// </summary>
        /// <param name="channelKeyPrefix">The prefix for the channel key.</param>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="receivePort">The receive port.</param>
        /// <param name="route">The chain of messaging objects that need to be bound by channels.</param>
        private void BindResponseChannels(string channelKeyPrefix, Application targetApplication, ReceivePort receivePort, IList<MessagingObject> route)
        {
            _logger.LogDebug(TraceMessages.BindingChannelsForReceivePortResponse, RuleName, receivePort.Name);

            // Assume route is in order, that is, first entry is the start of the route.  As the route is built
            // in this class, not expecting anything other than intermediaries and optionally an endpoint.
            for (var i = 0; i < route.Count - 1; i++)
            {
                // Determine from and to steps
                var fromStep = route[i];
                var toStep = route[i + 1];

                // Create channel
                var channel = new TriggerChannel(MigrationTargetResources.TriggerChannelName)
                {
                    Description = string.Format(CultureInfo.CurrentCulture, MigrationTargetResources.TriggerChannelDescription, toStep),
                    Key = $"{channelKeyPrefix}:{ModelConstants.TriggerChannelLeafKey}Response:{fromStep.Name.FormatKey()}-{toStep.Name.FormatKey()}",
                    Rating = ConversionRating.FullConversion
                };

                // Are we going from a routing slip router?
                if (fromStep is RoutingSlipRouter)
                {
                    // Set URL
                    var scenarioStep = toStep.Properties[ModelConstants.ScenarioStepName];
                    channel.TriggerUrl = $"/routingManager/route/{scenarioStep}";

                    // Label the channel appropriately
                    channel.Properties.Add(ModelConstants.RouteLabel, MigrationTargetResources.RouteToChannelLabel);
                }
                else
                {
                    // Label the channel appropriately
                    channel.Properties.Add(ModelConstants.RouteLabel, MigrationTargetResources.RouteFromChannelLabel);
                }

                // Add channel to application
                targetApplication.Channels.Add(channel);

                // Bind channel with endpoint and intermediaries
                if (fromStep is Endpoint endpoint)
                {
                    endpoint.OutputChannelKeyRef = channel.Key;
                }

                if (fromStep is Intermediary intermediary)
                {
                    intermediary.OutputChannelKeyRefs.Add(channel.Key);
                }

                ((Intermediary)toStep).InputChannelKeyRefs.Add(channel.Key);
            }
        }
    }
}
