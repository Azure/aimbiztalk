// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert.Tests
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
