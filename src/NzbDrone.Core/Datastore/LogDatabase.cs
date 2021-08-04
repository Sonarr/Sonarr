using System;
using System.Data;

namespace NzbDrone.Core.Datastore
{
    public interface ILogDatabase : IDatabase
    {
    }

    public class LogDatabase : ILogDatabase
    {
        private readonly IDatabase _database;

        public LogDatabase(IDatabase database)
        {
            _database = database;
        }

        public IDbConnection OpenConnection()
        {
            return _database.OpenConnection();
        }

        public Version Version => _database.Version;

        public int Migration => _database.Migration;

        public void Vacuum()
        {
            _database.Vacuum();
        }
    }
}
