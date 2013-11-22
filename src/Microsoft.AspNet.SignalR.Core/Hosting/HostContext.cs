// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Microsoft.AspNet.SignalR.Hosting
{
    public class HostContext
    {
        public IRequest Request { get; private set; }
        public IResponse Response { get; private set; }
        public IDictionary<string, object> Items { get; private set; }

        public HostContext(IRequest request, IResponse response)
        {
            Request = request;
            Response = response;
            Items = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
