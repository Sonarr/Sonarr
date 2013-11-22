// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Hubs
{
    internal class HubContext : IHubContext
    {
        public HubContext(Func<string, ClientHubInvocation, IList<string>, Task> send, string hubName, IConnection connection)
        {
            Clients = new ExternalHubConnectionContext(send, hubName);
            Groups = new GroupManager(connection, PrefixHelper.GetHubGroupName(hubName));
        }

        public IHubConnectionContext Clients { get; private set; }

        public IGroupManager Groups { get; private set; }

        private class ExternalHubConnectionContext : IHubConnectionContext
        {
            private readonly Func<string, ClientHubInvocation, IList<string>, Task> _send;
            private readonly string _hubName;

            public ExternalHubConnectionContext(Func<string, ClientHubInvocation, IList<string>, Task> send, string hubName)
            {
                _send = send;
                _hubName = hubName;
                All = AllExcept();
            }

            public dynamic All
            {
                get;
                private set;
            }

            public dynamic AllExcept(params string[] exclude)
            {
                return new ClientProxy(_send, _hubName, exclude);
            }

            public dynamic Group(string groupName, params string[] exclude)
            {
                if (string.IsNullOrEmpty(groupName))
                {
                    throw new ArgumentException(Resources.Error_ArgumentNullOrEmpty, "groupName");
                }

                return new GroupProxy(_send, groupName, _hubName, exclude);
            }

            public dynamic Client(string connectionId)
            {
                if (string.IsNullOrEmpty(connectionId))
                {
                    throw new ArgumentException(Resources.Error_ArgumentNullOrEmpty, "connectionId");
                }

                return new ConnectionIdProxy(_send, connectionId, _hubName);
            }
        }
    }
}
