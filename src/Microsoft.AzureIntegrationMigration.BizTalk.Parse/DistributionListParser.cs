//-----------------------------------------------------------------------
// <copyright file="SendPortGroupFilterParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Parses the filter expressions from BizTalk send port groups.</summary>
//-----------------------------------------------------------------------

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
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Filters;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Parses the distribution lists.
    /// </summary>
    public sealed class DistributionListParser : BizTalkParserBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="DistributionListParser" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">A logger.</param>
        public DistributionListParser(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(DistributionListParser), model, context, logger)
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
                _logger.LogDebug(TraceMessages.SkippingParserAsTheSourceModelIsMissing, nameof(DistributionListParser));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningParser, nameof(DistributionListParser));

                foreach (var application in group.Applications)
                {
                    // Defensive check
                    if (application.Application.Bindings == null)
                    {
                        _logger.LogWarning(WarningMessages.BindingInfoNotFound, application.Application.Name);
                        continue;
                    }

                    _logger.LogDebug(TraceMessages.ParsingBizTalkSendPortGroupFilterForApplication, application.Application.Name);

                    var bindingFileDefinition = model.FindResourceDefinitionByKey(application.Application.Bindings.ResourceDefinitionKey, ModelConstants.ResourceDefinitionBindings);
                    var applicationResource = model.FindResourceByKey(application.Application?.ApplicationDefinition?.ResourceKey);

                    foreach (var distributionList in application.Application.Bindings.BindingInfo.DistributionListCollection)
                    {
                        _logger.LogDebug(TraceMessages.ParsingBizTalkSendPortGroupFilterForDistributionList, distributionList.Name);

                        // Set the resource key for the distribution list.
                        distributionList.ResourceKey = string.Concat(application.Application.Bindings.ResourceDefinitionKey, ":", distributionList.Name);

                        // Create the resource under the application.
                        var distributionListResource = new ResourceItem
                        {
                            Key = distributionList.ResourceKey,
                            Name = distributionList.Name,
                            Description = distributionList.Description,
                            Type = ModelConstants.ResourceDistributionList,
                            ParentRefId = bindingFileDefinition.RefId,
                            Rating = ConversionRating.NotSupported
                        };

                        distributionList.Resource = distributionListResource; // Maintain pointer to the resource.
                        distributionListResource.SourceObject = distributionList; // Maintain backward pointer.

                        bindingFileDefinition.Resources.Add(distributionListResource);
                     
                        if (applicationResource != null)
                        {
                            applicationResource.AddRelationship(new ResourceRelationship(distributionListResource.RefId, ResourceRelationshipType.Child));
                            distributionListResource.AddRelationship(new ResourceRelationship(applicationResource.RefId, ResourceRelationshipType.Parent));
                        }
                        else
                        {
                            var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindResourceDefinition, ModelConstants.ResourceApplication, application.Application?.ApplicationDefinition?.ResourceKey);
                            _logger.LogError(error);
                            context.Errors.Add(new ErrorMessage(error));
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(distributionList.Filter))
                            {
                                distributionList.FilterExpression = Filter.FromXml(distributionList.Filter);
                                distributionList.FilterExpression.ResourceKey = string.Concat(distributionList.Name, ":", "filter");

                                // Create the distribution filter resource.
                                var filterReportResource = new ResourceItem
                                {
                                    Key = distributionList.FilterExpression.ResourceKey,
                                    Name = distributionListResource.Name + " filter expression",                                    
                                    Type = ModelConstants.ResourceFilterExpression,
                                    ParentRefId = distributionListResource.RefId,
                                    Rating = ConversionRating.NotSupported
                                };

                                distributionList.FilterExpression.Resource = filterReportResource; // Maintain pointer to the resource.
                                filterReportResource.SourceObject = distributionList.FilterExpression; // Maintain backward pointer.
                                distributionListResource.Resources.Add(filterReportResource);
                            }
                        }
                        catch (Exception ex)
                        {
                            var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ErrorParsingDistributionListFilter, distributionList.Name, application.Application.Name, ex.Message);
                            context.Errors.Add(new ErrorMessage(message));
                            _logger.LogError(message);
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.CompletedParser, nameof(DistributionListParser));
            }
        }
    }
}
