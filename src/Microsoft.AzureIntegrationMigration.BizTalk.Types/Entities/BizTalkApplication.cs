// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="BizTalkApplication.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a BizTalk Application.</summary>
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
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Xsl;
    using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;

    #endregion

    /// <summary>
    /// Represents a BizTalk Application.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class BizTalkApplication
    {
        #region Public Instance Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BizTalkApplication"/> class.
        /// </summary>
        public BizTalkApplication()
        {
            Name = "(Unknown)";
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the name of the BizTalk application.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the keys of the resource containers that comprise the resources of this application.
        /// </summary>
        public List<string> ResourceContainerKeys { get; } = new List<string>();

        /// <summary>
        /// Gets the assemblies associated with the application.
        /// </summary>
        public List<AssemblyFile> Assemblies { get; } = new List<AssemblyFile>();

        /// <summary>
        /// Gets or sets the binding information for the application.
        /// </summary>
        public BindingFile Bindings { get; set; }

        /// <summary>
        /// Gets or sets the application definition information for the application.
        /// </summary>
        public ApplicationDefinitionFile ApplicationDefinition { get; set; }

        /// <summary>
        /// Gets or sets a list of <see cref="Orchestration"/> instances.
        /// </summary>
        public List<Orchestration> Orchestrations { get; } = new List<Orchestration>();

        /// <summary>
        /// Gets or sets a list of <see cref="Schema"/> instances
        /// </summary>
        public List<Schema> Schemas { get; } = new List<Schema>();

        /// <summary>
        /// Gets or sets a list of <see cref="Transform"/> instances.
        /// </summary>
        public List<Transform> Transforms { get; } = new List<Transform>();

        /// <summary>
        /// Gets or sets a list of <see cref="Pipeline"/> instances.
        /// </summary>
        public List<Pipeline> Pipelines { get; } = new List<Pipeline>();

        #endregion
    }
}
