using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Generator;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Renderer;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert
{
    /// <summary>
    /// Defines a class that implements a converter that renders templates.
    /// </summary>
    public sealed class TemplateRendererConverter : BizTalkConverterBase
    {
        /// <summary>
        /// Defines a file repository.
        /// </summary>
        private readonly IFileRepository _fileRepository;

        /// <summary>
        /// Defines a template repository.
        /// </summary>
        private readonly ITemplateRepository _repository;

        /// <summary>
        /// Defines a template renderer.
        /// </summary>
        private readonly ITemplateRenderer _renderer;

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="TemplateRendererConverter"/> class.
        /// </summary>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="renderer">The renderer.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public TemplateRendererConverter(IFileRepository fileRepository, ITemplateRepository repository, ITemplateRenderer renderer, IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(TemplateRendererConverter), model, context, logger)
        {
            // Validate and set the member.
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Renders templates specified in the target resources of messaging objects in the target model.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task ConvertInternalAsync(CancellationToken token)
        {
            // Ensure conversion path exists
            var conversionPath = new DirectoryInfo(Context.ConversionFolder);
            if (!_fileRepository.DoesDirectoryExist(conversionPath.FullName))
            {
                _logger.LogDebug(TraceMessages.CreatingConversionPath, conversionPath.FullName);

                _fileRepository.CreateDirectory(conversionPath.FullName);
            }

            // Get template paths
            var templatePaths = Context.TemplateFolders.Select(p => new DirectoryInfo(p));
            if (templatePaths.Any())
            {
                // Render templates
                var generatedFiles = 0;
                var messageBus = Model.MigrationTarget.MessageBus;
                if (messageBus != null)
                {
                    // Message Bus
                    generatedFiles += await GenerateFilesAsync(messageBus, templatePaths, conversionPath).ConfigureAwait(false);

                    // Applications
                    if (messageBus.Applications.Any())
                    {
                        foreach (var application in messageBus.Applications)
                        {
                            generatedFiles += await GenerateFilesAsync(application, templatePaths, conversionPath).ConfigureAwait(false);

                            // Messages
                            if (application.Messages.Any())
                            {
                                foreach (var message in application.Messages)
                                {
                                    generatedFiles += await GenerateFilesAsync(message, templatePaths, conversionPath).ConfigureAwait(false);
                                }
                            }

                            // Channels
                            if (application.Channels.Any())
                            {
                                foreach (var channel in application.Channels)
                                {
                                    generatedFiles += await GenerateFilesAsync(channel, templatePaths, conversionPath).ConfigureAwait(false);
                                }
                            }

                            // Intermediaries
                            if (application.Intermediaries.Any())
                            {
                                foreach (var intermediary in application.Intermediaries)
                                {
                                    generatedFiles += await GenerateFilesAsync(intermediary, templatePaths, conversionPath).ConfigureAwait(false);
                                }
                            }

                            // Endpoints
                            if (application.Endpoints.Any())
                            {
                                foreach (var endpoint in application.Endpoints)
                                {
                                    generatedFiles += await GenerateFilesAsync(endpoint, templatePaths, conversionPath).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }

                if (generatedFiles == 0)
                {
                    _logger.LogInformation(InformationMessages.NoResourceTemplatesToConvert);
                }
                else
                {
                    _logger.LogInformation(InformationMessages.GeneratedResourceTemplateFiles, generatedFiles);
                }
            }
            else
            {
                _logger.LogWarning(WarningMessages.NoTemplatePathsFound);
            }
        }

        /// <summary>
        /// Generates the files associated with a list of resource templates.
        /// </summary>
        /// <param name="messagingObject">The messaging object associated with the list of resource templates.</param>
        /// <param name="templatePaths">The list of source template paths.</param>
        /// <param name="conversionPathRoot">The root path for all converted files.</param>
        /// <returns></returns>
        private async Task<int> GenerateFilesAsync(MessagingObject messagingObject, IEnumerable<DirectoryInfo> templatePaths, DirectoryInfo conversionPathRoot)
        {
            var generatedFiles = 0;

            if (messagingObject.Resources.Any())
            {
                foreach (var resourceTemplate in messagingObject.Resources)
                {
                    foreach (var templateFile in resourceTemplate.ResourceTemplateFiles)
                    {
                        var foundTemplate = await GenerateFileAsync(messagingObject, resourceTemplate, templatePaths, templateFile, resourceTemplate.OutputPath, conversionPathRoot).ConfigureAwait(false);
                        if (!foundTemplate)
                        {
                            _logger.LogWarning(WarningMessages.TemplateFileNotFound, templateFile);
                        }
                        else
                        {
                            generatedFiles++;
                        }
                    }
                }
            }

            return generatedFiles;
        }

        /// <summary>
        /// Generates a template file by loading, rendering and saving the file to the conversion path
        /// if it is a .liquid file, otherwise just copy the file.
        /// </summary>
        /// <param name="messagingObject">The messaging object associated with the list of resource templates.</param>
        /// <param name="resourceTemplate">The resource template for this render.</param>
        /// <param name="templatePaths">The list of source template paths.</param>
        /// <param name="templateFile">The template file.</param>
        /// <param name="templateOutputPath">The output path for template files.</param>
        /// <param name="conversionPathRoot">The root path for all converted files.</param>
        /// <returns></returns>
        private async Task<bool> GenerateFileAsync(MessagingObject messagingObject, TargetResourceTemplate resourceTemplate, IEnumerable<DirectoryInfo> templatePaths, string templateFile, string templateOutputPath, DirectoryInfo conversionPathRoot)
        {
            var foundTemplate = false;

            foreach (var templatePath in templatePaths)
            {
                var templateFilePath = new FileInfo(Path.Combine(templatePath.FullName, templateFile));
                if (_fileRepository.DoesFileExist(templateFilePath.FullName))
                {
                    // Check extension
                    if (templateFilePath.Extension.ToUpperInvariant() == ".liquid".ToUpperInvariant())
                    {
                        _logger.LogTrace(TraceMessages.LoadingTemplate, templateFilePath.FullName);

                        // Load template
                        var templateContent = await _repository.LoadTemplateAsync(templateFilePath.FullName).ConfigureAwait(false);

                        _logger.LogDebug(TraceMessages.RenderingTemplate, templateFilePath.FullName);

                        // Render template
                        var renderedContent = await _renderer.RenderTemplateAsync(templateContent, Model, messagingObject, resourceTemplate).ConfigureAwait(false);

                        // Set output file path
                        var outputFilePath = new FileInfo(Path.Combine(conversionPathRoot.FullName, templateOutputPath, Path.GetFileNameWithoutExtension(templateFilePath.Name)));

                        _logger.LogTrace(TraceMessages.SavingTemplate, outputFilePath.FullName);

                        // Save rendered template
                        await _repository.SaveTemplateAsync(outputFilePath.FullName, renderedContent).ConfigureAwait(false);
                    }
                    else
                    {
                        // Set output file path
                        var outputFilePath = new FileInfo(Path.Combine(conversionPathRoot.FullName, templateOutputPath, templateFilePath.Name));

                        _logger.LogDebug(TraceMessages.CopyingTemplate, templateFilePath.FullName, outputFilePath.FullName);

                        // Create output path if some directories don't exist
                        if (!_fileRepository.DoesDirectoryExist(outputFilePath.FullName))
                        {
                            _fileRepository.CreateDirectory(outputFilePath.DirectoryName);
                        }

                        // Just a normal file, copy it to output path
                        _fileRepository.CopyFile(templateFilePath.FullName, outputFilePath.FullName);
                    }

                    foundTemplate = true;
                }
            }

            return foundTemplate;
        }
    }
}
