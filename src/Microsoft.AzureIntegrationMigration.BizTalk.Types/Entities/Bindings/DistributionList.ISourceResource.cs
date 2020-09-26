using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Filters;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces;
using Newtonsoft.Json;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings
{
    /// <summary>
    /// Extends the <see cref="DistributionList"/> class.
    /// </summary>
    public partial class DistributionList : ISourceResource
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

        /// <summary>
        /// Gets or sets the deserialized <see cref="FilterExpression"/> associated with the send port group.
        /// </summary>
        [XmlIgnore]
        public Filter FilterExpression { get; set; }
    }
}
