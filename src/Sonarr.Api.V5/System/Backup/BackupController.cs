using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Backup;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V5.System.Backup;

[V5ApiController("system/backup")]
public class BackupController : Controller
{
    private readonly IBackupService _backupService;
    private readonly IAppFolderInfo _appFolderInfo;
    private readonly IDiskProvider _diskProvider;

    private static readonly List<string> ValidExtensions = new() { ".zip", ".db", ".xml" };

    public BackupController(IBackupService backupService,
                            IAppFolderInfo appFolderInfo,
                            IDiskProvider diskProvider)
    {
        _backupService = backupService;
        _appFolderInfo = appFolderInfo;
        _diskProvider = diskProvider;
    }

    [HttpGet]
    [Produces("application/json")]
    public ActionResult<List<BackupResource>> GetAll()
    {
        var backups = _backupService.GetBackups();

        var resources = backups.Select(backup => new BackupResource
            {
                Id = GetBackupId(backup),
                Name = backup.Name,
                Path = $"/backup/{backup.Type.ToString().ToLowerInvariant()}/{backup.Name}",
                Size = backup.Size,
                Type = backup.Type,
                Time = backup.Time
            })
            .OrderByDescending(b => b.Time)
            .ToList();

        return resources;
    }

    [RestDeleteById]
    public ActionResult Delete(int id)
    {
        var backup = GetBackupById(id);

        if (backup == null)
        {
            throw new NotFoundException();
        }

        var path = GetBackupPath(backup);

        if (!_diskProvider.FileExists(path))
        {
            throw new NotFoundException();
        }

        _diskProvider.DeleteFile(path);

        return NoContent();
    }

    [HttpPost("restore/{id:int}")]
    [Produces("application/json")]
    public ActionResult<object> Restore([FromRoute] int id)
    {
        var backup = GetBackupById(id);

        if (backup == null)
        {
            return NotFound();
        }

        var path = GetBackupPath(backup);
        _backupService.Restore(path);

        return new { RestartRequired = true };
    }

    [HttpPost("restore/upload")]
    [Produces("application/json")]
    [RequestFormLimits(MultipartBodyLengthLimit = 5000000000)]
    public ActionResult<object> RestoreUpload()
    {
        var files = Request.Form.Files;

        if (files.Count == 0)
        {
            throw new BadRequestException("file must be provided");
        }

        var file = files[0];
        var extension = Path.GetExtension(file.FileName);

        if (!ValidExtensions.Contains(extension))
        {
            return BadRequest(new { error = $"Invalid extension, must be one of: {string.Join(", ", ValidExtensions)}" });
        }

        var path = Path.Combine(_appFolderInfo.TempFolder, $"sonarr_backup_restore{extension}");

        _diskProvider.SaveStream(file.OpenReadStream(), path);
        _backupService.Restore(path);
        _diskProvider.DeleteFile(path);

        return new { RestartRequired = true };
    }

    private string GetBackupPath(NzbDrone.Core.Backup.Backup backup)
    {
        return Path.Combine(_backupService.GetBackupFolder(backup.Type), backup.Name);
    }

    private static int GetBackupId(NzbDrone.Core.Backup.Backup backup)
    {
        return HashConverter.GetHashInt31($"backup-{backup.Type}-{backup.Name}");
    }

    private NzbDrone.Core.Backup.Backup? GetBackupById(int id)
    {
        return _backupService.GetBackups().SingleOrDefault(b => GetBackupId(b) == id);
    }
}
