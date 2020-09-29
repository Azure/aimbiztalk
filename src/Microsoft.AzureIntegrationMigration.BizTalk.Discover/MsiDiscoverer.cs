// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="MsiDiscoverer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Discoverer that discovers the contents of installer files (MSIs).</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Discover.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.Discover.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Discover
{
    /// <summary>
    /// Discoverer that discovers the contents of installer files (MSIs).
    /// </summary>
    public class MsiDiscoverer : BizTalkDiscovererBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of this discoverer.
        /// </summary>
        public const string DiscovererName = ModelConstants.ApplicationPrefix + "msidiscoverer";

        /// <summary>
        /// Defines the name of the binding file.
        /// </summary>
        private const string BindingFileName = "BindingInfo.xml";

        /// <summary>
        /// Defines the extension of the application definition file.
        /// </summary>
        private const string ApplicationDefinitionFileExtension = ".adf";

        /// <summary>
        /// Defines the extension of the assembly file.
        /// </summary>
        private const string AssemblyFileExtension = ".dll";

        /// <summary>
        /// Defines the extension of the cabinet file.
        /// </summary>
        private const string CabinetFileExtension = ".CAB";

        /// <summary>
        /// Defines the repository used to store files.
        /// </summary>
        private readonly IFileRepository _fileRepository;

        /// <summary>
        /// Creates a new instance of the <see cref="MsiDiscoverer" class./>
        /// </summary>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">The logger.</param>
        public MsiDiscoverer(IFileRepository fileRepository, IApplicationModel model, MigrationContext context, ILogger logger)
            : base(DiscovererName, model, context, logger)
        {
            // Validate and set the members
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = context.WorkingFolder ?? throw new ArgumentException(ErrorMessages.WorkingDirectoryCannotBeNull, nameof(context));
        }

        /// <summary>
        /// Performs the internal work of the discoverer.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        protected override void DiscoverInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            // Null check.
            _ = (AzureIntegrationServicesModel)model ?? throw new ArgumentNullException(nameof(model));
            _ = context ?? throw new ArgumentNullException(nameof(context)); 

            // Set up the folder structure for the migration.
            BootstrapWorkingFolder(context, context.WorkingFolder);

            // First find and load the MSI resources.
            UnpackMsiFiles(model, context);
        }

        /// <summary>
        /// Creates the folder for the migration.
        /// </summary>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="workingDirectory">The working directory to create the folder in.</param>
        private void BootstrapWorkingFolder(MigrationContext context, string workingDirectory)
        {
            _logger.LogDebug(TraceMessages.BootstrappingWorkingFolderInDirectory, workingDirectory);

            var artefactIdentifier = Path.Combine("unpack", $"biztalk_{DateTime.Now.ToString("s", CultureInfo.CurrentCulture).Replace(":", "-")}");

            var artefactFolder = _fileRepository.CreateArtefactFolder(workingDirectory, artefactIdentifier);

            context.WorkingFolder = artefactFolder.FullName;

            _logger.LogDebug(TraceMessages.BootstrappedWorkingFolder, context.WorkingFolder);
        }

        /// <summary>
        /// Checks that the files exist and unpacks the MSIs to the working folder.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is by design")]
        private void UnpackMsiFiles(AzureIntegrationServicesModel model, MigrationContext context)
        {
            // Create new migration source object model
            var group = new ParsedBizTalkApplicationGroup();

            // Get MSI containers
            var msiContainers = model.MigrationSource.ResourceContainers.Where(c => c.Type == ModelConstants.ResourceContainerMsi).ToArray();

            foreach (var msiContainer in msiContainers)
            {
                if (_fileRepository.CheckFileExists(msiContainer.ContainerLocation))
                {
                    _logger.LogDebug(TraceMessages.UnpackingTheMsi, msiContainer.ContainerLocation);

                    try
                    {
                        // Create application in source model
                        var application = new ParsedBizTalkApplication();
                        group.Applications.Add(application);

                        // Set new working folder
                        var appWorkingFolder = Path.Combine(context.WorkingFolder, msiContainer.Name);

                        // Create a folder for the application
                        application.WorkingFolder = _fileRepository.CreateApplicationWorkingFolder(appWorkingFolder, application.Id.ToString()).FullName;

                        // Set resource container reference
                        application.ResourceContainerKey = msiContainer.Key;

                        // Extract files in MSI file
                        var msiFiles = _fileRepository.ExtractFilesInMsi(msiContainer.ContainerLocation, application.WorkingFolder);
                        foreach (var msiFile in msiFiles.Select(f => new FileInfo(f)).GroupBy(f => f.FullName).Select(f => f.First()))
                        {
                            var resourceName = Path.GetFileNameWithoutExtension(msiFile.Name);
                            var resourceKey = string.Concat(msiContainer.Name, ":", resourceName);

                            if (msiFile.Extension == MsiDiscoverer.ApplicationDefinitionFileExtension)
                            {
                                // Application definition file
                                var adfFileContents = _fileRepository.ReadAdfFile(msiFile.FullName);
                                var adfResourceDefinition = new ResourceDefinition() { Key = resourceKey, Name = resourceName, Type = ModelConstants.ResourceDefinitionApplicationDefinition, ResourceContent = adfFileContents };
                                msiContainer.ResourceDefinitions.Add(adfResourceDefinition);

                                // Set resource container and definition references
                                application.Application.ApplicationDefinition = new ApplicationDefinitionFile(msiContainer.Key, adfResourceDefinition.Key);

                                _logger.LogDebug(TraceMessages.CreatedTheApplicationResourceDefinition, application.Application.ApplicationDefinition.ResourceDefinitionKey);

                            }
                            else if (msiFile.Extension != MsiDiscoverer.CabinetFileExtension)
                            {
                                // Any other file, just add as a file resource
                                var resourceDefinition = new ResourceDefinition() { Key = resourceKey, Name = resourceName, Type = ModelConstants.ResourceDefinitionFile, ResourceContent = msiFile.FullName };
                                msiContainer.ResourceDefinitions.Add(resourceDefinition);

                                _logger.LogDebug(TraceMessages.CreatedTheResourceDefinition, resourceDefinition.Key);
                            }
                        }

                        // Get CAB files
                        var cabFiles = _fileRepository.GetCabFiles(application.WorkingFolder);
                        foreach (var cabFile in cabFiles.Select(f => new FileInfo(f)).GroupBy(f => f.FullName).Select(f => f.First()))
                        {
                            // Add resource container
                            var containerName = Path.GetFileNameWithoutExtension(cabFile.Name);
                            var containerKey = string.Concat(msiContainer.Key, ":", containerName);
                            var cabContainer = new ResourceContainer() { Key = containerKey, Name = containerName, Type = ModelConstants.ResourceContainerCab, ContainerLocation = cabFile.FullName };
                            msiContainer.ResourceContainers.Add(cabContainer);

                            // Set resource container reference
                            application.Application.ResourceContainerKeys.Add(cabContainer.Key);

                            // Now extract files in Cabinet file
                            var cabPath = Path.Combine(application.WorkingFolder, Path.GetFileNameWithoutExtension(cabFile.Name));

                            _logger.LogDebug(TraceMessages.ExtractingCabFilesFromPath, cabPath);

                            var files = _fileRepository.ExtractFilesInCab(cabFile.FullName, cabPath);
                            foreach (var file in files.Select(f => new FileInfo(f)).GroupBy(f => f.FullName).Select(f => f.First()))
                            {
                                var name = Path.GetFileNameWithoutExtension(file.Name);
                                var key = string.Concat(cabContainer.Key, ":", name);

                                if (file.Name == MsiDiscoverer.BindingFileName)
                                {
                                    // Binding file
                                    var bindingFileContents = _fileRepository.ReadBindingFile(file.FullName);
                                    var bindingResourceDefinition = new ResourceDefinition() { Key = key, Name = name, Type = ModelConstants.ResourceDefinitionBindings, ResourceContent = bindingFileContents };
                                    cabContainer.ResourceDefinitions.Add(bindingResourceDefinition);

                                    // Set resource container and definition references
                                    application.Application.Bindings = new BindingFile(cabContainer.Key, bindingResourceDefinition.Key);

                                    _logger.LogDebug(TraceMessages.DiscoveredTheBindingFile, bindingResourceDefinition.Key);
                                }
                                else if (file.Extension == MsiDiscoverer.AssemblyFileExtension)
                                {
                                    // Assembly file
                                    var asmContainer = new ResourceContainer() { Key = key, Name = name, Type = ModelConstants.ResourceContainerAssembly, ContainerLocation = file.FullName };
                                    cabContainer.ResourceContainers.Add(asmContainer);

                                    // Set resource container.
                                    application.Application.Assemblies.Add(new AssemblyFile(asmContainer.Key));

                                    _logger.LogDebug(TraceMessages.DiscoveredTheAssembly, asmContainer.Key);
                                }
                                else
                                {
                                    // Any other file, just add as a file resource
                                    cabContainer.ResourceDefinitions.Add(new ResourceDefinition() { Key = key, Name = name, Type = ModelConstants.ResourceDefinitionFile, ResourceContent = file.FullName });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorProcessingMSIFile, msiContainer.ContainerLocation, ex.ToString());
                        context.Errors.Add(new ErrorMessage(message));
                        _logger.LogError(message);
                    }
                }
                else
                {
                    var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorMsiNotFound, msiContainer.ContainerLocation);
                    context.Errors.Add(new ErrorMessage(message));
                    _logger.LogError(message);
                }
            }

            model.MigrationSource.MigrationSourceModel = group;
        }
    }
}
