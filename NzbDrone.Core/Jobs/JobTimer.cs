using System;
using System.Timers;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Core.Jobs
{
    public class JobTimer :
        IHandle<ApplicationStartedEvent>,
        IHandle<ApplicationShutdownRequested>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IMessageAggregator _messageAggregator;
        private readonly Timer _timer;

        public JobTimer(IJobRepository jobRepository, IMessageAggregator messageAggregator)
        {
            _jobRepository = jobRepository;
            _messageAggregator = messageAggregator;
            _timer = new Timer();
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _timer.Interval = 1000 * 30;
            _timer.Elapsed += (o, args) => ExecuteCommands();
            _timer.Start();
        }

        private void ExecuteCommands()
        {
            var jobs = _jobRepository.GetPendingJobs();

            foreach (var jobDefinition in jobs)
            {
                var commandType = Type.GetType(jobDefinition.Name);
                var command = (ICommand)Activator.CreateInstance(commandType);

                _messageAggregator.PublishCommand(command);
            }
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            _timer.Stop();
        }
    }
}