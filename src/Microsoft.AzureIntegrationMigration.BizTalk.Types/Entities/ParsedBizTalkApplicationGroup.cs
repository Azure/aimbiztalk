//-----------------------------------------------------------------------
// <copyright file="ParsedBizTalkApplicationGroup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a group of BizTalk Applications.</summary>
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
    using System.Xml.Xsl;
    using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings;

    #endregion

    /// <summary>
    /// Represents a group of BizTalk Applications.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class ParsedBizTalkApplicationGroup
    {
        #region Public Instance Properties

        /// <summary>
        /// Gets or sets a list of <see cref="ParsedBizTalkApplication"/> instances.
        /// </summary>
        public IList<ParsedBizTalkApplication> Applications { get; } = new List<ParsedBizTalkApplication>();

        #endregion
    }
}
