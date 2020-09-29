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
using System.Xml;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.BizTalk.Parse.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Parse
{
    /// <summary>
    /// Describes the public behaviour of a parser.
    /// </summary>
    public abstract class BizTalkParserBase : IBizTalkParser
    {
        /// <summary>
        /// Defines the name of the parser.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the application model.
        /// </summary>
        private readonly AzureIntegrationServicesModel _model;

        /// <summary>
        /// Defines the context used to gather the migration information.
        /// </summary>
        private readonly MigrationContext _context;

        /// <summary>
        /// Creates a new instance of the <see cref="BizTalkParserBase" class./>
        /// </summary>
        /// <param name="parserName">The name of the BizTalk parser.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>        
        /// <param name="logger">A logger.</param>
        public BizTalkParserBase(string parserName, IApplicationModel model, MigrationContext context, ILogger logger)
        {
            // Validate and set the members
            _name = parserName ?? throw new ArgumentNullException(nameof(parserName));            
            _model = (AzureIntegrationServicesModel)model ?? throw new ArgumentNullException(nameof(model));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Runs the parser.
        /// </summary>
        public virtual void Parse()
        {
            ParseInternal(_model, _context);
        }

        /// <summary>
        /// Gets the name of the BizTalk parser.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Performs the internal work of the parser after null checking.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context"></param>
        protected abstract void ParseInternal(AzureIntegrationServicesModel model, MigrationContext context);

        /// <summary>
        /// Gets the name attribute from an <see cref="XmlNode"/> instance.
        /// </summary>
        /// <param name="node"><see cref="XmlNode"/> instance to get Name value for.</param>
        /// <returns>Value found.</returns>
        protected static string GetNodeNameAttribute(XmlNode node)
        {
            return GetAttributeValue(node, "Name");
        }

        /// <summary>
        /// Gets the value of a named attribute from an <see cref="XmlNode"/> instance.
        /// </summary>
        /// <param name="node">An <see cref="XmlNode"/> instance.</param>
        /// <param name="attributeName">Name of attribute to get value for.</param>
        /// <returns>Value of attribute, or null if no attribute found.</returns>
        protected static string GetAttributeValue(XmlNode node, string attributeName)
        {
            return GetAttributeValue(node, attributeName, null);
        }

        /// <summary>
        /// Gets the value of a named attribute from an <see cref="XmlNode"/> instance.
        /// </summary>
        /// <param name="node">An <see cref="XmlNode"/> instance.</param>
        /// <param name="attributeName">Name of attribute to get value for.</param>
        /// <param name="alternateAttributeName">Alternate attribute name to use if prime attribute name can't be found.</param>
        /// <returns>Value of attribute, or null if no attribute found.</returns>
        protected static string GetAttributeValue(XmlNode node, string attributeName, string alternateAttributeName)
        {
            if (node == null || string.IsNullOrWhiteSpace(attributeName))
            {
                return null;
            }

            var xmlAttribute = node.Attributes[attributeName];
            if (xmlAttribute == null)
            {
                if (string.IsNullOrWhiteSpace(alternateAttributeName))
                {
                    return null;
                }

                xmlAttribute = node.Attributes[alternateAttributeName];
            }

            if (xmlAttribute == null)
            {
                return null;
            }

            return xmlAttribute.Value;
        }
    }
}
