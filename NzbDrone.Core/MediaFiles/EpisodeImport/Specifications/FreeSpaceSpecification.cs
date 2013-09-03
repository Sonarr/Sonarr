using System;
using System.IO;
using NLog;
using NzbDrone.Common;
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
                var path = Directory.GetParent(localEpisode.Series.Path);
                var freeSpace = _diskProvider.GetAvailableSpace(path.FullName);

                if (freeSpace < localEpisode.Size + 100.Megabytes())
                {
                    _logger.Warn("Not enough free space to import: {0}", localEpisode);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to check free disk space while importing: " + localEpisode.Path, ex);
                throw;
            }
        }
    }
}
