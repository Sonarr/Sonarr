using System.Collections.Generic;
using System.Linq;
using NLog;
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
        private readonly ICustomFormatCalculationService _customFormatCalculationService;
        private readonly Logger _logger;

        public UpgradeSpecification(IConfigService configService,
                                    ICustomFormatCalculationService customFormatCalculationService,
                                    Logger logger)
        {
            _configService = configService;
            _customFormatCalculationService = customFormatCalculationService;
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var downloadPropersAndRepacks = _configService.DownloadPropersAndRepacks;
            var qualityComparer = new QualityModelComparer(localEpisode.Series.QualityProfile);

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
                    _logger.Debug("This file isn't a quality upgrade for all episodes. New Quality is {0}. Skipping {1}", localEpisode.Quality.Quality, localEpisode.Path);
                    return Decision.Reject("Not an upgrade for existing episode file(s). New Quality is {0}", localEpisode.Quality.Quality);
                }

                // Same quality, is not a language upgrade, propers/repacks are preferred and it is not a revision update
                // This will allow language upgrades of a lower revision to be imported, which are allowed to be grabbed,
                // they just don't import automatically.

                if (qualityCompare == 0 &&
                    downloadPropersAndRepacks != ProperDownloadTypes.DoNotPrefer &&
                    localEpisode.Quality.Revision.CompareTo(episodeFile.Quality.Revision) < 0)
                {
                    _logger.Debug("This file isn't a quality revision upgrade for all episodes. Skipping {0}", localEpisode.Path);
                    return Decision.Reject("Not a quality revision upgrade for existing episode file(s)");
                }
            }

            return Decision.Accept();
        }
    }
}
