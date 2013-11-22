// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public class ConnectionIdProxy : SignalProxy
    {
        public ConnectionIdProxy(Func<string, ClientHubInvocation, IList<string>, Task> send, string signal, string hubName, params string[] exclude) :
            base(send, signal, hubName, PrefixHelper.HubConnectionIdPrefix, exclude)
        {

        }
    }
}
