using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines
{
    /// <summary>
    /// Helper methods for the <see cref="Document"/> class.
    /// </summary>
    public partial class Document
    {
        /// <summary>
        /// Returns a <see cref="Document"/> object deserialized from XML.
        /// </summary>
        /// <param name="xml">The XML to create the Document from.</param>
        /// <returns>The <see cref="Document"/> object as defined in the supplied XML.</returns>
        public static Document FromXml(string xml)
        {
            Document document;

            var xmlSerializer = new XmlSerializer(typeof(Document));
            using (var r = new StringReader(xml))
            {
                document = (Document)xmlSerializer.Deserialize(r);
            }
            return document;
        }

        /// <summary>
        /// Finds the stage components.
        /// </summary>
        /// <param name="pipelineDocument">The pipeline document containing the stage components.</param>
        /// <returns>The stage components found, if any.</returns>
        public static IEnumerable<DocumentStageComponent> FindStageComponents(Document pipelineDocument)
        {
            return from stage in pipelineDocument.Stages
                   from component in stage.Components
                   where stage.Components != null
                   select component;
        }
    }
}
