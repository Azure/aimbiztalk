// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="BizTalkSchemaType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Enum of schema types.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations
{
    /// <summary>
    /// Enum of schema types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BizTalkSchemaType
    {
        /// <summary>
        /// The schema type is unknown or hasn't been determined.
        /// </summary>
        Unknown, 

        /// <summary>
        /// This is a property schema.
        /// </summary>
        Property,

        /// <summary>
        /// This is a document schema.
        /// </summary>
        Document
    }
}
