//-----------------------------------------------------------------------
// <copyright file="MetaModel.Methods.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Helper methods for the MetaModel class.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System.IO;
using System.Xml.Serialization;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations
{
    /// <summary>
    /// Helper methods for the <see cref="MetaModel"/> class.
    /// </summary>
    public partial class MetaModel
    {
        /// <summary>
        /// Returns a <see cref="MetaModel"/> object deserialized from XML.
        /// </summary>
        /// <param name="xml">The XML to create the MetaModel from.</param>
        /// <returns>The <see cref="MetaModel"/> object as defined in the supplied XML.</returns>
        public static MetaModel FromXml(string xml)
        {
            MetaModel metaModel;

            var xmlSerializer = new XmlSerializer(typeof(MetaModel));
            using (var r = new StringReader(xml))
            {
                metaModel = (MetaModel)xmlSerializer.Deserialize(r);
            }
            return metaModel;
        }
    }
}
