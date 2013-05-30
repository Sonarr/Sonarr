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


            MapRepository.Instance.ReflectionStrategy = new SimpleReflectionStrategy();

            return new Database(() =>
                {
                    var dataMapper = new DataMapper(SQLiteFactory.Instance, connectionString)
                    {
                        SqlMode = SqlModes.Text,
                    };

                    return dataMapper;
                });
        }

        private string GetConnectionString(string dbPath)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder();

            connectionBuilder.DataSource = dbPath;
            connectionBuilder.CacheSize = (int)-10.Megabytes();
            connectionBuilder.DateTimeKind = DateTimeKind.Utc;
            connectionBuilder.JournalMode = SQLiteJournalModeEnum.Wal;

            return connectionBuilder.ConnectionString;
        }
    }
}
