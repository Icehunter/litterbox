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
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    /// <summary>
    /// Compression Class
    /// </summary>
    public static class Compression {
        /// <summary>
        /// Copy Source => Destination (By Ref)
        /// </summary>
        /// <param name="source">source stream</param>
        /// <param name="destination">desition stream</param>
        public static void CopyTo(Stream source, Stream destination) {
            var bytes = new byte[16384];
            int count;
            while ((count = source.Read(bytes, 0, bytes.Length)) != 0) {
                destination.Write(bytes, 0, count);
            }
        }

        /// <summary>
        /// Zip String => Byte[] (Compressed)
        /// </summary>
        /// <param name="value">String Data</param>
        /// <returns>Byte[]</returns>
        public static byte[] Zip(string value) {
            var buffer = Encoding.UTF8.GetBytes(value);
            using (var input = new MemoryStream(buffer)) {
                using (var output = new MemoryStream()) {
                    using (var gZipStream = new GZipStream(output, CompressionMode.Compress)) {
                        CopyTo(input, gZipStream);
                    }
                    return output.ToArray();
                }
            }
        }

        /// <summary>
        /// Unzip Byte[] => String (Uncompressed)
        /// </summary>
        /// <param name="value">Byte[] Data</param>
        /// <returns>String</returns>
        public static string Unzip(byte[] value) {
            using (var input = new MemoryStream(value)) {
                using (var output = new MemoryStream()) {
                    using (var gZipStream = new GZipStream(input, CompressionMode.Decompress)) {
                        CopyTo(gZipStream, output);
                    }
                    return Encoding.UTF8.GetString(output.ToArray());
                }
            }
        }
    }
}