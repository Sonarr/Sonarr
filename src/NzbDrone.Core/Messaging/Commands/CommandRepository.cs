using System;
using System.Collections.Generic;
using System.Data.SQLite;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Messaging.Commands
{
    public interface ICommandRepository : IBasicRepository<CommandModel>
    {
        void Trim();
        void OrphanStarted();
        List<CommandModel> FindCommands(string name);
        List<CommandModel> FindQueuedOrStarted(string name);
        List<CommandModel> Queued();
        List<CommandModel> Started();
        void Start(CommandModel command);
        void End(CommandModel command);
    }

    public class CommandRepository : BasicRepository<CommandModel>, ICommandRepository
    {
        private readonly IMainDatabase _database;

        public CommandRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            _database = database;
        }

        public void Trim()
        {
            var date = DateTime.UtcNow.AddDays(-1);

            Delete(c => c.EndedAt < date);
        }

        public void OrphanStarted()
        {
            var mapper = _database.GetDataMapper();

            mapper.Parameters.Add(new SQLiteParameter("@orphaned", (int)CommandStatus.Orphaned));
            mapper.Parameters.Add(new SQLiteParameter("@started", (int)CommandStatus.Started));
            mapper.Parameters.Add(new SQLiteParameter("@ended", DateTime.UtcNow));

            mapper.ExecuteNonQuery(@"UPDATE Commands
                                     SET Status = @orphaned, EndedAt = @ended
                                     WHERE Status = @started");
        }

        public List<CommandModel> FindCommands(string name)
        {
            return Query.Where(c => c.Name == name).ToList();
        }

        public List<CommandModel> FindQueuedOrStarted(string name)
        {
            return Query.Where(c => c.Name == name)
                        .AndWhere("[Status] IN (0,1)")
                        .ToList();
        }

        public List<CommandModel> Queued()
        {
            return Query.Where(c => c.Status == CommandStatus.Queued);
        }

        public List<CommandModel> Started()
        {
            return Query.Where(c => c.Status == CommandStatus.Started);
        }

        public void Start(CommandModel command)
        {
            SetFields(command, c => c.StartedAt, c => c.Status);
        }

        public void End(CommandModel command)
        {
            SetFields(command, c => c.EndedAt, c => c.Status, c => c.Duration, c => c.Exception);
        }
    }
}
