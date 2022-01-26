using System;
using System.Data;

namespace NzbDrone.Core.Datastore
{
    public interface IMainDatabase : IDatabase
    {
    }

    public class MainDatabase : IMainDatabase
    {
        private readonly IDatabase _database;
        private readonly DatabaseType _databaseType;

        public MainDatabase(IDatabase database)
        {
            _database = database;
            _databaseType = _database == null ? DatabaseType.SQLite : _database.DatabaseType;
        }

        public IDbConnection OpenConnection()
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
