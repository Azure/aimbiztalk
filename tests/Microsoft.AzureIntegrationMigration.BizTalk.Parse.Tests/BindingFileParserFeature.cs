// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="BindingFileParserFeature.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Tests for parsing the bindings file.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Linq;
using System.Xml.Serialization;
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

#endregion

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse.Tests
{
    /// <summary>
    /// Tests for parsing the bindings file.
    /// </summary>
    public class BindingFileParserFeature
    {
        /// <summary>
        /// This is a good binding file XML with a module ref within it.
        /// </summary>
        private const string GoodBindings = "<?xml version=\"1.0\" encoding=\"utf-8\"?><BindingInfo xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Assembly=\"Microsoft.BizTalk.Deployment, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Version=\"3.5.1.0\" BindingStatus=\"PartiallyBound\" BoundEndpoints=\"2\" TotalEndpoints=\"12\"><Timestamp>2020-03-28T15:26:12.7248467+00:00</Timestamp><ModuleRefCollection><ModuleRef Name=\"[Application:ModuleRefParser.Test]\" Version=\"\" Culture=\"\" PublicKeyToken=\"\" FullName=\"[Application:Thunderstorm.Test], Version=, Culture=, PublicKeyToken=\"><Services /><TrackedSchemas><Schema FullName=\"OrchsToConvert.PropertySchema.PropertySchema\" AssemblyQualifiedName=\"OrchsToConvert.PropertySchema.PropertySchema,OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a\" AlwaysTrackAllProperties=\"false\" Description=\"\"><TrackedPropertyNames /></Schema><Schema FullName=\"OrchsToConvert.Schemas.USOrderPropertySchema\" AssemblyQualifiedName=\"OrchsToConvert.Schemas.USOrderPropertySchema,OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a\" AlwaysTrackAllProperties=\"false\" Description=\"\"><TrackedPropertyNames /></Schema><Schema FullName=\"OrchsToConvert.Schemas.PropertySchema1\" AssemblyQualifiedName=\"OrchsToConvert.Schemas.PropertySchema1,OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a\" AlwaysTrackAllProperties=\"false\" Description=\"\"><TrackedPropertyNames /></Schema><Schema FullName=\"OrchsToConvert.Schemas.PO\" RootName=\"PurchaseOrder\" AssemblyQualifiedName=\"OrchsToConvert.Schemas.PO,OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a\" AlwaysTrackAllProperties=\"false\"><TrackedPropertyNames /></Schema><Schema FullName=\"OrchsToConvert.Schema1\" RootName=\"Root\" AssemblyQualifiedName=\"OrchsToConvert.Schema1,OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a\" AlwaysTrackAllProperties=\"false\" Description=\"\"><TrackedPropertyNames /></Schema><Schema FullName=\"OrchsToConvert.Schemas.Schema2\" RootName=\"Root\" AssemblyQualifiedName=\"OrchsToConvert.Schemas.Schema2,OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a\" AlwaysTrackAllProperties=\"false\" Description=\"\"><TrackedPropertyNames /></Schema><Schema FullName=\"OrchsToConvert.Schemas.OrderUS\" RootName=\"USOrder\" AssemblyQualifiedName=\"OrchsToConvert.Schemas.OrderUS,OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a\" AlwaysTrackAllProperties=\"false\" Description=\"\"><TrackedPropertyNames /></Schema><Schema FullName=\"OrchsToConvert.GenericSchema\" RootName=\"Root\" AssemblyQualifiedName=\"OrchsToConvert.GenericSchema,OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a\" AlwaysTrackAllProperties=\"false\" Description=\"\"><TrackedPropertyNames /></Schema><Schema FullName=\"OrchsToConvert.RequestMsg\" RootName=\"UpgradeRequest\" AssemblyQualifiedName=\"OrchsToConvert.RequestMsg,OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a\" AlwaysTrackAllProperties=\"false\" Description=\"\"><TrackedPropertyNames /></Schema><Schema FullName=\"OrchsToConvert.Schemas.OrderFF\" RootName=\"Order\" AssemblyQualifiedName=\"OrchsToConvert.Schemas.OrderFF,OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a\" AlwaysTrackAllProperties=\"false\" Description=\"\"><TrackedPropertyNames /></Schema><Schema FullName=\"OrchsToConvert.MessageTypes.PurchaseOrder\" RootName=\"Root\" AssemblyQualifiedName=\"OrchsToConvert.MessageTypes.PurchaseOrder,OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a\" AlwaysTrackAllProperties=\"false\" Description=\"\"><TrackedPropertyNames /></Schema></TrackedSchemas></ModuleRef><ModuleRef Name=\"OrchsToConvert\" Version=\"1.0.0.0\" Culture=\"neutral\" PublicKeyToken=\"63a3d0af71fe5e2a\" FullName=\"OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a\"><Services><Service Name=\"BizTalkServerProject.SimpleOrch\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports><Port Name=\"ReceiveSendPort\" Modifier=\"2\" BindingOption=\"1\"><SendPortRef xsi:nil=\"true\" /><DistributionListRef xsi:nil=\"true\" /><ReceivePortRef Name=\"RequestReplyHttpReceive\" /></Port></Ports><Roles /><Host Name=\"BizTalkServerApplication\" NTGroupName=\"BizTalk Application Users\" Type=\"1\" Trusted=\"false\" /></Service><Service Name=\"OrchsToConvert.SchemaTypedMessaging\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports><Port Name=\"Port_1\" Modifier=\"2\" BindingOption=\"1\"><SendPortRef xsi:nil=\"true\" /><DistributionListRef xsi:nil=\"true\" /><ReceivePortRef xsi:nil=\"true\" /></Port><Port Name=\"Port_2\" Modifier=\"1\" BindingOption=\"1\"><SendPortRef xsi:nil=\"true\" /><DistributionListRef xsi:nil=\"true\" /><ReceivePortRef xsi:nil=\"true\" /></Port></Ports><Roles /><Host Name=\"BizTalkServerApplication\" NTGroupName=\"BizTalk Application Users\" Type=\"1\" Trusted=\"false\" /></Service><Service Name=\"OrchsToConvert.RequestReply\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports><Port Name=\"ReceiveResponse\" Modifier=\"2\" BindingOption=\"1\"><SendPortRef xsi:nil=\"true\" /><DistributionListRef xsi:nil=\"true\" /><ReceivePortRef Name=\"RequestReplyHttpReceive\" /></Port></Ports><Roles /><Host Name=\"BizTalkServerApplication\" NTGroupName=\"BizTalk Application Users\" Type=\"1\" Trusted=\"false\" /></Service><Service Name=\"OrchsToConvert.CallOrch\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports><Port Name=\"Port_1\" Modifier=\"2\" BindingOption=\"1\"><SendPortRef xsi:nil=\"true\" /><DistributionListRef xsi:nil=\"true\" /><ReceivePortRef xsi:nil=\"true\" /></Port><Port Name=\"Port_2\" Modifier=\"1\" BindingOption=\"1\"><SendPortRef xsi:nil=\"true\" /><DistributionListRef xsi:nil=\"true\" /><ReceivePortRef xsi:nil=\"true\" /></Port></Ports><Roles /><Host xsi:nil=\"true\" /></Service><Service Name=\"OrchsToConvert.StartOrch\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports /><Roles /><Host xsi:nil=\"true\" /></Service><Service Name=\"OrchsToConvert.CalleeOrchodx\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports /><Roles /><Host xsi:nil=\"true\" /></Service><Service Name=\"OrchsToConvert.EmptyOrch\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports /><Roles /><Host xsi:nil=\"true\" /></Service><Service Name=\"OrchsToConvert.PartnerBound\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports /><Roles /><Host xsi:nil=\"true\" /></Service><Service Name=\"OrchsToConvert.NackOrch\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports /><Roles /><Host xsi:nil=\"true\" /></Service><Service Name=\"OrchsToConvert.CorrelationSet\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports /><Roles /><Host xsi:nil=\"true\" /></Service><Service Name=\"OrchsToConvert.RoleLInk\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports><Port Name=\"Port_1\" Modifier=\"2\" BindingOption=\"1\"><SendPortRef xsi:nil=\"true\" /><DistributionListRef xsi:nil=\"true\" /><ReceivePortRef xsi:nil=\"true\" /></Port><Port Name=\"__RoleLink_1_PortType_11\" Modifier=\"2\" BindingOption=\"5\"><SendPortRef xsi:nil=\"true\" /><DistributionListRef xsi:nil=\"true\" /><ReceivePortRef xsi:nil=\"true\" /></Port></Ports><Roles><Role Name=\"Provider\" RoleLinkTypeName=\"OrchsToConvert.RoleLinkType_1\" RoleType=\"Uses\"><EnlistedParties /></Role><Role Name=\"Consumer\" RoleLinkTypeName=\"OrchsToConvert.RoleLinkType_1\" RoleType=\"Implements\"><EnlistedParties /></Role></Roles><Host xsi:nil=\"true\" /></Service><Service Name=\"OrchsToConvert.ScatterGather\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports /><Roles /><Host xsi:nil=\"true\" /></Service><Service Name=\"OrchsToConvert.ComplexCorrelation\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports><Port Name=\"Orders\" Modifier=\"2\" BindingOption=\"1\"><SendPortRef xsi:nil=\"true\" /><DistributionListRef xsi:nil=\"true\" /><ReceivePortRef xsi:nil=\"true\" /></Port></Ports><Roles /><Host xsi:nil=\"true\" /></Service><Service Name=\"OrchsToConvert.SequentialCorrelationMultiType\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports><Port Name=\"Port_1\" Modifier=\"2\" BindingOption=\"1\"><SendPortRef xsi:nil=\"true\" /><DistributionListRef xsi:nil=\"true\" /><ReceivePortRef xsi:nil=\"true\" /></Port></Ports><Roles /><Host xsi:nil=\"true\" /></Service><Service Name=\"OrchsToConvert.FilterExpression\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports><Port Name=\"Port_1\" Modifier=\"2\" BindingOption=\"1\"><SendPortRef xsi:nil=\"true\" /><DistributionListRef xsi:nil=\"true\" /><ReceivePortRef xsi:nil=\"true\" /></Port></Ports><Roles /><Host xsi:nil=\"true\" /></Service><Service Name=\"OrchsToConvert.ComplexlSequentialCorrelationOneTypeType\" State=\"Unenlisted\" TrackingOption=\"ServiceStartEnd MessageSendReceive OrchestrationEvents\" AnalyticsEnabled=\"false\" Description=\"\"><Ports><Port Name=\"Port_1\" Modifier=\"2\" BindingOption=\"1\"><SendPortRef xsi:nil=\"true\" /><DistributionListRef xsi:nil=\"true\" /><ReceivePortRef xsi:nil=\"true\" /></Port></Ports><Roles /><Host xsi:nil=\"true\" /></Service></Services><TrackedSchemas /></ModuleRef><ModuleRef Name=\"#\" Version=\"0.0\" Culture=\"neutral\" PublicKeyToken=\"\" FullName=\"#, Version=0.0, Culture=neutral, PublicKeyToken=\"><Services /><TrackedSchemas /></ModuleRef></ModuleRefCollection><SendPortCollection /><DistributionListCollection /><ReceivePortCollection><ReceivePort Name=\"RequestReplyHttpReceive\" IsTwoWay=\"true\" BindingOption=\"1\" AnalyticsEnabled=\"false\"><Description xsi:nil=\"true\" /><ReceiveLocations><ReceiveLocation Name=\"RequestReplyHttpReceiveLocation\"><Description xsi:nil=\"true\" /><Address>/requestreply/service.svc</Address><PublicAddress /><Primary>true</Primary><ReceiveLocationServiceWindowEnabled>false</ReceiveLocationServiceWindowEnabled><ReceiveLocationFromTime>2000-01-01T23:00:00</ReceiveLocationFromTime><ReceiveLocationToTime>2000-01-01T22:59:59</ReceiveLocationToTime><ReceiveLocationStartDateEnabled>false</ReceiveLocationStartDateEnabled><ReceiveLocationStartDate>2014-10-06T00:00:00</ReceiveLocationStartDate><ReceiveLocationEndDateEnabled>false</ReceiveLocationEndDateEnabled><ReceiveLocationEndDate>2014-10-07T23:59:59</ReceiveLocationEndDate><ReceiveLocationTransportType Name=\"WCF-BasicHttp\" Capabilities=\"899\" ConfigurationClsid=\"467c1a52-373f-4f09-9008-27af6b985f14\" /><ReceiveLocationTransportTypeData>&lt;CustomProps&gt;&lt;InboundNodeEncoding vt=\"8\"&gt;Xml&lt;/InboundNodeEncoding&gt;&lt;MessageEncoding vt=\"8\"&gt;Text&lt;/MessageEncoding&gt;&lt;ServiceCertificate vt=\"8\" /&gt;&lt;InboundBodyPathExpression vt=\"8\" /&gt;&lt;OutboundBodyLocation vt=\"8\"&gt;UseBodyElement&lt;/OutboundBodyLocation&gt;&lt;MaxReceivedMessageSize vt=\"3\"&gt;65536&lt;/MaxReceivedMessageSize&gt;&lt;InboundBodyLocation vt=\"8\"&gt;UseBodyElement&lt;/InboundBodyLocation&gt;&lt;AlgorithmSuite vt=\"8\"&gt;Basic256&lt;/AlgorithmSuite&gt;&lt;OutboundXmlTemplate vt=\"8\"&gt;&amp;lt;bts-msg-body xmlns=\"http://www.microsoft.com/schemas/bts2007\" encoding=\"xml\"/&amp;gt;&lt;/OutboundXmlTemplate&gt;&lt;TextEncoding vt=\"8\"&gt;utf-8&lt;/TextEncoding&gt;&lt;SecurityMode vt=\"8\"&gt;None&lt;/SecurityMode&gt;&lt;IncludeExceptionDetailInFaults vt=\"11\"&gt;0&lt;/IncludeExceptionDetailInFaults&gt;&lt;TransportClientCredentialType vt=\"8\"&gt;None&lt;/TransportClientCredentialType&gt;&lt;OpenTimeout vt=\"8\"&gt;00:01:00&lt;/OpenTimeout&gt;&lt;MessageClientCredentialType vt=\"8\"&gt;UserName&lt;/MessageClientCredentialType&gt;&lt;UseSSO vt=\"11\"&gt;0&lt;/UseSSO&gt;&lt;SuspendMessageOnFailure vt=\"11\"&gt;0&lt;/SuspendMessageOnFailure&gt;&lt;CloseTimeout vt=\"8\"&gt;00:01:00&lt;/CloseTimeout&gt;&lt;SendTimeout vt=\"8\"&gt;00:01:00&lt;/SendTimeout&gt;&lt;MaxConcurrentCalls vt=\"3\"&gt;200&lt;/MaxConcurrentCalls&gt;&lt;/CustomProps&gt;</ReceiveLocationTransportTypeData><ReceivePipeline Name=\"Microsoft.BizTalk.DefaultPipelines.PassThruReceive\" FullyQualifiedName=\"Microsoft.BizTalk.DefaultPipelines.PassThruReceive, Microsoft.BizTalk.DefaultPipelines, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Type=\"1\" TrackingOption=\"ServiceStartEnd MessageSendReceive PipelineEvents\" Description=\"\" /><ReceivePipelineData xsi:nil=\"true\" /><SendPipeline Name=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit\" FullyQualifiedName=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit, Microsoft.BizTalk.DefaultPipelines, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Type=\"2\" TrackingOption=\"ServiceStartEnd MessageSendReceive PipelineEvents\" Description=\"\" /><SendPipelineData xsi:nil=\"true\" /><Enable>false</Enable><ReceiveHandler Name=\"BizTalkServerIsolatedHost\" HostTrusted=\"false\"><TransportType Name=\"WCF-BasicHttp\" Capabilities=\"899\" ConfigurationClsid=\"467c1a52-373f-4f09-9008-27af6b985f14\" /></ReceiveHandler><ScheduleRecurrenceType>None</ScheduleRecurrenceType><ScheduleRecurenceFrom>2020-03-28T00:00:00</ScheduleRecurenceFrom><ScheduleRecurrenceInterval>1</ScheduleRecurrenceInterval><ScheduleDaysOfWeek>None</ScheduleDaysOfWeek><ScheduleMonths>None</ScheduleMonths><ScheduleMonthDays>None</ScheduleMonthDays><ScheduleOrdinalDayOfWeek>None</ScheduleOrdinalDayOfWeek><ScheduleOrdinalType>None</ScheduleOrdinalType><ScheduleIsOrdinal>false</ScheduleIsOrdinal><ScheduleLastDayOfMonth>false</ScheduleLastDayOfMonth><ScheduleAutoAdjustToDaylightSaving>true</ScheduleAutoAdjustToDaylightSaving><ScheduleTimeZone>UTC</ScheduleTimeZone></ReceiveLocation></ReceiveLocations><SendPipelineData xsi:nil=\"true\" /><Authentication>0</Authentication><Tracking>0</Tracking><Transforms /><OutboundTransforms /><RouteFailedMessage>false</RouteFailedMessage><ApplicationName>Thunderstorm.Test</ApplicationName></ReceivePort></ReceivePortCollection><PartyCollection xsi:nil=\"true\" /></BindingInfo>";

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
                .x(() => e = Record.Exception(() => new BindingFileParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new BindingFileParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => new BindingFileParser(model, context, logger)));

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
                .x(() => e = Record.Exception(() => parser = new BindingFileParser(model, context, logger)));

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
        public void ParseIsSkippedIfModelIsMissing(BindingFileParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
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
                .x(() => parser = new BindingFileParser(model, context, logger));

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
        public void ParseSuccessfulSingleApplication(BindingFileParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(new ParsedBizTalkApplication()
                    {
                        ResourceContainerKey = "TestMsi"
                    });

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    model.MigrationSource.ResourceContainers.Add(cabContainer);
                    var bindings = GoodBindings;
                    var resource = new ResourceDefinition() { Key = "BindingInfo.Key", Name = "BindingInfo", Type = ModelConstants.ResourceDefinitionBindings, ResourceContent = bindings };
                    cabContainer.ResourceDefinitions.Add(resource);

                    group.Applications[0].Application.Bindings = new BindingFile(cabContainer.Key, resource.Key);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object); 
            
            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new BindingFileParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the bindings should have deserialized correctly"
                .x(() =>
                {
                    group.Applications[0].Application.Bindings.BindingInfo.Should().NotBeNull();
                    var moduleRef = group.Applications[0].Application.Bindings.BindingInfo.ModuleRefCollection[1];
                    moduleRef.Name.Should().Be("OrchsToConvert");
                    moduleRef.Version.Should().Be("1.0.0.0");
                    moduleRef.Culture.Should().Be("neutral");
                    moduleRef.PublicKeyToken.Should().Be("63a3d0af71fe5e2a");
                    moduleRef.FullName.Should().Be("OrchsToConvert, Version=1.0.0.0, Culture=neutral, PublicKeyToken=63a3d0af71fe5e2a");
                });
        }

        /// <summary>
        /// Tests that a binding file with send port group is parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulSingleSendServices(BindingFileParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var serviceRefName = "serviceRefName";
            var serviceRefDescription = "serviceRefDescription";

            "Given a model"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo
                        {
                            ModuleRefCollection = new ModuleRef[]
                            {
                                new ModuleRef
                                {
                                    Name = "modeulRefName",
                                    Services = new ServiceRef[]
                                    {
                                        new ServiceRef{ Name = serviceRefName, Description = serviceRefDescription }
                                    }
                                }
                            }
                    };

                    var bindingInfoXml = string.Empty;

                    using (var stringwriter = new System.IO.StringWriter())
                    {
                        var serializer = new XmlSerializer(bindingInfo.GetType());
                        serializer.Serialize(stringwriter, bindingInfo);
                        bindingInfoXml = stringwriter.ToString();
                    }

                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(new ParsedBizTalkApplication()
                    {
                        ResourceContainerKey = "TestMsi"
                    });

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    model.MigrationSource.ResourceContainers.Add(cabContainer);
                    
                    var resource = new ResourceDefinition() { Key = "BindingInfo.Key", Name = "BindingInfo", Type = ModelConstants.ResourceDefinitionBindings, ResourceContent = bindingInfoXml };
                    cabContainer.ResourceDefinitions.Add(resource);

                    group.Applications[0].Application.Bindings = new BindingFile(cabContainer.Key, resource.Key);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new BindingFileParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the bindings should have deserialized correctly"
                .x(() =>
                {
                    group.Applications[0].Application.Bindings.BindingInfo.Should().NotBeNull();                    
                });

            "And the resource should have been created"
                .x(() =>
                {
                    // Check the service has been created.
                    model.MigrationSource.ResourceContainers[1].ResourceDefinitions[0].Resources.Should().HaveCount(1);
                    var serviceReference = model.MigrationSource.ResourceContainers[1].ResourceDefinitions[0].Resources[0];
                    serviceReference.Name.Should().Be(serviceRefName);
                    serviceReference.Description.Should().Be(serviceRefDescription);
                    serviceReference.Type.Should().Be(ModelConstants.ResourceServiceBinding);
                    serviceReference.ParentRefId.Should().Be(model.MigrationSource.ResourceContainers[1].ResourceDefinitions[0].RefId);
                    serviceReference.SourceObject.Should().NotBeNull();
                });
        }

        /// <summary>
        /// Tests that a binding file with send port group is parsed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulSingleSendPortGroup(BindingFileParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(new ParsedBizTalkApplication()
                    {
                        ResourceContainerKey = "TestMsi"
                    });

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    model.MigrationSource.ResourceContainers.Add(cabContainer);
                    var bindings = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<BindingInfo xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Assembly=\"Microsoft.BizTalk.Deployment, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Version=\"3.5.1.0\" BindingStatus=\"NoBindings\" BoundEndpoints=\"0\" TotalEndpoints=\"0\">\r\n  <Timestamp>2020-04-06T16:47:47.0044569+01:00</Timestamp>\r\n  <ModuleRefCollection>\r\n    <ModuleRef Name=\"[Application:SimpleMessagingApplication]\" Version=\"\" Culture=\"\" PublicKeyToken=\"\" FullName=\"[Application:SimpleMessagingApplication], Version=, Culture=, PublicKeyToken=\">\r\n      <Services />\r\n      <TrackedSchemas />\r\n    </ModuleRef>\r\n    <ModuleRef Name=\"#\" Version=\"0.0\" Culture=\"neutral\" PublicKeyToken=\"\" FullName=\"#, Version=0.0, Culture=neutral, PublicKeyToken=\">\r\n      <Services />\r\n      <TrackedSchemas />\r\n    </ModuleRef>\r\n  </ModuleRefCollection>\r\n  <SendPortCollection>\r\n    <SendPort Name=\"SimpleFileSendPort\" IsStatic=\"true\" IsTwoWay=\"false\" BindingOption=\"0\" AnalyticsEnabled=\"false\">\r\n      <Description xsi:nil=\"true\" />\r\n      <TransmitPipeline Name=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit\" FullyQualifiedName=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit, Microsoft.BizTalk.DefaultPipelines, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Type=\"2\" TrackingOption=\"ServiceStartEnd MessageSendReceive PipelineEvents\" Description=\"\" />\r\n      <PrimaryTransport>\r\n        <Address>C:\\BizTalk\\Outbound\\%MessageID%.xml</Address>\r\n        <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n        <TransportTypeData>&lt;CustomProps&gt;&lt;UseTempFileOnWrite vt=\"11\"&gt;0&lt;/UseTempFileOnWrite&gt;&lt;AllowCacheOnWrite vt=\"11\"&gt;0&lt;/AllowCacheOnWrite&gt;&lt;CopyMode vt=\"19\"&gt;1&lt;/CopyMode&gt;&lt;FileName vt=\"8\"&gt;%MessageID%.xml&lt;/FileName&gt;&lt;/CustomProps&gt;</TransportTypeData>\r\n        <RetryCount>3</RetryCount>\r\n        <RetryInterval>5</RetryInterval>\r\n        <ServiceWindowEnabled>false</ServiceWindowEnabled>\r\n        <FromTime>2020-03-28T00:00:00</FromTime>\r\n        <ToTime>2020-03-28T23:59:59</ToTime>\r\n        <Primary>true</Primary>\r\n        <OrderedDelivery>false</OrderedDelivery>\r\n        <DeliveryNotification>1</DeliveryNotification>\r\n        <SendHandler Name=\"BizTalkServerApplication\" HostTrusted=\"false\">\r\n          <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n        </SendHandler>\r\n      </PrimaryTransport>\r\n      <SecondaryTransport>\r\n        <Address />\r\n        <RetryCount>3</RetryCount>\r\n        <RetryInterval>5</RetryInterval>\r\n        <ServiceWindowEnabled>false</ServiceWindowEnabled>\r\n        <FromTime>2020-03-28T00:00:00</FromTime>\r\n        <ToTime>2020-03-28T23:59:59</ToTime>\r\n        <Primary>false</Primary>\r\n        <OrderedDelivery>false</OrderedDelivery>\r\n        <DeliveryNotification>1</DeliveryNotification>\r\n        <SendHandler xsi:nil=\"true\" />\r\n      </SecondaryTransport>\r\n      <ReceivePipelineData xsi:nil=\"true\" />\r\n      <Tracking>0</Tracking>\r\n      <Filter>&lt;?xml version=\"1.0\" encoding=\"utf-16\"?&gt;\r\n&lt;Filter xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"&gt;\r\n  &lt;Group&gt;\r\n    &lt;Statement Property=\"BTS.ReceivePortName\" Operator=\"0\" Value=\"OneWayReceivePort\" /&gt;\r\n    &lt;Statement Property=\"BTS.ReceivePipelineID\" Operator=\"5\" /&gt;\r\n  &lt;/Group&gt;\r\n&lt;/Filter&gt;</Filter>\r\n      <Transforms />\r\n      <OrderedDelivery>false</OrderedDelivery>\r\n      <Priority>5</Priority>\r\n      <StopSendingOnFailure>false</StopSendingOnFailure>\r\n      <RouteFailedMessage>false</RouteFailedMessage>\r\n      <ApplicationName>SimpleMessagingApplication</ApplicationName>\r\n    </SendPort>\r\n    <SendPort Name=\"SendPortWithORPredicate\" IsStatic=\"true\" IsTwoWay=\"false\" BindingOption=\"0\" AnalyticsEnabled=\"false\">\r\n      <Description xsi:nil=\"true\" />\r\n      <TransmitPipeline Name=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit\" FullyQualifiedName=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit, Microsoft.BizTalk.DefaultPipelines, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Type=\"2\" TrackingOption=\"ServiceStartEnd MessageSendReceive PipelineEvents\" Description=\"\" />\r\n      <PrimaryTransport>\r\n        <Address>C:\\BizTalk\\Outbound\\%MessageID%.xml</Address>\r\n        <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n        <TransportTypeData>&lt;CustomProps&gt;&lt;UseTempFileOnWrite vt=\"11\"&gt;0&lt;/UseTempFileOnWrite&gt;&lt;AllowCacheOnWrite vt=\"11\"&gt;0&lt;/AllowCacheOnWrite&gt;&lt;CopyMode vt=\"19\"&gt;1&lt;/CopyMode&gt;&lt;FileName vt=\"8\"&gt;%MessageID%.xml&lt;/FileName&gt;&lt;/CustomProps&gt;</TransportTypeData>\r\n        <RetryCount>3</RetryCount>\r\n        <RetryInterval>5</RetryInterval>\r\n        <ServiceWindowEnabled>false</ServiceWindowEnabled>\r\n        <FromTime>2020-03-28T00:00:00</FromTime>\r\n        <ToTime>2020-03-28T23:59:59</ToTime>\r\n        <Primary>true</Primary>\r\n        <OrderedDelivery>false</OrderedDelivery>\r\n        <DeliveryNotification>1</DeliveryNotification>\r\n        <SendHandler Name=\"BizTalkServerApplication\" HostTrusted=\"false\">\r\n          <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n        </SendHandler>\r\n      </PrimaryTransport>\r\n      <SecondaryTransport>\r\n        <Address />\r\n        <RetryCount>3</RetryCount>\r\n        <RetryInterval>5</RetryInterval>\r\n        <ServiceWindowEnabled>false</ServiceWindowEnabled>\r\n        <FromTime>2020-03-28T00:00:00</FromTime>\r\n        <ToTime>2020-03-28T23:59:59</ToTime>\r\n        <Primary>false</Primary>\r\n        <OrderedDelivery>false</OrderedDelivery>\r\n        <DeliveryNotification>1</DeliveryNotification>\r\n        <SendHandler xsi:nil=\"true\" />\r\n      </SecondaryTransport>\r\n      <ReceivePipelineData xsi:nil=\"true\" />\r\n      <Tracking>0</Tracking>\r\n      <Filter>&lt;?xml version=\"1.0\" encoding=\"utf-16\"?&gt;\r\n&lt;Filter xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"&gt;\r\n  &lt;Group&gt;\r\n    &lt;Statement Property=\"FILE.ReceivedFileName\" Operator=\"6\" /&gt;\r\n    &lt;Statement Property=\"FILE.FileCreationTime\" Operator=\"3\" Value=\"DateTime.Now()\" /&gt;\r\n    &lt;Statement Property=\"Microsoft.BizTalk.XLANGs.BTXEngine.OriginatorSID\" Operator=\"3\" Value=\"Test\" /&gt;\r\n  &lt;/Group&gt;\r\n  &lt;Group&gt;\r\n    &lt;Statement Property=\"AzureBlobStorage.BlobName\" Operator=\"2\" Value=\"X\" /&gt;\r\n    &lt;Statement Property=\"AzureBlobStorage.CreateTime\" Operator=\"4\" Value=\"Y\" /&gt;\r\n  &lt;/Group&gt;\r\n  &lt;Group&gt;\r\n    &lt;Statement Property=\"AzureBlobStorage.BlobType\" Operator=\"0\" Value=\"Z\" /&gt;\r\n  &lt;/Group&gt;\r\n&lt;/Filter&gt;</Filter>\r\n      <Transforms />\r\n      <OrderedDelivery>false</OrderedDelivery>\r\n      <Priority>5</Priority>\r\n      <StopSendingOnFailure>false</StopSendingOnFailure>\r\n      <RouteFailedMessage>false</RouteFailedMessage>\r\n      <ApplicationName>SimpleMessagingApplication</ApplicationName>\r\n    </SendPort>\r\n    <SendPort Name=\"SendPortWithReferencedProperty\" IsStatic=\"true\" IsTwoWay=\"false\" BindingOption=\"0\" AnalyticsEnabled=\"false\">\r\n      <Description xsi:nil=\"true\" />\r\n      <TransmitPipeline Name=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit\" FullyQualifiedName=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit, Microsoft.BizTalk.DefaultPipelines, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Type=\"2\" TrackingOption=\"ServiceStartEnd MessageSendReceive PipelineEvents\" Description=\"\" />\r\n      <PrimaryTransport>\r\n        <Address>C:\\BizTalk\\Inbound\\%MessageID%.xml</Address>\r\n        <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n        <TransportTypeData>&lt;CustomProps&gt;&lt;UseTempFileOnWrite vt=\"11\"&gt;0&lt;/UseTempFileOnWrite&gt;&lt;AllowCacheOnWrite vt=\"11\"&gt;0&lt;/AllowCacheOnWrite&gt;&lt;CopyMode vt=\"19\"&gt;1&lt;/CopyMode&gt;&lt;FileName vt=\"8\"&gt;%MessageID%.xml&lt;/FileName&gt;&lt;/CustomProps&gt;</TransportTypeData>\r\n        <RetryCount>3</RetryCount>\r\n        <RetryInterval>5</RetryInterval>\r\n        <ServiceWindowEnabled>false</ServiceWindowEnabled>\r\n        <FromTime>2020-04-05T23:00:00</FromTime>\r\n        <ToTime>2020-04-06T22:59:59</ToTime>\r\n        <Primary>true</Primary>\r\n        <OrderedDelivery>false</OrderedDelivery>\r\n        <DeliveryNotification>1</DeliveryNotification>\r\n        <SendHandler Name=\"BizTalkServerApplication\" HostTrusted=\"false\">\r\n          <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n        </SendHandler>\r\n      </PrimaryTransport>\r\n      <SecondaryTransport>\r\n        <Address />\r\n        <RetryCount>3</RetryCount>\r\n        <RetryInterval>5</RetryInterval>\r\n        <ServiceWindowEnabled>false</ServiceWindowEnabled>\r\n        <FromTime>2020-04-05T23:00:00</FromTime>\r\n        <ToTime>2020-04-06T22:59:59</ToTime>\r\n        <Primary>false</Primary>\r\n        <OrderedDelivery>false</OrderedDelivery>\r\n        <DeliveryNotification>1</DeliveryNotification>\r\n        <SendHandler xsi:nil=\"true\" />\r\n      </SecondaryTransport>\r\n      <ReceivePipelineData xsi:nil=\"true\" />\r\n      <Tracking>0</Tracking>\r\n      <Filter>&lt;?xml version=\"1.0\" encoding=\"utf-16\"?&gt;\r\n&lt;Filter xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"&gt;\r\n  &lt;Group&gt;\r\n    &lt;Statement Property=\"Microsoft.AzureIntegrationMigration.BizTalk.TestApps.Schemas.TestStringProperty1\" Operator=\"0\" Value=\"MatchThisOne\" /&gt;\r\n  &lt;/Group&gt;\r\n&lt;/Filter&gt;</Filter>\r\n      <Transforms />\r\n      <OrderedDelivery>false</OrderedDelivery>\r\n      <Priority>5</Priority>\r\n      <StopSendingOnFailure>false</StopSendingOnFailure>\r\n      <RouteFailedMessage>false</RouteFailedMessage>\r\n      <ApplicationName>SimpleMessagingApplication</ApplicationName>\r\n    </SendPort>\r\n  </SendPortCollection>\r\n  <DistributionListCollection>\r\n    <DistributionList Name=\"TestSendPortGroup\">\r\n      <SendPorts>\r\n        <SendPortRef Name=\"SendPortWithORPredicate\" />\r\n        <SendPortRef Name=\"SimpleFileSendPort\" />\r\n      </SendPorts>\r\n      <Filter>&lt;?xml version=\"1.0\" encoding=\"utf-16\"?&gt;\r\n&lt;Filter xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"&gt;\r\n  &lt;Group&gt;\r\n    &lt;Statement Property=\"AzureBlobStorage.BlobName\" Operator=\"0\" Value=\"Blobby\" /&gt;\r\n  &lt;/Group&gt;\r\n&lt;/Filter&gt;</Filter>\r\n      <ApplicationName>SimpleMessagingApplication</ApplicationName>\r\n      <Description xsi:nil=\"true\" />\r\n    </DistributionList>\r\n  </DistributionListCollection>\r\n  <ReceivePortCollection>\r\n    <ReceivePort Name=\"OneWayReceivePort\" IsTwoWay=\"false\" BindingOption=\"0\" AnalyticsEnabled=\"false\">\r\n      <Description xsi:nil=\"true\" />\r\n      <ReceiveLocations>\r\n        <ReceiveLocation Name=\"FileReceiveLocation\">\r\n          <Description xsi:nil=\"true\" />\r\n          <Address>C:\\BizTalk\\Inbound\\*.xml</Address>\r\n          <PublicAddress />\r\n          <Primary>true</Primary>\r\n          <ReceiveLocationServiceWindowEnabled>false</ReceiveLocationServiceWindowEnabled>\r\n          <ReceiveLocationFromTime>2020-03-24T00:00:00</ReceiveLocationFromTime>\r\n          <ReceiveLocationToTime>2020-03-24T23:59:59</ReceiveLocationToTime>\r\n          <ReceiveLocationStartDateEnabled>false</ReceiveLocationStartDateEnabled>\r\n          <ReceiveLocationStartDate>2020-03-24T00:00:00</ReceiveLocationStartDate>\r\n          <ReceiveLocationEndDateEnabled>false</ReceiveLocationEndDateEnabled>\r\n          <ReceiveLocationEndDate>2020-03-25T00:00:00</ReceiveLocationEndDate>\r\n          <ReceiveLocationTransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n          <ReceiveLocationTransportTypeData>&lt;CustomProps&gt;&lt;RenameReceivedFiles vt=\"11\"&gt;0&lt;/RenameReceivedFiles&gt;&lt;RemoveReceivedFileDelay vt=\"19\"&gt;10&lt;/RemoveReceivedFileDelay&gt;&lt;RemoveReceivedFileMaxInterval vt=\"19\"&gt;300000&lt;/RemoveReceivedFileMaxInterval&gt;&lt;FileMask vt=\"8\"&gt;*.xml&lt;/FileMask&gt;&lt;FileNetFailRetryInt vt=\"19\"&gt;5&lt;/FileNetFailRetryInt&gt;&lt;RemoveReceivedFileRetryCount vt=\"19\"&gt;5&lt;/RemoveReceivedFileRetryCount&gt;&lt;BatchSizeInBytes vt=\"19\"&gt;102400&lt;/BatchSizeInBytes&gt;&lt;PollingInterval vt=\"19\"&gt;60000&lt;/PollingInterval&gt;&lt;FileNetFailRetryCount vt=\"19\"&gt;5&lt;/FileNetFailRetryCount&gt;&lt;BatchSize vt=\"19\"&gt;20&lt;/BatchSize&gt;&lt;/CustomProps&gt;</ReceiveLocationTransportTypeData>\r\n          <ReceivePipeline Name=\"Microsoft.BizTalk.DefaultPipelines.PassThruReceive\" FullyQualifiedName=\"Microsoft.BizTalk.DefaultPipelines.PassThruReceive, Microsoft.BizTalk.DefaultPipelines, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Type=\"1\" TrackingOption=\"ServiceStartEnd MessageSendReceive PipelineEvents\" Description=\"\" />\r\n          <ReceivePipelineData xsi:nil=\"true\" />\r\n          <SendPipeline xsi:nil=\"true\" />\r\n          <SendPipelineData xsi:nil=\"true\" />\r\n          <Enable>false</Enable>\r\n          <ReceiveHandler Name=\"BizTalkServerApplication\" HostTrusted=\"false\">\r\n            <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n          </ReceiveHandler>\r\n          <ScheduleRecurrenceType>Day</ScheduleRecurrenceType>\r\n          <ScheduleRecurenceFrom>2020-03-24T00:00:00</ScheduleRecurenceFrom>\r\n          <ScheduleRecurrenceInterval>1</ScheduleRecurrenceInterval>\r\n          <ScheduleDaysOfWeek>None</ScheduleDaysOfWeek>\r\n          <ScheduleMonths>None</ScheduleMonths>\r\n          <ScheduleMonthDays>None</ScheduleMonthDays>\r\n          <ScheduleOrdinalDayOfWeek>None</ScheduleOrdinalDayOfWeek>\r\n          <ScheduleOrdinalType>None</ScheduleOrdinalType>\r\n          <ScheduleIsOrdinal>true</ScheduleIsOrdinal>\r\n          <ScheduleLastDayOfMonth>false</ScheduleLastDayOfMonth>\r\n          <ScheduleAutoAdjustToDaylightSaving>true</ScheduleAutoAdjustToDaylightSaving>\r\n          <ScheduleTimeZone>UTC</ScheduleTimeZone>\r\n        </ReceiveLocation>\r\n      </ReceiveLocations>\r\n      <SendPipelineData xsi:nil=\"true\" />\r\n      <Authentication>0</Authentication>\r\n      <Tracking>0</Tracking>\r\n      <Transforms />\r\n      <RouteFailedMessage>false</RouteFailedMessage>\r\n      <ApplicationName>SimpleMessagingApplication</ApplicationName>\r\n    </ReceivePort>\r\n  </ReceivePortCollection>\r\n  <PartyCollection xsi:nil=\"true\" />\r\n</BindingInfo>";
                    var resource = new ResourceDefinition() { Key = "BindingInfo.Key", Name = "BindingInfo", Type = ModelConstants.ResourceDefinitionBindings, ResourceContent = bindings };
                    cabContainer.ResourceDefinitions.Add(resource);

                    group.Applications[0].Application.Bindings = new BindingFile(cabContainer.Key, resource.Key);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new BindingFileParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the bindings should have deserialized correctly"
                .x(() =>
                {
                    group.Applications[0].Application.Bindings.BindingInfo.Should().NotBeNull();
                    group.Applications[0].Application.Bindings.BindingInfo.DistributionListCollection.Length.Should().Be(1);
                    group.Applications[0].Application.Bindings.BindingInfo.DistributionListCollection[0].SendPorts.Length.Should().Be(2);
                });
        }

        /// <summary>
        /// Scenario tests when the binding resource definition is missing.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureMisingBindingResourceDefinition(BindingFileParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var wrongKey = "wrongKey";

            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(new ParsedBizTalkApplication());
                    model.MigrationSource.MigrationSourceModel = group;

                    var container = new ResourceContainer() { Key = "BindingContainer.Key", Name = "Test", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    model.MigrationSource.ResourceContainers.Add(container);
                    var bindings = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<BindingInfo xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Assembly=\"Microsoft.BizTalk.Deployment, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Version=\"3.5.1.0\" BindingStatus=\"NoBindings\" BoundEndpoints=\"0\" TotalEndpoints=\"0\">\r\n  <Timestamp>2020-04-06T16:47:47.0044569+01:00</Timestamp>\r\n  <ModuleRefCollection>\r\n    <ModuleRef Name=\"[Application:SimpleMessagingApplication]\" Version=\"\" Culture=\"\" PublicKeyToken=\"\" FullName=\"[Application:SimpleMessagingApplication], Version=, Culture=, PublicKeyToken=\">\r\n      <Services />\r\n      <TrackedSchemas />\r\n    </ModuleRef>\r\n    <ModuleRef Name=\"#\" Version=\"0.0\" Culture=\"neutral\" PublicKeyToken=\"\" FullName=\"#, Version=0.0, Culture=neutral, PublicKeyToken=\">\r\n      <Services />\r\n      <TrackedSchemas />\r\n    </ModuleRef>\r\n  </ModuleRefCollection>\r\n  <SendPortCollection>\r\n    <SendPort Name=\"SimpleFileSendPort\" IsStatic=\"true\" IsTwoWay=\"false\" BindingOption=\"0\" AnalyticsEnabled=\"false\">\r\n      <Description xsi:nil=\"true\" />\r\n      <TransmitPipeline Name=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit\" FullyQualifiedName=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit, Microsoft.BizTalk.DefaultPipelines, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Type=\"2\" TrackingOption=\"ServiceStartEnd MessageSendReceive PipelineEvents\" Description=\"\" />\r\n      <PrimaryTransport>\r\n        <Address>C:\\BizTalk\\Outbound\\%MessageID%.xml</Address>\r\n        <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n        <TransportTypeData>&lt;CustomProps&gt;&lt;UseTempFileOnWrite vt=\"11\"&gt;0&lt;/UseTempFileOnWrite&gt;&lt;AllowCacheOnWrite vt=\"11\"&gt;0&lt;/AllowCacheOnWrite&gt;&lt;CopyMode vt=\"19\"&gt;1&lt;/CopyMode&gt;&lt;FileName vt=\"8\"&gt;%MessageID%.xml&lt;/FileName&gt;&lt;/CustomProps&gt;</TransportTypeData>\r\n        <RetryCount>3</RetryCount>\r\n        <RetryInterval>5</RetryInterval>\r\n        <ServiceWindowEnabled>false</ServiceWindowEnabled>\r\n        <FromTime>2020-03-28T00:00:00</FromTime>\r\n        <ToTime>2020-03-28T23:59:59</ToTime>\r\n        <Primary>true</Primary>\r\n        <OrderedDelivery>false</OrderedDelivery>\r\n        <DeliveryNotification>1</DeliveryNotification>\r\n        <SendHandler Name=\"BizTalkServerApplication\" HostTrusted=\"false\">\r\n          <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n        </SendHandler>\r\n      </PrimaryTransport>\r\n      <SecondaryTransport>\r\n        <Address />\r\n        <RetryCount>3</RetryCount>\r\n        <RetryInterval>5</RetryInterval>\r\n        <ServiceWindowEnabled>false</ServiceWindowEnabled>\r\n        <FromTime>2020-03-28T00:00:00</FromTime>\r\n        <ToTime>2020-03-28T23:59:59</ToTime>\r\n        <Primary>false</Primary>\r\n        <OrderedDelivery>false</OrderedDelivery>\r\n        <DeliveryNotification>1</DeliveryNotification>\r\n        <SendHandler xsi:nil=\"true\" />\r\n      </SecondaryTransport>\r\n      <ReceivePipelineData xsi:nil=\"true\" />\r\n      <Tracking>0</Tracking>\r\n      <Filter>&lt;?xml version=\"1.0\" encoding=\"utf-16\"?&gt;\r\n&lt;Filter xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"&gt;\r\n  &lt;Group&gt;\r\n    &lt;Statement Property=\"BTS.ReceivePortName\" Operator=\"0\" Value=\"OneWayReceivePort\" /&gt;\r\n    &lt;Statement Property=\"BTS.ReceivePipelineID\" Operator=\"5\" /&gt;\r\n  &lt;/Group&gt;\r\n&lt;/Filter&gt;</Filter>\r\n      <Transforms />\r\n      <OrderedDelivery>false</OrderedDelivery>\r\n      <Priority>5</Priority>\r\n      <StopSendingOnFailure>false</StopSendingOnFailure>\r\n      <RouteFailedMessage>false</RouteFailedMessage>\r\n      <ApplicationName>SimpleMessagingApplication</ApplicationName>\r\n    </SendPort>\r\n    <SendPort Name=\"SendPortWithORPredicate\" IsStatic=\"true\" IsTwoWay=\"false\" BindingOption=\"0\" AnalyticsEnabled=\"false\">\r\n      <Description xsi:nil=\"true\" />\r\n      <TransmitPipeline Name=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit\" FullyQualifiedName=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit, Microsoft.BizTalk.DefaultPipelines, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Type=\"2\" TrackingOption=\"ServiceStartEnd MessageSendReceive PipelineEvents\" Description=\"\" />\r\n      <PrimaryTransport>\r\n        <Address>C:\\BizTalk\\Outbound\\%MessageID%.xml</Address>\r\n        <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n        <TransportTypeData>&lt;CustomProps&gt;&lt;UseTempFileOnWrite vt=\"11\"&gt;0&lt;/UseTempFileOnWrite&gt;&lt;AllowCacheOnWrite vt=\"11\"&gt;0&lt;/AllowCacheOnWrite&gt;&lt;CopyMode vt=\"19\"&gt;1&lt;/CopyMode&gt;&lt;FileName vt=\"8\"&gt;%MessageID%.xml&lt;/FileName&gt;&lt;/CustomProps&gt;</TransportTypeData>\r\n        <RetryCount>3</RetryCount>\r\n        <RetryInterval>5</RetryInterval>\r\n        <ServiceWindowEnabled>false</ServiceWindowEnabled>\r\n        <FromTime>2020-03-28T00:00:00</FromTime>\r\n        <ToTime>2020-03-28T23:59:59</ToTime>\r\n        <Primary>true</Primary>\r\n        <OrderedDelivery>false</OrderedDelivery>\r\n        <DeliveryNotification>1</DeliveryNotification>\r\n        <SendHandler Name=\"BizTalkServerApplication\" HostTrusted=\"false\">\r\n          <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n        </SendHandler>\r\n      </PrimaryTransport>\r\n      <SecondaryTransport>\r\n        <Address />\r\n        <RetryCount>3</RetryCount>\r\n        <RetryInterval>5</RetryInterval>\r\n        <ServiceWindowEnabled>false</ServiceWindowEnabled>\r\n        <FromTime>2020-03-28T00:00:00</FromTime>\r\n        <ToTime>2020-03-28T23:59:59</ToTime>\r\n        <Primary>false</Primary>\r\n        <OrderedDelivery>false</OrderedDelivery>\r\n        <DeliveryNotification>1</DeliveryNotification>\r\n        <SendHandler xsi:nil=\"true\" />\r\n      </SecondaryTransport>\r\n      <ReceivePipelineData xsi:nil=\"true\" />\r\n      <Tracking>0</Tracking>\r\n      <Filter>&lt;?xml version=\"1.0\" encoding=\"utf-16\"?&gt;\r\n&lt;Filter xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"&gt;\r\n  &lt;Group&gt;\r\n    &lt;Statement Property=\"FILE.ReceivedFileName\" Operator=\"6\" /&gt;\r\n    &lt;Statement Property=\"FILE.FileCreationTime\" Operator=\"3\" Value=\"DateTime.Now()\" /&gt;\r\n    &lt;Statement Property=\"Microsoft.BizTalk.XLANGs.BTXEngine.OriginatorSID\" Operator=\"3\" Value=\"Test\" /&gt;\r\n  &lt;/Group&gt;\r\n  &lt;Group&gt;\r\n    &lt;Statement Property=\"AzureBlobStorage.BlobName\" Operator=\"2\" Value=\"X\" /&gt;\r\n    &lt;Statement Property=\"AzureBlobStorage.CreateTime\" Operator=\"4\" Value=\"Y\" /&gt;\r\n  &lt;/Group&gt;\r\n  &lt;Group&gt;\r\n    &lt;Statement Property=\"AzureBlobStorage.BlobType\" Operator=\"0\" Value=\"Z\" /&gt;\r\n  &lt;/Group&gt;\r\n&lt;/Filter&gt;</Filter>\r\n      <Transforms />\r\n      <OrderedDelivery>false</OrderedDelivery>\r\n      <Priority>5</Priority>\r\n      <StopSendingOnFailure>false</StopSendingOnFailure>\r\n      <RouteFailedMessage>false</RouteFailedMessage>\r\n      <ApplicationName>SimpleMessagingApplication</ApplicationName>\r\n    </SendPort>\r\n    <SendPort Name=\"SendPortWithReferencedProperty\" IsStatic=\"true\" IsTwoWay=\"false\" BindingOption=\"0\" AnalyticsEnabled=\"false\">\r\n      <Description xsi:nil=\"true\" />\r\n      <TransmitPipeline Name=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit\" FullyQualifiedName=\"Microsoft.BizTalk.DefaultPipelines.PassThruTransmit, Microsoft.BizTalk.DefaultPipelines, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Type=\"2\" TrackingOption=\"ServiceStartEnd MessageSendReceive PipelineEvents\" Description=\"\" />\r\n      <PrimaryTransport>\r\n        <Address>C:\\BizTalk\\Inbound\\%MessageID%.xml</Address>\r\n        <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n        <TransportTypeData>&lt;CustomProps&gt;&lt;UseTempFileOnWrite vt=\"11\"&gt;0&lt;/UseTempFileOnWrite&gt;&lt;AllowCacheOnWrite vt=\"11\"&gt;0&lt;/AllowCacheOnWrite&gt;&lt;CopyMode vt=\"19\"&gt;1&lt;/CopyMode&gt;&lt;FileName vt=\"8\"&gt;%MessageID%.xml&lt;/FileName&gt;&lt;/CustomProps&gt;</TransportTypeData>\r\n        <RetryCount>3</RetryCount>\r\n        <RetryInterval>5</RetryInterval>\r\n        <ServiceWindowEnabled>false</ServiceWindowEnabled>\r\n        <FromTime>2020-04-05T23:00:00</FromTime>\r\n        <ToTime>2020-04-06T22:59:59</ToTime>\r\n        <Primary>true</Primary>\r\n        <OrderedDelivery>false</OrderedDelivery>\r\n        <DeliveryNotification>1</DeliveryNotification>\r\n        <SendHandler Name=\"BizTalkServerApplication\" HostTrusted=\"false\">\r\n          <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n        </SendHandler>\r\n      </PrimaryTransport>\r\n      <SecondaryTransport>\r\n        <Address />\r\n        <RetryCount>3</RetryCount>\r\n        <RetryInterval>5</RetryInterval>\r\n        <ServiceWindowEnabled>false</ServiceWindowEnabled>\r\n        <FromTime>2020-04-05T23:00:00</FromTime>\r\n        <ToTime>2020-04-06T22:59:59</ToTime>\r\n        <Primary>false</Primary>\r\n        <OrderedDelivery>false</OrderedDelivery>\r\n        <DeliveryNotification>1</DeliveryNotification>\r\n        <SendHandler xsi:nil=\"true\" />\r\n      </SecondaryTransport>\r\n      <ReceivePipelineData xsi:nil=\"true\" />\r\n      <Tracking>0</Tracking>\r\n      <Filter>&lt;?xml version=\"1.0\" encoding=\"utf-16\"?&gt;\r\n&lt;Filter xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"&gt;\r\n  &lt;Group&gt;\r\n    &lt;Statement Property=\"Microsoft.AzureIntegrationMigration.BizTalk.TestApps.Schemas.TestStringProperty1\" Operator=\"0\" Value=\"MatchThisOne\" /&gt;\r\n  &lt;/Group&gt;\r\n&lt;/Filter&gt;</Filter>\r\n      <Transforms />\r\n      <OrderedDelivery>false</OrderedDelivery>\r\n      <Priority>5</Priority>\r\n      <StopSendingOnFailure>false</StopSendingOnFailure>\r\n      <RouteFailedMessage>false</RouteFailedMessage>\r\n      <ApplicationName>SimpleMessagingApplication</ApplicationName>\r\n    </SendPort>\r\n  </SendPortCollection>\r\n  <DistributionListCollection>\r\n    <DistributionList Name=\"TestSendPortGroup\">\r\n      <SendPorts>\r\n        <SendPortRef Name=\"SendPortWithORPredicate\" />\r\n        <SendPortRef Name=\"SimpleFileSendPort\" />\r\n      </SendPorts>\r\n      <Filter>&lt;?xml version=\"1.0\" encoding=\"utf-16\"?&gt;\r\n&lt;Filter xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"&gt;\r\n  &lt;Group&gt;\r\n    &lt;Statement Property=\"AzureBlobStorage.BlobName\" Operator=\"0\" Value=\"Blobby\" /&gt;\r\n  &lt;/Group&gt;\r\n&lt;/Filter&gt;</Filter>\r\n      <ApplicationName>SimpleMessagingApplication</ApplicationName>\r\n      <Description xsi:nil=\"true\" />\r\n    </DistributionList>\r\n  </DistributionListCollection>\r\n  <ReceivePortCollection>\r\n    <ReceivePort Name=\"OneWayReceivePort\" IsTwoWay=\"false\" BindingOption=\"0\" AnalyticsEnabled=\"false\">\r\n      <Description xsi:nil=\"true\" />\r\n      <ReceiveLocations>\r\n        <ReceiveLocation Name=\"FileReceiveLocation\">\r\n          <Description xsi:nil=\"true\" />\r\n          <Address>C:\\BizTalk\\Inbound\\*.xml</Address>\r\n          <PublicAddress />\r\n          <Primary>true</Primary>\r\n          <ReceiveLocationServiceWindowEnabled>false</ReceiveLocationServiceWindowEnabled>\r\n          <ReceiveLocationFromTime>2020-03-24T00:00:00</ReceiveLocationFromTime>\r\n          <ReceiveLocationToTime>2020-03-24T23:59:59</ReceiveLocationToTime>\r\n          <ReceiveLocationStartDateEnabled>false</ReceiveLocationStartDateEnabled>\r\n          <ReceiveLocationStartDate>2020-03-24T00:00:00</ReceiveLocationStartDate>\r\n          <ReceiveLocationEndDateEnabled>false</ReceiveLocationEndDateEnabled>\r\n          <ReceiveLocationEndDate>2020-03-25T00:00:00</ReceiveLocationEndDate>\r\n          <ReceiveLocationTransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n          <ReceiveLocationTransportTypeData>&lt;CustomProps&gt;&lt;RenameReceivedFiles vt=\"11\"&gt;0&lt;/RenameReceivedFiles&gt;&lt;RemoveReceivedFileDelay vt=\"19\"&gt;10&lt;/RemoveReceivedFileDelay&gt;&lt;RemoveReceivedFileMaxInterval vt=\"19\"&gt;300000&lt;/RemoveReceivedFileMaxInterval&gt;&lt;FileMask vt=\"8\"&gt;*.xml&lt;/FileMask&gt;&lt;FileNetFailRetryInt vt=\"19\"&gt;5&lt;/FileNetFailRetryInt&gt;&lt;RemoveReceivedFileRetryCount vt=\"19\"&gt;5&lt;/RemoveReceivedFileRetryCount&gt;&lt;BatchSizeInBytes vt=\"19\"&gt;102400&lt;/BatchSizeInBytes&gt;&lt;PollingInterval vt=\"19\"&gt;60000&lt;/PollingInterval&gt;&lt;FileNetFailRetryCount vt=\"19\"&gt;5&lt;/FileNetFailRetryCount&gt;&lt;BatchSize vt=\"19\"&gt;20&lt;/BatchSize&gt;&lt;/CustomProps&gt;</ReceiveLocationTransportTypeData>\r\n          <ReceivePipeline Name=\"Microsoft.BizTalk.DefaultPipelines.PassThruReceive\" FullyQualifiedName=\"Microsoft.BizTalk.DefaultPipelines.PassThruReceive, Microsoft.BizTalk.DefaultPipelines, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" Type=\"1\" TrackingOption=\"ServiceStartEnd MessageSendReceive PipelineEvents\" Description=\"\" />\r\n          <ReceivePipelineData xsi:nil=\"true\" />\r\n          <SendPipeline xsi:nil=\"true\" />\r\n          <SendPipelineData xsi:nil=\"true\" />\r\n          <Enable>false</Enable>\r\n          <ReceiveHandler Name=\"BizTalkServerApplication\" HostTrusted=\"false\">\r\n            <TransportType Name=\"FILE\" Capabilities=\"11\" ConfigurationClsid=\"5e49e3a6-b4fc-4077-b44c-22f34a242fdb\" />\r\n          </ReceiveHandler>\r\n          <ScheduleRecurrenceType>Day</ScheduleRecurrenceType>\r\n          <ScheduleRecurenceFrom>2020-03-24T00:00:00</ScheduleRecurenceFrom>\r\n          <ScheduleRecurrenceInterval>1</ScheduleRecurrenceInterval>\r\n          <ScheduleDaysOfWeek>None</ScheduleDaysOfWeek>\r\n          <ScheduleMonths>None</ScheduleMonths>\r\n          <ScheduleMonthDays>None</ScheduleMonthDays>\r\n          <ScheduleOrdinalDayOfWeek>None</ScheduleOrdinalDayOfWeek>\r\n          <ScheduleOrdinalType>None</ScheduleOrdinalType>\r\n          <ScheduleIsOrdinal>true</ScheduleIsOrdinal>\r\n          <ScheduleLastDayOfMonth>false</ScheduleLastDayOfMonth>\r\n          <ScheduleAutoAdjustToDaylightSaving>true</ScheduleAutoAdjustToDaylightSaving>\r\n          <ScheduleTimeZone>UTC</ScheduleTimeZone>\r\n        </ReceiveLocation>\r\n      </ReceiveLocations>\r\n      <SendPipelineData xsi:nil=\"true\" />\r\n      <Authentication>0</Authentication>\r\n      <Tracking>0</Tracking>\r\n      <Transforms />\r\n      <RouteFailedMessage>false</RouteFailedMessage>\r\n      <ApplicationName>SimpleMessagingApplication</ApplicationName>\r\n    </ReceivePort>\r\n  </ReceivePortCollection>\r\n  <PartyCollection xsi:nil=\"true\" />\r\n</BindingInfo>";
                    var resource = new ResourceDefinition() { Key = "BindingInfo.Key", Name = "BindingInfo", Type = ModelConstants.ResourceDefinitionBindings, ResourceContent = bindings };
                    container.ResourceDefinitions.Add(resource);

                    group.Applications[0].Application.Bindings = new BindingFile(container.Key, wrongKey);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new BindingFileParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be an error when parsing the bindings."
                .x(() =>
                {
                    context.Errors.Count.Should().Be(1);
                    context.Errors[0].Message.Should().Contain(wrongKey);
                    context.Errors[0].Message.Should().Contain(ModelConstants.ResourceDefinitionBindings);
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
        public void ParseFailureInvalidXml(BindingFileParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(new ParsedBizTalkApplication());
                    model.MigrationSource.MigrationSourceModel = group;

                    var container = new ResourceContainer() { Key = "BindingContainer.Key", Name = "Test", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    model.MigrationSource.ResourceContainers.Add(container);
                    var bindings = "<this is not xml>";
                    var resource = new ResourceDefinition() { Key = "BindingInfo.Key", Name = "BindingInfo", Type = ModelConstants.ResourceDefinitionBindings, ResourceContent = bindings };
                    container.ResourceDefinitions.Add(resource);

                    group.Applications[0].Application.Bindings = new BindingFile(container.Key, resource.Key);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new BindingFileParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
                .x(() => e.Should().BeNull());

            "And there should be an error when parsing the bindings."
                .x(() =>
                {
                    context.Errors.Count.Should().Be(1);
                });
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
        public void ParseFailureMissingBindings(BindingFileParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    group = new ParsedBizTalkApplicationGroup();
                    model.MigrationSource.MigrationSourceModel = group;
                    group.Applications.Add(new ParsedBizTalkApplication()
                    {
                        ResourceContainerKey = "TestMsi"
                    });

                    var msiContainer = new ResourceContainer() { Key = "TestMsi.Key", Name = "TestMsi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = @"C:\Test\Test.msi" };
                    model.MigrationSource.ResourceContainers.Add(msiContainer);

                    var cabContainer = new ResourceContainer() { Key = "TestCab.Key", Name = "TestCab", Type = ModelConstants.ResourceContainerCab, ContainerLocation = @"C:\Test\Test.CAB" };
                    model.MigrationSource.ResourceContainers.Add(cabContainer);
                    var bindings = GoodBindings;
                    var resource = new ResourceDefinition() { Key = "BindingInfo.Key", Name = "BindingInfo", Type = ModelConstants.ResourceDefinitionBindings, ResourceContent = bindings };
                    cabContainer.ResourceDefinitions.Add(resource);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new BindingFileParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the parser should not throw an exception"
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
                    invocation.Arguments[2].ToString().Should().Contain("Unable to find the binding info resource");
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
