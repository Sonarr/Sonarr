using NLog;
using Workarr.Configuration;
using Workarr.Download;
using Workarr.Extensions;
using Workarr.Organizer;
using Workarr.Parser.Model;
using Workarr.Tv;

namespace Workarr.MediaFiles.EpisodeImport.Specifications
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

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.ExistingFile)
            {
                _logger.Debug((string)"{0} is in series folder, skipping check", (string)localEpisode.Path);
                return ImportSpecDecision.Accept();
            }

            var episodeTitleRequired = _configService.EpisodeTitleRequired;

            if (episodeTitleRequired == EpisodeTitleRequiredType.Never)
            {
                _logger.Debug("Episode titles are never required, skipping check");
                return ImportSpecDecision.Accept();
            }

            if (!_buildFileNames.RequiresEpisodeTitle(localEpisode.Series, localEpisode.Episodes))
            {
                _logger.Debug("File name format does not require episode title, skipping check");
                return ImportSpecDecision.Accept();
            }

            var episodes = localEpisode.Episodes;
            var firstEpisode = episodes.First();
            var episodesInSeason = _episodeService.GetEpisodesBySeason(firstEpisode.SeriesId, firstEpisode.EpisodeNumber);
            var allEpisodesOnTheSameDay = firstEpisode.AirDateUtc.HasValue && episodes.All(e =>
                                              !e.AirDateUtc.HasValue ||
                                              e.AirDateUtc.Value == firstEpisode.AirDateUtc.Value);

            if (episodeTitleRequired == EpisodeTitleRequiredType.BulkSeasonReleases &&
                allEpisodesOnTheSameDay &&
                episodesInSeason.Count(e => !e.AirDateUtc.HasValue ||
                                            e.AirDateUtc.Value == firstEpisode.AirDateUtc.Value) < 4)
            {
                _logger.Debug("Episode title only required for bulk season releases");
                return ImportSpecDecision.Accept();
            }

            foreach (var episode in episodes)
            {
                var airDateUtc = episode.AirDateUtc;
                var title = episode.Title;

                if (airDateUtc.HasValue && DateTimeExtensions.Before(airDateUtc.Value, DateTime.UtcNow.AddHours(-48)))
                {
                    _logger.Debug("Episode aired more than 48 hours ago");
                    continue;
                }

                if (StringExtensions.IsNullOrWhiteSpace(title))
                {
                    _logger.Debug("Episode does not have a title and recently aired");

                    return ImportSpecDecision.Reject(ImportRejectionReason.TitleMissing, "Episode does not have a title and recently aired");
                }

                if (title.Equals("TBA"))
                {
                    _logger.Debug("Episode has a TBA title and recently aired");

                    return ImportSpecDecision.Reject(ImportRejectionReason.TitleTba, "Episode has a TBA title and recently aired");
                }
            }

            return ImportSpecDecision.Accept();
        }
    }
}
