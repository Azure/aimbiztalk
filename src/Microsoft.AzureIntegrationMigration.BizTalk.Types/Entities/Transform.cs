// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="Transform.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a BizTalk Transform.</summary>
//-----------------------------------------------------------------------

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Schema;
    using System.Xml.Xsl;
    using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
    using Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces;
    using Newtonsoft.Json;

    #endregion

    /// <summary>
    /// Represents a BizTalk Transform.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class Transform : CompiledComponent, ISourceResource
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Transform"/> class.
        /// </summary>
        public Transform()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transform"/> class with related resource container and definition name.
        /// </summary>
        /// <param name="resourceContainerKey">The name of the resource container key for this transform.</param>
        /// <param name="resourceDefinitionKey">The key of the resource definition containing the content of this transform.</param>
        public Transform(string resourceContainerKey, string resourceDefinitionKey)
        {
            ResourceContainerKey = resourceContainerKey ?? throw new ArgumentNullException(nameof(resourceContainerKey));
            ResourceDefinitionKey = resourceDefinitionKey ?? throw new ArgumentNullException(nameof(resourceDefinitionKey));
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the resource container key of the orchestration.
        /// </summary>
        public string ResourceContainerKey { get; set; }

        /// <summary>
        /// Gets or sets the resource definition key of the orchestration.
        /// </summary>
        public string ResourceDefinitionKey { get; set; }

        /// <summary>
        /// Gets or sets the full type name for the source schemas.
        /// </summary>
        public IList<string> SourceSchemaTypeNames { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the full type name for the target schemas.
        /// </summary>
        public IList<string> TargetSchemaTypeNames { get; } = new List<string>();

        #region ISourceResource Implementation

        /// <summary>
        /// A pointer to the associated resource created from this object.
        /// </summary>
        [JsonIgnore]
        public ResourceItem Resource { get; set; }

        /// <summary>
        /// The resource key for the associated resource.
        /// </summary>
        public string ResourceKey { get; set; }

        #endregion

        #endregion
    }
}
