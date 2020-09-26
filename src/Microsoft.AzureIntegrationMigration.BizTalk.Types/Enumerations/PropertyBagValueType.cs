//-----------------------------------------------------------------------
// <copyright file="PropertyBagValueType..cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Enum of property bag value types.</summary>
//-----------------------------------------------------------------------

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations
{
    #region Using Directives

    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    #endregion

    /// <summary>
    /// Enum of property bag value types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PropertyBagValueType
    {
        /// <summary>
        /// Unknown value type.
        /// </summary>
        Unknown,

        /// <summary>
        /// String value.
        /// </summary>
        String,

        /// <summary>
        /// Boolean value.
        /// </summary>
        Boolean,

        /// <summary>
        /// Integer value.
        /// </summary>
        Integer,

        /// <summary>
        /// Long integer value.
        /// </summary>
        Long,

        /// <summary>
        /// Decimal value.
        /// </summary>
        Decimal,
    }
}
