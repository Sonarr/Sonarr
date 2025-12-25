using System;
using System.Data;
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
        IDbConnection OpenConnection();
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
        private readonly Func<IDbConnection> _datamapperFactory;
        private readonly Func<CancellationToken, Task<IDbConnection>> _asyncDatamapperFactory;

        private readonly Logger _logger = NzbDroneLogger.GetLogger(typeof(Database));

        public Database(string databaseName, Func<IDbConnection> datamapperFactory, Func<CancellationToken, Task<IDbConnection>> asyncDatamapperFactory)
        {
            _databaseName = databaseName;
            _datamapperFactory = datamapperFactory;
            _asyncDatamapperFactory = asyncDatamapperFactory;
        }

        public IDbConnection OpenConnection()
        {
            return _datamapperFactory();
        }

        public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            return (DbConnection)await _asyncDatamapperFactory(cancellationToken).ConfigureAwait(false);
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
                var dbConnection = db as DbConnection;

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
                using (var db = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false))
                {
                    await db.ExecuteAsync(new CommandDefinition("Vacuum;", cancellationToken: cancellationToken)).ConfigureAwait(false);
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
