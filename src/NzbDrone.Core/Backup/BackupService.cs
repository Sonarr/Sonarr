using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Backup
{
    public interface IBackupService
    {
        void Backup(BackupType backupType);
        List<Backup> GetBackups();
        void Restore(string backupFileName);
        string GetBackupFolder();
        string GetBackupFolder(BackupType backupType);
    }

    public class BackupService : IBackupService, IExecute<BackupCommand>
    {
        private readonly IMainDatabase _maindDb;
        private readonly IMakeDatabaseBackup _makeDatabaseBackup;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IDiskProvider _diskProvider;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IArchiveService _archiveService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        private string _backupTempFolder;

        public static readonly Regex BackupFileRegex = new Regex(@"(nzbdrone|sonarr)_backup_(v[0-9.]+_)?[._0-9]+\.zip", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly byte[] SqliteHeader = Encoding.ASCII.GetBytes("SQLite format 3\0");

        public BackupService(IMainDatabase maindDb,
                             IMakeDatabaseBackup makeDatabaseBackup,
                             IDiskTransferService diskTransferService,
                             IDiskProvider diskProvider,
                             IAppFolderInfo appFolderInfo,
                             IArchiveService archiveService,
                             IConfigService configService,
                             Logger logger)
        {
            _maindDb = maindDb;
            _makeDatabaseBackup = makeDatabaseBackup;
            _diskTransferService = diskTransferService;
            _diskProvider = diskProvider;
            _appFolderInfo = appFolderInfo;
            _archiveService = archiveService;
            _configService = configService;
            _logger = logger;

            _backupTempFolder = Path.Combine(_appFolderInfo.TempFolder, "sonarr_backup");
        }

        public void Backup(BackupType backupType)
        {
            _logger.ProgressInfo("Starting Backup");

            var backupFolder = GetBackupFolder(backupType);

            _diskProvider.EnsureFolder(_backupTempFolder);
            _diskProvider.EnsureFolder(backupFolder);

            if (!_diskProvider.FolderWritable(backupFolder))
            {
                throw new UnauthorizedAccessException($"Backup folder {backupFolder} is not writable");
            }

            var dateNow = DateTime.Now;
            var backupFilename = $"sonarr_backup_v{BuildInfo.Version}_{dateNow:yyyy.MM.dd_HH.mm.ss}.zip";
            var backupPath = Path.Combine(backupFolder, backupFilename);

            Cleanup();

            if (backupType != BackupType.Manual)
            {
                CleanupOldBackups(backupType);
            }

            BackupConfigFile();
            BackupDatabase();
            CreateVersionInfo(dateNow);

            _logger.ProgressDebug("Creating backup zip");

            // Delete journal file created during database backup
            _diskProvider.DeleteFile(Path.Combine(_backupTempFolder, "sonarr.db-journal"));

            _archiveService.CreateZip(backupPath, _diskProvider.GetFiles(_backupTempFolder, false));

            Cleanup();

            _logger.ProgressDebug("Backup zip created");
        }

        public List<Backup> GetBackups()
        {
            var backups = new List<Backup>();

            foreach (var backupType in Enum.GetValues(typeof(BackupType)).Cast<BackupType>())
            {
                var folder = GetBackupFolder(backupType);

                if (_diskProvider.FolderExists(folder))
                {
                    backups.AddRange(GetBackupFiles(folder).Select(b => new Backup
                                                                        {
                                                                            Name = Path.GetFileName(b),
                                                                            Type = backupType,
                                                                            Size = _diskProvider.GetFileSize(b),
                                                                            Time = _diskProvider.FileGetLastWrite(b)
                                                                        }));
                }
            }

            return backups;
        }

        public void Restore(string backupFileName)
        {
            if (backupFileName.EndsWith(".zip"))
            {
                var temporaryPath = Path.Combine(_appFolderInfo.TempFolder, "sonarr_backup_restore");

                DeleteRestoreFolder(temporaryPath);

                try
                {
                    _archiveService.Extract(backupFileName, temporaryPath);

                    var files = _diskProvider.GetFiles(temporaryPath, false).ToList();
                    var configFiles = files.Where(IsConfigFile).ToList();
                    var databaseFiles = files.Where(IsDatabaseFile).ToList();

                    if (configFiles.Count == 0 && databaseFiles.Count == 0)
                    {
                        throw new RestoreBackupFailedException(HttpStatusCode.NotFound, "Unable to restore database file from backup");
                    }

                    configFiles.ForEach(ValidateConfigFile);
                    databaseFiles.ForEach(ValidateDatabase);

                    configFiles.ForEach(RestoreConfigFile);
                    databaseFiles.ForEach(RestoreDatabase);
                }
                finally
                {
                    DeleteRestoreFolder(temporaryPath);
                }

                return;
            }

            if (backupFileName.EndsWith(".xml"))
            {
                ValidateConfigFile(backupFileName);
                RestoreConfigFile(backupFileName);

                return;
            }

            ValidateDatabase(backupFileName);
            RestoreDatabase(backupFileName);
        }

        public string GetBackupFolder()
        {
            var backupFolder = _configService.BackupFolder;

            if (Path.IsPathRooted(backupFolder))
            {
                return backupFolder;
            }

            return Path.Combine(_appFolderInfo.GetAppDataPath(), backupFolder);
        }

        public string GetBackupFolder(BackupType backupType)
        {
            return Path.Combine(GetBackupFolder(), backupType.ToString().ToLower());
        }

        private void Cleanup()
        {
            if (_diskProvider.FolderExists(_backupTempFolder))
            {
                _diskProvider.EmptyFolder(_backupTempFolder);
            }
        }

        private void BackupDatabase()
        {
            if (_maindDb.DatabaseType == DatabaseType.SQLite)
            {
                _logger.ProgressDebug("Backing up database");

                _makeDatabaseBackup.BackupDatabase(_maindDb, _backupTempFolder);
            }
        }

        private void BackupConfigFile()
        {
            _logger.ProgressDebug("Backing up config.xml");

            var configFile = _appFolderInfo.GetConfigPath();
            var tempConfigFile = Path.Combine(_backupTempFolder, Path.GetFileName(configFile));

            _diskTransferService.TransferFile(configFile, tempConfigFile, TransferMode.Copy);
        }

        private void RestoreConfigFile(string file)
        {
            _diskProvider.MoveFile(file, _appFolderInfo.GetConfigPath(), true);
        }

        private void RestoreDatabase(string file)
        {
            _diskProvider.MoveFile(file, _appFolderInfo.GetDatabaseRestore(), true);
        }

        private void DeleteRestoreFolder(string path)
        {
            if (_diskProvider.FolderExists(path))
            {
                _diskProvider.DeleteFolder(path, true);
            }
        }

        private static bool IsConfigFile(string file)
        {
            return Path.GetFileName(file).Equals("Config.xml", StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsDatabaseFile(string file)
        {
            var fileName = Path.GetFileName(file);

            return fileName.Equals("nzbdrone.db", StringComparison.InvariantCultureIgnoreCase) ||
                   fileName.Equals("sonarr.db", StringComparison.InvariantCultureIgnoreCase);
        }

        private void ValidateDatabase(string file)
        {
            if (!IsSqliteDatabase(file))
            {
                throw new RestoreBackupFailedException(HttpStatusCode.BadRequest, "Backup database is not a valid SQLite database");
            }
        }

        private void ValidateConfigFile(string file)
        {
            if (!IsValidConfigFile(file))
            {
                throw new RestoreBackupFailedException(HttpStatusCode.BadRequest, "Backup config file is not a valid config file");
            }
        }

        private bool IsValidConfigFile(string file)
        {
            try
            {
                using (var stream = _diskProvider.OpenReadStream(file))
                {
                    return XDocument.Load(stream).Descendants(ConfigFileProvider.CONFIG_ELEMENT_NAME).Count() == 1;
                }
            }
            catch (XmlException)
            {
                return false;
            }
        }

        private bool IsSqliteDatabase(string file)
        {
            var header = new byte[SqliteHeader.Length];

            using (var stream = _diskProvider.OpenReadStream(file))
            {
                return stream.Read(header, 0, header.Length) == header.Length && header.SequenceEqual(SqliteHeader);
            }
        }

        private void CreateVersionInfo(DateTime dateNow)
        {
            var tempFile = Path.Combine(_backupTempFolder, "INFO");

            var builder = new StringBuilder();
            builder.AppendLine($"v{BuildInfo.Version}");
            builder.AppendLine($"{dateNow:yyyy-MM-dd HH:mm:ss}");

            _diskProvider.WriteAllText(tempFile, builder.ToString());
        }

        private void CleanupOldBackups(BackupType backupType)
        {
            var retention = _configService.BackupRetention;

            _logger.Debug("Cleaning up backup files older than {0} days", retention);
            var files = GetBackupFiles(GetBackupFolder(backupType));

            foreach (var file in files)
            {
                var lastWriteTime = _diskProvider.FileGetLastWrite(file);

                if (lastWriteTime.AddDays(retention) < DateTime.UtcNow)
                {
                    _logger.Debug("Deleting old backup file: {0}", file);
                    _diskProvider.DeleteFile(file);
                }
            }

            _logger.Debug("Finished cleaning up old backup files");
        }

        private IEnumerable<string> GetBackupFiles(string path)
        {
            var files = _diskProvider.GetFiles(path, false);

            return files.Where(f => BackupFileRegex.IsMatch(f));
        }

        public void Execute(BackupCommand message)
        {
            Backup(message.Type);
        }
    }
}
