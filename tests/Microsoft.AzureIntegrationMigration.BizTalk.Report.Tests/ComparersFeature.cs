using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Messages;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Report.Tests
{
    /// <summary>
    /// Class containing tests for the <see cref="Comparers"/>.
    /// </summary>
    public class ComparersFeature
    {
        #region SortResourceRelationships Tests

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortByRelationshipXLesser(Tuple<ResourceRelationship, ResourceItem> x, Tuple<ResourceRelationship, ResourceItem> y, int result)
        {
            "Given a resource relationship"
                .x(() =>
                {
                    x = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Parent },
                        new ResourceItem() { Name = "x", Type = ModelConstants.ResourceSendPort }
                        );
                    });

            "And a resource relationship to compare to"
                .x(() =>
                {
                    y = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "y", Type = ModelConstants.ResourceReceivePort }
                        );
                });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceRelationships(x, y));

            "Expect x to be greater than y"
                .x(() => {
                    result.Should().BeLessThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortByRelationshipXGreater(Tuple<ResourceRelationship, ResourceItem> x, Tuple<ResourceRelationship, ResourceItem> y, int result)
        {
            "Given a resource relationship"
                .x(() =>
                {
                    x = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.ReferencedBy },
                        new ResourceItem() { Name = "x", Type = ModelConstants.ResourceSendPort }
                        );
                });

            "And a resource relationship to compare to"
                .x(() =>
                {
                    y = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "y", Type = ModelConstants.ResourceReceivePort }
                        );
                });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceRelationships(x, y));

            "Expect x to be greater than y"
                .x(() => {
                    result.Should().BeGreaterThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortByTypeXLesser(Tuple<ResourceRelationship, ResourceItem> x, Tuple<ResourceRelationship, ResourceItem> y, int result)
        {
            "Given a resource relationship"
                .x(() =>
                {
                    x = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "x", Type = ModelConstants.ResourceMetaModel }
                        );
                });

            "And a resource relationship to compare to"
                .x(() =>
                {
                    y = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "y", Type = ModelConstants.ResourceReceivePort }
                        );
                });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceRelationships(x, y));

            "Expect x to be greater than y"
                .x(() => {
                    result.Should().BeLessThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortByTypeXGreater(Tuple<ResourceRelationship, ResourceItem> x, Tuple<ResourceRelationship, ResourceItem> y, int result)
        {
            "Given a resource relationship"
                .x(() =>
                {
                    x = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "x", Type = ModelConstants.ResourceSendPort }
                        );
                });

            "And a resource relationship to compare to"
                .x(() =>
                {
                    y = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "y", Type = ModelConstants.ResourceReceivePort }
                        );
                });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceRelationships(x, y));

            "Expect x to be greater than y"
                .x(() => {
                    result.Should().BeGreaterThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortByNameXLesser(Tuple<ResourceRelationship, ResourceItem> x, Tuple<ResourceRelationship, ResourceItem> y, int result)
        {
            "Given a resource relationship"
                .x(() =>
                {
                    x = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "x", Type = ModelConstants.ResourceSendPort }
                        );
                });

            "And a resource relationship to compare to"
                .x(() =>
                {
                    y = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "y", Type = ModelConstants.ResourceSendPort }
                        );
                });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceRelationships(x, y));

            "Expect x to be greater than y"
                .x(() => {
                    result.Should().BeLessThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortByNameXGreater(Tuple<ResourceRelationship, ResourceItem> x, Tuple<ResourceRelationship, ResourceItem> y, int result)
        {
            "Given a resource relationship"
                .x(() =>
                {
                    x = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "z-x", Type = ModelConstants.ResourceSendPort }
                        );
                });

            "And a resource relationship to compare to"
                .x(() =>
                {
                    y = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "y", Type = ModelConstants.ResourceSendPort }
                        );
                });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceRelationships(x, y));

            "Expect x to be greater than y"
                .x(() => {
                    result.Should().BeGreaterThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortByNameXYEqual(Tuple<ResourceRelationship, ResourceItem> x, Tuple<ResourceRelationship, ResourceItem> y, int result)
        {
            "Given a resource relationship"
                .x(() =>
                {
                    x = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "aaa", Type = ModelConstants.ResourceSendPort }
                        );
                });

            "And a resource relationship to compare to"
                .x(() =>
                {
                    y = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "aaa", Type = ModelConstants.ResourceSendPort }
                        );
                });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceRelationships(x, y));

            "Expect x to be greater than y"
                .x(() => {
                    result.Should().Be(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void NullCheckXNull(Tuple<ResourceRelationship, ResourceItem> x, Tuple<ResourceRelationship, ResourceItem> y, int result, Exception e)
        {
            "Given a resource relationship"
                .x(() => x.Should().BeNull());

            "And a resource relationship to compare to"
                .x(() =>
                {
                    y = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "aaa", Type = ModelConstants.ResourceSendPort }
                        );
                });

            "When comparing x and y"
                .x(() => e = Record.Exception(() => result = Comparers.SortResourceRelationships(x, y)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("x"));
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void NullCheckYNull(Tuple<ResourceRelationship, ResourceItem> x, Tuple<ResourceRelationship, ResourceItem> y, int result, Exception e)
        {
            "Given a resource relationship"
                .x(() =>
                {
                    x = new Tuple<ResourceRelationship, ResourceItem>(
                        new ResourceRelationship() { ResourceRelationshipType = ResourceRelationshipType.Child },
                        new ResourceItem() { Name = "aaa", Type = ModelConstants.ResourceSendPort }
                        );
                });

            "And a resource relationship to compare to"
                .x(() => y.Should().BeNull());

            "When comparing x and y"
                .x(() => e = Record.Exception(() => result = Comparers.SortResourceRelationships(x, y)));

            "Then the constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("y"));
        }

        #endregion

        #region SortResourcesByName Tests

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourcesByNameXLesser(ResourceNode x, ResourceNode y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceItem() { Name = "x", Type = ModelConstants.ResourceSendPort });

            "And a resource to compare to"
                .x(() => y = new ResourceItem() { Name = "y", Type = ModelConstants.ResourceSendPort });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourcesByName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().BeLessThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourcesByNameYLesser(ResourceNode x, ResourceNode y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceItem() { Name = "zx", Type = ModelConstants.ResourceSendPort });

            "And a resource to compare to"
                .x(() => y = new ResourceItem() { Name = "yz", Type = ModelConstants.ResourceSendPort });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourcesByName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().BeGreaterThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourcesByNameEqual(ResourceNode x, ResourceNode y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceItem() { Name = "a", Type = ModelConstants.ResourceSendPort });

            "And a resource to compare to"
                .x(() => y = new ResourceItem() { Name = "a", Type = ModelConstants.ResourceSendPort });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourcesByName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().Be(0);
                });
        }

        #endregion

        #region SortResourceDefinitionsByTypeAndName Tests

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourceDefinitionsByTypeAndNameXTypeLesser(ResourceDefinition x, ResourceDefinition y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceDefinition() { Name = "z", Type = ModelConstants.ResourceDefinitionApplicationDefinition });

            "And a resource relationship to compare to"
                .x(() => y = new ResourceDefinition() { Name = "y", Type = ModelConstants.ResourceDefinitionBindings });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceDefinitionsByTypeAndName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().BeLessThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourceDefinitionsByTypeAndNameYTypeLesser(ResourceDefinition x, ResourceDefinition y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceDefinition() { Name = "x", Type = ModelConstants.ResourceDefinitionBindings });

            "And a resource relationship to compare to"
                .x(() => y = new ResourceDefinition() { Name = "y", Type = ModelConstants.ResourceDefinitionApplicationDefinition });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceDefinitionsByTypeAndName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().BeGreaterThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourceDefinitionsByTypeAndNameXNameLesser(ResourceDefinition x, ResourceDefinition y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceDefinition() { Name = "x", Type = ModelConstants.ResourceDefinitionBindings });

            "And a resource relationship to compare to"
                .x(() => y = new ResourceDefinition() { Name = "y", Type = ModelConstants.ResourceDefinitionBindings });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceDefinitionsByTypeAndName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().BeLessThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourceDefinitionsByTypeAndNameYNameLesser(ResourceDefinition x, ResourceDefinition y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceDefinition() { Name = "zx", Type = ModelConstants.ResourceDefinitionBindings });

            "And a resource relationship to compare to"
                .x(() => y = new ResourceDefinition() { Name = "y", Type = ModelConstants.ResourceDefinitionBindings });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceDefinitionsByTypeAndName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().BeGreaterThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourceDefinitionsByTypeAndNameXYEqual(ResourceDefinition x, ResourceDefinition y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceDefinition() { Name = "a", Type = ModelConstants.ResourceDefinitionBindings });

            "And a resource relationship to compare to"
                .x(() => y = new ResourceDefinition() { Name = "a", Type = ModelConstants.ResourceDefinitionBindings });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceDefinitionsByTypeAndName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().Be(0);
                });
        }

        #endregion

        #region SortResourcesByTypeAndName Tests

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourceItemsByTypeAndNameXTypeLesser(ResourceItem x, ResourceItem y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceItem() { Name = "z", Type = ModelConstants.ResourceReceivePort });

            "And a resource relationship to compare to"
                .x(() => y = new ResourceItem() { Name = "y", Type = ModelConstants.ResourceSendPort });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceItemsByTypeAndName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().BeLessThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourceItemsByTypeAndNameYTypeLesser(ResourceItem x, ResourceItem y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceItem() { Name = "x", Type = ModelConstants.ResourceReceivePort });

            "And a resource relationship to compare to"
                .x(() => y = new ResourceItem() { Name = "y", Type = ModelConstants.ResourceReceiveLocation });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceItemsByTypeAndName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().BeGreaterThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourceItemsByTypeAndNameXNameLesser(ResourceItem x, ResourceItem y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceItem() { Name = "x", Type = ModelConstants.ResourceReceiveLocation });

            "And a resource relationship to compare to"
                .x(() => y = new ResourceItem() { Name = "y", Type = ModelConstants.ResourceReceiveLocation });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceItemsByTypeAndName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().BeLessThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourceItemsByTypeAndNameYNameLesser(ResourceItem x, ResourceItem y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceItem() { Name = "zx", Type = ModelConstants.ResourceReceiveLocation });

            "And a resource relationship to compare to"
                .x(() => y = new ResourceItem() { Name = "y", Type = ModelConstants.ResourceReceiveLocation });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceItemsByTypeAndName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().BeGreaterThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortResourceItemsByTypeAndNameXYEqual(ResourceItem x, ResourceItem y, int result)
        {
            "Given a resource x"
                .x(() => x = new ResourceItem() { Name = "a", Type = ModelConstants.ResourceReceiveLocation });

            "And a resource relationship to compare to"
                .x(() => y = new ResourceItem() { Name = "a", Type = ModelConstants.ResourceReceiveLocation });

            "When comparing x and y"
                .x(() => result = Comparers.SortResourceItemsByTypeAndName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().Be(0);
                });
        }


        #endregion

        #region SortTargetMessagingObjectByName Tests

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortTargetMessagingObjectByNameXLesser(MessagingObject x, MessagingObject y, int result)
        {
            "Given a resource x"
                .x(() => x = new DocumentMessage() { Name = "x" });

            "And a resource relationship to compare to"
                .x(() => y = new DocumentMessage() { Name = "y" });

            "When comparing x and y"
                .x(() => result = Comparers.SortTargetMessagingObjectByName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().BeLessThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortTargetMessagingObjectByNameXGreater(MessagingObject x, MessagingObject y, int result)
        {
            "Given a resource x"
                .x(() => x = new DocumentMessage() { Name = "z" });

            "And a resource relationship to compare to"
                .x(() => y = new DocumentMessage() { Name = "y" });

            "When comparing x and y"
                .x(() => result = Comparers.SortTargetMessagingObjectByName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().BeGreaterThan(0);
                });
        }

        /// <summary>
        /// Tests comparison between two resource relationships.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <param name="result">The result of the comparison.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void SortTargetMessagingObjectByNameXYEqual(MessagingObject x, MessagingObject y, int result)
        {
            "Given a resource x"
                .x(() => x = new DocumentMessage() { Name = "z" });

            "And a resource relationship to compare to"
                .x(() => y = new DocumentMessage() { Name = "z" });

            "When comparing x and y"
                .x(() => result = Comparers.SortTargetMessagingObjectByName(x, y));

            "Expect x to be less than y"
                .x(() => {
                    result.Should().Be(0);
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
