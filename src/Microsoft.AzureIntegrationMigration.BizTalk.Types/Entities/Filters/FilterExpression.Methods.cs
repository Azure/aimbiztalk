//-----------------------------------------------------------------------
// <copyright file="FilterExpression.Methods.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>BizTalk filter expressions.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces;
using Newtonsoft.Json;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Filters
{
    /// <summary>
    /// BizTalk filter expressions.
    /// </summary>    
    public partial class Filter
    {
        /// <summary>
        /// Returns a filterXml object deserialized from XML.
        /// </summary>
        /// <param name="xml">An XML string containing the filter expression.</param>
        /// <returns>A <see cref="Filter"/> object deserialized from the input.</returns>
        public static Filter FromXml(string xml)
        {
            Filter filterXml;
            var xmlSerializer = new XmlSerializer(typeof(Filter));
            using (var r = new StringReader(xml))
            {
                filterXml = (Filter)xmlSerializer.Deserialize(r);
            }
            return filterXml;
        }
    }
}
