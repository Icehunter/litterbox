// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILitterBox.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   ILitterBox.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Interfaces {
    using System;
    using System.Threading.Tasks;

    using LitterBox.Models;

    /// <summary>
    ///     The LitterBox interface.
    /// </summary>
    public interface ILitterBox {
        #region Events

        /// <summary>
        ///     ExceptionEvent Invoker
        /// </summary>
        event EventHandler<ExceptionEvent> ExceptionEvent;

        #endregion

        #region Getters

        /// <summary>
        ///     GetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <returns>LitterBoxItem T</returns>
        Task<LitterBoxItem<T>> GetItem<T>(string key);

        #endregion

        #region Connection Pools

        /// <summary>
        ///     Connection Objects
        /// </summary>
        ConnectionPool Pool { get; set; }

        /// <summary>
        ///     Get Pooled Connection Of Type T
        /// </summary>
        /// <typeparam name="T">Type T</typeparam>
        /// <returns>T IConnection</returns>
        T GetPooledConnection<T>();

        #endregion

        #region Setters

        /// <summary>
        ///     SetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="litter">Item T To Be Cached</param>
        /// <returns>Success True|False</returns>
        Task<bool> SetItem<T>(string key, LitterBoxItem<T> litter);

        #endregion

        #region Connection Based

        /// <summary>
        ///     Close/Dispose Connection And Reconnect With Existing Properties
        /// </summary>
        /// <returns>Success True|False</returns>
        Task<bool> Reconnect();

        /// <summary>
        ///     Flush Cache
        /// </summary>
        /// <returns>Success True|False</returns>
        Task<bool> Flush();

        #endregion

        #region Fire Forget

        /// <summary>
        ///     SetItem T (Fire Forget) By Key, LitterBoxItem T
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="litter">Item T To Be Cached</param>
        void SetItemFireAndForget<T>(string key, LitterBoxItem<T> litter);

        /// <summary>
        ///     SetItem T (Fire Forget) By Key, Generator, TimeToRefresh, TimeToLive
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action</param>
        /// <param name="timeToRefresh">How Long After Creation To Be Considered "Good"</param>
        /// <param name="timeToLive">How Long After Creation To Auto-Delete</param>
        void SetItemFireAndForget<T>(string key, Func<Task<T>> generator, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null);

        #endregion
    }
}