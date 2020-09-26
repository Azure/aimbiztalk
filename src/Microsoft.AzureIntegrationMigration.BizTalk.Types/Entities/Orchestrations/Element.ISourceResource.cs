using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces;
using Newtonsoft.Json;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations
{
    /// <summary>
    /// Represents an element in an ODX metamodel.
    /// </summary>
    public partial class Element : ISourceResource
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
