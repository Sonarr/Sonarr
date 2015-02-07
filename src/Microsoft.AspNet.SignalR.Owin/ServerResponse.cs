// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Owin.Infrastructure;

namespace Microsoft.AspNet.SignalR.Owin
{
    public partial class ServerResponse : IResponse
    {
        private readonly CancellationToken _callCancelled;
        private readonly IDictionary<string, object> _environment;
        private Stream _responseBody;

        public ServerResponse(IDictionary<string, object> environment)
        {
            _environment = environment;
            _callCancelled = _environment.Get<CancellationToken>(OwinConstants.CallCancelled);
        }

        public CancellationToken CancellationToken
        {
            get { return _callCancelled; }
        }

        public int StatusCode 
        {
            get
            {
                return _environment.Get<int>(OwinConstants.ResponseStatusCode);
            }
            set
            {
                _environment[OwinConstants.ResponseStatusCode] = value;
            }
        }

        public string ContentType
        {
            get { return ResponseHeaders.GetHeader("Content-Type"); }
            set { ResponseHeaders.SetHeader("Content-Type", value); }
        }

        public void Write(ArraySegment<byte> data)
        {
            ResponseBody.Write(data.Array, data.Offset, data.Count);
        }

        public Task Flush()
        {
#if NET45
            return ResponseBody.FlushAsync();
#else
            return TaskAsyncHelper.FromMethod(() => ResponseBody.Flush());
#endif
        }

        public Task End()
        {
            return TaskAsyncHelper.Empty;
        }

        public IDictionary<string, string[]> ResponseHeaders
        {
            get { return _environment.Get<IDictionary<string, string[]>>(OwinConstants.ResponseHeaders); }
        }

        public Stream ResponseBody
        {
            get
            {
                if (_responseBody == null)
                {
                    _responseBody = _environment.Get<Stream>(OwinConstants.ResponseBody);
                }

                return _responseBody;
            }
        }

        public Action DisableResponseBuffering
        {
            get { return _environment.Get<Action>(OwinConstants.DisableResponseBuffering) ?? (() => { }); }
        }
    }
}
