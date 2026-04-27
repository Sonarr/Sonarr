using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.Datastore
{
    public interface IDatabase
    {
        DbConnection OpenConnection();
        Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
        Version Version { get; }
        int Migration { get; }
        DatabaseType DatabaseType { get; }
        void Vacuum();
        Task VacuumAsync(CancellationToken cancellationToken = default);
    }

    public class Database : IDatabase
    {
        private readonly string _databaseName;
        private readonly Func<DbConnection> _datamapperFactory;
        private readonly Func<CancellationToken, Task<DbConnection>> _datamapperFactoryAsync;

        private readonly Logger _logger = NzbDroneLogger.GetLogger(typeof(Database));

        public Database(string databaseName, Func<DbConnection> datamapperFactory, Func<CancellationToken, Task<DbConnection>> datamapperFactoryAsync)
        {
            _databaseName = databaseName;
            _datamapperFactory = datamapperFactory;
            _datamapperFactoryAsync = datamapperFactoryAsync;
        }

        public DbConnection OpenConnection()
        {
            return _datamapperFactory();
        }

        public Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            return _datamapperFactoryAsync(cancellationToken);
        }

        public DatabaseType DatabaseType
        {
            get
            {
                using var db = _datamapperFactory();

                return db is SQLiteConnection ? DatabaseType.SQLite : DatabaseType.PostgreSQL;
            }
        }

        public Version Version
        {
            get
            {
                using var db = _datamapperFactory();
                var dbConnection = db;

                return DatabaseVersionParser.ParseServerVersion(dbConnection.ServerVersion);
            }
        }

        public int Migration
        {
            get
            {
                using (var db = _datamapperFactory())
                {
                    return db.QueryFirstOrDefault<int>("SELECT \"Version\" from \"VersionInfo\" ORDER BY \"Version\" DESC LIMIT 1");
                }
            }
        }

        public void Vacuum()
        {
            try
            {
                _logger.Info("Vacuuming {0} database", _databaseName);
                using (var db = _datamapperFactory())
                {
                    db.Execute("Vacuum;");
                }

                _logger.Info("{0} database compressed", _databaseName);
            }
            catch (Exception e)
            {
                _logger.Error(e, "An Error occurred while vacuuming database.");
            }
        }

        public async Task VacuumAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.Info("Vacuuming {0} database", _databaseName);
                await using (var db = await _datamapperFactoryAsync(cancellationToken))
                {
                    await db.ExecuteAsync("Vacuum;");
                }

                _logger.Info("{0} database compressed", _databaseName);
            }
            catch (Exception e)
            {
                _logger.Error(e, "An Error occurred while vacuuming database.");
            }
        }
    }

    public enum DatabaseType
    {
        SQLite,
        PostgreSQL
    }
}
