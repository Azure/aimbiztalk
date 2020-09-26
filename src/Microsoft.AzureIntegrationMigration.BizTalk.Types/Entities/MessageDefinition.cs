using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces;
using Newtonsoft.Json;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities
{
    /// <summary>
    /// Defines a type of message, i.e. a combination of root element and namespace.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class MessageDefinition : ISourceResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDefinition"/> class.
        /// </summary>
        /// <param name="rootElementName">The root element of the message.</param>
        /// <param name="xmlNamespace">The XML namespace of the message.</param>
        /// <param name="fullName">The fully qualified type name of the message.</param>
        /// <param name="localName">The local class name of the message.</param>
        /// <param name="resourceKey">The resource key.</param>
        public MessageDefinition(string rootElementName, string xmlNamespace, string fullName, string localName, string resourceKey)
        {
            RootElementName = rootElementName;
            XmlNamespace = xmlNamespace;
            FullName = fullName;
            LocalName = localName;
            ResourceKey = resourceKey;
        }

        /// <summary>
        /// Gets or sets the root element name of the message.
        /// </summary>
        public string RootElementName { get; set; }

        /// <summary>
        /// Gets or sets the XML namespace of the element.
        /// </summary>
        public string XmlNamespace { get; set; }

        /// <summary>
        /// Gets or sets the full name of the message type.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the local name of the message type.
        /// </summary>
        public string LocalName { get; set; }

        /// <summary>
        /// Gets or sets the promoted properties.
        /// </summary>
        public List<PromotedProperty> PromotedProperties { get; private set; } = new List<PromotedProperty>();

        #region ISourceResource Implementation

        /// <summary>
        /// A pointer to the associated resource created from this object.
        /// </summary>
        [JsonIgnore]
        public ResourceItem Resource { get; set; }

        /// <summary>
        /// Gets or sets the resource key of the message definition.
        /// </summary>
        public string ResourceKey { get; set; }

        #endregion

        /// <summary>
        /// Gets the message type of the message.
        /// </summary>
        public string MessageType 
        { 
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}#{1}", XmlNamespace, RootElementName);
            }
        }
    }
}
