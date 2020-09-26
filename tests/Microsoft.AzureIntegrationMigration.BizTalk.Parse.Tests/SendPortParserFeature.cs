using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse.Tests
{
    /// <summary>
    /// Tests for parsing the send ports.
    /// </summary>
    public class SendPortParserFeature
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
                .x(() => e = Record.Exception(() => new SendPortParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when null context is passed.
        /// </summary>
        /// <param name="parser">The parser.</param>
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
                .x(() => e = Record.Exception(() => new SendPortParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new SendPortParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction succeeds.
        /// </summary>
        /// <param name="parser">The parser.</param>
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
                .x(() => e = Record.Exception(() => parser = new SendPortParser(model, context, logger)));

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
        public void ParseIsSkippedIfModelIsMissing(SendPortParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
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
                .x(() => parser = new SendPortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests the happy path when a send port with a filter expression is parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulSingleFilterGroup(SendPortParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with send port definied in the binding and filter expression"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    var sendPorts = new List<SendPort>
                    {
                        new SendPort()
                        {
                            Name = "Send Port 1",
                            Filter = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Filter xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n  <Group>\n    <Statement Property=\"FILE.ReceivedFileName\" Operator=\"6\" />\n    <Statement Property=\"FILE.FileCreationTime\" Operator=\"3\" Value=\"SomeDate\" />\n    <Statement Property=\"Microsoft.BizTalk.XLANGs.BTXEngine.OriginatorSID\" Operator=\"3\" Value=\"Test\" />\n  </Group>\n  <Group>\n    <Statement Property=\"AzureBlobStorage.BlobName\" Operator=\"2\" Value=\"X\" />\n    <Statement Property=\"AzureBlobStorage.CreateTime\" Operator=\"4\" Value=\"Y\" />\n  </Group>\n  <Group>\n    <Statement Property=\"AzureBlobStorage.BlobType\" Operator=\"0\" Value=\"Z\" />\n  </Group>\n</Filter>",
                            PrimaryTransport = new TransportInfo
                            {
                                TransportType = new ProtocolType
                                {
                                    Name = "transportName"
                                }
                            },
                            IsTwoWay = true
                        }
                    };

                    bindingInfo.SendPortCollection = sendPorts.ToArray();

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
                .x(() => parser = new SendPortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the filter group should be correctly parsed."
                .x(() =>
                {
                    var sendPort = group.Applications[0].Application.Bindings.BindingInfo.SendPortCollection[0];
                    sendPort.FilterExpression.Should().NotBeNull();
                    sendPort.FilterExpression.Group.Should().NotBeNull();
                    sendPort.FilterExpression.Group.Length.Should().Be(3);
                    sendPort.FilterExpression.Group[0].Statement.Length.Should().Be(3);
                });

            "And the resources should be set."
                .x(() =>
                {
                    var appModel = (AzureIntegrationServicesModel)model;

                    // Check the send port resource has been created.
                    appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().NotBeNullOrEmpty();
                    appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().HaveCount(1);
                    var sendPortResource = appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0];
                    var resourceDefinition = appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0];

                    // Get the send port from the bindings.
                    var sendPort = group.Applications[0].Application.Bindings.BindingInfo.SendPortCollection[0];

                    // Validate the send port resource.
                    sendPortResource.Name.Should().Be(sendPort.Name);
                    sendPortResource.Description.Should().Be(sendPort.Description);
                    sendPortResource.Type.Should().Be(ModelConstants.ResourceSendPort);
                    sendPortResource.Key.Should().Be(sendPort.ResourceKey);

                    sendPortResource.Resources.Should().NotBeNullOrEmpty();
                    sendPortResource.Resources.Should().HaveCount(1);

                    sendPortResource.Properties.Should().ContainKey("Direction");
                    sendPortResource.Properties["Direction"].Should().Be("Two-Way");

                    sendPort.Resource.Should().Be(sendPortResource); // The pointer to the resource should be set.
                    sendPortResource.ParentRefId.Should().Be(resourceDefinition.RefId); // The parent ref ID should be set.
                    sendPortResource.SourceObject.Should().Be(sendPort); // The resource should have a pointer to the source object.

                    // Validate the filter resource.
                    var filterResource = sendPortResource.Resources[0];
                    filterResource.Name.Should().StartWith(sendPortResource.Name);
                    var expectedFilterKey = string.Concat(sendPort.Name, ":", "filter");
                    filterResource.Key.Should().Be(expectedFilterKey);
                    filterResource.Name.Should().EndWith("filter expression");
                    filterResource.Type.Should().Be(ModelConstants.ResourceFilterExpression);

                    sendPort.FilterExpression.Resource.Should().Be(filterResource); // The pointer to the resource should be set.
                    filterResource.ParentRefId.Should().Be(sendPortResource.RefId); // The parent ref ID should be set.
                    filterResource.SourceObject.Should().Be(sendPort.FilterExpression); // The resource should have a pointer to the source object.
                });
        }

        /// <summary>
        /// Scenario tests the happy path when a send port with an empty filter group has been parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulEmptyFilterString(SendPortParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with send port definied in the binding and no filter expression"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    var sendPorts = new List<SendPort>
                    {
                        new SendPort()
                        {
                            Name = "Send Port 1",
                            PrimaryTransport = new TransportInfo
                            {
                                TransportType = new ProtocolType
                                {
                                    Name = "transportName"
                                }
                            },
                            IsTwoWay = false
                        }
                    };

                    bindingInfo.SendPortCollection = sendPorts.ToArray();

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
                .x(() => parser = new SendPortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the filter group should be null - nothing created."
                .x(() =>
                {
                    var sendPort = group.Applications[0].Application.Bindings.BindingInfo.SendPortCollection[0];
                    sendPort.FilterExpression.Should().BeNull();
                    context.Errors.Count.Should().Be(0);
                });

            "And the resources should be set."
               .x(() =>
               {
                   var appModel = (AzureIntegrationServicesModel)model;

                   // Check the send port source has been created.
                   appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().NotBeNullOrEmpty();
                   appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().HaveCount(1);
                   var sendPortSource = appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0];

                   // Get the send port from the bindings.
                   var sendPort = group.Applications[0].Application.Bindings.BindingInfo.SendPortCollection[0];

                   // Validate the send port source.
                   sendPortSource.Name.Should().Be(sendPort.Name);
                   sendPortSource.Description.Should().Be(sendPort.Description);
                   sendPortSource.Type.Should().Be(ModelConstants.ResourceSendPort);
                   sendPortSource.Key.Should().Be(sendPort.ResourceKey);

                   sendPortSource.Resources.Should().BeNullOrEmpty();

                   sendPortSource.Properties.Should().ContainKey("Direction");
                   sendPortSource.Properties["Direction"].Should().Be("One-Way");
               });
        }

        /// <summary>
        /// Scenario tests when the filter contains invalid xml.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureInvalidXml(SendPortParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with send port definied in the binding and an invalid filter expression"
                .x(() =>
                {
                     var bindingInfo = new BindingInfo();
                     var sendPorts = new List<SendPort>
                     {
                        new SendPort()
                        {
                            Filter = "invalid xml",
                            Name = "Send Port 1",
                            PrimaryTransport = new TransportInfo
                            {
                                TransportType = new ProtocolType
                                {
                                    Name = "transportName"
                                }
                            }
                        }
                     };

                     bindingInfo.SendPortCollection = sendPorts.ToArray();

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
                .x(() => parser = new SendPortParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the parser should have recorded an error when processing the filter."
                .x(() =>
                {
                    var sendPort = group.Applications[0].Application.Bindings.BindingInfo.SendPortCollection[0];
                    sendPort.FilterExpression.Should().BeNull();
                    context.Errors.Count.Should().Be(1);
                });
        }

        /// <summary>
        /// Scenario tests the happy path when a send port with an empty filter group has been parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureMissingBindings(SendPortParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with send port definied in the binding and no filter expression"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    var sendPorts = new List<SendPort>
                    {
                        new SendPort()
                        {
                            Name = "Send Port 1",
                            PrimaryTransport = new TransportInfo
                            {
                                TransportType = new ProtocolType
                                {
                                    Name = "transportName"
                                }
                            }
                        }
                    };

                    bindingInfo.SendPortCollection = sendPorts.ToArray();

                    var application = new ParsedBizTalkApplication();

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
                .x(() => parser = new SendPortParser(model, context, logger));

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
    }
}
