// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert
{
    /// <summary>
    /// Defines an interface for objects which can walk the route of a scenario.
    /// </summary>
    public interface IScenarioRouteWalker
    {
        /// <summary>
        /// Walks the process manager route.
        /// </summary>
        /// <param name="rule">The name of the rule walking the route.</param>
        /// <param name="scenario">The name of the scenario representing the route.</param>
        /// <param name="activatingIntermediary">The intermediary which starts the route.</param>
        /// <param name="intermediaries">The intermediaries to check if they are in the route.</param>
        /// <param name="channels">The channels to check if they are in the route.</param>
        /// <returns>A list of messaging objects and the channel used to get them in the route.</returns>
        IList<(MessagingObject RoutingObject, Channel InputChannel)> WalkProcessManagerRoute(string rule, string scenario, Intermediary activatingIntermediary, IEnumerable<Intermediary> intermediaries, IEnumerable<Channel> channels);

        /// <summary>
        /// Walks the receive route.
        /// </summary>
        /// <param name="rule">The name of the rule walking the route.</param>
        /// <param name="scenario">The name of the scenario representing the route.</param>
        /// <param name="initiatingEndpoint">The endpoint which starts the route.</param>
        /// <param name="intermediaries">The intermediaries to check if they are in the route.</param>
        /// <param name="channels">The channels to check if they are in the route.</param>
        /// <returns>A list of messaging objects and the channel used to get them in the route.</returns>
        IList<(MessagingObject RoutingObject, Channel InputChannel)> WalkReceiveRoute(string rule, string scenario, Endpoint initiatingEndpoint, IEnumerable<Intermediary> intermediaries, IEnumerable<Channel> channels);

        /// <summary>
        /// Walks the send route.
        /// </summary>
        /// <param name="rule">The name of the rule walking the route.</param>
        /// <param name="scenario">The name of the scenario representing the route.</param>
        /// <param name="activatingIntermediary">The intermediary which starts the router.</param>
        /// <param name="intermediaries">The intermediaries to check if they are in the route.</param>
        /// <param name="channels">The channels to check if they are in the route.</param>
        /// <param name="endpoints">The endpoints to check if they are in the route.</param>
        /// <returns>A list of messaging objects and their input channel for each step in the route.</returns>
        IList<(MessagingObject RoutingObject, Channel InputChannel)> WalkSendRoute(string rule, string scenario, Intermediary activatingIntermediary, IEnumerable<Intermediary> intermediaries, IEnumerable<Channel> channels, IEnumerable<Endpoint> endpoints);        
    }
}
