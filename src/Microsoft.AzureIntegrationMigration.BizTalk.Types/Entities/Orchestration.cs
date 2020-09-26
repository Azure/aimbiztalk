//-----------------------------------------------------------------------
// <copyright file="Orchestration.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a BizTalk Orchestration.</summary>
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
    using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations;

    #endregion

    /// <summary>
    /// Represents a BizTalk Orchestration.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class Orchestration : CompiledComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Orchestration"/> class.
        /// </summary>
        public Orchestration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Orchestration"/> class with related resource container and definition name.
        /// </summary>
        /// <param name="resourceContainerKey">The key of the resource container for this orchestration.</param>
        /// <param name="resourceDefinitionKey">The key of the resource definition containing the content of this orchestration.</param>
        public Orchestration(string resourceContainerKey, string resourceDefinitionKey)
        {
            ResourceContainerKey = resourceContainerKey ?? throw new ArgumentNullException(nameof(resourceContainerKey));
            ResourceDefinitionKey = resourceDefinitionKey ?? throw new ArgumentNullException(nameof(resourceDefinitionKey));
        }

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
        /// Gets or sets the <see cref="MetaModel"/> which represents the BizTalk Orchestration.
        /// </summary>
        public MetaModel Model { get; set; }

        #endregion
    }
}
