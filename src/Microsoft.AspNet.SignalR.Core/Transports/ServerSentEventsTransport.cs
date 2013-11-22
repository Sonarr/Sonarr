// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hosting;

namespace Microsoft.AspNet.SignalR.Transports
{
    public class ServerSentEventsTransport : ForeverTransport
    {
        public ServerSentEventsTransport(HostContext context, IDependencyResolver resolver)
            : base(context, resolver)
        {
        }

        public override Task KeepAlive()
        {
            if (InitializeTcs == null || !InitializeTcs.Task.IsCompleted)
            {
                return TaskAsyncHelper.Empty;
            }

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return EnqueueOperation(state => PerformKeepAlive(state), this);
        }

        public override Task Send(PersistentResponse response)
        {
            OnSendingResponse(response);

            var context = new SendContext(this, response);

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return EnqueueOperation(state => PerformSend(state), context);
        }

        protected internal override Task InitializeResponse(ITransportConnection connection)
        {
            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return base.InitializeResponse(connection)
                       .Then(s => WriteInit(s), this);
        }

        private static Task PerformKeepAlive(object state)
        {
            var transport = (ServerSentEventsTransport)state;

            transport.OutputWriter.Write("data: {}");
            transport.OutputWriter.WriteLine();
            transport.OutputWriter.WriteLine();
            transport.OutputWriter.Flush();

            return transport.Context.Response.Flush();
        }

        private static Task PerformSend(object state)
        {
            var context = (SendContext)state;

            context.Transport.OutputWriter.Write("data: ");
            context.Transport.JsonSerializer.Serialize(context.State, context.Transport.OutputWriter);
            context.Transport.OutputWriter.WriteLine();
            context.Transport.OutputWriter.WriteLine();
            context.Transport.OutputWriter.Flush();

            return context.Transport.Context.Response.Flush();
        }

        private static Task WriteInit(ServerSentEventsTransport transport)
        {
            transport.Context.Response.ContentType = "text/event-stream";

            // "data: initialized\n\n"
            transport.OutputWriter.Write("data: initialized");
            transport.OutputWriter.WriteLine();
            transport.OutputWriter.WriteLine();
            transport.OutputWriter.Flush();

            return transport.Context.Response.Flush();
        }

        private class SendContext
        {
            public ServerSentEventsTransport Transport;
            public object State;

            public SendContext(ServerSentEventsTransport transport, object state)
            {
                Transport = transport;
                State = state;
            }
        }
    }
}
