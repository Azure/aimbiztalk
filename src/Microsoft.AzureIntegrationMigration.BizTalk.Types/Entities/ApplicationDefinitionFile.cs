// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="ApplicationDefinitionFile.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a discovered BizTalk application definition file.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.ApplicationDefinitions;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces;
using Newtonsoft.Json;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities
{
    /// <summary>
    /// Represents a discovered BizTalk application definition file.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class ApplicationDefinitionFile : ISourceResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDefinitionFile"/> class.
        /// </summary>
        public ApplicationDefinitionFile()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDefinitionFile"/> class with related resource container and definition name.
        /// </summary>
        /// <param name="resourceContainerKey">The key of the resource container for the application definition.</param>
        /// <param name="resourceDefinitionKey">The key of the resource definition containing the content of the application definition.</param>
        public ApplicationDefinitionFile(string resourceContainerKey, string resourceDefinitionKey)
        {
            ResourceContainerKey = resourceContainerKey ?? throw new ArgumentNullException(nameof(resourceContainerKey));
            ResourceDefinitionKey = resourceDefinitionKey ?? throw new ArgumentNullException(nameof(resourceDefinitionKey));
        }

        /// <summary>
        /// Gets or sets the resource container key of the application definition.
        /// </summary>
        public string ResourceContainerKey { get; set; }

        /// <summary>
        /// Gets or sets the resource definition key of the application definition.
        /// </summary>
        public string ResourceDefinitionKey { get; set; }

        #region ISourceResource Implementation

        /// <summary>
        /// A pointer to the associated resource created from this object.
        /// </summary>
        [JsonIgnore]
        public ResourceItem Resource { get; set; }

        /// <summary>
        /// Gets or sets the resource key of the application definition.
        /// </summary>
        public string ResourceKey { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the <see cref="ApplicationDefinition"/>.
        /// </summary>
        public ApplicationDefinition ApplicationDefinition { get; set; }
    }
}
