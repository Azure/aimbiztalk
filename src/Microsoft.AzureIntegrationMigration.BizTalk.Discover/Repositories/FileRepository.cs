// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="FileRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Defines a class used to interact with a file repository.</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Deployment.WindowsInstaller.Package;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Discover.Repositories
{
    /// <summary>
    /// Defines a class used to interact with a file repository.
    /// </summary>
    public class FileRepository : IFileRepository
    {
        /// <summary>
        /// Extracts the files contained in the cabinet file.
        /// </summary>
        /// <param name="cabFile">The cabinet file to extract.</param>
        /// <param name="path">The directory to extract the cab files to.</param>
        /// <returns>List of files in cabinet.</returns>
        public string[] ExtractFilesInCab(string cabFile, string path)
        {
            _ = cabFile ?? throw new ArgumentNullException(nameof(cabFile));
            _ = path ?? throw new ArgumentNullException(nameof(path));

            // Create CAB extract path
            Directory.CreateDirectory(path);

            // Unpack CAB file
            var cab = new Microsoft.Deployment.Compression.Cab.CabInfo(cabFile);
            cab.Unpack(path);

            // Get unpacked files
            var unpackedFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            return unpackedFiles.ToArray();
        }

        /// <summary>
        /// Extracts the files contained in the msi.
        /// </summary>
        /// <param name="msiFile">The msi file to extract.</param>
        /// <param name="path">The path to extract the files to.</param>
        /// <returns>List of files in MSI.</returns>
        public string[] ExtractFilesInMsi(string msiFile, string path)
        {
            _ = msiFile ?? throw new ArgumentNullException(nameof(msiFile));
            _ = path ?? throw new ArgumentNullException(nameof(path));

            using (var package = new InstallPackage(msiFile, DatabaseOpenMode.ReadOnly, null, path))
            {
                package.ExtractFiles();
            }

            var msiFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            return msiFiles;
        }

        /// <summary>
        /// Creates the working folder for the application.
        /// </summary>
        /// <param name="discovererWorkingFolder">The working folder for the discoverer.</param>
        /// <param name="applicationIdentifier">The identifier for the application.</param>
        /// <returns>The information regarding the created directory.</returns>
        public DirectoryInfo CreateApplicationWorkingFolder(string discovererWorkingFolder, string applicationIdentifier)
        {
            _ = discovererWorkingFolder ?? throw new ArgumentNullException(nameof(discovererWorkingFolder));
            _ = applicationIdentifier ?? throw new ArgumentNullException(nameof(applicationIdentifier));

            // Create a folder for the application.
            var applicationWorkingFolder = Path.Combine(discovererWorkingFolder, applicationIdentifier);
            return Directory.CreateDirectory(applicationWorkingFolder);            
        }

        /// <summary>
        /// Creates the artefact folder.
        /// </summary>
        /// <param name="discovererWorkingFolder">The working folder for the discoverer.</param>
        /// <param name="artefactIdentifier">The identifier for the artefact.</param>
        /// <returns>The information regarding the created directory.</returns>
        public DirectoryInfo CreateArtefactFolder(string discovererWorkingFolder, string artefactIdentifier)
        {
            _ = discovererWorkingFolder ?? throw new ArgumentNullException(nameof(discovererWorkingFolder));
            _ = artefactIdentifier ?? throw new ArgumentNullException(nameof(artefactIdentifier));

            var artefactFolder = Path.Combine(discovererWorkingFolder, artefactIdentifier);

            return Directory.CreateDirectory(artefactFolder);
        }

        /// <summary>
        /// Gets the CAB files.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search.</param>
        /// <returns>An array of the full names for the CAB files, or an empty array if no files are found.</returns>
        public string[] GetCabFiles(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));

            return Directory.GetFiles(path, "*.CAB", SearchOption.AllDirectories);
        }

        /// <summary>
        /// Reads the ADF file.
        /// </summary>
        /// <param name="path">The path to the ADF file.</param>
        /// <returns>The contents of the ADF file.</returns>
        public string ReadAdfFile(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));

            string fileContents;

            using (var reader = File.OpenText(path))
            {
                fileContents = reader.ReadToEnd();
            }

            return fileContents;
        }

        /// <summary>
        /// Reads the binding file.
        /// </summary>
        /// <param name="path">The path to the binding file.</param>
        /// <returns>The contents of the binding file.</returns>
        public string ReadBindingFile(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));

            string fileContents;

            using (var reader = File.OpenText(path))
            {
                fileContents = reader.ReadToEnd();
            }

            return fileContents;
        }

        /// <summary>
        /// Checks if the file exists.
        /// </summary>
        /// <param name="path">The path of the file to check.</param>
        /// <returns>True if the file exists otherwise false.</returns>
        public bool CheckFileExists(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));

            return File.Exists(path);
        }
    }
}
