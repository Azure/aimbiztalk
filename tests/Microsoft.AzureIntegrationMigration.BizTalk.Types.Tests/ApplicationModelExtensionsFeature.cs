// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Tests
{
    /// <summary>
    /// Tests for the <see cref="ApplicationModelExtensions"/> class.
    /// </summary>
    public class ApplicationModelExtensionsFeature
    {
        #region Application Model Extensions

        /// <summary>
        /// Scenario tests rule against the standard data model.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="resourceDefinition">The resourceDefinition to find.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void FindResourceDefinitionNoSuccess(AzureIntegrationServicesModel model, ResourceDefinition resourceDefinition)
        {
            "Given an empty source model"
                .x(() => model = new AzureIntegrationServicesModel());

            "When finding resource"
                .x(() => resourceDefinition = model.FindResourceDefinitionByKey(string.Empty, string.Empty));

            "Then resource definition should be null"
                .x(() => resourceDefinition.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests rule against the standard data model.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="resourceDefinition">The resourceDefinition to find.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void FindResourceDefinitionFirstLevelSuccess(AzureIntegrationServicesModel model, ResourceDefinition resourceDefinition)
        {
            "Given a source model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And the model has one resource definition"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "containerkey" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition { Key = "resourcekey", Type = "resourcetype" });
                });


            "When finding resource"
                .x(() => resourceDefinition = model.FindResourceDefinitionByKey("resourcekey", "resourcetype"));

            "Then resource definition should not be null"
                .x(() =>
                {
                    resourceDefinition.Should().NotBeNull();
                    resourceDefinition.Key.Should().Be("resourcekey");
                    resourceDefinition.Type.Should().Be("resourcetype");
                });
        }

        /// <summary>
        /// Scenario tests rule against the standard data model.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="resourceDefinition">The resourceDefinition to find.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void FindResourceThirdLevelSuccess(AzureIntegrationServicesModel model, ResourceDefinition resourceDefinition)
        {
            "Given a source model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And the model has one resource definition on level 3"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "containerkey1" });
                    
                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2" });
                    model.MigrationSource.ResourceContainers[1].ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2-1" });
                    model.MigrationSource.ResourceContainers[1].ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2-2" });

                    model.MigrationSource.ResourceContainers[1].ResourceContainers[1].ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2-2-1" });
                    model.MigrationSource.ResourceContainers[1].ResourceContainers[1].ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2-2-2" });

                    model.MigrationSource.ResourceContainers[1].ResourceContainers[1].ResourceContainers[1].ResourceDefinitions.Add(new ResourceDefinition { Key = "resourcekey", Type = "resourcetype" });
                });

            "When finding resource"
                .x(() => resourceDefinition = model.FindResourceDefinitionByKey("resourcekey", "resourcetype"));

            "Then resource definition should not be null"
                .x(() =>
                {
                    resourceDefinition.Should().NotBeNull();
                    resourceDefinition.Key.Should().Be("resourcekey");
                    resourceDefinition.Type.Should().Be("resourcetype");
                });
        }

        /// <summary>
        /// Scenario tests rule against the standard data model.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="resourceDefinition">The resourceDefinition to find.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void FindResourceThirdLevelNotFoundSuccess(AzureIntegrationServicesModel model, ResourceDefinition resourceDefinition)
        {
            "Given a source model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And the model has one resource definition on level 3 with different key"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "containerkey1" });

                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2" });
                    model.MigrationSource.ResourceContainers[1].ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2-1" });
                    model.MigrationSource.ResourceContainers[1].ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2-2" });

                    model.MigrationSource.ResourceContainers[1].ResourceContainers[1].ResourceDefinitions.Add(new ResourceDefinition { Key = "resourcekey1", Type = "resourcetype" });

                    model.MigrationSource.ResourceContainers[1].ResourceContainers[1].ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2-2-1" });
                    model.MigrationSource.ResourceContainers[1].ResourceContainers[1].ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2-2-2" });

                    model.MigrationSource.ResourceContainers[1].ResourceContainers[1].ResourceContainers[1].ResourceDefinitions.Add(new ResourceDefinition { Key = "resourcekey1", Type = "resourcetype" });
                });

            "When finding resource"
                .x(() => resourceDefinition = model.FindResourceDefinitionByKey("resourcekey", "resourcetype"));

            "Then resource definition should be null"
                .x(() =>
                {
                    resourceDefinition.Should().BeNull();
                });
        }

        /// <summary>
        /// Scenario tests rule against the standard data model.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="resourceDefinition">The resourceDefinition to find.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void FindResourceThirdLevelDefinitionKeySuccess(AzureIntegrationServicesModel model, ResourceDefinition resourceDefinition)
        {
            "Given a source model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And the model has one resource definition on level 3 with different key"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "containerkey1" });

                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2" });
                    model.MigrationSource.ResourceContainers[1].ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2-1" });
                    model.MigrationSource.ResourceContainers[1].ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2-2" });

                    model.MigrationSource.ResourceContainers[1].ResourceContainers[1].ResourceDefinitions.Add(new ResourceDefinition { Key = "resourcekey1", Type = "resourcetype" });

                    model.MigrationSource.ResourceContainers[1].ResourceContainers[1].ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2-2-1" });
                    model.MigrationSource.ResourceContainers[1].ResourceContainers[1].ResourceContainers.Add(new ResourceContainer() { Key = "containerkey2-2-2" });

                    model.MigrationSource.ResourceContainers[1].ResourceContainers[1].ResourceContainers[1].ResourceDefinitions.Add(new ResourceDefinition { Key = "resourcekey1", Type = "resourcetype" });
                });

            "When finding resource"
                .x(() => resourceDefinition = model.FindResourceDefinitionByKey("resourcekey1", "resourcetype"));

            "Then resource definition should be null"
                .x(() =>
                {
                    resourceDefinition.Should().NotBeNull();
                });
        }

        /// <summary>
        /// Scenario tests when the source model is empty no source is found.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="resource">The resource which should be found.</param>        
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void FindResourceItemByKeyWithEmptyModel(AzureIntegrationServicesModel model, ResourceItem resource)
        {
            "Given an empty source model"
                .x(() => model = new AzureIntegrationServicesModel());

            "When finding a resource"
                .x(() => resource = model.FindResourceByKey("key does not exist"));

            "Then resource definition should be null"
                .x(() => resource.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests when the source model contains resources at one level, the resource item is found
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="resource">The resource which should be found.</param>        
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void FindResourceItemByKeyWithOneLevel(AzureIntegrationServicesModel model, ResourceItem resource)
        {
            var resourceKeyToFind = "nameTofind";
            var resourceNameToFind = "nameToFind";

            "Given a source model with resources at the top level"
                .x(() =>
                {       
                    model = new AzureIntegrationServicesModel();

                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "containerkey1", Name = "containerName1" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition() { Key = "containerkey1-1", Name = "definitionName1-1" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Add(new ResourceItem() { Key = "containerkey1-1-1", Name = "resourceName1-1-1" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Add(new ResourceItem() { Key = resourceKeyToFind, Name = resourceNameToFind });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Add(new ResourceItem() { Key = "containerkey1-1-2", Name = "resourceName1-1-2" });
                });

            "When finding a resource"
                .x(() => resource = model.FindResourceByKey(resourceKeyToFind));

            "Then resource definition should be found"
                .x(() =>
                {
                    resource.Should().NotBeNull();
                    resource.Key.Should().Be(resourceKeyToFind);
                    resource.Name.Should().Be(resourceNameToFind);
                });
        }

        /// <summary>
        /// Scenario tests when the source model contains resources at two levels, the resource item is found
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="resource">The resource which should be found.</param>        
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void FindResourceItemByKeyWithTwoLevels(AzureIntegrationServicesModel model, ResourceItem resource)
        {
            var resourceKeyToFind = "nameTofind";
            var resourceNameToFind = "nameToFind";

            "Given a source model with resource definitions with resources"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();

                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "containerkey1", Name = "containerName1" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition() { Key = "containerkey1-1", Name = "definitionName1-1" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Add(new ResourceItem() { Key = "containerkey1-1-1", Name = "resourceName1-1-1" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Add(new ResourceItem() { Key = "containerkey1-1-2", Name = "resourceName1-1-2" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0].Resources.Add(new ResourceItem() { Key = "containerkey1-1-1-1", Name = "resourceName1-1-1-1" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0].Resources.Add(new ResourceItem() { Key = "containerkey1-1-1-2", Name = "resourceName1-1-1-2" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0].Resources.Add(new ResourceItem() { Key = resourceKeyToFind, Name = resourceNameToFind });
                });

            "When finding a resource"
                .x(() => resource = model.FindResourceByKey(resourceKeyToFind));

            "Then resource definition should be found"
                .x(() =>
                {
                    resource.Should().NotBeNull();
                    resource.Key.Should().Be(resourceKeyToFind);
                    resource.Name.Should().Be(resourceNameToFind);
                });
        }

        /// <summary>
        /// Scenario tests when the source model contains resources at two levels, but the resource is not there to be found.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="resource">The resource which should be found.</param>        
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void FindResourceItemByKeyWithTwoLevelsAndResourceNotPresent(AzureIntegrationServicesModel model, ResourceItem resource)
        {
            var resourceKeyToFind = "nameTofind";
            var resourceNameToFind = "nameToFind";

            "Given a source model with resource definitions with resources, containing resources"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();

                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "containerkey1", Name = "containerName1" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition() { Key = "containerkey1-1", Name = "definitionName1-1" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Add(new ResourceItem() { Key = "containerkey1-1-1", Name = "resourceName1-1-1" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Add(new ResourceItem() { Key = "containerkey1-1-2", Name = "resourceName1-1-2" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0].Resources.Add(new ResourceItem() { Key = "containerkey1-1-1-1", Name = "resourceName1-1-1-1" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0].Resources.Add(new ResourceItem() { Key = "containerkey1-1-1-2", Name = "resourceName1-1-1-2" });
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0].Resources.Add(new ResourceItem() { Key = resourceKeyToFind, Name = resourceNameToFind });
                });

            "When finding a resource that does not exist"
                .x(() => resource = model.FindResourceByKey("resourceDoesNotExist"));

            "Then resource definition should not be found"
                .x(() =>
                {
                    resource.Should().BeNull();
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
