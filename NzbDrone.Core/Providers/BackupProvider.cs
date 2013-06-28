using System;
using System.Linq;
using Ionic.Zip;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.Providers
{
    public class BackupProvider
    {
        private readonly IAppDirectoryInfo _appDirectoryInfo;

        public BackupProvider(IAppDirectoryInfo appDirectoryInfo)
        {
            _appDirectoryInfo = appDirectoryInfo;
        }

        public BackupProvider()
        {
        }

        public virtual string CreateBackupZip()
        {
            var configFile = _appDirectoryInfo.GetConfigPath();
            var zipFile = _appDirectoryInfo.GetConfigBackupFile();

            using (var zip = new ZipFile())
            {
                zip.AddFile(configFile, String.Empty);
                zip.Save(zipFile);
            }

            return zipFile;
        }
    }
}
