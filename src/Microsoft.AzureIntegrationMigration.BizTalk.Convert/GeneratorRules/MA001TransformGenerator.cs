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
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target.Intermediaries;
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
    public sealed class MA001TransformGenerator : BizTalkConverterBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "MA001";

        /// <summary>
        /// Defines a file repository.
        /// </summary>
        private readonly IFileRepository _fileRepository;

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="MA001TransformGenerator"/> class.
        /// </summary>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public MA001TransformGenerator(IFileRepository fileRepository, IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(MA001TransformGenerator), model, context, logger)
        {
            // Validate and set the member.
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate mapper files for each application in the target under the resource output path.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "The folder paths are lowercased, so must use a lowercase function.")]
        protected override async Task ConvertInternalAsync(CancellationToken token)
        {
            if (Model.MigrationTarget.MessageBus?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsMigrationTargetMessageBusMissing, RuleName, nameof(MA001TransformGenerator));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningGenerator, RuleName, nameof(MA001TransformGenerator));

                var conversionPath = Context.ConversionFolder;

                foreach (var targetApplication in Model.MigrationTarget.MessageBus.Applications)
                {
                    foreach (var message in targetApplication.Messages)
                    {
                        foreach (var messageTransform in message.MessageTransforms)
                        {
                            var messageResource = Model.FindResourceByKey(messageTransform.ResourceKeyRef);

                            if (messageResource == null)
                            {
                                _logger.LogWarning(WarningMessages.ResourceNotFoundByKey, messageTransform.ResourceKeyRef);
                            }
                            else
                            {
                                var mapResourceDefinition = Model.FindResourceDefinitionByRefId(messageResource.ParentRefId);

                                if (mapResourceDefinition == null)
                                {
                                    _logger.LogError(ErrorMessages.UnableToFindResourceDefinition, mapResourceDefinition.ParentRefId);
                                    Context.Errors.Add(new ErrorMessage(string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, mapResourceDefinition.ParentRefId)));
                                }
                                else
                                {
                                    var integrationAccountSchemaResources = message.Resources.Where(r => r.ResourceType == ModelConstants.ResourceTypeXslt);

                                    foreach (var integrationAccountSchemaResource in integrationAccountSchemaResources)
                                    {
                                        var fileName = $"{targetApplication.Name}.{messageResource.Name}".Replace(" ", string.Empty);

                                        var outputPath = new FileInfo(Path.Combine(conversionPath, Path.Combine(integrationAccountSchemaResource.OutputPath, $"{fileName}.xslt")));

                                        _fileRepository.WriteXmlFile(outputPath.FullName, mapResourceDefinition.ResourceContent.ToString());

                                        _logger.LogDebug(TraceMessages.SavingArtifact, outputPath.FullName);
                                    }
                                }
                            }
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.GeneratorCompleted, RuleName, nameof(MA001TransformGenerator));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
