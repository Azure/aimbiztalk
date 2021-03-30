// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="PromotedProperty.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a promoted property on a BizTalk Schema.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities
{
    /// <summary>
    /// Represents a promoted property on a BizTalk Schema.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class PromotedProperty
    {
        /// <summary>
        /// Gets or sets the promoted property XPath.
        /// </summary>
        public string XPath { get; set; }

        /// <summary>
        /// Gets or sets the type reference for the promoted property.
        /// </summary>
        public string PropertyType { get; set; }
    }
}
