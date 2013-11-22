// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public class StatefulSignalProxy : SignalProxy
    {
        private readonly StateChangeTracker _tracker;

        public StatefulSignalProxy(Func<string, ClientHubInvocation, IList<string>, Task> send, string signal, string hubName, string prefix, StateChangeTracker tracker)
            : base(send, signal, prefix, hubName, ListHelper<string>.Empty)
        {
            _tracker = tracker;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "The compiler generates calls to invoke this")]
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _tracker[binder.Name] = value;
            return true;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "The compiler generates calls to invoke this")]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _tracker[binder.Name];
            return true;
        }

        protected override ClientHubInvocation GetInvocationData(string method, object[] args)
        {
            return new ClientHubInvocation
            {
                Hub = HubName,
                Method = method,
                Args = args,
                Target = Signal,
                State = _tracker.GetChanges()
            };
        }
    }
}
