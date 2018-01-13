using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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

        public static readonly Regex BackupFileRegex = new Regex(@"(nzbdrone|sonarr)_backup_[._0-9]+\.zip", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

            _diskProvider.EnsureFolder(_backupTempFolder);
            _diskProvider.EnsureFolder(GetBackupFolder(backupType));

            var backupFilename = string.Format("sonarr_backup_{0:yyyy.MM.dd_HH.mm.ss}.zip", DateTime.Now);
            var backupPath = Path.Combine(GetBackupFolder(backupType), backupFilename);

            Cleanup();

            if (backupType != BackupType.Manual)
            {
                CleanupOldBackups(backupType);
            }

            BackupConfigFile();
            BackupDatabase();
            CreateVersionInfo();

            _logger.ProgressDebug("Creating backup zip");

            // Delete journal file created during database backup
            _diskProvider.DeleteFile(Path.Combine(_backupTempFolder, "sonarr.db-journal"));

            _archiveService.CreateZip(backupPath, _diskProvider.GetFiles(_backupTempFolder, SearchOption.TopDirectoryOnly));

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
                var restoredFile = false;
                var temporaryPath = Path.Combine(_appFolderInfo.TempFolder, "sonarr_backup_restore");

                _archiveService.Extract(backupFileName, temporaryPath);

                foreach (var file in _diskProvider.GetFiles(temporaryPath, SearchOption.TopDirectoryOnly))
                {
                    var fileName = Path.GetFileName(file);

                    if (fileName.Equals("Config.xml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _diskProvider.MoveFile(file, _appFolderInfo.GetConfigPath(), true);
                        restoredFile = true;
                    }

                    if (fileName.Equals("nzbdrone.db", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _diskProvider.MoveFile(file, _appFolderInfo.GetDatabaseRestore(), true);
                        restoredFile = true;
                    }

                    if (fileName.Equals("sonarr.db", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _diskProvider.MoveFile(file, _appFolderInfo.GetDatabaseRestore(), true);
                        restoredFile = true;
                    }
                }

                if (!restoredFile)
                {
                    throw new RestoreBackupFailedException(HttpStatusCode.NotFound, "Unable to restore database file from backup");
                }

                _diskProvider.DeleteFolder(temporaryPath, true);

                return;
            }

            _diskProvider.MoveFile(backupFileName, _appFolderInfo.GetDatabaseRestore(), true);
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

        private string GetBackupFolder(BackupType backupType)
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
            _logger.ProgressDebug("Backing up database");

            _makeDatabaseBackup.BackupDatabase(_maindDb, _backupTempFolder);
        }

        private void BackupConfigFile()
        {
            _logger.ProgressDebug("Backing up config.xml");

            var configFile = _appFolderInfo.GetConfigPath();
            var tempConfigFile = Path.Combine(_backupTempFolder, Path.GetFileName(configFile));

            _diskTransferService.TransferFile(configFile, tempConfigFile, TransferMode.Copy);
        }

        private void CreateVersionInfo()
        {
            var builder = new StringBuilder();

            builder.AppendLine(BuildInfo.Version.ToString());
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
            var files = _diskProvider.GetFiles(path, SearchOption.TopDirectoryOnly);

            return files.Where(f => BackupFileRegex.IsMatch(f));
        }

        public void Execute(BackupCommand message)
        {
            Backup(message.Type);
        }
    }
}
