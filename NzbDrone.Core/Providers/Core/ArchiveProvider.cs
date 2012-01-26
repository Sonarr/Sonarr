using System;
using System.IO;
using System.Linq;
using Ionic.Zip;
using NLog;
using Ninject;
using NzbDrone.Common;

namespace NzbDrone.Core.Providers.Core
{
    public class ArchiveProvider
    {
        private readonly EnviromentProvider _enviromentProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      
        [Inject]
        public ArchiveProvider(EnviromentProvider enviromentProvider)
        {
            _enviromentProvider = enviromentProvider;
        }

        public ArchiveProvider()
        {
            
        }

        public virtual void ExtractArchive(string compressedFile, string destination)
        {
            logger.Trace("Extracting archive [{0}] to [{1}]", compressedFile, destination);

            using (ZipFile zipFile = ZipFile.Read(compressedFile))
            {
                zipFile.ExtractAll(destination);
            }

            logger.Trace("Extraction complete.");
        }

        public virtual FileInfo CreateBackupZip()
        {
            try
            {
                var dbFile = PathExtentions.GetNzbDronoeDbFile(_enviromentProvider);
                var configFile = PathExtentions.GetConfigPath(_enviromentProvider);
                var zipFile = Path.Combine(PathExtentions.GetAppDataPath(_enviromentProvider), "NzbDrone_Backup.zip");

                using (var zip = new ZipFile())
                {
                    zip.AddFile(dbFile, String.Empty);
                    zip.AddFile(configFile, String.Empty);
                    zip.Save(zipFile);
                }

                return new FileInfo(zipFile);
            }
            catch(Exception ex)
            {
                logger.WarnException("Failed to create backup zip", ex);
                return null;
            }
        }
    }
}