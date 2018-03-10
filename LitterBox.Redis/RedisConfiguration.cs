// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RedisConfiguration.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   RedisConfiguration.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Redis {
    using LitterBox.Models;

    /// <summary>
    ///     Configuration For Connection
    /// </summary>
    public class RedisConfiguration : BaseConnectionConfiguration {
        /// <summary>
        ///     Database
        /// </summary>
        public int DatabaseID { get; set; } = 0;

        /// <summary>
        ///     Hostname
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";

        /// <summary>
        ///     Password
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        ///     Port
        /// </summary>
        public int Port { get; set; } = 6380;

        /// <summary>
        ///     UseSSL On Connection
        /// </summary>
        public bool UseSSL { get; set; } = true;
    }
}