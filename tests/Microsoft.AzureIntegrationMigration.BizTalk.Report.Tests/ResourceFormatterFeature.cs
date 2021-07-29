// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
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
    /// Class containing tests for the <see cref="ResourceFormatter"/>.
    /// </summary>
    public class ResourceFormatterFeature
    {
        #region Resource Container Friendly Name conversion

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource container types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceContainerTypeFriendlyNameMsi(string type, string friendlyName, Exception e)
        {
            "Given a resource container type"
                .x(() => type = ModelConstants.ResourceContainerMsi);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceContainerFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("MSI");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource container types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceContainerTypeFriendlyNameCab(string type, string friendlyName, Exception e)
        {
            "Given a resource container type"
                .x(() => type = ModelConstants.ResourceContainerCab);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceContainerFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("CAB");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource container types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceContainerTypeFriendlyNameAssembly(string type, string friendlyName, Exception e)
        {
            "Given a resource container type"
                .x(() => type = ModelConstants.ResourceContainerAssembly);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceContainerFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Assembly");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource container types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceContainerTypeFriendlyNameDefault(string type, string friendlyName, Exception e)
        {
            "Given a resource container type"
                .x(() => type = "An unknown resource container");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceContainerFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Resource Container");
                });
        }

        #endregion

        #region Resource Definition Friendly Name conversion

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource definition type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionTypeFriendlyNameApplicationDefinition(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionApplicationDefinition);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceDefinitionFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Application Definition File");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource definition type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionTypeFriendlyNameBindings(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionBindings);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceDefinitionFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Binding File");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource definition type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionTypeFriendlyNameSchema(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionSchema);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceDefinitionFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("XML Schema");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource definition type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionTypeFriendlyNameMap(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionMap);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceDefinitionFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("BizTalk Map");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource definition type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionTypeFriendlyNameReceivePipeline(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionReceivePipeline);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceDefinitionFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("BizTalk Receive Pipeline");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource definition type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionTypeFriendlyNameSendPipeline(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionSendPipeline);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceDefinitionFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("BizTalk Send Pipeline");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource definition type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionTypeFriendlyNameFile(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionFile);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceDefinitionFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("File Resource");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource definition type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionTypeFriendlyNameOrchestration(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionOrchestration);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceDefinitionFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("BizTalk Orchestration");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource definition type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionTypeFriendlyNameDefault(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = "This is some unknown text");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceDefinitionFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Resource Definition");
                });
        }

        #endregion

        #region Resource Friendly Name Tests

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameApplication(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceApplication);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("BizTalk Application");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameReceivePort(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceReceivePort);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Receive Port");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameReceiveLocation(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceReceiveLocation);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Receive Location");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameSendPort(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceSendPort);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Send Port");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameDistributionList(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDistributionList);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Send Port Group");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameFilterExpression(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceFilterExpression);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Subscription");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameService(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceServiceBinding);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Service (Orchestration)");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameDocumentSchema(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDocumentSchema);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Document Schema");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNamePropertySchema(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourcePropertySchema);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Property Schema");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameContextProperty(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceContextProperty);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Context Property");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameMessageType(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceMessageType);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Message Type");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNamePortType(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourcePortType);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Port Type");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameCorrelationType(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceCorrelationType);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Correlation Type");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameCorrelationSet(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceCorrelationDeclaration);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Correlation Set");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameMessage(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceMessageDeclaration);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Message");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameLogicalPort(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourcePortDeclaration);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Logical Port (Orchestration Port)");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameMap(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceMap);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("BizTalk Map");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameReceivePipeline(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceReceivePipeline);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Receive Pipeline");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameSendPipeline(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceSendPipeline);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Send Pipeline");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNamePipelineComponent(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourcePipelineComponent);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Pipeline Component");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameMetaModel(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceMetaModel);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Orchestration MetaModel (ODX)");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameModule(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceModule);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Module");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameServiceDeclaration(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceServiceDeclaration);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Service Declaration");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameMultipartMessageType(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceMultipartMessageType);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Multipart Message Type");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceFriendlyNameDefault(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = "some random text");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Resource");
                });
        }

        #endregion

        #region Resource Container Icon selection

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource container types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceContainerIconMsi(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceContainerMsi);

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceContainerIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("box-open");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource container types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceContainerIconCab(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceContainerCab);

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceContainerIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("file-archive");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource container types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceContainerIconAssembly(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceContainerAssembly);

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceContainerIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("-cubes");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource container types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceContainerIconDefault(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = "This is some unknown text");

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceContainerIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("clone");
                });
        }

        #endregion

        #region Resource Definition Icon selection

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionIconSchema(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionSchema);

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceDefinitionIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("-code");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionIconMap(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionMap);

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceDefinitionIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("-project-diagram");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionIconReceivePipeline(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionReceivePipeline);

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceDefinitionIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("-code-branch");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionIconSendPipeline(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionSendPipeline);

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceDefinitionIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("-code-branch");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionIconOrchestration(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionOrchestration);

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceDefinitionIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("-sitemap");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionIconFile(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionFile);

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceDefinitionIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("-file-check");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionIconBindings(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionBindings);

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceDefinitionIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("-file-code");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionIconApplicationDefinition(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDefinitionApplicationDefinition);

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceDefinitionIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("-file-code");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for formatting resource definition types.
        /// </summary>
        /// <param name="type">The resource container type.</param>
        /// <param name="iconSnippet">The HTML icon snippet.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceDefinitionIconDefault(string type, string iconSnippet, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = "some random text");

            "And a blank snippet"
                .x(() => iconSnippet.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => iconSnippet = ResourceFormatter.GetResourceDefinitionIconFromType(type)));

            "Then the method should NOT throw an exception and return a snippet"
                .x(() => {
                    e.Should().BeNull();
                    iconSnippet.Should().Contain("-file-exclamation");
                });
        }

        #endregion

        #region Resource Icon selection

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconApplication(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceApplication);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-cogs");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconReceivePort(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceReceivePort);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-arrow-circle-right");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconReceiveLocation(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceReceiveLocation);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-exchange-alt");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconSendPort(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceSendPort);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-arrow-circle-left");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconDistributionList(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDistributionList);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-object-group");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconFilterExpression(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceFilterExpression);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-filter");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconService(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceServiceBinding);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-sitemap");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconDocumentSchema(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceDocumentSchema);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-file");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconPropertySchema(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourcePropertySchema);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-file");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconContextProperty(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceContextProperty);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-flag");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconMessageType(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceMessageType);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-code");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconPortType(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourcePortType);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-sign-in");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconCorrelationType(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceCorrelationType);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-th-large");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconCorrelationSet(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceCorrelationDeclaration);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-th");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconMap(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceMap);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-project-diagram");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconMessage(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceMessageDeclaration);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-envelope");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconLogicalPort(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourcePortDeclaration);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-sign-out");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconOrchestration(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceServiceDeclaration);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-cog");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconMultipartMessageType(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceMultipartMessageType);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-vector-square");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconReceivePipeline(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceReceivePipeline);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-code-branch");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconSendPipeline(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourceSendPipeline);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-code-branch");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconPipelineComponent(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = ModelConstants.ResourcePipelineComponent);

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-expand");
                });
        }

        /// <summary>
        /// Scenario tests that the correct icon is returned for a resource.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ResourceIconDefault(string type, string friendlyName, Exception e)
        {
            "Given a resource definition type"
                .x(() => type = "some random text");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetResourceIconFromType(type)));

            "Then the method should NOT throw an exception and text is correct"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Contain("-expand");
                });
        }

        #endregion

        #region GetTargetResourceFriendlyName Tests

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameScript(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.scripts.powershell");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Deployment Script");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameXmlSchema(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.schemas.xml");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("XML Schema");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameResourceGroup(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.groups.azureresourcegroup");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Resource Group");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameRoutingProperties(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.configuration.routingproperties");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Routing Properties");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameServiceBus(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.messaging.azureservicebus");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Service Bus Namespace");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameSecurityRole(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.security.azurerole");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Security Role");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameApiManagement(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.services.azureapimanagement");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("API Management Service");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameAppServicePlan(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.services.azureappservice");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("App Service Plan");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameStorageAccount(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.services.azurestorage");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Storage Account");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameApplicationInsights(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.services.azureappinsights");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Application Insights");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameIntegrationAccount(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.stores.azureintegrationaccount");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Integration Account");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameAppConfiguration(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.stores.azureappconfiguration");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("App Configuration");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameKeyVault(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.stores.azurekeyvault");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Key Vault");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameAzureFunction(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.services.azurefunction");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Azure Function");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameLogicAppConsumption(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.workflows.azurelogicapp.consumption");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Consumption Logic App");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameLogicAppStandard(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.workflows.azurelogicapp.standard");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Standard Logic App");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameWebConnections(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.web.connections");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Web Connections");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameRoutingSlip(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.configuration.routingslip");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Routing Slip");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameConfigurationEntry(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.configuration.configurationentry");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Configuration Entry");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameServiceBusTopicSubscription(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.messaging.azureservicebustopicsubscription");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Service Bus Topic Subscription");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameServiceBusTopic(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.messaging.azureservicebustopic");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Service Bus Topic");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameConnectionGateway(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.web.connectiongateway");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be("Connection Gateway");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="friendlyName">The friendly name to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceFriendlyNameDefaultUnknown(string type, string friendlyName, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "unknown");

            "And a blank friendly name"
                .x(() => friendlyName.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => friendlyName = ResourceFormatter.GetTargetResourceFriendlyName(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    friendlyName.Should().Be(type);
                });
        }

        #endregion

        #region GetTargetResourceIconFromType Tests

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeScript(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.scripts.powershell");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-scroll");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeXmlSchema(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.schemas.xml");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fa fa-file-code");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeResourceGroup(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.groups.azureresourcegroup");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-cube");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeRoutingProperties(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.configuration.routingproperties");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-map-marker");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeServiceBus(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.messaging.azureservicebus");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-bus-alt");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeSecurityRole(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.security.azurerole");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-user-lock");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeApiManagement(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.services.azureapimanagement");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-cloud");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeAppServicePlan(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.services.azureappservice");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-server");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeStorageAccount(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.services.azurestorage");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("far fa-hdd");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeApplicationInsights(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.services.azureappinsights");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-lightbulb");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeIntegrationAccount(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.stores.azureintegrationaccount");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-th");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeAppConfiguration(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.stores.azureappconfiguration");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-cloud-upload-alt");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeKeyVault(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.stores.azurekeyvault");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-key");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeAzureFunction(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.services.azurefunction");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("far fa-bolt");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeLogicAppConsumption(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.workflows.azurelogicapp.consumption");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-project-diagram");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeLogicAppStandard(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.workflows.azurelogicapp.standard");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-project-diagram");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconWebConnections(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.web.connections");

            "And a blank friendly name"
                .x(() => icon.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-globe");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconRoutingSlip(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.configuration.routingslip");

            "And a blank friendly name"
                .x(() => icon.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-route");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconConfigurationEntry(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.configuration.configurationentry");

            "And a blank friendly name"
                .x(() => icon.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("far fa-bookmark");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconServiceBusTopicSubscription(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.messaging.azureservicebustopicsubscription");

            "And a blank friendly name"
                .x(() => icon.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-plus-square");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconServiceBusTopic(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.messaging.azureservicebustopic");

            "And a blank friendly name"
                .x(() => icon.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-tag");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconConnectionGateway(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "microsoft.web.connectiongateway");

            "And a blank friendly name"
                .x(() => icon.Should().BeNull());

            "When translating the friendly name type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-external-link-square");
                });
        }

        /// <summary>
        /// Scenario tests that the correct text is returned for formatting target resource types.
        /// </summary>
        /// <param name="type">The target resource type.</param>
        /// <param name="icon">The icon to generate.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void GetTargetResourceIconFromTypeDefaultUnknown(string type, string icon, Exception e)
        {
            "Given a target resource type"
                .x(() => type = "unknown");

            "And a blank icon"
                .x(() => icon.Should().BeNull());

            "When translating the icon from the type"
                .x(() => e = Record.Exception(() => icon = ResourceFormatter.GetTargetResourceIconFromType(type)));

            "Then the method should NOT throw an exception"
                .x(() => {
                    e.Should().BeNull();
                    icon.Should().Contain("fas fa-expand");
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
