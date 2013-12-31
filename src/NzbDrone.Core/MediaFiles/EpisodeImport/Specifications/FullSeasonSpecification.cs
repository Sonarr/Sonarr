using System;
using System.IO;
using NLog;
using NzbDrone.Common;
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

        public string RejectionReason { get { return "Full season file"; } }

        public bool IsSatisfiedBy(LocalEpisode localEpisode)
        {
            if (localEpisode.ParsedEpisodeInfo.FullSeason)
            {
                _logger.Trace("Single episode file detected as containing all episodes in the season");
                return false;
            }

            return true;
        }
    }
}
