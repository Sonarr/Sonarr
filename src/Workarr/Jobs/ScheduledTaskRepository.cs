using Workarr.Datastore;
using Workarr.Messaging.Events;

namespace Workarr.Jobs
{
    public interface IScheduledTaskRepository : IBasicRepository<ScheduledTask>
    {
        ScheduledTask GetDefinition(Type type);
        void SetLastExecutionTime(int id, DateTime executionTime, DateTime startTime);
    }

    public class ScheduledTaskRepository : BasicRepository<ScheduledTask>, IScheduledTaskRepository
    {
        public ScheduledTaskRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public ScheduledTask GetDefinition(Type type)
        {
            return Query(c => c.TypeName == type.FullName).Single();
        }

        public void SetLastExecutionTime(int id, DateTime executionTime, DateTime startTime)
        {
            var task = new ScheduledTask
                {
                    Id = id,
                    LastExecution = executionTime,
                    LastStartTime = startTime
                };

            SetFields(task, scheduledTask => scheduledTask.LastExecution, scheduledTask => scheduledTask.LastStartTime);
        }
    }
}
