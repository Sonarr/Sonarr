using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Processors.SQLite;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class NzbDroneSqliteProcessorFactory : SQLiteProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new MigrationDbFactory();
            var connection = factory.CreateConnection(connectionString);
            var generator = new SQLiteGenerator { compatabilityMode = CompatabilityMode.STRICT };
            return new NzbDroneSqliteProcessor(connection, generator, announcer, options, factory);
        }
    }
}
