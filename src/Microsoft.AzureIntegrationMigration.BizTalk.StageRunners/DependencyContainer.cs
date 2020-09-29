// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Generator;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Renderer;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.StageRunners
{
    /// <summary>
    /// Defines a class that builds an IoC container with services for the app.
    /// </summary>
    public static class DependencyContainer
    {
        /// <summary>
        /// Build the app container with all services.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="logger">The provided logger by the tool.</param>
        /// <param name="state">The run state provided by the tool.</param>
        /// <returns>A service provider.</returns>
        public static IServiceProvider BuildContainer(IApplicationModel model, ILogger logger, IRunState state)
        {
            _ = model ?? throw new ArgumentNullException(nameof(model));
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = state ?? throw new ArgumentNullException(nameof(state));

            // Create container
            var services = new ServiceCollection();

            // Add application model
            services.AddSingleton<IApplicationModel>(model);

            // Add logger
            services.AddSingleton<ILogger>(logger);

            // Add run state
            services.AddSingleton<IRunState>(state);

            // Add generators
            services.AddTransient<IResourceGenerator, YamlResourceGenerator>();

            // Add renderers
            services.AddSingleton<ITemplateRenderer, LiquidTemplateRenderer>();
            services.AddSingleton<ISnippetRenderer, LiquidSnippetRenderer>();

            // Add repositories
            services.AddTransient<BizTalk.Discover.Repositories.IFileRepository, BizTalk.Discover.Repositories.FileRepository>();
            services.AddTransient<BizTalk.Convert.Repositories.IFileRepository, BizTalk.Convert.Repositories.FileRepository>();
            services.AddTransient<IConfigurationRepository, FileConfigurationRepository>();
            services.AddTransient<ITemplateRepository, FileTemplateRepository>();

            // Add migration context
            services.AddSingleton<MigrationContext>();

            // Add discover components
            services.AddTransient<BizTalk.Discover.MsiDiscoverer>();
            services.AddTransient<BizTalk.Discover.AssemblyDiscoverer>();

            // Add parse components
            services.AddTransient<BizTalk.Parse.ApplicationDefinitionParser>();
            services.AddTransient<BizTalk.Parse.BizTalkApplicationParser>();
            services.AddTransient<BizTalk.Parse.BindingFileParser>();
            services.AddTransient<BizTalk.Parse.BizTalkOrchestrationParser>();
            services.AddTransient<BizTalk.Parse.BizTalkPipelineParser>();
            services.AddTransient<BizTalk.Parse.DistributionListParser>();
            services.AddTransient<BizTalk.Parse.DocumentSchemaParser>();
            services.AddTransient<BizTalk.Parse.PipelineComponentParser>();
            services.AddTransient<BizTalk.Parse.PropertySchemaPropertyParser>();
            services.AddTransient<BizTalk.Parse.OrchestrationCorrelationTypeParser>();
            services.AddTransient<BizTalk.Parse.OrchestrationMultiPartMessageTypeParser>();
            services.AddTransient<BizTalk.Parse.OrchestrationPortTypeParser>();
            services.AddTransient<BizTalk.Parse.OrchestrationServiceLinkTypeParser>();
            services.AddTransient<BizTalk.Parse.OrchestrationServiceDeclarationParser>();
            services.AddTransient<BizTalk.Parse.ReceivePortParser>();
            services.AddTransient<BizTalk.Parse.ReceivePortPipelineDataParser>();
            services.AddTransient<BizTalk.Parse.SendPortParser>();            
            services.AddTransient<BizTalk.Parse.SendPortPipelineDataParser>();
            services.AddTransient<BizTalk.Parse.TransformParser>();

            // Add analyzer components
            services.AddTransient<BizTalk.Analyze.ResourceGeneratorAnalyzer>();
            services.AddTransient<BizTalk.Analyze.DependencyRules.DP001SchemaDependencyAnalyzer>();
            services.AddTransient<BizTalk.Analyze.DependencyRules.DP002TransformDependencyAnalyzer>();
            services.AddTransient<BizTalk.Analyze.DependencyRules.DP003OrchestrationDependencyAnalyzer>();
            services.AddTransient<BizTalk.Analyze.DependencyRules.DP004ApplicationDependencyAnalyzer>();
            services.AddTransient<BizTalk.Analyze.DependencyRules.DP005DistributionListDependencyAnalyzer>();
            services.AddTransient<BizTalk.Analyze.DependencyRules.DP006ParentChildDependencyAnalyzer>();            
            services.AddTransient<BizTalk.Analyze.ConversionRules.MB001MessageBusAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.AP001ApplicationAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.AP002SystemApplicationAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.AP003ReceivePortScenarioAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.AP004SendPortScenarioAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.AP005OrchestrationScenarioAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.MB002MessageBoxAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.SC001SchemaAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.SC002PropertySchemaAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.RP001FtpReceivePortAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.RP002FileReceivePortAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.RP003HttpReceivePortAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.SP001FtpSendPortAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.SP002FileSendPortAnalyzer>();
            services.AddTransient<BizTalk.Analyze.ConversionRules.MA001TransformAnalyzer>();

            // Add report components
            services.AddTransient<BizTalk.Report.IReportWriter, BizTalk.Report.FileReportWriter>();
            services.AddTransient<BizTalk.Report.HtmlReportFormatter>();

            // Add convert components
            services.AddSingleton<BizTalk.Convert.IScenarioRouteWalker, BizTalk.Convert.ScenarioRouteWalker>();
            services.AddTransient<BizTalk.Convert.TemplateRendererConverter>();
            services.AddTransient<BizTalk.Convert.GeneratorRules.AP001ReceiveRoutingSlipGenerator>();
            services.AddTransient<BizTalk.Convert.GeneratorRules.AP002SendRoutingSlipGenerator>();
            services.AddTransient<BizTalk.Convert.GeneratorRules.AP003ReceiveConfigurationEntryGenerator>();
            services.AddTransient<BizTalk.Convert.GeneratorRules.AP004SendConfigurationEntryGenerator>();
            services.AddTransient<BizTalk.Convert.GeneratorRules.AP005SendRoutingPropertyGenerator>();
            services.AddTransient<BizTalk.Convert.GeneratorRules.AP006ReceiveRoutingPropertyGenerator>();
            services.AddTransient<BizTalk.Convert.GeneratorRules.AP007ProcessManagerRoutingSlipGenerator>();
            services.AddTransient<BizTalk.Convert.GeneratorRules.AP008ProcessManagerConfigurationEntryGenerator>();
            services.AddTransient<BizTalk.Convert.GeneratorRules.SC001DocumentSchemaGenerator>();
            services.AddTransient<BizTalk.Convert.GeneratorRules.SC002PropertySchemaGenerator>();
            services.AddTransient<BizTalk.Convert.GeneratorRules.MA001TransformGenerator>();
            services.AddTransient<BizTalk.Convert.GeneratorRules.WF001WorkflowGenerator>();

            // Build provider
            var provider = services.BuildServiceProvider();
            return provider;
        }
    }
}
