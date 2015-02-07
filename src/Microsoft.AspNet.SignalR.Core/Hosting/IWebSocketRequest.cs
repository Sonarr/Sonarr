// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Hosting
{
    public interface IWebSocketRequest : IRequest
    {
        /// <summary>
        /// Accepts an websocket request using the specified user function.
        /// </summary>
        /// <param name="callback">The callback that fires when the websocket is ready.</param>
        /// <param name="initTask">The task that completes when the websocket transport is ready.</param>
        Task AcceptWebSocketRequest(Func<IWebSocket, Task> callback, Task initTask);
    }
}
