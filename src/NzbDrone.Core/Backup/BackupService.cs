using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Marr.Data;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Backup
{
    public interface IBackupService
    {
        void Backup(BackupType backupType);
        List<Backup> GetBackups();
    }

    public class BackupService : IBackupService, IExecute<BackupCommand>
    {
        private readonly IDatabase _maindDb;
        private readonly IDiskProvider _diskProvider;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IArchiveService _archiveService;
        private readonly Logger _logger;

        private string _backupTempFolder;

        private static readonly Regex BackupFileRegex = new Regex(@"nzbdrone_backup_[._0-9]+\.zip", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public BackupService(IDatabase maindDb,
                             IDiskProvider diskProvider, 
                             IAppFolderInfo appFolderInfo, 
                             IArchiveService archiveService, 
                             Logger logger)
        {
            _maindDb = maindDb;
            _diskProvider = diskProvider;
            _appFolderInfo = appFolderInfo;
            _archiveService = archiveService;
            _logger = logger;

            _backupTempFolder = Path.Combine(_appFolderInfo.TempFolder, "nzbdrone_backup");
        }

        public void Backup(BackupType backupType)
        {
            _logger.ProgressInfo("Starting Backup");

            _diskProvider.EnsureFolder(_backupTempFolder);
            _diskProvider.EnsureFolder(GetBackupFolder(backupType));

            var backupFilename = String.Format("nzbdrone_backup_{0:yyyy.MM.dd_HH.mm.ss}.zip", DateTime.Now);
            var backupPath = Path.Combine(GetBackupFolder(backupType), backupFilename);

            Cleanup();

            if (backupType != BackupType.Manual)
            {
                CleanupOldBackups(backupType);
            }
            
            BackupConfigFile();
            BackupDatabase();

            _logger.ProgressDebug("Creating backup zip");
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
                                                                            Path = Path.GetFileName(b),
                                                                            Type = backupType,
                                                                            Time = _diskProvider.FileGetLastWriteUtc(b)
                                                                        }));
                }
            }

            return backups;
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

            using (var unitOfWork = new UnitOfWork(() => _maindDb.GetDataMapper()))
            {
                unitOfWork.BeginTransaction();

                var databaseFile = _appFolderInfo.GetNzbDroneDatabase();
                var tempDatabaseFile = Path.Combine(_backupTempFolder, Path.GetFileName(databaseFile));

                _diskProvider.CopyFile(databaseFile, tempDatabaseFile, true);

                unitOfWork.Commit();
            }
        }

        private void BackupConfigFile()
        {
            _logger.ProgressDebug("Backing up config.xml");

            var configFile = _appFolderInfo.GetConfigPath();
            var tempConfigFile = Path.Combine(_backupTempFolder, Path.GetFileName(configFile));

            _diskProvider.CopyFile(configFile, tempConfigFile, true);
        }

        private void CleanupOldBackups(BackupType backupType)
        {
            _logger.Debug("Cleaning up old backup files");
            var files = GetBackupFiles(GetBackupFolder(backupType));

            foreach (var file in files)
            {
                var lastWriteTime = _diskProvider.FileGetLastWriteUtc(file);

                if (lastWriteTime.AddDays(28) < DateTime.UtcNow)
                {
                    _logger.Debug("Deleting old backup file: {0}", file);
                    _diskProvider.DeleteFile(file);
                }
            }

            _logger.Debug("Finished cleaning up old backup files");
        }

        private String GetBackupFolder(BackupType backupType)
        {
            return Path.Combine(_appFolderInfo.GetBackupFolder(), backupType.ToString().ToLower());
        }

        private IEnumerable<String> GetBackupFiles(String path)
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
