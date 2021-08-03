using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class EpisodeTitleSpecification : IImportDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly IBuildFileNames _buildFileNames;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public EpisodeTitleSpecification(IConfigService configService,
                                         IBuildFileNames buildFileNames,
                                         IEpisodeService episodeService,
                                         Logger logger)
        {
            _configService = configService;
            _buildFileNames = buildFileNames;
            _episodeService = episodeService;
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.ExistingFile)
            {
                _logger.Debug("{0} is in series folder, skipping check", localEpisode.Path);
                return Decision.Accept();
            }

            var episodeTitleRequired = _configService.EpisodeTitleRequired;

            if (episodeTitleRequired == EpisodeTitleRequiredType.Never)
            {
                _logger.Debug("Episode titles are never required, skipping check");
                return Decision.Accept();
            }

            if (!_buildFileNames.RequiresEpisodeTitle(localEpisode.Series, localEpisode.Episodes))
            {
                _logger.Debug("File name format does not require episode title, skipping check");
                return Decision.Accept();
            }

            var episodes = localEpisode.Episodes;
            var firstEpisode = episodes.First();
            var episodesInSeason = _episodeService.GetEpisodesBySeason(firstEpisode.SeriesId, firstEpisode.EpisodeNumber);
            var allEpisodesOnTheSameDay = firstEpisode.AirDateUtc.HasValue && episodes.All(e =>
                                              e.AirDateUtc.HasValue &&
                                              e.AirDateUtc.Value == firstEpisode.AirDateUtc.Value);

            if (episodeTitleRequired == EpisodeTitleRequiredType.BulkSeasonReleases &&
                allEpisodesOnTheSameDay &&
                episodesInSeason.Count(e => e.AirDateUtc.HasValue &&
                                            e.AirDateUtc.Value == firstEpisode.AirDateUtc.Value) < 4)
            {
                _logger.Debug("Episode title only required for bulk season releases");
                return Decision.Accept();
            }

            foreach (var episode in episodes)
            {
                var airDateUtc = episode.AirDateUtc;
                var title = episode.Title;

                if (airDateUtc.HasValue && airDateUtc.Value.Before(DateTime.UtcNow.AddDays(-1)))
                {
                    _logger.Debug("Episode aired more than 1 day ago");
                    continue;
                }

                if (title.IsNullOrWhiteSpace())
                {
                    _logger.Debug("Episode does not have a title and recently aired");

                    return Decision.Reject("Episode does not have a title and recently aired");
                }

                if (title.Equals("TBA"))
                {
                    _logger.Debug("Episode has a TBA title and recently aired");

                    return Decision.Reject("Episode has a TBA title and recently aired");
                }
            }

            return Decision.Accept();
        }
    }
}
