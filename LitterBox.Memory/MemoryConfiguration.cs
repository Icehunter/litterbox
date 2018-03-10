// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemoryConfiguration.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   MemoryConfiguration.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Memory {
    using System;

    using LitterBox.Models;

    /// <summary>
    ///     Configuration For Connection
    /// </summary>
    public class MemoryConfiguration : BaseConnectionConfiguration {
        /// <summary>
        ///     ExpirationScanFrequency
        /// </summary>
        public TimeSpan ExpirationScanFrequency { get; set; } = new TimeSpan(0, 1, 0, 0);
    }
}