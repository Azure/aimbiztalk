//-----------------------------------------------------------------------
// <copyright file="ComponentPropertyBagFeature.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Defines the test spec for the ComponentPropertyBag class.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Xbehave;
using Xunit;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse.Tests
{
    /// <summary>
    /// Defines the test spec for the <see cref="ComponentPropertyBag"/> class.
    /// </summary>
    public class ComponentPropertyBagFeature
    {
#pragma warning disable CA1303 // Do not pass literals as localized parameters

        /// <summary>
        /// Helper class to build raw XML for the properties.
        /// </summary>
        /// <param name="properties">The properties to create the XML for.</param>
        /// <returns>The XML.</returns>
        private static string BuildXml(List<ComponentProperty> properties)
        {
            using var stringWriter = new StringWriter();
            using (var xmlWriter = XmlWriter.Create(stringWriter))
            {
                xmlWriter.WriteStartElement("Properties");

                foreach (var property in properties)
                {
                    xmlWriter.WriteStartElement(property.Name);
                    xmlWriter.WriteAttributeString("vt", property.ValueType);
                    xmlWriter.WriteString(property.Value);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }
            return stringWriter.ToString();
        }

        /// <summary>
        /// Scenario tests when the schema is retrieved, the schema is null.
        /// </summary>
        /// <param name="propertyBag">The property bag.</param>
        /// <param name="schema">The schema retreived.</param>
        /// <param name="e">The parser exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetSchemaReturnsNull(ComponentPropertyBag propertyBag, XmlSchema schema, Exception e)
        {
            "Given a component property bag"
               .x(() =>
               {
                   propertyBag = new ComponentPropertyBag();
               });

            "When getting the schema"
                .x(() => e = Record.Exception(() => schema = propertyBag.GetSchema()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the schema should be null"
                .x(() =>
                {
                    schema.Should().BeNull();
                });
        }

        /// <summary>
        /// Scenario tests when the XML contains no properties, then the property bag contains no properties.
        /// </summary>
        /// <param name="propertyBag">The property bag.</param>
        /// <param name="xml">The XML representing the property bag.</param>
        /// <param name="reader">The reader for the XML.</param>
        /// <param name="e">The parser exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ReadXmlWithNoProperties(ComponentPropertyBag propertyBag, string xml, XmlReader reader, Exception e)
        {
            "Given a component property bag"
               .x(() =>
               {
                   propertyBag = new ComponentPropertyBag();
               });

            "And xml containing no properties"
                .x(() =>
                {
                    xml = "<Properties/>";
                });

            "And a reader for the xml"
                .x(() =>
                {
                    reader = XmlReader.Create(new StringReader(xml));
                    reader.MoveToContent();
                });

            "When reading the xml"
                .x(() => e = Record.Exception(() => propertyBag.ReadXml(reader)));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And property bag is empty"
                .x(() =>
                {
                    propertyBag.Should().HaveCount(0);
                });
        }

        /// <summary>
        /// Scenario tests when the XML contains one property, then the property bag contains the expected property.
        /// </summary>
        /// <param name="propertyBag">The property bag.</param>
        /// <param name="property">The property to contain in the XML.</param>
        /// <param name="xml">The XML representing the property bag.</param>
        /// <param name="reader">The reader for the XML.</param>
        /// <param name="e">The parser exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ReadXmlWithOneProperty(ComponentPropertyBag propertyBag, ComponentProperty property, string xml, XmlReader reader, Exception e)
        {
            "Given a component property bag"
               .x(() =>
               {
                   propertyBag = new ComponentPropertyBag();
               });

            "And xml containing one property"
                .x(() =>
                {
                    property = new ComponentProperty { Name = "NameOfTheProperty", Value = "ValueOfTheProperty", ValueType = "ValueTypeOfTheProperty" };

                    var properties = new List<ComponentProperty> { property };

                    xml = BuildXml(properties);
                });

            "And a reader for the xml"
               .x(() =>
               {
                   reader = XmlReader.Create(new StringReader(xml));
                   reader.MoveToContent();
               });

            "When reading the xml"
                .x(() => e = Record.Exception(() => propertyBag.ReadXml(reader)));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the property bag contains one property"
                .x(() =>
                {
                    propertyBag.FirstOrDefault().Should().NotBeNull();
                });

            "And the properties should be as expected"
                .x(() =>
                {
                    propertyBag[0].Name.Should().Be(property.Name);
                    propertyBag[0].ValueType.Should().Be(property.ValueType);
                    propertyBag[0].Value.Should().Be(property.Value);
                });
        }

        /// <summary>
        /// Scenario tests when the XML contains multiple properties, then the property bag contains the expected properties.
        /// </summary>
        /// <param name="propertyBag">The property bag.</param>
        /// <param name="properties">The properties to contain in the XML.</param>
        /// <param name="xml">The XML representing the property bag.</param>
        /// <param name="reader">The reader for the XML.</param>
        /// <param name="e">The parser exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ReadXmlWithMultipleProperties(ComponentPropertyBag propertyBag, List<ComponentProperty> properties, string xml, XmlReader reader, Exception e)
        {
            "Given a component property bag"
               .x(() =>
               {
                   propertyBag = new ComponentPropertyBag();
               });

            "And xml containing multiple properties"
                .x(() =>
                {
                    properties = new List<ComponentProperty>
                    {
                        new ComponentProperty { Name = "NameOfThePropertyOne", Value = "ValueOfThePropertyOne", ValueType = "ValueTypeOfThePropertyOne" },
                        new ComponentProperty { Name = "NameOfThePropertyTwo", Value = "ValueOfThePropertyTwo", ValueType = "ValueTypeOfThePropertyTwo" },
                        new ComponentProperty { Name = "NameOfThePropertyThree", Value = "ValueOfThePropertyThree", ValueType = "ValueTypeOfThePropertyThree" }
                    };

                    xml = BuildXml(properties);
                });

            "And a reader for the xml"
               .x(() =>
               {
                   reader = XmlReader.Create(new StringReader(xml));
                   reader.MoveToContent();
               });

            "When reading the xml"
                .x(() => e = Record.Exception(() => propertyBag.ReadXml(reader)));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the property bag contains multiple properties"
                .x(() =>
                {
                    propertyBag.Should().HaveCount(properties.Count);
                });

            "And the properties should be as expected"
                .x(() =>
                {
                    foreach (var property in propertyBag)
                    {
                        // Find the property.
                        var testProperty = propertyBag.SingleOrDefault(p => p.Name == property.Name);
                        testProperty.Should().NotBeNull();

                        // Match the attributes.
                        property.Name.Should().Be(testProperty.Name);
                        property.ValueType.Should().Be(testProperty.ValueType);
                        property.Value.Should().Be(testProperty.Value);

                    }
                });
        }

        /// <summary>
        /// Scenario tests when the property bag is empty an empty XML is property is written.
        /// </summary>
        /// <param name="propertyBag">The property bag.</param>
        /// <param name="stringWriter">The string writer</param>
        /// <param name="xmlWriter">The XML writer</param>
        /// <param name="e">The parser exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void WriteXmlWithNoProperties(ComponentPropertyBag propertyBag, XmlWriter xmlWriter, StringWriter stringWriter, Exception e)
        {
            "Given an empty component property bag"
               .x(() =>
               {
                   propertyBag = new ComponentPropertyBag();
               });

            "And a writer for the xml"
               .x(() =>
               {
                   stringWriter = new StringWriter();

                   xmlWriter = XmlWriter.Create(stringWriter);
               });

            "When writing the xml"
                .x(() => e = Record.Exception(() => propertyBag.WriteXml(xmlWriter)));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the xml contains no properties"
                .x(() =>
                {
                    xmlWriter.Flush();

                    stringWriter.ToString().Should().NotBeEmpty();

                    var element = XElement.Parse(stringWriter.ToString());
                    element.Should().NotBeNull();
                    element.Name.LocalName.Should().Be("Properties");
                    element.Elements().Should().BeNullOrEmpty();
                });
        }

        /// <summary>
        /// Scenario tests when the property bag contains one property, valid XML is property is written.
        /// </summary>
        /// <param name="propertyBag">The property bag.</param>
        /// <param name="stringWriter">The string writer</param>
        /// <param name="xmlWriter">The XML writer</param>
        /// <param name="e">The parser exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void WriteXmlWithOneProperty(ComponentPropertyBag propertyBag, XmlWriter xmlWriter, StringWriter stringWriter, Exception e)
        {
            "Given a component property bag"
               .x(() =>
               {
                   propertyBag = new ComponentPropertyBag();
               });

            "And a writer for the xml"
               .x(() =>
               {
                   stringWriter = new StringWriter();

                   xmlWriter = XmlWriter.Create(stringWriter);
               });

            "And one property in the property bag"
                .x(() =>
                {
                    propertyBag.Add(
                        new ComponentProperty { Name = "TestName", Value = "TestValue", ValueType = "TestValueType" }
                        );
                });

            "When writing the xml"
                .x(() => e = Record.Exception(() => propertyBag.WriteXml(xmlWriter)));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the xml contains one property"
                .x(() =>
                {
                    xmlWriter.Flush();

                    stringWriter.ToString().Should().NotBeEmpty();

                    var element = XElement.Parse(stringWriter.ToString());
                    element.Should().NotBeNull();
                    element.Name.LocalName.Should().Be("Properties");
                    element.Elements().Should().HaveCount(1);

                    element.Elements().SingleOrDefault().Name.LocalName.Should().Be(propertyBag[0].Name);
                    element.Elements().SingleOrDefault().Value.Should().Be(propertyBag[0].Value);
                    element.Elements().SingleOrDefault().Attributes().SingleOrDefault(a => a.Name.LocalName == "vt").Should().NotBeNull().And.HaveValue(propertyBag[0].ValueType);
                });
        }

        /// <summary>
        /// Scenario tests when the property bag contains multiple properties, valid XML is property is written.
        /// </summary>
        /// <param name="propertyBag">The property bag.</param>
        /// <param name="stringWriter">The string writer</param>
        /// <param name="xmlWriter">The XML writer</param>
        /// <param name="e">The parser exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void WriteXmlWithMultipleProperties(ComponentPropertyBag propertyBag, XmlWriter xmlWriter, StringWriter stringWriter, Exception e)
        {
            "Given a component property bag"
               .x(() =>
               {
                   propertyBag = new ComponentPropertyBag();
               });

            "And a writer for the xml"
               .x(() =>
               {
                   stringWriter = new StringWriter();

                   xmlWriter = XmlWriter.Create(stringWriter);
               });

            "And one property in the property bag"
                .x(() =>
                {
                    propertyBag.AddRange(
                        new List<ComponentProperty>() {
                            new ComponentProperty { Name = "TestNameOne", Value = "TestValueOne", ValueType = "TestValueTypeOne" },
                            new ComponentProperty { Name = "TestNameTwo", Value = "TestValueTwo", ValueType = "TestValueTypeTwo" },
                            new ComponentProperty { Name = "TestNameThree", Value = "TestValueThree", ValueType = "TestValueTypeThree" }
                        });
                });

            "When writing the xml"
                .x(() => e = Record.Exception(() => propertyBag.WriteXml(xmlWriter)));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the xml contains one property"
                .x(() =>
                {
                    xmlWriter.Flush();

                    stringWriter.ToString().Should().NotBeEmpty();

                    var element = XElement.Parse(stringWriter.ToString());
                    element.Should().NotBeNull();
                    element.Name.LocalName.Should().Be("Properties");
                    element.Elements().Should().HaveCount(propertyBag.Count);

                    foreach (var property in propertyBag)
                    {
                        var propertyElement = element.Elements().SingleOrDefault(e => e.Name.LocalName == property.Name);
                        propertyElement.Should().NotBeNull();
                        propertyElement.Value.Should().Be(property.Value);
                        propertyElement.Attributes().SingleOrDefault(a => a.Name.LocalName == "vt").Should().NotBeNull().And.HaveValue(property.ValueType);
                    }
                });
        }
    }
#pragma warning restore CA1303 // Do not pass literals as localized parameters
}
