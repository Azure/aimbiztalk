using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert
{
    /// <summary>
    /// Describes the public behaviour of a converter.
    /// </summary>
    public interface IBizTalkConverter
    {
        /// <summary>
        /// Gets the name of the converter runner.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Runs a conversion component.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        Task ConvertAsync(CancellationToken token);
    }
}
