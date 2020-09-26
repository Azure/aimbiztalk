//-----------------------------------------------------------------------
// <copyright file="ApplicationDefinitionParserFeature.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Tests for parsing the application definition file.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
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
    /// Tests for parsing the application definition file.
    /// </summary>
    public class ApplicationDefinitionParserFeature
    {
        /// <summary>
        /// Defines a mocked logger.
        /// </summary>
        private Mock<ILogger> _mockLogger;

        #region Before Each Scenario

        /// <summary>
        /// Sets up state before each scenario.
        /// </summary>
        [Background]
        public void Setup()
        {
            "Given a new mock logger"
                .x(() => _mockLogger = new Mock<ILogger>());
        }

        #endregion

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
                .x(() => logger = _mockLogger.Object);

            "And null model"
                .x(() => model.Should().BeNull());

            "And a context"
                .x(() => context = new MigrationContext());

            "When constructing with null model"
                .x(() => e = Record.Exception(() => new ApplicationDefinitionParser(model, context, logger)));

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
                .x(() => logger = _mockLogger.Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a null context"
                .x(() => context.Should().BeNull());

            "When constructing with null context"
                .x(() => e = Record.Exception(() => new ApplicationDefinitionParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new ApplicationDefinitionParser(model, context, logger)));

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
                .x(() => logger = _mockLogger.Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = new MigrationContext());

            "When constructing"
                .x(() => e = Record.Exception(() => parser = new ApplicationDefinitionParser(model, context, logger)));

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
        public void ParseIsSkippedIfModelIsMissing(ApplicationDefinitionParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();                    
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ApplicationDefinitionParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests the happy path when a good application definition file is parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulSingleFilterGroup(ApplicationDefinitionParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = CreateGroup();
                    model.MigrationSource.MigrationSourceModel = group;

                    var container = new ResourceContainer() { Key = group.Applications[0].ResourceContainerKey, Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(container);
                    var adf = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ApplicationDefinition xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://Microsoft.BizTalk.ApplicationDeployment/ApplicationDefinition.xsd\">  <Properties>    <Property Name=\"DisplayName\" Value=\"SimpleMessagingApplication\" />    <Property Name=\"Guid\" Value=\"{319AC06C-0FAB-4B68-B2C9-2659DF322B63}\" />    <Property Name=\"Manufacturer\" Value=\"Generated by BizTalk Application Deployment\" />    <Property Name=\"Version\" Value=\"1.0.0.0\" />    <Property Name=\"ApplicationDescription\" Value=\"BizTalk Application 1\" />  </Properties>  <Resources>    <Resource Type=\"System.BizTalk:BizTalkBinding\" Luid=\"Application/SimpleMessagingApplication\">      <Properties>        <Property Name=\"IsDynamic\" Value=\"True\" />        <Property Name=\"IncludeGlobalPartyBinding\" Value=\"True\" />        <Property Name=\"ShortCabinetName\" Value=\"ITEM~0.CAB\" />        <Property Name=\"FullName\" Value=\"BindingInfo.xml\" />        <Property Name=\"Attributes\" Value=\"Archive\" />        <Property Name=\"CreationTime\" Value=\"2020-04-06 16:47:47Z\" />        <Property Name=\"LastAccessTime\" Value=\"2020-04-06 16:47:47Z\" />        <Property Name=\"LastWriteTime\" Value=\"2020-04-06 16:47:47Z\" />      </Properties>      <Files>        <File RelativePath=\"BindingInfo.xml\" Key=\"Binding\" />      </Files>    </Resource>  </Resources>  <References>    <Reference Name=\"BizTalk.System\" />    <Reference Name=\"Simple Referenced Application\" />  </References></ApplicationDefinition>";
                    var resource = new ResourceDefinition() { Key = "Applicatcation.adf.Key", Name = "ApplicationDefinition.adf", Type = ModelConstants.ResourceDefinitionApplicationDefinition, ResourceContent = adf };
                    container.ResourceDefinitions.Add(resource);

                    group.Applications[0].Application.ApplicationDefinition = new ApplicationDefinitionFile(container.Key, resource.Key);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ApplicationDefinitionParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the filter group should be correctly parsed."
                .x(() =>
                {
                    var applicationDefinition = group.Applications[0].Application.ApplicationDefinition.ApplicationDefinition;
                    applicationDefinition.Should().NotBeNull();
                    applicationDefinition.Properties.Length.Should().Be(5);
                    applicationDefinition.Resources.Length.Should().Be(1);
                    applicationDefinition.References.Length.Should().Be(2);
                });
        }

        /// <summary>
        /// Scenario tests the happy path when a good application definition file is parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureNoApplicationDefinition(ApplicationDefinitionParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = CreateGroup();
                    model.MigrationSource.MigrationSourceModel = group;

                    var container = new ResourceContainer() { Key = group.Applications[0].ResourceContainerKey, Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(container);
                    var adf = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ApplicationDefinition xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://Microsoft.BizTalk.ApplicationDeployment/ApplicationDefinition.xsd\">  <Properties>    <Property Name=\"DisplayName\" Value=\"SimpleMessagingApplication\" />    <Property Name=\"Guid\" Value=\"{319AC06C-0FAB-4B68-B2C9-2659DF322B63}\" />    <Property Name=\"Manufacturer\" Value=\"Generated by BizTalk Application Deployment\" />    <Property Name=\"Version\" Value=\"1.0.0.0\" />    <Property Name=\"ApplicationDescription\" Value=\"BizTalk Application 1\" />  </Properties>  <Resources>    <Resource Type=\"System.BizTalk:BizTalkBinding\" Luid=\"Application/SimpleMessagingApplication\">      <Properties>        <Property Name=\"IsDynamic\" Value=\"True\" />        <Property Name=\"IncludeGlobalPartyBinding\" Value=\"True\" />        <Property Name=\"ShortCabinetName\" Value=\"ITEM~0.CAB\" />        <Property Name=\"FullName\" Value=\"BindingInfo.xml\" />        <Property Name=\"Attributes\" Value=\"Archive\" />        <Property Name=\"CreationTime\" Value=\"2020-04-06 16:47:47Z\" />        <Property Name=\"LastAccessTime\" Value=\"2020-04-06 16:47:47Z\" />        <Property Name=\"LastWriteTime\" Value=\"2020-04-06 16:47:47Z\" />      </Properties>      <Files>        <File RelativePath=\"BindingInfo.xml\" Key=\"Binding\" />      </Files>    </Resource>  </Resources>  <References>    <Reference Name=\"BizTalk.System\" />    <Reference Name=\"Simple Referenced Application\" />  </References></ApplicationDefinition>";
                    var resource = new ResourceDefinition() { Key = "Applicatcation.adf.Key", Name = "ApplicationDefinition.adf", Type = ModelConstants.ResourceDefinitionApplicationDefinition, ResourceContent = adf };
                    container.ResourceDefinitions.Add(resource);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ApplicationDefinitionParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the model should be parsed with a warning."
                .x(() =>
                {
                    // There should be no exception logged - this is a handled scenario.
                    context.Errors.Count.Should().Be(0);

                    // The application definition cannot be read and so the name should be default
                    var group = (ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel;
                    group.Applications[0].Application.Name.Should().Be("(Unknown)");

                    // An error should be logged
                    var invocation = _mockLogger.Invocations.Where(i => i.Arguments[0].ToString() == "Warning").FirstOrDefault();
                    invocation.Should().NotBeNull();
                    invocation.Arguments[2].ToString().Should().Contain("Unable to find the application definition");
                });
        }

        /// <summary>
        /// Scenario tests when the XML for the .adf is invalid.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureInvalidXml(ApplicationDefinitionParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = CreateGroup();
                    model.MigrationSource.MigrationSourceModel = group;

                    var container = new ResourceContainer() { Key = "Test.Key", Name = "Test", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    model.MigrationSource.ResourceContainers.Add(container);
                    var adf = "<invalid-xml>";
                    var resource = new ResourceDefinition() { Key = "Test.adf.Key", Name = "Test.adf", Type = ModelConstants.ResourceDefinitionApplicationDefinition, ResourceContent = adf };
                    container.ResourceDefinitions.Add(resource);

                    group.Applications[0].Application.ApplicationDefinition = new ApplicationDefinitionFile(container.Key, resource.Key);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ApplicationDefinitionParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the parser should have recorded an error when processing the filter."
                .x(() =>
                {
                    var item = group.Applications[0].Application.ApplicationDefinition.ApplicationDefinition.Should().BeNull();
                    context.Errors.Count.Should().Be(1);
                });
        }

        /// <summary>
        /// Scenario tests when the XML for the .adf is invalid.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureEmptyXml(ApplicationDefinitionParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = CreateGroup();
                    model.MigrationSource.MigrationSourceModel = group;

                    var container = new ResourceContainer() { Key = "Test.Key", Name = "Test", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    model.MigrationSource.ResourceContainers.Add(container);
                    var adf = string.Empty;
                    var resource = new ResourceDefinition() { Key = "Test.adf.Key", Name = "Test.adf", Type = ModelConstants.ResourceDefinitionApplicationDefinition, ResourceContent = adf };
                    container.ResourceDefinitions.Add(resource);

                    group.Applications[0].Application.ApplicationDefinition = new ApplicationDefinitionFile(container.Key, resource.Key);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ApplicationDefinitionParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the parser should have recorded an error when processing the filter."
                .x(() =>
                {
                    var item = group.Applications[0].Application.ApplicationDefinition.ApplicationDefinition.Should().BeNull();
                    context.Errors.Count.Should().Be(1);
                });
        }

        #endregion

        /// <summary>
        /// Creates an empty group with the fields for parsing the application definition.
        /// </summary>
        /// <returns>A populated <see cref="ParsedBizTalkApplicationGroup"/>.</returns>
        private static ParsedBizTalkApplicationGroup CreateGroup()
        {
            var group = new ParsedBizTalkApplicationGroup();
            var application = new ParsedBizTalkApplication
            {
                ResourceContainerKey = "TestApplicationKey"
            };
            group.Applications.Add(application);
            return group;
        }
    }
}
