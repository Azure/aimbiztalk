// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Messages;
using Microsoft.AzureIntegrationMigration.BizTalk.Report.Tests.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Configuration;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.AzureIntegrationMigration.Runner.Engine;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Moq;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report.Tests
{
    public static class TestHelper
    {

        /// <summary>
        /// Builds a migration context.
        /// </summary>
        /// <returns>A migration context.</returns>
        public static MigrationContext BuildContext()
        {
            return new MigrationContext
            {
                WorkingFolder = @"c:\temp\workingfolder",
                ReportFilePath = @"report.html"
            };
        }

        /// <summary>
        /// Builds a source application.
        /// </summary>
        /// <returns>A migration context.</returns>
        public static AzureIntegrationServicesModel BuildModel()
        {
            // Creates the following test structure
            // - msi
            // --- adf file
            // --- cab 0
            // ----- assembly
            // ------- schema
            // --- cab 1
            // ----- binding info
            // ------- application
            var model = new AzureIntegrationServicesModel();

            // MSI
            var msi = new ResourceContainer()
            {
                Type = ModelConstants.ResourceContainerMsi,
                Name = "BTS2010FlatFilePipelineSample"
            };
            model.MigrationSource.ResourceContainers.Add(msi);

            // Application Definition File within MSI
            var adf = new ResourceDefinition()
            {
                Type = ModelConstants.ResourceDefinitionFile,
                Name = "ApplicationDefinition.adf"
            };

            msi.ResourceDefinitions.Add(adf);

            // CAB 0 within MSI
            var cab0 = new ResourceContainer()
            {
                Type = ModelConstants.ResourceContainerCab,
                Name = "ITEM~0"
            };
            msi.ResourceContainers.Add(cab0);

            // BizTalk assembly within CAB 0
            var assembly = new ResourceDefinition()
            {
                Type = ModelConstants.ResourceContainerAssembly,
                Name = "Test.BizTalk.Assembly.dll"
            };
            cab0.ResourceDefinitions.Add(assembly);

            // Schema within assembly
            var schema = new ResourceItem()
            {
                Type = ModelConstants.ResourceDocumentSchema,
                Name = "This is a test schema",
                RefId = "schema-1",
                Rating = ConversionRating.NotSupported
            };
            schema.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Error, Message = ModelResources.ExampleErrorMessage });
            assembly.Resources.Add(schema);

            // Message type within the schema
            var messageType = new ResourceItem()
            {
                Type = ModelConstants.ResourceDocumentSchema,
                Name = "This is a test schema",
                RefId = "message-type-1",
                Rating = ConversionRating.NotSupported
            };
            messageType.Properties.Add("Root element", "Root");
            messageType.ResourceRelationships.Add(new ResourceRelationship(schema.RefId, ResourceRelationshipType.Parent));
            schema.ResourceRelationships.Add(new ResourceRelationship(messageType.RefId, ResourceRelationshipType.Child));
            schema.Resources.Add(messageType);


            // CAB 1 within MSI
            var cab1 = new ResourceContainer()
            {
                Type = ModelConstants.ResourceContainerCab,
                Name = "ITEM~1"
            };
            msi.ResourceContainers.Add(cab1);

            // BindingInfo within CAB 1
            var bindingInfo = new ResourceDefinition()
            {
                Type = ModelConstants.ResourceDefinitionBindings,
                Name = "BindingInfo.xml"
            };
            cab1.ResourceDefinitions.Add(bindingInfo);

            // Application within BindingInfo
            var application = new ResourceItem()
            {
                Type = ModelConstants.ResourceApplication,
                Name = "This is the first application",
                RefId = "application-1",
                Rating = ConversionRating.PartialConversion
            };
            application.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Error, Message = ModelResources.ExampleErrorMessage });
            bindingInfo.Resources.Add(application);

            // Receive Port within application
            var rp = new ResourceItem()
            {
                Type = ModelConstants.ResourceReceivePort,
                Name = "This is a receive port",
                Description = "This is the description for the receive port.",
                RefId = "rp-1",
                Rating = ConversionRating.FullConversionWithFidelityLoss
            };
            rp.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = ModelResources.ExampleWarningMessage });
            rp.ResourceRelationships.Add(new ResourceRelationship(application.RefId, ResourceRelationshipType.Parent));
            application.ResourceRelationships.Add(new ResourceRelationship(rp.RefId, ResourceRelationshipType.Child));
            application.Resources.Add(rp);

            // Receive Location within receive port
            var rl = new ResourceItem()
            {
                Type = ModelConstants.ResourceReceiveLocation,
                Name = "This is a receive location",
                Description = "This is the description for the receive location.",
                RefId = "rl-1",
                Rating = ConversionRating.NoAutomaticConversion
            };
            rl.Properties.Add("Adapter", "FILE");
            rl.Properties.Add("Uri", "C:\\local\\input");
            rl.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Information, Message = ModelResources.ExampleInformationMessage });
            rp.Resources.Add(rl);

            // Build the target model
            BuildTargetModel(model);

            return model;
        }

        /// <summary>
        /// Builds a default run state for test purposes.
        /// </summary>
        /// <param name="model">The application model to include in the run state.</param>
        /// <returns>A run state object.</returns>
        public static RunState BuildRunState(IApplicationModel model)
        {
            var config = new RunnerConfiguration()
            {
                FailFast = true,
                FailStages = Runner.Core.Stages.All
            };
            config.Args.Add("microsoft.cli.findpaths", new string[] { "C:\\source\\repos\\ambiztalk\\src\\Microsoft.AzureIntegrationMigration.BizTalk.StageRunners\\bin\\Debug" });
            config.Args.Add("microsoft.cli.findpatterns", "*StageRunners*.dll");
            config.Args.Add("microsoft.cli.templateconfigpath", "C:\\source\\repos\\ambiztalk\\config");
            config.Args.Add("microsoft.biztalk.msidiscoverer.msidir", "C:\\Temp\\Scenarios");
            config.Args.Add("microsoft.biztalk.msidiscoverer.unpackdir", "C:\\Temp\\Output");

            var stageRunner1 = new Mock<IStageRunner>();
            stageRunner1.Object.Priority = 1;
            stageRunner1.Object.Skip = false;
            config.StageRunners.Add(stageRunner1.Object);

            var stageRunner2 = new Mock<IStageRunner>();
            stageRunner2.Object.Priority = 2;
            stageRunner2.Object.Skip = false;
            config.StageRunners.Add(stageRunner2.Object);

            var state = new RunState(config, model);
            return state;
        }

        /// <summary>
        /// Builds a target model.
        /// </summary>
        /// <param name="model">The model.</param>
        private static void BuildTargetModel(AzureIntegrationServicesModel model)
        {
            model.MigrationTarget.TargetEnvironment = AzureIntegrationServicesTargetEnvironment.Consumption;
            model.MigrationTarget.DeploymentEnvironment = "dev";
            model.MigrationTarget.AzurePrimaryRegion = "UK South";
            model.MigrationTarget.AzureSecondaryRegion = "West US";
            model.MigrationTarget.UniqueDeploymentId = "unit-test";
            model.MigrationTarget.MessageBus = new MessageBus()
            {
                Name = "Message Bus",
                Key = "MessageBus",
                Type = MessagingObjectType.MessageBus,
                Description = "Azure Integration Services Message Bus",
                ResourceMapKey = "messageBus",
                Rating = ConversionRating.NotSupported
            };

            // Message Bus resources
            var resource1 = new TargetResourceTemplate()
            {
                TemplateKey = "deployResources",
                TemplateType = "microsoft.template.powershell",
                ResourceName = "deploy-resources",
                ResourceType = "microsoft.scripts.powershell",
                OutputPath = ""
            };
            resource1.ResourceTemplateFiles.Add("Deploy-All.ps1.liquid");
            resource1.ResourceTemplateFiles.Add("TearDown-All.ps1.liquid");
            model.MigrationTarget.MessageBus.Resources.Add(resource1);

            var resource2 = new TargetResourceTemplate()
            {
                TemplateKey = "messageBusAzureResourceGroup",
                TemplateType = "microsoft.template.arm",
                ResourceName = "arrg-aimmsgbus-dev-uksouth",
                ResourceType = "microsoft.groups.azureresourcegroup",
                OutputPath = "messagebus/messagebusgroup",
            };
            resource2.Tags.Add("ApplicationName", "AIM-MessageBus");
            resource2.Tags.Add("Env", "dev");
            resource2.Parameters.Add("azure_primary_region", "UK South");
            resource2.Parameters.Add("azure_secondary_region", "West US");
            resource2.Parameters.Add("azure_subscription_id", "");
            resource2.ResourceTemplateFiles.Add("messagebus/messagebusgroup/messagebusgroup.dev.parameters.json.liquid");
            resource2.ResourceTemplateFiles.Add("messagebus/messagebusgroup/messagebusgroup.json.liquid");
            model.MigrationTarget.MessageBus.Resources.Add(resource2);

            var resource3 = new TargetResourceTemplate()
            {
                TemplateKey = "deployMessageBusGroup",
                TemplateType = "microsoft.template.powershell",
                ResourceName = "arrg-aimmsgbus-dev-uksouth",
                ResourceType = "microsoft.scripts.powershell",
                OutputPath = "messagebus/messagebusgroup",
            };
            resource3.Parameters.Add("azure_primary_region", "UK South");
            resource3.Parameters.Add("azure_secondary_region", "West US");
            resource3.Parameters.Add("azure_subscription_id", "");
            resource3.ResourceTemplateFiles.Add("messagebus/messagebusgroup/Deploy-10-MessageBusGroup.ps1.liquid");
            resource3.ResourceTemplateFiles.Add("messagebus/messagebusgroup/New-MessageBusGroup.ps1.liquid");
            resource3.ResourceTemplateFiles.Add("messagebus/messagebusgroup/TearDown-10-MessageBusGroup.ps1.liquid");
            resource3.ResourceTemplateFiles.Add("messagebus/messagebusgroup/Remove-MessageBusGroup.ps1.liquid");
            model.MigrationTarget.MessageBus.Resources.Add(resource3);

            var resource4 = new TargetResourceTemplate()
            {
                TemplateKey = "messageBusServiceAzureRole",
                TemplateType = "microsoft.template.json",
                ResourceName = "arapim-aimmsgbussvc-dev",
                ResourceType = "microsoft.security.azurerole",
                OutputPath = "messagebus/messagebusservice"
            };
            resource4.ResourceTemplateFiles.Add("messagebus/messagebusservice/messagebusservice.logicappsrole.json");
            model.MigrationTarget.MessageBus.Resources.Add(resource4);

            // Applications
            var application1 = new Application()
            {
                Name = "BTS2010FlatFilePipelineSample",
                Key = "MessageBus:BTS2010FlatFilePipelineSample",
                Type = MessagingObjectType.Application,
                Description = "Azure Integration Application",
                ResourceMapKey = "applicationBTS2010FlatFilePipelineSample",
                Rating = ConversionRating.NoRating,
            };
            model.MigrationTarget.MessageBus.Applications.Add(application1);

            var appresource1 = new TargetResourceTemplate()
            {
                TemplateKey = "applicationAzureResourceGroupBTS2010FlatFilePipelineSample",
                TemplateType = "microsoft.template.arm",
                ResourceName = "arrg-aimapp-bts2010flatfilepipelinesample-dev-uksouth",
                ResourceType = "microsoft.groups.azureresourcegroup",
                OutputPath = "applications/bts2010flatfilepipelinesample/bts2010flatfilepipelinesamplegroup",
            };
            appresource1.Tags.Add("ApplicationName", "BTS2010FlatFilePipelineSample");
            appresource1.Tags.Add("Env", "dev");
            appresource1.Parameters.Add("azure_primary_region", "UK South");
            appresource1.Parameters.Add("azure_secondary_region", "West US");
            appresource1.Parameters.Add("azure_subscription_id", "");
            appresource1.ResourceTemplateFiles.Add("application/applicationgroup/applicationgroup.json.liquid");
            appresource1.ResourceTemplateFiles.Add("application/applicationgroup/applicationgroup.dev.parameters.json.liquid");
            application1.Resources.Add(appresource1);

            var appresource2 = new TargetResourceTemplate()
            {
                TemplateKey = "deployApplicationGroupBTS2010FlatFilePipelineSample",
                TemplateType = "microsoft.template.powershell",
                ResourceName = "arrg-aimapp-bts2010flatfilepipelinesample-dev-uksouth",
                ResourceType = "microsoft.scripts.powershell",
                OutputPath = "applications/bts2010flatfilepipelinesample/bts2010flatfilepipelinesamplegroup",
            };
            appresource2.Parameters.Add("azure_primary_region", "UK South");
            appresource2.Parameters.Add("azure_secondary_region", "West US");
            appresource2.Parameters.Add("azure_subscription_id", "");
            appresource2.ResourceTemplateFiles.Add("application/applicationgroup/Deploy-10-ApplicationGroup.ps1.liquid");
            appresource2.ResourceTemplateFiles.Add("application/applicationgroup/New-ApplicationGroup.ps1.liquid");
            appresource2.ResourceTemplateFiles.Add("application/applicationgroup/TearDown-10-ApplicationGroup.ps1.liquid");
            appresource2.ResourceTemplateFiles.Add("application/applicationgroup/Remove-ApplicationGroup.ps1.liquid");
            application1.Resources.Add(appresource2);

            // Messages in the application
            var documentmessage1 = new DocumentMessage()
            {
                MessageType = MessageType.Document,
                ContentType = MessageContentType.Xml,
                MessageSchema = new MessageSchema()
                {
                    Name = "NewOrder",
                    ResourceKeyRef = "BTS2010FlatFilePipelineSample:ITEM~0:BTS2010FlatFilePipelineSample:NewOrder:schema:Order"
                },
                Name = "NewOrder",
                Key = "MessageBus:BTS2010FlatFilePipelineSample:NewOrder",
                Type = MessagingObjectType.Message,
                Description = "Azure Integration Application Schema",
                ResourceMapKey = "applicationMessageBTS2010FlatFilePipelineSampleNewOrder",
                Rating = ConversionRating.NoRating
            };
            application1.Messages.Add(documentmessage1);

            var messageresource1 = new TargetResourceTemplate()
            {
                TemplateKey = "deploySchemaBTS2010FlatFilePipelineSampleNewOrder",
                TemplateType = "microsoft.template.powershell",
                ResourceName = "NewOrder",
                ResourceType = "microsoft.scripts.powershell",
                OutputPath = "applications/BTS2010FlatFilePipelineSample/messages/neworder",
            };
            messageresource1.Parameters.Add("azure_primary_region", "UK South");
            messageresource1.Parameters.Add("azure_secondary_region", "West US");
            messageresource1.Parameters.Add("azure_subscription_id", "");
            messageresource1.Parameters.Add("azure_resource_group_name", "arrg-aimmsgbus-dev-uksouth");
            messageresource1.Parameters.Add("azure_integration_account_name", "arintacc-aimartifactstore-dev-uksouth");
            messageresource1.Parameters.Add("azure_message_schema_name", "BTS2010FlatFilePipelineSample.NewOrder");
            messageresource1.ResourceTemplateFiles.Add("messages/schemas/Deploy-100-Schema.ps1.liquid");
            messageresource1.ResourceTemplateFiles.Add("messages/schemas/New-Schema.ps1.liquid");
            messageresource1.ResourceTemplateFiles.Add("messages/schemas/Remove-Schema.ps1.liquid");
            messageresource1.ResourceTemplateFiles.Add("messages/schemas/TearDown-100-Schema.ps1.liquid");
            documentmessage1.Resources.Add(messageresource1);

            var messageresource2 = new TargetResourceTemplate()
            {
                TemplateKey = "messagesAzureIntegrationAccountSchemaBTS2010FlatFilePipelineSampleNewOrder",
                TemplateType = "microsoft.template.json",
                ResourceName = "NewOrder",
                ResourceType = "microsoft.schemas.xml",
                OutputPath = "applications/BTS2010FlatFilePipelineSample/messages/neworder",
            };
            messageresource2.Tags.Add("Application", "BTS2010FlatFilePipelineSample");
            messageresource2.Tags.Add("Env", "dev");
            messageresource2.Parameters.Add("azure_primary_region", "UK South");
            messageresource2.Parameters.Add("azure_secondary_region", "West US");
            messageresource2.Parameters.Add("azure_subscription_id", "");
            messageresource2.Parameters.Add("azure_resource_group_name", "arrg-aimmsgbus-dev-uksouth");
            messageresource2.Parameters.Add("azure_integration_account_name", "arintacc-aimartifactstore-dev-uksouth");
            messageresource2.Parameters.Add("azure_message_schema_name", "BTS2010FlatFilePipelineSample.NewOrder");
            messageresource2.ResourceTemplateFiles.Add("messages/schemas/requestbody.dev.json.liquid");
            documentmessage1.Resources.Add(messageresource2);

            // Endpoints
            var ftpadapter1 = new AdapterEndpoint()
            {
                Type = MessagingObjectType.Endpoint,
                AdapterProtocol = "FTP",
                MessageExchangePattern = MessageExchangePattern.Receive,
                Name = "FTP Receive",
                Description = "FTP Receive Adapter",
                EndpointType = EndpointType.Adapter,
                MessageDeliveryGuarantee = MessageDeliveryGuarantee.AtLeastOnce,
                Activator = true,
            };
            ftpadapter1.MessageKeyRefs.Add("MessageBus:BTS2010FlatFilePipelineSample:NewOrder");
            ftpadapter1.OutputChannelKeyRef = "MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:RcvFile.FTP-RoutingSlipRouter";
            ftpadapter1.Properties.Add("scenario", "Aim.BizTalk.FTPPassthru.RcvFile.RcvFile.FTP");
            ftpadapter1.Properties.Add("scenarioStep", "ftpReceiveAdapter");
            ftpadapter1.Properties.Add("configuration", new Dictionary<string, object>
            {
                { "btsReceivePortName", "RcvFile" }
            });
            ftpadapter1.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Error, Message = ModelResources.ExampleErrorMessage });
            ftpadapter1.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = ModelResources.ExampleWarningMessage });
            ftpadapter1.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Information, Message = ModelResources.ExampleInformationMessage });
            application1.Endpoints.Add(ftpadapter1);

            var adapterresource1 = new TargetResourceTemplate()
            {
                TemplateKey = "deploySchemaBTS2010FlatFilePipelineSampleNewOrder",
                TemplateType = "microsoft.template.powershell",
                ResourceName = "NewOrder",
                ResourceType = "microsoft.scripts.powershell",
                OutputPath = "applications/BTS2010FlatFilePipelineSample/messages/neworder",
            };
            adapterresource1.Parameters.Add("azure_primary_region", "UK South");
            adapterresource1.Parameters.Add("azure_secondary_region", "West US");
            adapterresource1.Parameters.Add("azure_subscription_id", "");
            adapterresource1.Parameters.Add("azure_resource_group_name", "arrg-aimmsgbus-dev-uksouth");
            adapterresource1.Parameters.Add("azure_integration_account_name", "arintacc-aimartifactstore-dev-uksouth");
            adapterresource1.Parameters.Add("azure_message_schema_name", "BTS2010FlatFilePipelineSample.NewOrder");
            adapterresource1.ResourceTemplateFiles.Add("messages/schemas/Deploy-100-Schema.ps1.liquid");
            adapterresource1.ResourceTemplateFiles.Add("messages/schemas/New-Schema.ps1.liquid");
            adapterresource1.ResourceTemplateFiles.Add("messages/schemas/Remove-Schema.ps1.liquid");
            adapterresource1.ResourceTemplateFiles.Add("messages/schemas/TearDown-100-Schema.ps1.liquid");
            ftpadapter1.Resources.Add(adapterresource1);

            var adapterresource2 = new TargetResourceTemplate()
            {
                TemplateKey = "messagesAzureIntegrationAccountSchemaBTS2010FlatFilePipelineSampleNewOrder",
                TemplateType = "microsoft.template.json",
                ResourceName = "NewOrder",
                ResourceType = "microsoft.schemas.xml",
                OutputPath = "applications/BTS2010FlatFilePipelineSample/messages/neworder",
            };
            adapterresource2.Tags.Add("Application", "BTS2010FlatFilePipelineSample");
            adapterresource2.Tags.Add("Env", "dev");
            adapterresource2.Parameters.Add("azure_primary_region", "UK South");
            adapterresource2.Parameters.Add("azure_secondary_region", "West US");
            adapterresource2.Parameters.Add("azure_subscription_id", "");
            adapterresource2.Parameters.Add("azure_resource_group_name", "arrg-aimmsgbus-dev-uksouth");
            adapterresource2.Parameters.Add("azure_integration_account_name", "arintacc-aimartifactstore-dev-uksouth");
            adapterresource2.Parameters.Add("azure_message_schema_name", "BTS2010FlatFilePipelineSample.NewOrder");
            adapterresource2.ResourceTemplateFiles.Add("messages/schemas/requestbody.dev.json.liquid");
            ftpadapter1.Resources.Add(adapterresource2);

            // Create the channel for the FTP adapter
            var channel1 = new TriggerChannel()
            {
                Type = MessagingObjectType.Channel,
                ChannelType = ChannelType.PointToPoint,
                Key = "MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:RcvFile.FTP-RoutingSlipRouter",
                Name = "Trigger Channel",
                Description = "The trigger channel sends a message to a URL endpoint which triggers the Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries.RoutingSlipRouter intermediary.",

            };
            application1.Channels.Add(channel1);

            var routingSlipRouter = new RoutingSlipRouter()
            {
                Type = MessagingObjectType.Intermediary,
                IntermediaryType = IntermediaryType.MessageRouter,
                MessageRouterType = MessageRouterType.ContentBasedRouter,
                Name = "Routing Slip Router",
                Key = "MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:RoutingSlipRouter:RcvFile.FTP-ContentPromoter",
                Description = "The routing slip router calls the next step in the route which is Content Promoter.",
            };
            routingSlipRouter.InputChannelKeyRefs.Add("MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:RcvFile.FTP-RoutingSlipRouter");
            routingSlipRouter.OutputChannelKeyRefs.Add("MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:RoutingSlipRouter-ContentPromoter");
            application1.Intermediaries.Add(routingSlipRouter);

            var channel2 = new TriggerChannel()
            {
                Type = MessagingObjectType.Channel,
                ChannelType = ChannelType.PointToPoint,
                Key = "MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:RoutingSlipRouter-ContentPromoter",
                Name = "Trigger Channel",
                Description = "The trigger channel sends a message to a URL endpoint which triggers the Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries.ContentPromoter intermediary.",
                TriggerUrl = "/routingManager/route/contentPromoter"

            };
            application1.Channels.Add(channel2);

            var contentPromoter = new ContentPromoter()
            {
                Type = MessagingObjectType.Intermediary,
                IntermediaryType = IntermediaryType.MessageProcessor,
                MessageProcessorType = MessageProcessorType.ContentPromoter,
            };
            contentPromoter.InputChannelKeyRefs.Add("MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:RoutingSlipRouter-ContentPromoter");
            contentPromoter.OutputChannelKeyRefs.Add("MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:ContentPromoter-RoutingSlipRouter");
            application1.Intermediaries.Add(contentPromoter);

            var channel3 = new TriggerChannel()
            {
                Type = MessagingObjectType.Channel,
                ChannelType = ChannelType.PointToPoint,
                Key = "MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:ContentPromoter-RoutingSlipRouter",
                Name = "Trigger Channel",
                Description = "The trigger channel sends a message to a URL endpoint which triggers the Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries.RoutingSlipRouter intermediary.",

            };
            application1.Channels.Add(channel3);

            var routingSlipRouter2 = new RoutingSlipRouter()
            {
                Type = MessagingObjectType.Intermediary,
                IntermediaryType = IntermediaryType.MessageRouter,
                MessageRouterType = MessageRouterType.ContentBasedRouter,
                Name = "Routing Slip Router",
                Key = "MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:ContentPromoter",
                Description = "The routing slip router calls the next step in the route which is Content Promoter.",
            };
            routingSlipRouter2.InputChannelKeyRefs.Add("MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:RoutingSlipRouter-ContentPromoter");
            routingSlipRouter2.OutputChannelKeyRefs.Add("MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:RoutingSlipRouter-ContentPromoter");
            application1.Intermediaries.Add(routingSlipRouter2);

            var channel4 = new TriggerChannel()
            {
                Type = MessagingObjectType.Channel,
                ChannelType = ChannelType.PointToPoint,
                Key = "MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:RoutingSlipRouter-ContentPromoter",
                Name = "Trigger Channel",
                Description = "The trigger channel sends a message to a URL endpoint which triggers the Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries.ContentPromoter intermediary.",
                TriggerUrl = "/routingManager/route/contentPromoter"
            };
            application1.Channels.Add(channel4);

            var contentPromoter2 = new ContentPromoter()
            {
                Type = MessagingObjectType.Intermediary,
                IntermediaryType = IntermediaryType.MessageProcessor,
                MessageProcessorType = MessageProcessorType.ContentPromoter,
                Name = "Content Promoter",
                Key = "MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:ContentPromoter",
            };
            contentPromoter2.InputChannelKeyRefs.Add("MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:RoutingSlipRouter-ContentPromoter");
            contentPromoter2.OutputChannelKeyRefs.Add("MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:RoutingSlipRouter-ContentPromoter");
            application1.Intermediaries.Add(contentPromoter2);

            var channel5 = new TriggerChannel()
            {
                Type = MessagingObjectType.Channel,
                ChannelType = ChannelType.PointToPoint,
                Key = "MessageBus:Aim.BizTalk.FTPPassthru:RcvFile:RcvFile.FTP:TriggerChannel:RoutingSlipRouter-ContentPromoter",
                Name = "Trigger Channel",
                Description = "The trigger channel sends a message to a URL endpoint which triggers the Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries.ContentPromoter intermediary.",
                TriggerUrl = "/routingManager/route/contentPromoter"
            };
            application1.Channels.Add(channel5);

            // System application
            var systemapp = new Application()
            {
                Name = "System Application",
                Key = "MessageBus:SystemApplication",
                Type = MessagingObjectType.Application,
                Description = "Azure Integration System Application",
                ResourceMapKey = "applicationSystemApplication",
                Rating = ConversionRating.NoRating,
            };
            model.MigrationTarget.MessageBus.Applications.Add(systemapp);

            // Channels in the system application
            var topicchannel1 = new TopicChannel()
            {
                ChannelType = ChannelType.PublishSubscribe,
                Name = "Message Box",
                Key = "MessageBus:SystemApplication:MessageBox",
                Type = MessagingObjectType.Channel,
                Description = "Azure Integration Message Box",
                ResourceMapKey = "messageBox",
                Rating = ConversionRating.NotSupported
            };
            systemapp.Channels.Add(topicchannel1);
        }
    }
}
