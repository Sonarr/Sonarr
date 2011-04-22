using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class EpisodeProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly QualityProvider _quality;
        private readonly SeasonProvider _seasons;
        private readonly SeriesProvider _series;
        private readonly IRepository _sonicRepo;
        private readonly TvDbProvider _tvDb;

        public EpisodeProvider(IRepository sonicRepo, SeriesProvider seriesProvider,
                               SeasonProvider seasonProvider, TvDbProvider tvDbProvider,
                               QualityProvider quality)
        {
            _sonicRepo = sonicRepo;
            _series = seriesProvider;
            _tvDb = tvDbProvider;
            _seasons = seasonProvider;
            _quality = quality;
        }

        public EpisodeProvider()
        {
        }

        public virtual Episode GetEpisode(long id)
        {
            return _sonicRepo.Single<Episode>(id);
        }

        public virtual Episode GetEpisode(int seriesId, int seasonNumber, int episodeNumber)
        {
            return
                _sonicRepo.Single<Episode>(
                    c => c.SeriesId == seriesId && c.SeasonNumber == seasonNumber && c.EpisodeNumber == episodeNumber);
        }

        public virtual IList<Episode> GetEpisodeBySeries(long seriesId)
        {
            return _sonicRepo.Find<Episode>(e => e.SeriesId == seriesId);
        }

        public virtual IList<Episode> GetEpisodeBySeason(long seasonId)
        {
            return _sonicRepo.Find<Episode>(e => e.SeasonId == seasonId);
        }

        public virtual IList<Episode> GetEpisodeByParseResult(EpisodeParseResult parseResult)
        {
            return _sonicRepo.Find<Episode>(e =>
                                            e.SeriesId == parseResult.SeriesId &&
                                            e.SeasonNumber == parseResult.SeasonNumber &&
                                            parseResult.Episodes.Contains(e.EpisodeNumber));

        }

        public virtual String GetSabTitle(EpisodeParseResult parseResult)
        {
            //Show Name - 1x01-1x02 - Episode Name
            //Show Name - 1x01 - Episode Name
            var episodeString = new List<String>();

            foreach (var episode in parseResult.Episodes)
            {
                episodeString.Add(String.Format("{0}x{1}", parseResult.SeasonNumber, episode));
            }

            var epNumberString = String.Join("-", episodeString);
            var series = _series.GetSeries(parseResult.SeriesId);
            var folderName = new DirectoryInfo(series.Path).Name;

            var result = String.Format("{0} - {1} - {2} {3}", folderName, epNumberString, parseResult.EpisodeTitle, parseResult.Quality);

            if (parseResult.Proper)
            {
                result += " [Proper]";
            }

            return result;
        }

        /// <summary>
        ///   Comprehensive check on whether or not this episode is needed.
        /// </summary>
        /// <param name = "parsedReport">Episode that needs to be checked</param>
        /// <returns></returns>
        public virtual bool IsNeeded(EpisodeParseResult parsedReport)
        {
            foreach (var episode in parsedReport.Episodes)
            {
                var episodeInfo = GetEpisode(parsedReport.SeriesId, parsedReport.SeasonNumber, episode);

                if (episodeInfo == null)
                {

                    //Todo: How do we want to handle this really? Episode could be released before information is on TheTvDB 
                    //(Parks and Rec did this a lot in the first season, from experience)
                    //Keivan: Should automatically add the episode to db with minimal information. then update the description/title when available.
                    episodeInfo = new Episode
                                      {
                                          SeriesId = parsedReport.SeriesId,
                                          AirDate = DateTime.Now.Date,
                                          EpisodeNumber = episode,
                                          SeasonNumber = parsedReport.SeasonNumber,
                                          Title = String.Empty,
                                          Overview = String.Empty,
                                          Language = "en"
                                      };

                    _sonicRepo.Add(episodeInfo);

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

                return true; //If we get to this point and the file has not yet been rejected then accept it
            }

            return false;
        }

        public virtual void RefreshEpisodeInfo(int seriesId)
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

                    Logger.Trace("Updating info for [{0}] - S{1}E{2}", targetSeries.SeriesName, episode.SeasonNumber, episode.EpisodeNumber);
                    var newEpisode = new Episode
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
                    Logger.FatalException(
                        String.Format("An error has occurred while updating episode info for series {0}", seriesId), e);
                    failCount++;
                }
            }

            _sonicRepo.AddMany(newList);
            _sonicRepo.UpdateMany(updateList);

            Logger.Debug("Finished episode refresh for series:{0}. Successful:{1} - Failed:{2} ",
                         targetSeries.SeriesName, successCount, failCount);
        }

        public virtual void RefreshEpisodeInfo(Season season)
        {
            Logger.Info("Starting episode info refresh for season {0} of series:{1}", season.SeasonNumber,
                        season.SeriesId);
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

                    Logger.Trace("Updating info for series:{0} - episode:{1}", targetSeries.SeriesName,
                                 episode.EpisodeNumber);
                    var newEpisode = new Episode
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


                    //TODO: Replace this db check with a local check. Should make things even faster
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
                    Logger.FatalException(
                        String.Format("An error has occurred while updating episode info for season {0} of series {1}",
                                      season.SeasonNumber, season.SeriesId), e);
                    failCount++;
                }
            }

            _sonicRepo.AddMany(newList);
            _sonicRepo.UpdateMany(updateList);

            Logger.Debug("Finished episode refresh for series:{0}. Successful:{1} - Failed:{2} ",
                         targetSeries.SeriesName, successCount, failCount);
        }

        public virtual void DeleteEpisode(int episodeId)
        {
            _sonicRepo.Delete<Episode>(episodeId);
        }

        public virtual void UpdateEpisode(Episode episode)
        {
            _sonicRepo.Update(episode);
        }
    }
}