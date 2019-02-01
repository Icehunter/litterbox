// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISuccessResult.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   ISuccessResult.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Interfaces {
    /// <summary>
    ///     Success Result Interface
    /// </summary>
    public interface ISuccessResult {
        /// <summary>
        ///     Bool Indication Success Of Action
        /// </summary>
        bool IsSuccessful { get; set; }
    }
}