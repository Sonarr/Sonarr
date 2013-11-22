// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    /// <summary>
    /// Generates a server id
    /// </summary>
    public interface IServerIdManager
    {
        /// <summary>
        /// The id of the server.
        /// </summary>
        string ServerId { get; }
    }
}
