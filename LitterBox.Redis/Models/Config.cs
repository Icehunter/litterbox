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
    using System;

    /// <summary>
    /// Configuration For Connection
    /// </summary>
    public class Config {
        /// <summary>
        /// Hostname
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; } = 6379;

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Database
        /// </summary>
        public int DatabaseID { get; set; } = 0;

        /// <summary>
        /// Connection PoolSize
        /// </summary>
        public int PoolSize { get; set; } = 5;

        /// <summary>
        /// DefaultTimeToLive (1 Day)
        /// </summary>
        public TimeSpan DefaultTimeToLive { get; set; } = new TimeSpan(1, 0, 0, 0);

        /// <summary>
        /// DefaultTimeToRefresh (5 Minutes)
        /// </summary>
        public TimeSpan DefaultTimeToRefresh { get; set; } = new TimeSpan(0, 0, 5, 0);
    }
}