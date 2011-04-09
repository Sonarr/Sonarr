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
        private readonly SeriesProvider _series;
        private readonly ISeasonProvider _seasons;
        private readonly TvDbProvider _tvDb;
        private readonly HistoryProvider _history;
        private readonly QualityProvider _quality;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public EpisodeProvider(IRepository sonicRepo, SeriesProvider seriesProvider,
            ISeasonProvider seasonProvider, TvDbProvider tvDbProvider,
            HistoryProvider history, QualityProvider quality)
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
        /// <param name="parsedReport">Episode that needs to be checked</param>
        /// <returns></returns>
        public bool IsNeeded(EpisodeParseResult parsedReport)
        {
            foreach (var episode in parsedReport.Episodes)
            {
                var episodeInfo = GetEpisode(parsedReport.SeriesId, parsedReport.SeasonNumber, episode);

                if (episodeInfo == null)
                {
                    //Todo: How do we want to handle this really? Episode could be released before information is on TheTvDB 
                    //(Parks and Rec did this a lot in the first season, from experience)
                    //Keivan: Should automatically add the episode to db with minimal information. then update the description/title when avilable.
                    throw new NotImplementedException("Episode was not found in the database");
                }

                var file = episodeInfo.EpisodeFile;

                if (file != null)
                {
                    //If not null we need to see if this episode has the quality as the download (or if it is better)
                    if (file.Quality == parsedReport.Quality && file.Proper) continue;

                    //There will never be a time when the episode quality is less than what we have and we want it... ever.... I think.
                    if (file.Quality > parsedReport.Quality) continue;

                    //Now we need to handle upgrades and actually pay attention to the Cutoff Value
                    if (file.Quality < parsedReport.Quality)
                    {
                        var quality = _quality.Find(episodeInfo.Series.QualityProfileId);

                        if (quality.Cutoff <= file.Quality && file.Proper) continue;
                    }
                }

                //IsInHistory? (NZBDrone)
                if (_history.Exists(episodeInfo.EpisodeId, parsedReport.Quality, parsedReport.Proper))
                {
                    Logger.Debug("Episode in history: {0}", episode.ToString());
                    continue;
                }

                return true;//If we get to this point and the file has not yet been rejected then accept it
            }

            return false;

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

        public void UpdateEpisode(Episode episode)
        {
            _sonicRepo.Update(episode);
        }

        private bool IsSeasonIgnored(EpisodeParseResult episode)
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