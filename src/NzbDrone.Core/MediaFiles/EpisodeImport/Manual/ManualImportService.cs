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
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation;
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
        List<ManualImportItem> GetMediaFiles(string path, string downloadId, bool filterExistingFiles);
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
        private readonly IAugmentingService _augmentingService;
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
                                   IAugmentingService augmentingService,
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
            _augmentingService = augmentingService;
            _importApprovedEpisodes = importApprovedEpisodes;
            _trackedDownloadService = trackedDownloadService;
            _downloadedEpisodesImportService = downloadedEpisodesImportService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ManualImportItem> GetMediaFiles(string path, string downloadId, bool filterExistingFiles)
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

                var rootFolder = Path.GetDirectoryName(path);
                return new List<ManualImportItem> { ProcessFile(rootFolder, rootFolder, path, downloadId) };
            }

            return ProcessFolder(path, path, downloadId, filterExistingFiles);
        }

        private List<ManualImportItem> ProcessFolder(string rootFolder, string baseFolder, string downloadId, bool filterExistingFiles)
        {
            DownloadClientItem downloadClientItem = null;
            var directoryInfo = new DirectoryInfo(baseFolder);
            var series = _parsingService.GetSeries(directoryInfo.Name);

            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                downloadClientItem = trackedDownload.DownloadItem;

                if (series == null)
                {
                    series = trackedDownload.RemoteEpisode.Series;
                }
            }

            if (series == null)
            {
                var files = _diskScanService.FilterFiles(baseFolder, _diskScanService.GetVideoFiles(baseFolder, false));
                var subfolders = _diskScanService.FilterFiles(baseFolder, _diskProvider.GetDirectories(baseFolder));

                var processedFiles = files.Select(file => ProcessFile(rootFolder, baseFolder, file, downloadId));
                var processedFolders = subfolders.SelectMany(subfolder => ProcessFolder(rootFolder, subfolder, downloadId, filterExistingFiles));

                return processedFiles.Concat(processedFolders).Where(i => i != null).ToList();
            }

            var folderInfo = Parser.Parser.ParseTitle(directoryInfo.Name);
            var seriesFiles = _diskScanService.GetVideoFiles(baseFolder).ToList();
            var decisions = _importDecisionMaker.GetImportDecisions(seriesFiles, series, downloadClientItem, folderInfo, SceneSource(series, baseFolder), filterExistingFiles);

            return decisions.Select(decision => MapItem(decision, rootFolder, downloadId, directoryInfo.Name)).ToList();
        }

        private ManualImportItem ProcessFile(string rootFolder, string baseFolder, string file, string downloadId)
        {
            DownloadClientItem downloadClientItem = null;
            var relativeFile = baseFolder.GetRelativePath(file);
            var series = _parsingService.GetSeries(relativeFile.Split('\\', '/')[0]);

            if (series == null)
            {
                series = _parsingService.GetSeries(relativeFile);
            }

            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                downloadClientItem = trackedDownload.DownloadItem;

                if (series == null)
                {
                    series = trackedDownload.RemoteEpisode.Series;
                }
            }

            if (series == null)
            {
                var relativeParseInfo = Parser.Parser.ParsePath(relativeFile);

                if (relativeParseInfo != null)
                {
                    series = _seriesService.FindByTitle(relativeParseInfo.SeriesTitle);
                }
            }

            if (series == null)
            {
                var localEpisode = new LocalEpisode();
                localEpisode.Path = file;
                localEpisode.Quality = QualityParser.ParseQuality(file);
                localEpisode.Language = LanguageParser.ParseLanguage(file);
                localEpisode.Size = _diskProvider.GetFileSize(file);

                return MapItem(new ImportDecision(localEpisode, new Rejection("Unknown Series")), rootFolder, downloadId, null);
            }

            var importDecisions = _importDecisionMaker.GetImportDecisions(new List<string> {file},
                series, downloadClientItem, null, SceneSource(series, baseFolder));

            if (importDecisions.Any())
            {
                return MapItem(importDecisions.First(), rootFolder, downloadId, null);
            }

            return new ManualImportItem
            {
                DownloadId = downloadId,
                Path = file,
                RelativePath = rootFolder.GetRelativePath(file),
                Name = Path.GetFileNameWithoutExtension(file),
                Rejections = new List<Rejection>()
            };
        }

        private bool SceneSource(Series series, string folder)
        {
            return !(series.Path.PathEquals(folder) || series.Path.IsParentPath(folder));
        }

        private ManualImportItem MapItem(ImportDecision decision, string rootFolder, string downloadId, string folderName)
        {
            var item = new ManualImportItem();

            item.Path = decision.LocalEpisode.Path;
            item.FolderName = folderName;
            item.RelativePath = rootFolder.GetRelativePath(decision.LocalEpisode.Path);
            item.Name = Path.GetFileNameWithoutExtension(decision.LocalEpisode.Path);
            item.DownloadId = downloadId;

            if (decision.LocalEpisode.Series != null)
            {
                item.Series = decision.LocalEpisode.Series;
            }

            if (decision.LocalEpisode.Episodes.Any() && decision.LocalEpisode.Episodes.Select(c => c.SeasonNumber).Distinct().Count() == 1)
            {
                var seasons = decision.LocalEpisode.Episodes.Select(c => c.SeasonNumber).Distinct().ToList();

                if (seasons.Empty())
                {
                    _logger.Warn("Expected one season, but found none for: {0}", decision.LocalEpisode.Path);
                }
                else if (seasons.Count > 1)
                {
                    _logger.Warn("Expected one season, but found {0} ({1}) for: {2}", seasons.Count, string.Join(", ", seasons), decision.LocalEpisode.Path);
                }
                else
                {
                    item.SeasonNumber = decision.LocalEpisode.SeasonNumber;
                    item.Episodes = decision.LocalEpisode.Episodes;
                }
            }

            item.Quality = decision.LocalEpisode.Quality;
            item.Language = decision.LocalEpisode.Language;
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
                var fileEpisodeInfo = Parser.Parser.ParsePath(file.Path) ?? new ParsedEpisodeInfo();
                var existingFile = series.Path.IsParentPath(file.Path);
                TrackedDownload trackedDownload = null;

                var localEpisode = new LocalEpisode
                {
                    ExistingFile = false,
                    Episodes = episodes,
                    FileEpisodeInfo = fileEpisodeInfo,
                    Path = file.Path,
                    Quality = file.Quality,
                    Language = file.Language,
                    Series = series,
                    Size = 0
                };

                if (file.DownloadId.IsNotNullOrWhiteSpace())
                {
                    trackedDownload = _trackedDownloadService.Find(file.DownloadId);
                    if (trackedDownload != null)
                    {
                        localEpisode.DownloadClientEpisodeInfo = trackedDownload.RemoteEpisode.ParsedEpisodeInfo;
                    }
                }

                if (file.FolderName.IsNotNullOrWhiteSpace())
                {
                    localEpisode.FolderEpisodeInfo = Parser.Parser.ParseTitle(file.FolderName);
                }

                localEpisode = _augmentingService.Augment(localEpisode, false);

                // Apply the user-chosen values.
                localEpisode.Series = series;
                localEpisode.Episodes = episodes;
                localEpisode.Quality = file.Quality;

                //TODO: Cleanup non-tracked downloads

                var importDecision = new ImportDecision(localEpisode);

                if (trackedDownload == null)
                {
                    imported.AddRange(_importApprovedEpisodes.Import(new List<ImportDecision> { importDecision }, !existingFile, null, message.ImportMode));
                }
                else
                {
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
                            trackedDownload.RemoteEpisode.Series) && trackedDownload.DownloadItem.CanMoveFiles)
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
