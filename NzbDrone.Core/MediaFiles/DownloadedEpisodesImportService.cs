using System;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public class DownloadedEpisodesImportService : IExecute<DownloadedEpisodesScanCommand>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly ISeriesService _seriesService;
        private readonly IMoveEpisodeFiles _episodeFileMover;
        private readonly IParsingService _parsingService;
        private readonly IConfigService _configService;
        private readonly IMessageAggregator _messageAggregator;
        private readonly Logger _logger;

        public DownloadedEpisodesImportService(IDiskProvider diskProvider,
            IDiskScanService diskScanService,
            ISeriesService seriesService,
            IMoveEpisodeFiles episodeFileMover,
            IParsingService parsingService,
            IConfigService configService,
            IMessageAggregator messageAggregator,
            Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _seriesService = seriesService;
            _episodeFileMover = episodeFileMover;
            _parsingService = parsingService;
            _configService = configService;
            _messageAggregator = messageAggregator;
            _logger = logger;
        }

        public void ProcessDownloadedEpisodesFolder()
        {
            //TODO: We should also process the download client's category folder
            var downloadedEpisodesFolder = _configService.DownloadedEpisodesFolder;

            if (String.IsNullOrEmpty(downloadedEpisodesFolder))
            {
                _logger.Warn("Downloaded Episodes Folder is not configured");
                return;
            }

            foreach (var subfolder in _diskProvider.GetDirectories(downloadedEpisodesFolder))
            {
                try
                {
                    if (!_seriesService.SeriesPathExists(subfolder))
                    {
                        ProcessSubFolder(new DirectoryInfo(subfolder));

                        if (_diskProvider.GetFolderSize(subfolder) < 50.Megabytes())
                        {
                            _diskProvider.DeleteFolder(subfolder, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error has occurred while importing folder: " + subfolder, e);
                }
            }

            foreach (var videoFile in _diskScanService.GetVideoFiles(downloadedEpisodesFolder, false))
            {
                try
                {
                    var series = _parsingService.GetSeries(Path.GetFileNameWithoutExtension(videoFile));

                    if (series == null)
                    {
                        _logger.Trace("Unknown Series for file: {0}", videoFile);
                    }

                    ProcessVideoFile(videoFile, series);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("An error has occurred while importing video file" + videoFile, ex);
                }
            }
        }

        public void ProcessSubFolder(DirectoryInfo subfolderInfo)
        {
            var series = _parsingService.GetSeries(subfolderInfo.Name);

            if (series == null)
            {
                _logger.Trace("Unknown Series {0}", subfolderInfo.Name);
                return;
            }

            var files = _diskScanService.GetVideoFiles(subfolderInfo.FullName);

            foreach (var file in files)
            {
                ProcessVideoFile(file, series);
            }
        }

        private void ProcessVideoFile(string videoFile, Series series)
        {
            if (_diskProvider.IsFileLocked(new FileInfo(videoFile)))
            {
                _logger.Trace("[{0}] is currently locked by another process, skipping", videoFile);
                return;
            }

            var episodeFile = _diskScanService.ImportFile(series, videoFile);

            if (episodeFile != null)
            {
                _episodeFileMover.MoveEpisodeFile(episodeFile, true);
                _messageAggregator.PublishEvent(new EpisodeImportedEvent(episodeFile));
            }
        }

        public void Execute(DownloadedEpisodesScanCommand message)
        {
            ProcessDownloadedEpisodesFolder();
        }
    }
}