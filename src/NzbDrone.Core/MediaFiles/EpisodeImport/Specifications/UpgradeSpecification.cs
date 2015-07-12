using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;
using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class UpgradeSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public UpgradeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var qualityComparer = new QualityModelComparer(localEpisode.Series.Profile);
            var languageComparer = new LanguageComparer(localEpisode.Series.LanguageProfile);
            var profile = localEpisode.Series.Profile.Value;
            
            if (localEpisode.Episodes.Any(e => e.EpisodeFileId != 0 && qualityComparer.Compare(e.EpisodeFile.Value.Quality, localEpisode.Quality) > 0))
            {
                _logger.Debug("This file isn't a quality upgrade for all episodes. Skipping {0}", localEpisode.Path);
                return Decision.Reject("Not an upgrade for existing episode file(s)");
            }
            
            if (localEpisode.Episodes.Any(e => e.EpisodeFileId != 0 &&
                                          languageComparer.Compare(e.EpisodeFile.Value.Language, localEpisode.Language) > 0 &&
                                          qualityComparer.Compare(e.EpisodeFile.Value.Quality, localEpisode.Quality) == 0))
            {
                    _logger.Debug("This file isn't a language upgrade for all episodes. Skipping {0}", localEpisode.Path);
                    return Decision.Reject("Not an upgrade for existing episode file(s)");
            }

            return Decision.Accept();
        }
    }
}
