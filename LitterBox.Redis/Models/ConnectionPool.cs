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

namespace LitterBox.Redis.Models {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class ConnectionPool {
        /// <summary>
        /// Counter Storages
        /// </summary>
        private int _roundRobinCounter;

        /// <summary>
        /// PoolSize
        /// </summary>
        public int PoolSize { get; set; } = 5;

        /// <summary>
        /// Current Connections
        /// </summary>
        public List<Connection> Connections { get; set; } = new List<Connection>();

        /// <summary>
        /// Get A RoundRobin Connection
        /// </summary>
        /// <returns></returns>
        public Connection GetPooledConnection() {
            var index = (int) (this.IncrementCount() % this.PoolSize);
            return this.Connections[index];
        }

        /// <summary>
        /// Atomic Increase On RoundRobinCounter
        /// </summary>
        /// <returns>uint index</returns>
        private uint IncrementCount() {
            var value = Interlocked.CompareExchange(ref this._roundRobinCounter, ++this._roundRobinCounter, this._roundRobinCounter);
            return (uint) value;
        }

        /// <summary>
        /// Initialize ConnectionPool
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public async Task Initialize(Config config) {
            for (var i = 0; i < this.PoolSize; i++) {
                var connection = new Connection(config);
                await connection.Connect().ConfigureAwait(false);
                this.Connections.Add(connection);
            }
        }
    }
}