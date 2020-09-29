// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="BindingFile.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a discovered BizTalk binding file.</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities
{
    /// <summary>
    /// Represents a discovered BizTalk binding file.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class BindingFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindingFile"/> class.
        /// </summary>
        public BindingFile()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingFile"/> class with related resource container and definition name.
        /// </summary>
        /// <param name="resourceContainerKey">The key of the resource container for these bindings.</param>
        /// <param name="resourceDefinitionKey">The key of the resource definition containing the content of these bindings.</param>
        public BindingFile(string resourceContainerKey, string resourceDefinitionKey)
        {
            ResourceContainerKey = resourceContainerKey ?? throw new ArgumentNullException(nameof(resourceContainerKey));
            ResourceDefinitionKey = resourceDefinitionKey ?? throw new ArgumentNullException(nameof(resourceDefinitionKey));
        }

        /// <summary>
        /// Gets or sets the resource container key of the bindings.
        /// </summary>
        public string ResourceContainerKey { get; set; }

        /// <summary>
        /// Gets or sets the resource definition key of the bindings.
        /// </summary>
        public string ResourceDefinitionKey { get; set; }

        /// <summary>
        /// Gets or sets the binding info.
        /// </summary>
        public BindingInfo BindingInfo { get; set; }
    }
}
