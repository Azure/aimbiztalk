using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types
{
    /// <summary>
    /// Defines extensions for the <see cref="string"/> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a formatted string suitable for a key in the object model.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>A suitable key for the object model.</returns>
        public static string FormatKey(this string value)
        {
            return value.Replace(" ", string.Empty);
        }

        /// <summary>
        /// Converts a file path string by removing illegal characters.
        /// </summary>
        /// <param name="value">The file path to convert.</param>
        /// <returns>The converted file path.</returns>
        public static string ToSafeFilePath(this string value)
        {
            // Strip out any double forward or back slashes.
            var safeFilePath = value.Replace("\\\\", "\\").Replace("//", "/");

            // Normalise to a forward slash path separator.
            safeFilePath = safeFilePath.Replace("\\", "/");

            var pattern = new Regex("[\\/:*?\"<>| ]", RegexOptions.Compiled);

            // Loop through the folders in the path and sanitize each folder.
            var sanitizedFolders = new List<string>();
            foreach (var folder in safeFilePath.Split('/'))
            {
                // Remove invalid characters.
                var sanitizedFolder = pattern.Replace(folder, string.Empty);

                if (!string.IsNullOrEmpty(sanitizedFolder))
                {
                    sanitizedFolders.Add(sanitizedFolder);
                }
            }

            // Return the sanitized path.
            return string.Join("/", sanitizedFolders);
        }
    }
}
