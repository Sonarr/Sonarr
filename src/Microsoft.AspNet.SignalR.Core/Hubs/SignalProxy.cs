// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public abstract class SignalProxy : DynamicObject, IClientProxy
    {
        private readonly IList<string> _exclude;
        private readonly string _prefix;

        protected SignalProxy(Func<string, ClientHubInvocation, IList<string>, Task> send, string signal, string hubName, string prefix, IList<string> exclude)
        {
            Send = send;
            Signal = signal;
            HubName = hubName;
            _prefix = prefix;
            _exclude = exclude;
        }

        protected Func<string, ClientHubInvocation, IList<string>, Task> Send { get; private set; }
        protected string Signal { get; private set; }
        protected string HubName { get; private set; }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            return false;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "The compiler generates calls to invoke this")]
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = Invoke(binder.Name, args);
            return true;
        }

        public Task Invoke(string method, params object[] args)
        {
            var invocation = GetInvocationData(method, args);

            string signal = _prefix + HubName + "." + Signal;

            return Send(signal, invocation, _exclude);
        }

        protected virtual ClientHubInvocation GetInvocationData(string method, object[] args)
        {
            return new ClientHubInvocation
            {
                Hub = HubName,
                Method = method,
                Args = args,
                Target = Signal
            };
        }
    }
}
