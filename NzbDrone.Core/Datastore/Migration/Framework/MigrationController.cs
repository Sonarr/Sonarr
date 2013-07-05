using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Sqlite;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public interface IMigrationController
    {
        void MigrateToLatest(string connectionString, MigrationType migrationType);
    }

    public class MigrationController : IMigrationController
    {
        private readonly IAnnouncer _announcer;
        private readonly ISQLiteAlter _sqLiteAlter;

        private static readonly HashSet<string> MigrationCache = new HashSet<string>();

        public MigrationController(IAnnouncer announcer, ISQLiteAlter sqLiteAlter)
        {
            _announcer = announcer;
            _sqLiteAlter = sqLiteAlter;
        }

        public void MigrateToLatest(string connectionString, MigrationType migrationType)
        {
            lock (MigrationCache)
            {
                _announcer.Heading("Migrating " + connectionString);

                if (MigrationCache.Contains(connectionString.ToLower())) return;

                var assembly = Assembly.GetExecutingAssembly();

                var migrationContext = new RunnerContext(_announcer)
                    {
                        Namespace = "NzbDrone.Core.Datastore.Migration",
                        ApplicationContext = new MigrationContext
                            {
                                MigrationType = migrationType,
                                SQLiteAlter = _sqLiteAlter
                            }
                    };

                var options = new MigrationOptions { PreviewOnly = false, Timeout = 60 };
                var factory = new SqliteProcessorFactory();
                var processor = factory.Create(connectionString, _announcer, options);
                var runner = new MigrationRunner(assembly, migrationContext, processor);
                runner.MigrateUp(true);

                MigrationCache.Add(connectionString.ToLower());
            }
        }
    }
}
