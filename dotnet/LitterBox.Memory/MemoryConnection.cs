// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemoryConnection.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   MemoryConnection.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Memory {
    using System.Threading.Tasks;

    using LitterBox.Interfaces;

    /// <summary>
    ///     The Connection
    /// </summary>
    internal class MemoryConnection : IConnection {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MemoryConnection" /> class.
        /// </summary>
        /// <param name="configuration">configuration</param>
        public MemoryConnection(MemoryConfiguration configuration) {
            this._configuration = configuration;
        }

        /// <summary>
        ///     MemoryCache
        /// </summary>
        public CacheStore Cache { get; set; }

        /// <summary>
        ///     Connection Config
        /// </summary>
        private MemoryConfiguration _configuration { get; }

        #region Internals

        /// <summary>
        ///     Connect to MemoryCache
        /// </summary>
        /// <returns>Raw MemoryCache</returns>
        public async Task Connect() {
            await Task.Run(
                () => {
                    this.Cache = new CacheStore(this._configuration);
                }).ConfigureAwait(false);
        }

        /// <summary>
        ///     Close/Dispose Connection And Reconnect With Existing Properties
        /// </summary>
        /// <returns>
        ///     <see cref="Task" />
        /// </returns>
        public async Task Reconnect() {
            // There is no "reconnecting" with memory caching
            await Task.FromResult(0);
        }

        /// <summary>
        ///     Flush (Dispose) Of Cache / Recreate
        /// </summary>
        /// <returns>Success True|False</returns>
        public async Task<bool> Flush() {
            await Task.Run(
                () => {
                    this.Cache.Flush();
                }).ConfigureAwait(false);
            return true;
        }

        #endregion
    }
}