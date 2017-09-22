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

namespace LitterBox.Redis {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LitterBox.Interfaces;
    using LitterBox.Models;
    using LitterBox.Redis.Models;

    /// <summary>
    /// Memoize Class Instance
    /// </summary>
    public class Memoize : ILitterBox {
        /// <summary>
        /// Memoize
        /// </summary>
        /// <param name="config">config properties</param>
        private Memoize(Config config) {
            this._config = config ?? new Config();

            this._connectionPool = new ConnectionPool {
                PoolSize = this._config.PoolSize
            };
            // connect on a seperate thread; relative to poolSize
            Task.Run(async () => await this._connectionPool.Initialize(this._config)).Wait();
        }

        #region Instance

        /// <summary>
        /// Lazy Value
        /// </summary>
        public static Memoize GetInstance(Config config = null) => new Lazy<Memoize>(() => new Memoize(config)).Value;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Internal ExceptionEvent Invoker
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void RaiseException(Exception e) {
            this.ExceptionEvent?.Invoke(this, new ExceptionEvent(this, e));
        }

        #endregion

        #region Redis Properties

        private ConnectionPool _connectionPool { get; set; }

        /// <summary>
        /// RedisCache Config
        /// </summary>
        private Config _config { get; set; }

        /// <summary>
        /// Private Cache Of InProcess Items To Prevent Multiple Requests For The Same Object
        /// </summary>
        private ConcurrentDictionary<string, bool> _inProcess = new ConcurrentDictionary<string, bool>();

        #endregion

        #region ILitterBox Implementation

        #region Events

        /// <summary>
        /// EventHandler For ExceptionEvent
        /// </summary>
        public event EventHandler<ExceptionEvent> ExceptionEvent = delegate { };

        #endregion

        #region Connection Based

        /// <summary>
        /// Close/Dispose Connection And Reconnect With Existing Properties
        /// </summary>
        /// <returns>Success True|False</returns>
        public async Task<bool> Reconnect() {
            this._inProcess.Clear();
            try {
                foreach (var connection in this._connectionPool.Connections) {
                    await connection.Reconnect().ConfigureAwait(false);
                }
                return true;
            }
            catch (Exception ex) {
                this.RaiseException(ex);
            }
            return false;
        }

        /// <summary>
        /// Flush Cache
        /// </summary>
        /// <returns>Success True|False</returns>
        public async Task<bool> Flush() {
            this._inProcess.Clear();
            try {
                return await this._connectionPool.Connections.First().Flush().ConfigureAwait(false);
            }
            catch (Exception ex) {
                this.RaiseException(ex);
            }
            return false;
        }

        #endregion

        #region Getters

        /// <summary>
        /// GetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <returns>LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>> GetItem<T>(string key) {
            LitterBoxItem<T> result = null;

            try {
                var litterValue = await this._connectionPool.GetPooledConnection().Cache.HashGetAsync(key, "litter").ConfigureAwait(false);
                if (litterValue.HasValue) {
                    result = Utilities.Deserialize<LitterBoxItem<T>>(Compression.Unzip(litterValue));
                }
            }
            catch (Exception ex) {
                this.RaiseException(ex);
            }

            return result;
        }

        /// <summary>
        /// GetItem T By Key, Generator, TimeToRefresh, TimeToLive
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action If Not Found</param>
        /// <param name="timeToRefresh">How Long After Creation To Be Considered "Good"</param>
        /// <param name="timeToLive">How Long After Creation To Auto-Delete</param>
        /// <returns>LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>> GetItem<T>(string key, Func<T> generator, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null) {
            return await this.GetItem(key, async () => await Task.Run(generator).ConfigureAwait(false), timeToRefresh, timeToLive).ConfigureAwait(false);
        }

        /// <summary>
        /// GetItem T By Key, Generator, TimeToRefresh, TimeToLive
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action If Not Found</param>
        /// <param name="timeToRefresh">How Long After Creation To Be Considered "Good"</param>
        /// <param name="timeToLive">How Long After Creation To Auto-Delete</param>
        /// <returns>LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>> GetItem<T>(string key, Func<Task<T>> generator, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null) {
            var result = await this.GetItem<T>(key).ConfigureAwait(false);

            int? toLive = null;
            int? toRefresh = null;
            if (timeToLive != null) {
                toLive = ((TimeSpan) timeToLive).Seconds;
            }
            if (timeToRefresh != null) {
                toRefresh = ((TimeSpan) timeToRefresh).Seconds;
            }

            try {
                if (result != null) {
                    if (result.IsStale() || result.IsExpired()) {
                        this.SetItemFireAndForget(key, generator, timeToRefresh, timeToLive);
                    }
                }
                else {
                    var item = await Task.Run(generator).ConfigureAwait(false);
                    if (item != null) {
                        result = new LitterBoxItem<T> {
                            Key = key,
                            Value = item,
                            TimeToLive = toLive,
                            TimeToRefresh = toRefresh
                        };
                        this.SetItemFireAndForget(key, result);
                    }
                }
            }
            catch (Exception ex) {
                this.RaiseException(ex);
                var item = await Task.Run(generator).ConfigureAwait(false);
                if (item != null) {
                    result = new LitterBoxItem<T> {
                        Key = key,
                        Value = item,
                        TimeToLive = toLive,
                        TimeToRefresh = toRefresh
                    };
                }
            }

            return result;
        }

        /// <summary>
        /// GetItems T By Keys
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <returns>List LitterBoxItem T</returns>
        public async Task<List<LitterBoxItem<T>>> GetItems<T>(List<string> keys) {
            var tasks = new List<Task<LitterBoxItem<T>>>();

            foreach (var key in keys) {
                tasks.Add(Task.Run(() => this.GetItem<T>(key)));
            }

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            return results.ToList();
        }

        /// <summary>
        /// GetItems T By Keys, Generators, TimeToRefresh, TimeToLive
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="generators">Generator Actions If Not Found</param>
        /// <param name="timeToRefresh">How Long After Creation To Be Considered "Good"</param>
        /// <param name="timeToLive">How Long After Creation To Auto-Delete</param>
        /// <returns>List LitterBoxItem T</returns>
        public async Task<List<LitterBoxItem<T>>> GetItems<T>(List<string> keys, List<Func<T>> generators, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null) {
            return await this.GetItems(keys, generators.Select(generator => (Func<Task<T>>) (async () => await Task.Run(generator).ConfigureAwait(false))).ToList(), timeToRefresh, timeToLive).ConfigureAwait(false);
        }

        /// <summary>
        /// GetItems T By Keys, Generators, TimeToRefresh, TimeToLive
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="generators">Generator Actions If Not Found</param>
        /// <param name="timeToRefresh">How Long After Creation To Be Considered "Good"</param>
        /// <param name="timeToLive">How Long After Creation To Auto-Delete</param>
        /// <returns>List LitterBoxItem T</returns>
        public async Task<List<LitterBoxItem<T>>> GetItems<T>(List<string> keys, List<Func<Task<T>>> generators, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null) {
            if (keys.Count != generators.Count) {
                this.RaiseException(new Exception("Keys.Count/Generators.Count Must Be Equal"));
                return new List<LitterBoxItem<T>>();
            }

            var tasks = new List<Task<LitterBoxItem<T>>>();

            for (var i = 0; i < keys.Count; i++) {
                var key = keys[i];
                var generator = generators[i];
                tasks.Add(Task.Run(async () => await this.GetItem(key, generator, timeToRefresh, timeToLive).ConfigureAwait(false)));
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
        /// <returns>Success True|False</returns>
        public async Task<bool> SetItem<T>(string key, LitterBoxItem<T> litter) {
            var success = false;

            litter.TimeToRefresh = litter.TimeToRefresh ?? (int) this._config.DefaultTimeToRefresh.TotalSeconds;
            litter.TimeToLive = litter.TimeToLive ?? (int) this._config.DefaultTimeToLive.TotalSeconds;

            try {
                var json = Utilities.Serialize(litter);
                if (!string.IsNullOrWhiteSpace(json)) {
                    var HashSetSuccess = await this._connectionPool.GetPooledConnection().Cache.HashSetAsync(key, "litter", Compression.Zip(json));
                    var KeyExpireSuccess = await this._connectionPool.GetPooledConnection().Cache.KeyExpireAsync(key, TimeSpan.FromSeconds((double) litter.TimeToLive));
                    success = HashSetSuccess & KeyExpireSuccess;
                }
            }
            catch (Exception ex) {
                this.RaiseException(ex);
            }

            this._inProcess.TryRemove(key, out bool removed);

            return success;
        }

        /// <summary>
        /// SetItems T By Keys
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="litters">Items T To Be Cached</param>
        /// <returns>Success True|False</returns>
        public async Task<List<bool>> SetItems<T>(List<string> keys, List<LitterBoxItem<T>> litters) {
            if (keys.Count != litters.Count) {
                this.RaiseException(new Exception("Keys.Count/LitterBoxItems.Count Must Be Equal"));
                return new List<bool>();
            }

            var tasks = new List<Task<bool>>();

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
            if (litter == null) {
                return;
            }
            Task.Run(async () => {
                if (this._inProcess.ContainsKey(key)) {
                    return;
                }
                this._inProcess.TryAdd(key, true);
                await this.SetItem(key, litter).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// SetItem T (Fire Forget) By Key, Generator, TimeToRefresh, TimeToLive
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
        /// SetItem T (Fire Forget) By Key, Generator, TimeToRefresh, TimeToLive
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action</param>
        /// <param name="timeToRefresh">How Long After Creation To Be Considered "Good"</param>
        /// <param name="timeToLive">How Long After Creation To Auto-Delete</param>
        public void SetItemFireAndForget<T>(string key, Func<Task<T>> generator, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null) {
            Task.Run(async () => {
                if (this._inProcess.ContainsKey(key)) {
                    return;
                }
                this._inProcess.TryAdd(key, true);

                int? toLive = null;
                int? toRefresh = null;
                if (timeToLive != null) {
                    toLive = ((TimeSpan) timeToLive).Seconds;
                }
                if (timeToRefresh != null) {
                    toRefresh = ((TimeSpan) timeToRefresh).Seconds;
                }

                try {
                    var item = await Task.Run(generator).ConfigureAwait(false);
                    if (item != null) {
                        var litter = new LitterBoxItem<T> {
                            Key = key,
                            Value = item,
                            TimeToLive = toLive,
                            TimeToRefresh = toRefresh
                        };
                        await this.SetItem(key, litter).ConfigureAwait(false);
                    }
                }
                catch (Exception ex) {
                    this.RaiseException(ex);
                }
            }).ConfigureAwait(false);
        }

        #endregion

        #endregion
    }
}