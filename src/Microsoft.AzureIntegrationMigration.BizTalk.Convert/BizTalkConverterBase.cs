// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert
{
    /// <summary>
    /// Defines a base class for a converter.
    /// </summary>
    public abstract class BizTalkConverterBase : IBizTalkConverter
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of the converter.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Default constructor for dependency injection.
        /// </summary>
        /// <param name="converterName">The name of the converter.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="logger">An instance of a <see cref="ILogger"/> to be used for logging within the class.</param>
        protected BizTalkConverterBase(string converterName, IApplicationModel model, MigrationContext context, ILogger logger)
        {
            // Validate and set the members.
            _name = converterName ?? throw new ArgumentNullException(nameof(converterName));
            Model = (AzureIntegrationServicesModel)model ?? throw new ArgumentNullException(nameof(model));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets or sets the <see cref="MigrationContext"/>.
        /// </summary>
        protected MigrationContext Context { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AzureIntegrationServicesModel"/>.
        /// </summary>
        protected AzureIntegrationServicesModel Model { get; set; }

        /// <summary>
        /// Runs the converter component.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        public async Task ConvertAsync(CancellationToken token)
        {
            _logger.LogInformation(InformationMessages.RunningBizTalkConverter, Name);

            await ConvertInternalAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the name of the BizTalk converter.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Performs the work of conversion.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected abstract Task ConvertInternalAsync(CancellationToken token);
    }
}
