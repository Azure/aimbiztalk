// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="BindingFileParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Parses the bindings file.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Globalization;
using System.Linq;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Parses the bindings file.
    /// </summary>
    public sealed class BindingFileParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of this parser.
        /// </summary>
        public const string ParserName = ModelConstants.ApplicationPrefix + "bindingfileparser";

        /// <summary>
        /// Creates a new instance of the <see cref="BindingFileParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public BindingFileParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(ParserName, model, context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Discovers the <see cref="BindingInfo"/> contents from the binding file XML.
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
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(BindingFileParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(BindingFileParser));

                foreach (var application in group.Applications)
                {
                    try
                    {
                        // Defensive check
                        if (application.Application.Bindings == null)
                        {
                            _logger.LogWarning(WarningMessages.BindingInfoNotFound, application.Application.Name);
                            continue;
                        }

                        _logger.LogDebug(TraceMessages.ParsingBizTalkBindingFileFromResourceContainer, application.Application.Bindings.ResourceContainerKey);

                        var bindingResourceDefinition = model.FindResourceDefinitionByKey(application.Application.Bindings.ResourceDefinitionKey, ModelConstants.ResourceDefinitionBindings);

                        if (bindingResourceDefinition != null)
                        {
                            var bindingInfo = BindingInfo.FromXml((string)bindingResourceDefinition.ResourceContent);
                            application.Application.Bindings.BindingInfo = bindingInfo;

                            ParseServiceBindings(bindingInfo, bindingResourceDefinition);
                        }
                        else
                        {
                            var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceDefinitionBindings, application.Application.Bindings.ResourceDefinitionKey);
                            _logger.LogError(error);
                            context.Errors.Add(new ErrorMessage(error));
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorReadingBindingInfo, application.Application.Name, ex.Message);
                        context.Errors.Add(new ErrorMessage(message));
                        _logger.LogError(message);
                    }
                }

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(BindingFileParser));
            }
        }

        /// <summary>
        /// Parses the service bindings.
        /// </summary>
        /// <param name="bindingInfo">The binding info containing the service bindings to parse.</param>
        /// <param name="bindingResourceDefintion">The binding resource definition to add the new resources to.</param>
        private void ParseServiceBindings(BindingInfo bindingInfo, ResourceDefinition bindingResourceDefintion)
        {
            foreach (var serviceBinding in bindingInfo.ModuleRefCollection.SelectMany(m => m.Services))
            {
                // Create the resource under the binding definition.
                var serviceBindingResource = new ResourceItem
                {
                    Key = string.Concat(bindingResourceDefintion.Key, ":", serviceBinding.Name),
                    Name = serviceBinding.Name,
                    Description = serviceBinding.Description,
                    Type = ModelConstants.ResourceServiceBinding,
                    ParentRefId = bindingResourceDefintion.RefId,
                    Rating = ConversionRating.NotSupported
                };
                serviceBinding.ResourceKey = serviceBindingResource.Key;
                serviceBinding.Resource = serviceBindingResource; // Maintain pointer to the resource.
                serviceBindingResource.SourceObject = serviceBinding; // Maintain backward pointer.

                bindingResourceDefintion.Resources.Add(serviceBindingResource);

                _logger.LogTrace(TraceMessages.ResourceCreated, nameof(BindingFileParser), serviceBindingResource.Key, serviceBindingResource.Name, serviceBindingResource.Type, bindingResourceDefintion.Key);
            }
        }
    }
}
