using System;
using System.Data.SQLite;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Datastore
{
    public interface IConnectionStringFactory
    {
        bool DisableWal { get; }
        string MainDbConnectionString { get; }
        string LogDbConnectionString { get; }
        string GetDatabasePath(string connectionString);
    }

    public class ConnectionStringFactory : IConnectionStringFactory
    {
        public ConnectionStringFactory(IAppFolderInfo appFolderInfo, IStartupContext startupContext)
        {
            DisableWal = startupContext.Flags.Contains(StartupContext.DISABLE_WAL);
            MainDbConnectionString = GetConnectionString(appFolderInfo.GetDatabase(), DisableWal);
            LogDbConnectionString = GetConnectionString(appFolderInfo.GetLogDatabase(), DisableWal);
        }

        public bool DisableWal { get; private set; }
        public string MainDbConnectionString { get; private set; }
        public string LogDbConnectionString { get; private set; }

        public string GetDatabasePath(string connectionString)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder(connectionString);

            return connectionBuilder.DataSource;
        }

        private static string GetConnectionString(string dbPath, bool disableWal)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder();

            connectionBuilder.DataSource = dbPath;
            connectionBuilder.CacheSize = (int)-10000;
            connectionBuilder.DateTimeKind = DateTimeKind.Utc;

            if (OsInfo.IsOsx || disableWal)
            {
                connectionBuilder.JournalMode = SQLiteJournalModeEnum.Truncate;
            }
            else
            {
                connectionBuilder.JournalMode = SQLiteJournalModeEnum.Wal;
            }

            connectionBuilder.Pooling = true;
            connectionBuilder.Version = 3;
            
            if (OsInfo.IsOsx)
            {
                connectionBuilder.Add("Full FSync", true);
            }

            return connectionBuilder.ConnectionString;
        }
    }
}
