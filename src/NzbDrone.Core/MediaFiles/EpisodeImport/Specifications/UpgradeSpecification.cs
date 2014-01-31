using System.Linq;
using NLog;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class UpgradeSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public UpgradeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public string RejectionReason { get { return "Is Sample"; } }

        public bool IsSatisfiedBy(LocalEpisode localEpisode)
        {
            var qualityComparer = new QualityModelComparer(localEpisode.Series.QualityProfile);
            if (localEpisode.Episodes.Any(e => e.EpisodeFileId != 0 && qualityComparer.Compare(e.EpisodeFile.Value.Quality, localEpisode.Quality) > 0))
            {
                _logger.Trace("This file isn't an upgrade for all episodes. Skipping {0}", localEpisode.Path);
                return false;
            }

            return true;
        }
    }
}
