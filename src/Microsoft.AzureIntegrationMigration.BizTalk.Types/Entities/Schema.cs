// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="Schema.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Represents a BizTalk Schema.</summary>
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
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;
    using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
    using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
    using Microsoft.AzureIntegrationMigration.BizTalk.Types.Interfaces;
    using Newtonsoft.Json;

    #endregion

    /// <summary>
    /// Represents a BizTalk Schema.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Serializable]
    public class Schema : CompiledComponent, ISourceResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class.
        /// </summary>
        public Schema()
        {
            SchemaType = BizTalkSchemaType.Unknown;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class with related resource container and definition name.
        /// </summary>
        /// <param name="resourceContainerKey">The key of the resource container for this schema.</param>
        /// <param name="resourceDefinitionKey">The key of the resource definition containing the content of this schema.</param>
        public Schema(string resourceContainerKey, string resourceDefinitionKey)
            : this()
        {
            ResourceContainerKey = resourceContainerKey ?? throw new ArgumentNullException(nameof(resourceContainerKey));
            ResourceDefinitionKey = resourceDefinitionKey ?? throw new ArgumentNullException(nameof(resourceDefinitionKey));
        }

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the resource container key of the schema.
        /// </summary>
        public string ResourceContainerKey { get; set; }

        /// <summary>
        /// Gets or sets the resource definition key of the schema.
        /// </summary>
        public string ResourceDefinitionKey { get; set; }

        #region ISourceResource Implementation

        /// <summary>
        /// A pointer to the associated resource created from this object.
        /// </summary>
        [JsonIgnore]
        public ResourceItem Resource { get; set; }

        /// <summary>
        /// Gets or sets the resource key of the application definition.
        /// </summary>
        public string ResourceKey { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the XML namespace for the schema.
        /// </summary>
        public string XmlNamespace { get; set; }

        /// <summary>
        /// Gets or sets the schema type.
        /// </summary>
        public BizTalkSchemaType SchemaType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the document schema is an envelope schema.
        /// </summary>
        public bool IsEnvelope { get; set; }

        /// <summary>
        /// Gets or sets the context properties.
        /// </summary>
        public List<ContextProperty> ContextProperties { get; private set; } = new List<ContextProperty>();

        /// <summary>
        /// Gets or sets the promoted properties.
        /// </summary>
        public List<PromotedProperty> PromotedProperties { get; private set; } = new List<PromotedProperty>();

        /// <summary>
        /// Gets or sets the distinguished fields.
        /// </summary>
        public List<DistinguishedField> DistinguishedFields { get; private set; } = new List<DistinguishedField>();

        /// <summary>
        /// Gets or sets the message definitions.
        /// </summary>
        public List<MessageDefinition> MessageDefinitions { get; private set; } = new List<MessageDefinition>();

        /// <summary>
        /// Gets or sets the reference to the schema.
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Gets or sets the name of the root node in the schema.
        /// </summary>
        public string RootNodeName { get; set; }

        /// <summary>
        /// Gets or sets the XPath to the body document.  Only valid for an Envelope schema.
        /// </summary>
        public string BodyXPath { get; set; }

        #endregion
    }
}
