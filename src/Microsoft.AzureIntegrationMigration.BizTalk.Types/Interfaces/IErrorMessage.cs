// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="IErrorMessage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Interface for error messages.</summary>
//-----------------------------------------------------------------------

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces
{
    #region Using Directives

    using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;

    #endregion

    /// <summary>
    /// Interface for error messages.
    /// </summary>
    public interface IErrorMessage
    {
        #region Properties

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        string Message { get; set; }

        /// <summary>
        /// Gets or sets the StackTrace for this error.
        /// </summary>
        string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the severity of the error.
        /// </summary>
        ErrorSeverity Severity { get; set; }

        #endregion
    }
}
