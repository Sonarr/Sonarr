// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Transports
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Disposable fields are disposed from a different method")]
    public class ForeverFrameTransport : ForeverTransport
    {
        private const string _initPrefix = "<!DOCTYPE html>" +
                                           "<html>" +
                                           "<head>" +
                                           "<title>SignalR Forever Frame Transport Stream</title>\r\n" +
                                           "<script>\r\n" + //"    debugger;\r\n"+
                                           "    var $ = window.parent.jQuery,\r\n" +
                                           "        ff = $ ? $.signalR.transports.foreverFrame : null,\r\n" +
                                           "        c =  ff ? ff.getConnection('";

        private const string _initSuffix = "') : null,\r\n" +
                                            "        r = ff ? ff.receive : function() {};\r\n" +
                                            "    ff ? ff.started(c) : '';" +
                                            "</script></head>" +
                                            "<body>\r\n";

        private HTMLTextWriter _htmlOutputWriter;

        public ForeverFrameTransport(HostContext context, IDependencyResolver resolver)
            : base(context, resolver)
        {
        }

        /// <summary>
        /// Pointed to the HTMLOutputWriter to wrap output stream with an HTML friendly one
        /// </summary>
        public override TextWriter OutputWriter
        {
            get
            {
                return HTMLOutputWriter;
            }
        }

        private HTMLTextWriter HTMLOutputWriter
        {
            get
            {
                if (_htmlOutputWriter == null)
                {
                    _htmlOutputWriter = new HTMLTextWriter(Context.Response);
                    _htmlOutputWriter.NewLine = "\n";
                }

                return _htmlOutputWriter;
            }
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

            var context = new ForeverFrameTransportContext(this, response);

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return EnqueueOperation(s => PerformSend(s), context);
        }

        protected internal override Task InitializeResponse(ITransportConnection connection)
        {
            uint frameId;
            string rawFrameId = Context.Request.QueryString["frameId"];
            if (String.IsNullOrWhiteSpace(rawFrameId) || !UInt32.TryParse(rawFrameId, NumberStyles.None, CultureInfo.InvariantCulture, out frameId))
            {
                // Invalid frameId passed in
                throw new InvalidOperationException(Resources.Error_InvalidForeverFrameId);
            }

            string initScript = _initPrefix +
                                frameId.ToString(CultureInfo.InvariantCulture) +
                                _initSuffix;

            var context = new ForeverFrameTransportContext(this, initScript);

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return base.InitializeResponse(connection).Then(s => Initialize(s), context);
        }

        private static Task Initialize(object state)
        {
            var context = (ForeverFrameTransportContext)state;

            var initContext = new ForeverFrameTransportContext(context.Transport, context.State);

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return WriteInit(initContext);
        }

        private static Task WriteInit(ForeverFrameTransportContext context)
        {
            context.Transport.Context.Response.ContentType = "text/html; charset=UTF-8";

            context.Transport.HTMLOutputWriter.WriteRaw((string)context.State);
            context.Transport.HTMLOutputWriter.Flush();

            return context.Transport.Context.Response.Flush();
        }

        private static Task PerformSend(object state)
        {
            var context = (ForeverFrameTransportContext)state;

            context.Transport.HTMLOutputWriter.WriteRaw("<script>r(c, ");
            context.Transport.JsonSerializer.Serialize(context.State, context.Transport.HTMLOutputWriter);
            context.Transport.HTMLOutputWriter.WriteRaw(");</script>\r\n");
            context.Transport.HTMLOutputWriter.Flush();

            return context.Transport.Context.Response.Flush();
        }

        private static Task PerformKeepAlive(object state)
        {
            var transport = (ForeverFrameTransport)state;

            transport.HTMLOutputWriter.WriteRaw("<script>r(c, {});</script>");
            transport.HTMLOutputWriter.WriteLine();
            transport.HTMLOutputWriter.WriteLine();
            transport.HTMLOutputWriter.Flush();

            return transport.Context.Response.Flush();
        }

        private class ForeverFrameTransportContext
        {
            public ForeverFrameTransport Transport;
            public object State;

            public ForeverFrameTransportContext(ForeverFrameTransport transport, object state)
            {
                Transport = transport;
                State = state;
            }
        }

        private class HTMLTextWriter : BufferTextWriter
        {
            public HTMLTextWriter(IResponse response)
                : base(response)
            {
            }

            public void WriteRaw(string value)
            {
                base.Write(value);
            }

            public override void Write(string value)
            {
                base.Write(JavascriptEncode(value));
            }

            public override void WriteLine(string value)
            {
                base.WriteLine(JavascriptEncode(value));
            }

            private static string JavascriptEncode(string input)
            {
                return input.Replace("<", "\\u003c").Replace(">", "\\u003e");
            }
        }
    }
}
