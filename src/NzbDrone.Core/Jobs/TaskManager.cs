using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Backup;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Download;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.Housekeeping;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Update.Commands;

namespace NzbDrone.Core.Jobs
{
    public interface ITaskManager
    {
        IList<ScheduledTask> GetPending();
        List<ScheduledTask> GetAll();
        DateTime GetNextExecution(Type type);
    }

    public class TaskManager : ITaskManager, IHandle<ApplicationStartedEvent>, IHandle<CommandExecutedEvent>, IHandleAsync<ConfigSavedEvent>
    {
        private readonly IScheduledTaskRepository _scheduledTaskRepository;
        private readonly IConfigService _configService;
        private readonly Logger _logger;
        private readonly ICached<ScheduledTask> _cache;

        public TaskManager(IScheduledTaskRepository scheduledTaskRepository, IConfigService configService, ICacheManager cacheManager, Logger logger)
        {
            _scheduledTaskRepository = scheduledTaskRepository;
            _configService = configService;
            _cache = cacheManager.GetCache<ScheduledTask>(GetType());
            _logger = logger;
        }

        public IList<ScheduledTask> GetPending()
        {
            return _cache.Values
                         .Where(c => c.Interval > 0 && c.LastExecution.AddMinutes(c.Interval) < DateTime.UtcNow)
                         .ToList();
        }

        public List<ScheduledTask> GetAll()
        {
            return _cache.Values.ToList();
        }

        public DateTime GetNextExecution(Type type)
        {
            var scheduledTask = _cache.Find(type.FullName);

            return scheduledTask.LastExecution.AddMinutes(scheduledTask.Interval);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            var defaultTasks = new List<ScheduledTask>
                {
                    new ScheduledTask
                    {
                        Interval = 1,
                        TypeName = typeof(RefreshMonitoredDownloadsCommand).FullName,
                        Priority = CommandPriority.High
                    },

                    new ScheduledTask
                    {
                        Interval = 5,
                        TypeName = typeof(MessagingCleanupCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 6 * 60,
                        TypeName = typeof(ApplicationUpdateCheckCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 3 * 60,
                        TypeName = typeof(UpdateSceneMappingCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 6 * 60,
                        TypeName = typeof(CheckHealthCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 12 * 60,
                        TypeName = typeof(RefreshSeriesCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 24 * 60,
                        TypeName = typeof(HousekeepingCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 24 * 60,
                        TypeName = typeof(CleanUpRecycleBinCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 24 * 60,
                        TypeName = typeof(ImportListSyncCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = GetBackupInterval(),
                        TypeName = typeof(BackupCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = GetRssSyncInterval(),
                        TypeName = typeof(RssSyncCommand).FullName
                    }
                };

            var currentTasks = _scheduledTaskRepository.All().ToList();

            _logger.Trace("Initializing jobs. Available: {0} Existing: {1}", defaultTasks.Count(), currentTasks.Count());

            foreach (var job in currentTasks)
            {
                if (!defaultTasks.Any(c => c.TypeName == job.TypeName))
                {
                    _logger.Trace("Removing job from database '{0}'", job.TypeName);
                    _scheduledTaskRepository.Delete(job.Id);
                }
            }

            foreach (var defaultTask in defaultTasks)
            {
                var currentDefinition = currentTasks.SingleOrDefault(c => c.TypeName == defaultTask.TypeName) ?? defaultTask;

                currentDefinition.Interval = defaultTask.Interval;

                if (currentDefinition.Id == 0)
                {
                    currentDefinition.LastExecution = DateTime.UtcNow;
                }

                currentDefinition.Priority = defaultTask.Priority;

                _cache.Set(currentDefinition.TypeName, currentDefinition);
                _scheduledTaskRepository.Upsert(currentDefinition);
            }
        }

        private int GetBackupInterval()
        {
            var intervalMinutes = _configService.BackupInterval;

            if (intervalMinutes < 1)
            {
                intervalMinutes = 1;
            }

            if (intervalMinutes > 7)
            {
                intervalMinutes = 7;
            }

            return intervalMinutes * 60 * 24;
        }

        private int GetRssSyncInterval()
        {
            var interval = _configService.RssSyncInterval;

            if (interval > 0 && interval < 10)
            {
                return 10;
            }

            if (interval < 0)
            {
                return 0;
            }

            return interval;
        }

        public void Handle(CommandExecutedEvent message)
        {
            var scheduledTask = _scheduledTaskRepository.All().SingleOrDefault(c => c.TypeName == message.Command.Body.GetType().FullName);

            if (scheduledTask != null && message.Command.Body.UpdateScheduledTask)
            {
                _logger.Trace("Updating last run time for: {0}", scheduledTask.TypeName);

                var lastExecution = DateTime.UtcNow;

                _scheduledTaskRepository.SetLastExecutionTime(scheduledTask.Id, lastExecution);
                _cache.Find(scheduledTask.TypeName).LastExecution = lastExecution;
            }
        }

        public void HandleAsync(ConfigSavedEvent message)
        {
            var rss = _scheduledTaskRepository.GetDefinition(typeof(RssSyncCommand));
            rss.Interval = GetRssSyncInterval();

            var backup = _scheduledTaskRepository.GetDefinition(typeof(BackupCommand));
            backup.Interval = GetBackupInterval();

            _scheduledTaskRepository.UpdateMany(new List<ScheduledTask> { rss, backup });

            _cache.Find(rss.TypeName).Interval = rss.Interval;
            _cache.Find(backup.TypeName).Interval = backup.Interval;
        }
    }
}
