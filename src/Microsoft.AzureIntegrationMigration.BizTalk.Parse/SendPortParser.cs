// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Filters;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Defines a class to parse send ports.
    /// </summary>
    public sealed class SendPortParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="SendPortFilterParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public SendPortParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(SendPortParser), model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Implements the internal parsing logic.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The <see cref="MigrationContext"/> being parsed.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "By design as the exception messages are collated in an error object.")]
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(SendPortParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(SendPortParser));

                foreach (var application in group.Applications)
                {
                    // Defensive check
                    if (application.Application.Bindings == null)
                    {
                        _logger.LogWarning(WarningMessages.BindingInfoNotFound, application.Application.Name);
                        continue;
                    }

                    _logger.LogDebug(TraceMessages.ParsingBizTalkSendPortFiltersInApplication, application.Application.Name);

                    var bindingFileDefinition = model.FindResourceDefinitionByKey(application.Application.Bindings.ResourceDefinitionKey, ModelConstants.ResourceDefinitionBindings);
                    var applicationResource = model.FindResourceByKey(application.Application?.ApplicationDefinition?.ResourceKey);

                    foreach (var sendPort in application.Application.Bindings.BindingInfo.SendPortCollection)
                    {
                        try
                        {
                            sendPort.ResourceKey = string.Concat(application.Application.Bindings.ResourceDefinitionKey, ":", sendPort.Name);

                            // Create the resource under the binding file.
                            var sendPortResource = new ResourceItem
                            {
                                Key = sendPort.ResourceKey,
                                Name = sendPort.Name,
                                Description = sendPort.Description,
                                Type = ModelConstants.ResourceSendPort,
                                ParentRefId = bindingFileDefinition.RefId,
                                Rating = ConversionRating.NotSupported
                            };

                            sendPort.Resource = sendPortResource; // Maintain pointer to the resource.
                            sendPortResource.SourceObject = sendPort; // Maintain backward pointer.

                            // Static or dynamic port?
                            if (sendPort.IsStatic)
                            {
                                // Static port
                                sendPortResource.Properties.Add(ResourceItemProperties.SendPortTypeProperty, ResourceItemProperties.StaticSendPortType);
                                sendPortResource.Properties.Add(ResourceItemProperties.SendPortPrimaryTransportTypeProperty, sendPort.PrimaryTransport.TransportType?.Name);
                                sendPortResource.Properties.Add(ResourceItemProperties.SendPortPrimaryAddressProperty, sendPort.PrimaryTransport.Address);
                                if (sendPort.SecondaryTransport != null && !string.IsNullOrEmpty(sendPort.SecondaryTransport.Address))
                                {
                                    sendPortResource.Properties.Add(ResourceItemProperties.SendPortSecondaryTransportTypeProperty, sendPort.SecondaryTransport.TransportType?.Name);
                                    sendPortResource.Properties.Add(ResourceItemProperties.SendPortSecondaryAddressProperty, sendPort.SecondaryTransport.Address);
                                }
                            }
                            else
                            {
                                // Dynamic port
                                sendPortResource.Properties.Add(ResourceItemProperties.SendPortTypeProperty, ResourceItemProperties.DynamicSendPortType);
                            }

                            sendPortResource.Properties.Add(ResourceItemProperties.PortDirectionProperty, sendPort.IsTwoWay ? ResourceItemProperties.PortDirectionTwoWay : ResourceItemProperties.PortDirectionOneWay);

                            bindingFileDefinition.Resources.Add(sendPortResource);

                            if (applicationResource != null)
                            {
                                applicationResource.AddRelationship(new ResourceRelationship(sendPortResource.RefId, ResourceRelationshipType.Child));
                                sendPortResource.AddRelationship(new ResourceRelationship(applicationResource.RefId, ResourceRelationshipType.Parent));
                            }
                            else
                            {
                                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceApplication, application.Application?.ApplicationDefinition?.ResourceKey);
                                _logger.LogError(error);
                                context.Errors.Add(new ErrorMessage(error));
                            }

                            if (!string.IsNullOrEmpty(sendPort.Filter))
                            {
                                sendPort.FilterExpression = Filter.FromXml(sendPort.Filter);
                                sendPort.FilterExpression.ResourceKey = string.Concat(sendPort.Name, ":", "filter");

                                var filterResource = new ResourceItem
                                {                                    
                                    Key = sendPort.FilterExpression.ResourceKey,
                                    Name = sendPortResource.Name + " filter expression",
                                    Type = ModelConstants.ResourceFilterExpression,
                                    ParentRefId = sendPortResource.RefId,
                                    Rating = ConversionRating.NotSupported
                                };

                                sendPort.FilterExpression.Resource = filterResource; // Maintain pointer to the resource.
                                filterResource.SourceObject = sendPort.FilterExpression; // Maintain backward pointer.
                                sendPortResource.Resources.Add(filterResource);

                                _logger.LogDebug(TraceMessages.ParsedBizTalkSendPortFilterExpression, sendPort.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorParsingSendPort, sendPort.Name, application.Application.Name, ex.Message);
                            context.Errors.Add(new ErrorMessage(message));
                            _logger.LogError(message);
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(SendPortParser));
            }
        }
    }
}
