// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Resources;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.StageRunners
{
    /// <summary>
    /// Defines a model provider.
    /// </summary>
    public class AzureIntegrationServicesModelProvider : IApplicationModelProvider
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureIntegrationServicesModelProvider"/> class with the name
        /// of the plugin, priority and a generic logger.
        /// </summary>
        /// <param name="pluginName">The name of the plugin.</param>
        /// <param name="stage">The stage.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="logger">A logger.</param>
        public AzureIntegrationServicesModelProvider(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region IApplicationModelProvider Interface Implementation

        /// <summary>
        /// Returns a new instance of the <see cref="AzureIntegrationServicesModel"/> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="AzureIntegrationServicesModel"/> class.</returns>
        public IApplicationModel GetModel()
        {
            _logger.LogDebug(TraceMessages.RunningModelProvider);

            return new AzureIntegrationServicesModel();
        }

        #endregion
    }
}
