using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
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
            var languageComparer = new LanguageComparer(localEpisode.Series.LanguageProfile);

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
                var customFormatScore = GetCustomFormatScore(localEpisode);

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

                var customFormats = _customFormatCalculationService.ParseCustomFormat(episodeFile);
                var episodeFileCustomFormatScore = localEpisode.Series.QualityProfile.Value.CalculateCustomFormatScore(customFormats);

                if (qualityCompare == 0 && customFormatScore < episodeFileCustomFormatScore)
                {
                    _logger.Debug("This file isn't a custom format upgrade for episode. Skipping {0}", localEpisode.Path);
                    return Decision.Reject("Not a custom format upgrade for existing episode file(s)");
                }
            }

            return Decision.Accept();
        }

        private int GetCustomFormatScore(LocalEpisode localEpisode)
        {
            var series = localEpisode.Series;
            var scores = new List<int>();
            var fileFormats = new List<CustomFormat>();
            var folderFormats = new List<CustomFormat>();
            var clientFormats = new List<CustomFormat>();

            if (localEpisode.FileEpisodeInfo != null)
            {
                fileFormats = _customFormatCalculationService.ParseCustomFormat(localEpisode.FileEpisodeInfo);
            }

            if (localEpisode.FolderEpisodeInfo != null)
            {
                folderFormats = _customFormatCalculationService.ParseCustomFormat(localEpisode.FolderEpisodeInfo);
            }

            if (localEpisode.DownloadClientEpisodeInfo != null)
            {
                clientFormats = _customFormatCalculationService.ParseCustomFormat(localEpisode.DownloadClientEpisodeInfo);
            }

            var formats = fileFormats.Union(folderFormats.Union(clientFormats)).ToList();

            return series.QualityProfile.Value.CalculateCustomFormatScore(formats);
        }
    }
}
