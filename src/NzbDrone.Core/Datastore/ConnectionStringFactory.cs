using System;
using System.Data.SQLite;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Datastore
{
    public interface IConnectionStringFactory
    {
        string MainDbConnectionString { get; }
        string LogDbConnectionString { get; }
    }

    public class ConnectionStringFactory : IConnectionStringFactory
    {
        public ConnectionStringFactory(IAppFolderInfo appFolderInfo)
        {
            MainDbConnectionString = GetConnectionString(appFolderInfo.GetNzbDroneDatabase());
            LogDbConnectionString = GetConnectionString(appFolderInfo.GetLogDatabase());
        }

        public string MainDbConnectionString { get; private set; }
        public string LogDbConnectionString { get; private set; }

        private static string GetConnectionString(string dbPath)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder();

            connectionBuilder.DataSource = dbPath;
            connectionBuilder.CacheSize = (int)-10.Megabytes();
            connectionBuilder.DateTimeKind = DateTimeKind.Utc;
            connectionBuilder.JournalMode = OsInfo.IsOsx ? SQLiteJournalModeEnum.Truncate : SQLiteJournalModeEnum.Wal;
            connectionBuilder.Pooling = true;

            return connectionBuilder.ConnectionString;
        }
    }
}