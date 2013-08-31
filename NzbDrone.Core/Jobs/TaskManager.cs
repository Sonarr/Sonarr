using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Messaging.Events;
using NzbDrone.Common.Messaging.Tracking;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Instrumentation.Commands;
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

    public class TaskManager : ITaskManager, IHandle<ApplicationStartedEvent>, IHandleAsync<CommandExecutedEvent>, IHandleAsync<ConfigSavedEvent>
    {
        private readonly IScheduledTaskRepository _scheduledTaskRepository;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public TaskManager(IScheduledTaskRepository scheduledTaskRepository, IConfigService configService, Logger logger)
        {
            _scheduledTaskRepository = scheduledTaskRepository;
            _configService = configService;
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
                    new ScheduledTask{ Interval = _configService.RssSyncInterval, TypeName = typeof(RssSyncCommand).FullName},
                    new ScheduledTask{ Interval = 12*60, TypeName = typeof(UpdateXemMappingsCommand).FullName},
                    new ScheduledTask{ Interval = 12*60, TypeName = typeof(RefreshSeriesCommand).FullName},
                    new ScheduledTask{ Interval = 1, TypeName = typeof(DownloadedEpisodesScanCommand).FullName},
                    new ScheduledTask{ Interval = 60, TypeName = typeof(ApplicationUpdateCommand).FullName},
                    new ScheduledTask{ Interval = 1*60, TypeName = typeof(TrimLogCommand).FullName},
                    new ScheduledTask{ Interval = 1, TypeName = typeof(TrackedCommandCleanupCommand).FullName}
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

        public void HandleAsync(ConfigSavedEvent message)
        {
            var rss = _scheduledTaskRepository.GetDefinition(typeof (RssSyncCommand));
            rss.Interval = _configService.RssSyncInterval;
            _scheduledTaskRepository.Update(rss);
        }
    }
}