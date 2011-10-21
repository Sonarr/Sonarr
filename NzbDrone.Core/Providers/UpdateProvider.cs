using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    class UpdateProvider
    {
        private readonly HttpProvider _httpProvider;
        private readonly ConfigProvider _configProvider;
        private readonly EnviromentProvider _enviromentProvider;
        private readonly DiskProvider _diskProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex ParseRegex = new Regex(@"(?:\>)(?<filename>NzbDrone.+?(?<version>\d+\.\d+\.\d+\.\d+).+?)(?:\<\/A\>)", RegexOptions.IgnoreCase);

        public const string SandboxFolderName = "nzbdrone_update";

        [Inject]
        public UpdateProvider(HttpProvider httpProvider, ConfigProvider configProvider, EnviromentProvider enviromentProvider, DiskProvider diskProvider)
        {
            _httpProvider = httpProvider;
            _configProvider = configProvider;
            _enviromentProvider = enviromentProvider;
            _diskProvider = diskProvider;
        }

        public UpdateProvider()
        {

        }

        private List<UpdatePackage> GetAvailablePackages()
        {
            var updateList = new List<UpdatePackage>();
            var rawUpdateList = _httpProvider.DownloadString(_configProvider.UpdateUrl);
            var matches = ParseRegex.Matches(rawUpdateList);

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
                Logger.Debug("An update is available ({0}) => ({1})", _enviromentProvider.Version, latestAvailable.Version);
                return latestAvailable;
            }

            Logger.Trace("No updates available");
            return null;
        }

        public virtual void PreformUpdate(UpdatePackage updatePackage)
        {
            var tempSubFolder = Path.Combine(_enviromentProvider.TempPath, SandboxFolderName);
            var packageDestination = Path.Combine(tempSubFolder, updatePackage.FileName);

            Logger.Info("Downloading update package from [{0}] to [{1}]", updatePackage.Url, packageDestination);
            _httpProvider.DownloadFile(updatePackage.Url, packageDestination);
            Logger.Info("Download completed for update package from [{0}]", updatePackage.FileName);

            Logger.Info("Extracting Update package");
            _diskProvider.ExtractArchive(packageDestination, tempSubFolder);
            Logger.Info("Update package extracted successfully");
        }

    }
}
