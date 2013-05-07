using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public interface IJobRepository : IBasicRepository<JobDefinition>
    {
        IList<JobDefinition> GetPendingJobs();
        JobDefinition GetDefinition(Type type);
    }

    public class JobRepository : BasicRepository<JobDefinition>, IJobRepository, IHandle<ApplicationStartedEvent>
    {
        private readonly Logger _logger;

        public JobRepository(IDatabase database, Logger logger, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
            _logger = logger;
        }

        public JobDefinition GetDefinition(Type type)
        {
            return Query.Single(c => c.Name == type.FullName);
        }


        public IList<JobDefinition> GetPendingJobs()
        {
            return Query.Where(c => c.Interval != 0).ToList().Where(c => c.LastExecution < DateTime.Now.AddMinutes(-c.Interval)).ToList();
        }

        public void Handle(ApplicationStartedEvent message)
        {
           /* var currentJobs = All().ToList();


            var timers = new[]
                {
                    new JobDefinition{ Interval = 25, Name = typeof(RssSyncCommand).FullName},
                    new JobDefinition{ Interval = 24*60, Name = typeof(UpdateXemMappings).FullName}
                };


            _logger.Debug("Initializing jobs. Available: {0} Existing:{1}", timers.Count(), currentJobs.Count());

            foreach (var job in currentJobs)
            {
                if (!timers.Any(c => c.Name == job.Name))
                {
                    _logger.Debug("Removing job from database '{0}'", job.Name);
                    Delete(job.Id);
                }
            }

            foreach (var job in timers)
            {
                var currentDefinition = currentJobs.SingleOrDefault(c => c.Name == job.GetType().ToString());

                if (currentDefinition == null)
                {
                    currentDefinition = job;
                }

                currentDefinition.Interval = job.Interval;

                Upsert(currentDefinition);
            }*/
        }
    }
}
