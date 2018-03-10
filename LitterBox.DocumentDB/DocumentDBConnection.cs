// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentDBConnection.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   DocumentDBConnection.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.DocumentDB {
    using System.Net;
    using System.Threading.Tasks;

    using LitterBox.Interfaces;
    using LitterBox.JsonContractResolvers;

    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;

    using Newtonsoft.Json;

    /// <summary>
    ///     The Connection
    /// </summary>
    internal class DocumentDBConnection : IConnection {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DocumentDBConnection" /> class.
        /// </summary>
        /// <param name="configuration">configuration</param>
        public DocumentDBConnection(DocumentDBConfiguration configuration) {
            this._configuration = configuration;
        }

        /// <summary>
        ///     DocumentClientCache
        /// </summary>
        public DocumentClient Cache { get; set; }

        /// <summary>
        ///     Connection Config
        /// </summary>
        private DocumentDBConfiguration _configuration { get; }

        #region Internals

        /// <summary>
        ///     Connect to MemoryCache
        /// </summary>
        /// <returns>Raw MemoryCache</returns>
        public async Task Connect() {
            await Task.Run(
                () => {
                    this.Cache = new DocumentClient(
                        this._configuration.EndpointURI,
                        this._configuration.PrimaryKey,
                        new JsonSerializerSettings {
                            ContractResolver = new CamelCaseExceptDictionaryKeysContractResolver()
                        },
                        this._configuration.ConnectionPolicy);
                }).ConfigureAwait(false);
        }

        /// <summary>
        ///     Close/Dispose Connection And Reconnect With Existing Properties
        /// </summary>
        /// <returns>
        ///     <see cref="Task" />
        /// </returns>
        public async Task Reconnect() {
            if (this.Cache != null) {
                this.Cache.Dispose();
            }

            await this.Connect().ConfigureAwait(false);
        }

        /// <summary>
        ///     Flush (Dispose) Of Cache / Recreate
        /// </summary>
        /// <returns>Success True|False</returns>
        public async Task<bool> Flush() {
            return await Task.Run(
                       async () => {
                           var uri = UriFactory.CreateStoredProcedureUri(this._configuration.Database, this._configuration.Collection, "Flush");
                           var requestOptions = new RequestOptions {
                               ConsistencyLevel = ConsistencyLevel.Eventual,
                               DisableRUPerMinuteUsage = true,
                               PartitionKey = new PartitionKey(this._configuration.PartitionKey)
                           };
                           var response = await this.Cache.ExecuteStoredProcedureAsync<dynamic>(uri, requestOptions);

                           return response?.StatusCode == HttpStatusCode.OK;
                       }).ConfigureAwait(false);
        }

        #endregion
    }
}