// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentDBConfiguration.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   DocumentDBConfiguration.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.DocumentDB {
    using System;

    using LitterBox.Models;

    using Microsoft.Azure.Documents.Client;

    /// <summary>
    ///     Configuration For Connection
    /// </summary>
    public class DocumentDBConfiguration : BaseConnectionConfiguration {
        /// <summary>
        ///     Collection
        /// </summary>
        public string Collection { get; set; }

        /// <summary>
        ///     ConnectionPolicy
        /// </summary>
        public ConnectionPolicy ConnectionPolicy { get; set; } = new ConnectionPolicy();

        /// <summary>
        ///     Database
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        ///     EndpointURI
        /// </summary>
        public Uri EndpointURI { get; set; }

        /// <summary>
        ///     PartitionKey
        /// </summary>
        public string PartitionKey { get; set; } = "cache";

        /// <summary>
        ///     PrimaryKey
        /// </summary>
        public string PrimaryKey { get; set; }
    }
}