using System;
using System.Data.SQLite;
using System.IO;
using NzbDrone.Common.Disk;
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
        public ConnectionStringFactory(IAppFolderInfo appFolderInfo, IStartupContext startupContext, IDiskProvider diskProvider)
        {
            DisableWal = startupContext.Flags.Contains(StartupContext.DISABLE_WAL);
            AppDataDriveType = diskProvider.GetMount(appFolderInfo.AppDataFolder).DriveType;

            MainDbConnectionString = GetConnectionString(appFolderInfo.GetDatabase(), DisableWal, AppDataDriveType);
            LogDbConnectionString = GetConnectionString(appFolderInfo.GetLogDatabase(), DisableWal, AppDataDriveType);
        }


        public bool DisableWal { get; private set; }

        public DriveType AppDataDriveType { get; private set; }
        public string MainDbConnectionString { get; private set; }
        public string LogDbConnectionString { get; private set; }

        public string GetDatabasePath(string connectionString)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder(connectionString);

            return connectionBuilder.DataSource;
        }

        private static string GetConnectionString(string dbPath, bool disableWal, DriveType appDataDriveType)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder();

            connectionBuilder.DataSource = dbPath;
            connectionBuilder.CacheSize = (int)-10000;
            connectionBuilder.DateTimeKind = DateTimeKind.Utc;

            connectionBuilder.JournalMode = SQLiteJournalModeEnum.Wal;

            if (appDataDriveType == DriveType.Network)
            {
                connectionBuilder.JournalMode = SQLiteJournalModeEnum.Truncate;
            }

            if (disableWal)
            {
                connectionBuilder.JournalMode = SQLiteJournalModeEnum.Truncate;
            }

            if(OsInfo.IsOsx)
            {
                connectionBuilder.JournalMode = SQLiteJournalModeEnum.Truncate;
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
