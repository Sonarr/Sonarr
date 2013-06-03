using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public interface IEpisodeService
    {
        Episode GetEpisode(int id);
        Episode GetEpisode(int seriesId, int seasonNumber, int episodeNumber, bool useScene = false);
        Episode FindEpisode(int seriesId, int seasonNumber, int episodeNumber, bool useScene = false);
        Episode GetEpisode(int seriesId, DateTime date);
        Episode FindEpisode(int seriesId, DateTime date);
        List<Episode> GetEpisodeBySeries(int seriesId);
        List<Episode> GetEpisodesBySeason(int seriesId, int seasonNumber);
        PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec);
        List<Episode> GetEpisodesByFileId(int episodeFileId);
        List<Episode> EpisodesWithFiles();
        void UpdateEpisode(Episode episode);
        List<int> GetEpisodeNumbersBySeason(int seriesId, int seasonNumber);
        void SetEpisodeIgnore(int episodeId, bool isIgnored);
        bool IsFirstOrLastEpisodeOfSeason(int episodeId);
        void UpdateEpisodes(List<Episode> episodes);
        List<Episode> EpisodesBetweenDates(DateTime start, DateTime end);
        void InsertMany(List<Episode> episodes);
        void UpdateMany(List<Episode> episodes);
    }

    public class EpisodeService : IEpisodeService,
        IHandle<EpisodeFileDeletedEvent>,
        IHandle<EpisodeFileAddedEvent>,
        IHandleAsync<SeriesDeletedEvent>
    {

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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

        public Episode GetEpisode(int seriesId, int seasonNumber, int episodeNumber, bool useSceneNumbering = false)
        {
            if (useSceneNumbering)
            {
                return _episodeRepository.GetEpisodeBySceneNumbering(seriesId, seasonNumber, episodeNumber);
            }
            return _episodeRepository.Find(seriesId, seasonNumber, episodeNumber);
        }

        public Episode FindEpisode(int seriesId, int seasonNumber, int episodeNumber, bool useSceneNumbering = false)
        {
            if (useSceneNumbering)
            {
                return _episodeRepository.FindEpisodeBySceneNumbering(seriesId, seasonNumber, episodeNumber);
            }
            return _episodeRepository.Find(seriesId, seasonNumber, episodeNumber);
        }

        public Episode GetEpisode(int seriesId, DateTime date)
        {
            return _episodeRepository.Get(seriesId, date);
        }

        public Episode FindEpisode(int seriesId, DateTime date)
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

        public PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec)
        {
            var episodeResult = _episodeRepository.EpisodesWithoutFiles(pagingSpec, false);

            return episodeResult;
        }

        public List<Episode> GetEpisodesByFileId(int episodeFileId)
        {
            return _episodeRepository.GetEpisodeByFileId(episodeFileId);
        }

        public List<Episode> EpisodesWithFiles()
        {
            return _episodeRepository.EpisodesWithFiles();
        }



        public void UpdateEpisode(Episode episode)
        {
            _episodeRepository.Update(episode);
        }

        public List<int> GetEpisodeNumbersBySeason(int seriesId, int seasonNumber)
        {
            return GetEpisodesBySeason(seriesId, seasonNumber).Select(c => c.Id).ToList();
        }

        public void SetEpisodeIgnore(int episodeId, bool isIgnored)
        {
            var episode = _episodeRepository.Get(episodeId);
            _episodeRepository.SetIgnoreFlat(episode, isIgnored);

            logger.Info("Ignore flag for Episode:{0} was set to {1}", episodeId, isIgnored);
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
                episode.Ignored = _configService.AutoIgnorePreviouslyDownloadedEpisodes;
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