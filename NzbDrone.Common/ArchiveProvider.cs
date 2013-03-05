using System.Linq;
using Ionic.Zip;
using NLog;

namespace NzbDrone.Common
{
    public class ArchiveProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public virtual void ExtractArchive(string compressedFile, string destination)
        {
            logger.Trace("Extracting archive [{0}] to [{1}]", compressedFile, destination);

            using (var zipFile = ZipFile.Read(compressedFile))
            {
                zipFile.ExtractAll(destination);
            }

            logger.Trace("Extraction complete.");
        }
    }
}