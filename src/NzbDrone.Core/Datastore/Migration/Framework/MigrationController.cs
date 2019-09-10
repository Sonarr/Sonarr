using System;
using System.Diagnostics;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public interface IMigrationController
    {
        void Migrate(string connectionString, MigrationContext migrationContext);
    }

    public class MigrationController : IMigrationController
    {
        private readonly Logger _logger;
        private readonly ILoggerProvider _migrationLoggerProvider;

        public MigrationController(Logger logger,
                                   ILoggerProvider migrationLoggerProvider)
        {
            _logger = logger;
            _migrationLoggerProvider = migrationLoggerProvider;
        }

        public void Migrate(string connectionString, MigrationContext migrationContext)
        {
            var sw = Stopwatch.StartNew();

            _logger.Info("*** Migrating {0} ***", connectionString);

            var serviceProvider = new ServiceCollection()
                .AddLogging(lb => lb.AddProvider(_migrationLoggerProvider))
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder => builder
                    .AddNzbDroneSQLite()
                    .WithGlobalConnectionString(connectionString)
                    .WithMigrationsIn(Assembly.GetExecutingAssembly()))
                .Configure<TypeFilterOptions>(opt => opt.Namespace = "NzbDrone.Core.Datastore.Migration")
                .Configure<ProcessorOptions>(opt => {
                        opt.PreviewOnly = false;
                        opt.Timeout = TimeSpan.FromSeconds(60);
                    })
#pragma warning disable 612
                // This is marked obsolete but the alternative is constructor injection in every migration...
                .Configure<RunnerOptions>(opt => opt.ApplicationContext = migrationContext)
#pragma warning restore 612
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                if (migrationContext.DesiredVersion.HasValue)
                {
                    runner.MigrateUp(migrationContext.DesiredVersion.Value);
                }
                else
                {
                    runner.MigrateUp();
                }

            }

            sw.Stop();

            _logger.Debug("Took: {0}", sw.Elapsed);
        }
    }
}
