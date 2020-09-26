using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Target;
using Microsoft.AzureIntegrationMigration.BizTalk.Analyze;
using Microsoft.AzureIntegrationMigration.BizTalk.Discover.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Analyze
{
    /// <summary>
    /// Stage runner for generating the target resources in the target model.
    /// </summary>
    public class ResourceGeneratorAnalyzer : StageRunnerBase, IStageAnalyzer
    {
        /// <summary>
        /// Defines the name of this stage runner.
        /// </summary>
        public const string PluginName = ModelConstants.ApplicationPrefix + "resourcegenerator";

        /// <summary>
        /// Defines an argument name representing the template configuration path.
        /// </summary>
        public const string ArgTemplateConfigPath = "microsoft.core.templateconfigpath";

        /// <summary>
        /// Defines an argument name representing the Azure Integration Services target.
        /// </summary>
        public const string ArgAzureIntegrationServicesTarget = "microsoft.core.target";

        /// <summary>
        /// Defines an argument name representing the Azure subscription ID.
        /// </summary>
        public const string ArgAzureSubscriptionId = "microsoft.core.subscriptionid";

        /// <summary>
        /// Defines an argument name representing the Azure primary region.
        /// </summary>
        public const string ArgAzurePrimaryRegion = "microsoft.core.primaryregion";

        /// <summary>
        /// Defines an argument name representing the Azure secondary region.
        /// </summary>
        public const string ArgAzureSecondaryRegion = "microsoft.core.secondaryregion";

        /// <summary>
        /// Defines an argument name representing the deployment environment.
        /// </summary>
        public const string ArgDeploymentEnvironment = "microsoft.core.deploymentenv";

        /// <summary>
        /// Defines an argument name representing a unique deployment ID.
        /// </summary>
        public const string ArgUniqueDeploymentId = "microsoft.core.uniquedeploymentid";

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines a directory containing template config files.
        /// </summary>
        private string _templateConfigDir;

        /// <summary>
        /// Defines the target for AIS.
        /// </summary>
        private AzureIntegrationServicesTargetEnvironment _target;

        /// <summary>
        /// Defines the Azure subscription ID.
        /// </summary>
        private string _subscriptionId;

        /// <summary>
        /// Defines the Azure primary region.
        /// </summary>
        private string _primaryRegion;

        /// <summary>
        /// Defines the Azure secondary region.
        /// </summary>
        private string _secondaryRegion;

        /// <summary>
        /// Defines the deployment environment.
        /// </summary>
        private string _deploymentEnvironment;

        /// <summary>
        /// Defines a unique deployment ID.
        /// </summary>
        private string _uniqueDeploymentId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceGeneratorAnalyzer"/> class with a generic logger.
        /// </summary>
        /// <param name="logger">A logger.</param>
        public ResourceGeneratorAnalyzer(ILogger logger)
            : base(PluginName, Stages.Analyze, 40, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Runs the stage runner.
        /// </summary>
        /// <param name="state">The execution state.</param>
        /// <param name="token">A cancellation token used to cancel this operation.</param>
        /// <returns>A task used to await the operation.</returns>
        protected override async Task InvokeRunAsync(IRunState state, CancellationToken token)
        {
            _ = state ?? throw new ArgumentNullException(nameof(state));

            _logger.LogTrace(TraceMessages.RunningResourceGeneratorAnalyzer);

            // Populate model with arguments from execution state
            var model = (AzureIntegrationServicesModel)state.Model;
            model.MigrationTarget.TargetEnvironment = _target;
            model.MigrationTarget.AzureSubscriptionId = _subscriptionId;
            model.MigrationTarget.AzurePrimaryRegion = _primaryRegion;
            model.MigrationTarget.AzureSecondaryRegion = _secondaryRegion;
            model.MigrationTarget.DeploymentEnvironment = _deploymentEnvironment;
            model.MigrationTarget.UniqueDeploymentId = _uniqueDeploymentId;

            // Get context
            var context = Container.GetRequiredService<MigrationContext>();
            context.TemplateConfigFolder = _templateConfigDir;

            // Generate resources
            var analyzer = Container.GetRequiredService<BizTalk.Analyze.ResourceGeneratorAnalyzer>();
            await analyzer.AnalyzeAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets arguments for this stage runner.
        /// </summary>
        protected override void GetArgs()
        {
            // Template Config Directory
            var argTemplateConfigDir = GetArg<string>(ArgTemplateConfigPath);
            if (!string.IsNullOrWhiteSpace(argTemplateConfigDir))
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArgTemplateConfigPath, argTemplateConfigDir);

                _templateConfigDir = new DirectoryInfo(argTemplateConfigDir).FullName;
            }

            // Azure Integration Services Target
            var argTarget = GetArg<string>(ArgAzureIntegrationServicesTarget);
            if (!string.IsNullOrWhiteSpace(argTarget))
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArgAzureIntegrationServicesTarget, argTarget);

                _target = Enum.Parse<AzureIntegrationServicesTargetEnvironment>(argTarget);
            }

            // Azure Subscription ID
            var argSubscriptionId = GetArg<string>(ArgAzureSubscriptionId);
            if (!string.IsNullOrWhiteSpace(argSubscriptionId))
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArgAzureSubscriptionId, argSubscriptionId);

                _subscriptionId = argSubscriptionId;
            }

            // Azure Primary Region
            var argPrimaryRegion = GetArg<string>(ArgAzurePrimaryRegion);
            if (!string.IsNullOrWhiteSpace(argPrimaryRegion))
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArgAzurePrimaryRegion, argPrimaryRegion);

                _primaryRegion = argPrimaryRegion;
            }

            // Azure Secondary Region
            var argSecondaryRegion = GetArg<string>(ArgAzureSecondaryRegion);
            if (!string.IsNullOrWhiteSpace(argSecondaryRegion))
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArgAzureSecondaryRegion, argSecondaryRegion);

                _secondaryRegion = argSecondaryRegion;
            }

            // Deployment Environment
            var argDeploymentEnv = GetArg<string>(ArgDeploymentEnvironment);
            if (!string.IsNullOrWhiteSpace(argDeploymentEnv))
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArgDeploymentEnvironment, argDeploymentEnv);

                _deploymentEnvironment = argDeploymentEnv;
            }

            // Unique Deployment ID
            var argUniqueDeploymentId = GetArg<string>(ArgUniqueDeploymentId);
            if (!string.IsNullOrWhiteSpace(argUniqueDeploymentId))
            {
                _logger.LogInformation(InformationMessages.ArgumentsFound, ArgUniqueDeploymentId, argUniqueDeploymentId);

                _uniqueDeploymentId = argUniqueDeploymentId;
            }
        }

        /// <summary>
        /// Validates the arguments found on the command line.
        /// </summary>
        protected override bool ValidateArgs()
        {
            var valid = true;

            // Template Config Path
            if (!string.IsNullOrWhiteSpace(_templateConfigDir))
            {
                var dirInfo = new DirectoryInfo(_templateConfigDir);
                if (!dirInfo.Exists)
                {
                    _logger.LogError(ErrorMessages.TemplateConfigDirectoryDoesNotExist, dirInfo.FullName);
                    valid = false;
                }
            }

            return valid;
        }
    }
}
