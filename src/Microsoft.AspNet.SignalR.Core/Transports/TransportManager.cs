// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNet.SignalR.Hosting;

namespace Microsoft.AspNet.SignalR.Transports
{
    /// <summary>
    /// The default <see cref="ITransportManager"/> implementation.
    /// </summary>
    public class TransportManager : ITransportManager
    {
        private readonly ConcurrentDictionary<string, Func<HostContext, ITransport>> _transports = new ConcurrentDictionary<string, Func<HostContext, ITransport>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of <see cref="TransportManager"/> class.
        /// </summary>
        /// <param name="resolver">The default <see cref="IDependencyResolver"/>.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Those are factory methods")]
        public TransportManager(IDependencyResolver resolver)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException("resolver");
            }

            Register("foreverFrame", context => new ForeverFrameTransport(context, resolver));
            Register("serverSentEvents", context => new ServerSentEventsTransport(context, resolver));
            Register("longPolling", context => new LongPollingTransport(context, resolver));
            Register("webSockets", context => new WebSocketTransport(context, resolver));
        }

        /// <summary>
        /// Adds a new transport to the list of supported transports.
        /// </summary>
        /// <param name="transportName">The specified transport.</param>
        /// <param name="transportFactory">The factory method for the specified transport.</param>
        public void Register(string transportName, Func<HostContext, ITransport> transportFactory)
        {
            if (String.IsNullOrEmpty(transportName))
            {
                throw new ArgumentNullException("transportName");
            }

            if (transportFactory == null)
            {
                throw new ArgumentNullException("transportFactory");
            }

            _transports.TryAdd(transportName, transportFactory);
        }

        /// <summary>
        /// Removes a transport from the list of supported transports.
        /// </summary>
        /// <param name="transportName">The specified transport.</param>
        public void Remove(string transportName)
        {
            if (String.IsNullOrEmpty(transportName))
            {
                throw new ArgumentNullException("transportName");
            }

            Func<HostContext, ITransport> removed;
            _transports.TryRemove(transportName, out removed);
        }

        /// <summary>
        /// Gets the specified transport for the specified <see cref="HostContext"/>.
        /// </summary>
        /// <param name="hostContext">The <see cref="HostContext"/> for the current request.</param>
        /// <returns>The <see cref="ITransport"/> for the specified <see cref="HostContext"/>.</returns>
        public ITransport GetTransport(HostContext hostContext)
        {
            if (hostContext == null)
            {
                throw new ArgumentNullException("hostContext");
            }

            string transportName = hostContext.Request.QueryString["transport"];

            if (String.IsNullOrEmpty(transportName))
            {
                return null;
            }

            Func<HostContext, ITransport> factory;
            if (_transports.TryGetValue(transportName, out factory))
            {
                return factory(hostContext);
            }

            return null;
        }

        /// <summary>
        /// Determines whether the specified transport is supported.
        /// </summary>
        /// <param name="transportName">The name of the transport to test.</param>
        /// <returns>True if the transport is supported, otherwise False.</returns>
        public bool SupportsTransport(string transportName)
        {
            if (String.IsNullOrEmpty(transportName))
            {
                return false;
            }

            return _transports.ContainsKey(transportName);
        }
    }
}
