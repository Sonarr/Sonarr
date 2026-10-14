using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Retention
{
    public interface IEpisodeRetentionService
    {
        EpisodeRetentionPlan Preview(int seriesId);
        EpisodeRetentionPlan Trim(int seriesId, string previewToken = null);
    }

    public class EpisodeRetentionService : IEpisodeRetentionService,
                                           IExecute<EpisodeRetentionCommand>,
                                           IHandle<DownloadCompletedEvent>,
                                           IHandle<UntrackedDownloadCompletedEvent>,
                                           IHandle<EpisodeImportedEvent>
    {
        private static readonly object[] SeriesLocks = Enumerable.Range(0, 64).Select(_ => new object()).ToArray();
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IDeleteMediaFiles _deleteMediaFiles;
        private readonly IManageCommandQueue _commandQueue;
        private readonly Logger _logger;

        public EpisodeRetentionService(ISeriesService seriesService,
                                       IEpisodeService episodeService,
                                       IMediaFileService mediaFileService,
                                       IDeleteMediaFiles deleteMediaFiles,
                                       IManageCommandQueue commandQueue,
                                       Logger logger)
        {
            _seriesService = seriesService;
            _episodeService = episodeService;
            _mediaFileService = mediaFileService;
            _deleteMediaFiles = deleteMediaFiles;
            _commandQueue = commandQueue;
            _logger = logger;
        }

        public EpisodeRetentionPlan Preview(int seriesId)
        {
            return BuildPlan(_seriesService.GetSeries(seriesId));
        }

        public EpisodeRetentionPlan Trim(int seriesId, string previewToken = null)
        {
            lock (SeriesLocks[(int)((uint)seriesId % SeriesLocks.Length)])
            {
                if (_commandQueue.GetStarted().Any(c => c.Body.RequiresDiskAccess && !(c.Body is EpisodeRetentionCommand)))
                {
                    throw new NzbDroneClientException(HttpStatusCode.Conflict, "Wait for disk operations to finish before trimming episodes.");
                }

                var series = _seriesService.GetSeries(seriesId);
                var plan = BuildPlan(series);

                if (previewToken != null && !string.Equals(plan.PreviewToken, previewToken, StringComparison.Ordinal))
                {
                    throw new NzbDroneClientException(HttpStatusCode.Conflict, "Episode retention changed. Preview the trim again.");
                }

                if (!plan.Enabled || (!plan.FilesToDelete.Any() && !plan.EpisodesToUnmonitor.Any()))
                {
                    _logger.Debug(
                        "Skipping episode retention for '{0}' (series {1}): {2}",
                        series.Title,
                        series.Id,
                        plan.Enabled ? "no changes needed" : "retention disabled");
                    return plan;
                }

                _logger.Info(
                    "Starting episode retention for '{0}' (series {1}): keep {2} episodes, delete {3} files, unmonitor {4} episodes",
                    series.Title,
                    series.Id,
                    plan.KeepEpisodes,
                    plan.FilesToDelete.Count,
                    plan.EpisodesToUnmonitor.Count);

                var deletedFiles = 0;
                var unmonitoredEpisodes = 0;
                var unmonitoredMissingEpisodes = 0;

                try
                {
                    foreach (var file in plan.FilesToDelete)
                    {
                        var episodes = plan.Episodes.Where(e => e.EpisodeFileId == file.Id).ToList();
                        var monitoredCount = episodes.Count(e => e.Monitored);
                        _deleteMediaFiles.DeleteEpisodeFile(series, file, DeleteMediaFileReason.Retention);
                        deletedFiles++;

                        // Only unmonitor a file's episodes after its deletion succeeds.
                        _episodeService.SetMonitored(episodes.Select(e => e.Id), false);
                        unmonitoredEpisodes += monitoredCount;
                    }

                    if (plan.OlderMissingEpisodesToUnmonitor.Any())
                    {
                        _episodeService.SetMonitored(plan.OlderMissingEpisodesToUnmonitor.Select(e => e.Id), false);
                        unmonitoredMissingEpisodes = plan.OlderMissingEpisodesToUnmonitor.Count;
                        unmonitoredEpisodes += unmonitoredMissingEpisodes;
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

                _logger.Info(
                    "Completed episode retention for '{0}' (series {1}): keep {2} episodes, deleted {3} files, unmonitored {4} episodes ({5} older missing)",
                    series.Title,
                    series.Id,
                    plan.KeepEpisodes,
                    deletedFiles,
                    unmonitoredEpisodes,
                    unmonitoredMissingEpisodes);

                return plan;
            }
        }

        private EpisodeRetentionPlan BuildPlan(Series series)
        {
            if (series.RetentionKeepEpisodes < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(series.RetentionKeepEpisodes), "Keep at least one episode.");
            }

            var episodes = _episodeService.GetEpisodeBySeries(series.Id);
            var files = _mediaFileService.GetFilesBySeries(series.Id);
            var plan = new EpisodeRetentionPlan
            {
                SeriesId = series.Id,
                Enabled = series.RetentionEnabled,
                KeepEpisodes = series.RetentionKeepEpisodes,
                Episodes = episodes
            };

            if (series.RetentionEnabled)
            {
                var fileIds = files.Select(f => f.Id).ToHashSet();
                var downloaded = episodes.Where(e => fileIds.Contains(e.EpisodeFileId) && (series.RetentionIncludeSpecials || e.SeasonNumber != 0))
                                         .OrderByDescending(e => e.SeasonNumber)
                                         .ThenByDescending(e => e.EpisodeNumber)
                                         .ThenByDescending(e => e.Id)
                                         .ToList();
                var older = downloaded.Skip(series.RetentionKeepEpisodes).Select(e => e.Id).ToHashSet();

                // A shared file is indivisible. Every linked episode must be eligible for deletion.
                plan.FilesToDelete = files.Where(f => episodes.Any(e => e.EpisodeFileId == f.Id) &&
                                                     episodes.Where(e => e.EpisodeFileId == f.Id).All(e => older.Contains(e.Id)))
                                          .OrderBy(f => f.Id)
                                          .ToList();
                var deletingIds = plan.FilesToDelete.Select(f => f.Id).ToHashSet();
                plan.RetainedEpisodes = downloaded.Where(e => !deletingIds.Contains(e.EpisodeFileId)).ToList();
                plan.EpisodesToUnmonitor = episodes.Where(e => deletingIds.Contains(e.EpisodeFileId) && e.Monitored).ToList();

                if (downloaded.Any())
                {
                    // Match the script boundary before trimming so gaps among downloaded episodes stay monitored.
                    var oldestDownloaded = downloaded.Last();
                    plan.OlderMissingEpisodesToUnmonitor = episodes.Where(e => !e.HasFile && e.Monitored &&
                                                                              (series.RetentionIncludeSpecials || e.SeasonNumber != 0) &&
                                                                              e.CompareTo(oldestDownloaded) < 0)
                                                                  .ToList();
                    plan.EpisodesToUnmonitor.AddRange(plan.OlderMissingEpisodesToUnmonitor);
                }
            }

            var snapshot = JsonSerializer.SerializeToUtf8Bytes(new
            {
                series.Id,
                series.Path,
                series.RetentionEnabled,
                series.RetentionKeepEpisodes,
                series.RetentionIncludeSpecials,
                Files = files.OrderBy(f => f.Id).Select(f => new { f.Id, f.RelativePath, f.Size, f.DateAdded }),
                Episodes = episodes.OrderBy(e => e.Id).Select(e => new { e.Id, e.SeasonNumber, e.EpisodeNumber, e.EpisodeFileId, e.Monitored }),
                Retained = plan.RetainedEpisodes.Select(e => e.Id).OrderBy(id => id),
                Missing = plan.OlderMissingEpisodesToUnmonitor.Select(e => e.Id).OrderBy(id => id)
            });
            plan.PreviewToken = Convert.ToHexString(SHA256.HashData(snapshot));
            return plan;
        }

        public void Execute(EpisodeRetentionCommand message)
        {
            if (message.Trigger == CommandTrigger.Manual && string.IsNullOrWhiteSpace(message.PreviewToken))
            {
                throw new NzbDroneClientException(HttpStatusCode.BadRequest, "Preview the trim before deleting episodes.");
            }

            Trim(message.SeriesId, message.PreviewToken);
        }

        public void Handle(EpisodeImportedEvent message)
        {
            // Untracked folder scans do not publish a download-completed event. Disk commands
            // serialize this queued cleanup behind the scan, after its entire import batch.
            if (message.NewDownload && string.IsNullOrWhiteSpace(message.DownloadId) &&
                _commandQueue.GetStarted().Any(c => c.Body is DownloadedEpisodesScanCommand))
            {
                Queue(message.ImportedEpisode.SeriesId);
            }
        }

        public void Handle(DownloadCompletedEvent message)
        {
            Queue(message.SeriesId);
        }

        public void Handle(UntrackedDownloadCompletedEvent message)
        {
            Queue(message.Series.Id);
        }

        private void Queue(int seriesId)
        {
            if (_seriesService.GetSeries(seriesId).RetentionEnabled)
            {
                _commandQueue.Push(new EpisodeRetentionCommand { SeriesId = seriesId });
            }
        }
    }
}
