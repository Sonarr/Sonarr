using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using Dapper;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.Datastore
{
    public interface IDatabase
    {
        IDbConnection OpenConnection();
        Version Version { get; }
        int Migration { get; }
        DatabaseType DatabaseType { get; }
        void Vacuum();
    }

    public class Database : IDatabase
    {
        private readonly string _databaseName;
        private readonly Func<IDbConnection> _datamapperFactory;

        private readonly Logger _logger = NzbDroneLogger.GetLogger(typeof(Database));

        public Database(string databaseName, Func<IDbConnection> datamapperFactory)
        {
            _databaseName = databaseName;
            _datamapperFactory = datamapperFactory;
        }

        public IDbConnection OpenConnection()
        {
            return _datamapperFactory();
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
                var version = Regex.Replace(dbConnection.ServerVersion, @"\(.*?\)", "");

                return new Version(version);
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
    }

    public enum DatabaseType
    {
        SQLite,
        PostgreSQL
    }
}
