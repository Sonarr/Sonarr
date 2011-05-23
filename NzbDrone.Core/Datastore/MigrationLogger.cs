using System;
using System.Collections.Generic;
using Migrator.Framework;
using NLog;

namespace NzbDrone.Core.Datastore
{
    class MigrationLogger : ILogger
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Started(List<long> currentVersion, long finalVersion)
        {
            Logger.Info("Starting Datastore migration {0} -> {1}", String.Join(",", currentVersion), finalVersion);
        }

        public void MigrateUp(long version, string migrationName)
        {
            Logger.Info("Starting MigrateUp {0} [{1}]", version, migrationName);
        }

        public void MigrateDown(long version, string migrationName)
        {
            Logger.Info("Starting MigrateDown {0} [{1}]", version, migrationName);
        }

        public void Skipping(long version)
        {
            Logger.Info("Skipping MigrateDown {0}", version);
        }

        public void RollingBack(long originalVersion)
        {
            Logger.Info("Rolling Back to {0}", originalVersion);
        }

        public void ApplyingDBChange(string sql)
        {
            Logger.Info("Applying DB Change {0}", sql);
        }

        public void Exception(long version, string migrationName, Exception ex)
        {
            Logger.ErrorException(migrationName + " " + version, ex);
        }

        public void Exception(string message, Exception ex)
        {
            Logger.ErrorException(message, ex);
        }

        public void Finished(List<long> currentVersion, long finalVersion)
        {
            Logger.Info("Finished Datastore migration {0} -> {1}", String.Join(",", currentVersion), finalVersion);
        }

        public void Log(string format, params object[] args)
        {
            Logger.Info(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Logger.Warn(format, args);
        }

        public void Trace(string format, params object[] args)
        {
            Logger.Trace(format, args);
        }
    }
}