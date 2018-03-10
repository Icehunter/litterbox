// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LitterBoxItem.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   LitterBoxItem.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Models {
    using System;

    using Newtonsoft.Json;

    /// <summary>
    ///     The litter box item.
    /// </summary>
    /// <typeparam name="T">Type T</typeparam>
    public class LitterBoxItem<T> {
        /// <summary>
        ///     Type Of Cache (This Is Only Present If The Value Returned Actually Came From A Cache)
        /// </summary>
        [JsonIgnore]
        public Type CacheType { get; set; }

        /// <summary>
        ///     Creation Time
        /// </summary>
        [JsonProperty(PropertyName = "created")]
        public DateTime Created { get; set; } = DateTime.Now;

        /// <summary>
        ///     Key In Cache (This Is Only Present If The Value Returned Actually Came From A Cache)
        /// </summary>
        [JsonIgnore]
        public string Key { get; set; }

        /// <summary>
        ///     Time (seconds) After Creation To Expire
        /// </summary>
        [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
        public int? TimeToLive { get; set; }

        /// <summary>
        ///     Time (seconds) After Creation To Be Stale/Need Refreshing
        /// </summary>
        [JsonProperty(PropertyName = "ttr", NullValueHandling = NullValueHandling.Ignore)]
        public int? TimeToRefresh { get; set; }

        /// <summary>
        ///     T Value Of Cached Item
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public T Value { get; set; }

        /// <summary>
        ///     Helper Function For Expiration
        /// </summary>
        /// <returns>True|False</returns>
        public bool IsExpired() {
            if (this.TimeToLive == null) {
                return false;
            }

            return DateTime.Now > this.Created.Add(TimeSpan.FromSeconds((double) this.TimeToLive));
        }

        /// <summary>
        ///     Helper Function For Expiration
        /// </summary>
        /// <returns>True|False</returns>
        public bool IsStale() {
            if (this.TimeToRefresh == null) {
                return false;
            }

            return DateTime.Now > this.Created.Add(TimeSpan.FromSeconds((double) this.TimeToRefresh));
        }
    }
}