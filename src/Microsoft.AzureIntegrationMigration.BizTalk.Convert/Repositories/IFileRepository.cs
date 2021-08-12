// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert.Repositories
{
    /// <summary>
    /// Defines an interface used to interact with a file repository.
    /// </summary>
    public interface IFileRepository
    {
        /// <summary>
        /// Creates the specified directory.
        /// </summary>
        /// <param name="path">The directory to create.</param>
        /// <returns>The information regarding the created directory.</returns>
        DirectoryInfo CreateDirectory(string path);

        /// <summary>
        /// Does the specified directory exist.
        /// </summary>
        /// <param name="path">The directory to check.</param>
        /// <returns>True if the directory exists, otherwise False.</returns>
        bool DoesDirectoryExist(string path);

        /// <summary>
        /// Does the specified file exist.
        /// </summary>
        /// <param name="filePath">The file to check.</param>
        /// <returns>True if the file exists, otherwise False.</returns>
        bool DoesFileExist(string filePath);

        /// <summary>
        /// Writes the xml content to disk at the given path.
        /// </summary>
        /// <param name="filePath">The full name of the file to write to.</param>
        /// <param name="content">The file content.</param>
        void WriteXmlFile(string filePath, string content);

        /// <summary>
        /// Writes a Json token to a file.
        /// </summary>
        /// <param name="filePath">The full name of the file to write to.</param>
        /// <param name="json">The Json token to write to the file.</param>
        void WriteJsonFile(string filePath, JToken json);

        /// <summary>
        /// Copies the file.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="destinationFilePath">The destination file path.</param>
        void CopyFile(string sourceFilePath, string destinationFilePath);
    }
}
