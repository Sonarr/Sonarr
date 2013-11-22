// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    /// <summary>
    /// A server to server command.
    /// </summary>
    internal class ServerCommand
    {
        /// <summary>
        /// Gets or sets the id of the command where this message originated from.
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// Gets of sets the command type.
        /// </summary>
        public ServerCommandType ServerCommandType { get; set; }

        /// <summary>
        /// Gets or sets the value for this command.
        /// </summary>
        public object Value { get; set; }

        internal bool IsFromSelf(string serverId)
        {
            return serverId.Equals(ServerId);
        }
    }
}
