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

namespace LitterBox.Memory.Models {
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    internal class Connection {
        /// <summary>
        /// Connection
        /// </summary>
        /// <param name="config">config</param>
        public Connection(Config config) {
            this._config = config;
        }

        /// <summary>
        /// MemoryCache
        /// </summary>
        public CacheStore Cache { get; set; }

        /// <summary>
        /// Connection Config
        /// </summary>
        private Config _config { get; set; }

        #region Internals

        /// <summary>
        /// Connect to MemoryCache
        /// </summary>
        /// <returns>Raw MemoryCache</returns>
        internal async Task Connect() {
            await Task.Run(() => {
                this.Cache = new CacheStore(new CacheStoreOptions {
                    ExpirationScanFrequency = this._config.ExpirationScanFrequency
                });
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Flush (Dispose) Of Cache / Recreate
        /// </summary>
        /// <returns>Success True|False</returns>
        internal async Task Flush() {
            await Task.Run(() => {
                this.Cache.Flush();
            }).ConfigureAwait(false);
        }

        #endregion
    }
}