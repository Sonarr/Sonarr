// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR
{
    /// <summary>
    /// Manages groups for a connection.
    /// </summary>
    public interface IGroupManager
    {
        /// <summary>
        /// Adds a connection to the specified group. 
        /// </summary>
        /// <param name="connectionId">The connection id to add to the group.</param>
        /// <param name="groupName">The name of the group</param>
        /// <returns>A task that represents the connection id being added to the group.</returns>
        Task Add(string connectionId, string groupName);

        /// <summary>
        /// Removes a connection from the specified group.
        /// </summary>
        /// <param name="connectionId">The connection id to remove from the group.</param>
        /// <param name="groupName">The name of the group</param>
        /// <returns>A task that represents the connection id being removed from the group.</returns>
        Task Remove(string connectionId, string groupName);
    }
}
