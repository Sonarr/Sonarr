using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Core.Jobs
{
    public interface IJobRepository : IInitializable, IBasicRepository<JobDefinition>
    {
        IList<JobDefinition> GetPendingJobs();
        JobDefinition GetDefinition(Type type);
    }

    public class JobRepository : BasicRepository<JobDefinition>, IJobRepository
    {
        private readonly IEnumerable<IJob> _jobs;
        private readonly Logger _logger;

        public JobRepository(IObjectDatabase objectDatabase, IEnumerable<IJob> jobs, Logger logger)
            : base(objectDatabase)
        {
            _jobs = jobs;
            _logger = logger;
        }

        public JobDefinition GetDefinition(Type type)
        {
            return Queryable.Single(c => c.TypeName == type.FullName);
        }


        public IList<JobDefinition> GetPendingJobs()
        {
            return Queryable.Where(c => c.Enable && c.LastExecution < DateTime.UtcNow.AddMinutes(c.Interval)).ToList();
        }

        public void Init()
        {
            var currentJobs = All();
            _logger.Debug("Initializing jobs. Available: {0} Existing:{1}", _jobs.Count(), currentJobs.Count());

            foreach (var currentJob in currentJobs)
            {
                if (_jobs.All(c => c.GetType().ToString() != currentJob.TypeName))
                {
                    _logger.Debug("Removing job from database '{0}'", currentJob.Name);
                    Delete(currentJob.OID);
                }
            }

            foreach (var job in _jobs)
            {
                var jobDefinition = currentJobs.SingleOrDefault(c => c.TypeName == job.GetType().ToString());

                if (jobDefinition == null)
                {
                    jobDefinition = new JobDefinition
                        {
                            TypeName = job.GetType().ToString(),
                            LastExecution = DateTime.Now
                        };
                }

                jobDefinition.Enable = job.DefaultInterval.TotalSeconds > 0;
                jobDefinition.Name = job.Name;

                jobDefinition.Interval = Convert.ToInt32(job.DefaultInterval.TotalMinutes);

                Upsert(jobDefinition);
            }
        }
    }
}