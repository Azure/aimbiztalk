// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Generator;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze
{
    /// <summary>
    /// Defines a class that implements a resource generator that uses configuration to generate target resources
    /// in the target model.
    /// </summary>
    public sealed class ResourceGeneratorAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines a configuration repository.
        /// </summary>
        private readonly IConfigurationRepository _repository;

        /// <summary>
        /// Defines a resource generator.
        /// </summary>
        private readonly IResourceGenerator _generator;

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="ResourceGeneratorAnalyzer"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public ResourceGeneratorAnalyzer(IConfigurationRepository repository, IResourceGenerator generator, IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(ResourceGeneratorAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates target resources from config using the target model.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get temp location for rendered template configuration
            var tempPath = Path.Combine(Path.GetTempPath(), "aim-config");

            _logger.LogDebug(TraceMessages.RenderingTemplateConfigInTempPath, tempPath);

            // Render the Liquid configuration first into a new temp location
            await _repository.RenderConfigurationAsync(Model, Context.TemplateConfigFolder, tempPath).ConfigureAwait(false);

            _logger.LogTrace(TraceMessages.RetrievingConfigurationFromTempPath, tempPath);

            // Get configuration from temp path
            var config = _repository.GetConfiguration(tempPath);

            _logger.LogDebug(TraceMessages.GeneratingResourcesInTargetModel);

            // Generate resources in target model using rendered configuration
            await _generator.GenerateResourcesAsync(Model, config, token).ConfigureAwait(false);
        }
    }
}
