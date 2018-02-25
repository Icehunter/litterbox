// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CacheStore.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   CacheStore.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Memory {
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.Timers;

    using LitterBox.Memory.Models;

    /// <summary>
    ///     The CacheStore
    /// </summary>
    internal class CacheStore : IDisposable {
        /// <summary>
        ///     Private Storage
        /// </summary>
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="CacheStore" /> class.
        /// </summary>
        /// <param name="configuration">configuration</param>
        public CacheStore(MemoryConfiguration configuration) {
            this._configuration = configuration;
            this._expirationTimer = new Timer(this._configuration.ExpirationScanFrequency.TotalMilliseconds);
            this._expirationTimer.Elapsed += this.ExpirationTimerOnElapsed;
            this._expirationTimer.Start();
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="CacheStore" /> class.
        /// </summary>
        ~CacheStore() {
            this.Dispose();
        }

        /// <summary>
        ///     Cache Configuration
        /// </summary>
        private MemoryConfiguration _configuration { get; }

        /// <summary>
        ///     Private Scan Timer
        /// </summary>
        private Timer _expirationTimer { get; }

        #region IDisposable Implementation

        /// <summary>
        ///     Dispose Object
        /// </summary>
        public void Dispose() {
            this._expirationTimer.Stop();
            this._expirationTimer.Elapsed -= this.ExpirationTimerOnElapsed;
            this._cache.Clear();
        }

        #endregion

        /// <summary>
        ///     Flush Cache
        /// </summary>
        public void Flush() {
            this._cache.Clear();
        }

        /// <summary>
        ///     Set Value
        /// </summary>
        /// <param name="key">Cache Key</param>
        /// <param name="value">Cache Value</param>
        /// <param name="expiry">Cache Expiry</param>
        public void Set(string key, byte[] value, TimeSpan? expiry = null) {
            var cacheItem = new CacheItem {
                Expiry = this._configuration.DefaultTimeToLive,
                Value = value
            };
            if (expiry != null) {
                cacheItem.Expiry = (TimeSpan) expiry;
            }

            this._cache.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
        }

        /// <summary>
        ///     TryGet Value
        /// </summary>
        /// <param name="key">Cache Key</param>
        /// <param name="bytes">Out Cache Value</param>
        /// <returns>Success True|False</returns>
        public bool TryGetValue(string key, out byte[] bytes) {
            if (this._cache.TryGetValue(key, out var cacheItem)) {
                bytes = cacheItem.Value;
                return true;
            }

            bytes = null;
            return false;
        }

        /// <summary>
        ///     Expiration Timer Tick
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="elapsedEventArgs">Event Arguments</param>
        private void ExpirationTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            Task.Run(
                () => {
                    foreach (var item in this._cache) {
                        var cacheItem = item.Value;
                        if (DateTime.Now > cacheItem.Created.Add(cacheItem.Expiry)) {
                            this._cache.TryRemove(item.Key, out var removed);
                        }
                    }
                }).ConfigureAwait(false);
        }
    }
}