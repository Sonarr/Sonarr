using System;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using NLog;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class DatabaseIntegrityCheck : IProvideHealthCheck
    {
        private readonly IMainDatabase _database;
        private readonly Logger _logger;

        public DatabaseIntegrityCheck(IMainDatabase database, Logger logger)
        {
            _database = database;
            _logger = logger;
        }

        public bool CheckOnStartup => true;

        public bool CheckOnSchedule => true;

        public HealthCheck Check()
        {
            try
            {
                if (_database.DatabaseType != DatabaseType.SQLite)
                {
                    return new HealthCheck(GetType());
                }

                using var connection = _database.OpenConnection();
                var result = connection.Query<string>("PRAGMA quick_check(1);").FirstOrDefault();

                if (string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
                {
                    return new HealthCheck(GetType());
                }

                var message = string.IsNullOrWhiteSpace(result)
                    ? "SQLite database integrity check did not return a result. Restore from a known-good backup if database errors are occurring."
                    : $"SQLite database integrity check failed: {result}. Restore from a known-good backup.";

                _logger.Error(message);

                return new HealthCheck(
                    GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.DatabaseIntegrity,
                    message,
                    "#i-am-getting-an-error-database-disk-image-is-malformed");
            }
            catch (SQLiteException ex)
            {
                _logger.Error(ex, "SQLite database integrity check failed");

                return new HealthCheck(
                    GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.DatabaseIntegrity,
                    $"SQLite database integrity check failed: {ex.Message}. Restore from a known-good backup.",
                    "#i-am-getting-an-error-database-disk-image-is-malformed");
            }
        }
    }
}
