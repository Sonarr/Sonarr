// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR
{
    /// <summary>
    /// Manages groups for a connection and allows sending messages to the group.
    /// </summary>
    public interface IConnectionGroupManager : IGroupManager
    {
        /// <summary>
        /// Sends a value to the specified group.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        /// <param name="value">The value to send.</param>
        /// <param name="excludeConnectionIds">The list of connection ids to exclude</param>
        /// <returns>A task that represents when send is complete.</returns>
        Task Send(string groupName, object value, params string[] excludeConnectionIds);
    }
}
