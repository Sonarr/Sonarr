using System;
using System.Data.SQLite;
using System.IO;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NLog;

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
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public ConnectionStringFactory(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, IConfigFileProvider configFileProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _configFileProvider = configFileProvider;
            _logger = logger;

            MainDbConnectionString = GetConnectionString(appFolderInfo.GetDatabase());
            LogDbConnectionString = GetConnectionString(appFolderInfo.GetLogDatabase());
        }

        public string MainDbConnectionString { get; private set; }
        public string LogDbConnectionString { get; private set; }

        public string GetDatabasePath(string connectionString)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder(connectionString);

            return connectionBuilder.DataSource;
        }

        private string GetConnectionString(string dbPath)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder();

            connectionBuilder.DataSource = dbPath;
            connectionBuilder.CacheSize = (int)-10000;
            connectionBuilder.DateTimeKind = DateTimeKind.Utc;

            connectionBuilder.JournalMode = GetJournalMode(dbPath);

            if (connectionBuilder.JournalMode == SQLiteJournalModeEnum.Truncate)
            {
                connectionBuilder.Add("Full FSync", true);
            }

            connectionBuilder.Pooling = true;
            connectionBuilder.Version = 3;
           
            return connectionBuilder.ConnectionString;
        }

        private SQLiteJournalModeEnum GetJournalMode(string path)
        {
            var driveType = _diskProvider.GetMount(path).DriveType;

            if (driveType == DriveType.Network || driveType == DriveType.Unknown)
            {
                _logger.Debug("Network filesystem store for application data detected, disabling WAL mode for SQLite");
                return SQLiteJournalModeEnum.Truncate;
            }

            if (_configFileProvider.DatabaseJournalMode != (DatabaseJournalType)SQLiteJournalModeEnum.Wal)
            {
                _logger.Debug("DatabaseJournalMode tag detected in config.xml, disabling WAL mode for SQLite");
                return SQLiteJournalModeEnum.Truncate;
            }

            if (OsInfo.IsOsx)
            {
                _logger.Debug("macOS operating system detected, disabling WAL mode for SQLite");
                return SQLiteJournalModeEnum.Truncate;
            }

            return SQLiteJournalModeEnum.Wal;
        }
    }
}