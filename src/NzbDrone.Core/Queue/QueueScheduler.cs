using System.Threading;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using Timer = System.Timers.Timer;

namespace NzbDrone.Core.Queue
{
    public class QueueScheduler : IHandle<ApplicationStartedEvent>,
                                  IHandle<ApplicationShutdownRequested>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;
        private static readonly Timer Timer = new Timer();
        private static CancellationTokenSource _cancellationTokenSource;

        public QueueScheduler(IEventAggregator eventAggregator, Logger logger)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private void CheckQueue()
        {
            try
            {
                Timer.Enabled = false;
                _eventAggregator.PublishEvent(new UpdateQueueEvent());
            }
            finally
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    Timer.Enabled = true;
                }
            }
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Timer.Interval = 1000 * 30;
            Timer.Elapsed += (o, args) => Task.Factory.StartNew(CheckQueue, _cancellationTokenSource.Token)
                .LogExceptions();

            Timer.Start();
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            _logger.Info("Shutting down queue scheduler");
            _cancellationTokenSource.Cancel(true);
            Timer.Stop();
        }
    }
}
