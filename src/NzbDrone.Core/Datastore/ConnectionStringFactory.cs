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

        public ConnectionStringFactory(IAppFolderInfo appFolderInfo, IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;

            MainDbConnection = _configFileProvider.PostgresHost.IsNotNullOrWhiteSpace() ? GetPostgresConnectionString(_configFileProvider.PostgresMainDb) :
                GetConnectionString(appFolderInfo.GetDatabase());

            LogDbConnection = _configFileProvider.PostgresHost.IsNotNullOrWhiteSpace() ? GetPostgresConnectionString(_configFileProvider.PostgresLogDb) :
                GetConnectionString(appFolderInfo.GetLogDatabase());
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

        private DatabaseConnectionInfo GetPostgresConnectionString(string dbName)
        {
            var connectionBuilder = new NpgsqlConnectionStringBuilder
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
    }
}
