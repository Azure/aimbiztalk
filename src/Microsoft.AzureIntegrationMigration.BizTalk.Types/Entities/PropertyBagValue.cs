// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="PropertyBagValue.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents the value for a property bag entry.</summary>
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

    using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;

    #endregion

    /// <summary>
    /// Represents the value for a property bag entry.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class PropertyBagValue
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBagValue"/> type.
        /// </summary>
        /// <param name="value">String value to use.</param>
        public PropertyBagValue(string value) : this(value, PropertyBagValueType.String)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBagValue"/> type.
        /// </summary>
        /// <param name="value">Value to use.</param>
        /// <param name="type">Type of value.</param>
        public PropertyBagValue(string value, PropertyBagValueType type)
        {
            this.Value = value;
            this.Type = type;
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the type of stored value.
        /// </summary>
        public ValueType Type { get; set; }

        /// <summary>
        /// Gets or sets the actual value stored.
        /// </summary>
        public string Value { get; set; }

        #endregion
    }
}
