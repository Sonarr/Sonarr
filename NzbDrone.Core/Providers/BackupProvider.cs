using System;
using System.Linq;
using Ionic.Zip;
using NLog;
using Ninject;
using NzbDrone.Common;

namespace NzbDrone.Core.Providers
{
    public class BackupProvider
    {
        private readonly EnviromentProvider _enviromentProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      
        [Inject]
        public BackupProvider(EnviromentProvider enviromentProvider)
        {
            _enviromentProvider = enviromentProvider;
        }

        public BackupProvider()
        {
            
        }

        public virtual string CreateBackupZip()
        {
            var dbFile = _enviromentProvider.GetNzbDronoeDbFile();
            var configFile = _enviromentProvider.GetConfigPath();
            var zipFile = _enviromentProvider.GetConfigBackupFile();

            using (var zip = new ZipFile())
            {
                zip.AddFile(dbFile, String.Empty);
                zip.AddFile(configFile, String.Empty);
                zip.Save(zipFile);
            }

            return zipFile;
        }
    }
}
