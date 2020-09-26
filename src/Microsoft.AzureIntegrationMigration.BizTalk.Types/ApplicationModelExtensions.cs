using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types
{
    /// <summary>
    /// Defines extensions for the <see cref="AzureIntegrationServicesModel"/> class.
    /// </summary>
    public static class ApplicationModelExtensions
    {
        #region Resources

        #region Find All Resources

        /// <summary>
        /// Finds all resources in the source model.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <returns>A list of all discovered resources.</returns>
        public static IList<ResourceItem> FindAllResources(this AzureIntegrationServicesModel model)
        {
            _ = model = model ?? throw new ArgumentNullException(nameof(model));
            var results = new List<ResourceItem>();
            model.MigrationSource.ResourceContainers.ToList().ForEach(c => results.AddRange(c.FindAllResources()));
            return results;
        }

        /// <summary>
        /// Finds all resources in the container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>A list of all discovered resources.</returns>
        public static List<ResourceItem> FindAllResources(this ResourceContainer container)
        {
            _ = container = container ?? throw new ArgumentNullException(nameof(container));
            var results = new List<ResourceItem>();
            container.ResourceContainers.ToList().ForEach(c => results.AddRange(c.FindAllResources()));
            container.ResourceDefinitions.ToList().ForEach(d => results.AddRange(d.FindAllResources()));
            return results;
        }

        /// <summary>
        /// Finds all resources in the resource definition.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <returns>A list of all discovered resources.</returns>
        public static List<ResourceItem> FindAllResources(this ResourceDefinition definition)
        {
            _ = definition = definition ?? throw new ArgumentNullException(nameof(definition));
            var results = new List<ResourceItem>();
            results.AddRange(definition.Resources);
            definition.Resources.ToList().ForEach(c => results.AddRange(c.FindAllResources()));
            return results;
        }

        /// <summary>
        /// Finds all resources in the resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>A list of all discovered resources.</returns>
        public static List<ResourceItem> FindAllResources(this ResourceItem resource)
        {
            _ = resource = resource ?? throw new ArgumentNullException(nameof(resource));
            var results = new List<ResourceItem>();
            results.AddRange(resource.Resources);
            resource.Resources.ToList().ForEach(c => results.AddRange(c.FindAllResources()));
            return results;
        }

        #endregion

        #region Find Resources

        /// <summary>
        /// Finds all resources in the resource, recursively.
        /// </summary>
        /// <param name="model">The source model.</param>
        /// <param name="key">The key for the resource to find.</param>
        /// <returns>A list of all discovered resources.</returns>
        public static ResourceItem FindResourceByKey(this AzureIntegrationServicesModel model, string key)
        {
            _ = model = model ?? throw new ArgumentNullException(nameof(model));
            var results = model.FindAllResources();
            return results.Where(r => r.Key == key).FirstOrDefault();
        }

        /// <summary>
        /// Finds a resource anywhere in the tree by its RefId.
        /// </summary>
        /// <param name="model">The source model.</param>
        /// <param name="key">The key for the resource to find.</param>
        /// <returns>A list of all discovered resources.</returns>
        public static ResourceItem FindResourceByRefId(this AzureIntegrationServicesModel model, string refId)
        {
            _ = model = model ?? throw new ArgumentNullException(nameof(model));
            var results = model.FindAllResources();
            return results.Where(r => r.RefId == refId).FirstOrDefault();
        }

        /// <summary>
        /// Gets all the resources of a given type within a resource container.
        /// </summary>
        /// <param name="model">The <see cref="ResourceContainer"/>.</param>
        /// <param name="type">The resource type to filter on.</param>
        /// <returns>A list of <see cref="ResourceItem"/> matching the pattern.</returns>
        public static IList<ResourceItem> FindResourcesByType(this AzureIntegrationServicesModel model, string type)
        {
            _ = model = model ?? throw new ArgumentNullException(nameof(model));
            return model.FindAllResources().Where(r => r.Type == type).ToList();
        }

        /// <summary>
        /// Gets all the resources of a given type within a resource container.
        /// </summary>
        /// <param name="container">The <see cref="ResourceContainer"/>.</param>
        /// <param name="type">The resource type to filter on.</param>
        /// <returns>A list of <see cref="ResourceItem"/> matching the pattern.</returns>
        public static List<ResourceItem> FindResourcesByType(this ResourceContainer container, string type)
        {
            _ = container = container ?? throw new ArgumentNullException(nameof(container));
            var results = new List<ResourceItem>();
            container.ResourceContainers.ToList().ForEach(c => results.AddRange(c.FindResourcesByType(type)));
            container.ResourceDefinitions.ToList().ForEach(d => results.AddRange(d.FindResourcesByType(type)));
            return results;
        }

        /// <summary>
        /// Gets all the resources matching the type within the resource definition.
        /// </summary>
        /// <param name="definition">The resource definition.</param>
        /// <param name="type">The resource type.</param>
        /// <returns>A list of the <see cref="ResourceItem"/> matching the pattern.</returns>
        public static List<ResourceItem> FindResourcesByType(this ResourceDefinition definition, string type)
        {
            _ = definition = definition ?? throw new ArgumentNullException(nameof(definition));
            var results = new List<ResourceItem>();
            results.AddRange(definition.Resources.Where(r => r.Type == type));
            definition.Resources.ToList().ForEach(c => results.AddRange(c.FindResourcesByType(type)));
            return results;
        }

        /// <summary>
        /// Gets all the resources matching the type within the resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="type">The resource type.</param>
        /// <returns>A list of the <see cref="ResourceItem"/> matching the pattern.</returns>
        public static List<ResourceItem> FindResourcesByType(this ResourceItem resource, string type)
        {
            _ = resource = resource ?? throw new ArgumentNullException(nameof(resource));
            var results = new List<ResourceItem>();
            results.AddRange(resource.Resources.Where(r => r.Type == type));
            resource.Resources.ToList().ForEach(c => results.AddRange(c.FindResourcesByType(type)));
            return results;
        }

        #endregion

        #region Find Related Resources

        /// <summary>
        /// Gets all the resources matching the type within the resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="model">The model to search.</param>
        /// <param name="relationshipType">The type of relationship to filter by.</param>
        /// <param name="type">The resource type.</param>
        /// <returns>A list of the <see cref="ResourceItem"/> matching the pattern.</returns>
        public static List<ResourceItem> FindRelatedResourcesByType(this ResourceItem resource, AzureIntegrationServicesModel model, ResourceRelationshipType relationshipType, string type)
        {
            _ = resource = resource ?? throw new ArgumentNullException(nameof(resource));

            var relatedResources = new List<ResourceItem>();

            var children = resource.ResourceRelationships?.Where(r => r.ResourceRelationshipType == relationshipType);
            if (children != null && children.Any())
            {
                foreach (var child in children)
                {
                    var relatedResource = model.FindResourceByRefId(child.ResourceRefId);
                    if (relatedResource != null && relatedResource.Type == type)
                    {
                        relatedResources.Add(relatedResource);
                    }
                }
            }

            return relatedResources;
        }

        #endregion

        #endregion

        #region Resource Containers

        #region Find Resource Containers

        /// <summary>
        /// Gets all the resource containers of a given type within the source model.
        /// </summary>
        /// <param name="source">The <see cref="ResourceItem"/>.</param>
        /// <param name="type">The resource container type to filter on.</param>
        /// <returns>A list of <see cref="ResourceContainer"/> matching the pattern.</returns>
        public static List<ResourceContainer> FindResourceContainersByType(this MigrationSource source, string type)
        {
            _ = source = source ?? throw new ArgumentNullException(nameof(source));
            var results = new List<ResourceContainer>();
            results.AddRange(source.ResourceContainers.FindResourceContainersByType(type));
            return results;
        }

        /// <summary>
        /// Gets all the resource containers of a given type within the list of resource containers (recursive).
        /// </summary>
        /// <param name="source">The <see cref="ResourceItem"/>.</param>
        /// <param name="type">The resource container type to filter on.</param>
        /// <returns>A list of <see cref="ResourceContainer"/> matching the pattern.</returns>
        public static List<ResourceContainer> FindResourceContainersByType(this IList<ResourceContainer> source, string type)
        {
            _ = source = source ?? throw new ArgumentNullException(nameof(source));
            var results = new List<ResourceContainer>();
            results.AddRange(source.Where(c => c.Type == type));
            source.ToList().ForEach(c => results.AddRange(c.ResourceContainers.FindResourceContainersByType(type)));
            return results;
        }

        #endregion

        #endregion

        #region Resource Definitions

        #region Find All Resource Definitions

        /// <summary>
        /// Finds all resource definitions.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <returns>The resource definitions.</returns>
        public static IList<ResourceDefinition> FindAllResourceDefinitions(this AzureIntegrationServicesModel model)
        {
            _ = model = model ?? throw new ArgumentNullException(nameof(model));
            var results = new List<ResourceDefinition>();
            model.MigrationSource.ResourceContainers.ToList().ForEach(c => results.AddRange(c.FindAllResourceDefinitions()));
            return results;
        }

        /// <summary>
        /// Finds all resource definitions in the container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>The resource definitions.</returns>
        public static List<ResourceDefinition> FindAllResourceDefinitions(this ResourceContainer container)
        {
            _ = container = container ?? throw new ArgumentNullException(nameof(container));
            var results = new List<ResourceDefinition>();
            results.AddRange(container.ResourceDefinitions);
            container.ResourceContainers.ToList().ForEach(c => results.AddRange(c.FindAllResourceDefinitions()));            
            return results;
        }

        #endregion

        #region Find Resource Definitions

        /// <summary>
        /// Finds the resource definition by refId.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="refId">The refId to search on.</param>
        /// <returns>The resource definition if found, or null.</returns>
        public static ResourceDefinition FindResourceDefinitionByRefId(this AzureIntegrationServicesModel model, string refId)
        {
            _ = model = model ?? throw new ArgumentNullException(nameof(model));
            var results = model.FindAllResourceDefinitions();
            return results.Where(rd => rd.RefId == refId).FirstOrDefault();
        }

        /// <summary>
        /// Finds a resource definition with the given resource key.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="definitionKey">The definition key.</param>
        /// <param name="definitionType">The definition type.</param>
        /// <returns>A resource definition, or null if none found.</returns>
        public static ResourceDefinition FindResourceDefinitionByKey(this AzureIntegrationServicesModel model, string definitionKey, string definitionType)
        {
            _ = model ?? throw new ArgumentNullException(nameof(model));
            _ = definitionKey ?? throw new ArgumentNullException(nameof(definitionKey));
            _ = definitionType ?? throw new ArgumentNullException(nameof(definitionType));

            var results = model.FindAllResourceDefinitions();
            return results.Where(rd => rd.Key == definitionKey && rd.Type == definitionType).FirstOrDefault();
        }

        #endregion

        #endregion

        #region Resource Templates

        /// <summary>
        /// Finds all resource templates in the target model.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <returns>A list of all discovered resource templatees.</returns>
        public static IList<TargetResourceTemplate> FindAllTargetResourceTemplates(this AzureIntegrationServicesModel model)
        {
            _ = model = model ?? throw new ArgumentNullException(nameof(model));
            var results = new List<TargetResourceTemplate>();

            results.AddRange(model.MigrationTarget.MessageBus.Resources);
            results.AddRange(model.MigrationTarget.MessageBus.Applications.SelectMany(a => a.Resources));
            results.AddRange(model.MigrationTarget.MessageBus.Applications.SelectMany(a => a.Messages).SelectMany(m => m.Resources));
            results.AddRange(model.MigrationTarget.MessageBus.Applications.SelectMany(a => a.Channels).SelectMany(c => c.Resources));            
            results.AddRange(model.MigrationTarget.MessageBus.Applications.SelectMany(a => a.Intermediaries).SelectMany(i => i.Resources));
            results.AddRange(model.MigrationTarget.MessageBus.Applications.SelectMany(a => a.Endpoints).SelectMany(e => e.Resources));
            
            return results;
        }

        #endregion
    }
}
