using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;
namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class UnverifiedSceneNumberingSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public UnverifiedSceneNumberingSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode)
        {
            if (localEpisode.Episodes.Any(v => v.UnverifiedSceneNumbering))
            {
                _logger.Debug("This file uses unverified scene numbers, will not auto-import until numbering is confirmed on TheXEM. Skipping {0}", localEpisode.Path);
                return Decision.Reject("This show has individual episode mappings on TheXEM but the mapping for this episode has not been confirmed yet by their administrators. TheXEM needs manual input.");
            }

            return Decision.Accept();
        }
    }
}
