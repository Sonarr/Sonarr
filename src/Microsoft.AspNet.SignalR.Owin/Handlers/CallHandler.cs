// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Owin.Infrastructure;

namespace Microsoft.AspNet.SignalR.Owin
{
    public class CallHandler
    {
        private readonly ConnectionConfiguration _configuration;
        private readonly PersistentConnection _connection;

        public CallHandler(ConnectionConfiguration configuration, PersistentConnection connection)
        {
            _configuration = configuration;
            _connection = connection;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var serverRequest = new ServerRequest(environment);
            var serverResponse = new ServerResponse(environment);
            var hostContext = new HostContext(serverRequest, serverResponse);

            string origin = serverRequest.RequestHeaders.GetHeader("Origin");

            if (_configuration.EnableCrossDomain)
            {
                // Add CORS response headers support
                if (!String.IsNullOrEmpty(origin))
                {
                    serverResponse.ResponseHeaders.SetHeader("Access-Control-Allow-Origin", origin);
                    serverResponse.ResponseHeaders.SetHeader("Access-Control-Allow-Credentials", "true");
                }
            }
            else
            {
                string callback = serverRequest.QueryString["callback"];

                // If it's a JSONP request and we're not allowing cross domain requests then block it
                // If there's an origin header and it's not a same origin request then block it.

                if (!String.IsNullOrEmpty(callback) || 
                    (!String.IsNullOrEmpty(origin) && !IsSameOrigin(serverRequest.Url, origin)))
                {
                    return EndResponse(environment, 403, Resources.Forbidden_CrossDomainIsDisabled);
                }
            }

            // Add the nosniff header for all responses to prevent IE from trying to sniff mime type from contents
            serverResponse.ResponseHeaders.SetHeader("X-Content-Type-Options", "nosniff");

            // REVIEW: Performance
            hostContext.Items[HostConstants.SupportsWebSockets] = environment.SupportsWebSockets();
            hostContext.Items[HostConstants.ShutdownToken] = environment.GetShutdownToken();
            hostContext.Items[HostConstants.DebugMode] = environment.GetIsDebugEnabled();

            serverRequest.DisableRequestCompression();
            serverResponse.DisableResponseBuffering();

            _connection.Initialize(_configuration.Resolver, hostContext);

            if (!_connection.Authorize(serverRequest))
            {
                // If we failed to authorize the request then return a 403 since the request
                // can't do anything
                return EndResponse(environment, 403, "Forbidden");
            }
            else
            {
                return _connection.ProcessRequest(hostContext);
            }
        }

        private static Task EndResponse(IDictionary<string, object> environment, int statusCode, string reason)
        {
            environment[OwinConstants.ResponseStatusCode] = statusCode;
            environment[OwinConstants.ResponseReasonPhrase] = reason;

            return TaskAsyncHelper.Empty;
        }

        private static bool IsSameOrigin(Uri requestUri, string origin)
        {
            Uri originUri;
            if (!Uri.TryCreate(origin.Trim(), UriKind.Absolute, out originUri))
            {
                return false;
            }

            return (requestUri.Scheme == originUri.Scheme) &&
                   (requestUri.Host == originUri.Host) &&
                   (requestUri.Port == originUri.Port);
        }
    }
}
