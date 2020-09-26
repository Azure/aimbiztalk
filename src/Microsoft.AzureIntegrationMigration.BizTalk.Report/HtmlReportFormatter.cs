using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.BizTalk.Report.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Report.SummaryReport;
using Microsoft.AzureIntegrationMigration.BizTalk.Report.TargetReport;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report
{
    /// <summary>
    /// Creates a BizTalk migration report in HTML.
    /// </summary>
    public sealed class HtmlReportFormatter : BizTalkReporterBase
    {
        /// <summary>
        /// HTML utility.
        /// </summary>
        private readonly HtmlDocument _report = new HtmlDocument();

        /// <summary>
        /// Tracks the files that will be created.
        /// </summary>
        private readonly ReportFileStructure _files = new ReportFileStructure();

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlReportFormatter"/> class.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="state">The run state.</param>
        /// <param name="writer">The report writer.</param>
        /// <param name="logger">The logger.</param>
        public HtmlReportFormatter(IApplicationModel model, MigrationContext context, IRunState state, IReportWriter writer, ILogger logger)
            : base(model, context, state, writer, logger)
        {
            logger.LogTrace(TraceMessages.ReporterConstruct, nameof(HtmlReportFormatter));
        }

        #region Bootstrapping the Report Generation

        /// <summary>
        /// Executes the formatting of the report.
        /// </summary>
        protected override void ReportInternal()
        {
            Logger.LogDebug(TraceMessages.LoadingHtmlTemplate);

            // Load the HTML from resource file
            _report.LoadHtml(HtmlResources.HtmlPageTemplate);

            // Build the file structure.
            BuildFileStructure();

            // Build menu information
            BuildApplicationMenuItems();
            UpdateHomePageLink();
            InitializeHtmlPages();

            // Build summary information
            BuildSummarySection();

            // Build source application information
            BuildSourceApplications();

            // Build the message bus app
            if (Model.MigrationTarget.MessageBus != null) BuildTargetMessageBusApplication();

            // Build the target application information
            if (Model.MigrationTarget.MessageBus != null) BuildTargetApplications();

            // Write to file
            WriteToFile();
        }

        /// <summary>
        /// Builds the list of files that will be created for the report and allocates data to them.
        /// </summary>
        private void BuildFileStructure()
        {
            _files.Summary = new ReportFile<ReportSummary>(HtmlResources.FileListSummary, null, Model.Report.Summary, string.Empty)
            {
                Filename = Path.GetFileName(Context.ReportFilePath)
            };

            foreach (var container in Model.MigrationSource.ResourceContainers)
            {
                var application = container.FindResourcesByType(ModelConstants.ResourceApplication).FirstOrDefault();

                // Defensive check.
                if (application == null)
                {
                    Logger.LogWarning(WarningMessages.NoValidResourcesFound, container.Name);
                    continue;
                }
                _files.SourceApplications.Add(new ReportFile<ResourceContainer>(application.Name, application.Description, container, HtmlResources.FileListPrefixSource));
            }

            _files.TargetMessageBus = new ReportFile<MessageBus>(HtmlResources.FileListMessageBus, HtmlResources.FileListMessageBusDescription, Model.MigrationTarget.MessageBus, "MessageBus");

            var targetApplications = TargetScenarioModeller.DecodeTargetModel(Model.MigrationTarget);

            foreach (var application in targetApplications)
            {
                _files.TargetApplications.Add(new ReportFile<TargetApplication>(application.Application.Name, application.Application.Description, application, HtmlResources.FileListPrefixTarget));
            }
        }

        /// <summary>
        /// This builds the application menu items.
        /// </summary>
        private void BuildApplicationMenuItems()
        {
            Logger.LogDebug(TraceMessages.ApplicationMenuBuildingList);

            // Get a reference to the parent element for the source menu items.
            var container = _report.DocumentNode.SelectSingleNode(HtmlResources.SourceApplicationMenuJQuery);
            container.RemoveAllChildren();

            // Add links to the source files
            foreach (var application in _files.SourceApplications)
            {
                Logger.LogDebug(TraceMessages.ApplicationMenuBuildingItem, application.ReportData.Name);

                var node = HtmlNode.CreateNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.SnippetDropdownMenuItem, application.Filename, application.ReportData.Name));
                container.AppendChild(node);
            }

            // Get reference to the container for the target app
            container = _report.DocumentNode.SelectSingleNode(HtmlResources.TargetApplicationMenuJQuery);
            container.RemoveAllChildren();

            var mbnode = HtmlNode.CreateNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.SnippetDropdownMenuItem, _files.TargetMessageBus.Filename, _files.TargetMessageBus.ReportData.Name));
            container.AppendChild(mbnode);

            // Add links to target app
            foreach (var application in _files.TargetApplications)
            {
                Logger.LogDebug(TraceMessages.ApplicationMenuBuildingItem, application.ReportData.Application.Name);

                var node = HtmlNode.CreateNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.SnippetDropdownMenuItem, application.Filename, application.ReportData.Application.Name));
                container.AppendChild(node);
            }
        }

        /// <summary>
        /// Updates the home page link in the template HTML page to match the parameter.
        /// </summary>
        private void UpdateHomePageLink()
        {
            var filename = Path.GetFileName(_files.Summary.Filename);
            var node = _report.DocumentNode.SelectSingleNode(HtmlResources.HomePageLinkJQuery);
            node.Attributes["href"].Value = filename;
        }

        /// <summary>
        /// This loads the blank report page into the pages.
        /// </summary>
        private void InitializeHtmlPages()
        {
            var html = _report.DocumentNode.OuterHtml;
            _files.Summary.ReportHtml.LoadHtml(html);
            foreach(var source in _files.SourceApplications)
            {
                source.ReportHtml.LoadHtml(html);
            }

            foreach(var target in _files.TargetApplications)
            {
                target.ReportHtml.LoadHtml(html);
            }

            _files.TargetMessageBus.ReportHtml.LoadHtml(html);
        }

        #endregion

        #region Build Summary Section

        /// <summary>
        /// Builds the summary (first page) of the report.
        /// </summary>
        private void BuildSummarySection()
        {
            SummaryReportRenderer.RenderSummaryReport(_files, State);
        }

        #endregion

        #region Source Application Section

        /// <summary>
        /// Builds the source applications.
        /// </summary>
        private void BuildSourceApplications()
        {
            foreach (var source in _files.SourceApplications)
            {
                // Get a reference to the parent element for the menu items.
                var containerNode = source.ReportHtml.DocumentNode.SelectSingleNode(HtmlResources.ReportContentJQuery);
                var application = source.ReportData.FindResourcesByType(ModelConstants.ResourceApplication).FirstOrDefault();

                Logger.LogDebug(TraceMessages.ApplicationSectionAddingItem, application.Name);

                var snippet = HtmlResources.SourceApplicationSectionSnippet;
                snippet = snippet.Replace("{refId}", application.RefId);
                snippet = snippet.Replace("{name}", application.Name);
                snippet = snippet.Replace("{description}", application.Description);
                var node = CreateNodeWithSnippetContent(snippet);
                containerNode.AppendChild(node);

                // Recursively build the resources by type at the next level down.
                var childNode = node.SelectSingleNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.ApplicationSectionChildJQuery, application.RefId));

                // Get a list of the resource types discovered
                var resourceTypes = from sourceResource in source.ReportData.FindAllResources()
                                    select new { sourceResource.Type, FriendlyName = ResourceFormatter.GetResourceFriendlyName(sourceResource.Type) };

                foreach (var resourceType in resourceTypes.OrderBy(r => r.FriendlyName).Distinct())
                {
                    BuildSourceApplicationResources(childNode, source.ReportData.FindResourcesByType(resourceType.Type));
                }

                // Create navigation for the resources.
                BuildSourceApplicationResourceSelector(source);
            }
        }

        /// <summary>
        /// This builds a list of resource entries within the containing node.
        /// </summary>
        /// <param name="containerNode">The containing HTML node.</param>
        /// <param name="resourceItems">The resources.</param>
        private void BuildSourceApplicationResources(HtmlNode containerNode, IList<ResourceItem> resourceItems)
        {
            if (resourceItems.Count == 0) return;

            // Sort by name.
            var resources = resourceItems.ToList();
            resources.Sort(Comparers.SortResourcesByName);

            // Create a header entry
            var sectionNode = HtmlNode.CreateNode(HtmlResources.SnippetEmptyDiv);
            sectionNode.InnerHtml = string.Format(CultureInfo.CurrentCulture, HtmlResources.ApplicationSubsectionHeading, resourceItems.First().Type, ResourceFormatter.GetResourceFriendlyName(resourceItems.First().Type));
            containerNode.AppendChild(sectionNode);

            // Iterate through the resource definitions
            foreach (var resource in resources)
            {
                Logger.LogDebug(TraceMessages.ResourceAddingItem, resource.Name, resource.Type);

                var snippet = HtmlResources.SourceApplicationResourceSnippet;
                snippet = snippet.Replace("{icon}", ResourceFormatter.GetResourceIconFromType(resource.Type));
                snippet = snippet.Replace("{refId}", resource.RefId);
                snippet = snippet.Replace("{name}", resource.Name);
                snippet = snippet.Replace("{description}", string.IsNullOrEmpty(resource.Description) ? HtmlResources.ResourceDefaultDescription : resource.Description);
                var node = CreateNodeWithSnippetContent(snippet);
                containerNode.AppendChild(node);

                // Build the analysis content for this resource.
                var childNode = node.SelectSingleNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.ApplicationResourceChildJQuery, resource.RefId));

                // Create the properties.
                childNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetHeading, HtmlResources.AnalysisHeadingProperties)));
                foreach (var property in resource.Properties)
                {
                    childNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetProperty, property.Key, property.Value)));
                }
                if (resource.Properties.Count == 0)
                {
                    // Add a placeholder if there are no relationships.
                    childNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetInformationMessage, HtmlResources.AnalysisMessageNoProperties)));
                }

                // Create the references.
                childNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetHeading, HtmlResources.AnalysisHeadingReferences)));

                var rels = new List<Tuple<ResourceRelationship, ResourceItem>>(); // contains the resources for sorting.
                resource.ResourceRelationships.ToList().ForEach(r => rels.Add(new Tuple<ResourceRelationship, ResourceItem>(r, Model.FindResourceByRefId(r.ResourceRefId)))); // Adds the relationship plus the associated resource
                rels.Sort(Comparers.SortResourceRelationships);  // Sort using a comparison delegate

                foreach (var reference in rels)
                {
                    var parentApp = _files.SourceApplications.Where(s => s.ReportData.FindAllResources().Contains(reference.Item2)).FirstOrDefault();
                    var filename = parentApp == null ? string.Empty : parentApp.Filename;
                    childNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetLink, filename, reference.Item1.ResourceRefId, ResourceFormatter.GetResourceFriendlyName(reference.Item2.Type), reference.Item1.ResourceRelationshipType, reference.Item2.Name)));
                }
                if (resource.ResourceRelationships.Count == 0)
                {
                    // Add a placeholder if there are no relationships.
                    childNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetInformationMessage, HtmlResources.AnalysisMessageNoReferenceLinks)));
                }

                // Create the analysis messages:
                childNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetHeading, HtmlResources.AnalysisHeadingMessages)));
                foreach (var message in resource.ReportMessages)
                {
                    if (message.Severity == MessageSeverity.Error)
                    {
                        childNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetErrorMessage, message.Message)));
                    }
                    else if (message.Severity == MessageSeverity.Warning)
                    {
                        childNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetWarningMessage, message.Message)));
                    }
                    else
                    {
                        childNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetInformationMessage, message.Message)));
                    }
                }
                if (resource.ReportMessages.Count == 0)
                {
                    // Add a placeholder if there are no relationships.
                    childNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetInformationMessage, HtmlResources.AnalysisMessageNoMessages)));
                }
            }
        }

        /// <summary>
        /// This builds a new dropdown in the navbar so you can select resources.
        /// </summary>
        private void BuildSourceApplicationResourceSelector(ReportFile<ResourceContainer> source)
        {

            // Get a reference to the navbar <ul>.
            var container = source.ReportHtml.DocumentNode.SelectSingleNode(HtmlResources.NavbarListJQuery);

            // Create a new <li> within the navbar.
            var dropdownNode = HtmlNode.CreateNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.SnippetNavbarDropdown, HtmlResources.NavbarResourcesHeading, HtmlResources.NavbarResourcesId));
            container.AppendChild(dropdownNode);

            // Select the dropdown list as the current node.
            var dropdown = dropdownNode.SelectSingleNode(HtmlResources.NavbarResourcesDropdownJQuery);
            dropdown.RemoveAllChildren();


            // Get a list of the resource types discovered
            var resourceTypes = from sourceResource in source.ReportData.FindAllResources()
                                select new { sourceResource.Type, FriendlyName = ResourceFormatter.GetResourceFriendlyName(sourceResource.Type) };

            foreach (var resourceType in resourceTypes.OrderBy(r => r.FriendlyName).Distinct())
            {
                Logger.LogDebug(TraceMessages.ApplicationMenuBuildingItem, resourceType);

                var node = HtmlNode.CreateNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.SnippetDropdownMenuItem, string.Concat("#", resourceType.Type), resourceType.FriendlyName));
                dropdown.AppendChild(node);
            }
        }

        #endregion

        #region Target Message Bus Application

        /// <summary>
        /// Iterates through each application and builds the application resources within.
        /// </summary>
        private void BuildTargetMessageBusApplication()
        {
            Logger.LogDebug(TraceMessages.ApplicationSectionAddingItem, HtmlResources.FileListMessageBus);

            // Get a reference to the parent element for the menu items.
            var container = _files.TargetMessageBus.ReportHtml.DocumentNode.SelectSingleNode(HtmlResources.ReportContentJQuery);
            var messageBus = _files.TargetMessageBus.ReportData;
            TargetReportRenderer.RenderMessageBusApplication(container, messageBus);
        }

        #endregion

        #region Target Application Report

        /// <summary>
        /// Builds the target applications.
        /// </summary>
        private void BuildTargetApplications()
        {
            foreach (var target in _files.TargetApplications)
            {
                // Get a reference to the parent element for the menu items.
                var containerNode = target.ReportHtml.DocumentNode.SelectSingleNode(HtmlResources.ReportContentJQuery);
                var application = target.ReportData;

                Logger.LogDebug(TraceMessages.ApplicationSectionAddingItem, application.Application.Name);

                var applicationContainerNode = TargetReportRenderer.RenderTargetApplicationHeading(containerNode, application);
                TargetReportRenderer.RenderApplicationSummary(applicationContainerNode, application);
                application.Scenarios.ToList().ForEach(a => TargetReportRenderer.RenderTargetScenario(applicationContainerNode, a));
                if (application.Scenarios.Count == 0) TargetReportRenderer.RenderNoScenario(applicationContainerNode);

                // Create a navbar menu for the scenario
                if (target.ReportData.Scenarios.Count > 0)
                {
                    TargetReportRenderer.BuildScenarioSelector(target);
                }
            }
        }

        #endregion

        #region Utility Report Building

        /// <summary>
        /// Creates an HTML node with the snippet inserted as inner HTML.
        /// </summary>
        /// <param name="snippet">The HTML snippet.</param>
        /// <returns>A node with the HTML inserted.</returns>
        private static HtmlNode CreateNodeWithSnippetContent(string snippet)
        {
            var node = HtmlNode.CreateNode(HtmlResources.SnippetEmptyDiv);
            node.InnerHtml = snippet;
            return node;
        }

        #endregion

        #region Write to File

        /// <summary>
        /// Writes the content of the report HTML to the writer.
        /// </summary>
        private void WriteToFile()
        {
            Logger.LogDebug(TraceMessages.WritingReportToFile, Context.ReportFilePath);

            // The Text and ParsedText properties of the HtmlDocument object for some reason are not
            // updated with the DOM changes.  Need to access DocumentNode.  Probably a bug!
            Writer.WriteText(Context.ReportFilePath, _files.Summary.ReportHtml.DocumentNode.OuterHtml);

            foreach (var section in _files.SourceApplications)
            {
                Writer.WriteText(Path.Combine(Path.GetDirectoryName(Context.ReportFilePath), section.Filename), section.ReportHtml.DocumentNode.OuterHtml);
            }

            Writer.WriteText(Path.Combine(Path.GetDirectoryName(Context.ReportFilePath), _files.TargetMessageBus.Filename), _files.TargetMessageBus.ReportHtml.DocumentNode.OuterHtml);

            foreach (var section in _files.TargetApplications)
            {
                Writer.WriteText(Path.Combine(Path.GetDirectoryName(Context.ReportFilePath), section.Filename), section.ReportHtml.DocumentNode.OuterHtml);
            }
        }

        #endregion
    }
}
