using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
    /// Represents a class used to parse receive ports.
    /// </summary>
    public sealed class ReceivePortParser : PipelineDataParser
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="ReceivePortParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public ReceivePortParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(ReceivePortParser), model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Formats the error message.
        /// </summary>
        /// <param name="direction">The direction of the pipeline.</param>
        /// <param name="resourceIdentifier">The identified of the resource, i.e. port name, receive location name etc.</param>
        /// <param name="applicationName">The name of the application.</param>
        /// <param name="message">The exception message.</param>
        /// <returns>The formated error message</returns>
        protected override string FormatErrorMessage(PipelineDirection direction, string resourceIdentifier, string applicationName, string message)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorParsingReceiveLocationPipelineData, resourceIdentifier, direction, applicationName, message);
        }

        /// <summary>
        /// Parses the receive location data stored as XML in the bindings.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The migration context.</param>        
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(ReceivePortParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(ReceivePortParser));

                foreach (var application in group.Applications)
                {
                    // Defensive check
                    if (application.Application.Bindings == null)
                    {
                        _logger.LogWarning(WarningMessages.BindingInfoNotFound, application.Application.Name);
                        continue;
                    }

                    if (application.Application.Bindings.BindingInfo.ReceivePortCollection != null)
                    {
                        _logger.LogDebug(TraceMessages.ParsingBizTalkReceivePortCollectionInApplication, application.Application.Name);

                        var bindingFileDefinition = model.FindResourceDefinitionByKey(application.Application.Bindings.ResourceDefinitionKey, ModelConstants.ResourceDefinitionBindings);
                        var applicationResource = model.FindResourceByKey(application.Application?.ApplicationDefinition?.ResourceKey);

                        foreach (var receivePort in application.Application.Bindings.BindingInfo.ReceivePortCollection)
                        {
                            _logger.LogDebug(TraceMessages.ParsingBizTalkReceivePort, receivePort.Name);

                            receivePort.ResourceKey = string.Concat(application.Application.Bindings.ResourceDefinitionKey, ":", receivePort.Name);

                            // Create the resource under the binding file.
                            var receivePortResource = new ResourceItem
                            {
                                Key = receivePort.ResourceKey,
                                Name = receivePort.Name,
                                Description = receivePort.Description,
                                Type = ModelConstants.ResourceReceivePort,
                                ParentRefId = bindingFileDefinition.RefId,
                                Rating = ConversionRating.NotSupported
                            };

                            receivePort.Resource = receivePortResource; // Maintain pointer to the resource.
                            receivePortResource.SourceObject = receivePort; // Maintain backward pointer.
                            bindingFileDefinition.Resources.Add(receivePortResource);
                            receivePortResource.Properties.Add(ResourceItemProperties.PortDirectionProperty, receivePort.IsTwoWay ? ResourceItemProperties.PortDirectionTwoWay : ResourceItemProperties.PortDirectionOneWay);

                            if (applicationResource != null)
                            {
                                applicationResource.AddRelationship(new ResourceRelationship(receivePortResource.RefId, ResourceRelationshipType.Child));
                                receivePortResource.AddRelationship(new ResourceRelationship(applicationResource.RefId, ResourceRelationshipType.Parent));
                            }
                            else
                            {
                                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceApplication, application.Application?.ApplicationDefinition?.ResourceKey);
                                _logger.LogError(error);
                                context.Errors.Add(new ErrorMessage(error));
                            }

                            if (receivePort.ReceiveLocations != null)
                            {
                                foreach (var receiveLocation in receivePort.ReceiveLocations)
                                {
                                    _logger.LogDebug(TraceMessages.ParsingBizTalkReceiveLocationPipelineData, receiveLocation.Name);

                                    // Extract the send pipeline custom config.
                                    receiveLocation.SendPipelineCustomConfiguration = ParsePipelineData(application, receiveLocation.Name, receiveLocation.SendPipelineData, PipelineDirection.Send, context.Errors);

                                    // Extract the receive pipeline custom config.
                                    receiveLocation.ReceivePipelineCustomConfiguration = ParsePipelineData(application, receiveLocation.Name, receiveLocation.ReceivePipelineData, PipelineDirection.Receive, context.Errors);

                                    receiveLocation.ResourceKey = string.Concat(receivePort.Name, ":", receiveLocation.Name);

                                    // Create the resource item for the receive location.
                                    var receiveLocationResource = new ResourceItem
                                    {
                                        Key = receiveLocation.ResourceKey,
                                        Name = receiveLocation.Name,
                                        Description = receiveLocation.Description,
                                        Type = ModelConstants.ResourceReceiveLocation,
                                        ParentRefId = receivePortResource.RefId,
                                        Rating = ConversionRating.NotSupported
                                    };

                                    receiveLocation.Resource = receiveLocationResource; // Maintain pointer to the resource.
                                    receiveLocationResource.SourceObject = receiveLocation; // Maintain backward pointer.
                                    receiveLocationResource.Properties.Add(ResourceItemProperties.PortTransportTypeProperty, receiveLocation.ReceiveLocationTransportType.Name);
                                    receiveLocationResource.Properties.Add(ResourceItemProperties.ReceiveLocationAddressProperty, receiveLocation.Address);

                                    receivePortResource.Resources.Add(receiveLocationResource);
                                }
                            }
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(ReceivePortParser));
            }
        }
    }
}
