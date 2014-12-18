using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDownloadedEpisodesImportService
    {
        List<ImportResult> ProcessRootFolder(DirectoryInfo directoryInfo);
        List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, DownloadClientItem downloadClientItem = null);
        List<ImportResult> ProcessPath(string path, DownloadClientItem downloadClientItem = null);
    }

    public class DownloadedEpisodesImportService : IDownloadedEpisodesImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly ISeriesService _seriesService;
        private readonly IParsingService _parsingService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedEpisodes _importApprovedEpisodes;
        private readonly ISampleService _sampleService;
        private readonly Logger _logger;

        public DownloadedEpisodesImportService(IDiskProvider diskProvider,
            IDiskScanService diskScanService,
            ISeriesService seriesService,
            IParsingService parsingService,
            IMakeImportDecision importDecisionMaker,
            IImportApprovedEpisodes importApprovedEpisodes,
            ISampleService sampleService,
            Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _seriesService = seriesService;
            _parsingService = parsingService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedEpisodes = importApprovedEpisodes;
            _sampleService = sampleService;
            _logger = logger;
        }

        public List<ImportResult> ProcessRootFolder(DirectoryInfo directoryInfo)
        {
            var results = new List<ImportResult>();

            foreach (var subFolder in _diskProvider.GetDirectories(directoryInfo.FullName))
            {
                var folderResults = ProcessFolder(new DirectoryInfo(subFolder));
                results.AddRange(folderResults);
            }

            foreach (var videoFile in _diskScanService.GetVideoFiles(directoryInfo.FullName, false))
            {
                var fileResults = ProcessFile(new FileInfo(videoFile));
                results.AddRange(fileResults);
            }

            return results;
        }

        public List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, DownloadClientItem downloadClientItem = null)
        {
            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var series = _parsingService.GetSeries(cleanedUpName);

            if (series == null)
            {
                _logger.Debug("Unknown Series {0}", cleanedUpName);

                return new List<ImportResult>
                       {
                           UnknownSeriesResult("Unknown Series")
                       };
            }

            return ProcessFolder(directoryInfo, series, downloadClientItem);
        }

        private List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, Series series,
                                                DownloadClientItem downloadClientItem = null)
        {
            if (_seriesService.SeriesPathExists(directoryInfo.FullName))
            {
                _logger.Warn("Unable to process folder that is mapped to an existing show");
                return new List<ImportResult>();
            }

            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var quality = QualityParser.ParseQuality(cleanedUpName);

            _logger.Debug("{0} folder quality: {1}", cleanedUpName, quality);

            var videoFiles = _diskScanService.GetVideoFiles(directoryInfo.FullName);

            if (downloadClientItem == null)
            {
                foreach (var videoFile in videoFiles)
                {
                    if (_diskProvider.IsFileLocked(videoFile))
                    {
                        return new List<ImportResult>
                               {
                                   FileIsLockedResult(videoFile)
                               };
                    }
                }
            }

            var decisions = _importDecisionMaker.GetImportDecisions(videoFiles.ToList(), series, true, quality);
            var importResults = _importApprovedEpisodes.Import(decisions, true, downloadClientItem);

            if ((downloadClientItem == null || !downloadClientItem.IsReadOnly) && importResults.Any() && ShouldDeleteFolder(directoryInfo, series))
            {
                _logger.Debug("Deleting folder after importing valid files");
                _diskProvider.DeleteFolder(directoryInfo.FullName, true);
            }

            return importResults;
        }

        public List<ImportResult> ProcessFile(FileInfo fileInfo, DownloadClientItem downloadClientItem = null)
        {
            var series = _parsingService.GetSeries(Path.GetFileNameWithoutExtension(fileInfo.Name));

            if (series == null)
            {
                _logger.Debug("Unknown Series for file: {0}", fileInfo.Name);

                return new List<ImportResult>
                       {
                           UnknownSeriesResult(String.Format("Unknown Series for file: {0}", fileInfo.Name), fileInfo.FullName)
                       };
            }

            return ProcessFile(fileInfo, series, downloadClientItem);
        }

        private List<ImportResult> ProcessFile(FileInfo fileInfo, Series series, DownloadClientItem downloadClientItem = null)
        {
            if (downloadClientItem == null)
            {
                if (_diskProvider.IsFileLocked(fileInfo.FullName))
                {
                    return new List<ImportResult>
                           {
                               FileIsLockedResult(fileInfo.FullName)
                           };
                }
            }

            var decisions = _importDecisionMaker.GetImportDecisions(new List<string>() { fileInfo.FullName }, series, true);
            return _importApprovedEpisodes.Import(decisions, true, downloadClientItem);
        }

        public List<ImportResult> ProcessPath(string path, DownloadClientItem downloadClientItem = null)
        {
            if (_diskProvider.FolderExists(path))
            {
                return ProcessFolder(new DirectoryInfo(path), downloadClientItem);
            }
        
            return ProcessFile(new FileInfo(path), downloadClientItem);
        }

        private string GetCleanedUpFolderName(string folder)
        {
            folder = folder.Replace("_UNPACK_", "")
                           .Replace("_FAILED_", "");

            return folder;
        }

        private bool ShouldDeleteFolder(DirectoryInfo directoryInfo, Series series)
        {
            var videoFiles = _diskScanService.GetVideoFiles(directoryInfo.FullName);

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

        private ImportResult FileIsLockedResult(string videoFile)
        {
            _logger.Debug("[{0}] is currently locked by another process, skipping", videoFile);
            return new ImportResult(new ImportDecision(new LocalEpisode { Path = videoFile }, "Locked file, try again later"), "Locked file, try again later");
        }

        private ImportResult UnknownSeriesResult(string message, string videoFile = null)
        {
            var localEpisode = videoFile == null ? null : new LocalEpisode { Path = videoFile };

            return new ImportResult(new ImportDecision(localEpisode, "Unknown Series"), message);
        }
    }
}
