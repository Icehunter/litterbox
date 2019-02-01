// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITenancy.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   ITenancy.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Interfaces {
    using System;
    using System.Threading.Tasks;

    using LitterBox.Models;

    /// <summary>
    ///     The Tenancy interface.
    /// </summary>
    public interface ITenancy {
        #region Events

        /// <summary>
        ///     ExceptionEvent Invoker
        /// </summary>
        event EventHandler<ExceptionEvent> ExceptionEvent;

        #endregion

        #region Connection Based

        /// <summary>
        ///     Close/Dispose Connection And Reconnect With Existing Properties
        /// </summary>
        /// <returns>Success True|False (For Each Cache)</returns>
        Task<ReconnectionResult[]> Reconnect();

        /// <summary>
        ///     Flush Cache
        /// </summary>
        /// <returns>Success True|False (For Each Cache)</returns>
        Task<FlushResult[]> Flush();

        #endregion

        #region Getters

        /// <summary>
        ///     GetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <returns>TenancyItem => LitterBoxItem T</returns>
        Task<LitterBoxItem<T>> GetItem<T>(string key);

        /// <summary>
        ///     GetItem T By Key, Generator, TimeToRefresh, TimeToLive
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="generator">Generator Action If Not Found</param>
        /// <param name="timeToRefresh">How Long After Creation To Be Considered "Good"</param>
        /// <param name="timeToLive">How Long After Creation To Auto-Delete</param>
        /// <returns>TenancyItem => LitterBoxItem T</returns>
        Task<LitterBoxItem<T>> GetItem<T>(string key, Func<Task<T>> generator, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null);

        /// <summary>
        ///     GetItems T By Keys
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <returns>List TenancyItem => LitterBoxItem T</returns>
        Task<LitterBoxItem<T>[]> GetItems<T>(string[] keys);

        /// <summary>
        ///     GetItems T By Keys, Generators, TimeToRefresh, TimeToLive
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="generators">Generator Actions If Not Found</param>
        /// <param name="timeToRefresh">How Long After Creation To Be Considered "Good"</param>
        /// <param name="timeToLive">How Long After Creation To Auto-Delete</param>
        /// <returns>List TenancyItem => LitterBoxItem T</returns>
        Task<LitterBoxItem<T>[]> GetItems<T>(string[] keys, Func<Task<T>>[] generators, TimeSpan? timeToRefresh = null, TimeSpan? timeToLive = null);

        #endregion

        #region Setters

        /// <summary>
        ///     SetItem T By Key
        /// </summary>
        /// <typeparam name="T">Type Of Cached Value</typeparam>
        /// <param name="key">Key Lookup</param>
        /// <param name="litter">Item T To Be Cached</param>
        /// <returns>Success True|False (For Each Cache)</returns>
        Task<StorageResult[]> SetItem<T>(string key, LitterBoxItem<T> litter);

        /// <summary>
        ///     SetItems T By Keys
        /// </summary>
        /// <typeparam name="T">Type Of Cached Values</typeparam>
        /// <param name="keys">Key Lookups</param>
        /// <param name="litters">Items T To Be Cached</param>
        /// <returns>Success True|False (For Each Cache)</returns>
        Task<StorageResult[][]> SetItems<T>(string[] keys, LitterBoxItem<T>[] litters);

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