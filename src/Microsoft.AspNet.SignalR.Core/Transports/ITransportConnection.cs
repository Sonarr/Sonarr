// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Transports
{
    public interface ITransportConnection
    {
        IDisposable Receive(string messageId, Func<PersistentResponse, object, Task<bool>> callback, int maxMessages, object state);

        Task Send(ConnectionMessage message);
    }
}
