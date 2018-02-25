// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CacheItem.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   CacheItem.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Memory {
    using System;

    /// <summary>
    ///     Memory CacheItem
    /// </summary>
    internal class CacheItem {
        /// <summary>
        ///     Insertion Time
        /// </summary>
        public DateTime Created { get; set; } = DateTime.Now;

        /// <summary>
        ///     Expiry Of Item (Expiration Scan)
        /// </summary>
        public TimeSpan Expiry { get; set; }

        /// <summary>
        ///     Cache Value (T LitterBoxItem Gzipped Bytes)
        /// </summary>
        public byte[] Value { get; set; }
    }
}