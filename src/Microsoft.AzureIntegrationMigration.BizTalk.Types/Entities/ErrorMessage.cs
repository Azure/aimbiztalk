//-----------------------------------------------------------------------
// <copyright file="ErrorMessage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a base error message.</summary>
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

    using Enumerations;
    using Interfaces;

    #endregion

    /// <summary>
    /// Represents a base error message.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class ErrorMessage : IErrorMessage
    {
        #region Public Instance Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        public ErrorMessage(Exception ex) : this(ex.Message, ex, ErrorSeverity.Error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        public ErrorMessage(string message) : this(message, ErrorSeverity.Error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="severity">Severity of error.</param>
        public ErrorMessage(string message, ErrorSeverity severity) : this(message, null, severity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="ex">Exception caught.</param>
        /// <param name="severity">Severity of error.</param>
        public ErrorMessage(string message, Exception ex, ErrorSeverity severity) : this()
        {
            this.Message = message;
            if (ex != null)
            {
                this.StackTrace = ex.StackTrace;
            }
            this.Severity = severity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        public ErrorMessage()
        {
        }

        #endregion

        #region Public Instance Properties

        #region IErrorMessage Implementation

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the StackTrace for this error.
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the severity of the error.
        /// </summary>
        public ErrorSeverity Severity { get; set; }

        #endregion

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Gets a string representation of this instance.
        /// </summary>
        /// <returns>String representation of this instance.</returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}", this.Severity, this.Message);
        }

        #endregion
    }
}
