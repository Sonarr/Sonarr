using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Parser.Model;

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
            if (localEpisode.Episodes.Any(e => e.EpisodeFileId != 0 && e.EpisodeFile.Value.Quality > localEpisode.Quality))
            {
                _logger.Trace("This file isn't an upgrade for all episodes. Skipping {0}", localEpisode.Path);
                return false;
            }

            return true;
        }
    }
}
