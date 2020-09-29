// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse.Tests
{
    /// <summary>
    /// Defines the test spec for the <see cref="ReceivePortParser"/> class.
    /// </summary>
#pragma warning disable CA1303
    public class ReceivePortParserFeature
    {
        private const string ValidSendData = "<Root xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Stages><Stage CategoryId=\"9d0e4108-4cce-4536-83fa-4a5040674ad6\"><Components><Component Name=\"Microsoft.BizTalk.Component.JsonEncoder\"><Properties><RemoveOuterEnvelope vt=\"11\">-1</RemoveOuterEnvelope></Properties></Component></Components></Stage></Stages></Root>";

        private const string ValidReceiveData = "<Root xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Stages><Stage CategoryId=\"e85b35f0-46a7-4e9e-a6f0-c78cf065c5ad\"><Components><Component Name=\"Microsoft.BizTalk.Component.FFDasmComp\"><Properties><HeaderSpecName vt=\"8\">HeaderSpectTestName</HeaderSpecName><PreserveHeader vt=\"11\">-1</PreserveHeader></Properties></Component></Components></Stage></Stages></Root>";

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
                .x(() => e = Record.Exception(() => new ReceivePortParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new ReceivePortParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new ReceivePortParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => parser = new ReceivePortParser(model, context, logger)));

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
        public void ParseIsSkippedIfModelIsMissing(ReceivePortParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
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
                .x(() => parser = new ReceivePortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests when there are no receive ports, the parser does not throw an exception.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulNoReceivePorts(ReceivePortParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with no receive ports"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = CreateGroup(Array.Empty<ReceivePort>());
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests when there is one receive port, and no receive locations
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="receivePorts">The receive ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulOneReceivePortNoReceiveLocations(ReceivePortParser parser, ILogger logger, List<ReceivePort> receivePorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with a receive port"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    receivePorts = new List<ReceivePort>
                    {
                        new ReceivePort
                        {
                            ResourceKey = "receivePortKey",
                            Name = "receivePortName",
                            Description ="receivePortDescription"
                        }
                    };
                    
                    bindingInfo.ReceivePortCollection = receivePorts.ToArray();

                    var application = new ParsedBizTalkApplication();
                    application.Application.Bindings = new BindingFile(bindingResourceContainerKey, bindingResourceDefinitionKey)
                    {
                        BindingInfo = bindingInfo
                    };

                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(application);

                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.MigrationSourceModel = group;

                    var bindingResourceContainer = new ResourceContainer
                    {
                        Key = bindingResourceContainerKey
                    };

                    var bindingResourceDefinition = new ResourceDefinition
                    {
                        Key = bindingResourceDefinitionKey,
                        Type = ModelConstants.ResourceDefinitionBindings
                    };

                    bindingResourceContainer.ResourceDefinitions.Add(bindingResourceDefinition);
                    model.MigrationSource.ResourceContainers.Add(bindingResourceContainer);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the resources should be set."
               .x(() =>
               {

                   var appModel = (AzureIntegrationServicesModel)model;

                   // Check the receive port source has been created.
                   appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().NotBeNullOrEmpty();
                   appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().HaveCount(1);
                   var receivePortSource = appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0];
                                       
                   // Get the receive port from the bindings.
                   var receivePort = group.Applications[0].Application.Bindings.BindingInfo.ReceivePortCollection[0];

                   // Validate the receive port source.                   
                   receivePortSource.Name.Should().Be(receivePort.Name);
                   receivePortSource.Description.Should().Be(receivePort.Description);
                   receivePortSource.Type.Should().Be(ModelConstants.ResourceReceivePort);
                   receivePortSource.Key.Should().Be(receivePort.ResourceKey);                   
                   receivePortSource.Resources.Should().BeNullOrEmpty();                   
               });
        }

        /// <summary>
        /// Scenario tests when there is one receive port, one receive location and no send or receive data.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="receivePorts">The receive ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulOneReceivePortOneReceiveLocationAndNoData(ReceivePortParser parser, ILogger logger, List<ReceivePort> receivePorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with a receive port and receive location"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    receivePorts = new List<ReceivePort>
                    {
                        new ReceivePort
                        {
                            ResourceKey = "receivePortKey",
                            Name = "receivePortName",
                            Description ="receivePortDescription",
                            ReceiveLocations = new ReceiveLocation[]
                            {
                               new ReceiveLocation
                               {
                                   ResourceKey = "receiveLocationKey",
                                   Name = "receiveLocationName",
                                   Description = "receiveLocationDescription",
                                   ReceiveLocationTransportType = new ProtocolType{ Name = "receiveLocationTransportType"}
                               }
                            }
                        }
                    };

                    bindingInfo.ReceivePortCollection = receivePorts.ToArray();

                    var application = new ParsedBizTalkApplication();
                    application.Application.Bindings = new BindingFile(bindingResourceContainerKey, bindingResourceDefinitionKey)
                    {
                        BindingInfo = bindingInfo
                    };

                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(application);

                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.MigrationSourceModel = group;

                    var bindingResourceContainer = new ResourceContainer
                    {
                        Key = bindingResourceContainerKey
                    };

                    var bindingResourceDefinition = new ResourceDefinition
                    {
                        Key = bindingResourceDefinitionKey,
                        Type = ModelConstants.ResourceDefinitionBindings
                    };

                    bindingResourceContainer.ResourceDefinitions.Add(bindingResourceDefinition);
                    model.MigrationSource.ResourceContainers.Add(bindingResourceContainer);

                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the resources should be set."
               .x(() =>
               {

                   var appModel = (AzureIntegrationServicesModel)model;

                   // Check the receive port source has been created.
                   appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().NotBeNullOrEmpty();
                   appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().HaveCount(1);
                   var receivePortResource = appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0];
                   var resourceDefinition = appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0];

                   // Get the receive port from the bindings.
                   var receivePort = group.Applications[0].Application.Bindings.BindingInfo.ReceivePortCollection[0];

                   // Validate the receive port resource.                   
                   receivePortResource.Name.Should().Be(receivePort.Name);
                   receivePortResource.Description.Should().Be(receivePort.Description);
                   receivePortResource.Type.Should().Be(ModelConstants.ResourceReceivePort);
                   receivePortResource.Key.Should().Be(receivePort.ResourceKey);
                   receivePortResource.Resources.Should().NotBeEmpty();
                   receivePortResource.Resources.Should().HaveCount(1);
                   receivePortResource.Properties.Should().HaveCount(1);

                   receivePort.Resource.Should().Be(receivePortResource); // The pointer to the resource should be set.
                   receivePortResource.ParentRefId.Should().Be(resourceDefinition.RefId); // The parent ref ID should be set.
                   receivePortResource.SourceObject.Should().Be(receivePort); // The resource should have a pointer to the source object. 

                   // Get the receive location from the bindings.
                   var receiveLocation = group.Applications[0].Application.Bindings.BindingInfo.ReceivePortCollection[0].ReceiveLocations[0];

                   // Validate the receive location.
                   var receiveLocationResource = appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0].Resources[0];
                   receiveLocationResource.Description.Should().Be(receiveLocation.Description);
                   receiveLocationResource.Type.Should().Be(ModelConstants.ResourceReceiveLocation);
                   receiveLocationResource.Key.Should().Be(receiveLocation.ResourceKey);
                   receiveLocationResource.Properties.Should().HaveCount(2);

                   receiveLocation.Resource.Should().Be(receiveLocationResource); // The pointer to the resource should be set.
                   receiveLocationResource.ParentRefId.Should().Be(receivePortResource.RefId); // The parent ref ID should be set.
                   receiveLocationResource.SourceObject.Should().Be(receiveLocation); // The resource should have a pointer to the source object.
               });
        }

        /// <summary>
        /// Scenario tests when there is one receive location and invalid receive data.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="receivePorts">The receive ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseOneReceiveLocationAndInvalidReceiveData(ReceivePortParser parser, ILogger logger, List<ReceivePort> receivePorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with a receive port, receive location and invalid receive data"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    receivePorts = new List<ReceivePort>
                    {
                        new ReceivePort
                        {
                            ResourceKey = "receivePortKey",
                            Name = "receivePortName",
                            Description ="receivePortDescription",
                            ReceiveLocations = new ReceiveLocation[]
                            {
                                new ReceiveLocation
                                {
                                    ResourceKey = "receivelocationResourceKey",
                                    Name = "receiveLocationName",
                                    Description = "receiveLocationDescription",
                                    ReceivePipelineData = "Invalid XML",
                                    ReceiveLocationTransportType = new ProtocolType{ Name = "receiveLocationTransportType"}
                                }
                            }
                        }
                    };

                    bindingInfo.ReceivePortCollection = receivePorts.ToArray();

                    var application = new ParsedBizTalkApplication();
                    application.Application.Bindings = new BindingFile(bindingResourceContainerKey, bindingResourceDefinitionKey)
                    {
                        BindingInfo = bindingInfo
                    };

                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(application);

                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.MigrationSourceModel = group;

                    var bindingResourceContainer = new ResourceContainer
                    {
                        Key = bindingResourceContainerKey
                    };

                    var bindingResourceDefinition = new ResourceDefinition
                    {
                        Key = bindingResourceDefinitionKey,
                        Type = ModelConstants.ResourceDefinitionBindings
                    };

                    bindingResourceContainer.ResourceDefinitions.Add(bindingResourceDefinition);
                    model.MigrationSource.ResourceContainers.Add(bindingResourceContainer);

                    application.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                    var container = new ResourceContainer();
                    container.ResourceDefinitions.Add(new ResourceDefinition());
                    container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                    model.MigrationSource.ResourceContainers.Add(container);

                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be one error"
                .x(() =>
                {
                    context.Errors.Should().HaveCount(1);
                    context.Errors[0].Message.Should().Contain("Receive");
                });
        }

        /// <summary>
        /// Scenario tests when there is one receive location and invalid send data.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="receivePorts">The receive ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseOneReceiveLocationAndInvalidSendData(ReceivePortParser parser, ILogger logger, List<ReceivePort> receivePorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with a receive port, receive location and invalid send data"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    receivePorts = new List<ReceivePort>
                    {
                        new ReceivePort
                        {
                            ResourceKey = "receivePortKey",
                            Name = "receivePortName",
                            Description ="receivePortDescription",
                            ReceiveLocations = new ReceiveLocation[]
                            {
                                new ReceiveLocation
                                {
                                    ResourceKey = "receivelocationResourceKey",
                                    Name = "receiveLocationName",
                                    Description = "receiveLocationDescription",
                                    SendPipelineData = "Invalid XML",
                                    ReceiveLocationTransportType = new ProtocolType{ Name = "receiveLocationTransportType"}
                                }
                            }
                        }
                    };

                    bindingInfo.ReceivePortCollection = receivePorts.ToArray();

                    var application = new ParsedBizTalkApplication();
                    application.Application.Bindings = new BindingFile(bindingResourceContainerKey, bindingResourceDefinitionKey)
                    {
                        BindingInfo = bindingInfo
                    };

                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(application);

                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.MigrationSourceModel = group;

                    var bindingResourceContainer = new ResourceContainer
                    {
                        Key = bindingResourceContainerKey
                    };

                    var bindingResourceDefinition = new ResourceDefinition
                    {
                        Key = bindingResourceDefinitionKey,
                        Type = ModelConstants.ResourceDefinitionBindings
                    };

                    bindingResourceContainer.ResourceDefinitions.Add(bindingResourceDefinition);
                    model.MigrationSource.ResourceContainers.Add(bindingResourceContainer);

                    application.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                    var container = new ResourceContainer();
                    container.ResourceDefinitions.Add(new ResourceDefinition());
                    container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                    model.MigrationSource.ResourceContainers.Add(container);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be one error"
                .x(() =>
                {
                    context.Errors.Should().HaveCount(1);
                    context.Errors[0].Message.Should().Contain("Send");
                });
        }

        /// <summary>
        /// Scenario tests when there is one receive location and valid send data.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="receivePorts">The receive ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseOneReceiveLocationAndValidSendData(ReceivePortParser parser, ILogger logger, List<ReceivePort> receivePorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with a receive port, receive location and valid send data"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    receivePorts = new List<ReceivePort>
                    {
                        new ReceivePort
                        {
                            ResourceKey = "receivePortKey",
                            Name = "receivePortName",
                            Description ="receivePortDescription",
                            ReceiveLocations = new ReceiveLocation[]
                            {
                                new ReceiveLocation
                                {
                                    ResourceKey = "receivelocationResourceKey",
                                    Name = "receiveLocationName",
                                    Description = "receiveLocationDescription",
                                    SendPipelineData = ValidSendData,
                                    ReceiveLocationTransportType = new ProtocolType{ Name = "receiveLocationTransportType"}
                                }
                            }
                        }
                    };

                    bindingInfo.ReceivePortCollection = receivePorts.ToArray();

                    var application = new ParsedBizTalkApplication();
                    application.Application.Bindings = new BindingFile(bindingResourceContainerKey, bindingResourceDefinitionKey)
                    {
                        BindingInfo = bindingInfo
                    };

                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(application);

                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.MigrationSourceModel = group;

                    var bindingResourceContainer = new ResourceContainer
                    {
                        Key = bindingResourceContainerKey
                    };

                    var bindingResourceDefinition = new ResourceDefinition
                    {
                        Key = bindingResourceDefinitionKey,
                        Type = ModelConstants.ResourceDefinitionBindings
                    };

                    bindingResourceContainer.ResourceDefinitions.Add(bindingResourceDefinition);
                    model.MigrationSource.ResourceContainers.Add(bindingResourceContainer);

                    application.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                    var container = new ResourceContainer();
                    container.ResourceDefinitions.Add(new ResourceDefinition());
                    container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                    model.MigrationSource.ResourceContainers.Add(container);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be no errors"
                .x(() =>
                {
                    context.Errors.Should().HaveCount(0);
                });

            "And there should be valid send configuration"
                .x(() =>
                {
                    receivePorts[0].ReceiveLocations[0].SendPipelineCustomConfiguration.Should().NotBeNull();
                    receivePorts[0].ReceiveLocations[0].SendPipelineCustomConfiguration.Stages.Should().HaveCount(1);

                    var rootStage = receivePorts[0].ReceiveLocations[0].SendPipelineCustomConfiguration.Stages.FirstOrDefault();
                    rootStage.Components.Should().NotBeNull();
                    rootStage.Components[0].Should().NotBeNull();
                    rootStage.Components[0].Name.Should().NotBeNullOrWhiteSpace();

                    rootStage.Components[0].Properties.Should().HaveCount(1);
                    var property = rootStage.Components[0].Properties.SingleOrDefault();
                    property.Should().NotBeNull();
                    property.Name.Should().NotBeNullOrWhiteSpace();
                    property.Value.Should().NotBeNullOrWhiteSpace();
                    property.ValueType.Should().NotBeNullOrWhiteSpace();
                });

            "And there should be no receive configuration"
                .x(() =>
                {
                    receivePorts[0].ReceiveLocations[0].ReceivePipelineCustomConfiguration.Should().BeNull();
                });
        }

        /// <summary>
        /// Scenario tests when there is one receive location and valid receive data.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="receivePorts">The receive ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseOneReceiveLocationAndValidReceiveData(ReceivePortParser parser, ILogger logger, List<ReceivePort> receivePorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with a receive port, receive location and valid receive data"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    receivePorts = new List<ReceivePort>
                    {
                        new ReceivePort
                        {
                            ResourceKey = "receivePortKey",
                            Name = "receivePortName",
                            Description ="receivePortDescription",
                            ReceiveLocations = new ReceiveLocation[]
                            {
                                new ReceiveLocation
                                {
                                    ResourceKey = "receivelocationResourceKey",
                                    Name = "receiveLocationName",
                                    Description = "receiveLocationDescription",
                                    ReceivePipelineData = ValidReceiveData,
                                    ReceiveLocationTransportType = new ProtocolType{ Name = "receiveLocationTransportType"}
                                }
                            }
                        }
                    };

                    bindingInfo.ReceivePortCollection = receivePorts.ToArray();

                    var application = new ParsedBizTalkApplication();
                    application.Application.Bindings = new BindingFile(bindingResourceContainerKey, bindingResourceDefinitionKey)
                    {
                        BindingInfo = bindingInfo
                    };

                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(application);

                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.MigrationSourceModel = group;

                    var bindingResourceContainer = new ResourceContainer
                    {
                        Key = bindingResourceContainerKey
                    };

                    var bindingResourceDefinition = new ResourceDefinition
                    {
                        Key = bindingResourceDefinitionKey,
                        Type = ModelConstants.ResourceDefinitionBindings
                    };

                    bindingResourceContainer.ResourceDefinitions.Add(bindingResourceDefinition);
                    model.MigrationSource.ResourceContainers.Add(bindingResourceContainer);

                    application.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                    var container = new ResourceContainer();
                    container.ResourceDefinitions.Add(new ResourceDefinition());
                    container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                    model.MigrationSource.ResourceContainers.Add(container);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be no errors"
                .x(() =>
                {
                    context.Errors.Should().HaveCount(0);
                });

            "And there should be valid receive configuration"
                .x(() =>
                {
                    receivePorts[0].ReceiveLocations[0].ReceivePipelineCustomConfiguration.Should().NotBeNull();
                    receivePorts[0].ReceiveLocations[0].ReceivePipelineCustomConfiguration.Stages.Should().HaveCount(1);

                    var rootStage = receivePorts[0].ReceiveLocations[0].ReceivePipelineCustomConfiguration.Stages.FirstOrDefault();
                    rootStage.Components.Should().NotBeNull();
                    rootStage.Components[0].Should().NotBeNull();
                    rootStage.Components[0].Name.Should().NotBeNullOrWhiteSpace();

                    rootStage.Components[0].Properties.Should().HaveCount(2);

                    var propertyOne = rootStage.Components[0].Properties.ElementAt(0);
                    propertyOne.Name.Should().NotBeNullOrWhiteSpace();
                    propertyOne.Value.Should().NotBeNullOrWhiteSpace();
                    propertyOne.ValueType.Should().NotBeNullOrWhiteSpace();

                    var propertyTwo = rootStage.Components[0].Properties.ElementAt(1);
                    propertyTwo.Name.Should().NotBeNullOrWhiteSpace();
                    propertyTwo.Value.Should().NotBeNullOrWhiteSpace();
                    propertyTwo.ValueType.Should().NotBeNullOrWhiteSpace();
                });

            "And there should be no send configuration"
                .x(() =>
                {
                    receivePorts[0].ReceiveLocations[0].SendPipelineCustomConfiguration.Should().BeNull();
                });
        }

        /// <summary>
        /// Scenario tests when there is one receive location with valid send and receive data.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="receivePorts">The receive ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseOneReceiveLocationAndValidSendAndReceiveData(ReceivePortParser parser, ILogger logger, List<ReceivePort> receivePorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with a receive port, receive location, valid send and receive data"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    receivePorts = new List<ReceivePort>
                    {
                        new ReceivePort
                        {
                            ResourceKey = "receivePortKey",
                            Name = "receivePortName",
                            Description ="receivePortDescription",
                            ReceiveLocations = new ReceiveLocation[]
                            {
                                new ReceiveLocation
                                {
                                    ResourceKey = "receivelocationResourceKey",
                                    Name = "receiveLocationName",
                                    Description = "receiveLocationDescription",
                                    ReceivePipelineData = ValidReceiveData,
                                    SendPipelineData = ValidSendData,
                                    ReceiveLocationTransportType = new ProtocolType{ Name = "receiveLocationTransportType"}
                                }
                            }
                        }
                    };

                    bindingInfo.ReceivePortCollection = receivePorts.ToArray();

                    var application = new ParsedBizTalkApplication();
                    application.Application.Bindings = new BindingFile(bindingResourceContainerKey, bindingResourceDefinitionKey)
                    {
                        BindingInfo = bindingInfo
                    };

                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(application);

                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.MigrationSourceModel = group;

                    var bindingResourceContainer = new ResourceContainer
                    {
                        Key = bindingResourceContainerKey
                    };

                    var bindingResourceDefinition = new ResourceDefinition
                    {
                        Key = bindingResourceDefinitionKey,
                        Type = ModelConstants.ResourceDefinitionBindings
                    };

                    bindingResourceContainer.ResourceDefinitions.Add(bindingResourceDefinition);
                    model.MigrationSource.ResourceContainers.Add(bindingResourceContainer);

                    application.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                    var container = new ResourceContainer();
                    container.ResourceDefinitions.Add(new ResourceDefinition());
                    container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                    model.MigrationSource.ResourceContainers.Add(container);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be no errors"
                .x(() =>
                {
                    context.Errors.Should().HaveCount(0);
                });

            "And there should be valid receive configuration"
                .x(() =>
                {
                    receivePorts[0].ReceiveLocations[0].ReceivePipelineCustomConfiguration.Should().NotBeNull();
                    receivePorts[0].ReceiveLocations[0].ReceivePipelineCustomConfiguration.Stages.Should().HaveCount(1);

                    var rootStage = receivePorts[0].ReceiveLocations[0].ReceivePipelineCustomConfiguration.Stages.FirstOrDefault();
                    rootStage.Components.Should().NotBeNull();
                    rootStage.Components[0].Should().NotBeNull();
                    rootStage.Components[0].Name.Should().NotBeNullOrWhiteSpace();

                    rootStage.Components[0].Properties.Should().HaveCount(2);

                    var propertyOne = rootStage.Components[0].Properties.ElementAt(0);
                    propertyOne.Name.Should().NotBeNullOrWhiteSpace();
                    propertyOne.Value.Should().NotBeNullOrWhiteSpace();
                    propertyOne.ValueType.Should().NotBeNullOrWhiteSpace();

                    var propertyTwo = rootStage.Components[0].Properties.ElementAt(1);
                    propertyTwo.Name.Should().NotBeNullOrWhiteSpace();
                    propertyTwo.Value.Should().NotBeNullOrWhiteSpace();
                    propertyTwo.ValueType.Should().NotBeNullOrWhiteSpace();
                });

            "And there should be send configuration"
                .x(() =>
                {
                    receivePorts[0].ReceiveLocations[0].SendPipelineCustomConfiguration.Should().NotBeNull();
                    receivePorts[0].ReceiveLocations[0].SendPipelineCustomConfiguration.Stages.Should().HaveCount(1);

                    var rootStage = receivePorts[0].ReceiveLocations[0].SendPipelineCustomConfiguration.Stages.FirstOrDefault();
                    rootStage.Components.Should().NotBeNull();
                    rootStage.Components[0].Should().NotBeNull();
                    rootStage.Components[0].Name.Should().NotBeNullOrWhiteSpace();

                    rootStage.Components[0].Properties.Should().HaveCount(1);
                    var property = rootStage.Components[0].Properties.SingleOrDefault();
                    property.Should().NotBeNull();
                    property.Name.Should().NotBeNullOrWhiteSpace();
                    property.Value.Should().NotBeNullOrWhiteSpace();
                    property.ValueType.Should().NotBeNullOrWhiteSpace();
                });
        }

        /// <summary>
        /// Scenario tests when there are no receive ports, the parser does not throw an exception.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureNoBindings(ReceivePortParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with no receive ports"
                .x(() =>
                {
                    var application = new ParsedBizTalkApplication();

                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(application);

                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.MigrationSourceModel = group;

                    application.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                    var container = new ResourceContainer();
                    container.ResourceDefinitions.Add(new ResourceDefinition());
                    container.ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                    model.MigrationSource.ResourceContainers.Add(container);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

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
                    invocation.Arguments[2].ToString().Should().Contain("Unable to find the binding info resource");
                });
        }

        #endregion

        /// <summary>
        /// Creates a BizTalk group with the fields for receive location parsing.
        /// </summary>
        /// <returns>A populated <see cref="ParsedBizTalkApplicationGroup"/>.</returns>
        private static ParsedBizTalkApplicationGroup CreateGroup(ReceivePort[] receivePortCollection)
        {
            var group = new ParsedBizTalkApplicationGroup();
            group.Applications.Add(new ParsedBizTalkApplication()
            {
                Application = new BizTalkApplication
                {
                    Bindings = new BindingFile("TestContainer", "BindingInfo")
                    {
                        BindingInfo = new BindingInfo
                        {
                            ReceivePortCollection = receivePortCollection
                        }
                    }
                }
            });

            return group;
        }
    }
#pragma warning restore CA1303 // Do not pass literals as localized parameters
}
