// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="IBizTalkAnalyzer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Describes the public behaviour of an analyzer.</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Analyze
{
    /// <summary>
    /// Describes the public behaviour of an analyzer.
    /// </summary>
    public interface IBizTalkAnalyzer
    {
        /// <summary>
        /// Gets the name of the analyzer runner.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Analyzes the contents of the parsed source and builds the output report.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task used to await the operation.</returns>
        Task AnalyzeAsync(CancellationToken token);
    }
}
