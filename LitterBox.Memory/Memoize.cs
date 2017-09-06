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

namespace LitterBox.Memory {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;

    public class Memoize : ILitterBox {
        /// <summary>
        /// Memoize
        /// </summary>
        /// <param name="config">config</param>
        private Memoize(Config config) {
            this._config = config ?? new Config();

            // connect on a seperate thread
            Task.Run(async () => {
                this._connection = new Connection(this._config);
                await this._connection.Connect().ConfigureAwait(false);
            }).Wait();
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

        #region Memory Properties

        /// <summary>
        /// Connection Object
        /// </summary>
        private Connection _connection { get; set; }

        /// <summary>
        /// MemoryCache Config
        /// </summary>
        private Config _config { get; set; }

        /// <summary>
        /// Private Cache Of InProcess Items To Prevent Multiple Requests For The Same Object
        /// </summary>
        private ConcurrentDictionary<string, bool> _inProcess = new ConcurrentDictionary<string, bool>();

        #endregion

        #region ILitterBox Implementation

        /// <summary>
        /// EventHandler For ExceptionEvent
        /// </summary>
        public event EventHandler<ExceptionEvent> ExceptionEvent = delegate { };

        /// <summary>
        /// Close/Dispose Connection And Reconnect With Existing Properties
        /// </summary>
        /// <returns>Success True|False</returns>
        public async Task<bool> Reconnect() {
            return await Task.Run(() => true).ConfigureAwait(false);
        }

        /// <summary>
        /// Flush Cache
        /// </summary>
        /// <returns>Success True|False</returns>
        public async Task<bool> Flush() {
            try {
                await this._connection.Flush().ConfigureAwait(false);
                return true;
            }
            catch (Exception ex) {
                this.RaiseException(ex);
            }
            return false;
        }

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
                result = await Task.Run(() => {
                    if (this._connection.Cache.TryGetValue(key, out byte[] bytes)) {
                        return Utilities.Deserialize<LitterBoxItem<T>>(Compression.Unzip(bytes));
                    }
                    return null;
                }).ConfigureAwait(false);
            }
            catch (Exception ex) {
                this.RaiseException(ex);
            }

            return result;
        }

        /// <summary>
        /// GetItem T By Key, Generator, StaleIn, Expiry
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action If Not Found</param>
        /// <param name="staleIn">How Long After Creation To Be Considered "Good"</param>
        /// <param name="expiry">How Long After Creation To Auto-Delete</param>
        /// <returns>LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>> GetItem<T>(string key, Func<T> generator, TimeSpan? staleIn = null, TimeSpan? expiry = null) {
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
        /// <returns>LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>> GetItem<T>(string key, Func<Task<T>> generator, TimeSpan? staleIn = null, TimeSpan? expiry = null) {
            LitterBoxItem<T> result = null;

            try {
                if (this._connection.Cache.TryGetValue(key, out byte[] bytes)) {
                    result = Utilities.Deserialize<LitterBoxItem<T>>(Compression.Unzip(bytes));
                    if (result.IsStale() || result.IsExpired()) {
                        this.SetItemFireAndForget(key, generator, staleIn, expiry);
                    }
                }
                else {
                    var item = await Task.Run(generator).ConfigureAwait(false);
                    if (item != null) {
                        result = new LitterBoxItem<T> {
                            Value = item,
                            Expiry = expiry,
                            StaleIn = staleIn
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
                        Value = item,
                        Expiry = expiry,
                        StaleIn = staleIn
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
                tasks.Add(Task.Run(async () => await this.GetItem<T>(key).ConfigureAwait(false)));
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
        /// <returns>List LitterBoxItem T</returns>
        public async Task<List<LitterBoxItem<T>>> GetItems<T>(List<string> keys, List<Func<T>> generators, TimeSpan? staleIn = null, TimeSpan? expiry = null) {
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
        /// <returns>List LitterBoxItem T</returns>
        public async Task<List<LitterBoxItem<T>>> GetItems<T>(List<string> keys, List<Func<Task<T>>> generators, TimeSpan? staleIn = null, TimeSpan? expiry = null) {
            if (keys.Count != generators.Count) {
                this.RaiseException(new Exception("Keys.Count/Generators.Count Must Be Equal"));
                return new List<LitterBoxItem<T>>();
            }

            var tasks = new List<Task<LitterBoxItem<T>>>();

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
        /// <returns>Success True|False</returns>
        public async Task<bool> SetItem<T>(string key, LitterBoxItem<T> litter) {
            var success = false;

            var staleIn = litter.StaleIn ?? this._config.DefaultStaleIn;
            var expiry = litter.Expiry ?? this._config.DefaultExpiry;

            litter.StaleIn = staleIn;
            litter.Expiry = expiry;

            try {
                var json = Utilities.Serialize(litter);
                if (!string.IsNullOrWhiteSpace(json)) {
                    await Task.Run(() => this._connection.Cache.Set(key, Compression.Zip(json), expiry)).ConfigureAwait(false);
                    success = true;
                }
            }
            catch (Exception ex) {
                this.RaiseException(ex);
            }

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
            Task.Run(async () => {
                if (this._inProcess.ContainsKey(key))
                {
                    return;
                }
                this._inProcess.TryAdd(key, true);
                await this.SetItem(key, litter).ConfigureAwait(false);
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
            Task.Run(async () => {
                if (this._inProcess.ContainsKey(key))
                {
                    return;
                }
                this._inProcess.TryAdd(key, true);

                try {
                    var item = await Task.Run(generator).ConfigureAwait(false);
                    if (item != null) {
                        var litter = new LitterBoxItem<T> {
                            Value = item,
                            Expiry = expiry,
                            StaleIn = staleIn
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