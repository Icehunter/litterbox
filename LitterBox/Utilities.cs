// MIT License
// 
// Copyright(c) 2017 Ryan Wilson <syndicated.life@gmail.com> (http://syndicated.life/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace LitterBox {
    using LitterBox.JsonContractResolvers;
    using Newtonsoft.Json;

    public static class Utilities {
        #region JSON Handlers

        /// <summary>
        /// Convert T To Json
        /// </summary>
        /// <typeparam name="T">Type Of Cached Item</typeparam>
        /// <param name="value">Value Of Cached Item</param>
        /// <returns>Json Representation</returns>
        public static string Serialize<T>(T value) {
            return JsonConvert.SerializeObject(value, Formatting.None, new JsonSerializerSettings {
                ContractResolver = new CamelCaseExceptDictionaryKeysContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        /// <summary>
        /// Convert Json To T
        /// </summary>
        /// <typeparam name="T">Type Of Cached Item</typeparam>
        /// <param name="value">Value Of Cached Item</param>
        /// <returns>T Representation</returns>
        public static T Deserialize<T>(string value) {
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings {
                ContractResolver = new CamelCaseExceptDictionaryKeysContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        #endregion
    }
}