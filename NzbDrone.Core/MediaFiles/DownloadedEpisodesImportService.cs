using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public class DownloadedEpisodesImportService : IExecute<DownloadedEpisodesScanCommand>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly ISeriesService _seriesService;
        private readonly IParsingService _parsingService;
        private readonly IConfigService _configService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedEpisodes _importApprovedEpisodes;
        private readonly Logger _logger;

        public DownloadedEpisodesImportService(IDiskProvider diskProvider,
            IDiskScanService diskScanService,
            ISeriesService seriesService,
            IParsingService parsingService,
            IConfigService configService,
            IMakeImportDecision importDecisionMaker,
            IImportApprovedEpisodes importApprovedEpisodes,
            Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _seriesService = seriesService;
            _parsingService = parsingService;
            _configService = configService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedEpisodes = importApprovedEpisodes;
            _logger = logger;
        }

        private void ProcessDownloadedEpisodesFolder()
        {
            //TODO: We should also process the download client's category folder
            var downloadedEpisodesFolder = _configService.DownloadedEpisodesFolder;

            if (String.IsNullOrEmpty(downloadedEpisodesFolder))
            {
                _logger.Warn("Drone Factory folder is not configured");
                return;
            }

            if (!_diskProvider.FolderExists(downloadedEpisodesFolder))
            {
                _logger.Warn("Drone Factory folder [{0}] doesn't exist.", downloadedEpisodesFolder);
                return;
            }

            foreach (var subFolder in _diskProvider.GetDirectories(downloadedEpisodesFolder))
            {
                try
                {
                    if (_seriesService.SeriesPathExists(subFolder))
                    {
                        continue;
                    }

                    var importedFiles = ProcessSubFolder(new DirectoryInfo(subFolder));

                    if (importedFiles.Any())
                    {
                        if (_diskProvider.GetFolderSize(subFolder) < NotSampleSpecification.SampleSizeLimit)
                        {
                            _diskProvider.DeleteFolder(subFolder, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error has occurred while importing folder: " + subFolder, e);
                }
            }

            foreach (var videoFile in _diskScanService.GetVideoFiles(downloadedEpisodesFolder, false))
            {
                try
                {
                    ProcessVideoFile(videoFile);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("An error has occurred while importing video file" + videoFile, ex);
                }
            }
        }

        private List<ImportDecision> ProcessSubFolder(DirectoryInfo subfolderInfo)
        {
            var cleanedUpName = GetCleanedUpFolderName(subfolderInfo.Name);
            var series = _parsingService.GetSeries(cleanedUpName);

            if (series == null)
            {
                _logger.Debug("Unknown Series {0}", cleanedUpName);
                return new List<ImportDecision>();
            }

            var videoFiles = _diskScanService.GetVideoFiles(subfolderInfo.FullName);

            return ProcessFiles(videoFiles, series);
        }

        private void ProcessVideoFile(string videoFile)
        {
            var series = _parsingService.GetSeries(Path.GetFileNameWithoutExtension(videoFile));

            if (series == null)
            {
                _logger.Debug("Unknown Series for file: {0}", videoFile);
                return;
            }

            if (_diskProvider.IsFileLocked(new FileInfo(videoFile)))
            {
                _logger.Debug("[{0}] is currently locked by another process, skipping", videoFile);
                return;
            }

            ProcessFiles(new[] { videoFile }, series);
        }

        private List<ImportDecision> ProcessFiles(IEnumerable<string> videoFiles, Series series)
        {
            var decisions = _importDecisionMaker.GetImportDecisions(videoFiles, series, true);
            return _importApprovedEpisodes.Import(decisions, true);
        }

        private string GetCleanedUpFolderName(string folder)
        {
            folder = folder.Replace("_UNPACK_", "")
                           .Replace("_FAILED_", "");

            return folder;
        }

        public void Execute(DownloadedEpisodesScanCommand message)
        {
            ProcessDownloadedEpisodesFolder();
        }
    }
}