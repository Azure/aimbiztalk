// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.IO;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.DependencyRules;
using Microsoft.AzureIntegrationMigration.BizTalk.Cli.Tests.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Discover;
using Microsoft.AzureIntegrationMigration.BizTalk.Discover.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse;
using Microsoft.AzureIntegrationMigration.BizTalk.Report;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.ConversionRules;
using Microsoft.AzureIntegrationMigration.Runner.Engine;
using Microsoft.AzureIntegrationMigration.Runner.Configuration;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Cli.Tests
{
    class Program
    {
        static void Main()
        {
            using (var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace)))
            {
                var logger = loggerFactory.CreateLogger<Program>();

                var outputPath = @"../../../../../output/";
                outputPath = Path.GetFullPath(outputPath);

                var context = new MigrationContext() { WorkingFolder = outputPath };

                // Paths to the MSI test files
                var msiPaths = new string[]
                {
                @"../../../../../tests/BTS2010FlatFilePipelineSample.msi",
                @"../../../../../tests/BTS2020SimpleMessagingApplication.msi",
                @"../../../../../tests/BTS2020SimpleReferencedApplication.msi",
                @"../../../../../tests/BTS2020SimplePipelineDataApplication.msi",

                // Path to scenarios MSI
                @"../../../../../scenarios/001-FtpPassthru/msi/Aim.FtpPassthru.msi",
                @"../../../../../scenarios/002-XmlMapping/msi/XmlMapping.msi",
                @"../../../../../scenarios/003-SimpleOrchestration/msi/Aim.SimpleOrchestration.msi",
                @"../../../../../scenarios/004-HttpJsonOrch/msi/Aim.HttpJsonOrch.msi",
                };

                // New application model
                var model = new AzureIntegrationServicesModel();

                // New run state
                var state = new RunState(new RunnerConfiguration(), model);

                // Create the input criteria (paths to the MSIs)
                foreach (var path in msiPaths)
                {
                    var pathInfo = new FileInfo(path);
                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = pathInfo.Name, Name = pathInfo.Name, Type = ModelConstants.ResourceContainerMsi, ContainerLocation = pathInfo.FullName });
                }

                // Setup the discoverers.
                var discoverers = new IBizTalkDiscoverer[]
                {
                    new MsiDiscoverer(new FileRepository(), model, context, logger),
                    new AssemblyDiscoverer(model, context, logger)
                };

                logger.LogInformation(InformationMessages.DiscoveringStarted);

                // Run the discoverers.
                foreach (var discoverer in discoverers)
                {
                    discoverer.Discover();
                }

                logger.LogInformation(InformationMessages.DiscoveringComplete);

                // Setup the parsers
                var parsers = new IBizTalkParser[]
                {
                    // Parse the application definition file
                    new ApplicationDefinitionParser(model, context, logger),
                    // Parse the application name
                    new BizTalkApplicationParser(model, context, logger),
                    // Parse the module Refs
                    new BindingFileParser(model, context, logger),
                    // Parse the send port
                    new SendPortParser(model, context, logger),
                    // Parse the Orchestration Odx contents.
                    new BizTalkOrchestrationParser(model, context, logger),
                    // Parse the send port group filters
                    new DistributionListParser(model, context, logger),
                    // Parse the pipelines
                    new BizTalkPipelineParser(model, context, logger),
                    // Parse the property schemas
                    new PropertySchemaPropertyParser(model, context, logger),
                    // Parse the document schemas
                    new DocumentSchemaParser(model, context, logger),
                    // Parse the send pipeline data
                    new SendPortPipelineDataParser(model, context, logger),
                    // Parse the receive pipeline data
                    new ReceivePortPipelineDataParser(model, context, logger),
                    // Parse the receive port
                    new ReceivePortParser(model, context, logger),
                    // Parse the transforms
                    new TransformParser(model, context, logger),
                    // Parse the pipeline components
                    new PipelineComponentParser(model, context, logger),
                    // Parse the orchestration correlation types
                    new OrchestrationCorrelationTypeParser(model, context, logger),
                    // Parse the orchestration port types
                    new OrchestrationPortTypeParser(model, context, logger),
                    // Parse the orchestration multi part message types
                    new OrchestrationMultiPartMessageTypeParser(model, context, logger),
                    // Parse the orchestration service declarations
                    new OrchestrationServiceDeclarationParser(model, context, logger)
                };

                logger.LogInformation(InformationMessages.ParsingStarted);

                foreach (var parser in parsers)
                {
                    parser.Parse();
                }

                logger.LogInformation(InformationMessages.ParsingComplete);

                var analyzers = new IBizTalkAnalyzer[]
                {
                    new DP001SchemaDependencyAnalyzer(model, context, logger),
                    new DP002TransformDependencyAnalyzer(model, context, logger),
                    new DP003OrchestrationDependencyAnalyzer(model, context, logger),
                    new DP004ApplicationDependencyAnalyzer(model, context, logger),
                    new DP005DistributionListDependencyAnalyzer(model, context, logger),
                    new DP006ParentChildDependencyAnalyzer(model, context, logger),
                    new MB001MessageBusAnalyzer(model, context, logger),
                    new AP001ApplicationAnalyzer(model, context, logger),
                    new AP002SystemApplicationAnalyzer(model, context, logger),
                    new MB002MessageBoxAnalyzer(model, context, logger),
                    new SC001SchemaAnalyzer(model, context, logger),
                    new SC002PropertySchemaAnalyzer(model, context, logger)
                };
                
                logger.LogInformation(InformationMessages.AnalysisStarted);

                foreach (var analyzer in analyzers) 
                {
                    var task = analyzer.AnalyzeAsync(CancellationToken.None);
                    task.Wait();
                }

                logger.LogInformation(InformationMessages.AnalysisComplete);

                logger.LogInformation(InformationMessages.ReportingStarting);

                var reportWriter = new FileReportWriter();
                context.ReportFilePath = Path.Combine(context.WorkingFolder, "BizTalkMigrationReport.html");
                var reporters = new IBizTalkReporter[]
                {
                    new HtmlReportFormatter(model, context, state, reportWriter, logger)
                };

                foreach (var reporter in reporters)
                {
                    reporter.Report();
                }

                logger.LogInformation(InformationMessages.ReportingComplete);

                // Output the results
                var serializer = new JsonSerializer();
                using var writer = new StringWriter();
                serializer.Serialize(writer, model);
                File.WriteAllText(Path.Combine(context.WorkingFolder, "model.json"), writer.ToString());
            }
        }
    }
}
