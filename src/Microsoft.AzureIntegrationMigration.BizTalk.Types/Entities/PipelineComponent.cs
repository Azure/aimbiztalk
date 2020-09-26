//-----------------------------------------------------------------------
// <copyright file="PipelineComponent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a BizTalk pipeline component.</summary>
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

    using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;

    #endregion

    /// <summary>
    /// Represents a BizTalk pipeline component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class PipelineComponent
    {
        #region Public Instance Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineComponent"/> class.
        /// </summary>
        public PipelineComponent()
        {
            PropertyBag = new Dictionary<string, PropertyBagValue>();
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the stage this component belongs to.
        /// </summary>
        public PipelineStage Stage { get; set; }

        /// <summary>
        /// Gets or sets the name of the original component.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the original component.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the original component.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the full type of the original component.
        /// </summary>
        public string FullType { get; set; }

        /// <summary>
        /// Gets or sets the version of the original component.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the property bag for this component.
        /// </summary>
        public IDictionary<string, PropertyBagValue> PropertyBag { get; }

        #endregion
    }
}
