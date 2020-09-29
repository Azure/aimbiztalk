// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities
{
    /// <summary>
    /// Defines extension methods for the <see cref="Orchestration"/> class.
    /// </summary>
    public static class OrchestrationExtensions
    {
        /// <summary>
        /// Finds the module for the orchestration.
        /// </summary>
        /// <param name="orchestration">The orchestration to search.</param>
        /// <returns>Module, if found.</returns>
        public static Element FindModule(this Orchestration orchestration)
        {
            return orchestration.Model.Element.SingleOrDefault(e => e.Type == MetaModelConstants.ElementTypeModule);
        }

        /// <summary>
        /// Finds the correlation types for the orchestration.
        /// </summary>
        /// <param name="orchestration">The orchestration to search.</param>
        /// <returns>Correlation types, if found.</returns>
        public static Element[] FindCorrelationTypes(this Orchestration orchestration)
        {
            return orchestration.Model.Element.SingleOrDefault(e => e.Type == MetaModelConstants.ElementTypeModule)?.Element1?.Where(m => m.Type == MetaModelConstants.ElementTypeCorrelationType).ToArray();                    
        }

        /// <summary>
        /// Finds the port types for the orchestration.
        /// </summary>
        /// <param name="orchestration">The orchestration to search.</param>
        /// <returns>Port types, if found.</returns>
        public static Element[] FindPortTypes(this Orchestration orchestration)
        {
            return orchestration.Model.Element.SingleOrDefault(e => e.Type == MetaModelConstants.ElementTypeModule)?.Element1?.Where(m => m.Type == MetaModelConstants.ElementTypePortType).ToArray();
        }

        /// <summary>
        /// Finds the service link types for the orchestration.
        /// </summary>
        /// <param name="orchestration">The orchestration to search.</param>
        /// <returns>Port types, if found.</returns>
        public static Element[] FindServiceLinkTypes(this Orchestration orchestration)
        {
            return orchestration.Model.Element.SingleOrDefault(e => e.Type == MetaModelConstants.ElementTypeModule)?.Element1?.Where(m => m.Type == MetaModelConstants.ElementTypeServiceLinkType).ToArray();
        }

        /// <summary>
        /// Finds the multipart message types for the orchestration.
        /// </summary>
        /// <param name="orchestration">The orchestration to search.</param>
        /// <returns>Multipart message tyopes, if found.</returns>
        public static Element[] FindMultiPartMessageTypes(this Orchestration orchestration)
        {
            return orchestration.Model.Element.SingleOrDefault(e => e.Type == MetaModelConstants.ElementTypeModule)?.Element1?.Where(m => m.Type == MetaModelConstants.ElementTypeMultipartMessageType).ToArray();
        }

        /// <summary>
        /// Finds the service declaration for the orchestration.
        /// </summary>
        /// <param name="orchestration">The orchestration to search.</param>
        /// <returns>Service declaration, if found.</returns>
        public static Element FindServiceDeclaration(this Orchestration orchestration)
        {
            return orchestration.Model.Element.SingleOrDefault(e => e.Type == MetaModelConstants.ElementTypeModule)?.Element1?.SingleOrDefault(s => s.Type == MetaModelConstants.ElementTypeServiceDeclaration);
        }

        /// <summary>
        /// Finds the message declarations for the orchestration.
        /// </summary>
        /// <param name="orchestration">The orchestration to search.</param>
        /// <returns>Message declarations, if found.</returns>
        public static Element[] FindMessageDeclarations(this Orchestration orchestration)
        {
            return orchestration.Model.Element.SingleOrDefault(e => e.Type == MetaModelConstants.ElementTypeModule)
                ?.Element1?.SingleOrDefault(s => s.Type == MetaModelConstants.ElementTypeServiceDeclaration)
                ?.Element1?.Where(ms => ms.Type == MetaModelConstants.ElementTypeMessageDeclaration).ToArray();
        }

        /// <summary>
        /// Finds the correlation declarations for the orchestration.
        /// </summary>
        /// <param name="orchestration">The orchestration to search.</param>
        /// <returns>Correlation declarations, if found.</returns>
        public static Element[] FindCorrelationDeclarations(this Orchestration orchestration)
        {
            return orchestration.Model.Element.SingleOrDefault(e => e.Type == MetaModelConstants.ElementTypeModule)
                ?.Element1.SingleOrDefault(s => s.Type == MetaModelConstants.ElementTypeServiceDeclaration)
                ?.Element1?.Where(ms => ms.Type == MetaModelConstants.ElementTypeCorrelationDeclaration).ToArray();
        }

        /// <summary>
        /// Finds the port declarations for the orchestration.
        /// </summary>
        /// <param name="orchestration">The orchestration to search.</param>
        /// <returns>Port declarations, if found.</returns>
        public static Element[] FindPortDeclarations(this Orchestration orchestration)
        {
            return orchestration.Model.Element.SingleOrDefault(e => e.Type == MetaModelConstants.ElementTypeModule)
                ?.Element1?.SingleOrDefault(s => s.Type == MetaModelConstants.ElementTypeServiceDeclaration)
                ?.Element1?.Where(ms => ms.Type == MetaModelConstants.ElementTypePortDeclaration).ToArray();
        }

        /// <summary>
        /// Finds the transform elements in the orchestration.
        /// </summary>
        /// <param name="orchestration">The orchestration to search.</param>
        /// <returns>Transforms, if found.</returns>
        public static Element[] FindTransforms(this Orchestration orchestration)
        {
            // Get the service declaration.
            var serviceDeclaration = orchestration.FindServiceDeclaration();
            return serviceDeclaration.FindTransforms();
        }

        /// <summary>
        /// Finds the transform elements nested within the element.
        /// </summary>
        /// <param name="element">The element to search within.</param>
        /// <returns>Transforms, if found.</returns>
        public static Element[] FindTransforms(this Element element)
        {
            if (element == null)
            {
                return null;
            }
            else
            {
                var elements = FlattenElements(element);
                return elements.Where(e => e.Type == MetaModelConstants.ElementTypeTransform).ToArray();
            }
        }

        /// <summary>
        /// Recursively walks the element hierachy.
        /// </summary>
        /// <param name="element">The element to search.</param>
        /// <returns>The child elements found.</returns>
        private static IEnumerable<Element> FlattenElements(Element element)
        {
            yield return element;
            if (element.Element1 != null)
            {
                foreach(var childElement in element.Element1)
                {
                    foreach(var grandChildElement in FlattenElements(childElement))
                    {
                        yield return grandChildElement;
                    }
                }
            }
        }
    }
}
