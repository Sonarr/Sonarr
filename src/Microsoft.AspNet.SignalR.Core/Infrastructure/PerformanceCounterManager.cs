// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
#if !UTILS
using Microsoft.AspNet.SignalR.Tracing;
#endif

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    /// <summary>
    /// Manages performance counters using Windows performance counters.
    /// </summary>
    public class PerformanceCounterManager : IPerformanceCounterManager
    {
        /// <summary>
        /// The performance counter category name for SignalR counters.
        /// </summary>
        public const string CategoryName = "SignalR";

        private readonly static PropertyInfo[] _counterProperties = GetCounterPropertyInfo();
        private readonly static IPerformanceCounter _noOpCounter = new NoOpPerformanceCounter();
        private volatile bool _initialized;
        private object _initLocker = new object();

#if !UTILS
        private readonly TraceSource _trace;

        public PerformanceCounterManager(DefaultDependencyResolver resolver)
            : this(resolver.Resolve<ITraceManager>())
        {

        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public PerformanceCounterManager(ITraceManager traceManager)
            : this()
        {
            if (traceManager == null)
            {
                throw new ArgumentNullException("traceManager");
            }

            _trace = traceManager["SignalR.PerformanceCounterManager"];
        }
#endif

        public PerformanceCounterManager()
        {
            InitNoOpCounters();
        }

        /// <summary>
        /// Gets the performance counter representing the total number of connection Connect events since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Connections Connected", Description = "The total number of connection Connect events since the application was started.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ConnectionsConnected { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the total number of connection Reconnect events since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Connections Reconnected", Description = "The total number of connection Reconnect events since the application was started.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ConnectionsReconnected { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the total number of connection Disconnect events since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Connections Disconnected", Description = "The total number of connection Disconnect events since the application was started.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ConnectionsDisconnected { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of connections currently connected.
        /// </summary>
        [PerformanceCounter(Name = "Connections Current", Description = "The number of connections currently connected.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ConnectionsCurrent { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the toal number of messages received by connections (server to client) since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Connection Messages Received Total", Description = "The toal number of messages received by connections (server to client) since the application was started.", CounterType = PerformanceCounterType.NumberOfItems64)]
        public IPerformanceCounter ConnectionMessagesReceivedTotal { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the total number of messages sent by connections (client to server) since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Connection Messages Sent Total", Description = "The total number of messages sent by connections (client to server) since the application was started.", CounterType = PerformanceCounterType.NumberOfItems64)]
        public IPerformanceCounter ConnectionMessagesSentTotal { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of messages received by connections (server to client) per second.
        /// </summary>
        [PerformanceCounter(Name = "Connection Messages Received/Sec", Description = "The number of messages received by connections (server to client) per second.", CounterType = PerformanceCounterType.RateOfCountsPerSecond32)]
        public IPerformanceCounter ConnectionMessagesReceivedPerSec { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of messages sent by connections (client to server) per second.
        /// </summary>
        [PerformanceCounter(Name = "Connection Messages Sent/Sec", Description = "The number of messages sent by connections (client to server) per second.", CounterType = PerformanceCounterType.RateOfCountsPerSecond32)]
        public IPerformanceCounter ConnectionMessagesSentPerSec { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the total number of messages received by subscribers since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Message Bus Messages Received Total", Description = "The total number of messages received by subscribers since the application was started.", CounterType = PerformanceCounterType.NumberOfItems64)]
        public IPerformanceCounter MessageBusMessagesReceivedTotal { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of messages received by a subscribers per second.
        /// </summary>
        [PerformanceCounter(Name = "Message Bus Messages Received/Sec", Description = "The number of messages received by subscribers per second.", CounterType = PerformanceCounterType.RateOfCountsPerSecond32)]
        public IPerformanceCounter MessageBusMessagesReceivedPerSec { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of messages received by the scaleout message bus per second.
        /// </summary>
        [PerformanceCounter(Name = "Scaleout Message Bus Messages Received/Sec", Description = "The number of messages received by the scaleout message bus per second.", CounterType = PerformanceCounterType.RateOfCountsPerSecond32)]
        public IPerformanceCounter ScaleoutMessageBusMessagesReceivedPerSec { get; private set; }


        /// <summary>
        /// Gets the performance counter representing the total number of messages published to the message bus since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Messages Bus Messages Published Total", Description = "The total number of messages published to the message bus since the application was started.", CounterType = PerformanceCounterType.NumberOfItems64)]
        public IPerformanceCounter MessageBusMessagesPublishedTotal { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of messages published to the message bus per second.
        /// </summary>
        [PerformanceCounter(Name = "Messages Bus Messages Published/Sec", Description = "The number of messages published to the message bus per second.", CounterType = PerformanceCounterType.RateOfCountsPerSecond32)]
        public IPerformanceCounter MessageBusMessagesPublishedPerSec { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the current number of subscribers to the message bus.
        /// </summary>
        [PerformanceCounter(Name = "Message Bus Subscribers Current", Description = "The current number of subscribers to the message bus.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter MessageBusSubscribersCurrent { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the total number of subscribers to the message bus since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Message Bus Subscribers Total", Description = "The total number of subscribers to the message bus since the application was started.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter MessageBusSubscribersTotal { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of new subscribers to the message bus per second.
        /// </summary>
        [PerformanceCounter(Name = "Message Bus Subscribers/Sec", Description = "The number of new subscribers to the message bus per second.", CounterType = PerformanceCounterType.RateOfCountsPerSecond32)]
        public IPerformanceCounter MessageBusSubscribersPerSec { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of workers allocated to deliver messages in the message bus.
        /// </summary>
        [PerformanceCounter(Name = "Message Bus Allocated Workers", Description = "The number of workers allocated to deliver messages in the message bus.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter MessageBusAllocatedWorkers { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of workers currently busy delivering messages in the message bus.
        /// </summary>
        [PerformanceCounter(Name = "Message Bus Busy Workers", Description = "The number of workers currently busy delivering messages in the message bus.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter MessageBusBusyWorkers { get; private set; }

        /// <summary>
        /// Gets the performance counter representing representing the current number of topics in the message bus.
        /// </summary>
        [PerformanceCounter(Name = "Message Bus Topics Current", Description = "The number of topics in the message bus.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter MessageBusTopicsCurrent { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the total number of all errors processed since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Errors: All Total", Description = "The total number of all errors processed since the application was started.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ErrorsAllTotal { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of all errors processed per second.
        /// </summary>
        [PerformanceCounter(Name = "Errors: All/Sec", Description = "The number of all errors processed per second.", CounterType = PerformanceCounterType.RateOfCountsPerSecond32)]
        public IPerformanceCounter ErrorsAllPerSec { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the total number of hub resolution errors processed since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Errors: Hub Resolution Total", Description = "The total number of hub resolution errors processed since the application was started.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ErrorsHubResolutionTotal { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of hub resolution errors per second.
        /// </summary>
        [PerformanceCounter(Name = "Errors: Hub Resolution/Sec", Description = "The number of hub resolution errors per second.", CounterType = PerformanceCounterType.RateOfCountsPerSecond32)]
        public IPerformanceCounter ErrorsHubResolutionPerSec { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the total number of hub invocation errors processed since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Errors: Hub Invocation Total", Description = "The total number of hub invocation errors processed since the application was started.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ErrorsHubInvocationTotal { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of hub invocation errors per second.
        /// </summary>
        [PerformanceCounter(Name = "Errors: Hub Invocation/Sec", Description = "The number of hub invocation errors per second.", CounterType = PerformanceCounterType.RateOfCountsPerSecond32)]
        public IPerformanceCounter ErrorsHubInvocationPerSec { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the total number of transport errors processed since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Errors: Tranport Total", Description = "The total number of transport errors processed since the application was started.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ErrorsTransportTotal { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of transport errors per second.
        /// </summary>
        [PerformanceCounter(Name = "Errors: Transport/Sec", Description = "The number of transport errors per second.", CounterType = PerformanceCounterType.RateOfCountsPerSecond32)]
        public IPerformanceCounter ErrorsTransportPerSec { get; private set; }


        /// <summary>
        /// Gets the performance counter representing the number of logical streams in the currently configured scaleout message bus provider.
        /// </summary>
        [PerformanceCounter(Name = "Scaleout Streams Total", Description = "The number of logical streams in the currently configured scaleout message bus provider.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ScaleoutStreamCountTotal { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of logical streams in the currently configured scaleout message bus provider that are in the open state.
        /// </summary>
        [PerformanceCounter(Name = "Scaleout Streams Open", Description = "The number of logical streams in the currently configured scaleout message bus provider that are in the open state", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ScaleoutStreamCountOpen { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of logical streams in the currently configured scaleout message bus provider that are in the buffering state.
        /// </summary>
        [PerformanceCounter(Name = "Scaleout Streams Buffering", Description = "The number of logical streams in the currently configured scaleout message bus provider that are in the buffering state", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ScaleoutStreamCountBuffering { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the total number of scaleout errors since the application was started.
        /// </summary>
        [PerformanceCounter(Name = "Scaleout Errors Total", Description = "The total number of scaleout errors since the application was started.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ScaleoutErrorsTotal { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the number of scaleout errors per second.
        /// </summary>
        [PerformanceCounter(Name = "Scaleout Errors/Sec", Description = "The number of scaleout errors per second.", CounterType = PerformanceCounterType.RateOfCountsPerSecond32)]
        public IPerformanceCounter ScaleoutErrorsPerSec { get; private set; }

        /// <summary>
        /// Gets the performance counter representing the current scaleout send queue length.
        /// </summary>
        [PerformanceCounter(Name = "Scaleout Send Queue Length", Description = "The current scaleout send queue length.", CounterType = PerformanceCounterType.NumberOfItems32)]
        public IPerformanceCounter ScaleoutSendQueueLength { get; private set; }

        /// <summary>
        /// Initializes the performance counters.
        /// </summary>
        /// <param name="instanceName">The host instance name.</param>
        /// <param name="hostShutdownToken">The CancellationToken representing the host shutdown.</param>
        public void Initialize(string instanceName, CancellationToken hostShutdownToken)
        {
            if (_initialized)
            {
                return;
            }

            var needToRegisterWithShutdownToken = false;
            lock (_initLocker)
            {
                if (!_initialized)
                {
                    instanceName = instanceName ?? Guid.NewGuid().ToString();
                    SetCounterProperties(instanceName);
                    // The initializer ran, so let's register the shutdown cleanup
                    if (hostShutdownToken != CancellationToken.None)
                    {
                        needToRegisterWithShutdownToken = true;
                    }
                    _initialized = true;
                }
            }

            if (needToRegisterWithShutdownToken)
            {
                hostShutdownToken.Register(UnloadCounters);
            }
        }

        private void UnloadCounters()
        {
            lock (_initLocker)
            {
                if (!_initialized)
                {
                    // We were never initalized
                    return;
                }
            }

            var counterProperties = this.GetType()
                .GetProperties()
                .Where(p => p.PropertyType == typeof(IPerformanceCounter));

            foreach (var property in counterProperties)
            {
                var counter = property.GetValue(this, null) as IPerformanceCounter;
                counter.Close();
                counter.RemoveInstance();
            }
        }

        private void InitNoOpCounters()
        {
            // Set all the counter properties to no-op by default.
            // These will get reset to real counters when/if the Initialize method is called.
            foreach (var property in _counterProperties)
            {
                property.SetValue(this, new NoOpPerformanceCounter(), null);
            }
        }

        private void SetCounterProperties(string instanceName)
        {
            var loadCounters = true;

            foreach (var property in _counterProperties)
            {
                PerformanceCounterAttribute attribute = GetPerformanceCounterAttribute(property);

                if (attribute == null)
                {
                    continue;
                }

                IPerformanceCounter counter = null;

                if (loadCounters)
                {
                    counter = LoadCounter(CategoryName, attribute.Name, instanceName, isReadOnly:false);

                    if (counter == null)
                    {
                        // We failed to load the counter so skip the rest
                        loadCounters = false;
                    }
                }

                counter = counter ?? _noOpCounter;

                property.SetValue(this, counter, null);
            }
        }

        internal static PropertyInfo[] GetCounterPropertyInfo()
        {
            return typeof(PerformanceCounterManager)
                .GetProperties()
                .Where(p => p.PropertyType == typeof(IPerformanceCounter))
                .ToArray();
        }

        internal static PerformanceCounterAttribute GetPerformanceCounterAttribute(PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(PerformanceCounterAttribute), false)
                    .Cast<PerformanceCounterAttribute>()
                    .SingleOrDefault();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This file is shared")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Counters are disposed later")]
        public IPerformanceCounter LoadCounter(string categoryName, string counterName, string instanceName, bool isReadOnly)
        {
            // See http://msdn.microsoft.com/en-us/library/356cx381.aspx for the list of exceptions
            // and when they are thrown. 
            try
            {
                var counter = new PerformanceCounter(categoryName, counterName, instanceName, isReadOnly);

                // Initialize the counter sample
                counter.NextSample();

                return new PerformanceCounterWrapper(counter);
            }
#if UTILS
            catch (InvalidOperationException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
            catch (Win32Exception) { return null; }
            catch (PlatformNotSupportedException) { return null; }
#else
            catch (InvalidOperationException ex)
            {
                _trace.TraceEvent(TraceEventType.Error, 0, "Performance counter failed to load: " + ex.GetBaseException());
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                _trace.TraceEvent(TraceEventType.Error, 0, "Performance counter failed to load: " + ex.GetBaseException());
                return null;
            }
            catch (Win32Exception ex)
            {
                _trace.TraceEvent(TraceEventType.Error, 0, "Performance counter failed to load: " + ex.GetBaseException());
                return null;
            }
            catch (PlatformNotSupportedException ex)
            {
                _trace.TraceEvent(TraceEventType.Error, 0, "Performance counter failed to load: " + ex.GetBaseException());
                return null;
            }
#endif
        }
    }
}
