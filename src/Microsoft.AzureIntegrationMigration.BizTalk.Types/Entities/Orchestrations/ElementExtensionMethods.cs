using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations
{
    /// <summary>
    /// Extension methods for the <see cref="Element"/> class.
    /// </summary>
    public static class ElementExtensionMethods
    {
        /// <summary>
        /// Gets the value of the property using the key supplied.
        /// </summary>
        /// <param name="element">The element containing the property.</param>
        /// <param name="propertyKey">The key of the property.</param>
        /// <returns>The value of the name property in the element.</returns>
        public static string FindPropertyValue(this Element element, string propertyKey)
        {
            return element.Property?.FirstOrDefault(p => p.Name == propertyKey)?.Value;
        }
    }
}
