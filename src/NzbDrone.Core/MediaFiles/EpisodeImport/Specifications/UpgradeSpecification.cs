using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class UpgradeSpecification : IImportDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly IEpisodeFilePreferredWordCalculator _episodeFilePreferredWordCalculator;
        private readonly Logger _logger;

        public UpgradeSpecification(IConfigService configService,
                                    IPreferredWordService preferredWordService,
                                    IEpisodeFilePreferredWordCalculator episodeFilePreferredWordCalculator,
                                    Logger logger)
        {
            _configService = configService;
            _episodeFilePreferredWordCalculator = episodeFilePreferredWordCalculator;
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var downloadPropersAndRepacks = _configService.DownloadPropersAndRepacks;
            var qualityComparer = new QualityModelComparer(localEpisode.Series.QualityProfile);
            var languageComparer = new LanguageComparer(localEpisode.Series.LanguageProfile);
            var preferredWordScore = localEpisode.PreferredWordScore;

            foreach (var episode in localEpisode.Episodes.Where(e => e.EpisodeFileId > 0))
            {
                var episodeFile = episode.EpisodeFile.Value;

                if (episodeFile == null)
                {
                    _logger.Trace("Unable to get episode file details from the DB. EpisodeId: {0} EpisodeFileId: {1}", episode.Id, episode.EpisodeFileId);
                    continue;
                }

                var qualityCompare = qualityComparer.Compare(localEpisode.Quality.Quality, episodeFile.Quality.Quality);
                var languageCompare = languageComparer.Compare(localEpisode.Language, episodeFile.Language);

                if (qualityCompare < 0)
                {
                    _logger.Debug("This file isn't a quality upgrade for all episodes. Skipping {0}", localEpisode.Path);
                    return Decision.Reject("Not an upgrade for existing episode file(s)");
                }

                // Same quality, is not a language upgrade, propers/repacks are preferred and it is not a revision update
                // This will allow language upgrades of a lower revision to be imported, which are allowed to be grabbed,
                // they just don't import automatically.

                if (qualityCompare == 0 &&
                    languageCompare <= 0 &&
                    downloadPropersAndRepacks != ProperDownloadTypes.DoNotPrefer &&
                    localEpisode.Quality.Revision.CompareTo(episodeFile.Quality.Revision) < 0)
                {
                    _logger.Debug("This file isn't a quality revision upgrade for all episodes. Skipping {0}", localEpisode.Path);
                    return Decision.Reject("Not a quality revision upgrade for existing episode file(s)");
                }

                if (languageCompare < 0 && qualityCompare == 0)
                {
                    _logger.Debug("This file isn't a language upgrade for all episodes. Skipping {0}", localEpisode.Path);
                    return Decision.Reject("Not a language upgrade for existing episode file(s)");
                }

                var episodeFilePreferredWordScore = _episodeFilePreferredWordCalculator.Calculate(localEpisode.Series, episodeFile);

                if (qualityCompare == 0 && languageCompare == 0 && preferredWordScore < episodeFilePreferredWordScore)
                {
                    _logger.Debug("This file isn't a preferred word upgrade for all episodes. Skipping {0}", localEpisode.Path);
                    return Decision.Reject("Not a preferred word upgrade for existing episode file(s)");
                }
            }

            return Decision.Accept();
        }
    }
}
