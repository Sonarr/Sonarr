// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Tracing;
using Microsoft.AspNet.SignalR.Transports;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    public class Connection : IConnection, ITransportConnection, ISubscriber
    {
        private readonly IMessageBus _bus;
        private readonly IJsonSerializer _serializer;
        private readonly string _baseSignal;
        private readonly string _connectionId;
        private readonly IList<string> _signals;
        private readonly DiffSet<string> _groups;
        private readonly IPerformanceCounterManager _counters;

        private bool _disconnected;
        private bool _aborted;
        private readonly TraceSource _traceSource;
        private readonly IAckHandler _ackHandler;
        private readonly IProtectedData _protectedData;

        public Connection(IMessageBus newMessageBus,
                          IJsonSerializer jsonSerializer,
                          string baseSignal,
                          string connectionId,
                          IList<string> signals,
                          IList<string> groups,
                          ITraceManager traceManager,
                          IAckHandler ackHandler,
                          IPerformanceCounterManager performanceCounterManager,
                          IProtectedData protectedData)
        {
            if (traceManager == null)
            {
                throw new ArgumentNullException("traceManager");
            }

            _bus = newMessageBus;
            _serializer = jsonSerializer;
            _baseSignal = baseSignal;
            _connectionId = connectionId;
            _signals = new List<string>(signals.Concat(groups));
            _groups = new DiffSet<string>(groups);
            _traceSource = traceManager["SignalR.Connection"];
            _ackHandler = ackHandler;
            _counters = performanceCounterManager;
            _protectedData = protectedData;
        }

        public string DefaultSignal
        {
            get
            {
                return _baseSignal;
            }
        }

        IList<string> ISubscriber.EventKeys
        {
            get
            {
                return _signals;
            }
        }

        public event Action<ISubscriber, string> EventKeyAdded;

        public event Action<ISubscriber, string> EventKeyRemoved;

        public Action<TextWriter> WriteCursor { get; set; }

        public string Identity
        {
            get
            {
                return _connectionId;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for debugging purposes.")]
        private TraceSource Trace
        {
            get
            {
                return _traceSource;
            }
        }

        public Subscription Subscription
        {
            get;
            set;
        }

        public Task Send(ConnectionMessage message)
        {
            Message busMessage = CreateMessage(message.Signal, message.Value);

            if (message.ExcludedSignals != null)
            {
                busMessage.Filter = String.Join("|", message.ExcludedSignals);
            }

            if (busMessage.WaitForAck)
            {
                Task ackTask = _ackHandler.CreateAck(busMessage.CommandId);
                return _bus.Publish(busMessage).Then(task => task, ackTask);
            }

            return _bus.Publish(busMessage);
        }

        private Message CreateMessage(string key, object value)
        {
            var command = value as Command;

            ArraySegment<byte> messageBuffer = GetMessageBuffer(value);

            var message = new Message(_connectionId, key, messageBuffer);

            if (command != null)
            {
                // Set the command id
                message.CommandId = command.Id;
                message.WaitForAck = command.WaitForAck;
            }

            return message;
        }

        private ArraySegment<byte> GetMessageBuffer(object value)
        {
            using (var stream = new MemoryStream(128))
            {
                var bufferWriter = new BufferTextWriter((buffer, state) =>
                {
                    ((MemoryStream)state).Write(buffer.Array, buffer.Offset, buffer.Count);
                },
                stream,
                reuseBuffers: true,
                bufferSize: 1024);

                using (bufferWriter)
                {
                    _serializer.Serialize(value, bufferWriter);
                    bufferWriter.Flush();

                    return new ArraySegment<byte>(stream.ToArray());
                }
            }
        }

        public IDisposable Receive(string messageId, Func<PersistentResponse, object, Task<bool>> callback, int maxMessages, object state)
        {
            var receiveContext = new ReceiveContext(this, callback, state);

            return _bus.Subscribe(this,
                                  messageId,
                                  (result, s) => MessageBusCallback(result, s),
                                  maxMessages,
                                  receiveContext);
        }

        private static Task<bool> MessageBusCallback(MessageResult result, object state)
        {
            var context = (ReceiveContext)state;

            return context.InvokeCallback(result);
        }

        private PersistentResponse GetResponse(MessageResult result)
        {
            // Do a single sweep through the results to process commands and extract values
            ProcessResults(result);

            Debug.Assert(WriteCursor != null, "Unable to resolve the cursor since the method is null");

            var response = new PersistentResponse(ExcludeMessage, WriteCursor);
            response.Terminal = result.Terminal;

            if (!result.Terminal)
            {
                // Only set these properties if the message isn't terminal
                response.Messages = result.Messages;
                response.Disconnect = _disconnected;
                response.Aborted = _aborted;
                response.TotalCount = result.TotalCount;
            }

            PopulateResponseState(response);

            _counters.ConnectionMessagesReceivedTotal.IncrementBy(result.TotalCount);
            _counters.ConnectionMessagesReceivedPerSec.IncrementBy(result.TotalCount);

            return response;
        }

        private bool ExcludeMessage(Message message)
        {
            if (String.IsNullOrEmpty(message.Filter))
            {
                return false;
            }

            string[] exclude = message.Filter.Split('|');

            return exclude.Any(signal => Identity.Equals(signal, StringComparison.OrdinalIgnoreCase) ||
                                                    _signals.Contains(signal) ||
                                                    _groups.Contains(signal));
        }

        private void ProcessResults(MessageResult result)
        {
            result.Messages.Enumerate<object>(message => message.IsAck || message.IsCommand,
                                              (state, message) =>
                                              {
                                                  if (message.IsAck)
                                                  {
                                                      _ackHandler.TriggerAck(message.CommandId);
                                                  }
                                                  else if (message.IsCommand)
                                                  {
                                                      var command = _serializer.Parse<Command>(message.Value, message.Encoding);
                                                      ProcessCommand(command);

                                                      // Only send the ack if this command is waiting for it
                                                      if (message.WaitForAck)
                                                      {
                                                          // If we're on the same box and there's a pending ack for this command then
                                                          // just trip it
                                                          if (!_ackHandler.TriggerAck(message.CommandId))
                                                          {
                                                              _bus.Ack(_connectionId, message.CommandId).Catch();
                                                          }
                                                      }
                                                  }
                                              }, null);
        }

        private void ProcessCommand(Command command)
        {
            switch (command.CommandType)
            {
                case CommandType.AddToGroup:
                    {
                        var name = command.Value;

                        if (EventKeyAdded != null)
                        {
                            _groups.Add(name);
                            EventKeyAdded(this, name);
                        }
                    }
                    break;
                case CommandType.RemoveFromGroup:
                    {
                        var name = command.Value;

                        if (EventKeyRemoved != null)
                        {
                            _groups.Remove(name);
                            EventKeyRemoved(this, name);
                        }
                    }
                    break;
                case CommandType.Disconnect:
                    _disconnected = true;
                    break;
                case CommandType.Abort:
                    _aborted = true;
                    break;
            }
        }

        private void PopulateResponseState(PersistentResponse response)
        {
            PopulateResponseState(response, _groups, _serializer, _protectedData, _connectionId);
        }

        internal static void PopulateResponseState(PersistentResponse response,
                                                   DiffSet<string> groupSet,
                                                   IJsonSerializer serializer,
                                                   IProtectedData protectedData,
                                                   string connectionId)
        {
            bool anyChanges = groupSet.DetectChanges();

            if (anyChanges)
            {
                // Create a protected payload of the sorted list
                IEnumerable<string> groups = groupSet.GetSnapshot();

                // Remove group prefixes before any thing goes over the wire
                string groupsString = connectionId + ':' + serializer.Stringify(PrefixHelper.RemoveGroupPrefixes(groups)); ;

                // The groups token
                response.GroupsToken = protectedData.Protect(groupsString, Purposes.Groups);
            }
        }

        private class ReceiveContext
        {
            private readonly Connection _connection;
            private readonly Func<PersistentResponse, object, Task<bool>> _callback;
            private readonly object _callbackState;

            public ReceiveContext(Connection connection, Func<PersistentResponse, object, Task<bool>> callback, object callbackState)
            {
                _connection = connection;
                _callback = callback;
                _callbackState = callbackState;
            }

            public Task<bool> InvokeCallback(MessageResult result)
            {
                var response = _connection.GetResponse(result);

                return _callback(response, _callbackState);
            }
        }
    }
}
