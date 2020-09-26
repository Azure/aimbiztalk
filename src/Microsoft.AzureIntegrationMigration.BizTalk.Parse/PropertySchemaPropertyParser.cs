//-----------------------------------------------------------------------
// <copyright file="PropertySchemaPropertyParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Parses the properties from property schemas.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Parses the properties from property schemas.
    /// </summary>
    public sealed class PropertySchemaPropertyParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of this parser.
        /// </summary>
        public const string ParserName = ModelConstants.ApplicationPrefix + "propertyschemapropertyparser";

        /// <summary>
        /// Creates a new instance of the <see cref="PropertySchemaPropertyParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public PropertySchemaPropertyParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(ParserName, model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Perfroms the internal parsing logic.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The migration context.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "By design as the exception messages are collated in an error object.")]
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            // Null check
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(PropertySchemaPropertyParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(PropertySchemaPropertyParser));

                foreach (var application in group.Applications)
                {
                    _logger.LogDebug(TraceMessages.ParsingBizTalkSchemasInApplication, application.Application.Name);

                    foreach (var schema in application.Application.Schemas.Where(s => s.SchemaType == BizTalkSchemaType.Property))
                    {
                        try
                        {
                            // Find resource definition
                            var resourceDefinition = model.FindResourceDefinitionByKey(schema.ResourceDefinitionKey, ModelConstants.ResourceDefinitionSchema);

                            // If the schema is empty then error and continue.
                            if (resourceDefinition == null || string.IsNullOrEmpty((string)resourceDefinition.ResourceContent))
                            {
                                var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorSchemaXmlEmpty, schema.Name, application.Application.Name);
                                context.Errors.Add(new ErrorMessage(message));
                                _logger.LogError(message);
                                continue;
                            }

                            _logger.LogDebug(TraceMessages.ParsingBizTalkPropertySchema, schema.FullName);

                            // Create the resource under the property schema's resource definition.
                            var propertySchemaResource = new ResourceItem
                            {
                                Key = schema.ResourceKey,
                                Name = schema.Name,
                                Description = schema.FullName,
                                Type = ModelConstants.ResourcePropertySchema,
                                ParentRefId = resourceDefinition.RefId,
                                Rating = ConversionRating.NotSupported
                            };

                            schema.Resource = propertySchemaResource; // Maintain pointer to the resource.
                            propertySchemaResource.SourceObject = schema; // Maintain backward pointer.
                            propertySchemaResource.Properties.Add(ResourceItemProperties.DotnetTypeNameProperty, schema.FullName);
                            propertySchemaResource.Properties.Add(ResourceItemProperties.ModuleNameProperty, schema.ModuleName);
                            resourceDefinition.Resources.Add(propertySchemaResource);

                            var xml = new XmlDocument() { XmlResolver = null };
                            using (var sReader = new StringReader((string)resourceDefinition.ResourceContent))
                            {
                                using (var reader = XmlReader.Create(sReader, new XmlReaderSettings() { XmlResolver = null }))
                                {
                                    xml.Load(reader);
                                }
                            }

                            var nodes = xml.SelectNodes(@"/*[local-name()='schema']/*[local-name()='element']");

                            foreach (XmlNode propertyNode in nodes)
                            {
                                var property = new ContextProperty()
                                {
                                    PropertyName = GetAttributeValue(propertyNode, "name"),
                                    Namespace = schema.Namespace,
                                    DataType = GetAttributeValue(propertyNode, "type")
                                };

                                property.FullyQualifiedName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", property.Namespace, property.PropertyName);
                                property.ResourceKey = schema.ResourceKey + ":" + property.PropertyName;
                                schema.ContextProperties.Add(property);

                                // Create the resource under the property schema's resource.
                                var contextPropertyResource = new ResourceItem
                                {
                                    Key = property.ResourceKey,
                                    Name = property.PropertyName,
                                    Description = property.FullyQualifiedName,
                                    Type = ModelConstants.ResourceContextProperty,
                                    ParentRefId = propertySchemaResource.RefId,
                                    Rating = ConversionRating.NotSupported
                                };

                                property.Resource = contextPropertyResource; // Maintain pointer to the resource.
                                contextPropertyResource.SourceObject = property; // Maintain backward pointer.
                                contextPropertyResource.Properties.Add(ResourceItemProperties.DotnetTypeNameProperty, property.FullyQualifiedName);
                                contextPropertyResource.Properties.Add(ResourceItemProperties.ContextPropertyDataTypeProperty, property.DataType);
                                contextPropertyResource.Properties.Add(ResourceItemProperties.XmlNamespaceProperty, property.Namespace);
                                propertySchemaResource.Resources.Add(contextPropertyResource);

                                _logger.LogDebug(TraceMessages.ParsedTheBizTalkContextPropertyInSchema, property.FullyQualifiedName, schema.FullName);
                            }
                        }
                        catch (Exception ex)
                        {
                            var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorParsingSchemaXml, schema.Name, application.Application.Name, ex.Message);
                            context.Errors.Add(new ErrorMessage(message));
                            _logger.LogError(message);
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(PropertySchemaPropertyParser));
            }
        }
    }
}
