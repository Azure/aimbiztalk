// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="ComponentPropertyBag.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary> Defines the class used to represent a property bag associated with a pipeline.</summary>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines
{
    /// <summary>
    /// Defines the class used to represent a property bag associated with a pipeline
    /// component in the BizTalk bindings.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class ComponentPropertyBag : List<ComponentProperty>, IXmlSerializable
    {
        private const string ValueTypeAttributeName = "vt";

        /// <summary>
        /// Gets the schema associated with the property bag.
        /// </summary>
        /// <returns>The schema.</returns>
        /// <remarks>There is no schema associated with this type.</remarks>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Populates the property bag from from the <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The reader used to populate the component property list.</param>
        public void ReadXml(XmlReader reader)
        {
            var elementProperties = (XElement)XNode.ReadFrom(reader);

            var properties =
                elementProperties.Elements()
                .Select(e => new ComponentProperty()
                {
                    Name = e.Name.LocalName,
                    Value = e.Value,
                    ValueType =
                        e.Attributes().FirstOrDefault
                            (a => a.Name.LocalName == ValueTypeAttributeName).Value
                });

            AddRange(properties);
        }

        /// <summary>
        /// Writes the property bag as XML.
        /// </summary>
        /// <param name="writer">The writer used to write the property bag.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Properties");

            foreach (var property in this)
            {
                writer.WriteStartElement(property.Name);
                writer.WriteAttributeString(ValueTypeAttributeName, property.ValueType);
                writer.WriteString(property.Value);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}
