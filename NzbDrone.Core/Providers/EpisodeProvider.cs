using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using PetaPoco;

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
            _database.Insert(episode);
        }

        public virtual Episode GetEpisode(long id)
        {
            return AttachSeries(_database.Single<Episode>(id));
        }

        public virtual Episode GetEpisode(int seriesId, int seasonNumber, int episodeNumber)
        {
            return AttachSeries(_database.SingleOrDefault<Episode>("WHERE SeriesId = @0 AND SeasonNumber = @1 AND EpisodeNumber = @2", seriesId, seasonNumber, episodeNumber));
        }

        public virtual Episode GetEpisode(int seriesId, DateTime date)
        {
            return AttachSeries(_database.SingleOrDefault<Episode>("WHERE SeriesId = @0 AND AirDate = @1", seriesId, date.Date));
        }

        public virtual IList<Episode> GetEpisodeBySeries(long seriesId)
        {
            return AttachSeries(_database.Fetch<Episode>("WHERE SeriesId = @0", seriesId));
        }

        public virtual IList<Episode> GetEpisodesBySeason(long seriesId, int seasonNumber)
        {
            return AttachSeries(_database.Fetch<Episode>("WHERE SeriesId = @0 AND SeasonNumber = @1", seriesId, seasonNumber));
        }

        public virtual List<Episode> GetEpisodes(EpisodeParseResult parseResult)
        {
            if (parseResult.Series == null)
            {
                Logger.Debug("Episode Parse Result is Invalid, skipping");
                return new List<Episode>();
            }

            var episodes = new List<Episode>();

            foreach (var ep in parseResult.EpisodeNumbers)
            {
                var episode = GetEpisode(parseResult.Series.SeriesId, parseResult.SeasonNumber, ep);

                if (episode == null)
                    return new List<Episode>();

                episodes.Add(episode);
            }

            return episodes;
        }

        public virtual IList<Episode> EpisodesWithoutFiles(bool includeSpecials)
        {
            var episodes = _database.Query<Episode>("WHERE (EpisodeFileId=0 OR EpisodeFileId=NULL) AND AirDate<=@0",
                                                    DateTime.Now.Date);
            if (includeSpecials)
                return episodes.Where(e => e.SeasonNumber > 0).ToList();

            return AttachSeries(episodes.ToList());
        }

        public virtual IList<Episode> EpisodesByFileId(int episodeFileId)
        {
            return AttachSeries(_database.Fetch<Episode>("WHERE EpisodeFileId = @0", episodeFileId));
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
                    //DateTime throws an error in SQLServer per message below:
                    //SqlDateTime overflow. Must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM.
                    //So lets hack it so it works for SQLServer (as well as SQLite), perhaps we can find a better solution
                    //Todo: Fix this hack
                    if (episode.FirstAired < new DateTime(1753, 1, 1))
                        episode.FirstAired = new DateTime(1753, 1, 1);

                    Logger.Trace("Updating info for [{0}] - S{1}E{2}", tvDbSeriesInfo.SeriesName, episode.SeasonNumber, episode.EpisodeNumber);

                    //first check using tvdbId, this should cover cases when and episode number in a season is changed
                    var episodeToUpdate = seriesEpisodes.Where(e => e.TvDbEpisodeId == episode.Id).FirstOrDefault();

                    //not found, try using season/episode number
                    if (episodeToUpdate == null)
                    {
                        episodeToUpdate = seriesEpisodes.Where(e => e.SeasonNumber == episode.SeasonNumber && e.EpisodeNumber == episode.EpisodeNumber).FirstOrDefault();
                    }

                    //Episode doesn't exist locally
                    if (episodeToUpdate == null)
                    {
                        episodeToUpdate = new Episode();
                        newList.Add(episodeToUpdate);
                    }
                    else
                    {
                        updateList.Add(episodeToUpdate);
                    }

                    episodeToUpdate.SeriesId = series.SeriesId;
                    episodeToUpdate.TvDbEpisodeId = episode.Id;
                    episodeToUpdate.AirDate = episode.FirstAired.Date;
                    episodeToUpdate.EpisodeNumber = episode.EpisodeNumber;
                    episodeToUpdate.SeasonNumber = episode.SeasonNumber;
                    episodeToUpdate.Title = episode.EpisodeName;
                    episodeToUpdate.Overview = episode.Overview;

                    successCount++;
                }
                catch (Exception e)
                {
                    Logger.FatalException(
                        String.Format("An error has occurred while updating episode info for series {0}", tvDbSeriesInfo.SeriesName), e);
                    failCount++;
                }
            }

            using (var tran = _database.GetTransaction())
            {
                newList.ForEach(episode => _database.Insert(episode));
                updateList.ForEach(episode => _database.Update(episode));

                //Shouldn't run if Database is a mock since transaction will be null
                if (_database.GetType().Namespace != "Castle.Proxies" && tran != null)
                {
                    tran.Complete();
                }

            }

            Logger.Info("Finished episode refresh for series: {0}. Successful: {1} - Failed: {2} ",
                         tvDbSeriesInfo.SeriesName, successCount, failCount);
        }



        public virtual void UpdateEpisode(Episode episode)
        {
            _database.Update(episode);
        }

        public virtual bool IsIgnored(int seriesId, int seasonNumber)
        {

            var unIgnoredCount = _database.ExecuteScalar<int>(
                "SELECT COUNT (*) FROM Episodes WHERE SeriesId=@0 AND SeasonNumber=@1 AND Ignored='0'", seriesId, seasonNumber);

            return unIgnoredCount == 0;
        }

        public virtual IList<int> GetSeasons(int seriesId)
        {
            return _database.Fetch<Int32>("SELECT DISTINCT SeasonNumber FROM Episodes WHERE SeriesId=@0", seriesId).OrderBy(c => c).ToList();
        }

        public virtual void SetSeasonIgnore(long seriesId, int seasonNumber, bool isIgnored)
        {
            Logger.Info("Setting ignore flag on Series:{0} Season:{1} to {2}", seriesId, seasonNumber, isIgnored);
            var episodes = GetEpisodesBySeason(seriesId, seasonNumber);

            using (var tran = _database.GetTransaction())
            {
                foreach (var episode in episodes)
                {
                    episode.Ignored = isIgnored;
                    _database.Update(episode);
                }

                //Shouldn't run if Database is a mock since transaction will be null
                if (_database.GetType().Namespace != "Castle.Proxies" && tran != null)
                {
                    tran.Complete();
                }

                Logger.Info("Ignore flag for Series:{0} Season:{1} successfully set to {2}", seriesId, seasonNumber, isIgnored);
            }


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
            episode.Series = _seriesProvider.GetSeries(episode.SeriesId);
            return episode;
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      