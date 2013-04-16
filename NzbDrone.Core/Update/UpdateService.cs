using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Update
{
    public interface IUpdateService
    {
        void InstallAvailableUpdate();
        Dictionary<DateTime, string> GetUpdateLogFiles();
    }
}

public class UpdateService : IUpdateService
{
    private readonly IUpdatePackageProvider _updatePackageProvider;
    private readonly EnvironmentProvider _environmentProvider;

    private readonly DiskProvider _diskProvider;
    private readonly IHttpProvider _httpProvider;
    private readonly ConfigFileProvider _configFileProvider;
    private readonly ArchiveProvider _archiveProvider;
    private readonly ProcessProvider _processProvider;
    private readonly Logger _logger;


    public UpdateService(IUpdatePackageProvider updatePackageProvider, EnvironmentProvider environmentProvider, DiskProvider diskProvider,
        IHttpProvider httpProvider, ConfigFileProvider configFileProvider, ArchiveProvider archiveProvider, ProcessProvider processProvider, Logger logger)
    {
        _updatePackageProvider = updatePackageProvider;
        _environmentProvider = environmentProvider;
        _diskProvider = diskProvider;
        _httpProvider = httpProvider;
        _configFileProvider = configFileProvider;
        _archiveProvider = archiveProvider;
        _processProvider = processProvider;
        _logger = logger;
    }

    public void InstallAvailableUpdate()
    {
        var latestAvailable = _updatePackageProvider.GetLatestUpdate();

        if (latestAvailable == null || latestAvailable.Version <= _environmentProvider.Version)
        {
            _logger.Debug("No update available.");
            return;
        }

        InstallUpdate(latestAvailable);

    }

    private void InstallUpdate(UpdatePackage updatePackage)
    {
        var packageDestination = Path.Combine(_environmentProvider.GetUpdateSandboxFolder(), updatePackage.FileName);

        if (_diskProvider.FolderExists(_environmentProvider.GetUpdateSandboxFolder()))
        {
            _logger.Info("Deleting old update files");
            _diskProvider.DeleteFolder(_environmentProvider.GetUpdateSandboxFolder(), true);
        }

        _logger.Info("Downloading update package from [{0}] to [{1}]", updatePackage.Url, packageDestination);
        _httpProvider.DownloadFile(updatePackage.Url, packageDestination);
        _logger.Info("Download completed for update package from [{0}]", updatePackage.FileName);

        _logger.Info("Extracting Update package");
        _archiveProvider.ExtractArchive(packageDestination, _environmentProvider.GetUpdateSandboxFolder());
        _logger.Info("Update package extracted successfully");

        _logger.Info("Preparing client");
        _diskProvider.MoveDirectory(_environmentProvider.GetUpdateClientFolder(), _environmentProvider.GetUpdateSandboxFolder());


        _logger.Info("Starting update client");
        var startInfo = new ProcessStartInfo
        {
            FileName = _environmentProvider.GetUpdateClientExePath(),
            Arguments = string.Format("{0} {1}", _processProvider.GetCurrentProcess().Id, _configFileProvider.Guid)
        };

        var process = _processProvider.Start(startInfo);

        _processProvider.WaitForExit(process);
    }

    public Dictionary<DateTime, string> GetUpdateLogFiles()
    {
        var list = new Dictionary<DateTime, string>();

        if (_diskProvider.FolderExists(_environmentProvider.GetUpdateLogFolder()))
        {
            var provider = CultureInfo.InvariantCulture;
            var files = _diskProvider.GetFiles(_environmentProvider.GetUpdateLogFolder(), SearchOption.TopDirectoryOnly).ToList();

            foreach (var file in files.Select(c => new FileInfo(c)).OrderByDescending(c => c.Name))
            {
                list.Add(DateTime.ParseExact(file.Name.Replace(file.Extension, string.Empty), "yyyy.MM.dd-H-mm", provider), file.FullName);
            }
        }

        return list;
    }
}

