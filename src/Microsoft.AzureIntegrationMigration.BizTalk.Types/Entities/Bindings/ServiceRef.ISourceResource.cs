using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces;
using Newtonsoft.Json;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings
{
    /// <summary>
    /// Partial class that extends a service ref (orchestration binding).
    /// </summary>
    public partial class ServiceRef : ISourceResource
    {
        #region ISourceResource Implementation

        /// <summary>
        /// A pointer to the associated resource created from this object.
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        public ResourceItem Resource { get; set; }

        /// <summary>
        /// The resource key for the associated resource.
        /// </summary>
        [XmlIgnore]
        public string ResourceKey { get; set; }

        #endregion
    }
}
