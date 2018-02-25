// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseConnectionConfiguration.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   BaseConnectionConfiguration.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Models {
    using System;

    /// <summary>
    ///     Base Connection Configuration
    /// </summary>
    public class BaseConnectionConfiguration {
        /// <summary>
        ///     DefaultTimeToLive (1 Day)
        /// </summary>
        public TimeSpan DefaultTimeToLive { get; set; } = new TimeSpan(1, 0, 0, 0);

        /// <summary>
        ///     DefaultTimeToRefresh (5 Minutes)
        /// </summary>
        public TimeSpan DefaultTimeToRefresh { get; set; } = new TimeSpan(0, 0, 5, 0);

        /// <summary>
        ///     Connection PoolSize
        /// </summary>
        public int PoolSize { get; set; } = 5;
    }
}