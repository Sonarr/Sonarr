using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Tv
{
    public interface IEpisodeMonitoredService
    {
        void SetEpisodeMonitoredStatus(Series series, MonitoringOptions monitoringOptions);
    }

    public class EpisodeMonitoredService : IEpisodeMonitoredService
    {
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public EpisodeMonitoredService(ISeriesService seriesService, IEpisodeService episodeService, Logger logger)
        {
            _seriesService = seriesService;
            _episodeService = episodeService;
            _logger = logger;
        }

        public void SetEpisodeMonitoredStatus(Series series, MonitoringOptions monitoringOptions)
        {
            // Update the series without changing the episodes
            if (monitoringOptions == null)
            {
                _seriesService.UpdateSeries(series, false);
                return;
            }

            // Fallback for v2 endpoints
            if (monitoringOptions.Monitor == MonitorTypes.Unknown)
            {
                LegacySetEpisodeMonitoredStatus(series, monitoringOptions);
                return;
            }

            // Skip episode level monitoring and use season information when series was added
            if (monitoringOptions.Monitor == MonitorTypes.Skip)
            {
                return;
            }

            var firstSeason = series.Seasons.Select(s => s.SeasonNumber).Where(s => s > 0).MinOrDefault();
            var lastSeason = series.Seasons.Select(s => s.SeasonNumber).MaxOrDefault();

            _logger.Debug("[{0}] Setting episode monitored status to {1}", series.Title, monitoringOptions.Monitor);

            var monitoredSeasons = _episodeService.SetEpisodeMonitoredBySeries(series.Id, monitoringOptions.Monitor, firstSeason, lastSeason);

            foreach (var season in series.Seasons)
            {
                var seasonNumber = season.SeasonNumber;

                // Monitor the last season when:
                // - Not specials
                // - The latest season
                // - Set to monitor all episodes
                // - Set to monitor future episodes and series is continuing or not yet aired
                if (seasonNumber > 0 &&
                    seasonNumber == lastSeason &&
                    (monitoringOptions.Monitor == MonitorTypes.All ||
                     (monitoringOptions.Monitor == MonitorTypes.Future && series.Status is SeriesStatusType.Continuing or SeriesStatusType.Upcoming)))
                {
                    season.Monitored = true;
                }
                else if (seasonNumber == firstSeason && monitoringOptions.Monitor == MonitorTypes.Pilot)
                {
                    // Don't monitor season 1 if only the pilot episode is monitored
                    season.Monitored = false;
                }
                else if (monitoredSeasons.Contains(seasonNumber))
                {
                    // Monitor the season if it has any monitor episodes
                    season.Monitored = true;
                }

                // Don't monitor the season
                else
                {
                    season.Monitored = false;
                }
            }

            _seriesService.UpdateSeries(series, false);
        }

        private void LegacySetEpisodeMonitoredStatus(Series series, MonitoringOptions monitoringOptions)
        {
            _logger.Debug("[{0}] Setting episode monitored status.", series.Title);

            var episodes = _episodeService.GetEpisodeBySeries(series.Id);

            if (monitoringOptions.IgnoreEpisodesWithFiles)
            {
                _logger.Debug("Unmonitoring Episodes with Files");
                ToggleEpisodesMonitoredState(episodes.Where(e => e.HasFile), false);
            }
            else
            {
                _logger.Debug("Monitoring Episodes with Files");
                ToggleEpisodesMonitoredState(episodes.Where(e => e.HasFile), true);
            }

            if (monitoringOptions.IgnoreEpisodesWithoutFiles)
            {
                _logger.Debug("Unmonitoring Episodes without Files");
                ToggleEpisodesMonitoredState(episodes.Where(e => !e.HasFile && e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(DateTime.UtcNow)), false);
            }
            else
            {
                _logger.Debug("Monitoring Episodes without Files");
                ToggleEpisodesMonitoredState(episodes.Where(e => !e.HasFile && e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(DateTime.UtcNow)), true);
            }

            var lastSeason = series.Seasons.Select(s => s.SeasonNumber).MaxOrDefault();

            foreach (var s in series.Seasons)
            {
                var season = s;

                // If the season is unmonitored we should unmonitor all episodes in that season

                if (!season.Monitored)
                {
                    _logger.Debug("Unmonitoring all episodes in season {0}", season.SeasonNumber);
                    ToggleEpisodesMonitoredState(episodes.Where(e => e.SeasonNumber == season.SeasonNumber), false);
                }

                // If the season is not the latest season and all it's episodes are unmonitored the season will be unmonitored

                if (season.SeasonNumber < lastSeason)
                {
                    if (episodes.Where(e => e.SeasonNumber == season.SeasonNumber).All(e => !e.Monitored))
                    {
                        _logger.Debug("Unmonitoring season {0} because all episodes are not monitored", season.SeasonNumber);
                        season.Monitored = false;
                    }
                }
            }

            _episodeService.UpdateEpisodes(episodes);

            _seriesService.UpdateSeries(series, false);
        }

        private void ToggleEpisodesMonitoredState(IEnumerable<Episode> episodes, bool monitored)
        {
            foreach (var episode in episodes)
            {
                episode.Monitored = monitored;
            }
        }

        private void ToggleEpisodesMonitoredState(List<Episode> episodes, Func<Episode, bool> predicate)
        {
            ToggleEpisodesMonitoredState(episodes.Where(predicate), true);
            ToggleEpisodesMonitoredState(episodes.Where(e => !predicate(e)), false);
        }
    }
}
