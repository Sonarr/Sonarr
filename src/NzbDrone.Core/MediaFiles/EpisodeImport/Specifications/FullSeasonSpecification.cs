using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class FullSeasonSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public FullSeasonSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.FileEpisodeInfo == null)
            {
                return Decision.Accept();
            }

            if (localEpisode.FileEpisodeInfo.FullSeason)
            {
                _logger.Debug("Single episode file detected as containing all episodes in the season due to no episode parsed from the file name.");
                return Decision.Reject("Single episode file contains all episodes in seasons. Review file name or manually import");
            }

            return Decision.Accept();
        }
    }
}
