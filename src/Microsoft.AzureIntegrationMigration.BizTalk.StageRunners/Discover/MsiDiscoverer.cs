using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Discover.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Discover
{
    /// <summary>
    /// Defines a discover stage runner that unpacks BizTalk MSI files.
    /// </summary>
    public class MsiDiscoverer : StageRunnerBase, IStageDiscoverer
    {
        /// <summary>
        /// Defines the name of this stage runner.
        /// </summary>
        public const string PluginName = ModelConstants.ApplicationPrefix + "msidiscoverer";

        /// <summary>
        /// Defines an argument name representing one or more MSI file paths.
        /// </summary>
        public const string ArgMsiFiles = PluginName + ".msifiles";

        /// <summary>
        /// Defines an argument name representing a MSI directory.
        /// </summary>
        public const string ArgMsiDirectory = PluginName + ".msidir";

        /// <summary>
        /// Defines an argument name representing the directory where the MSI files will be unpacked.
        /// </summary>
        public const string ArgUnpackDirectory = PluginName + ".unpackdir";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines a list of MSI file paths.
        /// </summary>
        private readonly IList<string> _msiFiles = new List<string>();

        /// <summary>
        /// Defines a directory containing MSI files.
        /// </summary>
        private string _msiDirectory;

        /// <summary>
        /// Defines the unpack directory for MSI files.
        /// </summary>
        private string _unpackDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsiDiscoverer"/> class with a generic logger.
        /// </summary>
        /// <param name="logger">A logger.</param>
        public MsiDiscoverer(ILogger logger)
            : base(PluginName, Stages.Discover, 0, logger)
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

            // Get context
            var context = Container.GetRequiredService<MigrationContext>();

            // Add MSI files from directory if specified
            if (!string.IsNullOrWhiteSpace(_msiDirectory))
            {
                var dirInfo = new DirectoryInfo(_msiDirectory);
                var msiFiles = dirInfo.GetFiles("*.msi", SearchOption.AllDirectories);

                foreach (var file in msiFiles)
                {
                    _msiFiles.Add(file.FullName);
                }
            }

            // Anything to do?
            if (_msiFiles.Count == 0)
            {
                // Nothing to do, return an error
                _logger.LogError(ErrorMessages.NoMsiFilesOrDirectorySpecified);
                context.Errors.Add(new ErrorMessage(ErrorMessages.NoMsiFilesOrDirectorySpecified));

                await Task.CompletedTask.ConfigureAwait(false);
            }

            // Default unpack directory if not specified
            if (string.IsNullOrWhiteSpace(_unpackDirectory))
            {
                _unpackDirectory = Environment.CurrentDirectory;
            }

            _logger.LogInformation(InformationMessages.UnpackDirectory, _unpackDirectory);

            // Set up resource containers
            var model = (AzureIntegrationServicesModel)state.Model;

            // Create the input criteria (distinct paths to the MSIs)
            foreach (var path in _msiFiles.Select(p => new FileInfo(p)).GroupBy(f => f.Name).Select(f => f.First()))
            {
                var pathFullName = Directory.GetFiles(path.DirectoryName, Path.GetFileName(path.Name)).First();
                var name = Path.GetFileNameWithoutExtension(pathFullName);
                model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = name, Name = name, Type = ModelConstants.ResourceContainerMsi, ContainerLocation = pathFullName });
            }

            // Set working folder
            context.WorkingFolder = _unpackDirectory;

            // Call the discover logic (probably should be async)
            var discoverer = Container.GetRequiredService<BizTalk.Discover.MsiDiscoverer>();
            discoverer.Discover();

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Gets arguments for this stage runner.
        /// </summary>
        protected override void GetArgs()
        {
            // MSI Files
            var argValue = GetArg(ArgMsiFiles);
            if (argValue != null)
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArgMsiFiles, argValue);

                // Is it a string array (multiple files)?
                var argMsiFiles = argValue as string[];
                if (argMsiFiles != null)
                {
                    foreach (var filePath in argMsiFiles)
                    {
                        _msiFiles.Add(new FileInfo(filePath).FullName);
                    }
                }
                else
                {
                    _msiFiles.Add(new FileInfo((string)argValue).FullName);
                }
            }

            // MSI Directory
            var argMsiDir = GetArg<string>(ArgMsiDirectory);
            if (!string.IsNullOrWhiteSpace(argMsiDir))
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArgMsiDirectory, argMsiDir);

                _msiDirectory = new DirectoryInfo(argMsiDir).FullName;
            }

            // Unpack Directory
            var argUnpackDir = GetArg<string>(ArgUnpackDirectory);
            if (!string.IsNullOrWhiteSpace(argUnpackDir))
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArgUnpackDirectory, argUnpackDir);

                _unpackDirectory = new DirectoryInfo(argUnpackDir).FullName;
            }
        }

        /// <summary>
        /// Validates the arguments found on the command line.
        /// </summary>
        protected override bool ValidateArgs()
        {
            var valid = true;

            // MSI files
            if (_msiFiles.Count > 0)
            {
                foreach (var file in _msiFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (!fileInfo.Exists)
                    {
                        _logger.LogError(ErrorMessages.MsiFileDoesNotExist, fileInfo.FullName);
                        valid = false;
                    }
                }
            }

            // MSI directory
            if (!string.IsNullOrWhiteSpace(_msiDirectory))
            {
                var dirInfo = new DirectoryInfo(_msiDirectory);
                if (!dirInfo.Exists)
                {
                    _logger.LogError(ErrorMessages.MsiDirectoryDoesNotExist, dirInfo.FullName);
                    valid = false;
                }
            }

            // Unpack directory
            if (!string.IsNullOrWhiteSpace(_unpackDirectory))
            {
                var dirInfo = new DirectoryInfo(_unpackDirectory);
                if (!dirInfo.Exists)
                {
                    _logger.LogError(ErrorMessages.UnpackDirectoryDoesNotExist, dirInfo.FullName);
                    valid = false;
                }
            }

            return valid;
        }
    }
}
