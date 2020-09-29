// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert.GeneratorRules
{
    /// <summary>
    /// Implements rule SC002, which generate the property schema configuration.
    /// </summary>
    public class SC002PropertySchemaGenerator : BizTalkConverterBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "SC002";

        /// <summary>
        /// Defines a file repository.
        /// </summary>
        private readonly IFileRepository _fileRepository;

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="SC002PropertySchemaGenerator"/> class.
        /// </summary>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public SC002PropertySchemaGenerator(IFileRepository fileRepository, IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(SC002PropertySchemaGenerator), model, context, logger)
        {
            // Validate and set the member.
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Performs the work of conversion.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task ConvertInternalAsync(CancellationToken token)
        {
            if (Model.MigrationTarget.MessageBus?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsMigrationTargetMessageBusMissing, RuleName, nameof(SC001DocumentSchemaGenerator));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningGenerator, RuleName, nameof(SC002PropertySchemaGenerator));

                // Find the messages in the target application.
                var applicationMessages = from application in Model.MigrationTarget?.MessageBus?.Applications
                                          from message in application.Messages
                                          where message.RoutingProperties?.Count > 0
                                          select new { Application = application, Message = message };

                var conversionPath = Context.ConversionFolder;

                foreach (var applicationMessage in applicationMessages)
                {
                    var appConfigResource = applicationMessage.Message.Resources.SingleOrDefault(r => r.ResourceType == ModelConstants.ResourceTypeRoutingProperties);

                    if (appConfigResource != null)
                    {

                        // Get the routing properties for the schema.
                        // This logic is to handle multi-root schemas, the routing properties will be against each root element/nested type in the model, 
                        // and need to be consolidated for the configuration.
                        var routingProperties = from appMessage in applicationMessages
                                                from properties in appMessage.Message.RoutingProperties.ToList()
                                                where appMessage.Message.MessageSchema.Name == applicationMessage.Message.MessageSchema.Name
                                                select new { PropertyName = properties.Key, PropertyValue = properties.Value };

                        // Generate the config.
                        var routingConfig =
                            new JObject(
                                new JProperty("routingProperties",
                                new JArray(
                                    from routingProperty in routingProperties
                                    select new JObject(
                                        new JProperty("propertyName", routingProperty.PropertyName),
                                        new JProperty("propertyType", "xpath"),
                                        new JProperty("propertyValue", routingProperty.PropertyValue)
                                        )
                                    )
                                ));

                        var fileName = $"{applicationMessage.Application.Name}.{applicationMessage.Message.MessageSchema.Name}".Replace(" ", string.Empty);

                        var outputPath = new FileInfo(Path.Combine(conversionPath, Path.Combine(appConfigResource.OutputPath, $"{fileName}.json")));

                        _fileRepository.WriteJsonFile(outputPath.FullName, routingConfig);
                    }
                    else
                    {
                        var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypeRoutingProperties, applicationMessage.Message.Name);
                        _logger.LogError(error);
                        Context.Errors.Add(new ErrorMessage(error));
                    }
                }

                _logger.LogDebug(TraceMessages.GeneratorCompleted, RuleName, nameof(SC002PropertySchemaGenerator));

            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
