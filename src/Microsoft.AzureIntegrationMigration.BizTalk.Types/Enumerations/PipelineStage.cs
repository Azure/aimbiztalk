//-----------------------------------------------------------------------
// <copyright file="PipelineStage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Enum of pipeline stages.</summary>
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
    /// Enum of pipeline stages.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PipelineStage
    {
        /// <summary>
        /// Unknown pipeline stage.
        /// </summary>
        Unknown,

        /// <summary>
        /// Decode stage.
        /// </summary>
        Decode,

        /// <summary>
        /// Disassemble stage.
        /// </summary>
        Disassemble,

        /// <summary>
        /// Validate stage.
        /// </summary>
        Validate,

        /// <summary>
        /// Resolve party stage.
        /// </summary>
        ResolveParty,

        /// <summary>
        /// Encode stage.
        /// </summary>
        Encode,

        /// <summary>
        /// Assemble stage.
        /// </summary>
        Assemble,
    }
}
