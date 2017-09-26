// MIT License
// 
// Copyright(c) 2017 Ryan Wilson <syndicated.life@gmail.com> (http://syndicated.life/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace LitterBox.Memory.Models {
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.Timers;

    public class CacheStore : IDisposable {
        /// <summary>
        /// Private Storage
        /// </summary>
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();

        /// <summary>
        /// Create New CacheStore
        /// </summary>
        /// <param name="config"></param>
        public CacheStore(Config config) {
            this._config = config;
            this._expirationTimer = new Timer(this._config.ExpirationScanFrequency.TotalMilliseconds);
            this._expirationTimer.Elapsed += this.ExpirationTimerOnElapsed;
            this._expirationTimer.Start();
        }

        /// <summary>
        /// Cache Configuration
        /// </summary>
        private Config _config { get; }

        /// <summary>
        /// Private Scan Timer
        /// </summary>
        private Timer _expirationTimer { get; }

        #region IDisposable Implementation

        /// <summary>
        /// Dispose Object
        /// </summary>
        public void Dispose() {
            this._expirationTimer.Start();
            this._expirationTimer.Elapsed -= this.ExpirationTimerOnElapsed;
            this._cache.Clear();
        }

        #endregion

        /// <summary>
        /// Call Dispose
        /// </summary>
        ~CacheStore() {
            this.Dispose();
        }

        /// <summary>
        /// Expiration Timer Tick
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="elapsedEventArgs">Event Arguments</param>
        private void ExpirationTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            Task.Run(() => {
                foreach (var item in this._cache) {
                    var cacheItem = item.Value;
                    if (DateTime.Now > cacheItem.Created.Add(cacheItem.Expiry)) {
                        this._cache.TryRemove(item.Key, out var removed);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Flush Cache
        /// </summary>
        public void Flush() {
            this._cache.Clear();
        }

        /// <summary>
        /// TryGet Value
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
        /// Set Value
        /// </summary>
        /// <param name="key">Cache Key</param>
        /// <param name="value">Cache Value</param>
        /// <param name="expiry"></param>
        public void Set(string key, byte[] value, TimeSpan? expiry = null) {
            var cacheItem = new CacheItem {
                Expiry = this._config.DefaultTimeToLive,
                Value = value
            };
            if (expiry != null) {
                cacheItem.Expiry = (TimeSpan) expiry;
            }
            this._cache.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
        }
    }
}