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

namespace LitterBox.Interfaces {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LitterBox.Models;

    public interface ILitterBox {
        #region Events

        event EventHandler<ExceptionEvent> ExceptionEvent;

        #endregion

        #region Connection Based

        Task<bool> Reconnect();
        Task<bool> Flush();

        #endregion

        #region Getters

        Task<LitterBoxItem<T>> GetItem<T>(string key);
        Task<LitterBoxItem<T>> GetItem<T>(string key, Func<Task<T>> generator, TimeSpan? staleIn = null, TimeSpan? expiry = null);
        Task<List<LitterBoxItem<T>>> GetItems<T>(List<string> keys);
        Task<List<LitterBoxItem<T>>> GetItems<T>(List<string> keys, List<Func<Task<T>>> generators, TimeSpan? staleIn = null, TimeSpan? expiry = null);

        #endregion

        #region Setters

        Task<bool> SetItem<T>(string key, LitterBoxItem<T> litter);
        Task<List<bool>> SetItems<T>(List<string> keys, List<LitterBoxItem<T>> litters);

        #endregion

        #region Fire Forget

        void SetItemFireAndForget<T>(string key, LitterBoxItem<T> litter);
        void SetItemFireAndForget<T>(string key, Func<Task<T>> generator, TimeSpan? staleIn = null, TimeSpan? expiry = null);

        #endregion
    }
}