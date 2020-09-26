//-----------------------------------------------------------------------
// <copyright file="PipelineStage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Enum of pipeline directions.</summary>
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
    /// Enum of pipeline directions.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PipelineDirection
    {
        /// <summary>
        /// Unknown pipeline direction.
        /// </summary>
        Unknown,

        /// <summary>
        /// Receive pipeline direction.
        /// </summary>
        Receive,

        /// <summary>
        /// Send pipeline direction.
        /// </summary>
        Send,
    }
}
