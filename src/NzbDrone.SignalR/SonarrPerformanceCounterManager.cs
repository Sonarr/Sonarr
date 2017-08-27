using System.Threading;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace NzbDrone.SignalR
{
    public class SonarrPerformanceCounterManager : IPerformanceCounterManager
    {
        private readonly IPerformanceCounter _counter = new NoOpPerformanceCounter();

        public void Initialize(string instanceName, CancellationToken hostShutdownToken)
        {
            
        }

        public IPerformanceCounter LoadCounter(string categoryName, string counterName, string instanceName, bool isReadOnly)
        {
            return _counter;
        }

        public IPerformanceCounter ConnectionsConnected => _counter;
        public IPerformanceCounter ConnectionsReconnected => _counter;
        public IPerformanceCounter ConnectionsDisconnected => _counter;
        public IPerformanceCounter ConnectionsCurrent => _counter;
        public IPerformanceCounter ConnectionMessagesReceivedTotal => _counter;
        public IPerformanceCounter ConnectionMessagesSentTotal => _counter;
        public IPerformanceCounter ConnectionMessagesReceivedPerSec => _counter;
        public IPerformanceCounter ConnectionMessagesSentPerSec => _counter;
        public IPerformanceCounter MessageBusMessagesReceivedTotal => _counter;
        public IPerformanceCounter MessageBusMessagesReceivedPerSec => _counter;
        public IPerformanceCounter ScaleoutMessageBusMessagesReceivedPerSec => _counter;
        public IPerformanceCounter MessageBusMessagesPublishedTotal => _counter;
        public IPerformanceCounter MessageBusMessagesPublishedPerSec => _counter;
        public IPerformanceCounter MessageBusSubscribersCurrent => _counter;
        public IPerformanceCounter MessageBusSubscribersTotal => _counter;
        public IPerformanceCounter MessageBusSubscribersPerSec => _counter;
        public IPerformanceCounter MessageBusAllocatedWorkers => _counter;
        public IPerformanceCounter MessageBusBusyWorkers => _counter;
        public IPerformanceCounter MessageBusTopicsCurrent => _counter;
        public IPerformanceCounter ErrorsAllTotal => _counter;
        public IPerformanceCounter ErrorsAllPerSec => _counter;
        public IPerformanceCounter ErrorsHubResolutionTotal => _counter;
        public IPerformanceCounter ErrorsHubResolutionPerSec => _counter;
        public IPerformanceCounter ErrorsHubInvocationTotal => _counter;
        public IPerformanceCounter ErrorsHubInvocationPerSec => _counter;
        public IPerformanceCounter ErrorsTransportTotal => _counter;
        public IPerformanceCounter ErrorsTransportPerSec => _counter;
        public IPerformanceCounter ScaleoutStreamCountTotal => _counter;
        public IPerformanceCounter ScaleoutStreamCountOpen => _counter;
        public IPerformanceCounter ScaleoutStreamCountBuffering => _counter;
        public IPerformanceCounter ScaleoutErrorsTotal => _counter;
        public IPerformanceCounter ScaleoutErrorsPerSec => _counter;
        public IPerformanceCounter ScaleoutSendQueueLength => _counter;
        public IPerformanceCounter ConnectionsCurrentForeverFrame => _counter;
        public IPerformanceCounter ConnectionsCurrentLongPolling => _counter;
        public IPerformanceCounter ConnectionsCurrentServerSentEvents => _counter;
        public IPerformanceCounter ConnectionsCurrentWebSockets => _counter;
    }
}