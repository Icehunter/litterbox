// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Compression.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   Compression.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox {
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    /// <summary>
    ///     Compression Class
    /// </summary>
    public static class Compression {
        /// <summary>
        ///     Unzip Byte[] => String (Uncompressed)
        /// </summary>
        /// <param name="value">Byte[] Data</param>
        /// <returns>String</returns>
        public static string Unzip(byte[] value) {
            using (var input = new MemoryStream(value)) {
                using (var output = new MemoryStream()) {
                    using (var gZipStream = new GZipStream(input, CompressionMode.Decompress)) {
                        gZipStream.CopyTo(output);
                    }

                    return Encoding.UTF8.GetString(output.ToArray());
                }
            }
        }

        /// <summary>
        ///     Zip String => Byte[] (Compressed)
        /// </summary>
        /// <param name="value">String Data</param>
        /// <returns>Byte[]</returns>
        public static byte[] Zip(string value) {
            var buffer = Encoding.UTF8.GetBytes(value);
            using (var input = new MemoryStream(buffer)) {
                using (var output = new MemoryStream()) {
                    using (var gZipStream = new GZipStream(output, CompressionMode.Compress)) {
                        input.CopyTo(gZipStream);
                    }

                    return output.ToArray();
                }
            }
        }
    }
}