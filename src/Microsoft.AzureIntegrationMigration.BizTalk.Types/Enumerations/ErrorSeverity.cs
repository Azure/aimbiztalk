// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="ErrorSeverity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Enum of Error Severities.</summary>
//-----------------------------------------------------------------------

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    #endregion

    /// <summary>
    /// Enum of Error Severities.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ErrorSeverity
    {
        /// <summary>
        /// Unknown severity.
        /// </summary>
        Unknown,

        /// <summary>
        /// Information only.
        /// </summary>
        Info,

        /// <summary>
        /// Warning.
        /// </summary>
        Warning,

        /// <summary>
        /// Error.
        /// </summary>
        Error,

        /// <summary>
        /// Critical error.
        /// </summary>
        Critical
    }
}
