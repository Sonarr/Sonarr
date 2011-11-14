using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;


namespace NzbDrone.Core.Providers
{
    public class UpdateProvider
    {
        private readonly HttpProvider _httpProvider;
        private readonly ConfigProvider _configProvider;
        private readonly ConfigFileProvider _configFileProvider;
        private readonly EnviromentProvider _enviromentProvider;
        private readonly ArchiveProvider _archiveProvider;
        private readonly ProcessProvider _processProvider;
        private readonly DiskProvider _diskProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex parseRegex = new Regex(@"(?:\>)(?<filename>NzbDrone.+?(?<version>\d+\.\d+\.\d+\.\d+).+?)(?:\<\/A\>)", RegexOptions.IgnoreCase);



        [Inject]
        public UpdateProvider(HttpProvider httpProvider, ConfigProvider configProvider, ConfigFileProvider configFileProvider,
            EnviromentProvider enviromentProvider, ArchiveProvider archiveProvider, ProcessProvider processProvider, DiskProvider diskProvider)
        {
            _httpProvider = httpProvider;
            _configProvider = configProvider;
            _configFileProvider = configFileProvider;
            _enviromentProvider = enviromentProvider;
            _archiveProvider = archiveProvider;
            _processProvider = processProvider;
            _diskProvider = diskProvider;
        }

        public UpdateProvider()
        {

        }

        private List<UpdatePackage> GetAvailablePackages()
        {
            var updateList = new List<UpdatePackage>();
            var rawUpdateList = _httpProvider.DownloadString(_configProvider.UpdateUrl);
            var matches = parseRegex.Matches(rawUpdateList);

            foreach (Match match in matches)
            {
                var updatePackage = new UpdatePackage();
                updatePackage.FileName = match.Groups["filename"].Value;
                updatePackage.Url = _configProvider.UpdateUrl + updatePackage.FileName;
                updatePackage.Version = new Version(match.Groups["version"].Value);
                updateList.Add(updatePackage);
            }

            return updateList;
        }

        public virtual UpdatePackage GetAvilableUpdate()
        {
            var latestAvailable = GetAvailablePackages().OrderByDescending(c => c.Version).FirstOrDefault();

            if (latestAvailable != null && latestAvailable.Version > _enviromentProvider.Version)
            {
                logger.Debug("An update is available ({0}) => ({1})", _enviromentProvider.Version, latestAvailable.Version);
                return latestAvailable;
            }

            logger.Trace("No updates available");
            return null;
        }

        public virtual void StartUpdate(UpdatePackage updatePackage)
        {
            var packageDestination = Path.Combine(_enviromentProvider.GetUpdateSandboxFolder(), updatePackage.FileName);

            if (_diskProvider.FolderExists(_enviromentProvider.GetUpdateSandboxFolder()))
            {
                logger.Info("Deleting old update files");
                _diskProvider.DeleteFolder(_enviromentProvider.GetUpdateSandboxFolder(), true);
            }

            logger.Info("Downloading update package from [{0}] to [{1}]", updatePackage.Url, packageDestination);
            _httpProvider.DownloadFile(updatePackage.Url, packageDestination);
            logger.Info("Download completed for update package from [{0}]", updatePackage.FileName);

            logger.Info("Extracting Update package");
            _archiveProvider.ExtractArchive(packageDestination, _enviromentProvider.GetUpdateSandboxFolder());
            logger.Info("Update package extracted successfully");

            logger.Info("Preparing client");
            _diskProvider.CopyDirectory(_enviromentProvider.GetUpdateClientFolder(), _enviromentProvider.GetUpdateSandboxFolder());


            logger.Info("Starting update client");
            var startInfo = new ProcessStartInfo()
            {
                FileName = _enviromentProvider.GetUpdateClientExePath(),
                Arguments = string.Format("{0} {1}", _enviromentProvider.NzbDroneProcessIdFromEnviroment, _configFileProvider.Guid)
            };

            _processProvider.Start(startInfo);

        }

    }
}
