//-----------------------------------------------------------------------
// <copyright file="BizTalkApplicationParserFeature.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Tests for parsing application information out of the bindings file.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.ApplicationDefinitions;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
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
    /// Tests for parsing application information out of the bindings file.
    /// </summary>
    public class BizTalkApplicationParserFeature
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
                .x(() => e = Record.Exception(() => new BizTalkApplicationParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new BizTalkApplicationParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new BizTalkApplicationParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => parser = new BizTalkApplicationParser(model, context, logger)));

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
        public void ParseIsSkippedIfModelIsMissing(BizTalkApplicationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
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
                .x(() => parser = new BizTalkApplicationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests the happy path that an application binding is parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulSingleApplication(BizTalkApplicationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(new ParsedBizTalkApplication()
                    {
                        ResourceContainerKey = "TestMsi.Key",
                    });
                    model.MigrationSource.MigrationSourceModel = group;

                    var msiContainer = new ResourceContainer() 
                    { 
                        Key = "TestMsi.Key", 
                        Name = "TestMsi", 
                        Type = ModelConstants.ResourceContainerMsi, 
                        ContainerLocation = @"C:\Test\Test.msi" 
                    };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var adf = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ApplicationDefinition xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://Microsoft.BizTalk.ApplicationDeployment/ApplicationDefinition.xsd\">  <Properties>    <Property Name=\"DisplayName\" Value=\"BizTalkApplicationParser.Test\" />    <Property Name=\"Guid\" Value=\"{319AC06C-0FAB-4B68-B2C9-2659DF322B63}\" />    <Property Name=\"Manufacturer\" Value=\"Generated by BizTalk Application Deployment\" />    <Property Name=\"Version\" Value=\"1.0.0.0\" />    <Property Name=\"ApplicationDescription\" Value=\"BizTalk Application 1\" />  </Properties>  <Resources>    <Resource Type=\"System.BizTalk:BizTalkBinding\" Luid=\"Application/SimpleMessagingApplication\">      <Properties>        <Property Name=\"IsDynamic\" Value=\"True\" />        <Property Name=\"IncludeGlobalPartyBinding\" Value=\"True\" />        <Property Name=\"ShortCabinetName\" Value=\"ITEM~0.CAB\" />        <Property Name=\"FullName\" Value=\"BindingInfo.xml\" />        <Property Name=\"Attributes\" Value=\"Archive\" />        <Property Name=\"CreationTime\" Value=\"2020-04-06 16:47:47Z\" />        <Property Name=\"LastAccessTime\" Value=\"2020-04-06 16:47:47Z\" />        <Property Name=\"LastWriteTime\" Value=\"2020-04-06 16:47:47Z\" />      </Properties>      <Files>        <File RelativePath=\"BindingInfo.xml\" Key=\"Binding\" />      </Files>    </Resource>  </Resources>  <References>    <Reference Name=\"BizTalk.System\" />    <Reference Name=\"Simple Referenced Application\" />  </References></ApplicationDefinition>";
                    
                    var resourceDefinition = new ResourceDefinition() 
                    { 
                        Key = "ApplicationDefinition.adf.Key",  
                        Name = "ApplicationDefinition.adf", 
                        Type = ModelConstants.ResourceDefinitionApplicationDefinition, 
                        ResourceContent = adf 
                    };
                    msiContainer.ResourceDefinitions.Add(resourceDefinition);

                    group.Applications[0].Application.ApplicationDefinition = new ApplicationDefinitionFile(msiContainer.Key, resourceDefinition.Key);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object); 
            
            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new BizTalkApplicationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the application should have the correct name"
                .x(() => group.Applications[0].Application.Name.Should().Be("BizTalkApplicationParser.Test"));

            "And the resource should be created"
                .x(() =>
                {
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().NotBeNullOrEmpty();
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().HaveCount(1);
                    var resource = model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0];

                    var application = group.Applications[0].Application;
                    application.ApplicationDefinition.ResourceKey.Should().NotBeNullOrEmpty();
                    resource.Key.Should().Be(application.ApplicationDefinition.ResourceKey);
                    resource.Name.Should().Be("BizTalkApplicationParser.Test");
                    resource.Description.Should().Be("BizTalk Application 1");
                    resource.Type.Should().Be(ModelConstants.ResourceApplication);

                    application.ApplicationDefinition.Resource.Should().Be(resource); // The pointer to the resource should be set.
                    resource.ParentRefId.Should().Be(model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].RefId); // The parent ref ID should be set.
                    resource.SourceObject.Should().Be(application.ApplicationDefinition.ApplicationDefinition); // The resource should have a pointer to the ADF.
                });
        }

        /// <summary>
        /// Scenario tests that multiple applications can be parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulMultipleApplications(BizTalkApplicationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(new ParsedBizTalkApplication()
                    {
                        ResourceContainerKey = "TestMsi1.Key"
                    });
                    group.Applications.Add(new ParsedBizTalkApplication()
                    {
                        ResourceContainerKey = "TestMsi2.Key"
                    });
                    model.MigrationSource.MigrationSourceModel = group;

                    var msiContainer1 = new ResourceContainer() { Key = "TestMsi1.Key", Name = "TestMsi1", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test1.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer1);

                    var adf1 = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ApplicationDefinition xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://Microsoft.BizTalk.ApplicationDeployment/ApplicationDefinition.xsd\">  <Properties>    <Property Name=\"DisplayName\" Value=\"BizTalkApplicationParser.Test\" />    <Property Name=\"Guid\" Value=\"{319AC06C-0FAB-4B68-B2C9-2659DF322B63}\" />    <Property Name=\"Manufacturer\" Value=\"Generated by BizTalk Application Deployment\" />    <Property Name=\"Version\" Value=\"1.0.0.0\" />    <Property Name=\"ApplicationDescription\" Value=\"BizTalk Application 1\" />  </Properties>  <Resources>    <Resource Type=\"System.BizTalk:BizTalkBinding\" Luid=\"Application/SimpleMessagingApplication\">      <Properties>        <Property Name=\"IsDynamic\" Value=\"True\" />        <Property Name=\"IncludeGlobalPartyBinding\" Value=\"True\" />        <Property Name=\"ShortCabinetName\" Value=\"ITEM~0.CAB\" />        <Property Name=\"FullName\" Value=\"BindingInfo.xml\" />        <Property Name=\"Attributes\" Value=\"Archive\" />        <Property Name=\"CreationTime\" Value=\"2020-04-06 16:47:47Z\" />        <Property Name=\"LastAccessTime\" Value=\"2020-04-06 16:47:47Z\" />        <Property Name=\"LastWriteTime\" Value=\"2020-04-06 16:47:47Z\" />      </Properties>      <Files>        <File RelativePath=\"BindingInfo.xml\" Key=\"Binding\" />      </Files>    </Resource>  </Resources>  <References>    <Reference Name=\"BizTalk.System\" />    <Reference Name=\"Simple Referenced Application\" />  </References></ApplicationDefinition>";
                    var resource1 = new ResourceDefinition() { Key = "ApplicationDefinition.adf.Key", Name = "ApplicationDefinition.adf", Type = ModelConstants.ResourceDefinitionApplicationDefinition, ResourceContent = adf1 };
                    msiContainer1.ResourceDefinitions.Add(resource1);

                    group.Applications[0].Application.ApplicationDefinition = new ApplicationDefinitionFile(msiContainer1.Key, resource1.Key);

                    var msiContainer2 = new ResourceContainer() { Key = "TestMsi2.Key", Name = "TestMsi2", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test2.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer2);

                    var adf2 = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ApplicationDefinition xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://Microsoft.BizTalk.ApplicationDeployment/ApplicationDefinition.xsd\">  <Properties>    <Property Name=\"DisplayName\" Value=\"BizTalkApplicationParser.Second\" />    <Property Name=\"Guid\" Value=\"{319AC06C-0FAB-4B68-B2C9-2659DF322B63}\" />    <Property Name=\"Manufacturer\" Value=\"Generated by BizTalk Application Deployment\" />    <Property Name=\"Version\" Value=\"1.0.0.0\" />    <Property Name=\"ApplicationDescription\" Value=\"BizTalk Application 2\" />  </Properties>  <Resources>    <Resource Type=\"System.BizTalk:BizTalkBinding\" Luid=\"Application/SimpleMessagingApplication\">      <Properties>        <Property Name=\"IsDynamic\" Value=\"True\" />        <Property Name=\"IncludeGlobalPartyBinding\" Value=\"True\" />        <Property Name=\"ShortCabinetName\" Value=\"ITEM~0.CAB\" />        <Property Name=\"FullName\" Value=\"BindingInfo.xml\" />        <Property Name=\"Attributes\" Value=\"Archive\" />        <Property Name=\"CreationTime\" Value=\"2020-04-06 16:47:47Z\" />        <Property Name=\"LastAccessTime\" Value=\"2020-04-06 16:47:47Z\" />        <Property Name=\"LastWriteTime\" Value=\"2020-04-06 16:47:47Z\" />      </Properties>      <Files>        <File RelativePath=\"BindingInfo.xml\" Key=\"Binding\" />      </Files>    </Resource>  </Resources>  <References>    <Reference Name=\"BizTalk.System\" />    <Reference Name=\"Simple Referenced Application\" />  </References></ApplicationDefinition>";
                    var resource2 = new ResourceDefinition() { Key = "ApplicationDefinition.adf.Key", Name = "ApplicationDefinition.adf", Type = ModelConstants.ResourceDefinitionApplicationDefinition, ResourceContent = adf2 };
                    msiContainer2.ResourceDefinitions.Add(resource2);

                    group.Applications[1].Application.ApplicationDefinition = new ApplicationDefinitionFile(msiContainer2.Key, resource2.Key);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object); 
            
            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new BizTalkApplicationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the first application should have the correct name."
                .x(() => group.Applications[0].Application.Name.Should().Be("BizTalkApplicationParser.Test"));

            "And the first application should have a resource"
                .x(() =>
                {
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().NotBeNullOrEmpty();
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().HaveCount(1);
                    var resource = model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0];

                    var application = group.Applications[0].Application;
                    application.ApplicationDefinition.ResourceKey.Should().NotBeNullOrEmpty();
                    resource.Key.Should().Be(application.ApplicationDefinition.ResourceKey);
                    resource.Name.Should().Be("BizTalkApplicationParser.Test");
                    resource.Description.Should().Be("BizTalk Application 1");
                    resource.Type.Should().Be(ModelConstants.ResourceApplication);
                });
                        
            "And the second application should have the correct name."
                .x(() => group.Applications[1].Application.Name.Should().Be("BizTalkApplicationParser.Second"));

            "And the first application should have a resource"
               .x(() =>
               {
                   model.MigrationSource.ResourceContainers[1].ResourceDefinitions[0].Resources.Should().NotBeNullOrEmpty();
                   model.MigrationSource.ResourceContainers[1].ResourceDefinitions[0].Resources.Should().HaveCount(1);
                   var resource = model.MigrationSource.ResourceContainers[1].ResourceDefinitions[0].Resources[0];

                   var application = group.Applications[1].Application;
                   application.ApplicationDefinition.ResourceKey.Should().NotBeNullOrEmpty();
                   resource.Key.Should().Be(application.ApplicationDefinition.ResourceKey);
                   resource.Name.Should().Be("BizTalkApplicationParser.Second");
                   resource.Description.Should().Be("BizTalk Application 2");
                   resource.Type.Should().Be(ModelConstants.ResourceApplication);
               });
        }

        /// <summary>
        /// Scenario tests when invalid XML is passed into the parser.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureInvalidXml(BizTalkApplicationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(new ParsedBizTalkApplication() { ResourceContainerKey = "MSI.key" });
                    model.MigrationSource.MigrationSourceModel = group;

                    var container = new ResourceContainer() { Name = "Test", Key="MSI.key", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(container);
                    var adf = "<this is not xml>";
                    var resource = new ResourceDefinition() { Key = "ADF.Key", Name = "ApplicationDefinition", Type = ModelConstants.ResourceDefinitionApplicationDefinition, ResourceContent = adf };
                    container.ResourceDefinitions.Add(resource);

                    group.Applications[0].Application.ApplicationDefinition = new ApplicationDefinitionFile(container.Key, resource.Key);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object); 
            
            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new BizTalkApplicationParser(model, context, logger));

            "When parsing with invalid XML in the bindings file"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be an exception when parsing."
                .x(() =>
                {
                    // There should be an exception logged.
                    context.Errors.Count.Should().Be(1);

                    // The application definition cannot be read and so the name should be default
                    var group = (ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel;
                    group.Applications[0].Application.Name.Should().Be("(Unknown)");

                    // An error should be logged
                    var invocation = _mockLogger.Invocations.Where(i => i.Arguments[0].ToString() == "Error").FirstOrDefault();
                    invocation.Should().NotBeNull();
                    invocation.Arguments[2].ToString().Should().Contain("An error occurred reading application from application definition file");
                });
        }

        /// <summary>
        /// Scenario tests when invalid XML is passed into the parser.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureApplicationDefinitionNull(BizTalkApplicationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(new ParsedBizTalkApplication() { ResourceContainerKey = "MSI.key" });
                    model.MigrationSource.MigrationSourceModel = group;

                    var container = new ResourceContainer() { Name = "Test", Key = "MSI.key", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(container);
                    var adf = "<this is not xml>";
                    var resource = new ResourceDefinition() { Key = "ADF.Key", Name = "ApplicationDefinition", Type = ModelConstants.ResourceDefinitionApplicationDefinition, ResourceContent = adf };
                    container.ResourceDefinitions.Add(resource);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new BizTalkApplicationParser(model, context, logger));

            "When parsing with invalid XML in the bindings file"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be an exception when parsing."
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
        /// Scenario tests the failure path when an application name is blank.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureApplicationNameBlank(BizTalkApplicationParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(new ParsedBizTalkApplication()
                    {
                        ResourceContainerKey = "TestMsi.Key",
                    });
                    model.MigrationSource.MigrationSourceModel = group;

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var adf = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ApplicationDefinition xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://Microsoft.BizTalk.ApplicationDeployment/ApplicationDefinition.xsd\">  <Properties>    <Property Name=\"DisplayName\" Value=\"\" />    <Property Name=\"Guid\" Value=\"{319AC06C-0FAB-4B68-B2C9-2659DF322B63}\" />    <Property Name=\"Manufacturer\" Value=\"Generated by BizTalk Application Deployment\" />    <Property Name=\"Version\" Value=\"1.0.0.0\" />    <Property Name=\"ApplicationDescription\" Value=\"BizTalk Application 1\" />  </Properties>  <Resources>    <Resource Type=\"System.BizTalk:BizTalkBinding\" Luid=\"Application/SimpleMessagingApplication\">      <Properties>        <Property Name=\"IsDynamic\" Value=\"True\" />        <Property Name=\"IncludeGlobalPartyBinding\" Value=\"True\" />        <Property Name=\"ShortCabinetName\" Value=\"ITEM~0.CAB\" />        <Property Name=\"FullName\" Value=\"BindingInfo.xml\" />        <Property Name=\"Attributes\" Value=\"Archive\" />        <Property Name=\"CreationTime\" Value=\"2020-04-06 16:47:47Z\" />        <Property Name=\"LastAccessTime\" Value=\"2020-04-06 16:47:47Z\" />        <Property Name=\"LastWriteTime\" Value=\"2020-04-06 16:47:47Z\" />      </Properties>      <Files>        <File RelativePath=\"BindingInfo.xml\" Key=\"Binding\" />      </Files>    </Resource>  </Resources>  <References>    <Reference Name=\"BizTalk.System\" />    <Reference Name=\"Simple Referenced Application\" />  </References></ApplicationDefinition>";
                    var resource = new ResourceDefinition() { Key = "ApplicationDefinition.adf.Key", Name = "ApplicationDefinition.adf", Type = ModelConstants.ResourceDefinitionApplicationDefinition, ResourceContent = adf };
                    msiContainer.ResourceDefinitions.Add(resource);

                    group.Applications[0].Application.ApplicationDefinition = new ApplicationDefinitionFile(msiContainer.Key, resource.Key);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new BizTalkApplicationParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the context should have an error."
                .x(() => context.Errors.Count.Should().Be(1));
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
