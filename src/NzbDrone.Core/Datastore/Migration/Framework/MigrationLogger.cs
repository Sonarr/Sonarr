using System;
using FluentMigrator.Runner;
using NLog;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class MigrationLogger : IAnnouncer
    {
        private readonly Logger _logger;


        public MigrationLogger(Logger logger)
        {
            _logger = logger;
        }


        public void Heading(string message)
        {
            _logger.Info("*** {0} ***", message);
        }

        public void Say(string message)
        {
            _logger.Debug(message);
        }

        public void Emphasize(string message)
        {
            _logger.Warn(message);
        }

        public void Sql(string sql)
        {
            _logger.Debug(sql);
        }

        public void ElapsedTime(TimeSpan timeSpan)
        {
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Write(string message, bool escaped)
        {
            _logger.Info(message);
        }
    }
}
