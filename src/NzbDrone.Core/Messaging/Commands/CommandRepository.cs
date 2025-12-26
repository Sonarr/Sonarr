using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Messaging.Commands
{
    public interface ICommandRepository : IBasicRepository<CommandModel>
    {
        Task TrimAsync(CancellationToken cancellationToken = default);
        Task OrphanStartedAsync(CancellationToken cancellationToken = default);
        Task<List<CommandModel>> QueuedAsync(CancellationToken cancellationToken = default);
        Task StartAsync(CommandModel command, CancellationToken cancellationToken = default);
        Task EndAsync(CommandModel command, CancellationToken cancellationToken = default);
    }

    public class CommandRepository : BasicRepository<CommandModel>, ICommandRepository
    {
        public CommandRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public async Task TrimAsync(CancellationToken cancellationToken = default)
        {
            var date = DateTime.UtcNow.AddDays(-1);

            await DeleteAsync(c => c.EndedAt < date, cancellationToken).ConfigureAwait(false);
        }

        public async Task OrphanStartedAsync(CancellationToken cancellationToken = default)
        {
            var sql = @"UPDATE ""Commands"" SET ""Status"" = @Orphaned, ""EndedAt"" = @Ended WHERE ""Status"" = @Started";
            var args = new
            {
                Orphaned = (int)CommandStatus.Orphaned,
                Started = (int)CommandStatus.Started,
                Ended = DateTime.UtcNow
            };

            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            var cmd = new CommandDefinition(sql, args, cancellationToken: cancellationToken);
            await conn.ExecuteAsync(cmd).ConfigureAwait(false);
        }

        public async Task<List<CommandModel>> QueuedAsync(CancellationToken cancellationToken = default)
        {
            return await QueryAsync(x => x.Status == CommandStatus.Queued, cancellationToken).ConfigureAwait(false);
        }

        public async Task StartAsync(CommandModel command, CancellationToken cancellationToken = default)
        {
            await SetFieldsAsync(command, cancellationToken, c => c.StartedAt, c => c.Status).ConfigureAwait(false);
        }

        public async Task EndAsync(CommandModel command, CancellationToken cancellationToken = default)
        {
            await SetFieldsAsync(command, cancellationToken, c => c.EndedAt, c => c.Status, c => c.Duration, c => c.Exception).ConfigureAwait(false);
        }
    }
}
