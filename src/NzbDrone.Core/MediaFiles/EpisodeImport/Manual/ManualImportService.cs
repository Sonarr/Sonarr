using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Manual
{
    public interface IManualImportService
    {
        List<ManualImportItem> GetMediaFiles(string path, string downloadId);
    }

    public class ManualImportService : IExecute<ManualImportCommand>, IManualImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IParsingService _parsingService;
        private readonly IDiskScanService _diskScanService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IImportApprovedEpisodes _importApprovedEpisodes;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDownloadedEpisodesImportService _downloadedEpisodesImportService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ManualImportService(IDiskProvider diskProvider,
                                   IParsingService parsingService,
                                   IDiskScanService diskScanService,
                                   IMakeImportDecision importDecisionMaker,
                                   ISeriesService seriesService,
                                   IEpisodeService episodeService,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   IImportApprovedEpisodes importApprovedEpisodes,
                                   ITrackedDownloadService trackedDownloadService,
                                   IDownloadedEpisodesImportService downloadedEpisodesImportService,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _diskProvider = diskProvider;
            _parsingService = parsingService;
            _diskScanService = diskScanService;
            _importDecisionMaker = importDecisionMaker;
            _seriesService = seriesService;
            _episodeService = episodeService;
            _videoFileInfoReader = videoFileInfoReader;
            _importApprovedEpisodes = importApprovedEpisodes;
            _trackedDownloadService = trackedDownloadService;
            _downloadedEpisodesImportService = downloadedEpisodesImportService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ManualImportItem> GetMediaFiles(string path, string downloadId)
        {
            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);

                if (trackedDownload == null)
                {
                    return new List<ManualImportItem>();
                }

                path = trackedDownload.DownloadItem.OutputPath.FullPath;
            }

            if (!_diskProvider.FolderExists(path))
            {
                if (!_diskProvider.FileExists(path))
                {
                    return new List<ManualImportItem>();
                }

                return new List<ManualImportItem> { ProcessFile(path, downloadId) };
            }

            return ProcessFolder(path, downloadId);
        }

        private List<ManualImportItem> ProcessFolder(string folder, string downloadId)
        {
            var directoryInfo = new DirectoryInfo(folder);
            var series = _parsingService.GetSeries(directoryInfo.Name);

            if (series == null && downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                series = trackedDownload.RemoteEpisode.Series;
            }

            if (series == null)
            {
                var files = _diskScanService.GetVideoFiles(folder);

                return files.Select(file => ProcessFile(file, downloadId, folder)).Where(i => i != null).ToList();
            }

            var folderInfo = Parser.Parser.ParseTitle(directoryInfo.Name);
            var seriesFiles = _diskScanService.GetVideoFiles(folder).ToList();
            var decisions = _importDecisionMaker.GetImportDecisions(seriesFiles, series, folderInfo, SceneSource(series, folder));

            return decisions.Select(decision => MapItem(decision, folder, downloadId)).ToList();
        }

        private ManualImportItem ProcessFile(string file, string downloadId, string folder = null)
        {
            if (folder.IsNullOrWhiteSpace())
            {
                folder = new FileInfo(file).Directory.FullName;
            }

            var relativeFile = folder.GetRelativePath(file);

            var series = _parsingService.GetSeries(relativeFile.Split('\\', '/')[0]);

            if (series == null)
            {
                series = _parsingService.GetSeries(relativeFile);
            }

            if (series == null && downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                series = trackedDownload.RemoteEpisode.Series;
            }

            if (series == null)
            {
                var localEpisode = new LocalEpisode();
                localEpisode.Path = file;
                localEpisode.Quality = QualityParser.ParseQuality(file);
                localEpisode.Size = _diskProvider.GetFileSize(file);

                return MapItem(new ImportDecision(localEpisode, new Rejection("Unknown Series")), folder, downloadId);
            }

            var importDecisions = _importDecisionMaker.GetImportDecisions(new List<string> {file},
                series, null, SceneSource(series, folder));

            return importDecisions.Any() ? MapItem(importDecisions.First(), folder, downloadId) : null;
        }

        private bool SceneSource(Series series, string folder)
        {
            return !(series.Path.PathEquals(folder) || series.Path.IsParentPath(folder));
        }

        private ManualImportItem MapItem(ImportDecision decision, string folder, string downloadId)
        {
            var item = new ManualImportItem();

            item.Path = decision.LocalEpisode.Path;
            item.RelativePath = folder.GetRelativePath(decision.LocalEpisode.Path);
            item.Name = Path.GetFileNameWithoutExtension(decision.LocalEpisode.Path);
            item.DownloadId = downloadId;

            if (decision.LocalEpisode.Series != null)
            {
                item.Series = decision.LocalEpisode.Series;
            }

            if (decision.LocalEpisode.Episodes.Any())
            {
                item.SeasonNumber = decision.LocalEpisode.SeasonNumber;
                item.Episodes = decision.LocalEpisode.Episodes;
            }

            item.Quality = decision.LocalEpisode.Quality;
            item.Size = _diskProvider.GetFileSize(decision.LocalEpisode.Path);
            item.Rejections = decision.Rejections;

            return item;
        }

        public void Execute(ManualImportCommand message)
        {
            _logger.ProgressTrace("Manually importing {0} files using mode {1}", message.Files.Count, message.ImportMode);

            var imported = new List<ImportResult>();
            var importedTrackedDownload = new List<ManuallyImportedFile>();

            for (int i = 0; i < message.Files.Count; i++)
            {
                _logger.ProgressTrace("Processing file {0} of {1}", i + 1, message.Files.Count);

                var file = message.Files[i];
                var series = _seriesService.GetSeries(file.SeriesId);
                var episodes = _episodeService.GetEpisodes(file.EpisodeIds);
                var parsedEpisodeInfo = Parser.Parser.ParsePath(file.Path) ?? new ParsedEpisodeInfo();
                var mediaInfo = _videoFileInfoReader.GetMediaInfo(file.Path);
                var existingFile = series.Path.IsParentPath(file.Path);

                var localEpisode = new LocalEpisode
                {
                    ExistingFile = false,
                    Episodes = episodes,
                    MediaInfo = mediaInfo,
                    ParsedEpisodeInfo = parsedEpisodeInfo,
                    Path = file.Path,
                    Quality = file.Quality,
                    Series = series,
                    Size = 0
                };

                //TODO: Cleanup non-tracked downloads

                var importDecision = new ImportDecision(localEpisode);

                if (file.DownloadId.IsNullOrWhiteSpace())
                {
                    imported.AddRange(_importApprovedEpisodes.Import(new List<ImportDecision> { importDecision }, !existingFile, null, message.ImportMode));
                }

                else
                {
                    var trackedDownload = _trackedDownloadService.Find(file.DownloadId);
                    var importResult = _importApprovedEpisodes.Import(new List<ImportDecision> { importDecision }, true, trackedDownload.DownloadItem, message.ImportMode).First();

                    imported.Add(importResult);

                    importedTrackedDownload.Add(new ManuallyImportedFile
                                                {
                                                    TrackedDownload = trackedDownload,
                                                    ImportResult = importResult
                                                });
                }
            }

            _logger.ProgressTrace("Manually imported {0} files", imported.Count);

            foreach (var groupedTrackedDownload in importedTrackedDownload.GroupBy(i => i.TrackedDownload.DownloadItem.DownloadId).ToList())
            {
                var trackedDownload = groupedTrackedDownload.First().TrackedDownload;

                if (_diskProvider.FolderExists(trackedDownload.DownloadItem.OutputPath.FullPath))
                {
                    if (_downloadedEpisodesImportService.ShouldDeleteFolder(
                            new DirectoryInfo(trackedDownload.DownloadItem.OutputPath.FullPath),
                            trackedDownload.RemoteEpisode.Series) && !trackedDownload.DownloadItem.IsReadOnly)
                    {
                        _diskProvider.DeleteFolder(trackedDownload.DownloadItem.OutputPath.FullPath, true);
                    }
                }

                if (groupedTrackedDownload.Select(c => c.ImportResult).Count(c => c.Result == ImportResultType.Imported) >= Math.Max(1, trackedDownload.RemoteEpisode.Episodes.Count))
                {
                    trackedDownload.State = TrackedDownloadStage.Imported;
                    _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload));
                }
            }
        }
    }
}
