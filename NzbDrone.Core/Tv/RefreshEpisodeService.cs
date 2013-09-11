using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
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
        private readonly ISeasonService _seasonService;
        private readonly IMessageAggregator _messageAggregator;
        private readonly Logger _logger;

        public RefreshEpisodeService(IEpisodeService episodeService,
            ISeasonService seasonService, IMessageAggregator messageAggregator, Logger logger)
        {
            _episodeService = episodeService;
            _seasonService = seasonService;
            _messageAggregator = messageAggregator;
            _logger = logger;
        }

        public void RefreshEpisodeInfo(Series series, IEnumerable<Episode> remoteEpisodes)
        {
            _logger.Info("Starting episode info refresh for: {0}", series);
            var successCount = 0;
            var failCount = 0;

            var existingEpisodes = _episodeService.GetEpisodeBySeries(series.Id);
            var seasons = _seasonService.GetSeasonsBySeries(series.Id);

            var updateList = new List<Episode>();
            var newList = new List<Episode>();

            foreach (var episode in remoteEpisodes.OrderBy(e => e.SeasonNumber).ThenBy(e => e.EpisodeNumber))
            {
                try
                {
                    var episodeToUpdate = existingEpisodes.SingleOrDefault(e => e.SeasonNumber == episode.SeasonNumber && e.EpisodeNumber == episode.EpisodeNumber);

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
                    episodeToUpdate.TvDbEpisodeId = episode.TvDbEpisodeId;
                    episodeToUpdate.EpisodeNumber = episode.EpisodeNumber;
                    episodeToUpdate.SeasonNumber = episode.SeasonNumber;
                    episodeToUpdate.Title = episode.Title;
                    episodeToUpdate.Overview = episode.Overview;
                    episodeToUpdate.AirDate = episode.AirDate;
                    episodeToUpdate.AirDateUtc = episode.AirDateUtc;

                    successCount++;
                }
                catch (Exception e)
                {
                    _logger.FatalException(String.Format("An error has occurred while updating episode info for series {0}. {1}", series, episode), e);
                    failCount++;
                }
            }

            var allEpisodes = new List<Episode>();
            allEpisodes.AddRange(newList);
            allEpisodes.AddRange(updateList);

            AdjustMultiEpisodeAirTime(series, allEpisodes);

            _episodeService.DeleteMany(existingEpisodes);
            _episodeService.UpdateMany(updateList);
            _episodeService.InsertMany(newList);

            if (newList.Any())
            {
                _messageAggregator.PublishEvent(new EpisodeInfoAddedEvent(newList, series));
            }

            if (updateList.Any())
            {
                _messageAggregator.PublishEvent(new EpisodeInfoUpdatedEvent(updateList));
            }

            if (existingEpisodes.Any())
            {
                _messageAggregator.PublishEvent(new EpisodeInfoDeletedEvent(updateList));
            }

            if (failCount != 0)
            {
                _logger.Info("Finished episode refresh for series: {0}. Successful: {1} - Failed: {2} ",
                    series.Title, successCount, failCount);
            }
            else
            {
                _logger.Info("Finished episode refresh for series: {0}.", series);
            }
        }

        private static bool GetMonitoredStatus(Episode episode, IEnumerable<Season> seasons)
        {
            if (episode.EpisodeNumber == 0 && episode.SeasonNumber != 1)
            {
                return false;
            }

            var season = seasons.SingleOrDefault(c => c.SeasonNumber == episode.SeasonNumber);
            return season == null || season.Monitored;
        }

        private static void AdjustMultiEpisodeAirTime(Series series, IEnumerable<Episode> allEpisodes)
        {
            var groups =
                allEpisodes.Where(c => c.AirDateUtc.HasValue)
                    .GroupBy(e => new { e.SeasonNumber, e.AirDate })
                    .Where(g => g.Count() > 1)
                    .ToList();

            foreach (var group in groups)
            {
                var episodeCount = 0;
                foreach (var episode in @group.OrderBy(e => e.SeasonNumber).ThenBy(e => e.EpisodeNumber))
                {
                    episode.AirDateUtc = episode.AirDateUtc.Value.AddMinutes(series.Runtime * episodeCount);
                    episodeCount++;
                }
            }
        }
    }
}