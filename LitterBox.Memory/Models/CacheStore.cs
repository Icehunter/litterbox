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
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();

        public CacheStore(CacheStoreOptions cacheStoreOptions) {
            this._cacheStoreOptions = cacheStoreOptions;
            this._expirationTimer = new Timer(this._cacheStoreOptions.ExpirationScanFrequency.TotalMilliseconds);
            this._expirationTimer.Elapsed += this.ExpirationTimerOnElapsed;
            this._expirationTimer.Start();
        }

        private Timer _expirationTimer { get; set; }

        private CacheStoreOptions _cacheStoreOptions { get; set; }

        #region IDisposable Implementation

        public void Dispose() {
            this._expirationTimer.Start();
            this._expirationTimer.Elapsed -= this.ExpirationTimerOnElapsed;
            this._cache.Clear();
        }

        #endregion

        ~CacheStore() {
            this.Dispose();
        }

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

        public void Flush() {
            this._cache.Clear();
        }

        public bool TryGetValue(string key, out byte[] bytes) {
            if (this._cache.TryGetValue(key, out var cacheItem)) {
                bytes = cacheItem.Value;
                return true;
            }
            bytes = null;
            return false;
        }

        public void Set(string key, byte[] value, TimeSpan expiry) {
            var cacheItem = new CacheItem {
                Expiry = expiry,
                Value = value
            };
            this._cache.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
        }
    }
}