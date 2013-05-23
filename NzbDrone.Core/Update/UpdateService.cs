using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Update.Commands;

namespace NzbDrone.Core.Update
{
    public interface IUpdateService : IExecute<ApplicationUpdateCommand>
    {
        Dictionary<DateTime, string> GetUpdateLogFiles();
        UpdatePackage AvailableUpdate();
    }

    public class UpdateService : IUpdateService
    {
        private readonly IUpdatePackageProvider _updatePackageProvider;
        private readonly IEnvironmentProvider _environmentProvider;

        private readonly IDiskProvider _diskProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly ArchiveProvider _archiveProvider;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;


        public UpdateService(IUpdatePackageProvider updatePackageProvider, IEnvironmentProvider environmentProvider,
                             IDiskProvider diskProvider,
                             IHttpProvider httpProvider, IConfigFileProvider configFileProvider,
                             ArchiveProvider archiveProvider, IProcessProvider processProvider, Logger logger)
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


        private void InstallUpdate(UpdatePackage updatePackage)
        {
            var updateSandboxFolder = _environmentProvider.GetUpdateSandboxFolder();

            var packageDestination = Path.Combine(updateSandboxFolder, updatePackage.FileName);

            if (_diskProvider.FolderExists(updateSandboxFolder))
            {
                _logger.Info("Deleting old update files");
                _diskProvider.DeleteFolder(updateSandboxFolder, true);
            }

            _logger.Info("Downloading update package from [{0}] to [{1}]", updatePackage.Url, packageDestination);
            _httpProvider.DownloadFile(updatePackage.Url, packageDestination);
            _logger.Info("Download completed for update package from [{0}]", updatePackage.FileName);

            _logger.Info("Extracting Update package");
            _archiveProvider.ExtractArchive(packageDestination, updateSandboxFolder);
            _logger.Info("Update package extracted successfully");

            _logger.Info("Preparing client");
            _diskProvider.MoveDirectory(_environmentProvider.GetUpdateClientFolder(),
                                        updateSandboxFolder);


            _logger.Info("Starting update client");
            var startInfo = new ProcessStartInfo
                {
                    FileName = _environmentProvider.GetUpdateClientExePath(),
                    Arguments = string.Format("{0} {1}", _processProvider.GetCurrentProcess().Id)
                };

            var process = _processProvider.Start(startInfo);

            _processProvider.WaitForExit(process);

            _logger.Error("Update process failed");
        }

        public Dictionary<DateTime, string> GetUpdateLogFiles()
        {
            var list = new Dictionary<DateTime, string>();

            if (_diskProvider.FolderExists(_environmentProvider.GetUpdateLogFolder()))
            {
                var provider = CultureInfo.InvariantCulture;
                var files =
                    _diskProvider.GetFiles(_environmentProvider.GetUpdateLogFolder(), SearchOption.TopDirectoryOnly)
                                 .ToList();

                foreach (var file in files.Select(c => new FileInfo(c)).OrderByDescending(c => c.Name))
                {
                    list.Add(
                        DateTime.ParseExact(file.Name.Replace(file.Extension, string.Empty), "yyyy.MM.dd-H-mm", provider),
                        file.FullName);
                }
            }

            return list;
        }

        public UpdatePackage AvailableUpdate()
        {
            var latestAvailable = _updatePackageProvider.GetLatestUpdate();

            if (latestAvailable == null || latestAvailable.Version <= _environmentProvider.Version)
            {
                _logger.Debug("No update available.");
                return null;
            }

            return latestAvailable;
        }

        public void Execute(ApplicationUpdateCommand message)
        {
            var latestAvailable = AvailableUpdate();

            if (latestAvailable != null)
            {
                InstallUpdate(latestAvailable);
            }
        }
    }
}