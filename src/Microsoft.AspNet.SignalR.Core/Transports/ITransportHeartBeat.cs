// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNet.SignalR.Transports
{
    /// <summary>
    /// Manages tracking the state of connections.
    /// </summary>
    public interface ITransportHeartbeat
    {
        /// <summary>
        /// Adds a new connection to the list of tracked connections.
        /// </summary>
        /// <param name="connection">The connection to be added.</param>
        bool AddConnection(ITrackingConnection connection);

        /// <summary>
        /// Marks an existing connection as active.
        /// </summary>
        /// <param name="connection">The connection to mark.</param>
        void MarkConnection(ITrackingConnection connection);

        /// <summary>
        /// Removes a connection from the list of tracked connections.
        /// </summary>
        /// <param name="connection">The connection to remove.</param>
        void RemoveConnection(ITrackingConnection connection);

        /// <summary>
        /// Gets a list of connections being tracked.
        /// </summary>
        /// <returns>A list of connections.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This might be expensive.")]
        IList<ITrackingConnection> GetConnections();
    }
}
