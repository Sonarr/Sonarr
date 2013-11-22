// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Messaging;

namespace Microsoft.AspNet.SignalR.Transports
{
    internal static class TransportConnectionExtensions
    {
        internal static Task Close(this ITransportConnection connection, string connectionId)
        {
            return SendCommand(connection, connectionId, CommandType.Disconnect);
        }

        internal static Task Abort(this ITransportConnection connection, string connectionId)
        {
            return SendCommand(connection, connectionId, CommandType.Abort);
        }

        private static Task SendCommand(ITransportConnection connection, string connectionId, CommandType commandType)
        {
            var command = new Command
            {
                CommandType = commandType
            };

            var message = new ConnectionMessage(PrefixHelper.GetConnectionId(connectionId),
                                                command);

            return connection.Send(message);
        }
    }
}
