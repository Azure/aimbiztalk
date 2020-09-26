using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Report;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.ApplicationDefinitions;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Deployment.Resources;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze.DependencyRules
{
    /// <summary>
    /// Defines an Analyzer to resolve dependency between distribution lists and send ports.
    /// </summary>
    public sealed class DP005DistributionListDependencyAnalyzer : BizTalkAnalyzerBase
    {
        /// <summary>
        /// Defines the name of this rule.
        /// </summary>
        private const string RuleName = "DP005";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of a <see cref="DP005DistributionListDependencyAnalyzer"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">A logger.</param>
        public DP005DistributionListDependencyAnalyzer(IApplicationModel model, MigrationContext context, ILogger logger)
            : base(nameof(DP005DistributionListDependencyAnalyzer), model, context, logger)
        {
            // Validate and set the member.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Performs the internal work of analyzing the model.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        protected override async Task AnalyzeInternalAsync(CancellationToken token)
        {
            // Get parsed BizTalk model from the application model
            var resources = Model.FindAllResources();

            if (resources == null || !resources.Any())
            {
                _logger.LogDebug(TraceMessages.SkippingRuleAsSourceModelMissing, RuleName, nameof(DP005DistributionListDependencyAnalyzer));
            }
            else
            {
                _logger.LogDebug(TraceMessages.RunningRule, RuleName, nameof(DP005DistributionListDependencyAnalyzer));

                // Get a list of all the send port groups
                var distributionLists = resources.Where(r => r.Type == ModelConstants.ResourceDistributionList);

                foreach (var distributionList in distributionLists)
                {
                    if (distributionList.SourceObject == null || !(distributionList.SourceObject.GetType() == typeof(DistributionList)))
                    {
                        // Source model corrupt
                        _logger.LogError(ErrorMessages.UnableToFindAssociatedResource, distributionList.Type, distributionList.Name, distributionList.Key);
                        distributionList.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Error, Message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToFindAssociatedResource, distributionList.Type, distributionList.Name, distributionList.Key) });
                        continue;
                    }

                    // Look up the send ports in the send port group.
                    var dl = (DistributionList)distributionList.SourceObject;
                    foreach (var sendPortRef in dl.SendPorts)
                    {
                        var matchingSendPorts = resources.Where(r => r.Type == ModelConstants.ResourceSendPort && r.SourceObject != null && r.SourceObject.GetType() == typeof(SendPort) && ((SendPort)r.SourceObject).Name == sendPortRef.Name);
                        if (matchingSendPorts == null || !matchingSendPorts.Any())
                        {
                            // No send ports found matching the one referenced in the DL
                            _logger.LogWarning(WarningMessages.DistributionListSendPortNotFound, sendPortRef.Name, distributionList.Name, distributionList.Key);
                            distributionList.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.DistributionListSendPortNotFound, sendPortRef.Name, distributionList.Name, distributionList.Key) });
                        }
                        else if (matchingSendPorts.Count() > 1)
                        {
                            // There are multiple send ports found, for example when two apps being analyzed use the same name
                            _logger.LogWarning(WarningMessages.DistributionListSendPortMultipleMatches, sendPortRef.Name, distributionList.Name, distributionList.Key);
                            distributionList.ReportMessages.Add(new ReportMessage() { Severity = MessageSeverity.Warning, Message = string.Format(CultureInfo.CurrentCulture, WarningMessages.DistributionListSendPortMultipleMatches, sendPortRef.Name, distributionList.Name, distributionList.Key) });
                        }
                        else
                        {
                            // All good.  A send port has been found.
                            var sendPort = matchingSendPorts.Single();
                            distributionList.AddRelationship(new ResourceRelationship(sendPort.RefId, ResourceRelationshipType.CallsTo));
                            sendPort.AddRelationship(new ResourceRelationship(distributionList.RefId, ResourceRelationshipType.CalledBy));
                        }
                    }
                }

                _logger.LogDebug(TraceMessages.RuleCompleted, RuleName, nameof(DP005DistributionListDependencyAnalyzer));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
