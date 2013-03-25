using System;
using System.Data;
using Marr.Data;
using Mono.Data.Sqlite;
using NzbDrone.Core.Datastore.Migration.Framework;


namespace NzbDrone.Core.Datastore
{
    public interface IDbFactory
    {
        IDatabase Create(string dbPath, MigrationType migrationType = MigrationType.Main);
    }

    public class DbFactory : IDbFactory
    {
        private readonly IMigrationController _migrationController;

        public DbFactory(IMigrationController migrationController)
        {
            TableMapping.Map();
            _migrationController = migrationController;
        }

        public IDatabase Create(string dbPath, MigrationType migrationType = MigrationType.Main)
        {
            var connectionString = GetConnectionString(dbPath);

            _migrationController.MigrateToLatest(connectionString, migrationType);
            var dataMapper = new DataMapper(SqliteFactory.Instance, connectionString);
            return new Database(dataMapper);
        }

        private string GetConnectionString(string dbPath)
        {
            return String.Format("Data Source={0};Version=3;", dbPath);
        }
    }
}
