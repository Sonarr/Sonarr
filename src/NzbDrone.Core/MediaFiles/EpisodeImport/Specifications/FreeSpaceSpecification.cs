using System;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class FreeSpaceSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public FreeSpaceSpecification(IDiskProvider diskProvider,  Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public string RejectionReason { get { return "Not enough free space"; } }

        public bool IsSatisfiedBy(LocalEpisode localEpisode)
        {
            try
            {
                if (localEpisode.ExistingFile)
                {
                    _logger.Trace("Skipping free space check for existing episode");
                    return true;
                }

                var path = Directory.GetParent(localEpisode.Series.Path);
                var freeSpace = _diskProvider.GetAvailableSpace(path.FullName);

                if (!freeSpace.HasValue)
                {
                    _logger.Trace("Free space check returned an invalid result for: {0}", path);
                    return true;
                }

                if (freeSpace < localEpisode.Size + 100.Megabytes())
                {
                    _logger.Warn("Not enough free space ({0}) to import: {1} ({2})", freeSpace, localEpisode, localEpisode.Size);
                    return false;
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.Error("Unable to check free disk space while importing. " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to check free disk space while importing: " + localEpisode.Path, ex);
            }

            return true;
        }
    }
}
