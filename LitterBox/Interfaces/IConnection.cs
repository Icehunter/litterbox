// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConnection.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   IConnection.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Interfaces {
    using System.Threading.Tasks;

    public interface IConnection {
        Task Connect();

        Task<bool> Flush();

        Task Reconnect();
    }
}