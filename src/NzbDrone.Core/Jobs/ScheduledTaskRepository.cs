using System;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Jobs
{
    public interface IScheduledTaskRepository : IBasicRepository<ScheduledTask>
    {
        ScheduledTask GetDefinition(Type type);
        void SetLastExecutionTime(int id, DateTime executionTime);
    }

    public class ScheduledTaskRepository : BasicRepository<ScheduledTask>, IScheduledTaskRepository
    {
        public ScheduledTaskRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public ScheduledTask GetDefinition(Type type)
        {
            return Query.Where(c => c.TypeName == type.FullName).Single();
        }

        public void SetLastExecutionTime(int id, DateTime executionTime)
        {
            var task = new ScheduledTask
                {
                    Id = id,
                    LastExecution = executionTime
                };

            SetFields(task, scheduledTask => scheduledTask.LastExecution);
        }
    }
}
