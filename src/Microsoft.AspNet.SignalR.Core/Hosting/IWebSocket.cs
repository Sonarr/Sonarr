// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Hosting
{
    /// <summary>
    /// Represents a web socket.
    /// </summary>
    public interface IWebSocket
    {
        /// <summary>
        /// Invoked when data is sent over the websocket
        /// </summary>
        Action<string> OnMessage { get; set; }

        /// <summary>
        /// Invoked when the websocket closes
        /// </summary>
        Action OnClose { get; set; }

        /// <summary>
        /// Invoked when there is an error
        /// </summary>
        Action<Exception> OnError { get; set; }

        /// <summary>
        /// Sends data over the websocket.
        /// </summary>
        /// <param name="value">The value to send.</param>
        /// <returns>A <see cref="Task"/> that represents the send is complete.</returns>
        Task Send(string value);

        /// <summary>
        /// Sends a chunk of data over the websocket ("endOfMessage" flag set to false.)
        /// </summary>
        /// <param name="message"></param>
        /// <returns>A <see cref="Task"/> that represents the send is complete.</returns>
        Task SendChunk(ArraySegment<byte> message);

        /// <summary>
        /// Sends a zero byte data chunk with the "endOfMessage" flag set to true.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the flush is complete.</returns>
        Task Flush();
    }
}
