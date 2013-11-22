// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public class ClientProxy : DynamicObject, IClientProxy
    {
        private readonly Func<string, ClientHubInvocation, IList<string>, Task> _send;
        private readonly string _hubName;
        private readonly IList<string> _exclude;

        public ClientProxy(Func<string, ClientHubInvocation, IList<string>, Task> send, string hubName, IList<string> exclude)
        {
            _send = send;
            _hubName = hubName;
            _exclude = exclude;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Binder is passed in by the DLR")]
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = Invoke(binder.Name, args);
            return true;
        }

        public Task Invoke(string method, params object[] args)
        {
            var invocation = new ClientHubInvocation
            {
                Hub = _hubName,
                Method = method,
                Args = args
            };

            return _send(PrefixHelper.GetHubName(_hubName), invocation, _exclude);
        }
    }
}
