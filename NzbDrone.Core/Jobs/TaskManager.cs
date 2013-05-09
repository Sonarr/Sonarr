using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public interface ITaskManager
    {
        IList<ScheduledTask> GetPending();
    }

    public class TaskManager : IHandle<ApplicationStartedEvent>, ITaskManager
    {
        private readonly IScheduledTaskRepository _scheduledTaskRepository;
        private readonly Logger _logger;

        public TaskManager(IScheduledTaskRepository scheduledTaskRepository, Logger logger)
        {
            _scheduledTaskRepository = scheduledTaskRepository;
            _logger = logger;
        }


        public IList<ScheduledTask> GetPending()
        {
            return _scheduledTaskRepository.GetPendingJobs();
        }

        public void Handle(ApplicationStartedEvent message)
        {
            var defaultTasks = new[]
                {
                    new ScheduledTask{ Interval = 25, Name = typeof(RssSyncCommand).FullName},
                    new ScheduledTask{ Interval = 24*60, Name = typeof(UpdateXemMappings).FullName}
                };

            var currentTasks = _scheduledTaskRepository.All();


            _logger.Debug("Initializing jobs. Available: {0} Existing:{1}", defaultTasks.Count(), currentTasks.Count());


            foreach (var job in currentTasks)
            {
                if (!defaultTasks.Any(c => c.Name == job.Name))
                {
                    _logger.Debug("Removing job from database '{0}'", job.Name);
                    _scheduledTaskRepository.Delete(job.Id);
                }
            }

            foreach (var defaultTask in defaultTasks)
            {
                var currentDefinition = currentTasks.SingleOrDefault(c => c.Name == defaultTask.Name);

                if (currentDefinition == null)
                {
                    currentDefinition = defaultTask;
                    _scheduledTaskRepository.Upsert(currentDefinition);
                }

            }
        }
    }
}