// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    /// <summary>
    /// Handles commands from server to server.
    /// </summary>
    internal interface IServerCommandHandler
    {
        /// <summary>
        /// Sends a command to all connected servers.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Task SendCommand(ServerCommand command);

        /// <summary>
        /// Gets or sets a callback that is invoked when a command is received.
        /// </summary>
        Action<ServerCommand> Command { get; set; }
    }
}
