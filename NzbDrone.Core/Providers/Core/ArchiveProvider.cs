using System;
using System.IO;
using System.Linq;
using Ionic.Zip;
using NLog;
using NzbDrone.Common;

namespace NzbDrone.Core.Providers.Core
{
    public class ArchiveProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public virtual void ExtractArchive(string compressedFile, string destination)
        {
            logger.Trace("Extracting archive [{0}] to [{1}]", compressedFile, destination);

            using (ZipFile zipFile = ZipFile.Read(compressedFile))
            {
                zipFile.ExtractAll(destination);
            }

            logger.Trace("Extraction complete.");
        }
    }
}