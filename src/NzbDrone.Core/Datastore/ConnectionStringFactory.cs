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

            if (OsInfo.IsOsx)
            {
                connectionBuilder.Add("Full FSync", true);
            }

            connectionBuilder.Pooling = true;
            connectionBuilder.Version = 3;
           
            return connectionBuilder.ConnectionString;
        }

        private SQLiteJournalModeEnum GetJournalMode(string path)
        {
            if (_configFileProvider.DatabaseJournalMode != DatabaseJournalType.Auto)
            {
                _logger.Debug("Database journal mode overridden by config.xml, using {0} journal mode", _configFileProvider.DatabaseJournalMode);
                return (SQLiteJournalModeEnum)_configFileProvider.DatabaseJournalMode;
            }

            if (OsInfo.IsOsx)
            {
                _logger.Debug("macOS operating system, using Truncate database journal mode");
                return SQLiteJournalModeEnum.Truncate;
            }

            var mount = _diskProvider.GetMount(path);

            if (mount == null)
            {
                _logger.Debug("Database {0} located on unknown filesystem, using Truncate journal mode", path);
                return SQLiteJournalModeEnum.Truncate;
            }

            if (mount.DriveType == DriveType.Network || mount.DriveType == DriveType.Unknown)
            {
                _logger.Debug("Database {0} located on filesystem {1} with type {2}, using Truncate journal mode", path, mount.DriveFormat, mount.DriveType);
                return SQLiteJournalModeEnum.Truncate;
            }
            else
            {
                _logger.Debug("Database {0} located on filesystem {1} with type {2}, using WAL journal mode", path, mount.DriveFormat, mount.DriveType);
            }

            return SQLiteJournalModeEnum.Wal;
        }
    }
}