// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert.GeneratorRules
{
    /// <summary>
    /// Defines a class that implements a converter that copies schema to the target folder.
    /// </summary>
    public sealed class SC001DocumentSchemaGenerator : BizTalkConverterBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "SC001";

        /// <summary>
        /// Defines a file repository.
        /// </summary>
        private readonly IFileRepository _fileRepository;

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="SC001DocumentSchemaGenerator"/> class.
        /// </summary>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public SC001DocumentSchemaGenerator(IFileRepository fileRepository, IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(SC001DocumentSchemaGenerator), model, context, logger)
        {
            // Validate and set the member.
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate schema files for each application in the target under the resource output path.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "The folder paths are lowercased, so must use a lowercase function.")]
        protected override async Task ConvertInternalAsync(CancellationToken token)
        {
            if (Model.MigrationTarget.MessageBus?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsMigrationTargetMessageBusMissing, RuleName, nameof(SC001DocumentSchemaGenerator));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningGenerator, RuleName, nameof(SC001DocumentSchemaGenerator));

                var conversionPath = Context.ConversionFolder;

                foreach (var targetApplication in Model.MigrationTarget.MessageBus.Applications)
                {
                    foreach (var message in targetApplication.Messages)
                    {
                        // Key the resource for the schema (message definition from the model).
                        var messageResource = Model.FindResourceByKey(message.MessageSchema?.ResourceKeyRef);

                        if (messageResource == null)
                        {
                            _logger.LogWarning(WarningMessages.ResourceNotFoundByKey, message.MessageSchema?.ResourceKeyRef);
                        }
                        else
                        {
                            // Get the schema resource (which is a parent of the message definition) from the model.
                            var schemaResource = Model.FindResourceByRefId(messageResource.ParentRefId);

                            if (schemaResource == null)
                            {
                                _logger.LogWarning(WarningMessages.ResourceNotFoundByRefId, messageResource.ParentRefId);
                            }
                            else
                            {
                                // Get the schema resource definition which is the parent of the schema resource. This is required for the schema resource definition content.
                                var schemaResourceDefinition = Model.FindResourceDefinitionByRefId(schemaResource.ParentRefId);

                                if (schemaResourceDefinition == null)
                                {
                                    _logger.LogWarning(WarningMessages.ResourceDefinitionNotFound, schemaResource.ParentRefId);
                                }
                                else
                                {
                                    var integrationAccountSchemaResource = message.Resources.SingleOrDefault(r => r.ResourceType == ModelConstants.ResourceTypeXml);

                                    if (integrationAccountSchemaResource != null)
                                    {
                                        var fileName = $"{targetApplication.Name}.{message.MessageSchema.Name}".Replace(" ", string.Empty);

                                        var outputPath = new FileInfo(Path.Combine(conversionPath, Path.Combine(integrationAccountSchemaResource.OutputPath, $"{fileName}.xsd")));

                                        _fileRepository.WriteXmlFile(outputPath.FullName, schemaResourceDefinition.ResourceContent.ToString());

                                        _logger.LogDebug(TraceMessages.SavingArtifact, outputPath.FullName);
                                    }
                                    else
                                    {
                                        var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceTemplateForMessagingObject, ModelConstants.ResourceTypeXml, message.Name);
                                        _logger.LogError(error);
                                        Context.Errors.Add(new ErrorMessage(error));
                                    }
                                }
                            }
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.GeneratorCompleted, RuleName, nameof(SC001DocumentSchemaGenerator));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
