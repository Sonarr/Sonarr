// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Messaging
{
    internal class ScaleoutStreamManager
    {
        private readonly Func<int, IList<Message>, Task> _send;
        private readonly Action<int, ulong, ScaleoutMessage> _receive;
        private readonly ScaleoutStream[] _streams;

        public ScaleoutStreamManager(Func<int, IList<Message>, Task> send,
                                     Action<int, ulong, ScaleoutMessage> receive,
                                     int streamCount,
                                     TraceSource trace,
                                     IPerformanceCounterManager performanceCounters,
                                     ScaleoutConfiguration configuration)
        {
            _streams = new ScaleoutStream[streamCount];
            _send = send;
            _receive = receive;

            var receiveMapping = new ScaleoutMappingStore[streamCount];

            performanceCounters.ScaleoutStreamCountTotal.RawValue = streamCount;
            performanceCounters.ScaleoutStreamCountBuffering.RawValue = streamCount;
            performanceCounters.ScaleoutStreamCountOpen.RawValue = 0;

            for (int i = 0; i < streamCount; i++)
            {
                _streams[i] = new ScaleoutStream(trace, "Stream(" + i + ")", configuration.MaxQueueLength, performanceCounters);
                receiveMapping[i] = new ScaleoutMappingStore();
            }

            Streams = new ReadOnlyCollection<ScaleoutMappingStore>(receiveMapping);
        }

        public IList<ScaleoutMappingStore> Streams { get; private set; }

        public void Open(int streamIndex)
        {
            _streams[streamIndex].Open();
        }

        public void Close(int streamIndex)
        {
            _streams[streamIndex].Close();
        }

        public void OnError(int streamIndex, Exception exception)
        {
            _streams[streamIndex].SetError(exception);
        }

        public Task Send(int streamIndex, IList<Message> messages)
        {
            var context = new SendContext(this, streamIndex, messages);

            return _streams[streamIndex].Send(state => Send(state), context);
        }

        public void OnReceived(int streamIndex, ulong id, ScaleoutMessage message)
        {
            _receive(streamIndex, id, message);

            // We assume if a message has come in then the stream is open
            Open(streamIndex);
        }

        private static Task Send(object state)
        {
            var context = (SendContext)state;

            return context.StreamManager._send(context.Index, context.Messages);
        }

        private class SendContext
        {
            public ScaleoutStreamManager StreamManager;
            public int Index;
            public IList<Message> Messages;

            public SendContext(ScaleoutStreamManager scaleoutStream, int index, IList<Message> messages)
            {
                StreamManager = scaleoutStream;
                Index = index;
                Messages = messages;
            }
        }
    }
}
