using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert
{
    /// <summary>
    /// Defines a class to walk scenario routes.
    /// </summary>
    public class ScenarioRouteWalker : IScenarioRouteWalker
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the context.
        /// </summary>
        private readonly MigrationContext _context;

        /// <summary>
        /// Constructs an instance of the <see cref="ScenarioRouteWalker"/> class.
        /// </summary>
        /// <param name="context">The migration context.</param>
        /// <param name="logger">The logger.</param>
        public ScenarioRouteWalker(MigrationContext context, ILogger logger)
        {
            // Validate and set the members.
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Walks the process manager route.
        /// </summary>
        /// <param name="rule">The name of the rule walking the route.</param>
        /// <param name="scenario">The name of the scenario representing the route.</param>
        /// <param name="activatingIntermediary">The intermediary which starts the route.</param>
        /// <param name="intermediaries">The intermediaries to check if they are in the route.</param>
        /// <param name="channels">The channels to check if they are in the route.</param>
        /// <returns>A list of messaging objects and the channel used to get them in the route.</returns>
        public IList<(MessagingObject RoutingObject, Channel InputChannel)> WalkProcessManagerRoute(string rule, string scenario, Intermediary activatingIntermediary, IEnumerable<Intermediary> intermediaries, IEnumerable<Channel> channels)
        {
            _ = activatingIntermediary ?? throw new ArgumentNullException(nameof(activatingIntermediary));

            _logger.LogDebug(TraceMessages.WalkingOrchestrationRoute, rule, scenario, activatingIntermediary.Name);

            var route = new List<(MessagingObject, Channel)>
            {
                // Add the activating intermediary to the route.
                (activatingIntermediary, null)
            };

            // Walk the rest of the route.
            route.AddRange(WalkProcessManagerRouteRecursively(rule, scenario, intermediaries, channels, activatingIntermediary.OutputChannelKeyRefs));

            return route;
        }

        /// <summary>
        /// Walks the receive route.
        /// </summary>
        /// <param name="rule">The name of the rule walking the route.</param>
        /// <param name="scenario">The name of the scenario representing the route.</param>
        /// <param name="initiatingEndpoint">The endpoint which starts the route.</param>
        /// <param name="intermediaries">The intermediaries to check if they are in the route.</param>
        /// <param name="channels">The channels to check if they are in the route.</param>
        /// <returns>A list of messaging objects and the channel used to get them in the route.</returns>
        public IList<(MessagingObject RoutingObject, Channel InputChannel)> WalkReceiveRoute(string rule, string scenario, Endpoint initiatingEndpoint, IEnumerable<Intermediary> intermediaries, IEnumerable<Channel> channels)
        {
            _ = initiatingEndpoint ?? throw new ArgumentNullException(nameof(initiatingEndpoint));

            _logger.LogDebug(TraceMessages.WalkingReceiveRoute, rule, scenario, initiatingEndpoint.Name);

            var route = new List<(MessagingObject, Channel)>
            {
                // Add the initiating endpoint to the route.
                (initiatingEndpoint, null)
            };

            // Walk the rest of the route.
            route.AddRange(WalkReceiveRouteRecursively(rule, scenario, intermediaries, channels, initiatingEndpoint.OutputChannelKeyRef));

            return route;
        }

        /// <summary>
        /// Walks the send route.
        /// </summary>
        /// <param name="rule">The name of the rule walking the route.</param>
        /// <param name="scenario">The name of the scenario representing the route.</param>
        /// <param name="activatingIntermediary">The intermediary which starts the route.</param>
        /// <param name="intermediaries">The intermediaries to check if they are in the route.</param>
        /// <param name="channels">The channels to check if they are in the route.</param>
        /// <param name="endpoints">The endpoints to check if they are in the route.</param>
        /// <returns>A list of messaging objects and their input channel for each step in the route.</returns>
        public IList<(MessagingObject RoutingObject, Channel InputChannel)> WalkSendRoute(string rule, string scenario, Intermediary activatingIntermediary, IEnumerable<Intermediary> intermediaries, IEnumerable<Channel> channels, IEnumerable<Endpoint> endpoints)
        {
            _ = activatingIntermediary ?? throw new ArgumentNullException(nameof(activatingIntermediary));

            _logger.LogDebug(TraceMessages.WalkingSendRoute, rule, scenario, activatingIntermediary.Name);

            var route = new List<(MessagingObject, Channel)>
            {
                // Add the activating intermediary to the route.
                (activatingIntermediary, null)
            };

            // Walk the rest of the route.
            route.AddRange(WalkSendRouteRecursively(rule, scenario, intermediaries, channels, endpoints, activatingIntermediary.OutputChannelKeyRefs));

            return route;
        }

        /// <summary>
        /// Walks the process manager route recursively.
        /// </summary>
        /// <param name="rule">The name of the rule walking the route.</param>
        /// <param name="scenario">The name of the scenario representing the route.</param>
        /// <param name="intermediaries">The intermediaries to search.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="channelKeyRefs">The channel keys to follow the route for the next routing object.</param>
        /// <returns>The discovered route detailing each messaging object and input channel in the route channels.</returns>
        private IList<(MessagingObject RoutingObject, Channel InputChannel)> WalkProcessManagerRouteRecursively(string rule, string scenario, IEnumerable<Intermediary> intermediaries, IEnumerable<Channel> channels, IList<string> channelKeyRefs)
        {
            var discoveredRoutingObjects = new List<(MessagingObject, Channel)>();

            _logger.LogTrace(TraceMessages.CheckingInputChannels, rule, channelKeyRefs);

            // Loop through all of the channel keys.
            foreach (var channelKeyRef in channelKeyRefs)
            {
                var discoveredChannel = channels.FirstOrDefault(c => c.Key == channelKeyRef);
                if (discoveredChannel != null)
                {
                    // Only follow channels that are part of the route.
                    if (discoveredChannel.Properties.ContainsKey(ModelConstants.RouteLabel))
                    {
                        // Check if the next channel points to an intermediary.
                        var discoveredIntermediary = intermediaries.FirstOrDefault(i => i.InputChannelKeyRefs.Contains(channelKeyRef));
                        if (discoveredIntermediary != null)
                        {
                            _logger.LogTrace(TraceMessages.FoundIntermediaryAttachedToChannel, rule, discoveredIntermediary.Name, discoveredChannel.Name);

                            // Add the intermediary and carry on the route search.
                            discoveredRoutingObjects.Add((discoveredIntermediary, discoveredChannel));
                            discoveredRoutingObjects.AddRange(WalkProcessManagerRouteRecursively(rule, scenario, intermediaries, channels, discoveredIntermediary.OutputChannelKeyRefs));
                        }
                        else
                        {

                            _logger.LogError(ErrorMessages.UnableToFindMessagingObjectWithAnInputChannelInTargetModel, channelKeyRef);
                            _context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithAnInputChannelInTargetModel, channelKeyRef)));
                        }                    }
                    else
                    {
                        _logger.LogDebug(TraceMessages.IgnoringChannel, rule, discoveredChannel.Name);
                    }
                }
                else
                {
                    _logger.LogError(ErrorMessages.UnableToFindAChannelWithTheKeyInTargetModel, channelKeyRef);
                    _context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAChannelWithTheKeyInTargetModel, channelKeyRef)));
                }
            }

            return discoveredRoutingObjects;
        }

        /// <summary>
        /// Walks the receive route recursively.
        /// </summary>
        /// <param name="rule">The name of the rule walking the route.</param>
        /// <param name="scenario">The name of the scenario representing the route.</param>
        /// <param name="intermediaries">The intermediaries to search.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="inputChannelKeyRef">The channel key for the next object in the route.</param>
        /// <returns>The discovered route detailing each messaging object and input channel in the route channels.</returns>
        private IList<(MessagingObject RoutingObject, Channel InputChannel)> WalkReceiveRouteRecursively(string rule, string scenario, IEnumerable<Intermediary> intermediaries, IEnumerable<Channel> channels, string inputChannelKeyRef)
        {
            var discoveredIntermediaries = new List<(MessagingObject, Channel)>();

            _logger.LogTrace(TraceMessages.CheckingInputChannels, rule, inputChannelKeyRef);

            // Get the channel for the intermediary.
            var discoveredChannel = channels.FirstOrDefault(c => c.Key == inputChannelKeyRef);
            if (discoveredChannel != null)
            {
                // Only follow channels that are part of the route.
                if (discoveredChannel.Properties.ContainsKey(ModelConstants.RouteLabel))
                {
                    var discoveredIntermediary = intermediaries.FirstOrDefault(i => i.InputChannelKeyRefs.Contains(inputChannelKeyRef));
                    if (discoveredIntermediary != null)
                    {
                        _logger.LogTrace(TraceMessages.FoundIntermediaryAttachedToChannel, rule, discoveredIntermediary.Name, discoveredChannel.Name);

                        discoveredIntermediaries.Add((discoveredIntermediary, discoveredChannel));

                        // Loop through all of the output channels.
                        foreach (var outputChannelRef in discoveredIntermediary.OutputChannelKeyRefs)
                        {
                            discoveredIntermediaries.AddRange(WalkReceiveRouteRecursively(rule, scenario, intermediaries, channels, outputChannelRef));
                        }
                    }
                    else
                    {
                        _logger.LogError(ErrorMessages.UnableToFindAnIntermediaryWithTheKeyInTargetModel, inputChannelKeyRef);
                        _context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAnIntermediaryWithTheKeyInTargetModel, inputChannelKeyRef)));
                    }
                }
                else
                {
                    _logger.LogDebug(TraceMessages.IgnoringChannel, rule, discoveredChannel.Name);
                }
            }
            else
            {
                _logger.LogError(ErrorMessages.UnableToFindAChannelWithTheKeyInTargetModel, inputChannelKeyRef);
                _context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAChannelWithTheKeyInTargetModel, inputChannelKeyRef)));
            }

            return discoveredIntermediaries;
        }

        /// <summary>
        /// Walks the send route recursively.
        /// </summary>
        /// <param name="rule">The name of the rule walking the route.</param>
        /// <param name="scenario">The name of the scenario representing the route.</param>
        /// <param name="intermediaries">The intermediaries to search.</param>
        /// <param name="channels">The channels to search.</param>
        /// <param name="endpoints">The endpoints to search.</param>
        /// <param name="channelKeyRefs">The channel keys to follow the route for the next routing object.</param>
        /// <returns>The discovered routing objects.</returns>
        private IList<(MessagingObject RoutingObject, Channel InputChannel)> WalkSendRouteRecursively(string rule, string scenario, IEnumerable<Intermediary> intermediaries, IEnumerable<Channel> channels, IEnumerable<Endpoint> endpoints, IList<string> channelKeyRefs)
        {
            var discoveredRoutingObjects = new List<(MessagingObject, Channel)>();

            _logger.LogTrace(TraceMessages.CheckingInputChannels, rule, channelKeyRefs);

            // Loop through all of the channel keys.
            foreach (var channelKeyRef in channelKeyRefs)
            {
                var discoveredChannel = channels.FirstOrDefault(c => c.Key == channelKeyRef);
                if (discoveredChannel != null)
                {
                    // Only follow channels that are part of the route.
                    if (discoveredChannel.Properties.ContainsKey(ModelConstants.RouteLabel))
                    {
                        // Check if the next channel points to an intermediary.
                        var discoveredIntermediary = intermediaries.FirstOrDefault(i => i.InputChannelKeyRefs.Contains(channelKeyRef));
                        if (discoveredIntermediary != null)
                        {
                            _logger.LogTrace(TraceMessages.FoundIntermediaryAttachedToChannel, rule, discoveredIntermediary.Name, discoveredChannel.Name);

                            // Add the intermediary and carry on the route search.
                            discoveredRoutingObjects.Add((discoveredIntermediary, discoveredChannel));
                            discoveredRoutingObjects.AddRange(WalkSendRouteRecursively(rule, scenario, intermediaries, channels, endpoints, discoveredIntermediary.OutputChannelKeyRefs));
                        }
                        else
                        {
                            // Check to see if the channel points to an endpoint.
                            var discoveredEndpoint = endpoints.FirstOrDefault(e => e.InputChannelKeyRef == channelKeyRef);
                            if (discoveredEndpoint != null)
                            {
                                _logger.LogTrace(TraceMessages.FoundEndpointAttachedToChannel, rule, discoveredEndpoint.Name, discoveredChannel.Name);

                                // This is the end of this route, stop searching the branch.
                                discoveredRoutingObjects.Add((discoveredEndpoint, discoveredChannel));
                            }
                            else
                            {
                                _logger.LogError(ErrorMessages.UnableToFindMessagingObjectWithAnInputChannelInTargetModel, channelKeyRef);
                                _context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindMessagingObjectWithAnInputChannelInTargetModel, channelKeyRef)));
                            }
                        }
                    }
                    else
                    {
                        _logger.LogDebug(TraceMessages.IgnoringChannel, rule, discoveredChannel.Name);
                    }
                }
                else
                {
                    _logger.LogError(ErrorMessages.UnableToFindAChannelWithTheKeyInTargetModel, channelKeyRef);
                    _context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAChannelWithTheKeyInTargetModel, channelKeyRef)));
                }
            }

            return discoveredRoutingObjects;
        }
    }
}
