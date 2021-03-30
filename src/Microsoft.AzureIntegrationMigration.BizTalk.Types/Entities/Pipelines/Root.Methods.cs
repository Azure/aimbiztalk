// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="Root.Methods.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Helper methods for the Root class.</summary>
//-----------------------------------------------------------------------

using System.IO;
using System.Xml.Serialization;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines
{
    /// <summary>
    /// Helper methods for the <see cref="Root"/> class.
    /// </summary>
    public partial class Root
    {
        /// <summary>
        /// Creates a <see cref="Root"/> object from its XML representation.
        /// </summary>
        /// <param name="xml">The XML representing the <see cref="Root"/>.</param>
        /// <returns>The <see cref="Root"/> created from the XML.</returns>
        public static Root FromXml(string xml)
        {
            Root configuration;

            var xmlSerializer = new XmlSerializer(typeof(Root));
            using (var r = new StringReader(xml))
            {
                configuration = (Root)xmlSerializer.Deserialize(r);
            }
            return configuration;
        }
    }
}
