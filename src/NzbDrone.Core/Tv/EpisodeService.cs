using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Tv
{
    public interface IEpisodeService
    {
        Episode GetEpisode(int id);
        Episode FindEpisode(int seriesId, int seasonNumber, int episodeNumber, bool useScene = false);
        Episode FindEpisode(int seriesId, int absoluteEpisodeNumber);
        Episode FindEpisodeByName(int seriesId, int seasonNumber, string episodeTitle);
        Episode GetEpisode(int seriesId, String date);
        Episode FindEpisode(int seriesId, String date);
        List<Episode> GetEpisodeBySeries(int seriesId);
        List<Episode> GetEpisodesBySeason(int seriesId, int seasonNumber);
        PagingSpec<Episode> GetMissingEpisodes(PagingSpec<Episode> pagingSpec);
        PagingSpec<Episode> GetCutoffUnmetEpisodes(PagingSpec<Episode> pagingSpec);
        List<Episode> GetEpisodesByFileId(int episodeFileId);
        void UpdateEpisode(Episode episode);
        void SetEpisodeMonitored(int episodeId, bool monitored);
        bool IsFirstOrLastEpisodeOfSeason(int episodeId);
        void UpdateEpisodes(List<Episode> episodes);
        List<Episode> EpisodesBetweenDates(DateTime start, DateTime end);
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
        private readonly IQualityProfileRepository _qualityProfileRepository;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public EpisodeService(IEpisodeRepository episodeRepository, IQualityProfileRepository qualityProfileRepository, IConfigService configService, Logger logger)
        {
            _episodeRepository = episodeRepository;
            _qualityProfileRepository = qualityProfileRepository;
            _configService = configService;
            _logger = logger;
        }

        public Episode GetEpisode(int id)
        {
            return _episodeRepository.Get(id);
        }

        public Episode FindEpisode(int seriesId, int seasonNumber, int episodeNumber, bool useSceneNumbering = false)
        {
            if (useSceneNumbering)
            {
                return _episodeRepository.FindEpisodeBySceneNumbering(seriesId, seasonNumber, episodeNumber);
            }
            return _episodeRepository.Find(seriesId, seasonNumber, episodeNumber);
        }

        public Episode FindEpisode(int seriesId, int absoluteEpisodeNumber)
        {
            return _episodeRepository.Find(seriesId, absoluteEpisodeNumber);
        }

        public Episode GetEpisode(int seriesId, String date)
        {
            return _episodeRepository.Get(seriesId, date);
        }

        public Episode FindEpisode(int seriesId, String date)
        {
            return _episodeRepository.Find(seriesId, date);
        }

        public List<Episode> GetEpisodeBySeries(int seriesId)
        {
            return _episodeRepository.GetEpisodes(seriesId).ToList();
        }

        public List<Episode> GetEpisodesBySeason(int seriesId, int seasonNumber)
        {
            return _episodeRepository.GetEpisodes(seriesId, seasonNumber);
        }
        
        public Episode FindEpisodeByName(int seriesId, int seasonNumber, string episodeTitle) 
        {
            // TODO: can replace this search mechanism with something smarter/faster/better
            var search = Parser.Parser.NormalizeEpisodeTitle(episodeTitle);
            return _episodeRepository.GetEpisodes(seriesId, seasonNumber)
                .FirstOrDefault(e => 
                {
                    // normalize episode title
                    string title = Parser.Parser.NormalizeEpisodeTitle(e.Title);
                    // find episode title within search string
                    return (title.Length > 0) && search.Contains(title); 
                });
        }

        public PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec)
        {
            var episodeResult = _episodeRepository.GetMissingEpisodes(pagingSpec, false);

            return episodeResult;
        }

        public PagingSpec<Episode> GetCutoffUnmetEpisodes(PagingSpec<Episode> pagingSpec)
        {
            var allSpec = new PagingSpec<Episode>
            {
                SortKey = pagingSpec.SortKey,
                SortDirection = pagingSpec.SortDirection,
                FilterExpression = pagingSpec.FilterExpression
            };

            var allItems = _episodeRepository.GetCutoffUnmetEpisodes(allSpec, false);

            var qualityProfileComparers = _qualityProfileRepository.All().ToDictionary(v => v.Id, v => new { Profile = v, Comparer = new QualityModelComparer(v) });

            var filtered = allItems.Where(episode =>
            {
                var profile = qualityProfileComparers[episode.Series.QualityProfileId];
                return profile.Comparer.Compare(episode.EpisodeFile.Value.Quality.Quality, profile.Profile.Cutoff) < 0;
            }).ToList();

            pagingSpec.Records = filtered
                        .Skip(pagingSpec.PagingOffset())
                        .Take(pagingSpec.PageSize)
                        .ToList();
            pagingSpec.TotalRecords = filtered.Count;

            return pagingSpec;
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

        public void SetEpisodeMonitoredBySeason(int seriesId, int seasonNumber, bool monitored)
        {
            _episodeRepository.SetMonitoredBySeason(seriesId, seasonNumber, monitored);
        }

        public bool IsFirstOrLastEpisodeOfSeason(int episodeId)
        {
            var episode = GetEpisode(episodeId);
            var seasonEpisodes = GetEpisodesBySeason(episode.SeriesId, episode.SeasonNumber);

            //Ensure that this is either the first episode
            //or is the last episode in a season that has 10 or more episodes
            if (seasonEpisodes.First().EpisodeNumber == episode.EpisodeNumber || (seasonEpisodes.Count() >= 10 && seasonEpisodes.Last().EpisodeNumber == episode.EpisodeNumber))
                return true;

            return false;
        }

        public void UpdateEpisodes(List<Episode> episodes)
        {
            _episodeRepository.UpdateMany(episodes);
        }

        public List<Episode> EpisodesBetweenDates(DateTime start, DateTime end)
        {
            var episodes = _episodeRepository.EpisodesBetweenDates(start.ToUniversalTime(), end.ToUniversalTime());

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
                _logger.Trace("Detaching episode {0} from file.", episode.Id);
                episode.EpisodeFileId = 0;

                if (!message.ForUpgrade && _configService.AutoUnmonitorPreviouslyDownloadedEpisodes)
                {
                    episode.Monitored = false;
                }

                UpdateEpisode(episode);
            }
        }

        public void Handle(EpisodeFileAddedEvent message)
        {
            foreach (var episode in message.EpisodeFile.Episodes.Value)
            {
                _episodeRepository.SetFileId(episode.Id, message.EpisodeFile.Id);
                _logger.Debug("Linking [{0}] > [{1}]", message.EpisodeFile.Path, episode);
            }
        }
    }
}