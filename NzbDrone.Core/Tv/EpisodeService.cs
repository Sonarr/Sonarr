using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using TvdbLib.Data;

namespace NzbDrone.Core.Tv
{
    public interface IEpisodeService
    {
        void AddEpisode(Episode episode);
        Episode GetEpisode(int id);
        Episode GetEpisode(int seriesId, int seasonNumber, int episodeNumber);
        Episode GetEpisode(int seriesId, DateTime date);
        IList<Episode> GetEpisodeBySeries(int seriesId);
        IList<Episode> GetEpisodesBySeason(int seriesId, int seasonNumber);
        void MarkEpisodeAsFetched(int episodeId);
        IList<Episode> GetEpisodesByParseResult(EpisodeParseResult parseResult);
        IList<Episode> EpisodesWithoutFiles(bool includeSpecials);
        IList<Episode> GetEpisodesByFileId(int episodeFileId);
        IList<Episode> EpisodesWithFiles();
        void RefreshEpisodeInfo(Series series);
        void UpdateEpisode(Episode episode);
        IList<int> GetEpisodeNumbersBySeason(int seriesId, int seasonNumber);
        void SetEpisodeIgnore(int episodeId, bool isIgnored);
        bool IsFirstOrLastEpisodeOfSeason(int seriesId, int seasonNumber, int episodeNumber);
        void DeleteEpisodesNotInTvdb(Series series, IList<TvdbEpisode> tvdbEpisodes);
        void SetPostDownloadStatus(List<int> episodeIds, PostDownloadStatusType postDownloadStatus);
        void UpdateEpisodes(List<Episode> episodes);
        Episode GetEpisodeBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber);
    }

    public class EpisodeService : IEpisodeService
    {

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly TvDbProvider _tvDbProvider;
        private readonly ISeasonRepository _seasonRepository;
        private readonly EpisodeRepository _episodeRepository;

        public EpisodeService(TvDbProvider tvDbProviderProvider, ISeasonRepository seasonRepository, EpisodeRepository episodeRepository)
        {
            _tvDbProvider = tvDbProviderProvider;
            _seasonRepository = seasonRepository;
            _episodeRepository = episodeRepository;
        }

        public void AddEpisode(Episode episode)
        {
            episode.Ignored = _seasonRepository.IsIgnored(episode.SeriesId, episode.SeasonNumber);
            _episodeRepository.Insert(episode);
        }

        public virtual Episode GetEpisode(int id)
        {
            return _episodeRepository.Get(id);
        }

        public virtual Episode GetEpisode(int seriesId, int seasonNumber, int episodeNumber)
        {
            return _episodeRepository.Get(seriesId, seasonNumber, episodeNumber);
        }

        public virtual Episode GetEpisode(int seriesId, DateTime date)
        {
            return _episodeRepository.Get(seriesId, date);
        }

        public virtual IList<Episode> GetEpisodeBySeries(int seriesId)
        {
            return _episodeRepository.GetEpisodes(seriesId);
        }

        public virtual IList<Episode> GetEpisodesBySeason(int seriesId, int seasonNumber)
        {
            return _episodeRepository.GetEpisodes(seriesId, seasonNumber);
        }

        public virtual void MarkEpisodeAsFetched(int episodeId)
        {
            logger.Trace("Marking episode {0} as fetched.", episodeId);
            var episode = _episodeRepository.Get(episodeId);
            episode.GrabDate = DateTime.UtcNow;
            _episodeRepository.Update(episode);
        }

        public virtual IList<Episode> GetEpisodesByParseResult(EpisodeParseResult parseResult)
        {
            var result = new List<Episode>();

            if (parseResult.AirDate.HasValue)
            {
                if (parseResult.Series.SeriesType == SeriesType.Standard)
                {
                    //Todo: Collect this as a Series we want to treat as a daily series, or possible parsing error
                    logger.Warn("Found daily-style episode for non-daily series: {0}. {1}", parseResult.Series.Title, parseResult.OriginalString);
                    return new List<Episode>();
                }

                var episodeInfo = GetEpisode(parseResult.Series.SeriesId, parseResult.AirDate.Value);

                if (episodeInfo != null)
                {
                    result.Add(episodeInfo);
                    parseResult.EpisodeTitle = episodeInfo.Title;
                }

                return result;
            }

            if (parseResult.EpisodeNumbers == null)
                return result;

            //Set it to empty before looping through the episode numbers
            parseResult.EpisodeTitle = String.Empty;

            foreach (var episodeNumber in parseResult.EpisodeNumbers)
            {
                Episode episodeInfo = null;

                if (parseResult.SceneSource && parseResult.Series.UseSceneNumbering)
                    episodeInfo = GetEpisodeBySceneNumbering(parseResult.Series.SeriesId, parseResult.SeasonNumber, episodeNumber);

                if (episodeInfo == null)
                {
                    episodeInfo = GetEpisode(parseResult.Series.SeriesId, parseResult.SeasonNumber, episodeNumber);
                    if (episodeInfo == null && parseResult.AirDate != null)
                    {
                        episodeInfo = GetEpisode(parseResult.Series.SeriesId, parseResult.AirDate.Value);
                    }
                }

                if (episodeInfo != null)
                {
                    result.Add(episodeInfo);

                    if (parseResult.Series.UseSceneNumbering)
                    {
                        logger.Info("Using Scene to TVDB Mapping for: {0} - Scene: {1}x{2:00} - TVDB: {3}x{4:00}",
                                    parseResult.Series.Title,
                                    episodeInfo.SceneSeasonNumber,
                                    episodeInfo.SceneEpisodeNumber,
                                    episodeInfo.SeasonNumber,
                                    episodeInfo.EpisodeNumber);
                    }

                    if (parseResult.EpisodeNumbers.Count == 1)
                    {
                        parseResult.EpisodeTitle = episodeInfo.Title.Trim();
                    }
                    else
                    {
                        parseResult.EpisodeTitle = Parser.CleanupEpisodeTitle(episodeInfo.Title);
                    }
                }
                else
                {
                    logger.Debug("Unable to find {0}", parseResult);
                }
            }

            return result;
        }

        public virtual IList<Episode> EpisodesWithoutFiles(bool includeSpecials)
        {
            return _episodeRepository.EpisodesWithoutFiles(includeSpecials);
        }

        public virtual IList<Episode> GetEpisodesByFileId(int episodeFileId)
        {
            return _episodeRepository.GetEpisodeByFileId(episodeFileId);
        }

        public virtual IList<Episode> EpisodesWithFiles()
        {
            return _episodeRepository.EpisodesWithFiles();
        }

        public virtual void RefreshEpisodeInfo(Series series)
        {
            logger.Trace("Starting episode info refresh for series: {0}", series.Title.WithDefault(series.SeriesId));
            var successCount = 0;
            var failCount = 0;

            var tvdbEpisodes = _tvDbProvider.GetSeries(series.SeriesId, true)
                                            .Episodes
                                            .Where(episode => !string.IsNullOrWhiteSpace(episode.EpisodeName) ||
                                                              (episode.FirstAired < DateTime.Now.AddDays(2) && episode.FirstAired.Year > 1900))
                                            .ToList();

            var seriesEpisodes = GetEpisodeBySeries(series.SeriesId);
            var updateList = new List<Episode>();
            var newList = new List<Episode>();

            foreach (var episode in tvdbEpisodes.OrderBy(e => e.SeasonNumber).ThenBy(e => e.EpisodeNumber))
            {
                try
                {
                    logger.Trace("Updating info for [{0}] - S{1:00}E{2:00}", series.Title, episode.SeasonNumber, episode.EpisodeNumber);

                    //first check using tvdbId, this should cover cases when and episode number in a season is changed
                    var episodeToUpdate = seriesEpisodes.SingleOrDefault(e => e.TvDbEpisodeId == episode.Id);

                    //not found, try using season/episode number
                    if (episodeToUpdate == null)
                    {
                        episodeToUpdate = seriesEpisodes.SingleOrDefault(e => e.SeasonNumber == episode.SeasonNumber && e.EpisodeNumber == episode.EpisodeNumber);
                    }

                    if (episodeToUpdate == null)
                    {
                        episodeToUpdate = new Episode();
                        newList.Add(episodeToUpdate);

                        //If it is Episode Zero Ignore it (specials, sneak peeks.)
                        if (episode.EpisodeNumber == 0 && episode.SeasonNumber != 1)
                        {
                            episodeToUpdate.Ignored = true;
                        }
                        else
                        {
                            episodeToUpdate.Ignored = _seasonRepository.IsIgnored(series.SeriesId, episode.SeasonNumber);
                        }
                    }
                    else
                    {
                        updateList.Add(episodeToUpdate);
                    }

                    if ((episodeToUpdate.EpisodeNumber != episode.EpisodeNumber ||
                         episodeToUpdate.SeasonNumber != episode.SeasonNumber) &&
                        episodeToUpdate.EpisodeFileId > 0)
                    {
                        logger.Info("Unlinking episode file because TheTVDB changed the epsiode number...");
                        episodeToUpdate.EpisodeFile = null;
                    }

                    episodeToUpdate.SeriesId = series.SeriesId;
                    episodeToUpdate.TvDbEpisodeId = episode.Id;
                    episodeToUpdate.EpisodeNumber = episode.EpisodeNumber;
                    episodeToUpdate.SeasonNumber = episode.SeasonNumber;
                    episodeToUpdate.AbsoluteEpisodeNumber = episode.AbsoluteNumber;
                    episodeToUpdate.Title = episode.EpisodeName;

                    episodeToUpdate.Overview = episode.Overview.Truncate(3500);

                    if (episode.FirstAired.Year > 1900)
                        episodeToUpdate.AirDate = episode.FirstAired.Date;
                    else
                        episodeToUpdate.AirDate = null;

                    successCount++;
                }
                catch (Exception e)
                {
                    logger.FatalException(String.Format("An error has occurred while updating episode info for series {0}", series.Title), e);
                    failCount++;
                }
            }

            _episodeRepository.InsertMany(newList);
            _episodeRepository.UpdateMany(updateList);

            if (failCount != 0)
            {
                logger.Info("Finished episode refresh for series: {0}. Successful: {1} - Failed: {2} ",
                            series.Title, successCount, failCount);
            }
            else
            {
                logger.Info("Finished episode refresh for series: {0}.", series.Title);
            }

            DeleteEpisodesNotInTvdb(series, tvdbEpisodes);
        }

        public virtual void UpdateEpisode(Episode episode)
        {
            _episodeRepository.Update(episode);
        }


        public virtual IList<int> GetEpisodeNumbersBySeason(int seriesId, int seasonNumber)
        {
            return GetEpisodesBySeason(seriesId, seasonNumber).Select(c => c.OID).ToList();
        }

        public virtual void SetEpisodeIgnore(int episodeId, bool isIgnored)
        {
            var episode = _episodeRepository.Get(episodeId);
            episode.Ignored = isIgnored;
            _episodeRepository.Update(episode);

            logger.Info("Ignore flag for Episode:{0} was set to {1}", episodeId, isIgnored);
        }

        public virtual bool IsFirstOrLastEpisodeOfSeason(int seriesId, int seasonNumber, int episodeNumber)
        {
            var episodes = GetEpisodesBySeason(seriesId, seasonNumber).OrderBy(e => e.EpisodeNumber);

            if (!episodes.Any())
                return false;

            //Ensure that this is either the first episode
            //or is the last episode in a season that has 10 or more episodes
            if (episodes.First().EpisodeNumber == episodeNumber || (episodes.Count() >= 10 && episodes.Last().EpisodeNumber == episodeNumber))
                return true;

            return false;
        }

        public virtual void DeleteEpisodesNotInTvdb(Series series, IList<TvdbEpisode> tvdbEpisodes)
        {
            logger.Trace("Starting deletion of episodes that no longer exist in TVDB: {0}", series.Title.WithDefault(series.SeriesId));

            if (!tvdbEpisodes.Any()) return;

            var seriesEpisodeIds = _episodeRepository.GetEpisodes(series.SeriesId).Select(c => c.OID);

            var toBeDeleted = seriesEpisodeIds.Except(tvdbEpisodes.Select(e => e.Id));

            foreach (var id in toBeDeleted)
            {
                _episodeRepository.Delete(id);
            }

            logger.Trace("Deleted episodes that no longer exist in TVDB for {0}", series.SeriesId);
        }

        public virtual void SetPostDownloadStatus(List<int> episodeIds, PostDownloadStatusType postDownloadStatus)
        {
            if (episodeIds.Count == 0) throw new ArgumentException("episodeIds should contain one or more episode ids.");


            foreach (var episodeId in episodeIds)
            {
                var episode = _episodeRepository.Get(episodeId);
                episode.PostDownloadStatus = postDownloadStatus;
                _episodeRepository.Update(episode);
            }


            logger.Trace("Updating PostDownloadStatus for {0} episode(s) to {1}", episodeIds.Count, postDownloadStatus);
        }

        public virtual void UpdateEpisodes(List<Episode> episodes)
        {
            _episodeRepository.UpdateMany(episodes);
        }

        public virtual Episode GetEpisodeBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber)
        {
            return _episodeRepository.GetEpisodeBySceneNumbering(seriesId, seasonNumber, episodeNumber);
        }
    }
}