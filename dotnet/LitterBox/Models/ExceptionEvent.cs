// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionEvent.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   ExceptionEvent.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LitterBox.Models {
    using System;

    /// <summary>
    ///     ExceptionEvent Instance
    /// </summary>
    public class ExceptionEvent : EventArgs {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExceptionEvent" /> class.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="exception">exception</param>
        public ExceptionEvent(object sender, Exception exception) {
            this.Sender = sender;
            this.Exception = exception;
        }

        /// <summary>
        ///     Exception
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        ///     Sender
        /// </summary>
        public object Sender { get; set; }
    }
}