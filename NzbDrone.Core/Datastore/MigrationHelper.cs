using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

namespace NzbDrone.Core.Datastore
{
    public static class MigrationHelper
    {
        public static void MigrateToLatest(string connectionString, MigrationType migrationType)
        {
            var announcer = new NlogAnnouncer();
            var assembly = Assembly.GetExecutingAssembly();

            var migrationContext = new RunnerContext(announcer)
            {
                Namespace = "NzbDrone.Core.Datastore.Migrations",
                ApplicationContext = migrationType
            };

            var options = new MigrationOptions { PreviewOnly = false, Timeout = 60 };
            var factory = new FluentMigrator.Runner.Processors.Sqlite.SqliteProcessorFactory();
            var processor = factory.Create(connectionString, announcer, options);
            var runner = new MigrationRunner(assembly, migrationContext, processor);
            runner.MigrateUp(true);
        }
    }

    public class MigrationOptions : IMigrationProcessorOptions
    {
        public bool PreviewOnly { get; set; }
        public int Timeout { get; set; }
    }
}
