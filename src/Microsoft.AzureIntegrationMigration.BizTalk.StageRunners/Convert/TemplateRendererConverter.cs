// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze;
using Microsoft.AzureIntegrationMigration.BizTalk.Discover.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Convert
{
    /// <summary>
    /// Stage runner for generating the target resources in the target model.
    /// </summary>
    public class TemplateRendererConverter : StageRunnerBase, IStageConverter
    {
        /// <summary>
        /// Defines the name of this stage runner.
        /// </summary>
        public const string PluginName = ModelConstants.ApplicationPrefix + "templaterenderer";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines a list of directory containing template config files.
        /// </summary>
        private readonly List<string> _templatePaths = new List<string>();

        /// <summary>
        /// Defines a directory to store converted output.
        /// </summary>
        private string _conversionPath;

        /// <summary>
        /// Defines a directory to store generated output.
        /// </summary>
        private string _generationPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateRendererConverter"/> class with a generic logger.
        /// </summary>
        /// <param name="logger">A logger.</param>
        public TemplateRendererConverter(ILogger logger)
            : base(PluginName, Stages.Convert, 20, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Runs the stage runner.
        /// </summary>
        /// <param name="state">The execution state.</param>
        /// <param name="token">A cancellation token used to cancel this operation.</param>
        /// <returns>A task used to await the operation.</returns>
        protected override async Task InvokeRunAsync(IRunState state, CancellationToken token)
        {
            _ = state ?? throw new ArgumentNullException(nameof(state));

            _logger.LogTrace(TraceMessages.RunningTemplateRendererConverter);

            // Default conversion directory if not specified
            if (string.IsNullOrWhiteSpace(_conversionPath))
            {
                _conversionPath = Environment.CurrentDirectory;
            }

            // Default generation directory if not specified
            if (string.IsNullOrWhiteSpace(_generationPath))
            {
                _generationPath = Path.Combine(_conversionPath, ArtifactGeneratorConverter.DefaultGenerationPath);
            }

            // Get context
            var context = Container.GetRequiredService<MigrationContext>();
            context.ConversionFolder = _conversionPath;
            context.GenerationFolder = _generationPath;
            _templatePaths.Add(_generationPath);
            _templatePaths.ForEach(p => context.TemplateFolders.Add(p));

            // Render template
            var converter = Container.GetRequiredService<BizTalk.Convert.TemplateRendererConverter>();
            await converter.ConvertAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets arguments for this stage runner.
        /// </summary>
        protected override void GetArgs()
        {
            // Conversion Directory
            var argConversionDir = GetArg<string>(ArtifactGeneratorConverter.ArgConversionPath);
            if (!string.IsNullOrWhiteSpace(argConversionDir))
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArtifactGeneratorConverter.ArgConversionPath, argConversionDir);

                _conversionPath = new DirectoryInfo(argConversionDir).FullName;
            }

            // Generation Directory
            var argGenerationDir = GetArg<string>(ArtifactGeneratorConverter.ArgGenerationPath);
            if (!string.IsNullOrWhiteSpace(argGenerationDir))
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArtifactGeneratorConverter.ArgGenerationPath, argGenerationDir);

                _generationPath = new DirectoryInfo(argGenerationDir).FullName;
            }

            // Template Directories
            var argValue = GetArg(ArtifactGeneratorConverter.ArgTemplatePaths);
            if (argValue != null)
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArtifactGeneratorConverter.ArgTemplatePaths, argValue);

                // Should be an array of directories
                foreach (var dir in (string[])argValue)
                {
                    _templatePaths.Add(dir);
                }
            }
        }

        /// <summary>
        /// Validates the arguments found on the command line.
        /// </summary>
        protected override bool ValidateArgs()
        {
            var valid = true;

            // Template Directories
            _templatePaths.ForEach(p =>
            {
                var dirInfo = new DirectoryInfo(p);
                if (!dirInfo.Exists)
                {
                    _logger.LogError(ErrorMessages.TemplateDirectoryDoesNotExist, dirInfo.FullName);
                    valid = false;
                }
            });

            return valid;
        }
    }
}
