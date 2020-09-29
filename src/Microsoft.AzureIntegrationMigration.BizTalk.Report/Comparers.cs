// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Report
{
    /// <summary>
    /// Class containing comparison delegates.
    /// </summary>
    public static class Comparers
    {
        /// <summary>
        /// Standard Comparer delegate to sort resource relationships into a logical order for display.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <returns>An integer indicating the comparison between x and y.</returns>
        public static int SortResourceRelationships(Tuple<ResourceRelationship, ResourceItem> x, Tuple<ResourceRelationship, ResourceItem> y)
        {
            _ = x ?? throw new ArgumentNullException(nameof(x));
            _ = y ?? throw new ArgumentNullException(nameof(y));

            if (x.Item1.ResourceRelationshipType == y.Item1.ResourceRelationshipType)
            {
                // Equal - sort on type
                var resourceTypeComparison = string.Compare(ResourceFormatter.GetResourceFriendlyName(x.Item2.Type), ResourceFormatter.GetResourceFriendlyName(y.Item2.Type), true, CultureInfo.CurrentCulture);
                if (resourceTypeComparison == 0)
                {
                    // Equal - sort on name
                    return string.Compare(x.Item2.Name, y.Item2.Name, true, CultureInfo.CurrentCulture);
                }
                else
                {
                    return resourceTypeComparison;
                }
            };

            if (x.Item1.ResourceRelationshipType < y.Item1.ResourceRelationshipType)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Sorts resources by their name.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <returns>An integer indicating the comparison between x and y.</returns>
        public static int SortResourcesByName(ResourceNode x, ResourceNode y)
        {
            _ = x ?? throw new ArgumentNullException(nameof(x));
            _ = y ?? throw new ArgumentNullException(nameof(y));

            return string.Compare(x.Name, y.Name, true, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Sorts resources by their name.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <returns>An integer indicating the comparison between x and y.</returns>
        public static int SortResourceDefinitionsByTypeAndName(ResourceDefinition x, ResourceDefinition y)
        {
            _ = x ?? throw new ArgumentNullException(nameof(x));
            _ = y ?? throw new ArgumentNullException(nameof(y));

            if (x.Type == y.Type)
            {
                // Equal on type - sort on name
                return string.Compare(x.Name, y.Name, true, CultureInfo.CurrentCulture);
            };

            return string.Compare(ResourceFormatter.GetResourceDefinitionFriendlyName(x.Type), ResourceFormatter.GetResourceDefinitionFriendlyName(y.Type), true, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Sorts resources by their name.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <returns>An integer indicating the comparison between x and y.</returns>
        public static int SortResourceItemsByTypeAndName(ResourceItem x, ResourceItem y)
        {
            _ = x ?? throw new ArgumentNullException(nameof(x));
            _ = y ?? throw new ArgumentNullException(nameof(y));

            if (x.Type == y.Type)
            {
                // Equal on type - sort on name
                return string.Compare(x.Name, y.Name, true, CultureInfo.CurrentCulture);
            };

            return string.Compare(ResourceFormatter.GetResourceFriendlyName(x.Type), ResourceFormatter.GetResourceFriendlyName(y.Type), true, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Sorts target applications by their name.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <returns>An integer indicating the comparison between x and y.</returns>
        public static int SortTargetApplicationsByName(Application x, Application y)
        {
            _ = x ?? throw new ArgumentNullException(nameof(x));
            _ = y ?? throw new ArgumentNullException(nameof(y));

            return string.Compare(x.Name, y.Name, true, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Sorts messaging objects by their name.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <returns>An integer indicating the comparison between x and y.</returns>
        public static int SortTargetMessagingObjectByName(MessagingObject x, MessagingObject y)
        {
            _ = x ?? throw new ArgumentNullException(nameof(x));
            _ = y ?? throw new ArgumentNullException(nameof(y));

            return string.Compare(x.Name, y.Name, true, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Sorts messaging objects by their name.
        /// </summary>
        /// <param name="x">The first item to be compared.</param>
        /// <param name="y">The second item to be compared.</param>
        /// <returns>An integer indicating the comparison between x and y.</returns>
        public static int SortTargetResourceTemplatesByTypeAndName(TargetResourceTemplate x, TargetResourceTemplate y)
        {
            _ = x ?? throw new ArgumentNullException(nameof(x));
            _ = y ?? throw new ArgumentNullException(nameof(y));

            if (x.ResourceType == y.ResourceType)
            {
                // Equal on type - sort on name
                return string.Compare(x.ResourceName, y.ResourceName, true, CultureInfo.CurrentCulture);
            };

            return string.Compare(ResourceFormatter.GetTargetResourceFriendlyName(x.ResourceType), ResourceFormatter.GetTargetResourceFriendlyName(y.ResourceType), true, CultureInfo.CurrentCulture);
        }
    }
}
