using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class UpgradeSpecification : IImportDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public UpgradeSpecification(IConfigService configService,
                                    ICustomFormatCalculationService formatService,
                                    Logger logger)
        {
            _configService = configService;
            _formatService = formatService;
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var downloadPropersAndRepacks = _configService.DownloadPropersAndRepacks;
            var qualityProfile = localEpisode.Series.QualityProfile.Value;
            var qualityComparer = new QualityModelComparer(qualityProfile);

            foreach (var episode in localEpisode.Episodes.Where(e => e.EpisodeFileId > 0))
            {
                var episodeFile = episode.EpisodeFile.Value;

                if (episodeFile == null)
                {
                    _logger.Trace("Unable to get episode file details from the DB. EpisodeId: {0} EpisodeFileId: {1}", episode.Id, episode.EpisodeFileId);
                    continue;
                }

                var qualityCompare = qualityComparer.Compare(localEpisode.Quality.Quality, episodeFile.Quality.Quality);

                if (qualityCompare < 0)
                {
                    _logger.Debug("This file isn't a quality upgrade for all episodes. Existing quality: {0}. New Quality {1}. Skipping {2}", episodeFile.Quality.Quality, localEpisode.Quality.Quality, localEpisode.Path);
                    return Decision.Reject("Not an upgrade for existing episode file(s). Existing quality: {0}. New Quality {1}.", episodeFile.Quality.Quality, localEpisode.Quality.Quality);
                }

                // Same quality, propers/repacks are preferred and it is not a revision update. Reject revision downgrade.

                if (qualityCompare == 0 &&
                    downloadPropersAndRepacks != ProperDownloadTypes.DoNotPrefer &&
                    localEpisode.Quality.Revision.CompareTo(episodeFile.Quality.Revision) < 0)
                {
                    _logger.Debug("This file isn't a quality revision upgrade for all episodes. Skipping {0}", localEpisode.Path);
                    return Decision.Reject("Not a quality revision upgrade for existing episode file(s)");
                }

                var currentFormats = _formatService.ParseCustomFormat(episodeFile);
                var currentFormatScore = qualityProfile.CalculateCustomFormatScore(currentFormats);
                var newFormats = localEpisode.CustomFormats;
                var newFormatScore = localEpisode.CustomFormatScore;

                if (qualityCompare == 0 && newFormatScore < currentFormatScore)
                {
                    _logger.Debug("New item's custom formats [{0}] ({1}) do not improve on [{2}] ({3}), skipping",
                        newFormats != null ? newFormats.ConcatToString() : "",
                        newFormatScore,
                        currentFormats != null ? currentFormats.ConcatToString() : "",
                        currentFormatScore);

                    return Decision.Reject("Not a Custom Format upgrade for existing episode file(s)");
                }

                _logger.Debug("New item's custom formats [{0}] ({1}) improve on [{2}] ({3}), accepting",
                    newFormats != null ? newFormats.ConcatToString() : "",
                    newFormatScore,
                    currentFormats != null ? currentFormats.ConcatToString() : "",
                    currentFormatScore);
            }

            return Decision.Accept();
        }
    }
}
