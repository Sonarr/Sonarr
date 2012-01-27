﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            try
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
            catch (Exception ex)
            {
                logger.ErrorException("Failed to create backup zip", ex);
                throw;
            }
        }
    }
}
