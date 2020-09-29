// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Microsoft.AzureIntegrationMigration.BizTalk.Report.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.AzureIntegrationMigration.Runner.Engine;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report.SummaryReport
{
    /// <summary>
    /// Class for rendering the report summary section.
    /// </summary>
    public static class SummaryReportRenderer
    {
        /// <summary>
        /// Renders the summary report.
        /// </summary>
        /// <param name="files">The files to generate.</param>
        /// <param name="state">The run state.</param>
        public static void RenderSummaryReport(ReportFileStructure files, IRunState state)
        {
            _ = files ?? throw new ArgumentNullException(nameof(files));
            _ = state ?? throw new ArgumentNullException(nameof(state));

            // Get the container from the file.
            var container = files.Summary.ReportHtml.DocumentNode.SelectSingleNode(HtmlResources.ReportContentJQuery);

            // Create a section for the run summary
            var runInfoNode = HtmlNode.CreateNode(HtmlResources.SummaryRunInfoHeading);
            container.AppendChild(runInfoNode);
            var runInfoContainer = runInfoNode.SelectSingleNode(HtmlResources.SummaryRunInfoJQuery);

            // Render the timings.
            RenderTimings(runInfoContainer, (RunState)state);

            // Render the arguments.
            RenderArguments(runInfoContainer, (RunState)state);

            // Render the stage runners
            RenderStageRunners(runInfoContainer, (RunState)state);

            // Add a placeholder for when there are no source applications. 
            if (files.SourceApplications.Count == 0)
            {
                var node = HtmlNode.CreateNode(HtmlResources.SourceApplicationNoneFoundSnippet);
                container.AppendChild(node);
            }
            else
            {
                // Build a heading and a list of the file in the source and target reports.
                var headingNode = HtmlNode.CreateNode(HtmlResources.SummarySourceApplicationHeading);
                container.AppendChild(headingNode);
                var sourceContainer = headingNode.SelectSingleNode(HtmlResources.SummarySourceApplicationJQuery);

                foreach (var source in files.SourceApplications)
                {
                    var linkNode = HtmlNode.CreateNode(HtmlResources.SnippetEmptyDiv);
                    linkNode.InnerHtml = string.Format(CultureInfo.CurrentCulture, HtmlResources.SummaryReportFileLink, source.Filename, source.SectionName, source.ReportData.Description);
                    sourceContainer.AppendChild(linkNode);
                }
            }

            // Add a placeholder for when there are no target applications.
            if (files.TargetApplications.Count > 0 && files.TargetMessageBus != null)
            {
                // Build a heading and a list of the file in the source and target reports.
                var headingNode = HtmlNode.CreateNode(HtmlResources.SummaryTargetMessageBusHeading);
                container.AppendChild(headingNode);
                var sourceContainer = headingNode.SelectSingleNode(HtmlResources.SummaryTargetMessageBusJQuery);

                var linkNode = HtmlNode.CreateNode(HtmlResources.SnippetEmptyDiv);
                linkNode.InnerHtml = string.Format(CultureInfo.CurrentCulture, HtmlResources.SummaryReportFileLink, files.TargetMessageBus.Filename, files.TargetMessageBus.SectionName, files.TargetMessageBus.ReportData.Description);
                sourceContainer.AppendChild(linkNode);
            }

            // Add a placeholder for when there are no target applications.
            if (files.TargetApplications.Count == 0)
            {
                var node = HtmlNode.CreateNode(HtmlResources.TargetApplicationNoneFoundSnippet);
                container.AppendChild(node);
            }
            else
            {
                // Build a heading and a list of the file in the source and target reports.
                var headingNode = HtmlNode.CreateNode(HtmlResources.SummaryTargetApplicationHeading);
                container.AppendChild(headingNode);
                var sourceContainer = headingNode.SelectSingleNode(HtmlResources.SummaryTargetApplicationJQuery);

                foreach (var target in files.TargetApplications)
                {
                    var linkNode = HtmlNode.CreateNode(HtmlResources.SnippetEmptyDiv);
                    linkNode.InnerHtml = string.Format(CultureInfo.CurrentCulture, HtmlResources.SummaryReportFileLink, target.Filename, target.SectionName, target.ReportData.Application.Description);
                    sourceContainer.AppendChild(linkNode);
                }
            }
        }

        /// <summary>
        /// Renders the arguments passed to the tool.
        /// </summary>
        /// <param name="container">The HTML container.</param>
        /// <param name="state">The run state.</param>
        private static void RenderArguments(HtmlNode container, RunState state)
        {
            // Create the section for the arguments
            var snippet = HtmlResources.SummaryApplicationResourceSnippetExpand;
            var refId = Guid.NewGuid().ToString();
            snippet = snippet.Replace("{refId}", refId);
            snippet = snippet.Replace("{icon}", HtmlResources.SummaryReportIconArguments);
            snippet = snippet.Replace("{name}", HtmlResources.SummaryReportNameArguments);
            snippet = snippet.Replace("{description}", string.Format(CultureInfo.CurrentCulture, HtmlResources.SummaryReportArgumentCount, state.Configuration.Args.Count));

            var node = HtmlNode.CreateNode(HtmlResources.SnippetEmptyDiv);
            node.InnerHtml = snippet;
            container.AppendChild(node);

            // Build the argument list.
            var childNode = node.SelectSingleNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.ApplicationResourceChildJQuery, refId));
            RenderArgumentDictionary(childNode, state.Configuration.Args, string.Empty);
        }

        /// <summary>
        /// Renders a property dictionary.
        /// </summary>
        /// <param name="containerNode">The HTML container node.</param>
        /// <param name="dictionary">The dictionary to render.</param>
        /// <param name="indent">Indentation for child properties.</param>
        private static void RenderArgumentDictionary(HtmlNode containerNode, IDictionary<string, object> dictionary, string indent)
        {
            foreach (var property in dictionary)
            {
                if (property.Value == null)
                {
                    containerNode.AppendChild(
                        CreateNodeWithSnippetContent(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                HtmlResources.TargetAnalysisSnippetProperty,
                                indent,
                                property.Key,
                                HtmlResources.AnalysisMessageNull)));
                }
                else if(property.Value is string)
                {
                    containerNode.AppendChild(
                        CreateNodeWithSnippetContent(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                HtmlResources.TargetAnalysisSnippetProperty,
                                indent,
                                property.Key,
                                property.Value)));
                }
                else if (property.Value.GetType().IsArray)
                {
                    containerNode.AppendChild(
                        CreateNodeWithSnippetContent(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                HtmlResources.TargetAnalysisSnippetProperty,
                                indent,
                                property.Key,
                                string.Join(",", (string[])property.Value))));
                }
                else
                {
                    containerNode.AppendChild(
                        CreateNodeWithSnippetContent(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                HtmlResources.TargetAnalysisSnippetProperty,
                                indent,
                                property.Key,
                                property.Value.ToString())));
                }
            }
        }

        /// <summary>
        /// Renders the timings generated by the tool.
        /// </summary>
        /// <param name="container">The HTML container.</param>
        /// <param name="state">The run state.</param>
        private static void RenderTimings(HtmlNode container, RunState state)
        {
            // Get the start and end dates for the run
            var startDate = DateTimeOffset.Now;
            var discover = Stages.Discover.ToString("G");
            if (state.ExecutionState.ContainsKey(discover)) startDate = state.ExecutionState[discover].Started;

            // Create the section for the arguments
            var snippet = HtmlResources.SummaryApplicationResourceSnippetNoExpand;
            var refId = Guid.NewGuid().ToString();
            snippet = snippet.Replace("{refId}", refId);
            snippet = snippet.Replace("{icon}", HtmlResources.SummaryReportIconTimings);
            snippet = snippet.Replace("{name}", HtmlResources.SummaryReportNameTimings);
            snippet = snippet.Replace("{description}", string.Format(CultureInfo.CurrentCulture, HtmlResources.SummaryReportRunStartAndEnd, startDate));

            var node = HtmlNode.CreateNode(HtmlResources.SnippetEmptyDiv);
            node.InnerHtml = snippet;
            container.AppendChild(node);
        }

        /// <summary>
        /// Renders information on the report's stage runners.
        /// </summary>
        /// <param name="container">The HTML container.</param>
        /// <param name="state">The run state.</param>
        private static void RenderStageRunners(HtmlNode container, RunState state)
        {
            // Create the section for the stage runners
            var snippet = HtmlResources.SummaryApplicationResourceSnippetExpand;
            var refId = Guid.NewGuid().ToString();
            snippet = snippet.Replace("{refId}", refId);
            snippet = snippet.Replace("{icon}", HtmlResources.SummaryReportIconStageRunners);
            snippet = snippet.Replace("{name}", HtmlResources.SummaryReportNameStageRunners);
            snippet = snippet.Replace("{description}", string.Format(CultureInfo.CurrentCulture, HtmlResources.SummaryReportStageRunnerCount, state.Configuration.StageRunners.Count));

            var node = HtmlNode.CreateNode(HtmlResources.SnippetEmptyDiv);
            node.InnerHtml = snippet;
            container.AppendChild(node);

            // Build the argument list.
            var childNode = node.SelectSingleNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.ApplicationResourceChildJQuery, refId));
            foreach (var runner in state.Configuration.StageRunners)
            {
                childNode.AppendChild(
                        CreateNodeWithSnippetContent(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                HtmlResources.SummaryReportStageRunnerSnippet,
                                runner.Name,
                                runner.Priority,
                                runner.Skip,
                                runner.Stages.ToString())));
            }
        }

        /// <summary>
        /// Renders the stage runners.
        /// </summary>
        /// <param name="containerNode">The HTML container node.</param>
        /// <param name="stageRunners">The stage runners.</param>
        /// <param name="indent">The indent.</param>
        private static void RenderStageRunners(HtmlNode containerNode, IList<IStageRunner> stageRunners, string indent)
        {
            foreach (var runner in stageRunners)
            {
                containerNode.AppendChild(
                        CreateNodeWithSnippetContent(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                HtmlResources.SummaryReportStageRunnerSnippet,
                                indent,
                                runner.Name,
                                runner.Priority,
                                runner.Skip,
                                runner.Stages.ToString())));
            }
        }

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
    }
}
