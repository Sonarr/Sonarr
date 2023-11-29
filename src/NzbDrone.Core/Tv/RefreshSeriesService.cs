using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public class RefreshSeriesService : IExecute<RefreshSeriesCommand>
    {
        private readonly IProvideSeriesInfo _seriesInfo;
        private readonly ISeriesService _seriesService;
        private readonly IRefreshEpisodeService _refreshEpisodeService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskScanService _diskScanService;
        private readonly ICheckIfSeriesShouldBeRefreshed _checkIfSeriesShouldBeRefreshed;
        private readonly IConfigService _configService;
        private readonly IAutoTaggingService _autoTaggingService;
        private readonly Logger _logger;

        public RefreshSeriesService(IProvideSeriesInfo seriesInfo,
                                    ISeriesService seriesService,
                                    IRefreshEpisodeService refreshEpisodeService,
                                    IEventAggregator eventAggregator,
                                    IDiskScanService diskScanService,
                                    ICheckIfSeriesShouldBeRefreshed checkIfSeriesShouldBeRefreshed,
                                    IConfigService configService,
                                    IAutoTaggingService autoTaggingService,
                                    Logger logger)
        {
            _seriesInfo = seriesInfo;
            _seriesService = seriesService;
            _refreshEpisodeService = refreshEpisodeService;
            _eventAggregator = eventAggregator;
            _diskScanService = diskScanService;
            _checkIfSeriesShouldBeRefreshed = checkIfSeriesShouldBeRefreshed;
            _configService = configService;
            _autoTaggingService = autoTaggingService;
            _logger = logger;
        }

        private Series RefreshSeriesInfo(int seriesId)
        {
            // Get the series before updating, that way any changes made to the series after the refresh started,
            // but before this series was refreshed won't be lost.
            var series = _seriesService.GetSeries(seriesId);

            _logger.ProgressInfo("Updating {0}", series.Title);

            Series seriesInfo;
            List<Episode> episodes;

            try
            {
                var tuple = _seriesInfo.GetSeriesInfo(series.TvdbId);
                seriesInfo = tuple.Item1;
                episodes = tuple.Item2;
            }
            catch (SeriesNotFoundException)
            {
                if (series.Status != SeriesStatusType.Deleted)
                {
                    series.Status = SeriesStatusType.Deleted;
                    _seriesService.UpdateSeries(series, publishUpdatedEvent: false);
                    _logger.Debug("Series marked as deleted on tvdb for {0}", series.Title);
                    _eventAggregator.PublishEvent(new SeriesUpdatedEvent(series));
                }

                throw;
            }

            if (series.TvdbId != seriesInfo.TvdbId)
            {
                _logger.Warn("Series '{0}' (tvdbid {1}) was replaced with '{2}' (tvdbid {3}), because the original was a duplicate.", series.Title, series.TvdbId, seriesInfo.Title, seriesInfo.TvdbId);
                series.TvdbId = seriesInfo.TvdbId;
            }

            series.Title = seriesInfo.Title;
            series.Year = seriesInfo.Year;
            series.TitleSlug = seriesInfo.TitleSlug;
            series.TvRageId = seriesInfo.TvRageId;
            series.TvMazeId = seriesInfo.TvMazeId;
            series.ImdbId = seriesInfo.ImdbId;
            series.AirTime = seriesInfo.AirTime;
            series.Overview = seriesInfo.Overview;
            series.OriginalLanguage = seriesInfo.OriginalLanguage;
            series.Status = seriesInfo.Status;
            series.CleanTitle = seriesInfo.CleanTitle;
            series.SortTitle = seriesInfo.SortTitle;
            series.LastInfoSync = DateTime.UtcNow;
            series.Runtime = seriesInfo.Runtime;
            series.Images = seriesInfo.Images;
            series.Network = seriesInfo.Network;
            series.FirstAired = seriesInfo.FirstAired;
            series.LastAired = seriesInfo.LastAired;
            series.Ratings = seriesInfo.Ratings;
            series.Actors = seriesInfo.Actors;
            series.Genres = seriesInfo.Genres;
            series.Certification = seriesInfo.Certification;

            try
            {
                series.Path = new DirectoryInfo(series.Path).FullName;
                series.Path = series.Path.GetActualCasing();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Couldn't update series path for " + series.Path);
            }

            series.Seasons = UpdateSeasons(series, seriesInfo);

            _seriesService.UpdateSeries(series, publishUpdatedEvent: false);
            _refreshEpisodeService.RefreshEpisodeInfo(series, episodes);

            _logger.Debug("Finished series refresh for {0}", series.Title);
            _eventAggregator.PublishEvent(new SeriesUpdatedEvent(series));

            return series;
        }

        private List<Season> UpdateSeasons(Series series, Series seriesInfo)
        {
            var seasons = seriesInfo.Seasons.DistinctBy(s => s.SeasonNumber).ToList();

            foreach (var season in seasons)
            {
                var existingSeason = series.Seasons.FirstOrDefault(s => s.SeasonNumber == season.SeasonNumber);

                if (existingSeason == null)
                {
                    if (season.SeasonNumber == 0)
                    {
                        _logger.Debug("Ignoring season 0 for series [{0}] {1} by default", series.TvdbId, series.Title);
                        season.Monitored = false;
                        continue;
                    }

                    var monitorNewSeasons = series.MonitorNewItems == NewItemMonitorTypes.All;

                    _logger.Debug("New season ({0}) for series: [{1}] {2}, setting monitored to {3}", season.SeasonNumber, series.TvdbId, series.Title, monitorNewSeasons.ToString().ToLowerInvariant());
                    season.Monitored = monitorNewSeasons;
                }
                else
                {
                    season.Monitored = existingSeason.Monitored;
                }
            }

            return seasons;
        }

        private void RescanSeries(Series series, bool isNew, CommandTrigger trigger)
        {
            var rescanAfterRefresh = _configService.RescanAfterRefresh;

            if (isNew)
            {
                _logger.Trace("Forcing rescan of {0}. Reason: New series", series);
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.Never)
            {
                _logger.Trace("Skipping rescan of {0}. Reason: never rescan after refresh", series);
                _eventAggregator.PublishEvent(new SeriesScanSkippedEvent(series, SeriesScanSkippedReason.NeverRescanAfterRefresh));

                return;
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.AfterManual && trigger != CommandTrigger.Manual)
            {
                _logger.Trace("Skipping rescan of {0}. Reason: not after automatic scans", series);
                _eventAggregator.PublishEvent(new SeriesScanSkippedEvent(series, SeriesScanSkippedReason.RescanAfterManualRefreshOnly));

                return;
            }

            try
            {
                _diskScanService.Scan(series);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't rescan series {0}", series);
            }
        }

        private void UpdateTags(Series series)
        {
            _logger.Trace("Updating tags for {0}", series);

            var tagsAdded = new HashSet<int>();
            var tagsRemoved = new HashSet<int>();
            var changes = _autoTaggingService.GetTagChanges(series);

            foreach (var tag in changes.TagsToRemove)
            {
                if (series.Tags.Contains(tag))
                {
                    series.Tags.Remove(tag);
                    tagsRemoved.Add(tag);
                }
            }

            foreach (var tag in changes.TagsToAdd)
            {
                if (!series.Tags.Contains(tag))
                {
                    series.Tags.Add(tag);
                    tagsAdded.Add(tag);
                }
            }

            if (tagsAdded.Any() || tagsRemoved.Any())
            {
                _seriesService.UpdateSeries(series);
                _logger.Debug("Updated tags for '{0}'. Added: {1}, Removed: {2}", series.Title, tagsAdded.Count, tagsRemoved.Count);
            }
        }

        public void Execute(RefreshSeriesCommand message)
        {
            var trigger = message.Trigger;
            var isNew = message.IsNewSeries;
            _eventAggregator.PublishEvent(new SeriesRefreshStartingEvent(trigger == CommandTrigger.Manual));

            if (message.SeriesIds.Any())
            {
                foreach (var seriesId in message.SeriesIds)
                {
                    var series = _seriesService.GetSeries(seriesId);

                    try
                    {
                        series = RefreshSeriesInfo(seriesId);
                        UpdateTags(series);
                        RescanSeries(series, isNew, trigger);
                    }
                    catch (SeriesNotFoundException)
                    {
                        _logger.Error("Series '{0}' (tvdbid {1}) was not found, it may have been removed from TheTVDB.", series.Title, series.TvdbId);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Couldn't refresh info for {0}", series);
                        UpdateTags(series);
                        RescanSeries(series, isNew, trigger);
                        throw;
                    }
                }
            }
            else
            {
                var allSeries = _seriesService.GetAllSeries().OrderBy(c => c.SortTitle).ToList();

                foreach (var series in allSeries)
                {
                    var seriesLocal = series;
                    if (trigger == CommandTrigger.Manual || _checkIfSeriesShouldBeRefreshed.ShouldRefresh(seriesLocal))
                    {
                        try
                        {
                            seriesLocal = RefreshSeriesInfo(seriesLocal.Id);
                        }
                        catch (SeriesNotFoundException)
                        {
                            _logger.Error("Series '{0}' (tvdbid {1}) was not found, it may have been removed from TheTVDB.", seriesLocal.Title, seriesLocal.TvdbId);
                            continue;
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't refresh info for {0}", seriesLocal);
                        }

                        UpdateTags(series);
                        RescanSeries(seriesLocal, false, trigger);
                    }
                    else
                    {
                        _logger.Info("Skipping refresh of series: {0}", seriesLocal.Title);
                        UpdateTags(series);
                        RescanSeries(seriesLocal, false, trigger);
                    }
                }
            }

            _eventAggregator.PublishEvent(new SeriesRefreshCompleteEvent());
        }
    }
}
