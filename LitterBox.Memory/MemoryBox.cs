// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemoryBox.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   MemoryBox.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Memory {
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using LitterBox.Interfaces;
    using LitterBox.Models;

    /// <summary>
    ///     MemoryBox Class Instance
    /// </summary>
    public class MemoryBox : ILitterBox {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MemoryBox" /> class.
        /// </summary>
        /// <param name="configuration">configuration</param>
        private MemoryBox(MemoryConfiguration configuration) {
            this._configuration = configuration ?? new MemoryConfiguration();

            this.Pool = new ConnectionPool {
                PoolSize = 1
            };

            var connection = new MemoryConnection(this._configuration);

            // connect on a seperate thread; relative to poolSize
            Task.Run(async () => await this.Pool.Initialize(connection).ConfigureAwait(false)).Wait();
        }

        #region Event Handlers

        /// <summary>
        ///     Internal ExceptionEvent Invoker
        /// </summary>
        /// <param name="ex">e</param>
        protected internal virtual void RaiseException(Exception ex) {
            this.ExceptionEvent?.Invoke(this, new ExceptionEvent(this, ex));
        }

        #endregion

        #region Instance

        /// <summary>
        ///     Backing For GetInstance
        /// </summary>
        private static Lazy<MemoryBox> _instance { get; set; }

        /// <summary>
        ///     Lazy Instance Value
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <returns>
        ///     <see cref="MemoryBox" />
        /// </returns>
        public static MemoryBox GetInstance(MemoryConfiguration configuration = null) {
            return (_instance ?? (_instance = new Lazy<MemoryBox>(() => new MemoryBox(configuration)))).Value;
        }

        #endregion

        #region Memory Properties

        /// <summary>
        ///     MemoryCache Config
        /// </summary>
        private MemoryConfiguration _configuration { get; }

        /// <summary>
        ///     Private Cache Of InProcess Items To Prevent Multiple Requests For The Same Object
        /// </summary>
        private readonly ConcurrentDictionary<string, bool> _inProcess = new ConcurrentDictionary<string, bool>();

        #endregion

        #region ILitterBox Implementation

        #region Events

        /// <summary>
        ///     EventHandler For ExceptionEvent
        /// </summary>
        public event EventHandler<ExceptionEvent> ExceptionEvent = (sender, args) => { };

        #endregion

        #region Connection Based

        /// <summary>
        ///     Close/Dispose Connection And Reconnect With Existing Properties
        /// </summary>
        /// <returns>Success True|False</returns>
        public async Task<bool> Reconnect() {
            this._inProcess.Clear();
            return await Task.Run(() => true).ConfigureAwait(false);
        }

        /// <summary>
        ///     Flush Cache
        /// </summary>
        /// <returns>Success True|False</returns>
        public async Task<bool> Flush() {
            this._inProcess.Clear();
            try {
                return await this.GetPooledConnection<MemoryConnection>().Flush().ConfigureAwait(false);
            }
            catch (Exception ex) {
                this.RaiseException(ex);
            }

            return false;
        }

        #endregion

        #region Getters

        /// <summary>
        ///     GetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <returns>LitterBoxItem T</returns>
        public async Task<LitterBoxItem<T>> GetItem<T>(string key) {
            LitterBoxItem<T> result = null;

            try {
                result = await Task.Run(
                             () => {
                                 if (this.GetPooledConnection<MemoryConnection>().Cache.TryGetValue(key, out var bytes)) {
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

        #endregion

        #region Connection Pools

        /// <summary>
        ///     Connection Objects
        /// </summary>
        public ConnectionPool Pool { get; set; }

        /// <summary>
        ///     Get Pooled Connection Of Type T
        /// </summary>
        /// <typeparam name="T">Type T</typeparam>
        /// <returns>T IConnection</returns>
        public T GetPooledConnection<T>() {
            var connection = this.Pool.GetPooledConnection();
            return connection is T variable
                       ? variable
                       : default(T);
        }

        #endregion

        #region Setters

        /// <summary>
        ///     SetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="original">Item T To Be Cached</param>
        /// <returns>Success True|False</returns>
        public async Task<bool> SetItem<T>(string key, LitterBoxItem<T> original) {
            var success = false;

            // when using multi-caching; modifying the TTR and TTL on the object collides with other caches
            // clone using JsonConvert
            var litter = Utilities.Clone(original);

            litter.TimeToRefresh = litter.TimeToRefresh ?? (int) this._configuration.DefaultTimeToRefresh.TotalSeconds;
            litter.TimeToLive = litter.TimeToLive ?? (int) this._configuration.DefaultTimeToLive.TotalSeconds;

            try {
                var json = Utilities.Serialize(litter);
                if (!string.IsNullOrWhiteSpace(json)) {
                    await Task.Run(() => this.GetPooledConnection<MemoryConnection>().Cache.Set(key, Compression.Zip(json), TimeSpan.FromSeconds((double) litter.TimeToLive))).ConfigureAwait(false);
                    success = true;
                }
            }
            catch (Exception ex) {
                this.RaiseException(ex);
            }

            this._inProcess.TryRemove(key, out var removed);

            return success;
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
            if (litter == null) {
                return;
            }

            Task.Run(
                async () => {
                    if (this._inProcess.ContainsKey(key)) {
                        return;
                    }

                    this._inProcess.TryAdd(key, true);
                    await this.SetItem(key, litter).ConfigureAwait(false);
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
        public void SetItemFireAndForget<T>(string key, Func<Task<T>> generator, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null) {
            Task.Run(
                async () => {
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