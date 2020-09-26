//-----------------------------------------------------------------------
// <copyright file="TestConstants.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Defines test constants such as traits and categories.</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Discover.Tests
{
    /// <summary>
    /// Defines test constants such as traits and categories.
    /// </summary>
    public static class TestConstants
    {
        #region Traits

        /// <summary>
        /// Defines a category trait that can be used to define the type of test.
        /// </summary>
        public const string TraitCategory = "Category";

        #endregion

        #region Categories

        /// <summary>
        /// Defines a category for unit tests.
        /// </summary>
        public const string CategoryUnitTest = "Unit";

        /// <summary>
        /// Defines a category for integration tests.
        /// </summary>
        public const string CategoryIntegrationTest = "Integration";

        /// <summary>
        /// Defines a category for acceptance tests.
        /// </summary>
        public const string CategoryAcceptanceTest = "Acceptance";

        /// <summary>
        /// Defines a category for smoke tests.
        /// </summary>
        public const string CategorySmokeTest = "Smoke";

        #endregion
    }
}
