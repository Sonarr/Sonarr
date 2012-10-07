using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using PetaPoco;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public class EpisodeProvider
    {

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        //this will remove (1),(2) from the end of multi part episodes.
        private static readonly Regex multiPartCleanupRegex = new Regex(@"\(\d+\)$", RegexOptions.Compiled);

        private readonly TvDbProvider _tvDbProvider;
        private readonly SeasonProvider _seasonProvider;
        private readonly IDatabase _database;
        private readonly SeriesProvider _seriesProvider;

        [Inject]
        public EpisodeProvider(IDatabase database, SeriesProvider seriesProvider,
            TvDbProvider tvDbProviderProvider, SeasonProvider seasonProvider)
        {
            _tvDbProvider = tvDbProviderProvider;
            _seasonProvider = seasonProvider;
            _database = database;
            _seriesProvider = seriesProvider;
        }

        public EpisodeProvider()
        {
        }

        public virtual void AddEpisode(Episode episode)
        {
            //If Season is ignored ignore this episode
            episode.Ignored = _seasonProvider.IsIgnored(episode.SeriesId, episode.SeasonNumber);

            _database.Insert(episode);
        }

        public virtual Episode GetEpisode(long id)
        {
            var episode = _database.Fetch<Episode, Series, EpisodeFile>(@"SELECT * FROM Episodes
                                                            INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                            LEFT JOIN EpisodeFiles ON Episodes.EpisodeFileId = EpisodeFiles.EpisodeFileId
                                                            WHERE EpisodeId = @0", id).Single();

            if (episode.EpisodeFileId == 0)
                episode.EpisodeFile = null;

            return episode;
        }

        public virtual Episode GetEpisode(int seriesId, int seasonNumber, int episodeNumber)
        {
            var episode = _database.Fetch<Episode, Series, EpisodeFile>(@"SELECT * FROM Episodes 
                                                            INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                            LEFT JOIN EpisodeFiles ON Episodes.EpisodeFileId = EpisodeFiles.EpisodeFileId
                                                            WHERE Episodes.SeriesId = @0 AND Episodes.SeasonNumber = @1 AND Episodes.EpisodeNumber = @2", seriesId, seasonNumber, episodeNumber).SingleOrDefault();

            if (episode == null)
                return null;

            if (episode.EpisodeFileId == 0)
                episode.EpisodeFile = null;

            return episode;
        }

        public virtual Episode GetEpisode(int seriesId, DateTime date)
        {
            var episode = _database.Fetch<Episode, Series, EpisodeFile>(@"SELECT * FROM Episodes
                                                            INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                            LEFT JOIN EpisodeFiles ON Episodes.EpisodeFileId = EpisodeFiles.EpisodeFileId
                                                            WHERE Episodes.SeriesId = @0 AND AirDate = @1", seriesId, date.Date).SingleOrDefault();

            if (episode == null)
                return null;

            if (episode.EpisodeFileId == 0)
                episode.EpisodeFile = null;

            return episode;
        }

        public virtual IList<Episode> GetEpisodeBySeries(long seriesId)
        {
            var episodes = _database.Fetch<Episode, Series, EpisodeFile>(@"SELECT * FROM Episodes
                                                            INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                            LEFT JOIN EpisodeFiles ON Episodes.EpisodeFileId = EpisodeFiles.EpisodeFileId
                                                            WHERE Episodes.SeriesId = @0", seriesId);

            foreach (var episode in episodes)
            {
                if (episode.EpisodeFileId == 0)
                    episode.EpisodeFile = null;
            }

            return episodes;
        }

        public virtual IList<Episode> GetEpisodesBySeason(long seriesId, int seasonNumber)
        {
            var episodes = _database.Fetch<Episode, Series, EpisodeFile>(@"SELECT * FROM Episodes
                                                            INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                            LEFT JOIN EpisodeFiles ON Episodes.EpisodeFileId = EpisodeFiles.EpisodeFileId
                                                            WHERE Episodes.SeriesId = @0 AND Episodes.SeasonNumber = @1", seriesId, seasonNumber);

            foreach (var episode in episodes)
            {
                if (episode.EpisodeFileId == 0)
                    episode.EpisodeFile = null;
            }

            return episodes;
        }

        public virtual void MarkEpisodeAsFetched(int episodeId)
        {
            logger.Trace("Marking episode {0} as fetched.", episodeId);
            _database.Execute("UPDATE Episodes SET GrabDate=@0 WHERE EpisodeId=@1", DateTime.Now, episodeId);
        }

        public virtual IList<Episode> GetEpisodesByParseResult(EpisodeParseResult parseResult)
        {
            //Disabling auto add, need to make it a lot more conservative.
            var autoAddNew = false;

            var result = new List<Episode>();

            if (parseResult.AirDate.HasValue)
            {
                if (!parseResult.Series.IsDaily)
                {
                    //Todo: Collect this as a Series we want to treat as a daily series, or possible parsing error
                    logger.Warn("Found daily-style episode for non-daily series: {0}. {1}", parseResult.Series.Title, parseResult.OriginalString);
                    return new List<Episode>();
                }

                var episodeInfo = GetEpisode(parseResult.Series.SeriesId, parseResult.AirDate.Value);

                if (episodeInfo == null && autoAddNew)
                {
                    logger.Info("Episode {0} doesn't exist in db. adding it now. {1}", parseResult, parseResult.OriginalString);
                    episodeInfo = new Episode
                                      {
                                          SeriesId = parseResult.Series.SeriesId,
                                          AirDate = parseResult.AirDate.Value,
                                          Title = "TBD",
                                          Overview = String.Empty
                                      };

                    var episodesInSeries = GetEpisodeBySeries(parseResult.Series.SeriesId);

                    //Find the current season number
                    var maxSeasonNumber = episodesInSeries.Select(s => s.SeasonNumber).MaxOrDefault();

                    //Set the season number
                    episodeInfo.SeasonNumber = (maxSeasonNumber == 0) ? 1 : maxSeasonNumber;

                    //Find the latest episode number
                    var maxEpisodeNumber = episodesInSeries
                            .Where(w => w.SeasonNumber == episodeInfo.SeasonNumber)
                            .Select(s => s.EpisodeNumber).MaxOrDefault();

                    //Set the episode number to max + 1
                    episodeInfo.EpisodeNumber = maxEpisodeNumber + 1;

                    AddEpisode(episodeInfo);
                }
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
                var episodeInfo = GetEpisode(parseResult.Series.SeriesId, parseResult.SeasonNumber, episodeNumber);
                if (episodeInfo == null && parseResult.AirDate != null)
                {
                    episodeInfo = GetEpisode(parseResult.Series.SeriesId, parseResult.AirDate.Value);
                }
                //if still null we should add the temp episode
                if (episodeInfo == null && autoAddNew)
                {
                    logger.Info("Episode {0} doesn't exist in db. adding it now. {1}", parseResult, parseResult.OriginalString);
                    episodeInfo = new Episode
                    {
                        SeriesId = parseResult.Series.SeriesId,
                        AirDate = DateTime.Now.Date,
                        EpisodeNumber = episodeNumber,
                        SeasonNumber = parseResult.SeasonNumber,
                        Title = "TBD",
                        Overview = String.Empty,
                    };

                    if (parseResult.EpisodeNumbers.Count == 1 && parseResult.EpisodeNumbers.First() == 0)
                        episodeInfo.Ignored = true;

                    AddEpisode(episodeInfo);
                }

                if (episodeInfo != null)
                {
                    result.Add(episodeInfo);

                    if (parseResult.EpisodeNumbers.Count == 1)
                    {
                        parseResult.EpisodeTitle = episodeInfo.Title.Trim();
                    }
                    else
                    {
                        parseResult.EpisodeTitle = multiPartCleanupRegex.Replace(episodeInfo.Title, string.Empty).Trim();
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
            var episodes = _database.Query<Episode, Series>(@"SELECT Episodes.*, Series.* FROM Episodes
                                                        INNER JOIN Series
                                                        ON Episodes.SeriesId = Series.SeriesId
                                                        WHERE (EpisodeFileId=0 OR EpisodeFileId=NULL) AND Ignored = 0 AND AirDate<=@0",
                                                    DateTime.Now.Date);
            if (!includeSpecials)
                return episodes.Where(e => e.SeasonNumber > 0).ToList();

            return episodes.ToList();
        }

        public virtual IList<Episode> GetEpisodesByFileId(int episodeFileId)
        {
            return _database.Fetch<Episode, Series>(@"SELECT * FROM Episodes
                                                        INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                        WHERE EpisodeFileId = @0", episodeFileId);
        }

        public virtual IList<Episode> EpisodesWithFiles()
        {
            var episodes = _database.Fetch<Episode, Series, EpisodeFile>(@"SELECT Episodes.*, Series.*, EpisodeFiles.* FROM Episodes
                                                                    INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                                    INNER JOIN EpisodeFiles ON Episodes.EpisodeFileId = EpisodeFiles.EpisodeFileId");

            return episodes;
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

            _seasonProvider.EnsureSeasons(series.SeriesId, tvdbEpisodes.Select(c => c.SeasonNumber).Distinct());
            
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
                            episodeToUpdate.Ignored = _seasonProvider.IsIgnored(series.SeriesId, episode.SeasonNumber);
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

                        _database.Delete<EpisodeFile>(episodeToUpdate.EpisodeFileId);
                        episodeToUpdate.EpisodeFileId = 0;
                    }

                    episodeToUpdate.SeriesId = series.SeriesId;
                    episodeToUpdate.TvDbEpisodeId = episode.Id;
                    episodeToUpdate.EpisodeNumber = episode.EpisodeNumber;
                    episodeToUpdate.SeasonNumber = episode.SeasonNumber;
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

            _database.InsertMany(newList);
            _database.UpdateMany(updateList);

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
            _database.Update(episode);
        }

        public virtual IList<int> GetSeasons(int seriesId)
        {
            return _database.Fetch<Int32>("SELECT DISTINCT SeasonNumber FROM Episodes WHERE SeriesId=@0", seriesId).OrderBy(c => c).ToList();
        }

        public virtual IList<int> GetEpisodeNumbersBySeason(int seriesId, int seasonNumber)
        {
            return _database.Fetch<int>("SELECT EpisodeNumber FROM Episodes WHERE SeriesId=@0 AND SeasonNumber=@1", seriesId, seasonNumber).OrderBy(c => c).ToList();
        }

        public virtual void SetEpisodeIgnore(int episodeId, bool isIgnored)
        {
            _database.Execute(@"UPDATE Episodes SET Ignored = @0
                                WHERE EpisodeId = @1",
                isIgnored, episodeId);

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

            //Delete Episodes not matching TvDbIds for this series
            var tvDbIds = tvdbEpisodes.Select(e => e.Id);
            var tvDbIdString = String.Join(", ", tvDbIds);

            var tvDbIdQuery = String.Format("DELETE FROM Episodes WHERE SeriesId = {0} AND TvDbEpisodeId > 0 AND TvDbEpisodeId NOT IN ({1})",
                                                                                    series.SeriesId, tvDbIdString);

            _database.Execute(tvDbIdQuery);

            logger.Trace("Deleted episodes that no longer exist in TVDB for {0}", series.SeriesId);
        }

        public virtual void SetPostDownloadStatus(List<int> episodeIds, PostDownloadStatusType postDownloadStatus)
        {
            if (episodeIds.Count == 0) throw new ArgumentException("episodeIds should contain one or more episode ids.");

            var episodeIdString = String.Join(", ", episodeIds);

            var episodeIdQuery = String.Format(@"UPDATE Episodes SET PostDownloadStatus = {0}
                                                    WHERE EpisodeId IN ({1})", (int)postDownloadStatus, episodeIdString);

            logger.Trace("Updating PostDownloadStatus for all episodeIds in {0}", episodeIdString);
            _database.Execute(episodeIdQuery);
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      