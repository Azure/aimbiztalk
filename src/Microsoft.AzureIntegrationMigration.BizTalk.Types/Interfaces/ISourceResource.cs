// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces
{
    /// <summary>
    /// Defines the behaviour of a source resource object.
    /// </summary>
    public interface ISourceResource
    {
        /// <summary>
        /// A pointer to the resource created from this object.
        /// </summary>
        ResourceItem Resource { get; set; }

        /// <summary>
        /// Gets or sets the resource key for the object.
        /// </summary>
        string ResourceKey { get; set; }
    }
}
