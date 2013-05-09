using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Jobs
{
    public interface IScheduledTaskRepository : IBasicRepository<ScheduledTask>
    {
        IList<ScheduledTask> GetPendingJobs();
        ScheduledTask GetDefinition(Type type);
    }


    public class ScheduledTaskRepository : BasicRepository<ScheduledTask>, IScheduledTaskRepository
    {

        public ScheduledTaskRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
        }

        public ScheduledTask GetDefinition(Type type)
        {
            return Query.Single(c => c.Name == type.FullName);
        }


        public IList<ScheduledTask> GetPendingJobs()
        {
            return Query.Where(c => c.Interval != 0).ToList().Where(c => c.LastExecution < DateTime.Now.AddMinutes(-c.Interval)).ToList();
        }
    }
}
