using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class EpisodeProvider : IEpisodeProvider
    {
        //TODO: Remove parsing of the series name, it should be done in series provider

        private readonly IRepository _sonicRepo;
        private readonly ISeriesProvider _series;
        private readonly ISeasonProvider _seasons;
        private readonly ITvDbProvider _tvDb;
        private readonly IHistoryProvider _history;
        private readonly IQualityProvider _quality;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public EpisodeProvider(IRepository sonicRepo, ISeriesProvider seriesProvider, ISeasonProvider seasonProvider, ITvDbProvider tvDbProvider, IHistoryProvider history, IQualityProvider quality)
        {
            _sonicRepo = sonicRepo;
            _series = seriesProvider;
            _tvDb = tvDbProvider;
            _seasons = seasonProvider;
            _history = history;
            _quality = quality;
        }

        public Episode GetEpisode(long id)
        {
            return _sonicRepo.Single<Episode>(id);
        }

        public Episode GetEpisode(int seriesId, int seasonNumber, int episodeNumber)
        {
            return _sonicRepo.Single<Episode>(c => c.SeriesId == seriesId && c.SeasonNumber == seasonNumber && c.EpisodeNumber == episodeNumber);
        }

        public IList<Episode> GetEpisodeBySeries(long seriesId)
        {
            return _sonicRepo.Find<Episode>(e => e.SeriesId == seriesId);
        }

        public IList<Episode> GetEpisodeBySeason(long seasonId)
        {
            return _sonicRepo.Find<Episode>(e => e.SeasonId == seasonId);
        }

        public String GetSabTitle(Episode episode)
        {
            var series = _series.GetSeries(episode.SeriesId);
            if (series == null) throw new ArgumentException("Unknown series. ID: " + episode.SeriesId);

            //TODO: This method should return a standard title for the sab episode.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Comprehensive check on whether or not this episode is needed.
        /// </summary>
        /// <param name="episode">Episode that needs to be checked</param>
        /// <returns></returns>
        public bool IsNeeded(EpisodeModel episode)
        {
            //IsSeasonIgnored
            //IsQualityWanted
            //EpisodeFileExists
            //IsInHistory
            //IsOnDisk? (How to handle episodes that are downloaded manually?)

            if (IsSeasonIgnored(episode))
                return false;

            //Quickly check if this quality is wanted at all (We will later check if the quality is still needed)
            if (!_series.QualityWanted(episode.SeriesId, episode.Quality))
            {
                Logger.Debug("Quality [{0}] is not wanted for: {1}", episode.Quality, episode.SeriesTitle);
                return false;
            }

            //Check to see if there is an episode file for this episode
            var dbEpisode = GetEpisode(episode.SeriesId, episode.SeasonNumber, episode.EpisodeNumber);

            if (dbEpisode == null)
            {
                //Todo: How do we want to handle this really? Episode could be released before information is on TheTvDB (Parks and Rec did this a lot in the first season, from experience)
                throw new NotImplementedException("Episode was not found in the database");
            }

            episode.EpisodeId = dbEpisode.EpisodeId;

            var file = _sonicRepo.Single<EpisodeFile>(c => c.FileId == dbEpisode.FileId);

            if (file != null)
            {
                //If not null we need to see if this episode has the quality as the download (or if it is better)
                if (file.Quality == episode.Quality)
                {
                    //If the episodeFile is a Proper we don't need to download again
                    if (file.Proper)
                        return false;
                }

                //There will never be a time when the episode quality is less than what we have and we want it... ever.... I think.
                if (file.Quality > episode.Quality)
                    return false;

                //Now we need to handle upgrades and actually pay attention to the Cutoff Value
                if (file.Quality < episode.Quality)
                {
                    var series = _series.GetSeries(episode.SeriesId);
                    var quality = _quality.Find(series.QualityProfileId);

                    if (quality.Cutoff <= file.Quality)
                    {
                        //If the episodeFile is a Proper we don't need to download again
                        if (file.Proper)
                            return false;
                    }
                }
            }

            //IsInHistory? (NZBDrone)
            if (_history.Exists(dbEpisode.EpisodeId, episode.Quality, episode.Proper))
            {
                Logger.Debug("Episode in history: {0}", episode.ToString());
                return false;
            }

            return true;//If we get to this point and the file has not yet been rejected then accept it
        }

        public void RefreshEpisodeInfo(int seriesId)
        {
            Logger.Info("Starting episode info refresh for series:{0}", seriesId);
            int successCount = 0;
            int failCount = 0;
            var targetSeries = _tvDb.GetSeries(seriesId, true);

            var updateList = new List<Episode>();
            var newList = new List<Episode>();

            Logger.Debug("Updating season info for series:{0}", targetSeries.SeriesName);
            targetSeries.Episodes.Select(e => new { e.SeasonId, e.SeasonNumber })
                .Distinct().ToList()
                .ForEach(s => _seasons.EnsureSeason(seriesId, s.SeasonId, s.SeasonNumber));

            foreach (var episode in targetSeries.Episodes)
            {
                try
                {
                    //DateTime throws an error in SQLServer per message below:
                    //SqlDateTime overflow. Must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM.
                    //So lets hack it so it works for SQLServer (as well as SQLite), perhaps we can find a better solution
                    //Todo: Fix this hack
                    if (episode.FirstAired < new DateTime(1753, 1, 1))
                        episode.FirstAired = new DateTime(1753, 1, 1);

                    Logger.Trace("Updating info for series:{0} - episode:{1}", targetSeries.SeriesName, episode.EpisodeNumber);
                    var newEpisode = new Episode()
                      {
                          AirDate = episode.FirstAired,
                          EpisodeId = episode.Id,
                          EpisodeNumber = episode.EpisodeNumber,
                          Language = episode.Language.Abbriviation,
                          Overview = episode.Overview,
                          SeasonId = episode.SeasonId,
                          SeasonNumber = episode.SeasonNumber,
                          SeriesId = seriesId,
                          Title = episode.EpisodeName
                      };

                    if (_sonicRepo.Exists<Episode>(e => e.EpisodeId == newEpisode.EpisodeId))
                    {
                        updateList.Add(newEpisode);
                    }
                    else
                    {
                        newList.Add(newEpisode);
                    }

                    successCount++;
                }
                catch (Exception e)
                {
                    Logger.FatalException(String.Format("An error has occurred while updating episode info for series {0}", seriesId), e);
                    failCount++;
                }
            }

            _sonicRepo.AddMany(newList);
            _sonicRepo.UpdateMany(updateList);

            Logger.Debug("Finished episode refresh for series:{0}. Successful:{1} - Failed:{2} ", targetSeries.SeriesName, successCount, failCount);
        }

        public void RefreshEpisodeInfo(Season season)
        {
            Logger.Info("Starting episode info refresh for season {0} of series:{1}", season.SeasonNumber, season.SeriesId);
            int successCount = 0;
            int failCount = 0;
            var targetSeries = _tvDb.GetSeries(season.SeriesId, true);

            var updateList = new List<Episode>();
            var newList = new List<Episode>();

            foreach (var episode in targetSeries.Episodes.Where(e => e.SeasonId == season.SeasonId))
            {
                try
                {
                    //DateTime throws an error in SQLServer per message below:
                    //SqlDateTime overflow. Must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM.
                    //So lets hack it so it works for SQLServer (as well as SQLite), perhaps we can find a better solution
                    //Todo: Fix this hack
                    if (episode.FirstAired < new DateTime(1753, 1, 1))
                        episode.FirstAired = new DateTime(1753, 1, 1);

                    Logger.Trace("Updating info for series:{0} - episode:{1}", targetSeries.SeriesName, episode.EpisodeNumber);
                    var newEpisode = new Episode()
                    {
                        AirDate = episode.FirstAired,
                        EpisodeId = episode.Id,
                        EpisodeNumber = episode.EpisodeNumber,
                        Language = episode.Language.Abbriviation,
                        Overview = episode.Overview,
                        SeasonId = episode.SeasonId,
                        SeasonNumber = episode.SeasonNumber,
                        SeriesId = season.SeriesId,
                        Title = episode.EpisodeName
                    };

                    if (_sonicRepo.Exists<Episode>(e => e.EpisodeId == newEpisode.EpisodeId))
                    {
                        updateList.Add(newEpisode);
                    }
                    else
                    {
                        newList.Add(newEpisode);
                    }

                    successCount++;
                }
                catch (Exception e)
                {
                    Logger.FatalException(String.Format("An error has occurred while updating episode info for season {0} of series {1}", season.SeasonNumber, season.SeriesId), e);
                    failCount++;
                }
            }

            _sonicRepo.AddMany(newList);
            _sonicRepo.UpdateMany(updateList);

            Logger.Debug("Finished episode refresh for series:{0}. Successful:{1} - Failed:{2} ", targetSeries.SeriesName, successCount, failCount);
        }

        public void DeleteEpisode(int episodeId)
        {
            _sonicRepo.Delete<Episode>(episodeId);
        }

        private bool IsSeasonIgnored(EpisodeModel episode)
        {
            //Check if this Season is ignored
            if (_seasons.IsIgnored(episode.SeriesId, episode.SeasonNumber))
            {
                Logger.Debug("Season {0} is ignored for: {1}", episode.SeasonNumber, episode.SeriesTitle);
                return true;
            }

            Logger.Debug("Season {0} is wanted for: {1}", episode.SeasonNumber, episode.SeriesTitle);
            return false;
        }
    }
}