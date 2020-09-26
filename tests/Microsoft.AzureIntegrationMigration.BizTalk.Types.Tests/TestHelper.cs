using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Newtonsoft.Json;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Tests
{
    /// <summary>
    /// Contains reusable helper methods for testing such as creation of standard object models.
    /// </summary>
    public static class TestHelper
    {        /// <summary>
        /// Builds a migration context.
        /// </summary>
        /// <returns>A migration context.</returns>
        public static MigrationContext BuildContext()
        {
            return new MigrationContext
            {
                WorkingFolder = @"c:\temp\workingfolder"
            };
        }

        /// <summary>
        /// Creates a default object model for parsing and building a report.
        /// </summary>
        /// <returns></returns>
        public static void AddToMigrationSource(MigrationSource source, int depthCount, int definitionsCount, int reportCount, int innerReportCount)
        {
            _ = source = source ?? throw new ArgumentNullException(nameof(source));

            var resourceContainer = new ResourceContainer();
            source.ResourceContainers.Add(resourceContainer);

            foreach (var depth in Enumerable.Range(0, depthCount))
            {
                foreach (var definition in Enumerable.Range(0, definitionsCount))
                {
                    var resourceDefinition = new ResourceDefinition()
                    {
                        Key = $"key-{depth}-{definition}",
                        Name = $"name-{depth}-{definition}",
                        Type = $"type-{depth}-{definition}",
                    }; 

                    resourceContainer.ResourceDefinitions.Add(resourceDefinition);

                    foreach (var report in Enumerable.Range(0, reportCount))
                    {
                        var sourceResource = new ResourceItem
                        {
                            Key = $"key-{depth}-{definition}-{report}",
                            Name = $"name-{depth}-{definition}-{report}",
                            Type = $"type-{depth}-{definition}",
                            Rating = ConversionRating.FullConversion
                        };

                        resourceDefinition.Resources.Add(sourceResource);

                        foreach (var innerReport in Enumerable.Range(0, innerReportCount))
                        {
                            sourceResource.Resources.Add(new ResourceItem
                            {
                                Key = $"key-{depth}-{definition}-{report}-{innerReport}",
                                Name = $"name-{depth}-{definition}-{report}-{innerReport}",
                                RefId = $"refid-{depth}-{definition}-{report}-{innerReport}",
                                Type = $"type-{depth}-{definition}",
                                Rating = ConversionRating.NoRating
                            });
                        }
                    }
                }

                resourceContainer.Type = $"type-{depth}";

                var next = new ResourceContainer();
                resourceContainer.ResourceContainers.Add(next);
                resourceContainer = next;
            }
        }
    }
}
