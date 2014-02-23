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
        private readonly ISqLiteMigrationHelper _migrationHelper;

        public MigrationController(IAnnouncer announcer, ISQLiteAlter sqLiteAlter, ISqLiteMigrationHelper migrationHelper)
        {
            _announcer = announcer;
            _sqLiteAlter = sqLiteAlter;
            _migrationHelper = migrationHelper;
        }

        public void MigrateToLatest(string connectionString, MigrationType migrationType)
        {
            _announcer.Heading("Migrating " + connectionString);

            var assembly = Assembly.GetExecutingAssembly();

            var migrationContext = new RunnerContext(_announcer)
                {
                    Namespace = "NzbDrone.Core.Datastore.Migration",
                    ApplicationContext = new MigrationContext
                        {
                            MigrationType = migrationType,
                            SQLiteAlter = _sqLiteAlter,
                            MigrationHelper = _migrationHelper,
                        }
                };

            var options = new MigrationOptions { PreviewOnly = false, Timeout = 60 };
            var factory = new SqliteProcessorFactory();
            var processor = factory.Create(connectionString, _announcer, options);
            var runner = new MigrationRunner(assembly, migrationContext, processor);
            runner.MigrateUp(true);
        }
    }
}
