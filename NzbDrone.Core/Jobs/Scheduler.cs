using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Lifecycle;
using Timer = System.Timers.Timer;
using NzbDrone.Common.TPL;

namespace NzbDrone.Core.Jobs
{
    public class Scheduler :
        IHandle<ApplicationStartedEvent>,
        IHandle<ApplicationShutdownRequested>
    {
        private readonly ITaskManager _taskManager;
        private readonly IMessageAggregator _messageAggregator;
        private readonly Logger _logger;
        private static readonly Timer Timer = new Timer();
        private static CancellationTokenSource _cancellationTokenSource;

        public Scheduler(ITaskManager taskManager, IMessageAggregator messageAggregator, Logger logger)
        {
            _taskManager = taskManager;
            _messageAggregator = messageAggregator;
            _logger = logger;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Timer.Interval = 1000 * 30;
            Timer.Elapsed += (o, args) => Task.Factory.StartNew(ExecuteCommands, _cancellationTokenSource.Token)
                .LogExceptions();
            
            Timer.Start();
        }

        private void ExecuteCommands()
        {
            try
            {
                Timer.Enabled = false;

                var tasks = _taskManager.GetPending();

                _logger.Trace("Pending Tasks: {0}", tasks.Count);

                foreach (var task in tasks)
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    try
                    {
                        _messageAggregator.PublishCommand(task.TypeName);
                    }
                    catch (Exception e)
                    {
                        _logger.ErrorException("Error occurred while execution task " + task.TypeName, e);
                    }
                }
            }
            finally
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    Timer.Enabled = true;
                }
            }
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            _cancellationTokenSource.Cancel(true);
            Timer.Stop();
        }
    }
}