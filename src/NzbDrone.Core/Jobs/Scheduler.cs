using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using Timer = System.Timers.Timer;

namespace NzbDrone.Core.Jobs
{
    public class Scheduler :
        IHandle<ApplicationStartedEvent>,
        IHandle<ApplicationShutdownRequested>
    {
        private readonly ITaskManager _taskManager;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;
        private readonly Timer _timer = new Timer();
        private CancellationTokenSource _cancellationTokenSource;

        public Scheduler(ITaskManager taskManager, IManageCommandQueue commandQueueManager, Logger logger)
        {
            _taskManager = taskManager;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        private void ExecuteCommands()
        {
            try
            {
                _timer.Enabled = false;

                var tasks = _taskManager.GetPending().ToList();

                _logger.Trace("Pending Tasks: {0}", tasks.Count);

                foreach (var task in tasks)
                {
                    _commandQueueManager.Push(task.TypeName, task.LastExecution, task.LastStartTime, task.Priority, CommandTrigger.Scheduled);
                }
            }
            finally
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    _timer.Enabled = true;
                }
            }
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _timer.Interval = 1000 * 30;
            _timer.Elapsed += (o, args) => Task.Factory.StartNew(ExecuteCommands, _cancellationTokenSource.Token)
                .LogExceptions();

            _timer.Start();
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            _logger.Info("Shutting down scheduler");
            _cancellationTokenSource.Cancel(true);
            _timer.Stop();
        }
    }
}
