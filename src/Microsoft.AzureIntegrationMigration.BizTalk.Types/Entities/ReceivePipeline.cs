// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="ReceivePipeline.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a BizTalk Receive Pipeline.</summary>
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
    using System.Xml.Schema;
    using System.Xml.Xsl;

    #endregion

    /// <summary>
    /// Represents a BizTalk Receive Pipeline.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class ReceivePipeline : Pipeline
    {
        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the list of <see cref="PipelineComponent"/> instances for the Decode stage.
        /// </summary>
        public List<PipelineComponent> DecodeStageComponents
        {
            get
            {
                return Components.Where(c => c.Stage == Enumerations.PipelineStage.Decode).ToList();
            }
        }

        #endregion
    }
}
