using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public interface IRefreshEpisodeService
    {
        void RefreshEpisodeInfo(Series series, IEnumerable<Episode> remoteEpisodes);
    }

    public class RefreshEpisodeService : IRefreshEpisodeService
    {
        private readonly IEpisodeService _episodeService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public RefreshEpisodeService(IEpisodeService episodeService, IEventAggregator eventAggregator, Logger logger)
        {
            _episodeService = episodeService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void RefreshEpisodeInfo(Series series, IEnumerable<Episode> remoteEpisodes)
        {
            _logger.Info("Starting episode info refresh for: {0}", series);
            var successCount = 0;
            var failCount = 0;

            var existingEpisodes = _episodeService.GetEpisodeBySeries(series.Id);
            var seasons = series.Seasons;
            var hasExisting = existingEpisodes.Any();

            var updateList = new List<Episode>();
            var newList = new List<Episode>();
            var dupeFreeRemoteEpisodes = remoteEpisodes.DistinctBy(m => new { m.SeasonNumber, m.EpisodeNumber }).ToList();

            if (series.SeriesType == SeriesTypes.Anime)
            {
                dupeFreeRemoteEpisodes = MapAbsoluteEpisodeNumbers(dupeFreeRemoteEpisodes);
            }

            foreach (var episode in OrderEpisodes(series, dupeFreeRemoteEpisodes))
            {
                try
                {
                    var episodeToUpdate = GetEpisodeToUpdate(series, episode, existingEpisodes);

                    if (episodeToUpdate != null)
                    {
                        existingEpisodes.Remove(episodeToUpdate);
                        updateList.Add(episodeToUpdate);
                    }
                    else
                    {
                        episodeToUpdate = new Episode();
                        episodeToUpdate.Monitored = GetMonitoredStatus(episode, seasons);
                        newList.Add(episodeToUpdate);
                    }

                    episodeToUpdate.SeriesId = series.Id;
                    episodeToUpdate.TvdbId = episode.TvdbId;
                    episodeToUpdate.EpisodeNumber = episode.EpisodeNumber;
                    episodeToUpdate.SeasonNumber = episode.SeasonNumber;
                    episodeToUpdate.AbsoluteEpisodeNumber = episode.AbsoluteEpisodeNumber;
                    episodeToUpdate.AiredAfterSeasonNumber = episode.AiredAfterSeasonNumber;
                    episodeToUpdate.AiredBeforeSeasonNumber = episode.AiredBeforeSeasonNumber;
                    episodeToUpdate.AiredBeforeEpisodeNumber = episode.AiredBeforeEpisodeNumber;
                    episodeToUpdate.Title = episode.Title ?? "TBA";
                    episodeToUpdate.Overview = episode.Overview;
                    episodeToUpdate.AirDate = episode.AirDate;
                    episodeToUpdate.AirDateUtc = episode.AirDateUtc;
                    episodeToUpdate.Ratings = episode.Ratings;
                    episodeToUpdate.Images = episode.Images;

                    successCount++;
                }
                catch (Exception e)
                {
                    _logger.Fatal(e, "An error has occurred while updating episode info for series {0}. {1}", series, episode);
                    failCount++;
                }
            }

            UnmonitorReaddedEpisodes(series, newList, hasExisting);

            var allEpisodes = new List<Episode>();
            allEpisodes.AddRange(newList);
            allEpisodes.AddRange(updateList);

            AdjustMultiEpisodeAirTime(series, allEpisodes);
            AdjustDirectToDvdAirDate(series, allEpisodes);

            _episodeService.DeleteMany(existingEpisodes);
            _episodeService.UpdateMany(updateList);
            _episodeService.InsertMany(newList);

            _eventAggregator.PublishEvent(new EpisodeInfoRefreshedEvent(series, newList, updateList, existingEpisodes));

            if (failCount != 0)
            {
                _logger.Info("Finished episode refresh for series: {0}. Successful: {1} - Failed: {2} ",
                    series.Title,
                    successCount,
                    failCount);
            }
            else
            {
                _logger.Info("Finished episode refresh for series: {0}.", series);
            }
        }

        private bool GetMonitoredStatus(Episode episode, IEnumerable<Season> seasons)
        {
            if (episode.EpisodeNumber == 0 && episode.SeasonNumber != 1)
            {
                return false;
            }

            var season = seasons.SingleOrDefault(c => c.SeasonNumber == episode.SeasonNumber);
            return season == null || season.Monitored;
        }

        private void UnmonitorReaddedEpisodes(Series series, List<Episode> episodes, bool hasExisting)
        {
            if (series.AddOptions != null)
            {
                return;
            }

            var threshold = DateTime.UtcNow.AddDays(-14);

            var oldEpisodes = episodes.Where(e => e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(threshold)).ToList();

            if (oldEpisodes.Any())
            {
                if (hasExisting)
                {
                    _logger.Warn("Show {0} ({1}) had {2} old episodes appear, please check monitored status.", series.TvdbId, series.Title, oldEpisodes.Count);
                }
                else
                {
                    threshold = DateTime.UtcNow.AddDays(-1);

                    foreach (var episode in episodes)
                    {
                        if (episode.AirDateUtc.HasValue && episode.AirDateUtc.Value.Before(threshold))
                        {
                            episode.Monitored = false;
                        }
                    }

                    _logger.Warn("Show {0} ({1}) had {2} old episodes appear, unmonitored aired episodes to prevent unexpected downloads.", series.TvdbId, series.Title, oldEpisodes.Count);
                }
            }
        }

        private void AdjustMultiEpisodeAirTime(Series series, IEnumerable<Episode> allEpisodes)
        {
            var groups = allEpisodes.Where(c => c.AirDateUtc.HasValue)
                                    .GroupBy(e => new { e.SeasonNumber, e.AirDate })
                                    .Where(g => g.Count() > 1)
                                    .ToList();

            foreach (var group in groups)
            {
                if (group.Key.SeasonNumber != 0 && group.Count() > 3)
                {
                    _logger.Debug("Not adjusting multi-episode air times for series {0} season {1} since more than 3 episodes 'aired' on the same day", series.Title, group.Key.SeasonNumber);
                    continue;
                }

                var episodeCount = 0;

                foreach (var episode in group.OrderBy(e => e.SeasonNumber).ThenBy(e => e.EpisodeNumber))
                {
                    episode.AirDateUtc = episode.AirDateUtc.Value.AddMinutes(series.Runtime * episodeCount);
                    episodeCount++;
                }
            }
        }

        private void AdjustDirectToDvdAirDate(Series series, IEnumerable<Episode> allEpisodes)
        {
            if (series.Status == SeriesStatusType.Ended && allEpisodes.All(v => !v.AirDateUtc.HasValue) && series.FirstAired.HasValue)
            {
                foreach (var episode in allEpisodes)
                {
                    episode.AirDateUtc = series.FirstAired;
                    episode.AirDate = series.FirstAired.Value.ToString("yyyy-MM-dd");
                }
            }
        }

        private List<Episode> MapAbsoluteEpisodeNumbers(List<Episode> remoteEpisodes)
        {
            //Return all episodes with no abs number, but distinct for those with abs number
            return remoteEpisodes.Where(e => e.AbsoluteEpisodeNumber.HasValue)
                                 .OrderByDescending(e => e.SeasonNumber)
                                 .DistinctBy(e => e.AbsoluteEpisodeNumber.Value)
                                 .Concat(remoteEpisodes.Where(e => !e.AbsoluteEpisodeNumber.HasValue))
                                 .ToList();
        }

        private Episode GetEpisodeToUpdate(Series series, Episode episode, List<Episode> existingEpisodes)
        {
            if (series.SeriesType == SeriesTypes.Anime)
            {
                if (episode.AbsoluteEpisodeNumber.HasValue)
                {
                    var matchingEpisode = existingEpisodes.FirstOrDefault(e => e.AbsoluteEpisodeNumber == episode.AbsoluteEpisodeNumber);

                    if (matchingEpisode != null)
                    {
                        return matchingEpisode;
                    }
                }
            }

            return existingEpisodes.FirstOrDefault(e => e.SeasonNumber == episode.SeasonNumber && e.EpisodeNumber == episode.EpisodeNumber);
        }

        private IEnumerable<Episode> OrderEpisodes(Series series, List<Episode> episodes)
        {
            if (series.SeriesType == SeriesTypes.Anime)
            {
                var withAbs = episodes.Where(e => e.AbsoluteEpisodeNumber.HasValue)
                                      .OrderBy(e => e.AbsoluteEpisodeNumber);

                var withoutAbs = episodes.Where(e => !e.AbsoluteEpisodeNumber.HasValue)
                                         .OrderBy(e => e.SeasonNumber)
                                         .ThenBy(e => e.EpisodeNumber);

                return withAbs.Concat(withoutAbs);
            }

            return episodes.OrderBy(e => e.SeasonNumber).ThenBy(e => e.EpisodeNumber);
        }
    }
}
