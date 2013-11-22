// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Owin.Infrastructure;

namespace Microsoft.AspNet.SignalR.Owin.Handlers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HubDispatcherHandler
    {
        private readonly AppFunc _next;
        private readonly string _path;
        private readonly HubConfiguration _configuration;

        public HubDispatcherHandler(AppFunc next, string path, HubConfiguration configuration)
        {
            _next = next;
            _path = path;
            _configuration = configuration;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var path = environment.Get<string>(OwinConstants.RequestPath);
            if (path == null || !PrefixMatcher.IsMatch(_path, path))
            {
                return _next(environment);
            }

            var dispatcher = new HubDispatcher(_configuration);

            var handler = new CallHandler(_configuration, dispatcher);
            return handler.Invoke(environment);
        }
    }
}
