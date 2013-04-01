using System;
using System.Data.SQLite;
using Marr.Data;
using Marr.Data.Reflection;
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

        static DbFactory()
        {
            TableMapping.Map();
        }

        public DbFactory(IMigrationController migrationController)
        {
            _migrationController = migrationController;
        }

        public IDatabase Create(string dbPath, MigrationType migrationType = MigrationType.Main)
        {
            var connectionString = GetConnectionString(dbPath);

            _migrationController.MigrateToLatest(connectionString, migrationType);
            var dataMapper = new DataMapper(SQLiteFactory.Instance, connectionString)
                {
                    SqlMode = SqlModes.Text,
                };

            MapRepository.Instance.ReflectionStrategy = new SimpleReflectionStrategy();

            return new Database(dataMapper);
        }

        private string GetConnectionString(string dbPath)
        {
            return String.Format("Data Source={0};Version=3;", dbPath);
        }
    }
}
