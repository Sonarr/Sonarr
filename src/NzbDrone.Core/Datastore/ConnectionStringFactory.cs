using System;
using System.Data.SQLite;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;

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
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(ConnectionStringFactory));

        public ConnectionStringFactory(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider)
        {
            var mount = diskProvider.GetMount(appFolderInfo.AppDataFolder);
            var isNetworkDrive = mount.DriveType == System.IO.DriveType.Network;
            if (isNetworkDrive)
            {
                Logger.Warn("AppData folder {0} is located on the network drive {1} using a {2} filesystem. Is highly discouraged to use a SQLite database on network drives and may lead to database corruption.",
                    appFolderInfo.AppDataFolder, mount.RootDirectory, mount.DriveFormat);
            }

            MainDbConnectionString = GetConnectionString(appFolderInfo.GetNzbDroneDatabase(), isNetworkDrive);
            LogDbConnectionString = GetConnectionString(appFolderInfo.GetLogDatabase(), isNetworkDrive);
        }

        public string MainDbConnectionString { get; private set; }
        public string LogDbConnectionString { get; private set; }

        public string GetDatabasePath(string connectionString)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder(connectionString);

            return connectionBuilder.DataSource;
        }

        private static string GetConnectionString(string dbPath, bool isNetworkDrive)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder();

            connectionBuilder.DataSource = dbPath;
            connectionBuilder.CacheSize = (int)-10.Megabytes();
            connectionBuilder.DateTimeKind = DateTimeKind.Utc;
            connectionBuilder.JournalMode = OsInfo.IsOsx || isNetworkDrive ? SQLiteJournalModeEnum.Truncate : SQLiteJournalModeEnum.Wal;
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