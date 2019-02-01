// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CacheItem.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   CacheItem.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.DocumentDB {
    using LitterBox.Models;

    using Newtonsoft.Json;

    /// <summary>
    ///     The DocumentDB Item
    /// </summary>
    /// <typeparam name="T">Type T</typeparam>
    internal class CacheItem<T> : LitterBoxItem<T> {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CacheItem{T}" /> class.
        /// </summary>
        /// <param name="key">Key/ID Of Document</param>
        /// <param name="litter">Existing T LitterBoxItem</param>
        /// <param name="partitionKey">The default partition Key</param>
        public CacheItem(string key, LitterBoxItem<T> litter, string partitionKey = "none") {
            this.ID = key;
            this.Value = litter.Value;
            this.Created = litter.Created;
            this.TimeToLive = litter.TimeToLive;
            this.TimeToRefresh = litter.TimeToRefresh;
            this.PartitionKey = partitionKey;
        }

        /// <summary>
        ///     ID/Key Of Cached Item
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        /// <summary>
        ///     PartitionKey Of Cached Item
        /// </summary>
        [JsonProperty(PropertyName = "_partitionKey")]
        public string PartitionKey { get; set; }
    }
}