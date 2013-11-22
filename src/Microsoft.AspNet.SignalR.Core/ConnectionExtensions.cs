// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR
{
    public static class ConnectionExtensions
    {
        /// <summary>
        /// Sends a message to all connections subscribed to the specified signal. An example of signal may be a
        /// specific connection id.
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="connectionId">The connectionId to send to.</param>
        /// <param name="value">The value to publish.</param>
        /// <param name="excludeConnectionIds">The list of connection ids to exclude</param>
        /// <returns>A task that represents when the broadcast is complete.</returns>
        public static Task Send(this IConnection connection, string connectionId, object value, params string[] excludeConnectionIds)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(connectionId))
            {
                throw new ArgumentException(Resources.Error_ArgumentNullOrEmpty, "connectionId");
            }

            var message = new ConnectionMessage(PrefixHelper.GetConnectionId(connectionId),
                                                value,
                                                PrefixHelper.GetPrefixedConnectionIds(excludeConnectionIds));

            return connection.Send(message);
        }

        /// <summary>
        /// Broadcasts a value to all connections, excluding the connection ids specified.
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="value">The value to broadcast.</param>
        /// <param name="excludeConnectionIds">The list of connection ids to exclude</param>
        /// <returns>A task that represents when the broadcast is complete.</returns>
        public static Task Broadcast(this IConnection connection, object value, params string[] excludeConnectionIds)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            var message = new ConnectionMessage(connection.DefaultSignal,
                                                value,
                                                PrefixHelper.GetPrefixedConnectionIds(excludeConnectionIds));

            return connection.Send(message);
        }
    }
}
