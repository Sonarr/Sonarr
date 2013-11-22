// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.SignalR.Hubs
{
    internal class HubInvokerContext : IHubIncomingInvokerContext
    {
        public HubInvokerContext(IHub hub, StateChangeTracker tracker, MethodDescriptor methodDescriptor, IList<object> args)
        {
            Hub = hub;
            MethodDescriptor = methodDescriptor;
            Args = args;
            StateTracker = tracker;
        }

        public IHub Hub
        {
            get;
            private set;
        }

        public MethodDescriptor MethodDescriptor
        {
            get;
            private set;
        }

        public IList<object> Args
        {
            get;
            private set;
        }


        public StateChangeTracker StateTracker
        {
            get;
            private set;
        }
    }
}
