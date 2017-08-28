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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Memoize Class Instance
    /// </summary>
    public class Memoize : ILitterBox {
        /// <summary>
        /// Memoize
        /// </summary>
        /// <param name="config">config properties</param>
        /// <param name="poolSize">poolSize</param>
        private Memoize(Config config = null, int poolSize = 5) {
            this._config = config ?? new Config();
            this._poolSize = poolSize;

            // connect on a seperate thread; relative to poolSize
            Task.Run(async () => {
                for (var i = 0; i < this._poolSize; i++) {
                    var connection = new Connection(this._config);
                    await connection.Connect().ConfigureAwait(false);
                    this.Connections.Add(connection);
                }
            }).Wait();
        }

        /// <summary>
        /// Close/Dispose Connection And Reconnect With Existing Properties
        /// </summary>
        /// <returns>Success True|False</returns>
        public async Task<bool> Reconnect() {
            try {
                foreach (var connection in this.Connections) {
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
            try {
                return await this.Connections.First().Flush().ConfigureAwait(false);
            }
            catch (Exception ex) {
                this.RaiseException(ex);
            }
            return false;
        }

        #region Single Sets

        /// <summary>
        /// SetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="litter">Item T To Be Cached</param>
        /// <returns>Success True|False</returns>
        public async Task<bool> SetItem<T>(string key, LitterBoxItem<T> litter) {
            var success = false;

            try {
                var json = Utilities.Serialize(litter);
                if (!string.IsNullOrWhiteSpace(json)) {
                    success = await this.GetPooledConnection().Cache.HashSetAsync(key, "litter", Compression.Zip(json));
                }
            }
            catch (Exception ex) {
                this.RaiseException(ex);
            }

            return success;
        }

        #endregion

        #region Bulk Sets

        /// <summary>
        /// SetItems T By Keys
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="litters">Items T To Be Cached</param>
        /// <returns>Success True|False</returns>
        public async Task<List<bool>> SetItems<T>(List<string> keys, List<LitterBoxItem<T>> litters) {
            var results = new List<bool>(keys.Count);

            if (keys.Count != litters.Count) {
                this.RaiseException(new Exception("Keys.Count/LitterBoxItems.Count Must Be Equal"));
                return results;
            }

            for (var i = 0; i < keys.Count; i++) {
                var key = keys[i];
                var litter = litters[i];
                results.Add(await this.SetItem(key, litter).ConfigureAwait(false));
            }

            return results;
        }

        #endregion

        #region Instance

        /// <summary>
        /// Lazy Value
        /// </summary>
        public static Memoize GetInstance(Config config = null, int poolSize = 5) => new Lazy<Memoize>(() => new Memoize(config, poolSize)).Value;

        #endregion

        #region Single Gets

        /// <summary>
        /// GetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <returns>LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>> GetItem<T>(string key) {
            LitterBoxItem<T> result = null;

            try {
                var litterValue = await this.GetPooledConnection().Cache.HashGetAsync(key, "litter").ConfigureAwait(false);
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
        /// GetItem T By Key, Generator
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action If Not Found</param>
        /// <returns>LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>> GetItem<T>(string key, Func<T> generator) {
            return await this.GetItem(key, generator, Constants.DefaultStaleIn, Constants.DefaultExpiry).ConfigureAwait(false);
        }

        /// <summary>
        /// GetItem T By Key, Generator, StaleIn
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action If Not Found</param>
        /// <param name="staleIn">How Long After Creation To Be Considered "Good"</param>
        /// <returns>LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>> GetItem<T>(string key, Func<T> generator, TimeSpan staleIn) {
            return await this.GetItem(key, generator, staleIn, Constants.DefaultExpiry).ConfigureAwait(false);
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
        public async Task<LitterBoxItem<T>> GetItem<T>(string key, Func<T> generator, TimeSpan staleIn, TimeSpan expiry) {
            LitterBoxItem<T> result = null;

            try {
                var litterValue = await this.GetPooledConnection().Cache.HashGetAsync(key, "litter").ConfigureAwait(false);
                if (litterValue.IsNullOrEmpty) {
                    var item = await Task.Run(generator).ConfigureAwait(false);
                    result = new LitterBoxItem<T> {
                        Value = item,
                        Expiry = expiry,
                        StaleIn = staleIn
                    };
                    this.SetItemFireAndForget(key, result);
                }
                if (litterValue.HasValue) {
                    result = Utilities.Deserialize<LitterBoxItem<T>>(Compression.Unzip(litterValue));
                    if (result.IsStale() || result.IsExpired()) {
                        this.SetItemFireAndForget(key, generator, staleIn, expiry);
                    }
                }
            }
            catch (Exception ex) {
                this.RaiseException(ex);
                var item = await Task.Run(generator).ConfigureAwait(false);
                result = new LitterBoxItem<T> {
                    Value = item,
                    Expiry = expiry,
                    StaleIn = staleIn
                };
            }

            return result;
        }

        #endregion

        #region Bulk Gets

        /// <summary>
        /// GetItems T By Keys
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <returns>List LitterBoxItem T</returns>
        public async Task<List<LitterBoxItem<T>>> GetItems<T>(List<string> keys) {
            var results = new List<LitterBoxItem<T>>(keys.Count);
            foreach (var key in keys) {
                results.Add(await this.GetItem<T>(key).ConfigureAwait(false));
            }
            return results;
        }

        /// <summary>
        /// GetItems T By Keys, Generators
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="generators">Generator Actions If Not Found</param>
        /// <returns>List LitterBoxItem T</returns>
        public async Task<List<LitterBoxItem<T>>> GetItems<T>(List<string> keys, List<Func<T>> generators) {
            return await this.GetItems(keys, generators, Constants.DefaultStaleIn, Constants.DefaultExpiry).ConfigureAwait(false);
        }

        /// <summary>
        /// GetItems T By Keys, Generators, StaleIn
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="generators">Generator Actions If Not Found</param>
        /// <param name="staleIn">How Long After Creation To Be Considered "Good"</param>
        /// <returns>List LitterBoxItem T</returns>
        public async Task<List<LitterBoxItem<T>>> GetItems<T>(List<string> keys, List<Func<T>> generators, TimeSpan staleIn) {
            return await this.GetItems(keys, generators, staleIn, Constants.DefaultExpiry).ConfigureAwait(false);
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
        public async Task<List<LitterBoxItem<T>>> GetItems<T>(List<string> keys, List<Func<T>> generators, TimeSpan staleIn, TimeSpan expiry) {
            var results = new List<LitterBoxItem<T>>(keys.Count);

            if (keys.Count != generators.Count) {
                this.RaiseException(new Exception("Keys.Count/Generators.Count Must Be Equal"));
                return results;
            }

            for (var i = 0; i < keys.Count; i++) {
                var key = keys[i];
                var generator = generators[i];
                results.Add(await this.GetItem(key, generator, staleIn, expiry).ConfigureAwait(false));
            }

            return results;
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
            Task.Run(async () => { await this.SetItem(key, litter).ConfigureAwait(false); });
        }

        /// <summary>
        /// SetItem T (Fire Forget) By Key, Generator
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action</param>
        public void SetItemFireAndForget<T>(string key, Func<T> generator) {
            this.SetItemFireAndForget(key, generator, Constants.DefaultStaleIn, Constants.DefaultExpiry);
        }

        /// <summary>
        /// SetItem T (Fire Forget) By Key, Generator, StaleIn
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action</param>
        /// <param name="staleIn">How Long After Creation To Be Considered "Good"</param>
        public void SetItemFireAndForget<T>(string key, Func<T> generator, TimeSpan staleIn) {
            this.SetItemFireAndForget(key, generator, staleIn, Constants.DefaultExpiry);
        }

        /// <summary>
        /// SetItem T (Fire Forget) By Key, Generator, StaleIn, Expiry
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action</param>
        /// <param name="staleIn">How Long After Creation To Be Considered "Good"</param>
        /// <param name="expiry">How Long After Creation To Auto-Delete</param>
        public void SetItemFireAndForget<T>(string key, Func<T> generator, TimeSpan staleIn, TimeSpan expiry) {
            Task.Run(async () => {
                var item = await Task.Run(generator).ConfigureAwait(false);
                var litter = new LitterBoxItem<T> {
                    Value = item,
                    Expiry = expiry,
                    StaleIn = staleIn
                };
                await this.SetItem(key, litter).ConfigureAwait(false);
            });
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// EventHandler For ExceptionEvent
        /// </summary>
        public event EventHandler<ExceptionEvent> ExceptionEvent = delegate { };

        /// <summary>
        /// Internal ExceptionEvent Invoker
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void RaiseException(Exception e) {
            this.ExceptionEvent?.Invoke(this, new ExceptionEvent(this, e));
        }

        #endregion

        #region Redis Properties

        private Config _config { get; set; }

        private int _poolSize { get; set; }

        private uint _roundRobinCounter;

        private readonly object _counterLock = new object();

        private List<Connection> Connections { get; set; } = new List<Connection>();

        private Connection GetPooledConnection() {
            int index;
            lock (this._counterLock)
            {
                index = (int)(++this._roundRobinCounter % this._poolSize);
            }
            return this.Connections[index];
        }

        #endregion
    }
}