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

namespace LitterBox.Models {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LitterBox.Interfaces;

    /// <summary>
    /// Abstraction Around Multi-Level Caching
    /// </summary>
    public class Tenancy {
        /// <summary>
        /// Instiate A New Tenancy
        /// </summary>
        /// <param name="caches">Array Of Caches In Priority</param>
        public Tenancy(ILitterBox[] caches) {
            this._caches = caches;
        }

        /// <summary>
        /// Private Backing Of Cache Array
        /// </summary>
        private ILitterBox[] _caches { get; set; }

        #region Event Handlers

        /// <summary>
        /// Internal ExceptionEvent Invoker
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void RaiseException(Exception e) {
            this.ExceptionEvent?.Invoke(this, new ExceptionEvent(this, e));
        }

        #endregion

        #region ILitterBox Implementation

        #region Events

        /// <summary>
        /// EventHandler For ExceptionEvent
        /// </summary>
        public event EventHandler<ExceptionEvent> ExceptionEvent;

        #endregion

        #region Connection Based

        /// <summary>
        /// Close/Dispose Connection And Reconnect With Existing Properties
        /// </summary>
        /// <returns>Success True|False (For Each Cache)</returns>
        public async Task<List<TenancyResult<bool>>> Reconnect() {
            var results = new List<TenancyResult<bool>>();

            foreach (var cache in this._caches) {
                var success = await cache.Reconnect().ConfigureAwait(false);
                results.Add(new TenancyResult<bool> {
                    Type = cache.GetType(),
                    Result = success
                });
            }

            return results;
        }

        /// <summary>
        /// Flush Cache
        /// </summary>
        /// <returns>Success True|False (For Each Cache)</returns>
        public async Task<List<TenancyResult<bool>>> Flush() {
            var results = new List<TenancyResult<bool>>();

            foreach (var cache in this._caches) {
                var success = await cache.Flush().ConfigureAwait(false);
                results.Add(new TenancyResult<bool> {
                    Type = cache.GetType(),
                    Result = success
                });
            }

            return results;
        }

        #endregion

        #region Getters

        /// <summary>
        /// GetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <returns>TenancyResult => LitterBoxItem T</returns>
        public async Task<TenancyResult<LitterBoxItem<T>>> GetItem<T>(string key) {
            foreach (var cache in this._caches) {
                var result = await cache.GetItem<T>(key).ConfigureAwait(false);
                if (result != null) {
                    return new TenancyResult<LitterBoxItem<T>> {
                        Type = cache.GetType(),
                        Result = result
                    };
                }
            }
            return null;
        }

        /// <summary>
        /// GetItem T By Key, Generator, StaleIn, Expiry
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action If Not Found</param>
        /// <param name="staleIn">How Long After Creation To Be Considered "Good"</param>
        /// <param name="expiry">How Long After Creation To Auto-Delete</param>
        /// <returns>TenancyResult => LitterBoxItem T</returns>
        public async Task<TenancyResult<LitterBoxItem<T>>> GetItem<T>(string key, Func<T> generator, TimeSpan? staleIn = null, TimeSpan? expiry = null) {
            return await this.GetItem(key, async () => await Task.Run(generator).ConfigureAwait(false), staleIn, expiry).ConfigureAwait(false);
        }

        /// <summary>
        /// GetItem T By Key, Generator, StaleIn, Expiry
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action If Not Found</param>
        /// <param name="staleIn">How Long After Creation To Be Considered "Good"</param>
        /// <param name="expiry">How Long After Creation To Auto-Delete</param>
        /// <returns>TenancyResult => LitterBoxItem T</returns>
        public async Task<TenancyResult<LitterBoxItem<T>>> GetItem<T>(string key, Func<Task<T>> generator, TimeSpan? staleIn = null, TimeSpan? expiry = null) {
            var foundIndex = 0;
            LitterBoxItem<T> value = null;
            TenancyResult<LitterBoxItem<T>> result = null;

            for (var i = 0; i < this._caches.Length; i++) {
                var cache = this._caches[i];
                value = await cache.GetItem<T>(key).ConfigureAwait(false);
                if (value != null) {
                    foundIndex = i;
                    result = new TenancyResult<LitterBoxItem<T>> {
                        Type = cache.GetType(),
                        Result = value
                    };
                    break;
                }
            }

            if (result == null) {
                foundIndex = this._caches.Length;
                value = new LitterBoxItem<T> {
                    Expiry = expiry,
                    StaleIn = staleIn,
                    Value = await Task.Run(generator).ConfigureAwait(false)
                };
                result = new TenancyResult<LitterBoxItem<T>> {
                    Result = value
                };
            }

            for (var i = 0; i < foundIndex; i++) {
                var cache = this._caches[i];
                cache.SetItemFireAndForget(key, value);
            }

            return result;
        }

        /// <summary>
        /// GetItems T By Keys
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <returns>List TenancyResult => LitterBoxItem T</returns>
        public async Task<List<TenancyResult<LitterBoxItem<T>>>> GetItems<T>(List<string> keys) {
            var tasks = new List<Task<TenancyResult<LitterBoxItem<T>>>>();

            foreach (var key in keys) {
                tasks.Add(Task.Run(() => this.GetItem<T>(key)));
            }

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            return results.ToList();
        }

        /// <summary>
        /// GetItems T By Keys, Generators, StaleIn, Expiry
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="generators">Generator Actions If Not Found</param>
        /// <param name="staleIn">How Long After Creation To Be Considered "Good"</param>
        /// <param name="expiry">How Long After Creation To Auto-Delete</param>
        /// <returns>List TenancyResult => LitterBoxItem T</returns>
        public async Task<List<TenancyResult<LitterBoxItem<T>>>> GetItems<T>(List<string> keys, List<Func<T>> generators, TimeSpan? staleIn = null, TimeSpan? expiry = null) {
            return await this.GetItems(keys, generators.Select(generator => (Func<Task<T>>) (async () => await Task.Run(generator).ConfigureAwait(false))).ToList(), staleIn, expiry).ConfigureAwait(false);
        }

        /// <summary>
        /// GetItems T By Keys, Generators, StaleIn, Expiry
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="generators">Generator Actions If Not Found</param>
        /// <param name="staleIn">How Long After Creation To Be Considered "Good"</param>
        /// <param name="expiry">How Long After Creation To Auto-Delete</param>
        /// <returns>List TenancyResult => LitterBoxItem T</returns>
        public async Task<List<TenancyResult<LitterBoxItem<T>>>> GetItems<T>(List<string> keys, List<Func<Task<T>>> generators, TimeSpan? staleIn = null, TimeSpan? expiry = null) {
            if (keys.Count != generators.Count) {
                this.RaiseException(new Exception("Keys.Count/Generators.Count Must Be Equal"));
                return new List<TenancyResult<LitterBoxItem<T>>>();
            }

            var tasks = new List<Task<TenancyResult<LitterBoxItem<T>>>>();

            for (var i = 0; i < keys.Count; i++) {
                var key = keys[i];
                var generator = generators[i];
                tasks.Add(Task.Run(async () => await this.GetItem(key, generator, staleIn, expiry).ConfigureAwait(false)));
            }

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            return results.ToList();
        }

        #endregion

        #region Setters

        /// <summary>
        /// SetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="litter">Item T To Be Cached</param>
        /// <returns>Success True|False (For Each Cache)</returns>
        public async Task<List<TenancyResult<bool>>> SetItem<T>(string key, LitterBoxItem<T> litter) {
            var results = new List<TenancyResult<bool>>();

            foreach (var cache in this._caches) {
                var success = await cache.SetItem(key, litter).ConfigureAwait(false);
                results.Add(new TenancyResult<bool> {
                    Type = cache.GetType(),
                    Result = success
                });
            }

            return results;
        }

        /// <summary>
        /// SetItems T By Keys
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="litters">Items T To Be Cached</param>
        /// <returns>Success True|False (For Each Cache)</returns>
        public async Task<List<List<TenancyResult<bool>>>> SetItems<T>(List<string> keys, List<LitterBoxItem<T>> litters) {
            if (keys.Count != litters.Count) {
                this.RaiseException(new Exception("Keys.Count/LitterBoxItems.Count Must Be Equal"));
                return new List<List<TenancyResult<bool>>>();
            }

            var tasks = new List<Task<List<TenancyResult<bool>>>>();

            for (var i = 0; i < keys.Count; i++) {
                var key = keys[i];
                var litter = litters[i];
                tasks.Add(Task.Run(async () => await this.SetItem(key, litter).ConfigureAwait(false)));
            }

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            return results.ToList();
        }

        #endregion

        #region Fire Forget

        /// <summary>
        /// SetItem T (Fire Forget) By Key, LitterBoxItem T
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="litter">Item T To Be Cached</param>
        public void SetItemFireAndForget<T>(string key, LitterBoxItem<T> litter) {
            Task.Run(() => {
                foreach (var cache in this._caches) {
                    cache.SetItemFireAndForget(key, litter);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// SetItem T (Fire Forget) By Key, Generator, StaleIn, Expiry
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action</param>
        /// <param name="staleIn">How Long After Creation To Be Considered "Good"</param>
        /// <param name="expiry">How Long After Creation To Auto-Delete</param>
        public void SetItemFireAndForget<T>(string key, Func<T> generator, TimeSpan? staleIn = null, TimeSpan? expiry = null) {
            Task.Run(() => this.SetItemFireAndForget(key, () => Task.Run(generator), staleIn, expiry)).ConfigureAwait(false);
        }

        /// <summary>
        /// SetItem T (Fire Forget) By Key, Generator, StaleIn, Expiry
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action</param>
        /// <param name="staleIn">How Long After Creation To Be Considered "Good"</param>
        /// <param name="expiry">How Long After Creation To Auto-Delete</param>
        public void SetItemFireAndForget<T>(string key, Func<Task<T>> generator, TimeSpan? staleIn = null, TimeSpan? expiry = null) {
            Task.Run(() => {
                foreach (var cache in this._caches) {
                    cache.SetItemFireAndForget(key, generator, staleIn, expiry);
                }
            }).ConfigureAwait(false);
        }

        #endregion

        #endregion
    }
}