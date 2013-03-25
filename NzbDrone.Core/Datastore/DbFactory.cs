using System;
using System.Data;
using Marr.Data;
using Mono.Data.Sqlite;


namespace NzbDrone.Core.Datastore
{
    public interface IDbFactory
    {
        IDatabase Create(string dbPath = null);
    }

    public class DbFactory : IDbFactory
    {
        private const string MemoryConnectionString = "Data Source=:memory:;Version=3;New=True;";

        public IDatabase Create(string dbPath = null)
        {
            var connectionString = MemoryConnectionString;

            if (!string.IsNullOrWhiteSpace(dbPath))
            {
                connectionString = GetConnectionString(dbPath);
            }

            MigrationHelper.MigrateToLatest(connectionString, MigrationType.Main);
            var dataMapper = new DataMapper(SqliteFactory.Instance, connectionString);
            return new Database(dataMapper);
        }



        private string GetConnectionString(string dbPath)
        {
            return String.Format("Data Source={0};Version=3;", dbPath);
        }
    }
}
