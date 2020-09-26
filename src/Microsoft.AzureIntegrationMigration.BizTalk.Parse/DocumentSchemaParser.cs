using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Parses document schemas and creates source model resources for them.
    /// </summary>
    public sealed class DocumentSchemaParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="DocumentSchemaParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public DocumentSchemaParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(DocumentSchemaParser), model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Implements the internal logic of the parser.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The migration context.</param>
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            // Get the applications from the source model.
            var parsedApplications = model.GetSourceModel<ParsedBizTalkApplicationGroup>()?.Applications;

            if (parsedApplications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(DocumentSchemaParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(DocumentSchemaParser));

                // Iterate through the applications and then through the parsed schemas.
                foreach (var application in parsedApplications)
                {
                    var applicationResource = model.FindResourceByKey(application.Application?.ApplicationDefinition?.ResourceKey);

                    foreach (var schema in application.Application.Schemas.Where(s => s.SchemaType == BizTalkSchemaType.Document))
                    {
                        _logger.LogDebug(TraceMessages.ParsingBizTalkDocumentSchema, schema.Name);

                        // Find resource definition
                        var resourceDefinition = model.FindResourceDefinitionByKey(schema.ResourceDefinitionKey, ModelConstants.ResourceDefinitionSchema);
                        if (resourceDefinition != null)
                        {
                            var schemaResource = new ResourceItem()
                            {
                                Name = schema.Name,
                                Description = schema.XmlNamespace,
                                Type = ModelConstants.ResourceDocumentSchema,
                                Key = schema.ResourceKey,
                                ParentRefId = resourceDefinition.RefId,
                                Rating = ConversionRating.NotSupported
                            };

                            schema.Resource = schemaResource; // Maintain pointer to the resource.
                            schemaResource.SourceObject = schema; // Maintain backward pointer.
                            schemaResource.Properties.Add(ResourceItemProperties.XmlNamespaceProperty, schema.XmlNamespace);
                            schemaResource.Properties.Add(ResourceItemProperties.DotnetTypeNameProperty, schema.FullName);
                            schemaResource.Properties.Add(ResourceItemProperties.NumberOfRootNodesProperty, schema.MessageDefinitions.Count.ToString(CultureInfo.CurrentCulture));
                            schemaResource.Properties.Add(ResourceItemProperties.IsEnvelopeProperty, schema.IsEnvelope.ToString());

                            if (schema.IsEnvelope)
                            {
                                schemaResource.Properties.Add(ResourceItemProperties.BodyXPathProperty, schema.BodyXPath);
                            }

                            // Add the message definitions.
                            foreach (var messageDefinition in schema.MessageDefinitions)
                            {
                                _logger.LogDebug(TraceMessages.ParsingMessageTypeDefinition, messageDefinition.MessageType);

                                var messageDefResource = new ResourceItem()
                                {
                                    Name = messageDefinition.RootElementName,
                                    Description = messageDefinition.MessageType,
                                    Type = ModelConstants.ResourceMessageType,
                                    Key = messageDefinition.ResourceKey,
                                    ParentRefId = schemaResource.RefId,
                                    Rating = ConversionRating.NotSupported
                                };

                                messageDefinition.Resource = messageDefResource; // Maintain pointer to the resource.
                                messageDefResource.SourceObject = messageDefinition; // Maintain backward pointer.
                                messageDefResource.Properties.Add(ResourceItemProperties.RootNodeNameProperty, messageDefinition.RootElementName);
                                messageDefResource.Properties.Add(ResourceItemProperties.XmlNamespaceProperty, messageDefinition.XmlNamespace);

                                schemaResource.Resources.Add(messageDefResource);
                            }

                            resourceDefinition.Resources.Add(schemaResource);

                            if (applicationResource != null)
                            {
                                applicationResource.AddRelationship(new ResourceRelationship(schemaResource.RefId, ResourceRelationshipType.Child));
                                schemaResource.AddRelationship(new ResourceRelationship(applicationResource.RefId, ResourceRelationshipType.Parent));
                            }
                            else
                            {
                                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceApplication, application.Application?.ApplicationDefinition?.ResourceKey);
                                _logger.LogError(error);
                                context.Errors.Add(new ErrorMessage(error));
                            }
                        }
                        else
                        {
                            _logger.LogError(ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceDefinitionSchema, schema.ResourceDefinitionKey);
                            context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceDefinitionSchema, schema.ResourceDefinitionKey)));
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(DocumentSchemaParser));
            }
        }
    }
}
