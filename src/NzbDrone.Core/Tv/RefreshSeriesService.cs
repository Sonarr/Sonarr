using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.Tvdb;
using NzbDrone.Core.MetadataSource.Tvdb.Resource;
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
        private readonly ICommandResultReporter _commandResultReporter;
        private readonly ITvdbApiClient _tvdbApiClient;
        private readonly Logger _logger;

        public RefreshSeriesService(IProvideSeriesInfo seriesInfo,
                                    ISeriesService seriesService,
                                    IRefreshEpisodeService refreshEpisodeService,
                                    IEventAggregator eventAggregator,
                                    IDiskScanService diskScanService,
                                    ICheckIfSeriesShouldBeRefreshed checkIfSeriesShouldBeRefreshed,
                                    IConfigService configService,
                                    ICommandResultReporter commandResultReporter,
                                    ITvdbApiClient tvdbApiClient,
                                    Logger logger)
        {
            _seriesInfo = seriesInfo;
            _seriesService = seriesService;
            _refreshEpisodeService = refreshEpisodeService;
            _eventAggregator = eventAggregator;
            _diskScanService = diskScanService;
            _checkIfSeriesShouldBeRefreshed = checkIfSeriesShouldBeRefreshed;
            _configService = configService;
            _commandResultReporter = commandResultReporter;
            _tvdbApiClient = tvdbApiClient;
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
            series.TmdbId = seriesInfo.TmdbId;
            series.ImdbId = seriesInfo.ImdbId;
            series.MalIds = seriesInfo.MalIds;
            series.AniListIds = seriesInfo.AniListIds;
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
            series.OriginalCountry = seriesInfo.OriginalCountry;

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

            // Apply alternative episode ordering from TVDB if configured
            var hadAlternativeOrdering = series.EpisodeOrder != EpisodeOrderType.Default ||
                                          (series.Seasons != null && series.Seasons.Any(s => s.EpisodeOrderOverride.HasValue));
            episodes = ApplyAlternativeOrdering(series, episodes);

            _seriesService.UpdateSeries(series, publishUpdatedEvent: false);
            _refreshEpisodeService.RefreshEpisodeInfo(series, episodes);

            if (hadAlternativeOrdering)
            {
                _logger.ProgressInfo(
                    "Episode ordering updated for {0}. Use Preview Rename to rename files to match the new numbering.",
                    series.Title);
            }

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
                    season.EpisodeOrderOverride = existingSeason.EpisodeOrderOverride;
                }
            }

            return seasons;
        }

        private List<Episode> ApplyAlternativeOrdering(Series series, List<Episode> episodes)
        {
            if (series.EpisodeOrder == EpisodeOrderType.Default &&
                (series.Seasons == null || series.Seasons.All(s => !s.EpisodeOrderOverride.HasValue)))
            {
                return episodes;
            }

            var tvdbApiKey = _configService.TvdbApiKey;
            if (string.IsNullOrWhiteSpace(tvdbApiKey))
            {
                _logger.Warn("Alternative episode ordering is configured for {0} but no TVDB API key is set. Using default ordering.", series.Title);
                return episodes;
            }

            // Build a lookup of which ordering each season should use
            var seasonOrderings = new Dictionary<int, EpisodeOrderType>();
            foreach (var season in series.Seasons)
            {
                var effectiveOrder = season.EpisodeOrderOverride ?? series.EpisodeOrder;
                if (effectiveOrder != EpisodeOrderType.Default)
                {
                    seasonOrderings[season.SeasonNumber] = effectiveOrder;
                }
            }

            if (!seasonOrderings.Any())
            {
                return episodes;
            }

            // Group seasons by ordering type to minimise API calls
            var orderingGroups = seasonOrderings.GroupBy(kv => kv.Value).ToList();

            foreach (var group in orderingGroups)
            {
                var orderType = group.Key;
                var seasonNumbers = new HashSet<int>(group.Select(kv => kv.Key));

                _logger.Debug(
                    "Fetching {0} ordering from TVDB for {1} seasons {2}",
                    orderType,
                    series.Title,
                    string.Join(", ", seasonNumbers.OrderBy(s => s)));

                List<TvdbEpisodeResource> tvdbEpisodes;

                try
                {
                    tvdbEpisodes = _tvdbApiClient.GetEpisodesByOrdering(series.TvdbId, orderType);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to fetch {0} ordering from TVDB for {1}. Using default ordering for affected seasons.", orderType, series.Title);
                    continue;
                }

                if (tvdbEpisodes == null || !tvdbEpisodes.Any())
                {
                    _logger.Warn("No episodes returned from TVDB for {0} ordering of {1}. Using default ordering.", orderType, series.Title);
                    continue;
                }

                // Build lookup: TVDB episode ID → alternative S/E numbers
                var tvdbLookup = tvdbEpisodes
                    .Where(e => e.SeasonNumber.HasValue && e.Number.HasValue)
                    .ToDictionary(e => e.Id, e => e);

                foreach (var episode in episodes)
                {
                    if (episode.TvdbId == 0 || !seasonOrderings.ContainsKey(episode.SeasonNumber))
                    {
                        continue;
                    }

                    // Only remap if this episode's current season uses this ordering type
                    if (seasonOrderings[episode.SeasonNumber] != orderType)
                    {
                        continue;
                    }

                    if (tvdbLookup.TryGetValue(episode.TvdbId, out var tvdbEpisode))
                    {
                        _logger.Trace(
                            "Remapping episode {0} (TvdbId {1}): S{2:00}E{3:00} → S{4:00}E{5:00} ({6})",
                            episode.Title,
                            episode.TvdbId,
                            episode.SeasonNumber,
                            episode.EpisodeNumber,
                            tvdbEpisode.SeasonNumber,
                            tvdbEpisode.Number,
                            orderType);

                        episode.SeasonNumber = tvdbEpisode.SeasonNumber.Value;
                        episode.EpisodeNumber = tvdbEpisode.Number.Value;

                        if (tvdbEpisode.AbsoluteNumber.HasValue)
                        {
                            episode.AbsoluteEpisodeNumber = tvdbEpisode.AbsoluteNumber;
                        }
                    }
                    else
                    {
                        _logger.Warn(
                            "Episode '{0}' (TvdbId {1}) not found in {2} ordering. Keeping default S{3:00}E{4:00}.",
                            episode.Title,
                            episode.TvdbId,
                            orderType,
                            episode.SeasonNumber,
                            episode.EpisodeNumber);
                    }
                }
            }

            return episodes;
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
            var tagsUpdated = _seriesService.UpdateTags(series);

            if (tagsUpdated)
            {
                _seriesService.UpdateSeries(series);
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

                        // Mark the result as indeterminate so it's not marked as a full success,
                        // // but we can still process other series if needed.
                        _commandResultReporter.Report(CommandResult.Indeterminate);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Couldn't refresh info for {0}", series);
                        UpdateTags(series);
                        RescanSeries(series, isNew, trigger);

                        // Mark the result as indeterminate so it's not marked as a full success,
                        // but we can still process other series if needed.
                        _commandResultReporter.Report(CommandResult.Indeterminate);
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
