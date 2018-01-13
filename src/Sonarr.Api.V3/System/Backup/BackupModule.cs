using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Backup;
using Sonarr.Http;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.System.Backup
{
    public class BackupModule : SonarrRestModule<BackupResource>
    {
        private readonly IBackupService _backupService;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        private static readonly List<string> ValidExtensions = new List<string> { ".zip", ".db", ".xml" };

        public BackupModule(IBackupService backupService,
                            IAppFolderInfo appFolderInfo,
                            IDiskProvider diskProvider)
            : base("system/backup")
        {
            _backupService = backupService;
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
            GetResourceAll = GetBackupFiles;
            DeleteResource = DeleteBackup;

            Post[@"/restore/(?<id>[\d]{1,10})"] = x => Restore((int)x.Id);
            Post["/restore/upload"] = x => UploadAndRestore();
        }

        public List<BackupResource> GetBackupFiles()
        {
            var backups = _backupService.GetBackups();

            return backups.Select(b => new BackupResource
                                       {
                                           Id = GetBackupId(b),
                                           Name = b.Name,
                                           Path = $"/backup/{b.Type.ToString().ToLower()}/{b.Name}",
                                           Type = b.Type,
                                           Time = b.Time
                                       })
                                       .OrderByDescending(b => b.Time)
                                       .ToList();
        }

        private void DeleteBackup(int id)
        {
            var backup = GetBackup(id);
            var path = GetBackupPath(backup);

            if (!_diskProvider.FileExists(path))
            {
                throw new NotFoundException();
            }

            _diskProvider.DeleteFile(path);
        }

        public Response Restore(int id)
        {
            var backup = GetBackup(id);

            if (backup == null)
            {
                throw new NotFoundException();
            }

            var path = GetBackupPath(backup);

            _backupService.Restore(path);

            return new
                   {
                       RestartRequired = true
                   }.AsResponse();
        }

        public Response UploadAndRestore()
        {
            var files = Context.Request.Files.ToList();

            if (files.Empty())
            {
                throw new BadRequestException("file must be provided");
            }

            var file = files.First();
            var extension = Path.GetExtension(file.Name);

            if (!ValidExtensions.Contains(extension))
            {
                throw new UnsupportedMediaTypeException($"Invalid extension, must be one of: {ValidExtensions.Join(", ")}");
            }

            var path = Path.Combine(_appFolderInfo.TempFolder, $"sonarr_backup_restore{extension}");

            _diskProvider.SaveStream(file.Value, path);
            _backupService.Restore(path);

            // Cleanup restored file
            _diskProvider.DeleteFile(path);

            return new
                   {
                       RestartRequired = true
                   }.AsResponse();
        }

        private string GetBackupPath(NzbDrone.Core.Backup.Backup backup)
        {
            return Path.Combine(_backupService.GetBackupFolder(), backup.Type.ToString(), backup.Name);
        }

        private int GetBackupId(NzbDrone.Core.Backup.Backup backup)
        {
            return HashConverter.GetHashInt31($"backup-{backup.Type}-{backup.Name}");
        }

        private NzbDrone.Core.Backup.Backup GetBackup(int id)
        {
            return _backupService.GetBackups().SingleOrDefault(b => id == GetBackupId(b));
        }
    }
}
