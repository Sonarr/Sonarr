using System;
using System.Linq;
using Ionic.Zip;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.Providers
{
    public class BackupProvider
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public BackupProvider(IAppFolderInfo appFolderInfo)
        {
            _appFolderInfo = appFolderInfo;
        }

        public BackupProvider()
        {
        }

        public virtual string CreateBackupZip()
        {
            var configFile = _appFolderInfo.GetConfigPath();
            var zipFile = _appFolderInfo.GetConfigBackupFile();

            using (var zip = new ZipFile())
            {
                zip.AddFile(configFile, String.Empty);
                zip.Save(zipFile);
            }

            return zipFile;
        }
    }
}
