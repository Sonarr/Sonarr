using System.Diagnostics;
using System.Threading;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace NzbDrone.SignalR
{
    public class NoOpPerformanceCounterManager : IPerformanceCounterManager
    {
        private static readonly IPerformanceCounter noOpCounter = new NoOpPerformanceCounter();

        public void Initialize(string instanceName, CancellationToken hostShutdownToken)
        {

        }

        public IPerformanceCounter LoadCounter(string categoryName, string counterName, string instanceName, bool isReadOnly)
        {
            return noOpCounter;
        }

        public IPerformanceCounter ConnectionsConnected { get { return noOpCounter; } }
        public IPerformanceCounter ConnectionsReconnected { get { return noOpCounter; } }
        public IPerformanceCounter ConnectionsDisconnected { get { return noOpCounter; } }
        public IPerformanceCounter ConnectionsCurrent { get { return noOpCounter; } }
        public IPerformanceCounter ConnectionMessagesReceivedTotal { get { return noOpCounter; } }
        public IPerformanceCounter ConnectionMessagesSentTotal { get { return noOpCounter; } }
        public IPerformanceCounter ConnectionMessagesReceivedPerSec { get { return noOpCounter; } }
        public IPerformanceCounter ConnectionMessagesSentPerSec { get { return noOpCounter; } }
        public IPerformanceCounter MessageBusMessagesReceivedTotal { get { return noOpCounter; } }
        public IPerformanceCounter MessageBusMessagesReceivedPerSec { get { return noOpCounter; } }
        public IPerformanceCounter ScaleoutMessageBusMessagesReceivedPerSec { get { return noOpCounter; } }
        public IPerformanceCounter MessageBusMessagesPublishedTotal { get { return noOpCounter; } }
        public IPerformanceCounter MessageBusMessagesPublishedPerSec { get { return noOpCounter; } }
        public IPerformanceCounter MessageBusSubscribersCurrent { get { return noOpCounter; } }
        public IPerformanceCounter MessageBusSubscribersTotal { get { return noOpCounter; } }
        public IPerformanceCounter MessageBusSubscribersPerSec { get { return noOpCounter; } }
        public IPerformanceCounter MessageBusAllocatedWorkers { get { return noOpCounter; } }
        public IPerformanceCounter MessageBusBusyWorkers { get { return noOpCounter; } }
        public IPerformanceCounter MessageBusTopicsCurrent { get { return noOpCounter; } }
        public IPerformanceCounter ErrorsAllTotal { get { return noOpCounter; } }
        public IPerformanceCounter ErrorsAllPerSec { get { return noOpCounter; } }
        public IPerformanceCounter ErrorsHubResolutionTotal { get { return noOpCounter; } }
        public IPerformanceCounter ErrorsHubResolutionPerSec { get { return noOpCounter; } }
        public IPerformanceCounter ErrorsHubInvocationTotal { get { return noOpCounter; } }
        public IPerformanceCounter ErrorsHubInvocationPerSec { get { return noOpCounter; } }
        public IPerformanceCounter ErrorsTransportTotal { get { return noOpCounter; } }
        public IPerformanceCounter ErrorsTransportPerSec { get { return noOpCounter; } }
        public IPerformanceCounter ScaleoutStreamCountTotal { get { return noOpCounter; } }
        public IPerformanceCounter ScaleoutStreamCountOpen { get { return noOpCounter; } }
        public IPerformanceCounter ScaleoutStreamCountBuffering { get { return noOpCounter; } }
        public IPerformanceCounter ScaleoutErrorsTotal { get { return noOpCounter; } }
        public IPerformanceCounter ScaleoutErrorsPerSec { get { return noOpCounter; } }
        public IPerformanceCounter ScaleoutSendQueueLength { get { return noOpCounter; } }
    }

    public class NoOpPerformanceCounter : IPerformanceCounter
    {
        public string CounterName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public long RawValue
        {
            get
            {
                return 0L;
            }
            set
            {
            }
        }

        public long Decrement()
        {
            return 0L;
        }

        public long Increment()
        {
            return 0L;
        }

        public long IncrementBy(long value)
        {
            return 0L;
        }

        public void Close()
        {
        }

        public void RemoveInstance()
        {
        }

        public CounterSample NextSample()
        {
            return CounterSample.Empty;
        }
    }
}