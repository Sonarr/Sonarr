using System.Data.SQLite;
using System.Diagnostics;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public interface IMigrationController
    {
        void Migrate(string connectionString, MigrationContext migrationContext);
    }

    public class MigrationController : IMigrationController
    {
        private readonly IAnnouncer _announcer;

        public MigrationController(IAnnouncer announcer)
        {
            _announcer = announcer;
        }

        public void Migrate(string connectionString, MigrationContext migrationContext)
        {
            var sw = Stopwatch.StartNew();

            _announcer.Heading("Migrating " + connectionString);

            var assembly = Assembly.GetExecutingAssembly();

            var runnerContext = new RunnerContext(_announcer)
            {
                Namespace = "NzbDrone.Core.Datastore.Migration",
                ApplicationContext = migrationContext
            };

            var options = new MigrationOptions { PreviewOnly = false, Timeout = 60 };
            var factory = new NzbDroneSqliteProcessorFactory();
            var processor = factory.Create(connectionString, _announcer, options);

            try
            {
                var runner = new MigrationRunner(assembly, runnerContext, processor);

                if (migrationContext.DesiredVersion.HasValue)
                {
                    runner.MigrateUp(migrationContext.DesiredVersion.Value, true);
                }
                else
                {
                    runner.MigrateUp(true);
                }

                processor.Dispose();
            }
            catch (SQLiteException)
            {
                processor.Dispose();
                SQLiteConnection.ClearAllPools();
                throw;
            }

            sw.Stop();

            _announcer.ElapsedTime(sw.Elapsed);
        }
    }
}
