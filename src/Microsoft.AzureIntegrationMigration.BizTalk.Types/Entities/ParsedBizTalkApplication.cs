//-----------------------------------------------------------------------
// <copyright file="ParsedBizTalkApplication.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a parsed BizTalk Application.</summary>
//-----------------------------------------------------------------------

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities
{
    #region Using Directives

    using System;

    using Types.Interfaces;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Represents a parsed BizTalk Application.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class ParsedBizTalkApplication
    {
        #region Public Instance Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsedBizTalkApplication"/> class.
        /// </summary>
        public ParsedBizTalkApplication()
        {
            Id = Guid.NewGuid();
            Application = new BizTalkApplication();
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the unique id for this application.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the working folder for the application.
        /// </summary>
        public string WorkingFolder { get; set; }

        /// <summary>
        /// Gets or sets the Key of the resource container for the overall application.
        /// </summary>
        public string ResourceContainerKey { get; set; }

        /// <summary>
        /// Gets or sets the parsed <see cref="BizTalkApplication"/> instance.
        /// </summary>
        public BizTalkApplication Application { get; set; }

        #endregion
    }
}
