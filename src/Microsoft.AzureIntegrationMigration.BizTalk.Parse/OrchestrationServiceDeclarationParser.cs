using System;
using System.Globalization;
using System.Linq;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Parses the service declarations for orchestrations.
    /// </summary>
    public sealed class OrchestrationServiceDeclarationParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="OrchestrationServiceDeclarationParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public OrchestrationServiceDeclarationParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(OrchestrationServiceDeclarationParser), model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Implements the internal parsing logic.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The migration context.</param>
        protected override void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(OrchestrationServiceDeclarationParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(OrchestrationServiceDeclarationParser));

                foreach (var application in group.Applications)
                {
                    // Loop through all of the orchestrations.
                    foreach (var orchestration in application.Application.Orchestrations)
                    {
                        // Find the module for the orchestration.
                        var moduleResource = orchestration.Model?.Resource?.FindResourcesByType(ModelConstants.ResourceModule).SingleOrDefault();
                        if (moduleResource != null)
                        {
                            // Find service declaration
                            var serviceDeclaration = orchestration.FindServiceDeclaration();
                            if (serviceDeclaration != null)
                            {
                                var resourceName = serviceDeclaration.FindPropertyValue(MetaModelConstants.PropertyKeyName);
                                var resourceKey = string.Concat(moduleResource.Key, ":", resourceName);

                                var serviceDeclarationResource = new ResourceItem
                                {
                                    Name = resourceName,
                                    Key = resourceKey,
                                    Type = ModelConstants.ResourceServiceDeclaration,
                                    ParentRefId = moduleResource.RefId,
                                    Rating = ConversionRating.NotSupported
                                };

                                serviceDeclaration.Resource = serviceDeclarationResource; // Maintain pointer to resource.
                                serviceDeclarationResource.SourceObject = serviceDeclaration; // Maintain backward pointer.
                                moduleResource.Resources.Add(serviceDeclarationResource);

                                _logger.LogTrace(TraceMessages.ResourceCreated, nameof(OrchestrationServiceDeclarationParser), serviceDeclarationResource.Key, serviceDeclarationResource.Name, serviceDeclarationResource.Type, moduleResource.Key);

                                ParseMessageDeclarations(orchestration, serviceDeclarationResource);
                                ParseCorrelationDeclarations(orchestration, serviceDeclarationResource);
                                ParsePortDeclarations(orchestration, serviceDeclarationResource);
                            }
                            else
                            {
                                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindServiceDeclarationInOrchestrationModel, orchestration.ResourceContainerKey, orchestration.FullName);
                                _logger.LogError(error);
                                context.Errors.Add(new ErrorMessage(error));
                            };
                        }
                        else
                        {
                            var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResource, ModelConstants.ResourceModule, orchestration.ResourceDefinitionKey);
                            _logger.LogError(error);
                            context.Errors.Add(new ErrorMessage(error));
                        }
                    }
                }
                _logger.LogDebug(TraceMessages.CompletedParser, nameof(OrchestrationServiceDeclarationParser));
            }
        }
    
        /// <summary>
        /// Parse the message declarations.
        /// </summary>
        /// <param name="orchestration">The orchestration containing the message declarations.</param>
        /// <param name="serviceDeclarationResource">The service declaration to add the parsed resources to.</param>
        private void ParseMessageDeclarations(Orchestration orchestration, ResourceItem serviceDeclarationResource)
        {
            foreach (var messageDeclaration in orchestration.FindMessageDeclarations() ?? Array.Empty<Element>())
            {
                var resourceName = messageDeclaration.FindPropertyValue(MetaModelConstants.PropertyKeyName);
                var resourceKey = string.Concat(serviceDeclarationResource.Key, ":", resourceName);

                var messageDeclarationResource = new ResourceItem
                {
                    Name = resourceName,
                    Key = resourceKey,                    
                    Type = ModelConstants.ResourceMessageDeclaration,
                    ParentRefId = serviceDeclarationResource.RefId,
                    Rating = ConversionRating.NotSupported
                };

                messageDeclaration.Resource = messageDeclarationResource; // Maintain pointer to resource.
                messageDeclarationResource.SourceObject = messageDeclaration; // Maintain backward pointer.
                serviceDeclarationResource.Resources.Add(messageDeclarationResource);

                _logger.LogTrace(TraceMessages.ResourceCreated, nameof(OrchestrationServiceDeclarationParser), messageDeclarationResource.Key, messageDeclarationResource.Name, messageDeclarationResource.Type, messageDeclarationResource.Key);                
            }
        }

        /// <summary>
        /// Parse the correlation declarations.
        /// </summary>
        /// <param name="orchestration">The orchestration containing the correlation declarations.</param>
        /// <param name="serviceDeclarationResource">The service declaration to add the parsed resources to.</param>
        private void ParseCorrelationDeclarations(Orchestration orchestration, ResourceItem serviceDeclarationResource)
        {
            foreach (var correlationDeclaration in orchestration.FindCorrelationDeclarations() ?? Array.Empty<Element>())
            {
                var resourceName = correlationDeclaration.FindPropertyValue(MetaModelConstants.PropertyKeyName);
                var resourceKey = string.Concat(serviceDeclarationResource.Key, ":", resourceName);

                var correlationDeclarationResource = new ResourceItem
                {
                    Name = resourceName,
                    Key = resourceKey,
                    Type = ModelConstants.ResourceCorrelationDeclaration,
                    ParentRefId = serviceDeclarationResource.RefId,
                    Rating = ConversionRating.NotSupported
                };

                correlationDeclaration.Resource = correlationDeclarationResource; // Maintain pointer to resource.
                correlationDeclarationResource.SourceObject = correlationDeclaration; // Maintain backward pointer.
                serviceDeclarationResource.Resources.Add(correlationDeclarationResource);

                _logger.LogTrace(TraceMessages.ResourceCreated, nameof(OrchestrationServiceDeclarationParser), correlationDeclarationResource.Key, correlationDeclarationResource.Name, correlationDeclarationResource.Type, correlationDeclarationResource.Key);
            }
        }

        /// <summary>
        /// Parse the port declarations.
        /// </summary>
        /// <param name="orchestration">The orchestration containing the port declarations.</param>
        /// <param name="serviceDeclarationResource">The service declaration to add the parsed resources to.</param>
        private void ParsePortDeclarations(Orchestration orchestration, ResourceItem serviceDeclarationResource)
        {
            foreach (var portDeclaration in orchestration.FindPortDeclarations() ?? Array.Empty<Element>())
            {
                var resourceName = portDeclaration.FindPropertyValue(MetaModelConstants.PropertyKeyName);
                var resourceKey = string.Concat(serviceDeclarationResource.Key, ":", resourceName);

                var portDeclarationResource = new ResourceItem
                {
                    Name = resourceName,
                    Key = resourceKey,
                    Type = ModelConstants.ResourcePortDeclaration,
                    ParentRefId = serviceDeclarationResource.RefId,
                    Rating = ConversionRating.NotSupported
                };

                portDeclaration.Resource = portDeclarationResource; // Maintain pointer to resource.
                portDeclarationResource.SourceObject = portDeclaration; // Maintain backward pointer.
                serviceDeclarationResource.Resources.Add(portDeclarationResource);

                _logger.LogTrace(TraceMessages.ResourceCreated, nameof(OrchestrationServiceDeclarationParser), portDeclarationResource.Key, portDeclarationResource.Name, portDeclarationResource.Type, portDeclarationResource.Key);
            }
        }
    }
}
