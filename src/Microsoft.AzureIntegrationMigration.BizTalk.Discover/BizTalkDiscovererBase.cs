// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="BizTalkDiscovererBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Base class for the BizTalk Discoverers.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.Discover.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Discover
{
    /// <summary>
    /// Base class for the BizTalk Discoverers.
    /// </summary>
    public abstract class BizTalkDiscovererBase : IBizTalkDiscoverer
    {
        /// <summary>
        /// Defines the name of the discoverer.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the application model.
        /// </summary>
        private readonly AzureIntegrationServicesModel _model;

        /// <summary>
        /// Defines the context used to gather the migration information.
        /// </summary>
        private readonly MigrationContext _context;

        /// <summary>
        /// Creates a new instance of the <see cref="BizTalkDiscovererBase" class./>
        /// </summary>
        /// <param name="discovererName">The name of the BizTalk discoverer.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">The logger.</param>
        public BizTalkDiscovererBase(string discovererName, IApplicationModel model, MigrationContext context, ILogger logger)
        {
            // Validate and set the members
            _name = discovererName ?? throw new ArgumentNullException(nameof(discovererName));
            _model = (AzureIntegrationServicesModel)model ?? throw new ArgumentNullException(nameof(model));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Discovers the contents of the model.
        /// </summary>
        public void Discover()
        {
            _logger.LogInformation(InformationMessages.RunningBizTalkDiscoverer, Name);

            DiscoverInternal(_model, _context);
        }

        /// <summary>
        /// Gets the name of the BizTalk discoverers.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Performs the internal work of the discoverer after null checking.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        protected abstract void DiscoverInternal(AzureIntegrationServicesModel model, MigrationContext context);
    }
}
