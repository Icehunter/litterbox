// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tenancy.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   Tenancy.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using LitterBox.Interfaces;
    using LitterBox.Models;

    /// <summary>
    ///     Abstraction Around Multi-Level Caching
    /// </summary>
    public class Tenancy : ITenancy {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Tenancy" /> class.
        /// </summary>
        /// <param name="caches">Array Of Caches In Priority</param>
        public Tenancy(ILitterBox[] caches) {
            this._caches = caches;

            for (var i = 0; i < this._caches.Length; i++) {
                var cache = this._caches[i];
                cache.ExceptionEvent += this.CacheOnExceptionEvent;
            }
        }

        /// <summary>
        ///     Private Backing Of Cache Array
        /// </summary>
        private ILitterBox[] _caches { get; }

        #region Event Handlers

        /// <summary>
        ///     Internal ExceptionEvent Invoker
        /// </summary>
        /// <param name="e">e</param>
        protected internal virtual void RaiseException(Exception e) {
            this.ExceptionEvent?.Invoke(this, new ExceptionEvent(this, e));
        }

        /// <summary>
        ///     Handle Cache Exceptions
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void CacheOnExceptionEvent(object sender, ExceptionEvent e) {
            this.ExceptionEvent?.Invoke(sender, e);
        }

        #endregion

        #region ITenancy Implementation

        #region Events

        /// <summary>
        ///     EventHandler For ExceptionEvent
        /// </summary>
        public event EventHandler<ExceptionEvent> ExceptionEvent;

        #endregion

        #region Connection Based

        /// <summary>
        ///     Close/Dispose Connection And Reconnect With Existing Properties
        /// </summary>
        /// <returns>Success True|False (For Each Cache)</returns>
        public async Task<ReconnectionResult[]> Reconnect() {
            var results = new ReconnectionResult[this._caches.Length];

            for (var i = 0; i < this._caches.Length; i++) {
                var cache = this._caches[i];
                var success = await cache.Reconnect().ConfigureAwait(false);
                results[i] = new ReconnectionResult {
                    CacheType = cache.GetType(),
                    IsSuccessful = success
                };
            }

            return results;
        }

        /// <summary>
        ///     Flush Cache
        /// </summary>
        /// <returns>Success True|False (For Each Cache)</returns>
        public async Task<FlushResult[]> Flush() {
            var results = new FlushResult[this._caches.Length];

            for (var i = 0; i < this._caches.Length; i++) {
                var cache = this._caches[i];
                var success = await cache.Flush().ConfigureAwait(false);
                results[i] = new FlushResult {
                    CacheType = cache.GetType(),
                    IsSuccessful = success
                };
            }

            return results;
        }

        #endregion

        #region Getters

        /// <summary>
        ///     GetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <returns>TenancyItem => LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>> GetItem<T>(string key) {
            foreach (var cache in this._caches) {
                var result = await cache.GetItem<T>(key).ConfigureAwait(false);
                if (result != null) {
                    result.Key = key;
                    result.CacheType = cache.GetType();
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        ///     GetItem T By Key, Generator, TimeToRefresh, TimeToLive
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action If Not Found</param>
        /// <param name="timeToRefresh">How Long After Creation To Be Considered "Good"</param>
        /// <param name="timeToLive">How Long After Creation To Auto-Delete</param>
        /// <returns>TenancyItem => LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>> GetItem<T>(string key, Func<Task<T>> generator, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null) {
            var foundIndex = 0;
            LitterBoxItem<T> result = null;

            for (var i = 0; i < this._caches.Length; i++) {
                var cache = this._caches[i];
                result = await cache.GetItem<T>(key).ConfigureAwait(false);
                if (result != null) {
                    foundIndex = i;
                    result.Key = key;
                    result.CacheType = cache.GetType();
                    if (result.IsExpired() || result.IsStale()) {
                        cache.SetItemFireAndForget(key, generator, timeToRefresh, timeToLive);
                    }

                    break;
                }
            }

            int? toLive = null;
            int? toRefresh = null;
            if (timeToLive != null) {
                toLive = ((TimeSpan) timeToLive).Seconds;
            }

            if (timeToRefresh != null) {
                toRefresh = ((TimeSpan) timeToRefresh).Seconds;
            }

            if (result == null) {
                foundIndex = this._caches.Length;
                result = new LitterBoxItem<T> {
                    Value = await Task.Run(generator).ConfigureAwait(false),
                    TimeToLive = toLive,
                    TimeToRefresh = toRefresh
                };
            }

            for (var i = 0; i < foundIndex; i++) {
                var cache = this._caches[i];
                cache.SetItemFireAndForget(key, result);
            }

            return result;
        }

        /// <summary>
        ///     GetItems T By Keys
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <returns>List TenancyItem => LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>[]> GetItems<T>(string[] keys) {
            var tasks = keys.Select(key => Task.Run(async () => await this.GetItem<T>(key).ConfigureAwait(false)));

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        ///     GetItems T By Keys, Generators, TimeToRefresh, TimeToLive
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="generators">Generator Actions If Not Found</param>
        /// <param name="timeToRefresh">How Long After Creation To Be Considered "Good"</param>
        /// <param name="timeToLive">How Long After Creation To Auto-Delete</param>
        /// <returns>List TenancyItem => LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>[]> GetItems<T>(string[] keys, Func<Task<T>>[] generators, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null) {
            if (keys.Length != generators.Length) {
                this.RaiseException(new ArgumentException("Keys.Count/Generators.Count Must Be Equal"));
                return null;
            }

            var tasks = new List<Task<LitterBoxItem<T>>>();

            for (var i = 0; i < keys.Length; i++) {
                var key = keys[i];
                var generator = generators[i];
                tasks.Add(Task.Run(async () => await this.GetItem(key, generator, timeToRefresh, timeToLive).ConfigureAwait(false)));
            }

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        #endregion

        #region Setters

        /// <summary>
        ///     SetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="litter">Item T To Be Cached</param>
        /// <returns>Success True|False (For Each Cache)</returns>
        public async Task<StorageResult[]> SetItem<T>(string key, LitterBoxItem<T> litter) {
            var results = new StorageResult[this._caches.Length];

            for (var i = 0; i < this._caches.Length; i++) {
                var cache = this._caches[i];
                var success = await cache.SetItem(key, litter).ConfigureAwait(false);
                results[i] = new StorageResult {
                    CacheType = cache.GetType(),
                    IsSuccessful = success
                };
            }

            return results;
        }

        /// <summary>
        ///     SetItems T By Keys
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="litters">Items T To Be Cached</param>
        /// <returns>Success True|False (For Each Cache)</returns>
        public async Task<StorageResult[][]> SetItems<T>(string[] keys, LitterBoxItem<T>[] litters) {
            if (keys.Length != litters.Length) {
                this.RaiseException(new ArgumentException("Keys.Count/LitterBoxItems.Count Must Be Equal"));
                return null;
            }

            var tasks = new List<Task<StorageResult[]>>();

            for (var i = 0; i < keys.Length; i++) {
                var key = keys[i];
                var litter = litters[i];
                tasks.Add(Task.Run(async () => await this.SetItem(key, litter).ConfigureAwait(false)));
            }

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        #endregion

        #region Fire Forget

        /// <summary>
        ///     SetItem T (Fire Forget) By Key, LitterBoxItem T
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="litter">Item T To Be Cached</param>
        public void SetItemFireAndForget<T>(string key, LitterBoxItem<T> litter) {
            Task.Run(
                () => {
                    foreach (var cache in this._caches) {
                        cache.SetItemFireAndForget(key, litter);
                    }
                }).ConfigureAwait(false);
        }

        /// <summary>
        ///     SetItem T (Fire Forget) By Key, Generator, TimeToRefresh, TimeToLive
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action</param>
        /// <param name="timeToRefresh">How Long After Creation To Be Considered "Good"</param>
        /// <param name="timeToLive">How Long After Creation To Auto-Delete</param>
        public void SetItemFireAndForget<T>(string key, Func<T> generator, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null) {
            Task.Run(() => this.SetItemFireAndForget(key, () => Task.Run(generator), timeToRefresh, timeToLive)).ConfigureAwait(false);
        }

        /// <summary>
        ///     SetItem T (Fire Forget) By Key, Generator, TimeToRefresh, TimeToLive
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action</param>
        /// <param name="timeToRefresh">How Long After Creation To Be Considered "Good"</param>
        /// <param name="timeToLive">How Long After Creation To Auto-Delete</param>
        public void SetItemFireAndForget<T>(string key, Func<Task<T>> generator, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null) {
            Task.Run(
                () => {
                    foreach (var cache in this._caches) {
                        cache.SetItemFireAndForget(key, generator, timeToRefresh, timeToLive);
                    }
                }).ConfigureAwait(false);
        }

        #endregion

        #endregion
    }
}