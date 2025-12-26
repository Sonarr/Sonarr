using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Jobs
{
    public interface IScheduledTaskRepository : IBasicRepository<ScheduledTask>
    {
        Task<ScheduledTask> GetDefinitionAsync(Type type, CancellationToken cancellationToken = default);
        Task SetLastExecutionTimeAsync(int id, DateTime executionTime, DateTime startTime, CancellationToken cancellationToken = default);
    }

    public class ScheduledTaskRepository : BasicRepository<ScheduledTask>, IScheduledTaskRepository
    {
        public ScheduledTaskRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public async Task<ScheduledTask> GetDefinitionAsync(Type type, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(c => c.TypeName == type.FullName, cancellationToken).ConfigureAwait(false);
            return results.Single();
        }

        public async Task SetLastExecutionTimeAsync(int id, DateTime executionTime, DateTime startTime, CancellationToken cancellationToken = default)
        {
            var task = new ScheduledTask
            {
                Id = id,
                LastExecution = executionTime,
                LastStartTime = startTime
            };

            await SetFieldsAsync(task, cancellationToken, scheduledTask => scheduledTask.LastExecution, scheduledTask => scheduledTask.LastStartTime).ConfigureAwait(false);
        }
    }
}
