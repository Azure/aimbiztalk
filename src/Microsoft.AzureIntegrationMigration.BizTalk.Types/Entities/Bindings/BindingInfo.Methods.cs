//-----------------------------------------------------------------------
// <copyright file="BindingInfo.Methods.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>BizTalk bindings.</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Filters;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings
{
    /// <summary>
    /// BizTalk bindings.
    /// </summary>    
    public partial class BindingInfo
    {
        /// <summary>
        /// Converts the binding XML into a deserialized object.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>Returns a <see cref="BindingInfo"/> object deserialized from XML.</returns>
        public static BindingInfo FromXml(string xml)
        {
            BindingInfo bindingInfo;
            var xmlSerializer = new XmlSerializer(typeof(BindingInfo));
            using (var r = new StringReader(xml))
            {
                bindingInfo = (BindingInfo)xmlSerializer.Deserialize(r);
            }
            return bindingInfo;
        }
    }
}
