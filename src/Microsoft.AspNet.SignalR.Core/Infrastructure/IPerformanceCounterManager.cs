// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Diagnostics;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    /// <summary>
    /// Provides access to performance counters.
    /// </summary>
    public interface IPerformanceCounterManager
    {
        /// <summary>
        /// Initializes the performance counters.
        /// </summary>
        /// <param name="instanceName">The host instance name.</param>
        /// <param name="hostShutdownToken">The CancellationToken representing the host shutdown.</param>
        void Initialize(string instanceName, CancellationToken hostShutdownToken);

        /// <summary>
        /// Loads a performance counter.
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <param name="counterName">The counter name.</param>
        /// <param name="instanceName">The instance name.</param>
        /// <param name="isReadOnly">Whether the counter is read-only.</param>
        IPerformanceCounter LoadCounter(string categoryName, string counterName, string instanceName, bool isReadOnly);

        /// <summary>
        /// Gets the performance counter representing the total number of connection Connect events since the application was started.
        /// </summary>
        IPerformanceCounter ConnectionsConnected { get; }

        /// <summary>
        /// Gets the performance counter representing the total number of connection Reconnect events since the application was started.
        /// </summary>
        IPerformanceCounter ConnectionsReconnected { get; }

        /// <summary>
        /// Gets the performance counter representing the total number of connection Disconnect events since the application was started.
        /// </summary>
        IPerformanceCounter ConnectionsDisconnected { get; }

        /// <summary>
        /// Gets the performance counter representing the number of connections currently connected.
        /// </summary>
        IPerformanceCounter ConnectionsCurrent { get; }

        /// <summary>
        /// Gets the performance counter representing the total number of messages received by connections (server to client) since the application was started.
        /// </summary>
        IPerformanceCounter ConnectionMessagesReceivedTotal { get; }

        /// <summary>
        /// Gets the performance counter representing the total number of messages received by connections (server to client) since the application was started.
        /// </summary>
        IPerformanceCounter ConnectionMessagesSentTotal { get; }

        /// <summary>
        /// Gets the performance counter representing the number of messages received by connections (server to client) per second.
        /// </summary>
        IPerformanceCounter ConnectionMessagesReceivedPerSec { get; }

        /// <summary>
        /// Gets the performance counter representing the number of messages sent by connections (client to server) per second.
        /// </summary>
        IPerformanceCounter ConnectionMessagesSentPerSec { get; }

        /// <summary>
        /// Gets the performance counter representing the total number of messages received by subscribers since the application was started.
        /// </summary>
        IPerformanceCounter MessageBusMessagesReceivedTotal { get; }

        /// <summary>
        /// Gets the performance counter representing the number of messages received by a subscribers per second.
        /// </summary>
        IPerformanceCounter MessageBusMessagesReceivedPerSec { get; }

        /// <summary>
        /// Gets the performance counter representing the number of messages received by the scaleout message bus per second.
        /// </summary>
        IPerformanceCounter ScaleoutMessageBusMessagesReceivedPerSec { get; }

        /// <summary>
        /// Gets the performance counter representing the total number of messages published to the message bus since the application was started.
        /// </summary>
        IPerformanceCounter MessageBusMessagesPublishedTotal { get; }

        /// <summary>
        /// Gets the performance counter representing the number of messages published to the message bus per second.
        /// </summary>
        IPerformanceCounter MessageBusMessagesPublishedPerSec { get; }

        /// <summary>
        /// Gets the performance counter representing the current number of subscribers to the message bus.
        /// </summary>
        IPerformanceCounter MessageBusSubscribersCurrent { get; }

        /// <summary>
        /// Gets the performance counter representing the total number of subscribers to the message bus since the application was started.
        /// </summary>
        IPerformanceCounter MessageBusSubscribersTotal { get; }

        /// <summary>
        /// Gets the performance counter representing the number of new subscribers to the message bus per second.
        /// </summary>
        IPerformanceCounter MessageBusSubscribersPerSec { get; }

        /// <summary>
        /// Gets the performance counter representing the number of workers allocated to deliver messages in the message bus.
        /// </summary>
        IPerformanceCounter MessageBusAllocatedWorkers { get; }

        /// <summary>
        /// Gets the performance counter representing the number of workers currently busy delivering messages in the message bus.
        /// </summary>
        IPerformanceCounter MessageBusBusyWorkers { get; }

        /// <summary>
        /// Gets the performance counter representing representing the current number of topics in the message bus.
        /// </summary>
        IPerformanceCounter MessageBusTopicsCurrent { get; }

        /// <summary>
        /// Gets the performance counter representing the total number of all errors processed since the application was started.
        /// </summary>
        IPerformanceCounter ErrorsAllTotal { get; }

        /// <summary>
        /// Gets the performance counter representing the number of all errors processed per second.
        /// </summary>
        IPerformanceCounter ErrorsAllPerSec { get; }

        /// <summary>
        /// Gets the performance counter representing the total number of hub resolution errors processed since the application was started.
        /// </summary>
        IPerformanceCounter ErrorsHubResolutionTotal { get; }

        /// <summary>
        /// Gets the performance counter representing the number of hub resolution errors per second.
        /// </summary>
        IPerformanceCounter ErrorsHubResolutionPerSec { get; }

        /// <summary>
        /// Gets the performance counter representing the total number of hub invocation errors processed since the application was started.
        /// </summary>
        IPerformanceCounter ErrorsHubInvocationTotal { get; }

        /// <summary>
        /// Gets the performance counter representing the number of hub invocation errors per second.
        /// </summary>
        IPerformanceCounter ErrorsHubInvocationPerSec { get; }

        /// <summary>
        /// Gets the performance counter representing the total number of transport errors processed since the application was started.
        /// </summary>
        IPerformanceCounter ErrorsTransportTotal { get; }

        /// <summary>
        /// Gets the performance counter representing the number of transport errors per second.
        /// </summary>
        IPerformanceCounter ErrorsTransportPerSec { get; }

        /// <summary>
        /// Gets the performance counter representing the number of logical streams in the currently configured scaleout message bus provider.
        /// </summary>
        IPerformanceCounter ScaleoutStreamCountTotal { get; }

        /// <summary>
        /// Gets the performance counter representing the number of logical streams in the currently configured scaleout message bus provider that are in the open state.
        /// </summary>
        IPerformanceCounter ScaleoutStreamCountOpen { get; }

        /// <summary>
        /// Gets the performance counter representing the number of logical streams in the currently configured scaleout message bus provider that are in the buffering state.
        /// </summary>
        IPerformanceCounter ScaleoutStreamCountBuffering { get; }

        /// <summary>
        /// Gets the performance counter representing the total number of scaleout errors since the application was started.
        /// </summary>
        IPerformanceCounter ScaleoutErrorsTotal { get; }

        /// <summary>
        /// Gets the performance counter representing the number of scaleout errors per second.
        /// </summary>
        IPerformanceCounter ScaleoutErrorsPerSec { get; }

        /// <summary>
        /// Gets the performance counter representing the current scaleout send queue length.
        /// </summary>
        IPerformanceCounter ScaleoutSendQueueLength { get; }
    }
}
