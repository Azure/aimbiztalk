// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Types
{
    /// <summary>
    /// Defines extensions for the <see cref="Dictionary{TKey, TValue}"/> class.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Merges the specified dictionary into the current dictionary.
        /// </summary>
        /// <param name="mergeTo">The dictionary to merge into.</param>
        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> mergeTo, IDictionary<TKey, TValue> mergeFrom)
        {
            _ = mergeTo ?? throw new ArgumentNullException(nameof(mergeTo));

            foreach (var kvp in mergeFrom)
            {
                mergeTo.Add(kvp);
            }
        }
    }
}
