#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

#endregion

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse.Tests
{
    /// <summary>
    /// Tests for parsing the distribution lists.
    /// </summary>
    public class DistributionListParserFeature
    {
        /// <summary>
        /// Defines a mocked logger.
        /// </summary>
        private Mock<ILogger> _mockLogger;

        #region Before Each Scenario

        /// <summary>
        /// Sets up state before each scenario.
        /// </summary>
        [Background]
        public void Setup()
        {
            "Given a new mock logger"
                .x(() => _mockLogger = new Mock<ILogger>());
        }

        #endregion

        #region Constructor Tests

        /// <summary>
        /// Scenario tests that the object construction throws an exception when null model is passed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="e">The runner exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullModel(IBizTalkParser parser, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a parser"
                .x(() => parser.Should().BeNull());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And null model"
                .x(() => model.Should().BeNull());

            "And a context"
                .x(() => context = new MigrationContext());

            "When constructing with null model"
                .x(() => e = Record.Exception(() => new DistributionListParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when null context is passed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="e">The runner exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullContext(IBizTalkParser parser, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a parser"
                .x(() => parser.Should().BeNull());


            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a null context"
                .x(() => context.Should().BeNull());

            "When constructing with null context"
                .x(() => e = Record.Exception(() => new DistributionListParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("context"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when null logger is passed.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="e">The runner exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullLogger(IBizTalkParser parser, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a parser"
                .x(() => parser.Should().BeNull());

            "And null logger"
                .x(() => logger.Should().BeNull());

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = new MigrationContext());

            "When constructing with null logger"
                .x(() => e = Record.Exception(() => new DistributionListParser(model, context, logger)));

            "Then the parser constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction succeeds.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="e">The runner exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithSuccess(IBizTalkParser parser, ILogger logger, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a parser"
                .x(() => parser.Should().BeNull());

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = new MigrationContext());

            "When constructing"
                .x(() => e = Record.Exception(() => parser = new DistributionListParser(model, context, logger)));

            "Then the parser constructor should succeed"
                .x(() =>
                {
                    e.Should().BeNull();
                    parser.Should().NotBeNull();
                });
        }

        #endregion

        #region Parse Tests

        /// <summary>
        /// Scenario tests parser skips if the source model is missing.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseIsSkippedIfModelIsMissing(DistributionListParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, Exception e)
        {
            "Given a model"
                .x(() =>
                {
                    model = new AzureIntegrationServicesModel();
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new DistributionListParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());
        }

        /// <summary>
        /// Scenario tests the happy path when there is a distribution list and filter.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulSingleDistributionListAndFilter(DistributionListParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with a filter on the distribution list"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    var distributionList = new List<DistributionList>
                    {
                        new DistributionList()
                        {
                            ResourceKey = "distributionlistresourcekey",
                            Name = "Distribution List 1",
                            Description = "Distribution List Description 1.",
                            Filter = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<Filter xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n  <Group>\n    <Statement Property=\"AzureBlobStorage.BlobName\" Operator=\"0\" Value=\"Blobby\" />\n  </Group>\n</Filter>"
                        }
                    };

                    bindingInfo.DistributionListCollection = distributionList.ToArray();

                    var application = new ParsedBizTalkApplication();
                    application.Application.Bindings = new BindingFile(bindingResourceContainerKey, bindingResourceDefinitionKey)
                    {
                        BindingInfo = bindingInfo
                    };

                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(application);

                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.MigrationSourceModel = group;

                    var bindingResourceContainer = new ResourceContainer
                    {
                        Key = bindingResourceContainerKey
                    };

                    var bindingResourceDefinition = new ResourceDefinition
                    {
                        Key = bindingResourceDefinitionKey,
                        Type = ModelConstants.ResourceDefinitionBindings
                    };

                    bindingResourceContainer.ResourceDefinitions.Add(bindingResourceDefinition);
                    model.MigrationSource.ResourceContainers.Add(bindingResourceContainer);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new DistributionListParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the filter group should be correctly parsed."
                .x(() =>
                {
                    var distributionList = group.Applications[0].Application.Bindings.BindingInfo.DistributionListCollection[0];
                    distributionList.FilterExpression.Should().NotBeNull();
                    distributionList.FilterExpression.Group.Should().NotBeNull();
                    distributionList.FilterExpression.Group.Length.Should().Be(1);
                    distributionList.FilterExpression.Group[0].Statement.Length.Should().Be(1);
                    distributionList.FilterExpression.Group[0].Statement[0].Property.Should().Be("AzureBlobStorage.BlobName");
                });

            "And the resources should be set."
                .x(() =>
                {
                    var appModel = (AzureIntegrationServicesModel)model;

                    // Check the distribution list source has been created.
                    appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().NotBeNullOrEmpty();
                    appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().HaveCount(1);
                    var distributionListResource = appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0];

                    // Get the distribution list from the bindings.
                    var distributionList = group.Applications[0].Application.Bindings.BindingInfo.DistributionListCollection[0];

                    // Validate the distribution list resource.
                    distributionListResource.Name.Should().Be(distributionList.Name);
                    distributionListResource.Description.Should().Be(distributionList.Description);
                    distributionListResource.Type.Should().Be(ModelConstants.ResourceDistributionList);
                    distributionListResource.Key.Should().Be(distributionList.ResourceKey);

                    distributionListResource.Resources.Should().NotBeNullOrEmpty();
                    distributionListResource.Resources.Should().HaveCount(1);

                    distributionList.Resource.Should().Be(distributionListResource); // The pointer to the resource should be set.
                    distributionListResource.ParentRefId.Should().Be(appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].RefId); // The parent ref ID should be set.
                    distributionListResource.SourceObject.Should().Be(distributionList); // The resource should have a pointer to the source object.

                    // Validate the filter resource.
                    var filterResource = distributionListResource.Resources[0];
                    filterResource.Name.Should().StartWith(distributionListResource.Name);
                    var expectedFilterKey = string.Concat(distributionListResource.Name, ":", "filter");
                    filterResource.Key.Should().Be(expectedFilterKey);
                    filterResource.Name.Should().EndWith("filter expression");
                    filterResource.Type.Should().Be(ModelConstants.ResourceFilterExpression);

                    distributionList.FilterExpression.Resource.Should().Be(filterResource); // The pointer to the resource should be set.
                    filterResource.ParentRefId.Should().Be(distributionListResource.RefId); // The parent ref ID should be set.
                    filterResource.SourceObject.Should().Be(distributionList.FilterExpression); // The resource should have a pointer to the source object.
                });
        }

        /// <summary>
        /// Scenario tests the happy path there is a distribution list but no filter.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseSuccessfulSingleDistributionListAndNoFilter(DistributionListParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with no filter on the distribution list"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    var distributionList = new List<DistributionList>
                    {
                        new DistributionList()
                        {
                            ResourceKey = "distributionlistresourcekey",
                            Name = "Distribution List 1",
                            Description = "Distribution List Description 1."                            
                        }
                    };

                    bindingInfo.DistributionListCollection = distributionList.ToArray();

                    var application = new ParsedBizTalkApplication();
                    application.Application.Bindings = new BindingFile(bindingResourceContainerKey, bindingResourceDefinitionKey)
                    {
                        BindingInfo = bindingInfo
                    };

                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(application);

                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.MigrationSourceModel = group;

                    var bindingResourceContainer = new ResourceContainer
                    {
                        Key = bindingResourceContainerKey
                    };

                    var bindingResourceDefinition = new ResourceDefinition
                    {
                        Key = bindingResourceDefinitionKey,
                        Type = ModelConstants.ResourceDefinitionBindings
                    };

                    bindingResourceContainer.ResourceDefinitions.Add(bindingResourceDefinition);
                    model.MigrationSource.ResourceContainers.Add(bindingResourceContainer);

                    application.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new DistributionListParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the filter group should be null - nothing created."
                .x(() =>
                {
                    var item = group.Applications[0].Application.Bindings.BindingInfo.DistributionListCollection[0];
                    item.FilterExpression.Should().BeNull();
                    context.Errors.Count.Should().Be(0);
                });

            "And the resources should be set."
                .x(() =>
                {
                    var appModel = (AzureIntegrationServicesModel)model;

                    // Check the distribution list source has been created.
                    appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().NotBeNullOrEmpty();
                    appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Should().HaveCount(2);
                    appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[0].ResourceRelationships.Should().HaveCount(1);
                    var distributionListSource = appModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources[1];

                    // Get the distribution list from the bindings.
                    var distributionList = group.Applications[0].Application.Bindings.BindingInfo.DistributionListCollection[0];

                    // Validate the distribution list source.
                    distributionListSource.Name.Should().Be(distributionList.Name);
                    distributionListSource.Description.Should().Be(distributionList.Description);
                    distributionListSource.Type.Should().Be(ModelConstants.ResourceDistributionList);
                    distributionListSource.Key.Should().Be(distributionList.ResourceKey);

                    distributionListSource.Resources.Should().BeNullOrEmpty();                    
                });
        }

        /// <summary>
        /// Scenario tests when the filter contains invalid XML.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureInvalidXmlForFilter(DistributionListParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with invalid XML in the distribution list filter"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    var distributionList = new List<DistributionList>
                    {
                        new DistributionList()
                        {
                            ResourceKey = "distributionlistresourcekey",
                            Name = "Distribution List 1",
                            Description = "Distribution List Description 1.",
                            Filter = "<invalid-xml>"
                        }
                    };

                    bindingInfo.DistributionListCollection = distributionList.ToArray();

                    var application = new ParsedBizTalkApplication();
                    application.Application.Bindings = new BindingFile(bindingResourceContainerKey, bindingResourceDefinitionKey)
                    {
                        BindingInfo = bindingInfo
                    };

                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(application);

                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.MigrationSourceModel = group;

                    var bindingResourceContainer = new ResourceContainer
                    {
                        Key = bindingResourceContainerKey
                    };

                    var bindingResourceDefinition = new ResourceDefinition
                    {
                        Key = bindingResourceDefinitionKey,
                        Type = ModelConstants.ResourceDefinitionBindings
                    };

                    bindingResourceContainer.ResourceDefinitions.Add(bindingResourceDefinition);
                    model.MigrationSource.ResourceContainers.Add(bindingResourceContainer);

                    application.Application.ApplicationDefinition = new ApplicationDefinitionFile { ResourceKey = "ResourceKey" };
                    model.MigrationSource.ResourceContainers.Add(new ResourceContainer());
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions.Add(new ResourceDefinition());
                    model.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Resources.Add(new ResourceItem { Key = "ResourceKey" });
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new DistributionListParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the parser should have recorded an error when processing the filter."
                .x(() =>
                {
                    var item = group.Applications[0].Application.Bindings.BindingInfo.DistributionListCollection[0];
                    item.FilterExpression.Should().BeNull();
                    context.Errors.Count.Should().Be(1);
                });
        }

        /// <summary>
        /// Scenario tests the happy path when there is a distribution list and filter.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="model">The model.</param>
        /// <param name="group">The group.</param>
        /// <param name="e">The exception that is generated, if appropriate.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ParseFailureMissingBindings(DistributionListParser parser, ILogger logger, MigrationContext context, AzureIntegrationServicesModel model, ParsedBizTalkApplicationGroup group, Exception e)
        {
            var bindingResourceContainerKey = "bindingResourceContainerKey";
            var bindingResourceDefinitionKey = "bindingResourceDefinitionKey";

            "Given a model with a filter on the distribution list"
                .x(() =>
                {
                    var bindingInfo = new BindingInfo();
                    var distributionList = new List<DistributionList>
                    {
                        new DistributionList()
                        {
                            ResourceKey = "distributionlistresourcekey",
                            Name = "Distribution List 1",
                            Description = "Distribution List Description 1.",
                            Filter = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<Filter xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n  <Group>\n    <Statement Property=\"AzureBlobStorage.BlobName\" Operator=\"0\" Value=\"Blobby\" />\n  </Group>\n</Filter>"
                        }
                    };

                    bindingInfo.DistributionListCollection = distributionList.ToArray();

                    var application = new ParsedBizTalkApplication();

                    group = new ParsedBizTalkApplicationGroup();
                    group.Applications.Add(application);

                    model = new AzureIntegrationServicesModel();
                    model.MigrationSource.MigrationSourceModel = group;

                    var bindingResourceContainer = new ResourceContainer
                    {
                        Key = bindingResourceContainerKey
                    };

                    var bindingResourceDefinition = new ResourceDefinition
                    {
                        Key = bindingResourceDefinitionKey,
                        Type = ModelConstants.ResourceDefinitionBindings
                    };

                    bindingResourceContainer.ResourceDefinitions.Add(bindingResourceDefinition);
                    model.MigrationSource.ResourceContainers.Add(bindingResourceContainer);
                });

            "And a logger"
                .x(() => logger = _mockLogger.Object);

            "And a context"
                .x(() => context = new MigrationContext());

            "And a parser"
                .x(() => parser = new DistributionListParser(model, context, logger));

            "When parsing"
                .x(() => e = Record.Exception(() => parser.Parse()));

            "Then the code should not throw an exception"
                .x(() => e.Should().BeNull());

            "And the model should be parsed with a warning."
                .x(() =>
                {
                    // There should be no exception logged - this is a handled scenario.
                    context.Errors.Count.Should().Be(0);

                    // The application definition cannot be read and so the name should be default
                    var group = (ParsedBizTalkApplicationGroup)model.MigrationSource.MigrationSourceModel;
                    group.Applications[0].Application.Name.Should().Be("(Unknown)");

                    // An error should be logged
                    var invocation = _mockLogger.Invocations.Where(i => i.Arguments[0].ToString() == "Warning").FirstOrDefault();
                    invocation.Should().NotBeNull();
                    invocation.Arguments[2].ToString().Should().Contain("Unable to find the binding info resource");
                });
        }

        #endregion

        /// <summary>
        /// Creates a group with the fields for send port parsing.
        /// </summary>
        /// <returns>A populated <see cref="ParsedBizTalkApplicationGroup"/>.</returns>
        private static ParsedBizTalkApplicationGroup CreateGroup()
        {
            var group = new ParsedBizTalkApplicationGroup();
            var application = new ParsedBizTalkApplication();
            var bindingInfo = new BindingInfo();
            var distributionLists = new List<DistributionList>
            {
                new DistributionList() { Name = "Send Port Group 1", Filter = "" }
            };
            bindingInfo.DistributionListCollection = distributionLists.ToArray();
            application.Application.Bindings = new BindingFile("TestContainer", "BindingInfo")
            {
                BindingInfo = bindingInfo
            };
            group.Applications.Add(application);
            return group;
        }
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
