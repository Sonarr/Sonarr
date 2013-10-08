using System;
using System.Data.SQLite;
using Marr.Data;
using Marr.Data.Reflection;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Datastore
{
    public interface IDbFactory
    {
        IDatabase Create(MigrationType migrationType = MigrationType.Main);
    }


    public class DbFactory : IDbFactory
    {
        private readonly IMigrationController _migrationController;
        private readonly IConnectionStringFactory _connectionStringFactory;

        static DbFactory()
        {
            MapRepository.Instance.ReflectionStrategy = new SimpleReflectionStrategy();
            TableMapping.Map();
        }

        public static void RegisterDatabase(IContainer container)
        {
            container.Resolve<IDbFactory>().Create();

            container.Register(c => c.Resolve<IDbFactory>().Create());

            container.Resolve<IDbFactory>().Create(MigrationType.Log);

            container.Register<ILogRepository>(c =>
            {
                var db = c.Resolve<IDbFactory>().Create(MigrationType.Log);
                return new LogRepository(db, c.Resolve<IEventAggregator>());
            });
        }

        public DbFactory(IMigrationController migrationController, IConnectionStringFactory connectionStringFactory)
        {
            _migrationController = migrationController;
            _connectionStringFactory = connectionStringFactory;
        }

        public IDatabase Create(MigrationType migrationType = MigrationType.Main)
        {
            string connectionString;


            switch (migrationType)
            {
                case MigrationType.Main:
                    {
                        connectionString = _connectionStringFactory.MainDbConnectionString;
                        break;
                    }
                case MigrationType.Log:
                    {
                        connectionString = _connectionStringFactory.LogDbConnectionString;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("Invalid MigrationType");
                    }
            }



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
    }
}
