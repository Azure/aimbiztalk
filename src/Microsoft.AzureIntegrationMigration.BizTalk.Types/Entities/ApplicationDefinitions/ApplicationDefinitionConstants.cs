using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.ApplicationDefinitions
{
    /// <summary>
    /// Defines constants used in the ADF file.
    /// </summary>
    public static class ApplicationDefinitionConstants
    {
        #region Element Property Keys

        /// <summary>
        /// Defines a property key of the application name.
        /// </summary>
        public const string PropertyKeyDisplayName = "DisplayName";

        /// <summary>
        /// Defines a property key of the application description.
        /// </summary>
        public const string PropertyKeyDescription = "ApplicationDescription";

        #endregion

        #region References

        /// <summary>
        /// The name of the builtin system reference.
        /// </summary>
        public const string SystemReference = "BizTalk.System";

        #endregion
    }
}
