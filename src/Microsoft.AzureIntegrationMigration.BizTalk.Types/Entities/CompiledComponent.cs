// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="CompiledComponent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a base component that is a compiled part of a BizTalk application.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities
{
    /// <summary>
    /// Represents a base component that is a compiled part of a BizTalk application.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public abstract class CompiledComponent
    {

        /// <summary>
        /// Gets or sets the full name of the schema.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the name of the schema.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the module containing the schema.
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// Gets or sets the namespace for the schema.
        /// </summary>
        public string Namespace { get; set; }
    }
}
