using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public interface IEpisodeService
    {
        Episode GetEpisode(int id);
        List<Episode> GetEpisodes(IEnumerable<int> ids);
        Episode FindEpisode(int seriesId, int seasonNumber, int episodeNumber);
        Episode FindEpisode(int seriesId, int absoluteEpisodeNumber);
        Episode FindEpisodeByTitle(int seriesId, int seasonNumber, string releaseTitle);
        List<Episode> FindEpisodesBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber);
        List<Episode> FindEpisodesBySceneNumbering(int seriesId, int sceneAbsoluteEpisodeNumber);
        Episode FindEpisode(int seriesId, string date, int? part);
        List<Episode> GetEpisodeBySeries(int seriesId);
        List<Episode> GetEpisodesBySeason(int seriesId, int seasonNumber);
        List<Episode> GetEpisodesBySceneSeason(int seriesId, int sceneSeasonNumber);
        List<Episode> EpisodesWithFiles(int seriesId);
        PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec);
        List<Episode> GetEpisodesByFileId(int episodeFileId);
        void UpdateEpisode(Episode episode);
        void SetEpisodeMonitored(int episodeId, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        void UpdateEpisodes(List<Episode> episodes);
        List<Episode> EpisodesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        void InsertMany(List<Episode> episodes);
        void UpdateMany(List<Episode> episodes);
        void DeleteMany(List<Episode> episodes);
        void SetEpisodeMonitoredBySeason(int seriesId, int seasonNumber, bool monitored);
    }

    public class EpisodeService : IEpisodeService,
                                  IHandle<EpisodeFileDeletedEvent>,
                                  IHandle<EpisodeFileAddedEvent>,
                                  IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IEpisodeRepository _episodeRepository;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public EpisodeService(IEpisodeRepository episodeRepository, IConfigService configService, Logger logger)
        {
            _episodeRepository = episodeRepository;
            _configService = configService;
            _logger = logger;
        }

        public Episode GetEpisode(int id)
        {
            return _episodeRepository.Get(id);
        }

        public List<Episode> GetEpisodes(IEnumerable<int> ids)
        {
            return _episodeRepository.Get(ids).ToList();
        }

        public Episode FindEpisode(int seriesId, int seasonNumber, int episodeNumber)
        {
            return _episodeRepository.Find(seriesId, seasonNumber, episodeNumber);
        }

        public Episode FindEpisode(int seriesId, int absoluteEpisodeNumber)
        {
            return _episodeRepository.Find(seriesId, absoluteEpisodeNumber);
        }

        public List<Episode> FindEpisodesBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber)
        {
            return _episodeRepository.FindEpisodesBySceneNumbering(seriesId, seasonNumber, episodeNumber);
        }

        public List<Episode> FindEpisodesBySceneNumbering(int seriesId, int sceneAbsoluteEpisodeNumber)
        {
            return _episodeRepository.FindEpisodesBySceneNumbering(seriesId, sceneAbsoluteEpisodeNumber);
        }

        public Episode FindEpisode(int seriesId, string date, int? part)
        {
            return FindOneByAirDate(seriesId, date, part);
        }

        public List<Episode> GetEpisodeBySeries(int seriesId)
        {
            return _episodeRepository.GetEpisodes(seriesId).ToList();
        }

        public List<Episode> GetEpisodesBySeason(int seriesId, int seasonNumber)
        {
            return _episodeRepository.GetEpisodes(seriesId, seasonNumber);
        }

        public List<Episode> GetEpisodesBySceneSeason(int seriesId, int sceneSeasonNumber)
        {
            return _episodeRepository.GetEpisodesBySceneSeason(seriesId, sceneSeasonNumber);
        }

        public Episode FindEpisodeByTitle(int seriesId, int seasonNumber, string releaseTitle)
        {
            // TODO: can replace this search mechanism with something smarter/faster/better
            var normalizedReleaseTitle = Parser.Parser.NormalizeEpisodeTitle(releaseTitle);
            var cleanNormalizedReleaseTitle = Parser.Parser.CleanSeriesTitle(normalizedReleaseTitle);
            var episodes = _episodeRepository.GetEpisodes(seriesId, seasonNumber);

            var possibleMatches = episodes.SelectMany(
                episode => new[]
                {
                    new
                    {
                        Position = normalizedReleaseTitle.IndexOf(Parser.Parser.NormalizeEpisodeTitle(episode.Title), StringComparison.CurrentCultureIgnoreCase),
                        Length = Parser.Parser.NormalizeEpisodeTitle(episode.Title).Length,
                        Episode = episode
                    },
                    new
                    {
                        Position = cleanNormalizedReleaseTitle.IndexOf(Parser.Parser.CleanSeriesTitle(Parser.Parser.NormalizeEpisodeTitle(episode.Title)), StringComparison.CurrentCultureIgnoreCase),
                        Length = Parser.Parser.NormalizeEpisodeTitle(episode.Title).Length,
                        Episode = episode
                    }
                });

            var matches = possibleMatches
                                .Where(e => e.Episode.Title.Length > 0 && e.Position >= 0)
                                .OrderBy(e => e.Position)
                                .ThenByDescending(e => e.Length)
                                .ToList();

            if (matches.Any())
            {
                return matches.First().Episode;
            }

            return null;
        }

        public List<Episode> EpisodesWithFiles(int seriesId)
        {
            return _episodeRepository.EpisodesWithFiles(seriesId);
        }

        public PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec)
        {
            var episodeResult = _episodeRepository.EpisodesWithoutFiles(pagingSpec, true);

            return episodeResult;
        }

        public List<Episode> GetEpisodesByFileId(int episodeFileId)
        {
            return _episodeRepository.GetEpisodeByFileId(episodeFileId);
        }

        public void UpdateEpisode(Episode episode)
        {
            _episodeRepository.Update(episode);
        }

        public void SetEpisodeMonitored(int episodeId, bool monitored)
        {
            var episode = _episodeRepository.Get(episodeId);
            _episodeRepository.SetMonitoredFlat(episode, monitored);

            _logger.Debug("Monitored flag for Episode:{0} was set to {1}", episodeId, monitored);
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            _episodeRepository.SetMonitored(ids, monitored);
        }

        public void SetEpisodeMonitoredBySeason(int seriesId, int seasonNumber, bool monitored)
        {
            _episodeRepository.SetMonitoredBySeason(seriesId, seasonNumber, monitored);
        }

        public void UpdateEpisodes(List<Episode> episodes)
        {
            _episodeRepository.UpdateMany(episodes);
        }

        public List<Episode> EpisodesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var episodes = _episodeRepository.EpisodesBetweenDates(start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored);

            return episodes;
        }

        public void InsertMany(List<Episode> episodes)
        {
            _episodeRepository.InsertMany(episodes);
        }

        public void UpdateMany(List<Episode> episodes)
        {
            _episodeRepository.UpdateMany(episodes);
        }

        public void DeleteMany(List<Episode> episodes)
        {
            _episodeRepository.DeleteMany(episodes);
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            var episodes = GetEpisodeBySeries(message.Series.Id);
            _episodeRepository.DeleteMany(episodes);
        }

        public void Handle(EpisodeFileDeletedEvent message)
        {
            foreach (var episode in GetEpisodesByFileId(message.EpisodeFile.Id))
            {
                _logger.Debug("Detaching episode {0} from file.", episode.Id);

                var unmonitorForReason = message.Reason != DeleteMediaFileReason.Upgrade &&
                                         message.Reason != DeleteMediaFileReason.ManualOverride;

                _episodeRepository.ClearFileId(episode, unmonitorForReason && _configService.AutoUnmonitorPreviouslyDownloadedEpisodes);
            }
        }

        public void Handle(EpisodeFileAddedEvent message)
        {
            foreach (var episode in message.EpisodeFile.Episodes.Value)
            {
                _episodeRepository.SetFileId(episode, message.EpisodeFile.Id);
                _logger.Debug("Linking [{0}] > [{1}]", message.EpisodeFile.RelativePath, episode);
            }
        }

        private Episode FindOneByAirDate(int seriesId, string date, int? part)
        {
            var episodes = _episodeRepository.Find(seriesId, date);

            if (!episodes.Any())
            {
                return null;
            }

            if (episodes.Count == 1)
            {
                return episodes.First();
            }

            _logger.Debug("Multiple episodes with the same air date were found, will exclude specials");

            var regularEpisodes = episodes.Where(e => e.SeasonNumber > 0).ToList();

            if (regularEpisodes.Count == 1 && !part.HasValue)
            {
                _logger.Debug("Left with one episode after excluding specials");
                return regularEpisodes.First();
            }
            else if (part.HasValue && part.Value <= regularEpisodes.Count)
            {
                var sortedEpisodes = regularEpisodes.OrderBy(e => e.SeasonNumber)
                                                               .ThenBy(e => e.EpisodeNumber)
                                                                .ToList();

                return sortedEpisodes[part.Value - 1];
            }

            throw new InvalidOperationException("Multiple episodes with the same air date found");
        }
    }
}
