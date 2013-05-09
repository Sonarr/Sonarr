using System;
using System.Timers;
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
        private static readonly Timer Timer = new Timer();

        public Scheduler(ITaskManager taskManager, IMessageAggregator messageAggregator)
        {
            _taskManager = taskManager;
            _messageAggregator = messageAggregator;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            Timer.Interval = 1000 * 30;
            Timer.Elapsed += (o, args) => ExecuteCommands();
            Timer.Start();
        }

        private void ExecuteCommands()
        {
            var tasks = _taskManager.GetPending();

            foreach (var task in tasks)
            {
                var commandType = Type.GetType(task.Name);
                var command = (ICommand)Activator.CreateInstance(commandType);

                _messageAggregator.PublishCommand(command);
            }
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            Timer.Stop();
        }
    }


}