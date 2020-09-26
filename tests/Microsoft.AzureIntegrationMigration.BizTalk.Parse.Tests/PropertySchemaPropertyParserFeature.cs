//-----------------------------------------------------------------------
// <copyright file="PropertySchemaPropertyParserFeature.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Tests for parsing the properties out of property schemas.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#endregion

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse.Tests
{
    /// <summary>
    /// Tests for parsing the properties out of property schemas.
    /// </summary>
    public class PropertySchemaPropertyParserFeature
    {
        #region Constructor Tests
      
        /// <summary>
        /// Scenario tests that the object construction throws an exception when null model is passed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="e">The runner exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullModel(IBizTalkParser parser, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a parser"
                .x(() => parser.Should().BeNull());

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object); 
            
            "And null model"
                .x(() => model.Should().BeNull());

            "And a context"
                .x(() => context = new MigrationContext());

            "When constructing with null model"
                .x(() => e = Record.Exception(() => new PropertySchemaPropertyParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when null context is passed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="e">The runner exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullContext(IBizTalkParser parser, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a parser"
                .x(() => parser.Should().BeNull());

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a null context"
                .x(() => context.Should().BeNull());

            "When constructing with null context"
                .x(() => e = Record.Exception(() => new PropertySchemaPropertyParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("context"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when null logger is passed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="e">The runner exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullLogger(IBizTalkParser parser, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a parser"
                .x(() => parser.Should().BeNull());

            "And null logger"
                .x(() => logger.Should().BeNull());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = new MigrationContext());

            "When constructing with null logger"
                .x(() => e = Record.Exception(() => new PropertySchemaPropertyParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction succeeds.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="e">The runner exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithSuccess(IBizTalkParser parser, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a parser"
                .x(() => parser.Should().BeNull());

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = new MigrationContext());

            "When constructing"
                .x(() => e = Record.Exception(() => parser = new PropertySchemaPropertyParser(model, context, logger)));

            "Then the parser constructor should succeed"
                .x(() =>
                {
                    e.Should().BeNull();
                    parser.Should().NotBeNull();
                });
        }

        #endregion

        #region Parse Tests

        /// <summary>
        /// Scenario tests parser skips if the source model is missing.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseIsSkippedIfModelIsMissing(PropertySchemaPropertyParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new PropertySchemaPropertyParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests the happy path when a single filter group has been parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulPropertySchema(PropertySchemaPropertyParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;

                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "TestContainer.Key", Name = "TestContainer" });
                    var content = "<?xml version=\"1.0\" encoding=\"utf-16\"?><xs:schema xmlns=\"http://Microsoft.AzureIntegrationMigration.BizTalk.TestApps.Schemas.PropertySchema1\" xmlns:b=\"http://schemas.microsoft.com/BizTalk/2003\" targetNamespace=\"https://Microsoft.AzureIntegrationMigration.BizTalk.TestApps.Schemas.PropertySchema1\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"><xs:annotation><xs:appinfo><b:schemaInfo schema_type=\"property\" xmlns:b=\"http://schemas.microsoft.com/BizTalk/2003\" /></xs:appinfo></xs:annotation><xs:element name=\"TestStringProperty1\" type=\"xs:string\"><xs:annotation><xs:appinfo><b:fieldInfo propertyGuid=\"4efaeaed-05b9-4fe5-b05f-677b5d9e2a6b\" /></xs:appinfo></xs:annotation></xs:element><xs:element name=\"TestIntegerProperty1\" type=\"xs:int\"><xs:annotation><xs:appinfo><b:fieldInfo propertyGuid=\"bb67d7e6-103a-4567-a405-6d854bc02d5d\" /></xs:appinfo></xs:annotation></xs:element><xs:element name=\"TestBoolProperty1\" type=\"xs:boolean\"><xs:annotation><xs:appinfo><b:fieldInfo propertyGuid=\"5f6fbe77-d32b-4ff3-a9b4-e9602c7aa52f\" /></xs:appinfo></xs:annotation></xs:element></xs:schema>";
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition() { Key = "TestSchema.Key", Name = "TestSchema", Type = ModelConstants.ResourceDefinitionSchema, ResourceContent = content });

                    group.Applications.Add(new ParsedBizTalkApplication() { Application = new BizTalkApplication() });
                    group.Applications[0].Application.Schemas.Add(new Schema("TestContainer.Key", "TestSchema.Key")
                    {
                        SchemaType = BizTalkSchemaType.Property,
                        Namespace = "TestNamespace",
                        Name = "TestPropertySchema",
                        FullName = "TestNamespace.TestSchema"
                    });
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new PropertySchemaPropertyParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the property schema should be correctly parsed."
                .x(() =>
                {
                    var schema = group.Applications[0].Application.Schemas[0];
                    schema.SchemaType.Should().Be(BizTalkSchemaType.Property);
                    schema.ContextProperties.Count.Should().Be(3);
                    schema.ContextProperties[0].PropertyName.Should().Be("TestStringProperty1");
                    schema.ContextProperties[0].DataType.Should().Be("xs:string");
                    schema.ContextProperties[0].FullyQualifiedName.Should().Be("TestNamespace.TestStringProperty1");
                    schema.ContextProperties[1].PropertyName.Should().Be("TestIntegerProperty1");
                    schema.ContextProperties[1].DataType.Should().Be("xs:int");
                    schema.ContextProperties[1].FullyQualifiedName.Should().Be("TestNamespace.TestIntegerProperty1");
                    schema.ContextProperties[2].PropertyName.Should().Be("TestBoolProperty1");
                    schema.ContextProperties[2].DataType.Should().Be("xs:boolean");
                    schema.ContextProperties[2].FullyQualifiedName.Should().Be("TestNamespace.TestBoolProperty1");

                    // Check that the resources have been created
                    var resourceDefinition = model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0];
                    resourceDefinition.Resources.Count.Should().Be(1);
                    resourceDefinition.Resources[0].Name.Should().Be(schema.Name);
                    resourceDefinition.Resources[0].Resources.Count.Should().Be(3);
                    resourceDefinition.Resources[0].Resources[0].Name.Should().Be("TestStringProperty1");
                    resourceDefinition.Resources[0].Resources[1].Name.Should().Be("TestIntegerProperty1");
                    resourceDefinition.Resources[0].Resources[2].Name.Should().Be("TestBoolProperty1");
                    var schemaResource = resourceDefinition.Resources[0];

                    schema.Resource.Should().Be(schemaResource); // The pointer to the resource should be set.
                    schemaResource.ParentRefId.Should().Be(resourceDefinition.RefId); // The parent ref ID should be set.
                    schemaResource.SourceObject.Should().Be(schema); // The resource should have a pointer to the source object. 
                });
        }
        /// <summary>
        /// Parses a single property schema and leaves a normal schema unparsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulPropertySchemaPlusNormalSchema(PropertySchemaPropertyParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;

                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "TestContainer.Key", Name = "TestContainer" });
                    var content1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><xs:schema xmlns=\"http://Microsoft.AzureIntegrationMigration.BizTalk.TestApps.Schemas.PropertySchema1\" xmlns:b=\"http://schemas.microsoft.com/BizTalk/2003\" targetNamespace=\"https://Microsoft.AzureIntegrationMigration.BizTalk.TestApps.Schemas.PropertySchema1\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"><xs:annotation><xs:appinfo><b:schemaInfo schema_type=\"property\" xmlns:b=\"http://schemas.microsoft.com/BizTalk/2003\" /></xs:appinfo></xs:annotation><xs:element name=\"TestStringProperty1\" type=\"xs:string\"><xs:annotation><xs:appinfo><b:fieldInfo propertyGuid=\"4efaeaed-05b9-4fe5-b05f-677b5d9e2a6b\" /></xs:appinfo></xs:annotation></xs:element><xs:element name=\"TestIntegerProperty1\" type=\"xs:int\"><xs:annotation><xs:appinfo><b:fieldInfo propertyGuid=\"bb67d7e6-103a-4567-a405-6d854bc02d5d\" /></xs:appinfo></xs:annotation></xs:element><xs:element name=\"TestBoolProperty1\" type=\"xs:boolean\"><xs:annotation><xs:appinfo><b:fieldInfo propertyGuid=\"5f6fbe77-d32b-4ff3-a9b4-e9602c7aa52f\" /></xs:appinfo></xs:annotation></xs:element></xs:schema>";
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition() { Key = "TestSchema1.Key", Name = "TestSchema1", Type = ModelConstants.ResourceDefinitionSchema, ResourceContent = content1 });

                    var content2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><xs:schema xmlns=\"http://Microsoft.AzureIntegrationMigration.BizTalk.TestApps.Schemas.Schema2NoPromotions\" xmlns:b=\"http://schemas.microsoft.com/BizTalk/2003\" elementFormDefault=\"qualified\" targetNamespace=\"http://Microsoft.AzureIntegrationMigration.BizTalk.TestApps.Schemas.Schema2NoPromotions\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"><xs:element name=\"SimpleCar\"><xs:complexType><xs:sequence><xs:element name=\"Make\" type=\"xs:string\" /><xs:element name=\"Model\" type=\"xs:string\" /><xs:element name=\"DateRegistered\" type=\"xs:date\" /><xs:element name=\"Colour\" type=\"xs:string\" /></xs:sequence></xs:complexType></xs:element></xs:schema>";
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition() { Key = "TestSchema2.Key", Name = "TestSchema2", Type = ModelConstants.ResourceDefinitionSchema, ResourceContent = content2 });

                    group.Applications.Add(new ParsedBizTalkApplication() { Application = new BizTalkApplication() });
                    group.Applications[0].Application.Schemas.Add(new Schema("TestContainer.Key", "TestSchema1.Key")
                    {
                        SchemaType = BizTalkSchemaType.Property,
                        Namespace = "TestNamespace",
                        Name = "TestPropertySchema",
                        FullName = "TestNamespace.TestSchema"
                    });
                    var schema = new Schema("TestContainer", "TestSchema2.Key")
                    {
                        SchemaType = BizTalkSchemaType.Unknown,
                        Namespace = "TestNamespace",
                        Name = "Schema",
                        FullName = "TestNamespace.TestSchema"
                    };
                    group.Applications[0].Application.Schemas.Add(schema);
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new PropertySchemaPropertyParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the property schema should be correctly parsed."
                .x(() =>
                {
                    var schema = group.Applications[0].Application.Schemas[0];
                    schema.SchemaType.Should().Be(BizTalkSchemaType.Property);
                    schema.ContextProperties.Count.Should().Be(3);
                    schema.ContextProperties[0].PropertyName.Should().Be("TestStringProperty1");
                    schema.ContextProperties[0].DataType.Should().Be("xs:string");
                    schema.ContextProperties[0].FullyQualifiedName.Should().Be("TestNamespace.TestStringProperty1");
                    schema.ContextProperties[1].PropertyName.Should().Be("TestIntegerProperty1");
                    schema.ContextProperties[1].DataType.Should().Be("xs:int");
                    schema.ContextProperties[1].FullyQualifiedName.Should().Be("TestNamespace.TestIntegerProperty1");
                    schema.ContextProperties[2].PropertyName.Should().Be("TestBoolProperty1");
                    schema.ContextProperties[2].DataType.Should().Be("xs:boolean");
                    schema.ContextProperties[2].FullyQualifiedName.Should().Be("TestNamespace.TestBoolProperty1");
                });

            "And the message schema should be identified."
                .x(() =>
                {
                    var schema = group.Applications[0].Application.Schemas[1];
                    schema.SchemaType.Should().Be(BizTalkSchemaType.Unknown); // haven't parsed this one and left it unchanged
                    schema.ContextProperties.Count.Should().Be(0);
                });
        }

        /// <summary>
        /// Error raised when the property schema XML is invalid.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureInvalidXml(PropertySchemaPropertyParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;

                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "TestContainer.Key", Name = "TestContainer" });
                    var content = "<invalid-xml><b:schemaInfo schema_type=\"property\" xmlns:b=\"http://schemas.microsoft.com/BizTalk/2003\" />";
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition() { Key = "TestSchema.Key", Name = "TestSchema", Type = ModelConstants.ResourceDefinitionSchema, ResourceContent = content });

                    group.Applications.Add(new ParsedBizTalkApplication() { Application = new BizTalkApplication() });
                    group.Applications[0].Application.Schemas.Add(new Schema("TestContainer.Key", "TestSchema.Key") { SchemaType = BizTalkSchemaType.Property });
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new PropertySchemaPropertyParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the parser should have recorded an error when processing the filter."
                .x(() =>
                {
                    context.Errors.Count.Should().Be(1);
                });
        }

        /// <summary>
        /// Error found when the schema XML is empty.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureEmptyXml(PropertySchemaPropertyParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;

                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Name = "TestContainer" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition() { Key = "TestSchema.Key", Name = "TestSchema", Type = ModelConstants.ResourceDefinitionSchema, ResourceContent = string.Empty });

                    group.Applications.Add(new ParsedBizTalkApplication() { Application = new BizTalkApplication() });
                    group.Applications[0].Application.Schemas.Add(new Schema("TestContainer", "TestSchema.Key") { SchemaType = BizTalkSchemaType.Property });
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new PropertySchemaPropertyParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the parser should have recorded an error when processing the filter."
                .x(() =>
                {
                    context.Errors.Count.Should().Be(1);
                });
        }

        /// <summary>
        /// Ignores invalid XML in a message schema.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessInvalidXmlInMessageSchema(PropertySchemaPropertyParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;

                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "TestContainer.Key", Name = "TestContainer" });
                    var content = "<invalid-xml>";
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition() { Key = "TestSchema.Key", Name = "TestSchema", Type = ModelConstants.ResourceDefinitionSchema, ResourceContent = content });

                    group.Applications.Add(new ParsedBizTalkApplication() { Application = new BizTalkApplication() });
                    group.Applications[0].Application.Schemas.Add(new Schema("TestContainer.Key", "TestSchema.Key"));
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object); 
            
            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new PropertySchemaPropertyParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the parser should have recorded an error when processing the filter."
                .x(() =>
                {
                    var item = group.Applications[0].Application.Schemas[0].SchemaType.Should().Be(BizTalkSchemaType.Unknown); // haven't been able to ascertain the type
                    context.Errors.Count.Should().Be(0);
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters

