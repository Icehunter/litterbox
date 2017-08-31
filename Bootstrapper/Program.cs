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

namespace Bootstrapper {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class Program {
        static void Main(string[] args) {
            var memoryCache = LitterBox.Memory.Memoize.GetInstance();
            memoryCache.Flush().Wait();

            var redisCache = LitterBox.Redis.Memoize.GetInstance();
            redisCache.Flush().Wait();

            var xMemory = memoryCache.GetItem<string>("what").Result;
            Console.WriteLine($"memory: x === null: {xMemory == null}");

            var xRedis = redisCache.GetItem<string>("what").Result;
            Console.WriteLine($"redis: x === null: {xRedis == null}");

            var yMemory = memoryCache.GetItem("what", Task.Run(() => "happened")).Result;
            Console.WriteLine($"memory: y === \"happened\": {yMemory.Value == "happened"}");

            var yRedis = redisCache.GetItem("what", Task.Run(() => "happened")).Result;
            Console.WriteLine($"redis: y === \"happened\": {yRedis.Value == "happened"}");

            Thread.Sleep(1000);

            var zMemory = memoryCache.GetItem("what", Task.Run(() => "is it")).Result;
            Console.WriteLine($"memory: z == y: {zMemory.Value == yMemory.Value}");

            var zRedis = redisCache.GetItem("what", Task.Run(() => "is it")).Result;
            Console.WriteLine($"redis: z = y: {zRedis.Value == yRedis.Value}");

            Console.ReadKey();
        }
    }
}