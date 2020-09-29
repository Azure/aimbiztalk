// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="ReceivePortPipelineDataParserFeature.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Defines the test spec for the ReceivePortPipelineDataParser class.</summary>
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
    /// Defines the test spec for the <see cref="ReceivePortPipelineDataParser"/> class.
    /// </summary>
    public class ReceivePortPipelineDataParserFeature
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
                .x(() => e = Record.Exception(() => new ReceivePortPipelineDataParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new ReceivePortPipelineDataParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new ReceivePortPipelineDataParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => parser = new ReceivePortPipelineDataParser(model, context, logger)));

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
        public void ParseIsSkippedIfModelIsMissing(ReceivePortPipelineDataParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
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
                .x(() => parser = new ReceivePortPipelineDataParser(model, context, logger));

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
        public void ParseSuccessfulNoReceivePorts(PipelineDataParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
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
                .x(() => parser = new ReceivePortPipelineDataParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the receive pipeline is still empty"
                .x(() => group.Applications[0].Application.Bindings.BindingInfo.ReceivePortCollection.Should().BeNullOrEmpty());
        }

        /// <summary>
        /// Scenario tests when there is one senreceive port, and no data.
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
        public void ParseSuccessfulOneReceivePort(PipelineDataParser parser, ILogger logger, List<ReceivePort> receivePorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one receive port"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    receivePorts = new List<ReceivePort> { new ReceivePort() { Name = "Test Port" } };
                    group = CreateGroup(receivePorts.ToArray());
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortPipelineDataParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests when there is one receive port and invalid data.
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
        public void ParseOneReceivePortAndInvalidSendData(PipelineDataParser parser, ILogger logger, List<ReceivePort> receivePorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one receive port invalid send data XML"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    receivePorts = new List<ReceivePort> { new ReceivePort() { Name = "Test Port" } };
                    receivePorts[0].SendPipelineData = "Invalid XML";
                    group = CreateGroup(receivePorts.ToArray());
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortPipelineDataParser(model, context, logger));

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
        /// Scenario tests when there is one receive port and valid send data.
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
        public void ParseOneReceivePortAndValidSendData(PipelineDataParser parser, ILogger logger, List<ReceivePort> receivePorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one receive port valid send data XML"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    receivePorts = new List<ReceivePort> { new ReceivePort() { Name = "Test Port" } };
                    receivePorts[0].SendPipelineData = ValidData;
                    group = CreateGroup(receivePorts.ToArray());
                    model.MigrationSource.MigrationSourceModel = group;
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortPipelineDataParser(model, context, logger));

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
                    receivePorts[0].SendPipelineCustomConfiguration.Should().NotBeNull();
                    receivePorts[0].SendPipelineCustomConfiguration.Stages.Should().HaveCount(1);

                    var rootStage = receivePorts[0].SendPipelineCustomConfiguration.Stages.FirstOrDefault();
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
        /// Scenario tests when there is one senreceive port, and no data.
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
        public void ParseFailureMissingBindings(PipelineDataParser parser, ILogger logger, List<ReceivePort> receivePorts, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model with one receive port"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    receivePorts = new List<ReceivePort> { new ReceivePort() { Name = "Test Port" } };
                    group = CreateGroup(receivePorts.ToArray());
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications[0].Application.Bindings = null; // Set this to be null to force the skip in processing
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new ReceivePortPipelineDataParser(model, context, logger));

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
        /// Creates a group with the fields for receive port parsing.
        /// </summary>
        /// <param name="receivePortCollection">The receive port collection.</param>
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
