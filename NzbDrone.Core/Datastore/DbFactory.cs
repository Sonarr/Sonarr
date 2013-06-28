using System;
using System.Data.SQLite;
using Marr.Data;
using Marr.Data.Reflection;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Instrumentation;


namespace NzbDrone.Core.Datastore
{
    public interface IDbFactory
    {
        IDatabase Create(MigrationType migrationType = MigrationType.Main);
    }

    public class DbFactory : IDbFactory
    {
        private readonly IMigrationController _migrationController;
        private readonly IAppDirectoryInfo _appDirectoryInfo;

        static DbFactory()
        {
            MapRepository.Instance.ReflectionStrategy = new SimpleReflectionStrategy();
            TableMapping.Map();
        }

        public static void RegisterDatabase(IContainer container)
        {
            container.Register(c => c.Resolve<IDbFactory>().Create());

            container.Register<ILogRepository>(c =>
            {
                var db = c.Resolve<IDbFactory>().Create(MigrationType.Log);
                return new LogRepository(db, c.Resolve<IMessageAggregator>());
            });
        }

        public DbFactory(IMigrationController migrationController, IAppDirectoryInfo appDirectoryInfo)
        {
            _migrationController = migrationController;
            _appDirectoryInfo = appDirectoryInfo;
        }

        public IDatabase Create(MigrationType migrationType = MigrationType.Main)
        {
            string dbPath;

            switch (migrationType)
            {
                case MigrationType.Main:
                    {
                        dbPath = _appDirectoryInfo.GetNzbDroneDatabase();
                        break;
                    }
                case MigrationType.Log:
                    {
                        dbPath = _appDirectoryInfo.GetLogDatabase();
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("Invalid MigrationType");
                    }
            }


            var connectionString = GetConnectionString(dbPath);

            _migrationController.MigrateToLatest(connectionString, migrationType);

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
