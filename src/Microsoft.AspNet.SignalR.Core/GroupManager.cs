// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Messaging;

namespace Microsoft.AspNet.SignalR
{
    /// <summary>
    /// The default <see cref="IGroupManager"/> implementation.
    /// </summary>
    public class GroupManager : IConnectionGroupManager
    {
        private readonly IConnection _connection;
        private readonly string _groupPrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupManager"/> class.
        /// </summary>
        /// <param name="connection">The <see cref="IConnection"/> this group resides on.</param>
        /// <param name="groupPrefix">The prefix for this group. Either a <see cref="Microsoft.AspNet.SignalR.Hubs.IHub"/> name or <see cref="PersistentConnection"/> type name.</param>
        public GroupManager(IConnection connection, string groupPrefix)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            _connection = connection;
            _groupPrefix = groupPrefix;
        }

        /// <summary>
        /// Sends a value to the specified group.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        /// <param name="value">The value to send.</param>
        /// <param name="excludeConnectionIds">The list of connection ids to exclude</param>
        /// <returns>A task that represents when send is complete.</returns>
        public Task Send(string groupName, object value, params string[] excludeConnectionIds)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                throw new ArgumentException((Resources.Error_ArgumentNullOrEmpty), "groupName");
            }

            var qualifiedName = CreateQualifiedName(groupName);
            var message = new ConnectionMessage(qualifiedName,
                                                value,
                                                PrefixHelper.GetPrefixedConnectionIds(excludeConnectionIds));

            return _connection.Send(message);
        }

        /// <summary>
        /// Adds a connection to the specified group. 
        /// </summary>
        /// <param name="connectionId">The connection id to add to the group.</param>
        /// <param name="groupName">The name of the group</param>
        /// <returns>A task that represents the connection id being added to the group.</returns>
        public Task Add(string connectionId, string groupName)
        {
            if (connectionId == null)
            {
                throw new ArgumentNullException("connectionId");
            }

            if (groupName == null)
            {
                throw new ArgumentNullException("groupName");
            }

            var command = new Command
            {
                CommandType = CommandType.AddToGroup,
                Value = CreateQualifiedName(groupName),
                WaitForAck = true
            };

            return _connection.Send(connectionId, command);
        }

        /// <summary>
        /// Removes a connection from the specified group.
        /// </summary>
        /// <param name="connectionId">The connection id to remove from the group.</param>
        /// <param name="groupName">The name of the group</param>
        /// <returns>A task that represents the connection id being removed from the group.</returns>
        public Task Remove(string connectionId, string groupName)
        {
            if (connectionId == null)
            {
                throw new ArgumentNullException("connectionId");
            }

            if (groupName == null)
            {
                throw new ArgumentNullException("groupName");
            }

            var command = new Command
            {
                CommandType = CommandType.RemoveFromGroup,
                Value = CreateQualifiedName(groupName),
                WaitForAck = true
            };

            return _connection.Send(connectionId, command);
        }

        private string CreateQualifiedName(string groupName)
        {
            return _groupPrefix + "." + groupName;
        }
    }
}
