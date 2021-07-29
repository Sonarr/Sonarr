using FluentMigrator.Runner;
using Microsoft.Extensions.Logging;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class MigrationLoggerProvider : ILoggerProvider
    {
        private readonly Logger _logger;

        public MigrationLoggerProvider(Logger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new MigrationLogger(_logger, new FluentMigratorLoggerOptions() { ShowElapsedTime = true, ShowSql = true });
        }

        public void Dispose()
        {
        }
    }
}
