using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDownloadedEpisodesImportService
    {
        List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, DownloadClientItem downloadClientItem);
        List<ImportResult> ProcessFile(FileInfo fileInfo, DownloadClientItem downloadClientItem);
    }

    public class DownloadedEpisodesImportService : IDownloadedEpisodesImportService, IExecute<DownloadedEpisodesScanCommand>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly ISeriesService _seriesService;
        private readonly IParsingService _parsingService;
        private readonly IConfigService _configService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedEpisodes _importApprovedEpisodes;
        private readonly ISampleService _sampleService;
        private readonly Logger _logger;

        public DownloadedEpisodesImportService(IDiskProvider diskProvider,
            IDiskScanService diskScanService,
            ISeriesService seriesService,
            IParsingService parsingService,
            IConfigService configService,
            IMakeImportDecision importDecisionMaker,
            IImportApprovedEpisodes importApprovedEpisodes,
            ISampleService sampleService,
            Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _seriesService = seriesService;
            _parsingService = parsingService;
            _configService = configService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedEpisodes = importApprovedEpisodes;
            _sampleService = sampleService;
            _logger = logger;
        }

        public List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, DownloadClientItem downloadClientItem)
        {
            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var series = _parsingService.GetSeries(cleanedUpName);
            var quality = QualityParser.ParseQuality(cleanedUpName);
            _logger.Debug("{0} folder quality: {1}", cleanedUpName, quality);

            if (series == null)
            {
                _logger.Debug("Unknown Series {0}", cleanedUpName);
                return new List<ImportResult>();
            }

            var videoFiles = _diskScanService.GetVideoFiles(directoryInfo.FullName);
            var decisions = _importDecisionMaker.GetImportDecisions(videoFiles.ToList(), series, true, quality);

            var importResults = _importApprovedEpisodes.Import(decisions, true, downloadClientItem);

            if (!downloadClientItem.IsReadOnly && importResults.Any() && ShouldDeleteFolder(directoryInfo))
            {
                _logger.Debug("Deleting folder after importing valid files");
                _diskProvider.DeleteFolder(directoryInfo.FullName, true);
            }

            return importResults;
        }

        public List<ImportResult> ProcessFile(FileInfo fileInfo, DownloadClientItem downloadClientItem)
        {
            var series = _parsingService.GetSeries(Path.GetFileNameWithoutExtension(fileInfo.Name));

            if (series == null)
            {
                _logger.Debug("Unknown Series for file: {0}", fileInfo.Name);
                return new List<ImportResult>();
            }

            var decisions = _importDecisionMaker.GetImportDecisions(new List<string>() { fileInfo.FullName }, series, true);
            return _importApprovedEpisodes.Import(decisions, true, downloadClientItem);
        }

        private void ProcessDownloadedEpisodesFolder()
        {
            var downloadedEpisodesFolder = _configService.DownloadedEpisodesFolder;

            if (String.IsNullOrEmpty(downloadedEpisodesFolder))
            {
                _logger.Trace("Drone Factory folder is not configured");
                return;
            }

            if (!_diskProvider.FolderExists(downloadedEpisodesFolder))
            {
                _logger.Warn("Drone Factory folder [{0}] doesn't exist.", downloadedEpisodesFolder);
                return;
            }

            foreach (var subFolder in _diskProvider.GetDirectories(downloadedEpisodesFolder))
            {
                ProcessFolder(subFolder);
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

        private List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo)
        {
            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var series = _parsingService.GetSeries(cleanedUpName);
            var quality = QualityParser.ParseQuality(cleanedUpName);
            _logger.Debug("{0} folder quality: {1}", cleanedUpName, quality);

            if (series == null)
            {
                _logger.Debug("Unknown Series {0}", cleanedUpName);
                return new List<ImportResult>();
            }

            var videoFiles = _diskScanService.GetVideoFiles(directoryInfo.FullName);

            foreach (var videoFile in videoFiles)
            {
                if (_diskProvider.IsFileLocked(videoFile))
                {
                    _logger.Debug("[{0}] is currently locked by another process, skipping", videoFile);
                    return new List<ImportResult>();
                }
            }

            var decisions = _importDecisionMaker.GetImportDecisions(videoFiles.ToList(), series, true, quality);
            return _importApprovedEpisodes.Import(decisions, true);
        }

        private void ProcessVideoFile(string videoFile)
        {
            var series = _parsingService.GetSeries(Path.GetFileNameWithoutExtension(videoFile));

            if (series == null)
            {
                _logger.Debug("Unknown Series for file: {0}", videoFile);
                return;
            }

            if (_diskProvider.IsFileLocked(videoFile))
            {
                _logger.Debug("[{0}] is currently locked by another process, skipping", videoFile);
                return;
            }

            var decisions = _importDecisionMaker.GetImportDecisions(new [] { videoFile }.ToList(), series, true, null);
            _importApprovedEpisodes.Import(decisions, true);
        }

        private void ProcessFolder(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            try
            {
                if (_seriesService.SeriesPathExists(path))
                {
                    _logger.Warn("Unable to process folder that contains sorted TV Shows");
                    return;
                }

                var directoryFolderInfo = new DirectoryInfo(path);
                var importedFiles = ProcessFolder(directoryFolderInfo);
                
                if (importedFiles.Any() && ShouldDeleteFolder(directoryFolderInfo))
                {
                    _logger.Debug("Deleting folder after importing valid files");
                    _diskProvider.DeleteFolder(path, true);
                }
            }
            catch (Exception e)
            {
                _logger.ErrorException("An error has occurred while importing folder: " + path, e);
            }
        }

        private string GetCleanedUpFolderName(string folder)
        {
            folder = folder.Replace("_UNPACK_", "")
                           .Replace("_FAILED_", "");

            return folder;
        }

        private bool ShouldDeleteFolder(DirectoryInfo directoryInfo)
        {
            var videoFiles = _diskScanService.GetVideoFiles(directoryInfo.FullName);
            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var series = _parsingService.GetSeries(cleanedUpName);

            foreach (var videoFile in videoFiles)
            {
                var episodeParseResult = Parser.Parser.ParseTitle(Path.GetFileName(videoFile));

                if (episodeParseResult == null)
                {
                    _logger.Warn("Unable to parse file on import: [{0}]", videoFile);
                    return false;
                }

                var size = _diskProvider.GetFileSize(videoFile);
                var quality = QualityParser.ParseQuality(videoFile);

                if (!_sampleService.IsSample(series, quality, videoFile, size,
                    episodeParseResult.SeasonNumber))
                {
                    _logger.Warn("Non-sample file detected: [{0}]", videoFile);
                    return false;
                }
            }

            return true;
        }

        public void Execute(DownloadedEpisodesScanCommand message)
        {
            if (message.Path.IsNullOrWhiteSpace())
            {
                ProcessDownloadedEpisodesFolder();
            }

            else
            {
                ProcessFolder(message.Path);
            }
        }
    }
}