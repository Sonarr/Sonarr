using System;
using System.Data.Common;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

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

        public DbConnection OpenConnection()
        {
            var connection = _database.OpenConnection();

            if (_databaseType == DatabaseType.PostgreSQL)
            {
                return new ProfiledImplementations.NpgSqlConnection(connection, MiniProfiler.Current);
            }

            return new ProfiledDbConnection(connection, MiniProfiler.Current);
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
