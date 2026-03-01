using System;
using System.Data.Common;

namespace NzbDrone.Core.Datastore
{
    public interface ILogDatabase : IDatabase
    {
    }

    public class LogDatabase : ILogDatabase
    {
        private readonly IDatabase _database;
        private readonly DatabaseType _databaseType;

        public LogDatabase(IDatabase database)
        {
            _database = database;
            _databaseType = _database == null ? DatabaseType.SQLite : _database.DatabaseType;
        }

        public DbConnection OpenConnection()
        {
            return _database.OpenConnection();
        }

        public Version Version => _database.Version;

        public int Migration => _database.Migration;

        public DatabaseType DatabaseType => _databaseType;

        public void Vacuum()
        {
            _database.Vacuum();
        }
    }
}
