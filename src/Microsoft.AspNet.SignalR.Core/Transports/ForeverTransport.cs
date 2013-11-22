// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNet.SignalR.Tracing;

namespace Microsoft.AspNet.SignalR.Transports
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "The disposer is an optimization")]
    public abstract class ForeverTransport : TransportDisconnectBase, ITransport
    {
        private readonly IPerformanceCounterManager _counters;
        private IJsonSerializer _jsonSerializer;
        private string _lastMessageId;

        private const int MaxMessages = 10;

        protected ForeverTransport(HostContext context, IDependencyResolver resolver)
            : this(context,
                   resolver.Resolve<IJsonSerializer>(),
                   resolver.Resolve<ITransportHeartbeat>(),
                   resolver.Resolve<IPerformanceCounterManager>(),
                   resolver.Resolve<ITraceManager>())
        {
        }

        protected ForeverTransport(HostContext context,
                                   IJsonSerializer jsonSerializer,
                                   ITransportHeartbeat heartbeat,
                                   IPerformanceCounterManager performanceCounterWriter,
                                   ITraceManager traceManager)
            : base(context, heartbeat, performanceCounterWriter, traceManager)
        {
            _jsonSerializer = jsonSerializer;
            _counters = performanceCounterWriter;
        }

        protected string LastMessageId
        {
            get
            {
                if (_lastMessageId == null)
                {
                    _lastMessageId = Context.Request.QueryString["messageId"];
                }

                return _lastMessageId;
            }
        }

        protected IJsonSerializer JsonSerializer
        {
            get { return _jsonSerializer; }
        }

        internal TaskCompletionSource<object> InitializeTcs { get; set; }

        protected virtual void OnSending(string payload)
        {
            Heartbeat.MarkConnection(this);
        }

        protected virtual void OnSendingResponse(PersistentResponse response)
        {
            Heartbeat.MarkConnection(this);
        }

        public Func<string, Task> Received { get; set; }

        public Func<Task> TransportConnected { get; set; }

        public Func<Task> Connected { get; set; }

        public Func<Task> Reconnected { get; set; }

        // Unit testing hooks
        internal Action AfterReceive;
        internal Action BeforeCancellationTokenCallbackRegistered;
        internal Action BeforeReceive;
        internal Action<Exception> AfterRequestEnd;

        protected override void InitializePersistentState()
        {
            // PersistentConnection.OnConnected must complete before we can write to the output stream,
            // so clients don't indicate the connection has started too early.
            InitializeTcs = new TaskCompletionSource<object>();
            WriteQueue = new TaskQueue(InitializeTcs.Task);

            base.InitializePersistentState();
        }

        protected Task ProcessRequestCore(ITransportConnection connection)
        {
            Connection = connection;

            if (Context.Request.Url.LocalPath.EndsWith("/send", StringComparison.OrdinalIgnoreCase))
            {
                return ProcessSendRequest();
            }
            else if (IsAbortRequest)
            {
                return Connection.Abort(ConnectionId);
            }
            else
            {
                InitializePersistentState();

                return ProcessReceiveRequest(connection);
            }
        }

        public virtual Task ProcessRequest(ITransportConnection connection)
        {
            return ProcessRequestCore(connection);
        }

        public abstract Task Send(PersistentResponse response);

        public virtual Task Send(object value)
        {
            var context = new ForeverTransportContext(this, value);

            return EnqueueOperation(state => PerformSend(state), context);
        }

        protected internal virtual Task InitializeResponse(ITransportConnection connection)
        {
            return TaskAsyncHelper.Empty;
        }

        protected internal override Task EnqueueOperation(Func<object, Task> writeAsync, object state)
        {
            Task task = base.EnqueueOperation(writeAsync, state);

            // If PersistentConnection.OnConnected has not completed (as indicated by InitializeTcs),
            // the queue will be blocked to prevent clients from prematurely indicating the connection has
            // started, but we must keep receive loop running to continue processing commands and to
            // prevent deadlocks caused by waiting on ACKs.
            if (InitializeTcs == null || InitializeTcs.Task.IsCompleted)
            {
                return task;
            }

            return TaskAsyncHelper.Empty;
        }

        private Task ProcessSendRequest()
        {
            string data = Context.Request.Form["data"];

            if (Received != null)
            {
                return Received(data);
            }

            return TaskAsyncHelper.Empty;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are flowed to the caller.")]
        private Task ProcessReceiveRequest(ITransportConnection connection)
        {
            Func<Task> initialize = null;

            bool newConnection = Heartbeat.AddConnection(this);

            if (IsConnectRequest)
            {
                if (newConnection)
                {
                    initialize = Connected;

                    _counters.ConnectionsConnected.Increment();
                }
            }
            else
            {
                initialize = Reconnected;
            }

            var series = new Func<object, Task>[]
            { 
                state => ((Func<Task>)state).Invoke(),
                state => ((Func<Task>)state).Invoke()
            };

            var states = new object[] { TransportConnected ?? _emptyTaskFunc,
                                        initialize ?? _emptyTaskFunc };

            Func<Task> fullInit = () => TaskAsyncHelper.Series(series, states);

            return ProcessMessages(connection, fullInit);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The object is disposed otherwise")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are flowed to the caller.")]
        private Task ProcessMessages(ITransportConnection connection, Func<Task> initialize)
        {
            var disposer = new Disposer();

            if (BeforeCancellationTokenCallbackRegistered != null)
            {
                BeforeCancellationTokenCallbackRegistered();
            }

            var cancelContext = new ForeverTransportContext(this, disposer);

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            IDisposable registration = ConnectionEndToken.SafeRegister(state => Cancel(state), cancelContext);

            var lifetime = new RequestLifetime(this, _requestLifeTime);
            var messageContext = new MessageContext(this, lifetime, registration);

            if (BeforeReceive != null)
            {
                BeforeReceive();
            }

            try
            {
                // Ensure we enqueue the response initialization before any messages are received
                EnqueueOperation(state => InitializeResponse((ITransportConnection)state), connection)
                    .Catch((ex, state) => OnError(ex, state), messageContext);

                // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
                IDisposable subscription = connection.Receive(LastMessageId,
                                                              (response, state) => OnMessageReceived(response, state),
                                                               MaxMessages,
                                                               messageContext);


                disposer.Set(subscription);

                if (AfterReceive != null)
                {
                    AfterReceive();
                }

                // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
                initialize().Then(tcs => tcs.TrySetResult(null), InitializeTcs)
                            .Catch((ex, state) => OnError(ex, state), messageContext);
            }
            catch (OperationCanceledException ex)
            {
                InitializeTcs.TrySetCanceled();

                lifetime.Complete(ex);
            }
            catch (Exception ex)
            {
                InitializeTcs.TrySetCanceled();

                lifetime.Complete(ex);
            }

            return _requestLifeTime.Task;
        }

        private static void Cancel(object state)
        {
            var context = (ForeverTransportContext)state;

            context.Transport.Trace.TraceEvent(TraceEventType.Verbose, 0, "Cancel(" + context.Transport.ConnectionId + ")");

            ((IDisposable)context.State).Dispose();
        }

        private static Task<bool> OnMessageReceived(PersistentResponse response, object state)
        {
            var context = (MessageContext)state;

            response.TimedOut = context.Transport.IsTimedOut;

            // If we're telling the client to disconnect then clean up the instantiated connection.
            if (response.Disconnect)
            {
                // Send the response before removing any connection data
                return context.Transport.Send(response).Then(c => OnDisconnectMessage(c), context)
                                        .Then(() => TaskAsyncHelper.False);
            }
            else if (response.TimedOut || response.Aborted)
            {
                context.Registration.Dispose();

                if (response.Aborted)
                {
                    // If this was a clean disconnect raise the event.
                    return context.Transport.Abort()
                                            .Then(() => TaskAsyncHelper.False);
                }
            }

            if (response.Terminal)
            {
                // End the request on the terminal response
                context.Lifetime.Complete();

                return TaskAsyncHelper.False;
            }

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return context.Transport.Send(response)
                                    .Then(() => TaskAsyncHelper.True);
        }

        private static void OnDisconnectMessage(MessageContext context)
        {
            context.Transport.ApplyState(TransportConnectionStates.DisconnectMessageReceived);

            context.Registration.Dispose();

            // Remove connection without triggering disconnect
            context.Transport.Heartbeat.RemoveConnection(context.Transport);
        }

        private static Task PerformSend(object state)
        {
            var context = (ForeverTransportContext)state;

            if (!context.Transport.IsAlive)
            {
                return TaskAsyncHelper.Empty;
            }

            context.Transport.Context.Response.ContentType = JsonUtility.JsonMimeType;

            context.Transport.JsonSerializer.Serialize(context.State, context.Transport.OutputWriter);
            context.Transport.OutputWriter.Flush();

            return context.Transport.Context.Response.End();
        }

        private static void OnError(AggregateException ex, object state)
        {
            var context = (MessageContext)state;

            context.Transport.IncrementErrors();

            // Cancel any pending writes in the queue
            context.Transport.InitializeTcs.TrySetCanceled();

            // Complete the http request
            context.Lifetime.Complete(ex);
        }

        private class ForeverTransportContext
        {
            public object State;
            public ForeverTransport Transport;

            public ForeverTransportContext(ForeverTransport foreverTransport, object state)
            {
                State = state;
                Transport = foreverTransport;
            }
        }

        private class MessageContext
        {
            public ForeverTransport Transport;
            public RequestLifetime Lifetime;
            public IDisposable Registration;

            public MessageContext(ForeverTransport transport, RequestLifetime lifetime, IDisposable registration)
            {
                Registration = registration;
                Lifetime = lifetime;
                Transport = transport;
            }
        }

        private class RequestLifetime
        {
            private readonly HttpRequestLifeTime _lifetime;
            private readonly ForeverTransport _transport;

            public RequestLifetime(ForeverTransport transport, HttpRequestLifeTime lifetime)
            {
                _lifetime = lifetime;
                _transport = transport;
            }

            public void Complete()
            {
                Complete(error: null);
            }

            public void Complete(Exception error)
            {
                _lifetime.Complete(error);

                _transport.Dispose();

                if (_transport.AfterRequestEnd != null)
                {
                    _transport.AfterRequestEnd(error);
                }
            }
        }
    }
}
