// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReconnectionResult.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   ReconnectionResult.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Models {
    using System;

    using LitterBox.Interfaces;

    /// <summary>
    ///     Reconnection Results
    /// </summary>
    public class ReconnectionResult : ISuccessResult {
        /// <summary>
        ///     Type Of Cache
        /// </summary>
        public Type CacheType { get; set; }

        #region ISuccessResult Implementation

        /// <summary>
        ///     Bool Indication Success Of Reconnection
        /// </summary>
        public bool IsSuccessful { get; set; }

        #endregion
    }
}