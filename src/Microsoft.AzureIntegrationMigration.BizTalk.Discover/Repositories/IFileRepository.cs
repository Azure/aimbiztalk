// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="IFileRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Defines an interface used to interact with a file repository.</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Discover.Repositories
{
    /// <summary>
    /// Defines an interface used to interact with a file repository.
    /// </summary>
    public interface IFileRepository
    {
        /// <summary>
        /// Extracts the files contained in the cabinet file.
        /// </summary>
        /// <param name="cabFile">The cabinet file to extract.</param>
        /// <param name="path">The directory to extract the cab files to.</param>
        /// <returns>List of files in cabinet.</returns>
        string[] ExtractFilesInCab(string cabFile, string path);

        /// <summary>
        /// Extracts the files contained in the msi.
        /// </summary>
        /// <param name="msiFile">The msi file to extract.</param>
        /// <param name="path">The path to extract the files to.</param>
        /// <returns>List of files in MSI.</returns>
        string[] ExtractFilesInMsi(string msiFile, string path);

        /// <summary>
        /// Gets the CAB files.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search.</param>
        /// <returns>An array of the full names for the CAB files, or an empty array if no files are found.</returns>
        string[] GetCabFiles(string path);

        /// <summary>
        /// Creates the working folder for the application.
        /// </summary>
        /// <param name="discovererWorkingFolder">The working folder for the discoverer.</param>
        /// <param name="applicationIdentifier">The identifier for the application.</param>
        /// <returns>The information regarding the created directory.</returns>
        DirectoryInfo CreateApplicationWorkingFolder(string discovererWorkingFolder, string applicationIdentifier);

        /// <summary>
        /// Creates the artefact folder.
        /// </summary>
        /// <param name="discovererWorkingFolder">The working folder for the discoverer.</param>
        /// <param name="artefactIdentifier">The identifier for the artefact.</param>
        /// <returns>The information regarding the created directory.</returns>
        DirectoryInfo CreateArtefactFolder(string discovererWorkingFolder, string artefactIdentifier);

        /// <summary>
        /// Reads the ADF file.
        /// </summary>
        /// <param name="path">The path to the ADF file.</param>
        /// <returns>The contents of the ADF file.</returns>
        string ReadAdfFile(string path);

        /// <summary>
        /// Reads the binding file.
        /// </summary>
        /// <param name="path">The path to the binding file.</param>
        /// <returns>The contents of the binding file.</returns>
        string ReadBindingFile(string path);

        /// <summary>
        /// Checks if the file exists.
        /// </summary>
        /// <param name="path">The path of the file to check.</param>
        /// <returns>True if the file exists otherwise false.</returns>
        bool CheckFileExists(string path);
    }
}
