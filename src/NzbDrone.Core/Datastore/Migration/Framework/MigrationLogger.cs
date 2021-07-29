using System;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Logging;
using NLog;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class MigrationLogger : FluentMigratorLogger
    {
        private readonly Logger _logger;

        public MigrationLogger(Logger logger,
                               FluentMigratorLoggerOptions options)
        : base(options)
        {
            _logger = logger;
        }

        protected override void WriteHeading(string message)
        {
            _logger.Info("*** {0} ***", message);
        }

        protected override void WriteSay(string message)
        {
            _logger.Debug(message);
        }

        protected override void WriteEmphasize(string message)
        {
            _logger.Warn(message);
        }

        protected override void WriteSql(string sql)
        {
            _logger.Debug(sql);
        }

        protected override void WriteEmptySql()
        {
            _logger.Debug(@"No SQL statement executed.");
        }

        protected override void WriteElapsedTime(TimeSpan timeSpan)
        {
            _logger.Debug("Took: {0}", timeSpan);
        }

        protected override void WriteError(string message)
        {
            _logger.Error(message);
        }

        protected override void WriteError(Exception exception)
        {
            _logger.Error(exception);
        }
    }
}
