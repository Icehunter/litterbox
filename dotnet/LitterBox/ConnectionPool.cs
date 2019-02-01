// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionPool.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   ConnectionPool.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using LitterBox.Interfaces;

    /// <summary>
    ///     The ConnectionPool
    /// </summary>
    public class ConnectionPool {
        /// <summary>
        ///     Counter Storages
        /// </summary>
        private int _roundRobinCounter;

        /// <summary>
        ///     Current Connections
        /// </summary>
        public List<IConnection> Connections { get; set; } = new List<IConnection>();

        /// <summary>
        ///     PoolSize
        /// </summary>
        public int PoolSize { get; set; } = 5;

        /// <summary>
        ///     Get A RoundRobin IConnection
        /// </summary>
        /// <returns>
        ///     <see cref="IConnection" />
        /// </returns>
        public IConnection GetPooledConnection() {
            var index = (int) (this.IncrementCount() % this.PoolSize);
            return this.Connections[index];
        }

        /// <summary>
        ///     Initialize ConnectionPool
        /// </summary>
        /// <returns>
        ///     <see cref="Task" />
        /// </returns>
        public async Task Initialize(IConnection connection) {
            for (var i = 0; i < this.PoolSize; i++) {
                await connection.Connect().ConfigureAwait(false);
                this.Connections.Add(connection);
            }
        }

        /// <summary>
        ///     Atomic Increase On RoundRobinCounter
        /// </summary>
        /// <returns>uint index</returns>
        private uint IncrementCount() {
            var value = Interlocked.CompareExchange(ref this._roundRobinCounter, ++this._roundRobinCounter, this._roundRobinCounter);
            return (uint) value;
        }
    }
}