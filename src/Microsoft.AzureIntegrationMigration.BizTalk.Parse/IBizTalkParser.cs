// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//-----------------------------------------------------------------------
// <copyright file="IBizTalkParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Describes the public behaviour of a parser.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Describes the public behaviour of a parser.
    /// </summary>
    public interface IBizTalkParser
    {
        /// <summary>
        /// Parses the contents of the model.
        /// </summary>
        void Parse();
    }
}


