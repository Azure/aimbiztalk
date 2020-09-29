// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Channels;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Endpoints;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
using Microsoft.AzureIntegrationMigration.BizTalk.Report.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report.TargetReport
{
    /// <summary>
    /// Responsible for rendering target report elements.
    /// </summary>
    public static class TargetReportRenderer
    {
        /// <summary>
        /// Renders the heading for the target application.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        /// <param name="application">The target application</param>
        public static HtmlNode RenderTargetApplicationHeading(HtmlNode containerNode, TargetApplication application)
        {
            _ = containerNode ?? throw new ArgumentNullException(nameof(containerNode));
            _ = application ?? throw new ArgumentNullException(nameof(application));

            var snippet = HtmlResources.TargetApplicationSnippetNewSection;
            var refId = Guid.NewGuid().ToString();
            snippet = snippet.Replace("{refId}", refId);
            snippet = snippet.Replace("{name}", application.Application.Name);
            snippet = snippet.Replace("{stars}", BuildApplicationStarsSnippet(application.AverageConversionRating));
            snippet = snippet.Replace("{description}", string.IsNullOrEmpty(application.Application.Description) ? HtmlResources.ApplicationDefaultDescription : application.Application.Description);
            var node = CreateNodeWithSnippetContent(snippet);
            containerNode.AppendChild(node);

            // Return the node container.
            return node.SelectSingleNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.ApplicationSectionChildJQuery, refId));
        }

        /// <summary>
        /// Renders the summary information for the target application.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        /// <param name="application">The target application</param>
        public static void RenderApplicationSummary(HtmlNode containerNode, TargetApplication application)
        {
            _ = containerNode ?? throw new ArgumentNullException(nameof(containerNode));
            _ = application ?? throw new ArgumentNullException(nameof(application));

            // Create a header entry
            var sectionNode = HtmlNode.CreateNode(HtmlResources.SnippetEmptyDiv);
            sectionNode.InnerHtml = HtmlResources.TargetApplicationSummaryHeading;
            containerNode.AppendChild(sectionNode);

            // Create header for the application
            var snippet = HtmlResources.TargetApplicationSnippetSummary;
            var refId = Guid.NewGuid().ToString();
            snippet = snippet.Replace("{refId}", refId);

            // Build the description rows.
            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyNumberOfMessages, application.Application.Messages.Count));
            descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyNumberOfEndpoints, application.Application.Endpoints.Count));
            descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyNumberofChannels, application.Application.Channels.Count));
            descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyNumberOfIntermediaries, application.Application.Intermediaries.Count));
            descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyNumberOfScenarios, application.Scenarios.Count));
            descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyNumberOfResources, application.Resources.Count));
            snippet = snippet.Replace("{description}", descriptionBuilder.ToString());

            var node = CreateNodeWithSnippetContent(snippet);
            containerNode.AppendChild(node);
        }

        /// <summary>
        /// Renders a placeholder for when no scenarios are found.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        public static void RenderNoScenario(HtmlNode containerNode)
        {
            if (containerNode == null) throw new ArgumentNullException(nameof(containerNode));

            // Create default text
            containerNode.AppendChild(CreateNodeWithSnippetContent(HtmlResources.TargetApplicationNoScenarios));
        }

        /// <summary>
        /// Renders a target scenario by recursively walking the scenario stages.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        /// <param name="scenario">The target scenario.</param>
        public static void RenderTargetScenario(HtmlNode containerNode, TargetScenario scenario)
        {
            _ = containerNode ?? throw new ArgumentNullException(nameof(containerNode));
            _ = scenario ?? throw new ArgumentNullException(nameof(scenario));

            // Write the heading 
            RenderTargetScenarioHeading(containerNode, scenario);

            RecurseTargetScenarioStage(containerNode, scenario.Activator);
        }

        /// <summary>
        /// Renders the message bus application.
        /// </summary>
        /// <param name="containerNode">The HTML container node.</param>
        /// <param name="messageBus">The message bus.</param>
        public static void RenderMessageBusApplication(HtmlNode containerNode, MessageBus messageBus)
        {
            _ = containerNode ?? throw new ArgumentNullException(nameof(containerNode));
            _ = messageBus ?? throw new ArgumentNullException(nameof(messageBus));

            var snippet = HtmlResources.TargetMessageBusSectionSnippet;
            var refId = Guid.NewGuid().ToString();
            snippet = snippet.Replace("{refId}", refId);
            snippet = snippet.Replace("{name}", messageBus.Name);
            snippet = snippet.Replace("{description}", string.IsNullOrEmpty(messageBus.Description) ? HtmlResources.ApplicationDefaultDescription : messageBus.Description);
            var node = CreateNodeWithSnippetContent(snippet);
            containerNode.AppendChild(node);

            // Build the resources.
            var childNode = node.SelectSingleNode(HtmlResources.TargetMessageBusSectionJQuery);
            RenderTargetApplicationResources(childNode, messageBus.Resources);
        }

        /// <summary>
        /// This builds a new dropdown in the navbar so you can select resources.
        /// </summary>
        public static void BuildScenarioSelector(ReportFile<TargetApplication> target)
        {
            _ = target ?? throw new ArgumentNullException(nameof(target));

            // Get a reference to the navbar <ul>.
            var container = target.ReportHtml.DocumentNode.SelectSingleNode(HtmlResources.NavbarListJQuery);

            // Create a new <li> within the navbar.
            var dropdownNode = HtmlNode.CreateNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.SnippetNavbarDropdown, HtmlResources.NavbarScenariosHeading, HtmlResources.NavbarResourcesId));
            container.AppendChild(dropdownNode);

            // Select the dropdown list as the current node.
            var dropdown = dropdownNode.SelectSingleNode(HtmlResources.NavbarResourcesDropdownJQuery);
            dropdown.RemoveAllChildren();

            // Iterate through the scenarios
            foreach (var scenario in target.ReportData.Scenarios)
            {
                var node = HtmlNode.CreateNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.SnippetDropdownMenuItem, string.Concat("#", scenario.Name), scenario.Name));
                dropdown.AppendChild(node);
            }
        }

        /// <summary>
        /// This builds a list of resource entries within the containing node.
        /// </summary>
        /// <param name="containerNode">The containing HTML node.</param>
        /// <param name="resourceItems">The resources.</param>
        private static void RenderTargetApplicationResources(HtmlNode containerNode, IList<TargetResourceTemplate> resourceItems)
        {
            // Return if no resources
            if (resourceItems == null || resourceItems.Count == 0) return;

            // Sort
            var resources = resourceItems.ToList();
            resources.Sort(Comparers.SortTargetResourceTemplatesByTypeAndName);

            // Create a header entry
            var sectionNode = HtmlNode.CreateNode(HtmlResources.SnippetEmptyDiv);
            sectionNode.InnerHtml = string.Format(CultureInfo.CurrentCulture, HtmlResources.ApplicationSubsectionHeading, HtmlResources.TargetResourceSectionResources, HtmlResources.TargetResourceSectionResources);
            containerNode.AppendChild(sectionNode);

            // Iterate through the resource definitions
            foreach (var resource in resources)
            {
                var snippet = HtmlResources.TargetResourceSnippet;
                var refId = Guid.NewGuid().ToString();
                snippet = snippet.Replace("{icon}", ResourceFormatter.GetTargetResourceIconFromType(resource.ResourceType));
                snippet = snippet.Replace("{refId}", refId);
                snippet = snippet.Replace("{type}", ResourceFormatter.GetTargetResourceFriendlyName(resource.ResourceType));

                // Build the description rows.
                var descriptionBuilder = new StringBuilder();
                descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyResourceName, resource.ResourceName));
                descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyResourceType, resource.ResourceType));
                descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyTemplateType, resource.TemplateType));
                descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyTemplateKey, resource.TemplateKey));
                snippet = snippet.Replace("{description}", descriptionBuilder.ToString());

                var node = CreateNodeWithSnippetContent(snippet);
                containerNode.AppendChild(node);
            }
        }

        /// <summary>
        /// Renders a target scenario heading.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        /// <param name="scenario">The target scenario.</param>
        private static void RenderTargetScenarioHeading(HtmlNode containerNode, TargetScenario scenario)
        {
            // Create a header entry
            var sectionNode = HtmlNode.CreateNode(HtmlResources.SnippetEmptyDiv);
            sectionNode.InnerHtml = string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetScenarioHeading, scenario.Name, scenario.Name);
            containerNode.AppendChild(sectionNode);
        }

        /// <summary>
        /// Recursively walks the stages through the children to write the scenario.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        /// <param name="stage">The scenario stage.</param>
        private static void RecurseTargetScenarioStage(HtmlNode containerNode, TargetScenarioStage stage)
        {
            switch (stage.MessagingObject.Type)
            {
                case MessagingObjectType.Endpoint:
                    RenderTargetScenarioEndpoint(containerNode, (Endpoint)stage.MessagingObject);
                    break;

                case MessagingObjectType.Intermediary:
                    RenderTargetScenarioIntermediary(containerNode, (Intermediary)stage.MessagingObject);
                    break;

                case MessagingObjectType.Channel:
                    RenderTargetScenarioChannel(containerNode, (Channel)stage.MessagingObject);
                    break;
            }

            stage.FollowingStages.ToList().ForEach(s => RecurseTargetScenarioStage(containerNode, s));
        }

        /// <summary>
        /// Renders a target endpoint.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        /// <param name="endpoint">The target endpoint.</param>
        private static void RenderTargetScenarioEndpoint(HtmlNode containerNode, Endpoint endpoint)
        {
            var snippet = HtmlResources.TargetApplicationSnippetMessagingObject;
            var refId = Guid.NewGuid().ToString();
            snippet = snippet.Replace("{icon}", HtmlResources.TargetMessagingObjectIconEndpoint);
            snippet = snippet.Replace("{refId}", refId);
            snippet = snippet.Replace("{name}", endpoint.Name);
            snippet = snippet.Replace("{stars}", BuildResourceStarsSnippetFromRating((int)endpoint.Rating));

            // Build the description rows.
            var descriptionBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(endpoint.Description)) descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetDescriptionRow, endpoint.Description));
            descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyEndpointType, endpoint.EndpointType.ToString()));
            descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyMessageExchangePattern, endpoint.MessageExchangePattern.ToString()));
            descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyMessageDeliveryGuarantee, endpoint.MessageDeliveryGuarantee.ToString()));
            snippet = snippet.Replace("{description}", descriptionBuilder.ToString());

            var node = CreateNodeWithSnippetContent(snippet);
            containerNode.AppendChild(node);

            // Build the analysis content for this resource.
            var childNode = node.SelectSingleNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.ApplicationResourceChildJQuery, refId));

            // Create the properties.
            RenderMessagingObjectProperties(childNode, endpoint);

            // Create the analysis messages:
            RenderMessagingObjectMessages(childNode, endpoint);

            // List out the resources that will be created for the item.
            RenderMessagingObjectResources(childNode, endpoint);

            // Create the links.
            RenderMessagingObjectLinks(childNode, endpoint);
        }

        /// <summary>
        /// Renders a target intermediary.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        /// <param name="intermediary">The target intermediary.</param>
        private static void RenderTargetScenarioIntermediary(HtmlNode containerNode, Intermediary intermediary)
        {
            var snippet = HtmlResources.ApplicationSnippetResource;
            var refId = Guid.NewGuid().ToString();
            snippet = snippet.Replace("{icon}", HtmlResources.TargetMessagingObjectIconIntermediary);
            snippet = snippet.Replace("{refId}", refId);
            snippet = snippet.Replace("{name}", intermediary.Name);
            snippet = snippet.Replace("{stars}", BuildResourceStarsSnippetFromRating((int)intermediary.Rating));

            // Build the description rows.
            var descriptionBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(intermediary.Description)) descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetDescriptionRow, intermediary.Description));
            descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyIntermediaryType, intermediary.IntermediaryType.ToString()));
            if (intermediary is MessageRouter) descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyMessageRouterType, ((MessageRouter)intermediary).MessageRouterType.ToString()));
            if (intermediary is MessageProcessor) descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyMessageProcessorType, ((MessageProcessor)intermediary).MessageProcessorType.ToString()));
            snippet = snippet.Replace("{description}", descriptionBuilder.ToString());

            var node = CreateNodeWithSnippetContent(snippet);
            containerNode.AppendChild(node);

            // Build the analysis content for this resource.
            var childNode = node.SelectSingleNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.ApplicationResourceChildJQuery, refId));

            // Create the properties.
            RenderMessagingObjectProperties(childNode, intermediary);

            // Create the analysis messages:
            RenderMessagingObjectMessages(childNode, intermediary);

            // List out the resources that will be created for the item.
            RenderMessagingObjectResources(childNode, intermediary);

            // Create the links.
            RenderMessagingObjectLinks(childNode, intermediary);
        }

        /// <summary>
        /// Renders a target channel.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        /// <param name="channel">The target intermediary.</param>
        private static void RenderTargetScenarioChannel(HtmlNode containerNode, Channel channel)
        {
            var snippet = HtmlResources.ApplicationSnippetResource;
            var refId = Guid.NewGuid().ToString();
            snippet = snippet.Replace("{icon}", HtmlResources.TargetMessagingObjectIconChannel);
            snippet = snippet.Replace("{refId}", refId);
            snippet = snippet.Replace("{name}", channel.Name);
            snippet = snippet.Replace("{stars}", BuildResourceStarsSnippetFromRating((int)channel.Rating));

            // Build the description rows.
            var descriptionBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(channel.Description)) descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetDescriptionRow, channel.Description));
            descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyChannelType, channel.ChannelType.ToString()));
            if (channel is TriggerChannel && !string.IsNullOrEmpty(((TriggerChannel)channel).TriggerUrl)) descriptionBuilder.Append(string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPropertyRow, HtmlResources.TargetApplicationPropertyTriggerUrl, ((TriggerChannel)channel).TriggerUrl));
            snippet = snippet.Replace("{description}", descriptionBuilder.ToString());

            var node = CreateNodeWithSnippetContent(snippet);
            containerNode.AppendChild(node);

            // Build the analysis content for this resource.
            var childNode = node.SelectSingleNode(string.Format(CultureInfo.CurrentCulture, HtmlResources.ApplicationResourceChildJQuery, refId));

            // Create the properties.
            RenderMessagingObjectProperties(childNode, channel);

            // Create the analysis messages:
            RenderMessagingObjectMessages(childNode, channel);

            // List out the resources that will be created for the item.
            RenderMessagingObjectResources(childNode, channel);

            // Create the links.
            RenderMessagingObjectLinks(childNode, channel);
        }

        /// <summary>
        /// Renders the properties of the messaging object.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        /// <param name="messagingObject">The messaging object.</param>
        private static void RenderMessagingObjectProperties(HtmlNode containerNode, MessagingObject messagingObject)
        {
            containerNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetHeading, HtmlResources.AnalysisHeadingProperties)));
            if (messagingObject.Properties == null || messagingObject.Properties.Count == 0)
            {
                // Add a placeholder if there are no relationships.
                containerNode.AppendChild(
                    CreateNodeWithSnippetContent(
                        string.Format(
                            CultureInfo.CurrentCulture, 
                            HtmlResources.AnalysisSnippetInformationMessage, 
                            HtmlResources.AnalysisMessageNoProperties)));
            }
            else
            {
                RenderMessagingObjectPropertyDictionary(containerNode, messagingObject.Properties, string.Empty);
            }
        }

        /// <summary>
        /// Renders a property dictionary.
        /// </summary>
        /// <param name="containerNode">The HTML container node.</param>
        /// <param name="dictionary">The dictionary to render.</param>
        /// <param name="indent">Indentation for child properties.</param>
        private static void RenderMessagingObjectPropertyDictionary(HtmlNode containerNode, IDictionary<string, object> dictionary, string indent)
        {
            foreach (var property in dictionary)
            {
                if (property.Value is string)
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
                else if (property.Value is IDictionary<string, object>)
                {
                    containerNode.AppendChild(
                        CreateNodeWithSnippetContent(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                HtmlResources.TargetAnalysisSnippetProperty,
                                indent,
                                property.Key,
                                string.Empty)));
                    RenderMessagingObjectPropertyDictionary(containerNode, (IDictionary<string, object>)property.Value, string.Concat(indent, HtmlResources.TargetAnalysisSnippetPropertyIndent));
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
                                HtmlResources.AnalysisMessageNoProperties)));
                }
            }
        }

        /// <summary>
        /// Writes the messages associated with the item.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        /// <param name="messagingObject">The messaging object.</param>
        private static void RenderMessagingObjectMessages(HtmlNode containerNode, MessagingObject messagingObject)
        {
            containerNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetHeading, HtmlResources.AnalysisHeadingMessages)));
            if (messagingObject.ReportMessages == null || messagingObject.ReportMessages.Count == 0)
            {
                // Add a placeholder if there are no report messages.
                containerNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetInformationMessage, HtmlResources.AnalysisMessageNoMessages)));
            }
            else
            {
                foreach (var item in messagingObject.ReportMessages)
                {
                    if (item.Severity == MessageSeverity.Error)
                    {
                        containerNode.AppendChild(
                            CreateNodeWithSnippetContent(
                                string.Format(
                                    CultureInfo.CurrentCulture, 
                                    HtmlResources.AnalysisSnippetErrorMessage, 
                                    item.Message)));
                    }
                    else if (item.Severity == MessageSeverity.Warning)
                    {
                        containerNode.AppendChild(
                            CreateNodeWithSnippetContent(
                                string.Format(
                                    CultureInfo.CurrentCulture, 
                                    HtmlResources.AnalysisSnippetWarningMessage, 
                                    item.Message)));
                    }
                    else
                    {
                        containerNode.AppendChild(
                            CreateNodeWithSnippetContent(
                                string.Format(
                                    CultureInfo.CurrentCulture, 
                                    HtmlResources.AnalysisSnippetInformationMessage, 
                                    item.Message)));
                    }
                }
            }
        }

        /// <summary>
        /// Writes the resources associated with the item.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        /// <param name="messagingObject">The messaging object.</param>
        private static void RenderMessagingObjectResources(HtmlNode containerNode, MessagingObject messagingObject)
        {
            containerNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetHeading, HtmlResources.AnalysisHeadingResources)));
            if (messagingObject.Resources == null || messagingObject.Resources.Count == 0)
            {
                // Add a placeholder if there are no relationships.
                containerNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetInformationMessage, HtmlResources.AnalysisMessageNoResources)));
            }
            else
            {
                foreach (var resource in messagingObject.Resources)
                {
                    containerNode.AppendChild(
                        CreateNodeWithSnippetContent(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                HtmlResources.AnalysisSnippetTargetResource,
                                ResourceFormatter.GetTargetResourceIconFromType(resource.ResourceType),
                                ResourceFormatter.GetTargetResourceFriendlyName(resource.ResourceType),
                                resource.ResourceName)));
                }
            }
        }

        /// <summary>
        /// Writes the links associated with the item.
        /// </summary>
        /// <param name="containerNode">The HTML container to render into.</param>
        /// <param name="messagingObject">The messaging object.</param>
        private static void RenderMessagingObjectLinks(HtmlNode containerNode, MessagingObject messagingObject)
        {
            containerNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetHeading, HtmlResources.AnalysisHeadingLinks)));
            if (messagingObject.ReportLinks == null || messagingObject.ReportLinks.Count == 0)
            {
                // Add a placeholder if there are no relationships.
                containerNode.AppendChild(CreateNodeWithSnippetContent(string.Format(CultureInfo.CurrentCulture, HtmlResources.AnalysisSnippetInformationMessage, HtmlResources.AnalysisMessageNoReferenceLinks)));
            }
            else
            {
                foreach (var link in messagingObject.ReportLinks)
                {
                    containerNode.AppendChild(
                        CreateNodeWithSnippetContent(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                HtmlResources.AnalysisSnippetTargetReportLink,
                                link,
                                link)));
                }
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

        /// <summary>
        /// Builds the star rating based on the rating.
        /// </summary>
        /// <param name="numericRating">The numeric rating.</param>
        /// <returns>A star rating snippet.</returns>
        private static string BuildResourceStarsSnippetFromRating(decimal numericRating)
        {
            var snippet = new StringBuilder();
            for (var i = 0; i < ModelConstants.ConversionRatingMaximum; i++)
            {
                if (i < numericRating)
                {
                    snippet.Append(HtmlResources.ResourceStarSnippetFilled);
                }
                else
                {
                    snippet.Append(HtmlResources.ResourceStarSnippetEmpty);
                }
            }

            return snippet.ToString();
        }

        /// <summary>
        /// Builds a rating for the application based on the rating (0-100)
        /// </summary>
        /// <param name="rating">The rollup conversion rating for the application.</param>
        /// <returns>A star rating snippet.</returns>
        private static string BuildApplicationStarsSnippet(decimal rating)
        {
            var remaining = 100;
            var snippet = new StringBuilder();
            var percentSnippet = string.Format(CultureInfo.CurrentCulture, HtmlResources.TargetApplicationSnippetPercent, rating);

            while (rating >= 20)
            {
                snippet.Append(HtmlResources.TargetApplicationSnippetStarFilled);
                rating -= 20;
                remaining -= 20;
            }

            if (rating >= 10)
            {
                snippet.Append(HtmlResources.TargetApplicationSnippetHalfStarFilled);
                remaining -= 20;
            }

            while (remaining >= 20)
            {
                snippet.Append(HtmlResources.TargetApplicationSnippetStarEmpty);
                remaining -= 20;
            }

            snippet.Append(percentSnippet);

            return snippet.ToString();
        }
    }
}
