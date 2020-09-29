// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="ApplicationDefinition.Methods.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>BizTalk application definition.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.ApplicationDefinitions
{
    /// <summary>
    /// BizTalk application definition.
    /// </summary>
    public partial class ApplicationDefinition
    {
        /// <summary>
        /// Converts the application definition XML into a deserialized object.
        /// </summary>
        /// <param name="xml">The serialized XML of the ADF.</param>
        /// <returns>Returns a <see cref="ApplicationDefinition"/> object deserialized from XML.</returns>
        public static ApplicationDefinition FromXml(string xml)
        {
            ApplicationDefinition applicationDefinition;
            var xmlSerializer = new XmlSerializer(typeof(ApplicationDefinition));
            using (var r = new StringReader(xml))
            {
                applicationDefinition = (ApplicationDefinition)xmlSerializer.Deserialize(r);
            }
            return applicationDefinition;
        }

        /// <summary>
        /// Returns an item from the property bag.
        /// </summary>
        /// <param name="key">The key to search on in the property bag.</param>
        /// <returns>The value of the item, if found.</returns>
        public string GetPropertyValue(string key)
        {
            return Properties.Where(p => p.Name == key).SingleOrDefault()?.Value;
        }
    }
}
