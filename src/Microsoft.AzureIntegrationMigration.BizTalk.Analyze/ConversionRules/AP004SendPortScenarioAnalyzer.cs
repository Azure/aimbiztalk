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
    /// Implements rule AP004, which analyzes the send port source model and creates the message scenarios in the target model.
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
    public sealed class AP004SendPortScenarioAnalyzer : PortScenarioAnalyzer
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "AP004";

        /// <summary>
        /// Defines the key in the model for the message box channel key.
        /// </summary>
        private readonly string _messageBoxChannelKey = $"{ModelConstants.MessageBusLeafKey}:{ModelConstants.SystemApplicationLeafKey}:{ModelConstants.MessageBoxLeafKey}";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="AP004SendPortScenarioAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public AP004SendPortScenarioAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(AP004SendPortScenarioAnalyzer), RuleName, model, context, logger)
        {
            // Validate and set the member
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Analyze the send ports and build the scenario in the target model.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Send ports consist of an optional map, pipeline with components and an adapter.  Two way send ports also
        /// have a response path back to the message box.
        /// </para>
        /// <para>
        /// The basic pattern for analyzing the send port is to create the following in the target model:
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
        /// <para>
        /// For two way send ports, the route for the response path is:
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
        /// </remarks>
        /// <param name="sourceApplication">The application to parse for receive ports.</param>
        /// <param name="targetApplication">The application in the target.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "AddRange can't be added so wouldn't be consistent from a style perspective.")]
        protected override async Task AnalyzePortsAsync(ResourceItem sourceApplication, Application targetApplication, CancellationToken token)
        {
            _logger.LogDebug(TraceMessages.AnalyzingSendPortScenarios, RuleName, sourceApplication.Name);

            var scenarios = 0;
            var applicationName = targetApplication.Name.FormatKey();

            // TODO: Handle distribution lists

            // Find send ports in application
            var sendPorts = sourceApplication.FindRelatedResourcesByType(Model, ResourceRelationshipType.Child, ModelConstants.ResourceSendPort);
            foreach (var sendPort in sendPorts)
            {
                var sendPortSource = (SendPort)sendPort.SourceObject;
                var sendPortName = sendPort.Name.FormatKey();

                // Is there a pipeline?
                if (sendPortSource.TransmitPipeline != null)
                {
                    var route = new List<MessagingObject>();
                    var keyPrefix = $"{Model.MigrationTarget.MessageBus.Key}:{applicationName}:{sendPortName}";

                    // Create message subscriber
                    route.Add(CreateMessageSubscriberIntermediary(keyPrefix, targetApplication, sendPortSource));

                    // If there is a map, create the intermediary
                    if (sendPortSource.Transforms != null && sendPortSource.Transforms.Any())
                    {
                        // Find map resource items
                        var transforms = sendPort.FindRelatedResourcesByType(Model, ResourceRelationshipType.ReferencesTo, ModelConstants.ResourceMap);

                        // Add to route
                        var intermediary = CreateMapIntermediary(keyPrefix, targetApplication, transforms);
                        if (intermediary != null)
                        {
                            route.Add(intermediary);
                        }
                    }
                    else
                    {
                        _logger.LogTrace(TraceMessages.NoSendMapSpecifiedOnSendPort, RuleName, sendPortSource.Name);
                    }

                    // Create the intermediaries for the pipeline
                    route.AddRange(CreateSendPipelineIntermediaries(keyPrefix, sourceApplication, sendPortSource.TransmitPipeline, sendPortSource.SendPipelineCustomConfiguration));

                    // Create endpoint adapter
                    var endpointAdapter = CreateSendEndpoint(keyPrefix, sendPortSource);
                    route.Add(endpointAdapter);

                    // Binds the route by adding routing slip router intermediaries between the intermediaries and the endpoint in the send port
                    var boundRoute = BindRoute(keyPrefix, targetApplication, sendPortSource, route);

                    // Binds the channels between the message box (topic channel), intermediaries up to the endpoint (send adapter)
                    var topicChannelKey = _messageBoxChannelKey;
                    BindChannels(keyPrefix, topicChannelKey, targetApplication, sendPortSource, boundRoute);

                    // Add a new route for interchange batch completion handling, if required
                    var handleBatches = route.Any(s => s.Properties.ContainsKey(ModelConstants.HandleBatches) && (bool)s.Properties[ModelConstants.HandleBatches]);
                    if (handleBatches)
                    {
                        // Need to add interchange aggregation for individual messages that are to be sent as a batch, as would be
                        // done in an orchestration by building a batch of messages and calling send pipeline inline.  Instead, this
                        // adds an aggregator to the model which receives messages from an interchange queue and which then feeds the
                        // batch to the start of the send pipeline intermediaries.
                        BuildInterchangeAggregationRoute(keyPrefix, sourceApplication, targetApplication, sendPortSource);
                    }

                    // If port is two way, check for reverse path
                    if (sendPortSource.IsTwoWay)
                    {
                        AnalyzeSendPortResponse(keyPrefix, sourceApplication, targetApplication, sendPortSource, sendPort, endpointAdapter);
                    }

                    scenarios++;
                }
                else
                {
                    _logger.LogError(ErrorMessages.TransmitPipelineNotSetInSendPort, sendPort.Name);
                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.TransmitPipelineNotSetInSendPort, sendPort.Name)));
                }
            }

            if (scenarios > 0)
            {
                _logger.LogDebug(TraceMessages.FoundSendPortScenariosInApplication, RuleName, scenarios, sourceApplication.Name);
            }
            else
            {
                _logger.LogDebug(TraceMessages.NoSendPortsFoundInApplication, RuleName, sourceApplication.Name);
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Analyzes the response path of a two way send port to build the route back to the topic channel.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="sourceApplication">The application object in the source model.</param>
        /// <param name="targetApplication">The application object in the target model.</param>
        /// <param name="sendPortSource">The send port source.</param>
        /// <param name="sendPort">The send port resource item.</param>
        /// <param name="endpointAdapter">The endpoint adapter which triggers the response.</param>
        private void AnalyzeSendPortResponse(string intermediaryKeyPrefix, ResourceItem sourceApplication, Application targetApplication, SendPort sendPortSource, ResourceItem sendPort, MessagingObject endpointAdapter)
        {
            _logger.LogDebug(TraceMessages.SendPortIsTwoWay, RuleName, sendPort.Name);

            // Format the keys.
            var applicationName = targetApplication.Name.FormatKey();
            var sendPortName = sendPort.Name.FormatKey();

            // Set the scenario name on the endpoint adapter.
            endpointAdapter.Properties.Add(ModelConstants.ScenarioName, $"{applicationName}.{sendPortName}.Response");

            var route = new List<MessagingObject>
            {
                endpointAdapter
            };

            // Create the intermediaries for the pipeline
            route.AddRange(CreateReceivePipelineIntermediaries(intermediaryKeyPrefix, sourceApplication, sendPortSource.ReceivePipeline, sendPortSource.ReceivePipelineCustomConfiguration));

            // If there is a map, create the intermediary
            if (sendPortSource.InboundTransforms != null && sendPortSource.InboundTransforms.Any())
            {
                // Find map resource items
                var transforms = sendPort.FindRelatedResourcesByType(Model, ResourceRelationshipType.ReferencesTo, ModelConstants.ResourceMap);

                // Add to route
                var intermediary = CreateMapIntermediary(intermediaryKeyPrefix, targetApplication, transforms);
                if (intermediary != null)
                {
                    route.Add(intermediary);
                }
            }
            else
            {
                _logger.LogTrace(TraceMessages.NoReceiveMapSpecifiedOnSendPort, RuleName, sendPortSource.Name);
            }

            // Create the message agent intermediaries
            route.AddRange(CreateMessageAgentIntermediaries(intermediaryKeyPrefix, _messageBoxChannelKey, false, null));

            // Binds the route by adding routing slip router intermediaries between the intermediaries in the response from the send port 
            var boundRoute = BindResponseRoute(intermediaryKeyPrefix, targetApplication, sendPortSource, route);

            // Binds the channels between the endpoint and intermediaries up to the message box (topic channel)
            BindResponseChannels(intermediaryKeyPrefix, targetApplication, sendPortSource, boundRoute);
        }

        /// <summary>
        /// Builds a new route for interchange batching.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="sourceApplication">The application in the source.</param>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="sendPort">The send port.</param>
        private void BuildInterchangeAggregationRoute(string intermediaryKeyPrefix, ResourceItem sourceApplication, Application targetApplication, SendPort sendPort)
        {
            _logger.LogDebug(TraceMessages.BuildingNewRouteForInterchangeBatching, RuleName);

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

                var scenarioName = $"{targetApplication.Name.FormatKey()}.{sendPort.Name.FormatKey()}.{ModelConstants.InterchangeAggregatorLeafKey}";
                aggregatorIntermediary.Properties.Add(ModelConstants.ScenarioName, scenarioName);
                aggregatorIntermediary.Properties.Add(ModelConstants.ScenarioStepName, "interchangeAggregator");
                aggregatorIntermediary.Properties.Add(ModelConstants.ConfigurationEntry, new Dictionary<string, object>());
                aggregatorIntermediary.Properties.Add(ModelConstants.RoutingProperties, new Dictionary<string, object>());

                // Hook aggregator up to interchange queue
                aggregatorIntermediary.InputChannelKeyRefs.Add(interchangeQueueChannelKey);

                route.Add(aggregatorIntermediary);

                // Create the send pipeline intermediaries
                route.AddRange(CreateSendPipelineIntermediaries(intermediaryKeyPrefix, sourceApplication, sendPort.TransmitPipeline, sendPort.SendPipelineCustomConfiguration));

                // Create endpoint adapter
                route.Add(CreateSendEndpoint(intermediaryKeyPrefix, sendPort));

                // Binds the route by adding routing slip router intermediaries between the intermediaries and the endpoint in the send port
                var boundRoute = BindRoute(intermediaryKeyPrefix, targetApplication, sendPort, route);

                // Binds the channels between the message box (topic channel), intermediaries up to the endpoint (send adapter)
                BindChannels(intermediaryKeyPrefix, interchangeQueueChannelKey, targetApplication, sendPort, boundRoute);
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, interchangeQueueChannelKey);
                _logger.LogError(error);
                Context.Errors.Add(new ErrorMessage(error));
            }
        }

        /// <summary>
        /// Creates a message subscriber for a send port.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the endpoint key.</param>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="sendPort">The send port.</param>
        /// <returns>The created message subscriber.</returns>
        private MessageSubscriber CreateMessageSubscriberIntermediary(string intermediaryKeyPrefix, Application targetApplication, SendPort sendPort)
        {
            _logger.LogDebug(TraceMessages.CreatingMessageSubscriberIntermediary, RuleName, MigrationTargetResources.MessageSubscriberName, sendPort.Name);

            // Format names
            var applicationName = targetApplication.Name.FormatKey();
            var sendPortName = sendPort.Name.FormatKey();

            // Create an intermediary based on the message subscriber
            var messageSubscriberIntermediary = new MessageSubscriber(sendPort.Name)
            {
                Description = MigrationTargetResources.MessageSubscriberDescription,
                Key = $"{intermediaryKeyPrefix}:{ModelConstants.MessageSubscriberLeafKey}",
                ResourceMapKey = $"topicSubscriber{applicationName.Replace(".", "-").Replace(" ", string.Empty).Replace(":", "-")}{sendPortName.Replace(".", "-").Replace("/", "-").Replace(":", "-")}",
                Rating = ConversionRating.FullConversion,
                Activator = true
            };

            // Set scenario and step name as properties on the endpoint
            messageSubscriberIntermediary.Properties.Add(ModelConstants.ScenarioName, $"{applicationName}.{sendPortName}");
            messageSubscriberIntermediary.Properties.Add(ModelConstants.ScenarioStepName, "messageSubscriber");

            // Set BizTalk specific configuration properties
            var configurationEntries = new Dictionary<string, object>()
            {
                { ModelConstants.BizTalkSpName, $"{sendPort.Name}" },
                { ModelConstants.BizTalkSpId, $"{targetApplication.Name.FormatKey()}.{sendPort.Name.FormatKey()}" },
                { ModelConstants.BizTalkAckSendPortName, $"{sendPort.Name}" },
                { ModelConstants.BizTalkAckSendPortId, $"{targetApplication.Name.FormatKey()}.{sendPort.Name.FormatKey()}" }
            };

            // Add configuration properties
            messageSubscriberIntermediary.Properties.Add(ModelConstants.ConfigurationEntry, configurationEntries);

            // Set BizTalk specific routing properties
            var routingProperties = new Dictionary<string, object>()
            {
                { ModelConstants.BizTalkAckSendPortName, $"{sendPortName}" }
            };

            // Add routing properties
            messageSubscriberIntermediary.Properties.Add(ModelConstants.RoutingProperties, routingProperties);
            
            // Add subscription filter
            CreateMessageSubscriberFilter(targetApplication, messageSubscriberIntermediary, sendPort);

            return messageSubscriberIntermediary;
        }

        /// <summary>
        /// Creates a subscription and filter on the message subscriber for the send port.
        /// </summary>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="messageSubscriber">The message subscriber.</param>
        /// <param name="sendPort">The send port.</param>
        private void CreateMessageSubscriberFilter(Application targetApplication, MessageSubscriber messageSubscriber, SendPort sendPort)
        {
            var topicChannelKey = _messageBoxChannelKey;
            var sendPortSubscriptionName = sendPort.Name.FormatKey();

            // Find topic channel to add subscriptions to
            var topicChannelObject = Model.FindMessagingObject(topicChannelKey);
            if (topicChannelObject.application != null && topicChannelObject.messagingObject != null)
            {
                var topicChannel = (TopicChannel)topicChannelObject.messagingObject;

                // Subscriptions will be created independent of subscribers so that messages can be delivered to
                // the subscriptions, meaning the subscription lifetime is not tied to the consumer.  This is a durable
                // subscription and the only ones we intend to create.
                messageSubscriber.IsDurable = true;

                // Create subscription and name it after the send port
                var subscription = new Subscription(sendPortSubscriptionName, topicChannel.TopicName)
                {
                    IsDurable = messageSubscriber.IsDurable
                };

                // Is there a subscription on the send port?
                if (sendPort.FilterExpression?.Group != null)
                {
                    // Parse the filter on the send port and create a new one for the target model
                    subscription.Filters.Add(CreateSubscriptionFilter(targetApplication, sendPort));
                }
                else
                {
                    // No filter, in these cases BizTalk will automatically set up a subscription with the
                    // property SPTransportID system property, which is used by orchestrations to automatically
                    // promote this property and associated value on a message published through an orchestration
                    // send port.
                    // There are also cases where there is a Filter document set on the property which has
                    // no groups and is empty (sendPort.FilterExpression.Groups is null).  Treat this as having
                    // no subscription.
                    subscription.Filters.Add(CreateDefaultSubscriptionFilter(targetApplication, sendPort));
                }

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
        /// Creates a default subscription filter for a send port.
        /// </summary>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="sendPort">The send port containing the filter to replicate.</param>
        /// <returns>A subscription filter.</returns>
        private SubscriptionFilter CreateDefaultSubscriptionFilter(Application targetApplication, SendPort sendPort)
        {
            _logger.LogDebug(TraceMessages.CreatingDefaultSendPortSubscription, RuleName, sendPort.Name);

            // Create filter expression
            string targetFilterExpression;
            if (sendPort.IsStatic)
            {
                targetFilterExpression = $"{ModelConstants.BizTalkSpTransportId} = '{targetApplication.Name.FormatKey()}.{sendPort.Name.FormatKey()}.{sendPort.PrimaryTransport.TransportType.Name}'";
            }
            else
            {
                // It's a dynamic port, so won't have a primary transport
                targetFilterExpression = $"{ModelConstants.BizTalkSpTransportId} = '{targetApplication.Name.FormatKey()}.{sendPort.Name.FormatKey()}'";
            }

            // Create a group, doesn't matter too much what the operation is as there is only one filter
            var targetFilterGroup = new OrFilterGroup();
            targetFilterGroup.Filters.Add(new Filter() { FilterExpression = targetFilterExpression });

            _logger.LogTrace(TraceMessages.CreatedSubscriptionFilterStatement, RuleName, targetFilterExpression);

            // Create filter and return
            var subscriptionFilter = new SubscriptionFilter(targetFilterGroup);

            _logger.LogTrace(TraceMessages.CreatedDefaultSubscriptionFilterGroupForSendPort, RuleName, targetFilterGroup.Operation, sendPort.Name);

            return subscriptionFilter;
        }

        /// <summary>
        /// Creates a default subscription filter for a send port.
        /// </summary>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="sendPort">The send port containing the filter to replicate.</param>
        /// <returns>A subscription filter.</returns>
        private SubscriptionFilter CreateSubscriptionFilter(Application targetApplication, SendPort sendPort)
        {
            _logger.LogDebug(TraceMessages.CreatingSendPortSubscription, RuleName, sendPort.Name);

            // Create an overall OR filter group so that if any sub-group matches a condition
            var targetFilterGroup = new OrFilterGroup();

            // Add default
            string defaultTargetFilterExpression;
            if (sendPort.IsStatic)
            {
                defaultTargetFilterExpression = $"{ModelConstants.BizTalkSpTransportId} = '{targetApplication.Name.FormatKey()}.{sendPort.Name.FormatKey()}.{sendPort.PrimaryTransport.TransportType.Name}'";
            }
            else
            {
                // It's a dynamic port, so won't have a primary transport
                defaultTargetFilterExpression = $"{ModelConstants.BizTalkSpTransportId} = '{targetApplication.Name.FormatKey()}.{sendPort.Name.FormatKey()}'";
            }

            targetFilterGroup.Groups.Add(new AndFilterGroup() { Filters = { new Filter() { FilterExpression = defaultTargetFilterExpression } } });

            // Loop around the filter groups, converting each one to an AND filter group with a list of filter expressions
            foreach (var sendPortFilterGroup in sendPort.FilterExpression.Group)
            {
                var targetFilterSubGroup = new AndFilterGroup();
                foreach (var statement in sendPortFilterGroup.Statement)
                {
                    Filter filter = null;
                    var expressionProperty = MapSubscriptionFilterProperty(statement.Property);

                    switch (statement.Operator)
                    {
                        // Equals
                        case 0:

                            filter = new Filter()
                            {
                                FilterExpression = $"{expressionProperty} = '{statement.Value}'"
                            };
                            break;

                        // LessThan
                        case 1:

                            filter = new Filter()
                            {
                                FilterExpression = $"{expressionProperty} < '{statement.Value}'"
                            };
                            break;

                        // LessThanEqualTo
                        case 2:

                            filter = new Filter()
                            {
                                FilterExpression = $"{expressionProperty} <= '{statement.Value}'"
                            };
                            break;

                        // GreaterThan
                        case 3:

                            filter = new Filter()
                            {
                                FilterExpression = $"{expressionProperty} > '{statement.Value}'"
                            };
                            break;

                        // GreaterThanEqualTo
                        case 4:

                            filter = new Filter()
                            {
                                FilterExpression = $"{expressionProperty} >= '{statement.Value}'"
                            };
                            break;

                        // NotEqual
                        case 5:

                            filter = new Filter()
                            {
                                FilterExpression = $"{expressionProperty} != '{statement.Value}'"
                            };
                            break;

                        // Exists
                        case 6:

                            filter = new Filter()
                            {
                                FilterExpression = $"EXISTS ( {expressionProperty} )"
                            };
                            break;

                        default:

                            _logger.LogError(ErrorMessages.SubscriptionFilterOperatorNotSupported, statement.Operator);
                            Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.SubscriptionFilterOperatorNotSupported, statement.Operator)));

                            continue;
                    }

                    targetFilterSubGroup.Filters.Add(filter);

                    _logger.LogTrace(TraceMessages.CreatedSubscriptionFilterStatement, RuleName, filter.FilterExpression);
                }

                _logger.LogTrace(TraceMessages.CreatedSubscriptionFilterSubGroup, RuleName, targetFilterSubGroup.Operation, targetFilterSubGroup.Filters.Count);

                // Add to filter group
                targetFilterGroup.Groups.Add(targetFilterSubGroup);
            }

            _logger.LogTrace(TraceMessages.CreatedSubscriptionFilterGroupForSendPort, RuleName, targetFilterGroup.Operation, sendPort.Name);

            // Create filter and return
            var subscriptionFilter = new SubscriptionFilter(targetFilterGroup);
            return subscriptionFilter;
        }

        /// <summary>
        /// Creates an endpoint adapter from a send port.
        /// </summary>
        /// <param name="endpointKeyPrefix">The prefix for the endpoint key.</param>
        /// <param name="sendPort">The send port.</param>
        /// <returns>The created endpoint adapter.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Used as routing slip config key.")]
        private MessagingObject CreateSendEndpoint(string endpointKeyPrefix, SendPort sendPort)
        {
            AdapterEndpoint endpointAdapter;

            // Is this a static or dynamic send port (that is, has a transport been set)?
            if (sendPort.IsStatic)
            {
                _logger.LogDebug(TraceMessages.CreatingSendEndpointAdapter, RuleName, sendPort.PrimaryTransport.TransportType.Name, sendPort.Name);

                // TODO: Use primary transport only, but need to think about how to handle backup transport

                // Create an endpoint adapter (assume request-reply or send, but some adapters may be fire and forget which will be changed later by a specific rule)
                endpointAdapter = new AdapterEndpoint(sendPort.Name, sendPort.PrimaryTransport.TransportType.Name);

                // Set step name as a property on the endpoint
                endpointAdapter.Properties.Add(ModelConstants.ScenarioStepName, $"{sendPort.PrimaryTransport.TransportType.Name.ToLowerInvariant()}SendAdapter");
            }
            else
            {
                _logger.LogDebug(TraceMessages.CreatingDynamicSendEndpointAdapter, RuleName, sendPort.Name);

                // Create an endpoint adapter (assume request-reply or send, but some adapters may be fire and forget which will be changed later by a specific rule)
                endpointAdapter = new AdapterEndpoint(sendPort.Name, MigrationTargetResources.DynamicSendPortDefaultProtocol);

                // Set step name as a property on the endpoint
                endpointAdapter.Properties.Add(ModelConstants.ScenarioStepName, $"{MigrationTargetResources.DynamicSendPortDefaultProtocol.ToLowerInvariant()}SendAdapter");

                // TODO: Handle dynamic send port - need to figure out how to find the actual procotol from an orchestration
            }

            // Set common property values
            endpointAdapter.Activator = false;
            endpointAdapter.Description = sendPort.Description;
            endpointAdapter.Key = $"{endpointKeyPrefix}:{ModelConstants.AdapterEndpointLeafKey}";
            endpointAdapter.MessageDeliveryGuarantee = MessageDeliveryGuarantee.AtLeastOnce;
            endpointAdapter.MessageExchangePattern = sendPort.IsTwoWay ? MessageExchangePattern.RequestReply : MessageExchangePattern.Send;

            // Set BizTalk specific configuration properties
            var configurationEntries = new Dictionary<string, object>()
            {
                { ModelConstants.IsTwoWay, sendPort.IsTwoWay }
            };

            // Add configuration properties
            endpointAdapter.Properties.Add(ModelConstants.ConfigurationEntry, configurationEntries);

            // Set BizTalk specific routing properties
            var routingProperties = new Dictionary<string, object>()
            {
                { ModelConstants.BizTalkSpName, ModelConstants.BizTalkSpName },
                { ModelConstants.BizTalkSpId, ModelConstants.BizTalkSpId }
            };

            // Add routing properties if two-way
            if (sendPort.IsTwoWay)
            {
                // Set BizTalk specific routing properties
                routingProperties.Add(ModelConstants.BizTalkAckReceivePortName, ModelConstants.BizTalkAckReceivePortName);
                routingProperties.Add(ModelConstants.BizTalkAckReceivePortId, ModelConstants.BizTalkAckReceivePortId);
                routingProperties.Add(ModelConstants.BizTalkAckSendPortName, ModelConstants.BizTalkAckSendPortName);
                routingProperties.Add(ModelConstants.BizTalkAckSendPortId, ModelConstants.BizTalkAckSendPortId);
                routingProperties.Add(ModelConstants.BizTalkAckInboundTransportLocation, ModelConstants.BizTalkAckInboundTransportLocation);
                routingProperties.Add(ModelConstants.BizTalkAckOutboundTransportLocation, ModelConstants.BizTalkAckOutboundTransportLocation);
                routingProperties.Add(ModelConstants.BizTalkAckFailureCategory, ModelConstants.BizTalkAckFailureCategory);
                routingProperties.Add(ModelConstants.BizTalkAckFailureCode, ModelConstants.BizTalkAckFailureCode);
                routingProperties.Add(ModelConstants.BizTalkAckId, ModelConstants.BizTalkAckId);
                routingProperties.Add(ModelConstants.BizTalkAckType, ModelConstants.BizTalkAckType);
            }

            // Add routing properties
            endpointAdapter.Properties.Add(ModelConstants.RoutingProperties, routingProperties);

            // TODO: Add schema references from source application

            // By default, this isn't convertible unless overridden by a specific rule
            endpointAdapter.Rating = ConversionRating.NoAutomaticConversion;

            return endpointAdapter;
        }

        /// <summary>
        /// Binds the route with the routing slip router between the intermediaries and endpoint.
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="sendPort">The send port.</param>
        /// <param name="route">The chain of messaging objects that need to be bound by routing slip router.</param>
        /// <returns>Returns the new route bound with routing slip router intermediaries.</returns>
        private IList<MessagingObject> BindRoute(string intermediaryKeyPrefix, Application targetApplication, SendPort sendPort, IList<MessagingObject> route)
        {
            _logger.LogDebug(TraceMessages.BindingRouteForSendPort, RuleName, sendPort.Name);

            var boundRoute = new List<MessagingObject>();

            // Add routing slip routers
            for (var i = 1; i < route.Count; i++)
            {
                // Determine from and to steps
                var fromStep = route[i - 1];
                var toStep = route[i];

                // Add step and then follow with a routing slip router intermediary
                boundRoute.Add(fromStep);
                boundRoute.Add(CreateRoutingSlipRouterIntermediary(intermediaryKeyPrefix, fromStep.Name, toStep.Name));

                // If at the end, add last step
                if (i == (route.Count - 1))
                {
                    boundRoute.Add(toStep);
                }
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
        /// Binds the route with the routing slip router between the intermediaries and endpoint (if present).
        /// </summary>
        /// <param name="intermediaryKeyPrefix">The prefix for the intermediary key.</param>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="sendPort">The send port.</param>
        /// <param name="route">The chain of messaging objects that need to be bound by routing slip router.</param>
        /// <returns>Returns the new route bound with routing slip router intermediaries.</returns>
        private IList<MessagingObject> BindResponseRoute(string intermediaryKeyPrefix, Application targetApplication, SendPort sendPort, IList<MessagingObject> route)
        {
            _logger.LogDebug(TraceMessages.BindingRouteForSendPortResponse, RuleName, sendPort.Name);

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
                if (!(step is Endpoint))
                {
                    targetApplication.Intermediaries.Add((Intermediary)step);
                }
            }

            return boundRoute;
        }

        /// <summary>
        /// Binds the endpoint and intermediaries together.
        /// </summary>
        /// <param name="channelKeyPrefix">The prefix for the channel key.</param>
        /// <param name="activatorChannelKey">The key of the channel that hooks up to the activator intermediary.</param>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="sendPort">The send port.</param>
        /// <param name="route">The chain of messaging objects that need to be bound by channels.</param>
        private void BindChannels(string channelKeyPrefix, string activatorChannelKey, Application targetApplication, SendPort sendPort, IList<MessagingObject> route)
        {
            _logger.LogDebug(TraceMessages.BindingChannelsForSendPort, RuleName, sendPort.Name);

            // Find topic channel to hook up the route
            var activatorChannel = Model.FindMessagingObject(activatorChannelKey);
            if (activatorChannel.application != null && activatorChannel.messagingObject != null)
            {
                // First object in route should be an activating intermediary, hook it up to the activator channel (if it's not an activator, it's a bug!)
                var activatorIntermediary = (Intermediary)route.First();
                if (activatorIntermediary.Activator)
                {
                    activatorIntermediary.InputChannelKeyRefs.Add(activatorChannel.messagingObject.Key);

                    // Assume route is in order, that is, first entry is the start of the route.  As the route is built
                    // in this class, not expecting anything other than intermediaries and an endpoint.
                    for (var i = 0; i < route.Count - 1; i++)
                    {
                        // Determine from and to steps
                        var fromStep = route[i];
                        var toStep = route[i + 1];

                        // Create channel
                        var channel = new TriggerChannel(MigrationTargetResources.TriggerChannelName)
                        {
                            Description = string.Format(CultureInfo.CurrentCulture, MigrationTargetResources.TriggerChannelDescription, toStep.Name),
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
                        ((Intermediary)fromStep).OutputChannelKeyRefs.Add(channel.Key);

                        if (toStep is Endpoint toEndpoint)
                        {
                            toEndpoint.InputChannelKeyRef = channel.Key;
                        }

                        if (toStep is Intermediary toIntermediary)
                        {
                            toIntermediary.InputChannelKeyRefs.Add(channel.Key);
                        }
                    }
                }
                else
                {
                    var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.SendRouteStartNotAnActivator, activatorChannel.messagingObject.Name);
                    _logger.LogError(error);
                    Context.Errors.Add(new ErrorMessage(error));
                }
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithKeyInTargetModel, MessagingObjectType.Channel, activatorChannelKey);
                _logger.LogError(error);
                Context.Errors.Add(new ErrorMessage(error));
            }
        }

        /// <summary>
        /// Binds the intermediaries and endpoint together.
        /// </summary>
        /// <param name="channelKeyPrefix">The prefix for the channel key.</param>
        /// <param name="targetApplication">The application in the target.</param>
        /// <param name="sendPort">The send port.</param>
        /// <param name="route">The chain of messaging objects that need to be bound by channels.</param>
        private void BindResponseChannels(string channelKeyPrefix, Application targetApplication, SendPort sendPort, IList<MessagingObject> route)
        {
            _logger.LogDebug(TraceMessages.BindingChannelsForReceivePort, RuleName, sendPort.Name);

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
