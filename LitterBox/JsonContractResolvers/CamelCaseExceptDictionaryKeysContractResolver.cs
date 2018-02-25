// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CamelCaseExceptDictionaryKeysContractResolver.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   CamelCaseExceptDictionaryKeysContractResolver.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.JsonContractResolvers {
    using System;

    using Newtonsoft.Json.Serialization;

    /// <summary>
    ///     Handle dictoinary keys and don't lowercase them
    /// </summary>
    public class CamelCaseExceptDictionaryKeysContractResolver : CamelCasePropertyNamesContractResolver {
        /// <summary>
        ///     internal override to Resolver
        /// </summary>
        /// <param name="objectType">objectType</param>
        /// <returns>JsonDictionaryContract</returns>
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType) {
            var contract = base.CreateDictionaryContract(objectType);

            contract.DictionaryKeyResolver = propertyName => propertyName;

            return contract;
        }
    }
}