// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Transports
{
    internal class HttpRequestLifeTime
    {
        private readonly TaskCompletionSource<object> _lifetimeTcs = new TaskCompletionSource<object>();
        private readonly TransportDisconnectBase _transport;
        private readonly TaskQueue _writeQueue;
        private readonly TraceSource _trace;
        private readonly string _connectionId;

        public HttpRequestLifeTime(TransportDisconnectBase transport, TaskQueue writeQueue, TraceSource trace, string connectionId)
        {
            _transport = transport;
            _trace = trace;
            _connectionId = connectionId;
            _writeQueue = writeQueue;
        }

        public Task Task
        {
            get
            {
                return _lifetimeTcs.Task;
            }
        }

        public void Complete()
        {
            Complete(error: null);
        }

        public void Complete(Exception error)
        {
            _trace.TraceEvent(TraceEventType.Verbose, 0, "DrainWrites(" + _connectionId + ")");

            var context = new LifetimeContext(_transport, _lifetimeTcs, error);

            _transport.ApplyState(TransportConnectionStates.QueueDrained);

            // Drain the task queue for pending write operations so we don't end the request and then try to write
            // to a corrupted request object.
            _writeQueue.Drain().Catch().Finally(state =>
            {
                // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
                ((LifetimeContext)state).Complete();
            },
            context);

            if (error != null)
            {
                _trace.TraceEvent(TraceEventType.Error, 0, "CompleteRequest (" + _connectionId + ") failed: " + error.GetBaseException());
            }
            else
            {
                _trace.TraceInformation("CompleteRequest (" + _connectionId + ")");
            }
        }

        private class LifetimeContext
        {
            private readonly TaskCompletionSource<object> _lifetimeTcs;
            private readonly Exception _error;
            private readonly TransportDisconnectBase _transport;

            public LifetimeContext(TransportDisconnectBase transport, TaskCompletionSource<object> lifeTimetcs, Exception error)
            {
                _transport = transport;
                _lifetimeTcs = lifeTimetcs;
                _error = error;
            }

            public void Complete()
            {
                _transport.ApplyState(TransportConnectionStates.HttpRequestEnded);

                if (_error != null)
                {
                    _lifetimeTcs.TrySetUnwrappedException(_error);
                }
                else
                {
                    _lifetimeTcs.TrySetResult(null);
                }
            }
        }
    }
}
