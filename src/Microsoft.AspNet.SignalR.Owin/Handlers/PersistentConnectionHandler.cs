// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Owin.Infrastructure;

namespace Microsoft.AspNet.SignalR.Owin.Handlers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class PersistentConnectionHandler
    {
        private readonly AppFunc _next;
        private readonly string _path;
        private readonly Type _connectionType;
        private readonly ConnectionConfiguration _configuration;

        public PersistentConnectionHandler(AppFunc next, string path, Type connectionType, ConnectionConfiguration configuration)
        {
            _next = next;
            _path = path;
            _connectionType = connectionType;
            _configuration = configuration;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var path = environment.Get<string>(OwinConstants.RequestPath);
            if (path == null || !PrefixMatcher.IsMatch(_path, path))
            {
                return _next(environment);
            }

            var connectionFactory = new PersistentConnectionFactory(_configuration.Resolver);
            var connection = connectionFactory.CreateInstance(_connectionType);

            var handler = new CallHandler(_configuration, connection);
            return handler.Invoke(environment);
        }
    }
}
