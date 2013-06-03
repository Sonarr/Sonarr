using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Update.Commands;

namespace NzbDrone.Core.Jobs
{
    public interface ITaskManager
    {
        IList<ScheduledTask> GetPending();
    }

    public class TaskManager : IHandle<ApplicationStartedEvent>, IHandleAsync<CommandExecutedEvent>, ITaskManager
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
            return _scheduledTaskRepository.All().Where(c => c.LastExecution.AddMinutes(c.Interval) < DateTime.UtcNow).ToList();
        }

        public void Handle(ApplicationStartedEvent message)
        {
            var defaultTasks = new[]
                {
                    new ScheduledTask{ Interval = 25, TypeName = typeof(RssSyncCommand).FullName},
                    new ScheduledTask{ Interval = 12*60, TypeName = typeof(UpdateXemMappings).FullName},
                    new ScheduledTask{ Interval = 6*60, TypeName = typeof(RefreshSeriesCommand).FullName},
                    new ScheduledTask{ Interval = 1, TypeName = typeof(DownloadedEpisodesScanCommand).FullName},
                    new ScheduledTask{ Interval = 5, TypeName = typeof(ApplicationUpdateCommand).FullName}
                };

            var currentTasks = _scheduledTaskRepository.All();

            _logger.Debug("Initializing jobs. Available: {0} Existing:{1}", defaultTasks.Count(), currentTasks.Count());


            foreach (var job in currentTasks)
            {
                if (!defaultTasks.Any(c => c.TypeName == job.TypeName))
                {
                    _logger.Debug("Removing job from database '{0}'", job.TypeName);
                    _scheduledTaskRepository.Delete(job.Id);
                }
            }

            foreach (var defaultTask in defaultTasks)
            {
                var currentDefinition = currentTasks.SingleOrDefault(c => c.TypeName == defaultTask.TypeName) ?? defaultTask;

                currentDefinition.Interval = defaultTask.Interval;

                _scheduledTaskRepository.Upsert(currentDefinition);
            }
        }

        public void HandleAsync(CommandExecutedEvent message)
        {
            var scheduledTask = _scheduledTaskRepository.All().SingleOrDefault(c => c.TypeName == message.Command.GetType().FullName);

            if (scheduledTask != null)
            {
                _scheduledTaskRepository.SetLastExecutionTime(scheduledTask.Id, DateTime.UtcNow);
            }
        }
    }
}