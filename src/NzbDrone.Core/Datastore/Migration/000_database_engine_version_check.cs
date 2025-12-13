using System.Data;
using System.Text.RegularExpressions;
using FluentMigrator;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.Datastore.Migration
{
    [Maintenance(MigrationStage.BeforeAll, TransactionBehavior.None)]
    public class DatabaseEngineVersionCheck : ForwardOnlyMigration
    {
        protected readonly Logger _logger;

        public DatabaseEngineVersionCheck()
        {
            _logger = NzbDroneLogger.GetLogger(this);
        }

        public override void Up()
        {
            IfDatabase(ProcessorIdConstants.SQLite).Execute.WithConnection(LogSqliteVersion);
            IfDatabase(ProcessorIdConstants.PostgreSQL).Execute.WithConnection(LogPostgresVersion);
        }

        private void LogSqliteVersion(IDbConnection conn, IDbTransaction tran)
        {
            using (var versionCmd = conn.CreateCommand())
            {
                versionCmd.Transaction = tran;
                versionCmd.CommandText = "SELECT sqlite_version();";

                using (var reader = versionCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var version = reader.GetString(0);

                        _logger.Info("SQLite {0}", version);
                    }
                }
            }
        }

        private void LogPostgresVersion(IDbConnection conn, IDbTransaction tran)
        {
            using (var versionCmd = conn.CreateCommand())
            {
                versionCmd.Transaction = tran;
                versionCmd.CommandText = "SHOW server_version";

                using (var reader = versionCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var version = reader.GetString(0);
                        var cleanVersion = Regex.Replace(version, @"\(.*?\)", "");

                        _logger.Info("Postgres {0}", cleanVersion);
                    }
                }
            }
        }
    }
}
