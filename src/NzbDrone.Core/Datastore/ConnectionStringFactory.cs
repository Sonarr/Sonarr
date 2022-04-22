using System;
using System.Data.SQLite;
using System.Reflection;
using Npgsql;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Datastore
{
    public interface IConnectionStringFactory
    {
        string MainDbConnectionString { get; }
        string LogDbConnectionString { get; }
        string GetDatabasePath(string connectionString);
    }

    public class ConnectionStringFactory : IConnectionStringFactory
    {
        private readonly IConfigFileProvider _configFileProvider;

        public ConnectionStringFactory(IAppFolderInfo appFolderInfo, IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;

            MainDbConnectionString = _configFileProvider.PostgresHost.IsNotNullOrWhiteSpace() ? GetPostgresConnectionString(_configFileProvider.PostgresMainDb) :
                GetConnectionString(appFolderInfo.GetDatabase());

            LogDbConnectionString = _configFileProvider.PostgresHost.IsNotNullOrWhiteSpace() ? GetPostgresConnectionString(_configFileProvider.PostgresLogDb) :
                GetConnectionString(appFolderInfo.GetLogDatabase());
        }

        public string MainDbConnectionString { get; private set; }
        public string LogDbConnectionString { get; private set; }

        public string GetDatabasePath(string connectionString)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder(connectionString);

            return connectionBuilder.DataSource;
        }

        private static string GetConnectionString(string dbPath)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder();

            connectionBuilder.DataSource = dbPath;
            connectionBuilder.CacheSize = (int)-10000;
            connectionBuilder.DateTimeKind = DateTimeKind.Utc;
            connectionBuilder.JournalMode = OsInfo.IsOsx ? SQLiteJournalModeEnum.Truncate : SQLiteJournalModeEnum.Wal;
            connectionBuilder.Pooling = true;
            connectionBuilder.Version = 3;

            if (OsInfo.IsOsx)
            {
                connectionBuilder.Add("Full FSync", true);
            }

            return connectionBuilder.ConnectionString;
        }

        private string GetPostgresConnectionString(string dbName)
        {
            var connectionBuilder = new NpgsqlConnectionStringBuilder();

            connectionBuilder.Database = dbName;
            connectionBuilder.Host = _configFileProvider.PostgresHost;
            connectionBuilder.Username = _configFileProvider.PostgresUser;
            connectionBuilder.Password = _configFileProvider.PostgresPassword;
            connectionBuilder.Port = _configFileProvider.PostgresPort;
            connectionBuilder.Enlist = false;
            connectionBuilder.ApplicationName = "Sonarr v" + Assembly.GetExecutingAssembly().GetName().Version;

            return connectionBuilder.ConnectionString;
        }
    }
}
