using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Jobs
{
    public interface IScheduledTaskRepository : IBasicRepository<ScheduledTask>
    {
        ScheduledTask GetDefinition(Type type);
        void SetLastExecutionTime(int id, DateTime executionTime);
    }


    public class ScheduledTaskRepository : BasicRepository<ScheduledTask>, IScheduledTaskRepository
    {

        public ScheduledTaskRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
        }

        public ScheduledTask GetDefinition(Type type)
        {
            return Query.Single(c => c.TypeName == type.FullName);
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
