using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly TvDbProvider _tvDbProvider;
        private readonly IDatabase _database;
        private readonly SeriesProvider _seriesProvider;

        [Inject]
        public EpisodeProvider(IDatabase database, SeriesProvider seriesProvider, TvDbProvider tvDbProviderProvider)
        {
            _tvDbProvider = tvDbProviderProvider;
            _database = database;
            _seriesProvider = seriesProvider;
        }

        public EpisodeProvider()
        {
        }

        public virtual void AddEpisode(Episode episode)
        {
            //If Season is ignored ignore this episode
            if (IsIgnored(episode.SeriesId, episode.SeasonNumber))
                episode.Ignored = true;

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
            Logger.Trace("Marking episode {0} as fetched.", episodeId);
            _database.Execute("UPDATE Episodes SET GrabDate=@0 WHERE EpisodeId=@1", DateTime.Now, episodeId);
        }

        public virtual IList<Episode> GetEpisodesByParseResult(EpisodeParseResult parseResult, Boolean autoAddNew = false)
        {
            var result = new List<Episode>();

            if (parseResult.AirDate.HasValue)
            {
                var episodeInfo = GetEpisode(parseResult.Series.SeriesId, parseResult.AirDate.Value);

                if (episodeInfo == null && autoAddNew)
                {
                    Logger.Debug("Episode {0} doesn't exist in db. adding it now.", parseResult);
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
                    Logger.Debug("Episode {0} doesn't exist in db. adding it now.", parseResult);
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
                    parseResult.EpisodeTitle += String.Format(" + {0}", episodeInfo.Title);
                    parseResult.EpisodeTitle = parseResult.EpisodeTitle.Trim('+', ' ');
                }
                else
                {
                    Logger.Debug("Unable to find {0}-S{1:00}E{2:00}", parseResult.Series.Title, parseResult.SeasonNumber, episodeNumber);
                }
            }

            return result;
        }

        public virtual IList<Episode> EpisodesWithoutFiles(bool includeSpecials)
        {
            var episodes = _database.Query<Episode, Series>(@"SELECT Episodes.*, Series.Title FROM Episodes
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
            Logger.Info("Starting episode info refresh for series: {0}", series.Title.WithDefault(series.SeriesId));
            int successCount = 0;
            int failCount = 0;
            var tvDbSeriesInfo = _tvDbProvider.GetSeries(series.SeriesId, true);

            var seriesEpisodes = GetEpisodeBySeries(series.SeriesId);
            var updateList = new List<Episode>();
            var newList = new List<Episode>();

            foreach (var episode in tvDbSeriesInfo.Episodes)
            {
                try
                {
                    Logger.Trace("Updating info for [{0}] - S{1}E{2}", tvDbSeriesInfo.SeriesName, episode.SeasonNumber, episode.EpisodeNumber);

                    //first check using tvdbId, this should cover cases when and episode number in a season is changed
                    var episodeToUpdate = seriesEpisodes.Where(e => e.TvDbEpisodeId == episode.Id).SingleOrDefault();

                    //not found, try using season/episode number
                    if (episodeToUpdate == null)
                    {
                        episodeToUpdate = seriesEpisodes.Where(e => e.SeasonNumber == episode.SeasonNumber && e.EpisodeNumber == episode.EpisodeNumber).SingleOrDefault();
                    }

                    //Episode doesn't exist locally
                    if (episodeToUpdate == null)
                    {
                        episodeToUpdate = new Episode();
                        newList.Add(episodeToUpdate);

                        //If it is Episode Zero Ignore it, since it is new
                        if (episode.EpisodeNumber == 0)
                        {
                            episodeToUpdate.Ignored = true;
                        }
                        //Else we need to check if this episode should be ignored based on IsIgnored rules
                        else
                        {
                            episodeToUpdate.Ignored = IsIgnored(series.SeriesId, episode.SeasonNumber);
                        }
                    }
                    else
                    {
                        updateList.Add(episodeToUpdate);
                    }

                    episodeToUpdate.SeriesId = series.SeriesId;
                    episodeToUpdate.TvDbEpisodeId = episode.Id;
                    episodeToUpdate.EpisodeNumber = episode.EpisodeNumber;
                    episodeToUpdate.SeasonNumber = episode.SeasonNumber;
                    episodeToUpdate.Title = episode.EpisodeName;
                    episodeToUpdate.Overview = episode.Overview;

                    if (episode.FirstAired.Year > 1900)
                        episodeToUpdate.AirDate = episode.FirstAired.Date;
                    else
                        episodeToUpdate.AirDate = null;

                    successCount++;
                }
                catch (Exception e)
                {
                    Logger.FatalException(
                        String.Format("An error has occurred while updating episode info for series {0}", tvDbSeriesInfo.SeriesName), e);
                    failCount++;
                }
            }

            _database.InsertMany(newList);
            _database.UpdateMany(updateList);

            Logger.Info("Finished episode refresh for series: {0}. Successful: {1} - Failed: {2} ",
                         tvDbSeriesInfo.SeriesName, successCount, failCount);

            //DeleteEpisodesNotInTvdb
            DeleteEpisodesNotInTvdb(series, tvDbSeriesInfo);
        }

        public virtual void UpdateEpisode(Episode episode)
        {
            _database.Update(episode);
        }

        public virtual bool IsIgnored(int seriesId, int seasonNumber)
        {
            var episodes = _database.Fetch<Episode>(@"SELECT * FROM Episodes WHERE SeriesId=@0 AND SeasonNumber=@1", seriesId, seasonNumber);

            if (episodes == null || episodes.Count == 0)
            {
                if (seasonNumber == 0)
                    return true;

                //Don't check for a previous season if season is 1
                if (seasonNumber == 1)
                    return false;

                //else
                var lastSeasonsEpisodes = _database.Fetch<Episode>(@"SELECT * FROM Episodes 
                                                                     WHERE SeriesId=@0 AND SeasonNumber=@1", seriesId, seasonNumber - 1);

                if (lastSeasonsEpisodes != null && lastSeasonsEpisodes.Count > 0 && lastSeasonsEpisodes.Count == lastSeasonsEpisodes.Where(e => e.Ignored).Count())
                    return true;

                return false;
            }

            if (episodes.Count == episodes.Where(e => e.Ignored).Count())
                return true;

            return false;
        }

        public virtual IList<int> GetSeasons(int seriesId)
        {
            return _database.Fetch<Int32>("SELECT DISTINCT SeasonNumber FROM Episodes WHERE SeriesId=@0", seriesId).OrderBy(c => c).ToList();
        }

        public virtual IList<int> GetEpisodeNumbersBySeason(int seriesId, int seasonNumber)
        {
            return _database.Fetch<int>("SELECT EpisodeNumber FROM Episodes WHERE SeriesId=@0 AND SeasonNumber=@1", seriesId, seasonNumber).OrderBy(c => c).ToList();
        }

        public virtual void SetSeasonIgnore(long seriesId, int seasonNumber, bool isIgnored)
        {
            Logger.Info("Setting ignore flag on Series:{0} Season:{1} to {2}", seriesId, seasonNumber, isIgnored);

            _database.Execute(@"UPDATE Episodes SET Ignored = @0
                                WHERE SeriesId = @1 AND SeasonNumber = @2 AND Ignored = @3",
                isIgnored, seriesId, seasonNumber, !isIgnored);

            Logger.Info("Ignore flag for Series:{0} Season:{1} successfully set to {2}", seriesId, seasonNumber, isIgnored);
        }

        public virtual void SetEpisodeIgnore(int episodeId, bool isIgnored)
        {
            Logger.Info("Setting ignore flag on Episode:{0} to {1}", episodeId, isIgnored);

            _database.Execute(@"UPDATE Episodes SET Ignored = @0
                                WHERE EpisodeId = @1",
                isIgnored, episodeId);

            Logger.Info("Ignore flag for Episode:{0} successfully set to {1}", episodeId, isIgnored);
        }

        public virtual bool IsFirstOrLastEpisodeOfSeason(int seriesId, int seasonNumber, int episodeNumber)
        {
            var episodes = GetEpisodesBySeason(seriesId, seasonNumber).OrderBy(e => e.EpisodeNumber);

            if (episodes.Count() == 0)
                return false;

            //Ensure that this is either the first episode
            //or is the last episode in a season that has 10 or more episodes
            if (episodes.First().EpisodeNumber == episodeNumber || (episodes.Count() >= 10 && episodes.Last().EpisodeNumber == episodeNumber))
                return true;

            return false;
        }

        public IList<Episode> AttachSeries(IList<Episode> episodes)
        {
            if (episodes.Count == 0) return episodes;

            if (episodes.Select(c => c.SeriesId).Distinct().Count() > 1)
                throw new ArgumentException("Episodes belong to more than one series.");

            var series = _seriesProvider.GetSeries(episodes.First().SeriesId);
            episodes.ToList().ForEach(c => c.Series = series);

            return episodes;
        }

        public Episode AttachSeries(Episode episode)
        {
            if (episode == null) return episode;
            episode.Series = _seriesProvider.GetSeries(episode.SeriesId);
            return episode;
        }

        public virtual void DeleteEpisodesNotInTvdb(Series series, TvdbSeries tvDbSeriesInfo)
        {
            Logger.Trace("Starting deletion of episodes that no longer exist in TVDB: {0}", series.Title.WithDefault(series.SeriesId));

            //Delete Episodes not matching TvDbIds for this series
            var tvDbIds = tvDbSeriesInfo.Episodes.Select(e => e.Id);
            var tvDbIdString = String.Join(", ", tvDbIds);

            var tvDbIdQuery = String.Format("DELETE FROM Episodes WHERE SeriesId = {0} AND TvDbEpisodeId > 0 AND TvDbEpisodeId NOT IN ({1})",
                                                                                    series.SeriesId, tvDbIdString);

            _database.Execute(tvDbIdQuery);

            Logger.Trace("Deleted episodes that no longer exist in TVDB for {0}", series.SeriesId);
        }

        public virtual void SetPostDownloadStatus(List<int> episodeIds, PostDownloadStatusType postDownloadStatus)
        {
            if (episodeIds.Count == 0) throw new ArgumentException("episodeIds should contain one or more episode ids.");

            var episodeIdString = String.Join(", ", episodeIds);

            var episodeIdQuery = String.Format(@"UPDATE Episodes SET PostDownloadStatus = {0}
                                                    WHERE EpisodeId IN ({1})", (int)postDownloadStatus, episodeIdString);

            Logger.Trace("Updating PostDownloadStatus for all episodeIds in {0}", episodeIdString);
            _database.Execute(episodeIdQuery);
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      