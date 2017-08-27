﻿// MIT License
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

namespace LitterBox {
    using System;

    /// <summary>
    /// Constants Class Instance
    /// </summary>
    public static class Constants {
        /// <summary>
        /// Default Expiry Of Cached Item (Either From Created Or Insertion)
        /// </summary>
        public static TimeSpan DefaultExpiry = new TimeSpan(30, 0, 0, 0);

        /// <summary>
        /// Default Inactive Expiry Of Cached Item (From Created)
        /// </summary>
        public static TimeSpan DefaultInactiveExpiry = new TimeSpan(7, 0, 0, 0);

        /// <summary>
        /// Default Time When Cached Item Is Stale (From Created)
        /// </summary>
        public static TimeSpan DefaultStaleIn = new TimeSpan(1, 0, 0, 0);
    }
}