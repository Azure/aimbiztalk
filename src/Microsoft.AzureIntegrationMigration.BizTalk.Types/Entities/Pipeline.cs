//-----------------------------------------------------------------------
// <copyright file="Pipeline.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a BizTalk pipeline.</summary>
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
    using System.Xml.Serialization;
    using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
    using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
    using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
    using Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces;
    using Newtonsoft.Json;

    #endregion

    /// <summary>
    /// Represents a BizTalk pipeline.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class Pipeline : CompiledComponent, ISourceResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pipeline"/> class.
        /// </summary>
        public Pipeline()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pipeline"/> class with related resource container and definition name.
        /// </summary>
        /// <param name="resourceContainerKey">The key of the resource container for this pipeline.</param>
        /// <param name="resourceDefinitionKey">The name of the resource definition key containing the content of this pipeline.</param>
        public Pipeline(string resourceContainerKey, string resourceDefinitionKey)
        {
            ResourceContainerKey = resourceContainerKey ?? throw new ArgumentNullException(nameof(resourceContainerKey));
            ResourceDefinitionKey = resourceDefinitionKey ?? throw new ArgumentNullException(nameof(resourceDefinitionKey));
        }

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the resource container key of the pipeline.
        /// </summary>
        public string ResourceContainerKey { get; set; }

        /// <summary>
        /// Gets or sets the resource definition key of the pipeline.
        /// </summary>
        public string ResourceDefinitionKey { get; set; }

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

        /// <summary>
        /// Gets or sets the description for the pipeline.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the typed <see cref="Document"/> which represents the pipeline.
        /// </summary>
        public Document Document { get; set; }

        /// <summary>
        /// Gets or sets the direction of the pipeline.
        /// </summary>
        public PipelineDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="PipelineComponent"/> instances that make up this pipeline.
        /// </summary>
        public List<PipelineComponent> Components { get; } = new List<PipelineComponent>();
       
        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Gets an enumerable list of <see cref="PipelineComponent"/> instances which have the given stage.
        /// </summary>
        /// <param name="stage"><see cref="PipelineStage"/> to get components for.</param>
        /// <returns>Enumerable list of <see cref="PipelineComponent"/> instances.</returns>
        public IEnumerable<PipelineComponent> GetComponentsByStage(PipelineStage stage)
        {
            return Components.Where(pc => pc.Stage == stage);
        }

        #endregion
    }
}
