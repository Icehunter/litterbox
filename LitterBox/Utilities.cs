// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   Utilities.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox {
    using LitterBox.JsonContractResolvers;

    using Newtonsoft.Json;

    /// <summary>
    ///     The utilities.
    /// </summary>
    public static class Utilities {
        #region JSON Handlers

        /// <summary>
        ///     Convert T To Json
        /// </summary>
        /// <typeparam name="T">Type Of Cached Item</typeparam>
        /// <param name="value">Value Of Cached Item</param>
        /// <returns>Json Representation</returns>
        public static string Serialize<T>(T value) {
            return JsonConvert.SerializeObject(
                value,
                Formatting.None,
                new JsonSerializerSettings {
                    ContractResolver = new CamelCaseExceptDictionaryKeysContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto
                });
        }

        /// <summary>
        ///     Convert Json To T
        /// </summary>
        /// <typeparam name="T">Type Of Cached Item</typeparam>
        /// <param name="value">Value Of Cached Item</param>
        /// <returns>T Representation</returns>
        public static T Deserialize<T>(string value) {
            return JsonConvert.DeserializeObject<T>(
                value,
                new JsonSerializerSettings {
                    ContractResolver = new CamelCaseExceptDictionaryKeysContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto
                });
        }

        /// <summary>
        /// Clone Object Using JsonConvert
        /// </summary>
        /// <typeparam name="T">Type Of Value</typeparam>
        /// <param name="value">Value</param>
        /// <returns>T Value</returns>
        public static T Clone<T>(T value) {
            return Deserialize<T>(Serialize(value));
        }

        #endregion
    }
}