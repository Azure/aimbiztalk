// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.StageRunners.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Core;
using Microsoft.AzureIntegrationMigration.Runner.Engine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureIntegrationMigration.BizTalk.StageRunners
{
    /// <summary>
    /// Defines a base stage runner.
    /// </summary>
    public abstract class StageRunnerBase : IStageRunner
    {
        /// <summary>
        /// Defines the name of the stage runner.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Defines the stage.
        /// </summary>
        private readonly Stages _stage;

        /// <summary>
        /// Defines the priority.
        /// </summary>
        private int _priority;

        /// <summary>
        /// Defines a value indicating whether to skip execution of this runner.
        /// </summary>
        private bool _skip;

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the command line arguments.
        /// </summary>
        private IDictionary<string, object> _args;

        /// <summary>
        /// Defines the IoC container.
        /// </summary>
        private IServiceProvider _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="StageRunnerBase"/> class with the name
        /// of the plugin, priority and a generic logger.
        /// </summary>
        /// <param name="pluginName">The name of the plugin.</param>
        /// <param name="stage">The stage.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="logger">A logger.</param>
        public StageRunnerBase(string pluginName, Stages stage, int priority, ILogger logger)
        {
            _name = pluginName ?? throw new ArgumentNullException(nameof(pluginName));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stage = stage;
            _priority = priority;
        }

        #region IStageRunner Interface Implementation

        /// <summary>
        /// Gets the name of the stage runner.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets or sets the priority of the stage runner.
        /// </summary>
        public int Priority { get => _priority; set => _priority = value; }

        /// <summary>
        /// Gets or sets a value indicating whether to skip execution of this runner.
        /// </summary>
        public bool Skip { get => _skip; set => _skip = value; }

        /// <summary>
        /// Gets the stage this object will run in.
        /// </summary>
        public Stages Stages => _stage;

        /// <summary>
        /// Runs the stage runner.
        /// </summary>
        /// <param name="state">The execution state.</param>
        /// <param name="token">A cancellation token used to cancel this operation.</param>
        /// <returns>A task used to await the operation.</returns>
        public async Task RunAsync(IRunState state, CancellationToken token)
        {
            _ = state ?? throw new ArgumentNullException(nameof(state));

            _logger.LogDebug(TraceMessages.RunningPlugin, Name);

            // Check if cancellation requested
            token.ThrowIfCancellationRequested();

            // Get model and make sure it is one we support
            if (state.Model == null)
            {
                _logger.LogError(ErrorMessages.ApplicationModelExpected);

                throw new RunnerException(ErrorMessages.ApplicationModelExpected);
            }
            else if (!(state.Model is AzureIntegrationServicesModel))
            {
                var error = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ApplicationModelNotExpectedModelType, state.Model?.GetType().FullName);

                _logger.LogError(error);

                throw new RunnerException(error);
            }
            else
            {
                // Set up IoC container
                _container = DependencyContainer.BuildContainer(state.Model, _logger, state);

                // Allow derived component to get arguments
                _args = state?.Configuration?.Args;
                GetArgs();

                // Validate the arguments
                if (!ValidateArgs())
                {
                    // Failed, print out error and return
                    _logger.LogError(ErrorMessages.ArgumentsValidationFailed);

                    throw new RunnerException(ErrorMessages.ArgumentsValidationFailed);
                }
                else
                {
                    // Invoke runner
                    await InvokeRunAsync(state, token).ConfigureAwait(false);

                    // Any context errors?
                    var context = _container.GetRequiredService<MigrationContext>();
                    if (context.Errors.Count > 0)
                    {
                        // Build error list
                        var sb = new StringBuilder();
                        foreach (var error in context.Errors)
                        {
                            sb.AppendLine(string.Concat("--> ", error.Message));
                        }

                        throw new RunnerException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.PluginFailedWithErrors, Name, sb.ToString()));
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets the IoC container to resolve object instances.
        /// </summary>
        protected IServiceProvider Container => _container;

        /// <summary>
        /// Runs the stage runner.
        /// </summary>
        /// <param name="state">The execution state.</param>
        /// <param name="token">A cancellation token used to cancel this operation.</param>
        /// <returns>A task used to await the operation.</returns>
        protected abstract Task InvokeRunAsync(IRunState state, CancellationToken token);

        /// <summary>
        /// Gets an argument for this stage runner.
        /// </summary>
        /// <param name="arg">The key of the command line argument.</param>
        protected virtual TArg GetArg<TArg>(string arg)
        {
            if (_args != null && _args.Any())
            {
                if (_args.ContainsKey(arg))
                {
                    var argValue = _args[arg];
                    return argValue == null ? default : (TArg)argValue;
                }
            }

            return default;
        }

        /// <summary>
        /// Gets an argument for this stage runner.
        /// </summary>
        /// <param name="arg">The key of the command line argument.</param>
        protected virtual object GetArg(string arg)
        {
            if (_args != null && _args.Any())
            {
                if (_args.ContainsKey(arg))
                {
                    var argValue = _args[arg];
                    return argValue ?? null;
                }
            }

            return default;
        }

        /// <summary>
        /// Finds arguments for this stage runner.
        /// </summary>
        /// <remarks>
        /// Override to get arguments.
        /// </remarks>
        protected virtual void GetArgs()
        {
            // Nothing to do in base class
        }

        /// <summary>
        /// Validates the arguments found on the command line.
        /// </summary>
        protected virtual bool ValidateArgs()
        {
            return true;
        }
    }
}
