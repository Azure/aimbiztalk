//-----------------------------------------------------------------------
// <copyright file="IBizTalkDiscoverer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Defines the interface for a BizTalk Discoverer.</summary>
//-----------------------------------------------------------------------

namespace Microsoft.AzureIntegrationMigration.BizTalk.Discover
{
    /// <summary>
    /// Defines the interface for a BizTalk Discoverer.
    /// </summary>
    public interface IBizTalkDiscoverer
    {
        /// <summary>
        /// Discovers the contents of the model.
        /// </summary>
        void Discover();
    }
}
