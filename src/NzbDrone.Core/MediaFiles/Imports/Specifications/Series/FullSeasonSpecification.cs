using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Imports.Specifications.Series
{
    public class FullSeasonSpecification : BaseImportSeriesEspecification
    {
        private readonly Logger _logger;

        public FullSeasonSpecification(Logger logger)
        {
            _logger = logger;
        }

        public override Decision IsSatisfiedBy(LocalEpisode localEpisode)
        {
            if (localEpisode.ParsedEpisodeInfo != null && localEpisode.ParsedEpisodeInfo.FullSeason)
            {
                _logger.Debug("Single episode file detected as containing all episodes in the season");
                return Decision.Reject("Single episode file contains all episodes in seasons");
            }

            return Decision.Accept();
        }
    }
}
