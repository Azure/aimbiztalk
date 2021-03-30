// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="SendPortPipelineDataParserFeature.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Defines the test spec for the SendPortPipelineDataParser class.</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
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
    /// Defines the test spec for the <see cref="SendPortPipelineDataParser"/> class.
    /// </summary>
    public class SendPortPipelineDataParserFeature
    {
#pragma warning disable CA1303 

        private const string ValidData = "<Root xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Stages><Stage CategoryId=\"9d0e4108-4cce-4536-83fa-4a5040674ad6\"><Components><Component Name=\"Microsoft.BizTalk.Component.JsonEncoder\"><Properties><RemoveOuterEnvelope vt=\"11\">-1</RemoveOuterEnvelope></Properties></Component></Components></Stage></Stages></Root>";

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
                .x(() => e = Record.Exception(() => new SendPortPipelineDataParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new SendPortPipelineDataParser(model, context, logger)));

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

            "And a context"
                .x(() => context = new MigrationContext());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "When constructing with null logger"
                .x(() => e = Record.Exception(() => new SendPortPipelineDataParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => parser = new SendPortPipelineDataParser(model, context, logger)));

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
        public void ParseIsSkippedIfModelIsMissing(SendPortPipelineDataParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
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
                .x(() => parser = new SendPortPipelineDataParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests when there are no send ports, the parser does not throw an exception.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulNoSendPorts(PipelineDataParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = CreateGroup(Array.Empty<SendPort>());
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new SendPortPipelineDataParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests when there is one send port, and no send or receive data.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="sendPorts">The send ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulOneSendPort(PipelineDataParser parser, ILogger logger, List<SendPort> sendPorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one send port"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    sendPorts = new List<SendPort> { new SendPort() };
                    group = CreateGroup(sendPorts.ToArray());
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new SendPortPipelineDataParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests when there is one send port and invalid receive data.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="sendPorts">The send ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseOneSendPortAndInvalidReceiveData(PipelineDataParser parser, ILogger logger, List<SendPort> sendPorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one send port and invalid receive data XML"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    sendPorts = new List<SendPort> { new SendPort() };
                    sendPorts[0].ReceivePipelineData = "Invalid XML";
                    group = CreateGroup(sendPorts.ToArray());
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new SendPortPipelineDataParser(model, context, logger));

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
        /// Scenario tests when there is one send port and invalid send data.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="sendPorts">The send ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseOneSendPortAndInvalidSendData(PipelineDataParser parser, ILogger logger, List<SendPort> sendPorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one send port and invalid send data XML"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    sendPorts = new List<SendPort> { new SendPort() };
                    sendPorts[0].SendPipelineData = "Invalid XML";
                    group = CreateGroup(sendPorts.ToArray());
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new SendPortPipelineDataParser(model, context, logger));

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
        /// Scenario tests when there is one send port and valid send data.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="sendPorts">The send ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseOneSendPortAndValidSendData(PipelineDataParser parser, ILogger logger, List<SendPort> sendPorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one send port and valid send data XML"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    sendPorts = new List<SendPort> { new SendPort() };
                    sendPorts[0].SendPipelineData = ValidData;
                    group = CreateGroup(sendPorts.ToArray());
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new SendPortPipelineDataParser(model, context, logger));

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
                    sendPorts[0].SendPipelineCustomConfiguration.Should().NotBeNull();
                    sendPorts[0].SendPipelineCustomConfiguration.Stages.Should().HaveCount(1);

                    var rootStage = sendPorts[0].SendPipelineCustomConfiguration.Stages.FirstOrDefault();
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
                    sendPorts[0].ReceivePipelineCustomConfiguration.Should().BeNull();
                });
        }

        /// <summary>
        /// Scenario tests when there is one send port and valid receive data.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="sendPorts">The send ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseOneSendPortAndValidReceiveData(PipelineDataParser parser, ILogger logger, List<SendPort> sendPorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one send port and valid receive data XML"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    sendPorts = new List<SendPort> { new SendPort() };
                    sendPorts[0].ReceivePipelineData = ValidData;
                    group = CreateGroup(sendPorts.ToArray());
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new SendPortPipelineDataParser(model, context, logger));

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
                    sendPorts[0].ReceivePipelineCustomConfiguration.Should().NotBeNull();
                    sendPorts[0].ReceivePipelineCustomConfiguration.Stages.Should().HaveCount(1);

                    var rootStage = sendPorts[0].ReceivePipelineCustomConfiguration.Stages.FirstOrDefault();
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

            "And there should be no send configuration"
                .x(() =>
                {
                    sendPorts[0].SendPipelineCustomConfiguration.Should().BeNull();
                });
        }

        /// <summary>
        /// Scenario tests when there is one send port with valid send and receive data.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="sendPorts">The send ports.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseOneSendPortAndValidSendAndReceiveData(PipelineDataParser parser, ILogger logger, List<SendPort> sendPorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one send port and valid send/receive data XML"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    sendPorts = new List<SendPort> { new SendPort() };
                    sendPorts[0].ReceivePipelineData = ValidData;
                    sendPorts[0].SendPipelineData = ValidData;
                    group = CreateGroup(sendPorts.ToArray());
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new SendPortPipelineDataParser(model, context, logger));

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
                    sendPorts[0].ReceivePipelineCustomConfiguration.Should().NotBeNull();
                    sendPorts[0].ReceivePipelineCustomConfiguration.Stages.Should().HaveCount(1);

                    var rootStage = sendPorts[0].ReceivePipelineCustomConfiguration.Stages.FirstOrDefault();
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

            "And there should be send configuration"
                .x(() =>
                {
                    sendPorts[0].SendPipelineCustomConfiguration.Should().NotBeNull();
                    sendPorts[0].SendPipelineCustomConfiguration.Stages.Should().HaveCount(1);

                    var rootStage = sendPorts[0].SendPipelineCustomConfiguration.Stages.FirstOrDefault();
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
        /// Scenario tests when there are no send ports, the parser does not throw an exception.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureNoBindings(PipelineDataParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = CreateGroup(Array.Empty<SendPort>());
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications[0].Application.Bindings = null; // Blank bindings forces a skip in processing.
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new SendPortPipelineDataParser(model, context, logger));

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
        /// Creates a group with the fields for send port parsing.
        /// </summary>
        /// <param name="sendPortCollection">The send port collection.</param>
        /// <returns>A populated <see cref="ParsedBizTalkApplicationGroup"/>.</returns>
        private static ParsedBizTalkApplicationGroup CreateGroup(SendPort[] sendPortCollection)
        {
            var group = new ParsedBizTalkApplicationGroup();
            var application = new ParsedBizTalkApplication()
            {
                Application = new BizTalkApplication
                {
                    Bindings = new BindingFile("TestContainer", "BindingInfo")
                    {
                        BindingInfo = new BindingInfo
                        {
                            SendPortCollection = sendPortCollection
                        }
                    }
                }
            };
            group.Applications.Add(application);
            return group;
        }
    }
#pragma warning restore CA1303 // Do not pass literals as localized parameters
}
