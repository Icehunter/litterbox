// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RedisConnection.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   RedisConnection.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Redis {
    using System.Threading.Tasks;

    using LitterBox.Interfaces;

    using StackExchange.Redis;

    /// <summary>
    ///     The Connection
    /// </summary>
    internal class RedisConnection : IConnection {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RedisConnection" /> class.
        /// </summary>
        /// <param name="configuration">configuration</param>
        public RedisConnection(RedisConfiguration configuration) {
            this._configuration = configuration;
        }

        /// <summary>
        ///     Database (Redis)
        /// </summary>
        public IDatabase Cache { get; set; }

        /// <summary>
        ///     Connection Config
        /// </summary>
        private RedisConfiguration _configuration { get; }

        #region IConnection Implementation

        /// <summary>
        ///     Connect To Redis
        /// </summary>
        /// <returns>Raw Database</returns>
        public async Task Connect() {
            var multiplexer = await ConnectionMultiplexer.ConnectAsync(
                                  new ConfigurationOptions {
                                      AbortOnConnectFail = false,
                                      DefaultDatabase = this._configuration.DatabaseID,
                                      EndPoints = {
                                          {
                                              this._configuration.Host, this._configuration.Port
                                          }
                                      },
                                      KeepAlive = 30,
                                      Password = this._configuration.Password,
                                      Ssl = this._configuration.UseSSL
                                  }).ConfigureAwait(false);

            this.Cache = multiplexer.GetDatabase(this._configuration.DatabaseID);
        }

        /// <summary>
        ///     Close/Dispose Connection And Reconnect With Existing Properties
        /// </summary>
        /// <returns>
        ///     <see cref="Task" />
        /// </returns>
        public async Task Reconnect() {
            if (this.Cache != null) {
                await this.Cache.Multiplexer.CloseAsync().ConfigureAwait(false);
                this.Cache.Multiplexer.Dispose();
            }

            await this.Connect().ConfigureAwait(false);
        }

        /// <summary>
        ///     Flush Cache
        /// </summary>
        /// <returns>Success True|False</returns>
        public async Task<bool> Flush() {
            await Task.Run(() => this.Cache.Execute("FLUSHDB")).ConfigureAwait(false);
            return true;
        }

        #endregion
    }
}