using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Sqlite;

namespace NzbDrone.Core.Datastore.Migration.Sqlite
{
    public class MonoSqliteProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new MonoSqliteDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new SqliteProcessor(connection, new SqliteGenerator(), announcer, options, factory);
        }
    }
}