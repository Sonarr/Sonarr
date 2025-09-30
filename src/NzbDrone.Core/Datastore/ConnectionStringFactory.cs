using System;
using System.Data.SQLite;
using Npgsql;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Datastore
{
    public interface IConnectionStringFactory
    {
        DatabaseConnectionInfo MainDbConnection { get; }
        DatabaseConnectionInfo LogDbConnection { get; }
        string GetDatabasePath(string connectionString);
    }

    public class ConnectionStringFactory : IConnectionStringFactory
    {
        private readonly IConfigFileProvider _configFileProvider;
        private bool _usePostgres;
        private bool _usePostgresConnectionStrings;

        public ConnectionStringFactory(IAppFolderInfo appFolderInfo, IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;

            ValidatePostgresOptions();

            if (_usePostgres)
            {
                if (_usePostgresConnectionStrings)
                {
                    MainDbConnection = GetPostgresConnectionInfoFromConnectionString(_configFileProvider.PostgresMainDbConnectionString);
                    LogDbConnection = GetPostgresConnectionInfoFromConnectionString(_configFileProvider.PostgresLogDbConnectionString);
                    return;
                }

                MainDbConnection = GetPostgresConnectionInfoFromIndividualValues(_configFileProvider.PostgresMainDb);
                LogDbConnection = GetPostgresConnectionInfoFromIndividualValues(_configFileProvider.PostgresLogDb);
                return;
            }

            // Default to sqlite
            MainDbConnection = GetConnectionString(appFolderInfo.GetDatabase());
            LogDbConnection = GetConnectionString(appFolderInfo.GetLogDatabase());
        }

        public DatabaseConnectionInfo MainDbConnection { get; private set; }
        public DatabaseConnectionInfo LogDbConnection { get; private set; }

        public string GetDatabasePath(string connectionString)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder(connectionString);

            return connectionBuilder.DataSource;
        }

        private static DatabaseConnectionInfo GetConnectionString(string dbPath)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                CacheSize = (int)-20000,
                DateTimeKind = DateTimeKind.Utc,
                JournalMode = OsInfo.IsOsx ? SQLiteJournalModeEnum.Truncate : SQLiteJournalModeEnum.Wal,
                Pooling = true,
                Version = 3,
                BusyTimeout = 100
            };

            if (OsInfo.IsOsx)
            {
                connectionBuilder.Add("Full FSync", true);
            }

            return new DatabaseConnectionInfo(DatabaseType.SQLite, connectionBuilder.ConnectionString);
        }

        private DatabaseConnectionInfo GetPostgresConnectionInfoFromIndividualValues(string dbName)
        {
            var connectionBuilder = new NpgsqlConnectionStringBuilder()
            {
                Database = dbName,
                Host = _configFileProvider.PostgresHost,
                Username = _configFileProvider.PostgresUser,
                Password = _configFileProvider.PostgresPassword,
                Port = _configFileProvider.PostgresPort,
                Enlist = false
            };

            return new DatabaseConnectionInfo(DatabaseType.PostgreSQL, connectionBuilder.ConnectionString);
        }

        private DatabaseConnectionInfo GetPostgresConnectionInfoFromConnectionString(string connectionString)
        {
            var connectionBuilder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Enlist = false
            };

            return new DatabaseConnectionInfo(DatabaseType.PostgreSQL, connectionBuilder.ConnectionString);
        }

        /// <summary>
        /// Validates that either Postgres connection strings are both set or neither are set, and that either connection strings or
        /// other Postgres settings are set, but not both.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
        private void ValidatePostgresOptions()
        {
            var isMainDBConnectionStringSet = !string.IsNullOrWhiteSpace(_configFileProvider.PostgresMainDbConnectionString);
            var isLogDBConnectionStringSet = !string.IsNullOrWhiteSpace(_configFileProvider.PostgresLogDbConnectionString);
            var isHostSet = !string.IsNullOrWhiteSpace(_configFileProvider.PostgresHost);

            if (!isHostSet && !isMainDBConnectionStringSet && !isLogDBConnectionStringSet)
            {
                // No Postgres settings are set, so nothing to validate
                return;
            }

            _usePostgres = true;

            if (_configFileProvider.LogDbEnabled)
            {
                if (!isMainDBConnectionStringSet && isLogDBConnectionStringSet)
                {
                    throw new ArgumentException("Postgres MainDbConnectionString is set but LogDbConnectionString is not. Both must be set or neither.");
                }

                if (isLogDBConnectionStringSet && !isMainDBConnectionStringSet)
                {
                    throw new ArgumentException("Postgres LogDbConnectionString is set but MainDbConnectionString is not. Both must be set or neither.");
                }
            }

            // At this point either all required connection strings are set or neither, so only one needs to be checked
            var areConnectionStringConfigsSet = isMainDBConnectionStringSet;

            // This one _must_ be set if connection strings are not being used, so it is used as a test to see if the user attempted configuration via individual settings
            var areOtherPostgresConfigsSet = _configFileProvider.PostgresHost.IsNotNullOrWhiteSpace();

            if (areConnectionStringConfigsSet && areOtherPostgresConfigsSet)
            {
                throw new ArgumentException($"Either both Postgres connection strings must be set, or the other Postgres settings must be set, but not both.");
            }

            _usePostgresConnectionStrings = areConnectionStringConfigsSet;
        }
    }
}
