using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Retention
{
    public interface IEpisodeRetentionService
    {
        EpisodeRetentionPlan Preview(int seriesId);
    }

    public class EpisodeRetentionService : IEpisodeRetentionService,
                                           IExecute<EpisodeRetentionCommand>,
                                           IHandle<SeriesScannedEvent>,
                                           IHandle<SeriesScanSkippedEvent>
    {
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IDeleteMediaFiles _deleteMediaFiles;
        private readonly IManageCommandQueue _commandQueue;
        private readonly IDiskProvider _diskProvider;
        private readonly IRootFolderService _rootFolderService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public EpisodeRetentionService(ISeriesService seriesService,
                                       IEpisodeService episodeService,
                                       IMediaFileService mediaFileService,
                                       IDeleteMediaFiles deleteMediaFiles,
                                       IManageCommandQueue commandQueue,
                                       IDiskProvider diskProvider,
                                       IRootFolderService rootFolderService,
                                       IEventAggregator eventAggregator,
                                       Logger logger)
        {
            _seriesService = seriesService;
            _episodeService = episodeService;
            _mediaFileService = mediaFileService;
            _deleteMediaFiles = deleteMediaFiles;
            _commandQueue = commandQueue;
            _diskProvider = diskProvider;
            _rootFolderService = rootFolderService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public EpisodeRetentionPlan Preview(int seriesId)
        {
            var series = _seriesService.GetSeries(seriesId);
            return BuildPlan(series, _episodeService.GetEpisodeBySeries(seriesId), _mediaFileService.GetFilesBySeries(seriesId));
        }

        public void Execute(EpisodeRetentionCommand message)
        {
            if (message.Trigger == CommandTrigger.Manual && (message.Files == null || message.EpisodeIds == null))
            {
                throw new NzbDroneClientException(HttpStatusCode.BadRequest, "Select the files and missing episodes to trim.");
            }

            var series = _seriesService.GetSeries(message.SeriesId);
            var episodes = _episodeService.GetEpisodeBySeries(series.Id);
            var files = _mediaFileService.GetFilesBySeries(series.Id);
            ValidateSelections(message, files, episodes);
            var plan = BuildPlan(series, episodes, files);

            if (message.Files != null)
            {
                var selectedFiles = message.Files.ToHashSet();
                plan.FilesToDelete = plan.FilesToDelete.Where(f => selectedFiles.Contains(f.Id)).ToList();
            }

            if (message.EpisodeIds != null)
            {
                var selectedEpisodes = message.EpisodeIds.ToHashSet();
                plan.OlderMissingEpisodesToUnmonitor = plan.OlderMissingEpisodesToUnmonitor.Where(e => selectedEpisodes.Contains(e.Id)).ToList();
            }

            var deletingIds = plan.FilesToDelete.Select(f => f.Id).ToHashSet();
            plan.EpisodesToUnmonitor = episodes.Where(e => deletingIds.Contains(e.EpisodeFileId) && e.Monitored).ToList();
            plan.EpisodesToUnmonitor.AddRange(plan.OlderMissingEpisodesToUnmonitor);
            Trim(series, plan);
        }

        private void ValidateSelections(EpisodeRetentionCommand message, List<EpisodeFile> files, List<Episode> episodes)
        {
            var fileIds = files.Select(f => f.Id).ToHashSet();
            foreach (var id in (message.Files ?? new List<int>()).Distinct().Where(id => !fileIds.Contains(id)))
            {
                try
                {
                    if (_mediaFileService.Get(id).SeriesId != message.SeriesId)
                    {
                        throw new NzbDroneClientException(HttpStatusCode.BadRequest, "Selected files must belong to the series.");
                    }
                }
                catch (ModelNotFoundException)
                {
                    // A selected file may have been removed or upgraded while the command waited.
                }
            }

            var episodeIds = episodes.Select(e => e.Id).ToHashSet();
            foreach (var id in (message.EpisodeIds ?? new List<int>()).Distinct().Where(id => !episodeIds.Contains(id)))
            {
                try
                {
                    if (_episodeService.GetEpisode(id).SeriesId != message.SeriesId)
                    {
                        throw new NzbDroneClientException(HttpStatusCode.BadRequest, "Selected episodes must belong to the series.");
                    }
                }
                catch (ModelNotFoundException)
                {
                    // Episode metadata may have been removed while the command waited.
                }
            }
        }

        private void Trim(Series series, EpisodeRetentionPlan plan)
        {
            if (!plan.Enabled || (!plan.FilesToDelete.Any() && !plan.EpisodesToUnmonitor.Any()))
            {
                _logger.Debug(
                    "Skipping episode retention for '{0}' (series {1}): {2}",
                    series.Title,
                    series.Id,
                    plan.Enabled ? "no changes needed" : "retention disabled");
                return;
            }

            var rootFolder = _rootFolderService.GetBestRootFolderPath(series.Path);
            if (!_diskProvider.FolderExists(rootFolder) || _diskProvider.FolderEmpty(rootFolder) || !_diskProvider.FolderExists(series.Path))
            {
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Series or root folder is unavailable. Wait for disk access before trimming episodes.");
            }

            _logger.Info(
                "Starting episode retention for '{0}' (series {1}): keep {2} episodes, delete {3} files, unmonitor {4} episodes",
                series.Title,
                series.Id,
                plan.KeepEpisodes,
                plan.FilesToDelete.Count,
                plan.EpisodesToUnmonitor.Count);

            var episodesByFileId = plan.Episodes.ToLookup(e => e.EpisodeFileId);
            var deletedFiles = 0;
            var unmonitoredEpisodes = 0;
            var unmonitoredMissingEpisodes = 0;

            try
            {
                foreach (var file in plan.FilesToDelete)
                {
                    var episodes = episodesByFileId[file.Id].ToList();
                    var monitoredCount = episodes.Count(e => e.Monitored);
                    _deleteMediaFiles.DeleteEpisodeFile(series, file, DeleteMediaFileReason.Retention);
                    deletedFiles++;

                    // Only unmonitor a file's episodes after its deletion succeeds.
                    _episodeService.SetMonitored(episodes.Select(e => e.Id), false);
                    unmonitoredEpisodes += monitoredCount;
                    PublishEpisodeUpdates(episodes);
                }

                if (plan.OlderMissingEpisodesToUnmonitor.Any())
                {
                    _episodeService.SetMonitored(plan.OlderMissingEpisodesToUnmonitor.Select(e => e.Id), false);
                    unmonitoredMissingEpisodes = plan.OlderMissingEpisodesToUnmonitor.Count;
                    unmonitoredEpisodes += unmonitoredMissingEpisodes;
                    PublishEpisodeUpdates(plan.OlderMissingEpisodesToUnmonitor);
                }
            }
            catch
            {
                _logger.Warn(
                    "Episode retention failed for '{0}' (series {1}): deleted {2}/{3} files, unmonitored {4}/{5} episodes ({6} older missing)",
                    series.Title,
                    series.Id,
                    deletedFiles,
                    plan.FilesToDelete.Count,
                    unmonitoredEpisodes,
                    plan.EpisodesToUnmonitor.Count,
                    unmonitoredMissingEpisodes);
                throw;
            }
            finally
            {
                if (deletedFiles > 0 || unmonitoredEpisodes > 0)
                {
                    _eventAggregator.PublishEvent(new ModelEvent<Series>(_seriesService.GetSeries(series.Id), ModelAction.Updated));
                }
            }

            _logger.Info(
                "Completed episode retention for '{0}' (series {1}): keep {2} episodes, deleted {3} files, unmonitored {4} episodes ({5} older missing)",
                series.Title,
                series.Id,
                plan.KeepEpisodes,
                deletedFiles,
                unmonitoredEpisodes,
                unmonitoredMissingEpisodes);
        }

        private void PublishEpisodeUpdates(IEnumerable<Episode> episodes)
        {
            foreach (var episode in episodes)
            {
                _eventAggregator.PublishEvent(new ModelEvent<Episode>(_episodeService.GetEpisode(episode.Id), ModelAction.Updated));
            }
        }

        private static EpisodeRetentionPlan BuildPlan(Series series, List<Episode> episodes, List<EpisodeFile> files)
        {
            if (series.RetentionKeepEpisodes < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(series.RetentionKeepEpisodes), "Keep at least one episode.");
            }

            var plan = new EpisodeRetentionPlan
            {
                SeriesId = series.Id,
                Enabled = series.RetentionEnabled,
                KeepEpisodes = series.RetentionKeepEpisodes,
                Episodes = episodes
            };

            if (!series.RetentionEnabled)
            {
                return plan;
            }

            var airDates = episodes.Select(e => new
            {
                e.Id,
                AirDate = DateTime.TryParseExact(e.AirDate, Episode.AIR_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? (DateTime?)date : null
            }).Where(e => e.AirDate.HasValue).ToDictionary(e => e.Id, e => e.AirDate.Value);
            var eligible = episodes.Where(e => airDates.ContainsKey(e.Id) && (series.RetentionIncludeSpecials || e.SeasonNumber != 0)).ToList();
            var episodesByFileId = episodes.ToLookup(e => e.EpisodeFileId);
            var fileIds = files.Select(f => f.Id).ToHashSet();
            var downloaded = eligible.Where(e => fileIds.Contains(e.EpisodeFileId))
                                     .OrderByDescending(e => airDates[e.Id])
                                     .ThenByDescending(e => e.SeasonNumber)
                                     .ThenByDescending(e => e.EpisodeNumber)
                                     .ThenByDescending(e => e.Id)
                                     .ToList();
            var older = downloaded.Skip(series.RetentionKeepEpisodes).Select(e => e.Id).ToHashSet();

            // Shared files containing retained, excluded, or undated episodes are protected.
            plan.FilesToDelete = files.Where(f => episodesByFileId.Contains(f.Id) && episodesByFileId[f.Id].All(e => older.Contains(e.Id)))
                                      .OrderBy(f => f.Id)
                                      .ToList();
            var deletingIds = plan.FilesToDelete.Select(f => f.Id).ToHashSet();
            plan.RetainedEpisodes = downloaded.Where(e => !deletingIds.Contains(e.EpisodeFileId)).ToList();
            plan.EpisodesToUnmonitor = episodes.Where(e => deletingIds.Contains(e.EpisodeFileId) && e.Monitored).ToList();

            if (downloaded.Any())
            {
                // Use the pre-trim boundary so gaps among downloaded episodes stay monitored.
                var oldestDownloadedDate = airDates[downloaded.Last().Id];
                plan.OlderMissingEpisodesToUnmonitor = eligible.Where(e => !e.HasFile && e.Monitored && airDates[e.Id] < oldestDownloadedDate).ToList();
                plan.EpisodesToUnmonitor.AddRange(plan.OlderMissingEpisodesToUnmonitor);
            }

            return plan;
        }

        [EventHandleOrder(EventHandleOrder.Last)]
        public void Handle(SeriesScannedEvent message)
        {
            Queue(message.Series.Id);
        }

        [EventHandleOrder(EventHandleOrder.Last)]
        public void Handle(SeriesScanSkippedEvent message)
        {
            if (message.Reason == SeriesScanSkippedReason.NeverRescanAfterRefresh ||
                message.Reason == SeriesScanSkippedReason.RescanAfterManualRefreshOnly)
            {
                Queue(message.Series.Id);
            }
        }

        private void Queue(int seriesId)
        {
            if (_seriesService.GetSeries(seriesId).RetentionEnabled)
            {
                _commandQueue.Push(new EpisodeRetentionCommand
                {
                    SeriesId = seriesId,
                    SuppressMessages = true
                });
            }
        }
    }
}
