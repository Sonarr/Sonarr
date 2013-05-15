using System;
using System.Timers;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Core.Jobs
{
    [Singleton]
    public class Scheduler :
        IHandle<ApplicationStartedEvent>,
        IHandle<ApplicationShutdownRequested>
    {
        private readonly ITaskManager _taskManager;
        private readonly IMessageAggregator _messageAggregator;
        private readonly Logger _logger;
        private static readonly Timer Timer = new Timer();

        public Scheduler(ITaskManager taskManager, IMessageAggregator messageAggregator, Logger logger)
        {
            _taskManager = taskManager;
            _messageAggregator = messageAggregator;
            _logger = logger;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            Timer.Interval = 1000 * 30;
            Timer.Elapsed += (o, args) => ExecuteCommands();
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
                    try
                    {
                        var commandType = Type.GetType(task.TypeName);
                        var command = (ICommand)Activator.CreateInstance(commandType);

                        _messageAggregator.PublishCommand(command);
                    }
                    catch (Exception e)
                    {
                        _logger.ErrorException("Error occurred while execution task " + task.TypeName, e);
                    }
                }
            }
            finally
            {
                Timer.Enabled = true;
            }
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            Timer.Stop();
        }
    }
}