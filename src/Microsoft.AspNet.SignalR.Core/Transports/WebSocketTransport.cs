// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNet.SignalR.Tracing;

namespace Microsoft.AspNet.SignalR.Transports
{
    public class WebSocketTransport : ForeverTransport
    {
        private readonly HostContext _context;
        private IWebSocket _socket;
        private bool _isAlive = true;

        private readonly Action<string> _message;
        private readonly Action<bool> _closed;
        private readonly Action<Exception> _error;

        public WebSocketTransport(HostContext context,
                                  IDependencyResolver resolver)
            : this(context,
                   resolver.Resolve<IJsonSerializer>(),
                   resolver.Resolve<ITransportHeartbeat>(),
                   resolver.Resolve<IPerformanceCounterManager>(),
                   resolver.Resolve<ITraceManager>())
        {
        }

        public WebSocketTransport(HostContext context,
                                  IJsonSerializer serializer,
                                  ITransportHeartbeat heartbeat,
                                  IPerformanceCounterManager performanceCounterWriter,
                                  ITraceManager traceManager)
            : base(context, serializer, heartbeat, performanceCounterWriter, traceManager)
        {
            _context = context;
            _message = OnMessage;
            _closed = OnClosed;
            _error = OnError;
        }

        public override bool IsAlive
        {
            get
            {
                return _isAlive;
            }
        }

        public override CancellationToken CancellationToken
        {
            get
            {
                return CancellationToken.None;
            }
        }

        public override Task KeepAlive()
        {
            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return EnqueueOperation(state =>
            {
                var webSocket = (IWebSocket)state;
                return webSocket.Send("{}");
            },
            _socket);
        }

        public override Task ProcessRequest(ITransportConnection connection)
        {
            var webSocketRequest = _context.Request as IWebSocketRequest;

            // Throw if the server implementation doesn't support websockets
            if (webSocketRequest == null)
            {
                throw new InvalidOperationException(Resources.Error_WebSocketsNotSupported);
            }

            return webSocketRequest.AcceptWebSocketRequest(socket =>
            {
                _socket = socket;
                socket.OnClose = _closed;
                socket.OnMessage = _message;
                socket.OnError = _error;

                return ProcessRequestCore(connection);
            });
        }

        protected override TextWriter CreateResponseWriter()
        {
            return new BufferTextWriter(_socket);
        }

        public override Task Send(object value)
        {
            var context = new WebSocketTransportContext(this, value);

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return EnqueueOperation(state => PerformSend(state), context);
        }

        public override Task Send(PersistentResponse response)
        {
            OnSendingResponse(response);

            return Send((object)response);
        }

        private static Task PerformSend(object state)
        {
            var context = (WebSocketTransportContext)state;

            context.Transport.JsonSerializer.Serialize(context.State, context.Transport.OutputWriter);
            context.Transport.OutputWriter.Flush();

            return context.Transport._socket.Flush();
        }

        private void OnMessage(string message)
        {
            if (Received != null)
            {
                Received(message).Catch();
            }
        }

        private void OnClosed(bool clean)
        {
            Trace.TraceInformation("CloseSocket({0}, {1})", clean, ConnectionId);

            // If we performed a clean disconnect then we go through the normal disconnect routine.  However,
            // If we performed an unclean disconnect we want to mark the connection as "not alive" and let the
            // HeartBeat clean it up.  This is to maintain consistency across the transports.
            if (clean)
            {
                Abort();
            }

            _isAlive = false;
        }

        private void OnError(Exception error)
        {
            Trace.TraceError("OnError({0}, {1})", ConnectionId, error);
        }

        private class WebSocketTransportContext
        {
            public WebSocketTransport Transport;
            public object State;

            public WebSocketTransportContext(WebSocketTransport transport, object state)
            {
                Transport = transport;
                State = state;
            }
        }
    }
}