// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR
{
    /// <summary>
    /// A communication channel for a <see cref="PersistentConnection"/> and its connections.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// The main signal for this connection. This is the main signalr for a <see cref="PersistentConnection"/>.
        /// </summary>
        string DefaultSignal { get; }

        /// <summary>
        /// Sends a message to connections subscribed to the signal.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A task that returns when the message has be sent.</returns>
        Task Send(ConnectionMessage message);
    }
}
