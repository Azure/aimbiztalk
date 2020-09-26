using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert.Repositories
{
    /// <summary>
    /// Defines an implementation of a file repository.
    /// </summary>
    public class FileRepository : IFileRepository
    {
        /// <summary>
        /// Creates the specified directory.
        /// </summary>
        /// <param name="path">The directory to create.</param>
        /// <returns>The information regarding the created directory.</returns>
        public DirectoryInfo CreateDirectory(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            return Directory.CreateDirectory(dirInfo.FullName);
        }

        /// <summary>
        /// Does the specified directory exist.
        /// </summary>
        /// <param name="path">The directory to check.</param>
        /// <returns>True if the directory exists, otherwise False.</returns>
        public bool DoesDirectoryExist(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            return dirInfo.Exists;
        }

        /// <summary>
        /// Does the specified file exist.
        /// </summary>
        /// <param name="filePath">The file to check.</param>
        /// <returns>True if the file exists, otherwise False.</returns>
        public bool DoesFileExist(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Exists;
        }

        /// <summary>
        /// Writes the xml content to disk at the given path.
        /// </summary>
        /// <param name="filePath">The full name of the file to write to.</param>
        /// <param name="content">The file content.</param>
        public void WriteXmlFile(string filePath, string content)
        {
            var fileInfo = new FileInfo(filePath);

            // Create output path if some directories don't exist            
            if (!Directory.Exists(fileInfo.FullName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using (var writer = new StreamWriter(fileInfo.FullName, false))
            {
                var doc = new XmlDocument() { XmlResolver = null };
#pragma warning disable CA3075 // Insecure DTD processing in XML
                doc.LoadXml(content);
#pragma warning restore CA3075 // Insecure DTD processing in XML
                doc.Save(writer);
            }
        }

        /// <summary>
        /// Writes a Json object to file.
        /// </summary>
        /// <param name="filePath">The full name of the file to write to.</param>
        /// <param name="json">The Json object to write to the file.</param>
        public void WriteJsonFile(string filePath, JObject json)
        {
            _ = json ?? throw new ArgumentNullException(nameof(json));

            var fileInfo = new FileInfo(filePath);

            // Create output path if some directories don't exist            
            if (!Directory.Exists(fileInfo.FullName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using (var writer = new StreamWriter(fileInfo.FullName, false))

            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonWriter.Indentation = 4;
                json.WriteTo(jsonWriter);
            }
        }

        /// <summary>
        /// Copies the file.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="destinationFilePath">The destination file path.</param>
        public void CopyFile(string sourceFilePath, string destinationFilePath)
        {
            File.Copy(sourceFilePath, destinationFilePath, true);
        }
    }
}
