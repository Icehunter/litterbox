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

namespace LitterBox.DocumentDB.Models {
    using System;
    using Microsoft.Azure.Documents.Client;

    /// <summary>
    /// Configuration For Connection
    /// </summary>
    public class Config {
        /// <summary>
        /// DocumentDBEndpointURI
        /// </summary>
        public Uri DocumentDBEndpointURI { get; set; }

        /// <summary>
        /// DocumentDBPrimaryKey
        /// </summary>
        public string DocumentDBPrimaryKey { get; set; }

        /// <summary>
        /// DocumentDBDatabase
        /// </summary>
        public string DocumentDBDatabase { get; set; }

        /// <summary>
        /// DocumentDBCollection
        /// </summary>
        public string DocumentDBCollection { get; set; }

        /// <summary>
        /// ConnectionPolicy
        /// </summary>
        public ConnectionPolicy ConnectionPolicy { get; set; } = new ConnectionPolicy();

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