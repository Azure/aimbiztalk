// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="ComponentProperty.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents the properties of a pipeline component within the BizTalk bindings.</summary>
//-----------------------------------------------------------------------

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines
{
    /// <summary>
    /// Represents the properties of a pipeline component within the BizTalk bindings.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class ComponentProperty
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value type identifier of the property.
        /// </summary>
        public string ValueType { get; set; }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        public string Value { get; set; }
    }
}
