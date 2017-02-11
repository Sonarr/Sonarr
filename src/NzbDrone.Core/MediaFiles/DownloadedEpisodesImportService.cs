using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDownloadedEpisodesImportService
    {
        List<ImportResult> ProcessRootFolder(DirectoryInfo directoryInfo);
        List<ImportResult> ProcessPath(string path, ImportMode importMode = ImportMode.Auto, Series series = null, DownloadClientItem downloadClientItem = null);
        bool ShouldDeleteFolder(DirectoryInfo directoryInfo, Series series);
    }

    public class DownloadedEpisodesImportService : IDownloadedEpisodesImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly ISeriesService _seriesService;
        private readonly IParsingService _parsingService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedEpisodes _importApprovedEpisodes;
        private readonly IDetectSample _detectSample;
        private readonly Logger _logger;

        public DownloadedEpisodesImportService(IDiskProvider diskProvider,
                                               IDiskScanService diskScanService,
                                               ISeriesService seriesService,
                                               IParsingService parsingService,
                                               IMakeImportDecision importDecisionMaker,
                                               IImportApprovedEpisodes importApprovedEpisodes,
                                               IDetectSample detectSample,
                                               Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _seriesService = seriesService;
            _parsingService = parsingService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedEpisodes = importApprovedEpisodes;
            _detectSample = detectSample;
            _logger = logger;
        }

        public List<ImportResult> ProcessRootFolder(DirectoryInfo directoryInfo)
        {
            var results = new List<ImportResult>();

            foreach (var subFolder in _diskProvider.GetDirectories(directoryInfo.FullName))
            {
                var folderResults = ProcessFolder(new DirectoryInfo(subFolder), ImportMode.Auto, null);
                results.AddRange(folderResults);
            }

            foreach (var videoFile in _diskScanService.GetVideoFiles(directoryInfo.FullName, false))
            {
                var fileResults = ProcessFile(new FileInfo(videoFile), ImportMode.Auto, null);
                results.AddRange(fileResults);
            }

            return results;
        }

        public List<ImportResult> ProcessPath(string path, ImportMode importMode = ImportMode.Auto, Series series = null, DownloadClientItem downloadClientItem = null)
        {
            if (_diskProvider.FolderExists(path))
            {
                var directoryInfo = new DirectoryInfo(path);

                if (series == null)
                {
                    return ProcessFolder(directoryInfo, importMode, downloadClientItem);
                }

                return ProcessFolder(directoryInfo, importMode, series, downloadClientItem);
            }

            if (_diskProvider.FileExists(path))
            {
                var fileInfo = new FileInfo(path);

                if (series == null)
                {
                    return ProcessFile(fileInfo, importMode, downloadClientItem);
                }

                return ProcessFile(fileInfo, importMode, series, downloadClientItem);
            }

            _logger.Error("Import failed, path does not exist or is not accessible by Sonarr: {0}", path);
            return new List<ImportResult>();
        }

        public bool ShouldDeleteFolder(DirectoryInfo directoryInfo, Series series)
        {
            var videoFiles = _diskScanService.GetVideoFiles(directoryInfo.FullName);
            var rarFiles = _diskProvider.GetFiles(directoryInfo.FullName, SearchOption.AllDirectories).Where(f => Path.GetExtension(f).Equals(".rar", StringComparison.OrdinalIgnoreCase));

            foreach (var videoFile in videoFiles)
            {
                var episodeParseResult = Parser.Parser.ParseTitle(Path.GetFileName(videoFile));

                if (episodeParseResult == null)
                {
                    _logger.Warn("Unable to parse file on import: [{0}]", videoFile);
                    return false;
                }

                if (_detectSample.IsSample(series, videoFile, episodeParseResult.IsPossibleSpecialEpisode) != DetectSampleResult.Sample)
                {
                    _logger.Warn("Non-sample file detected: [{0}]", videoFile);
                    return false;
                }
            }

            if (rarFiles.Any(f => _diskProvider.GetFileSize(f) > 10.Megabytes()))
            {
                _logger.Warn("RAR file detected, will require manual cleanup");
                return false;
            }

            return true;
        }

        private List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, ImportMode importMode, DownloadClientItem downloadClientItem)
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

            return ProcessFolder(directoryInfo, importMode, series, downloadClientItem);
        }

        private List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, ImportMode importMode, Series series, DownloadClientItem downloadClientItem)
        {
            if (_seriesService.SeriesPathExists(directoryInfo.FullName))
            {
                _logger.Warn("Unable to process folder that is mapped to an existing show");
                return new List<ImportResult>();
            }

            var folderInfo = Parser.Parser.ParseTitle(directoryInfo.Name);
            var videoFiles = _diskScanService.FilterFiles(directoryInfo.FullName, _diskScanService.GetVideoFiles(directoryInfo.FullName));

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

            var decisions = _importDecisionMaker.GetImportDecisions(videoFiles.ToList(), series, downloadClientItem, folderInfo, true);
            var importResults = _importApprovedEpisodes.Import(decisions, true, downloadClientItem, importMode);

            if (importMode == ImportMode.Auto)
            {
                importMode = (downloadClientItem == null || downloadClientItem.CanMoveFiles) ? ImportMode.Move : ImportMode.Copy;
            }

            if (importMode == ImportMode.Move &&
                importResults.Any(i => i.Result == ImportResultType.Imported) &&
                ShouldDeleteFolder(directoryInfo, series))
            {
                _logger.Debug("Deleting folder after importing valid files");
                _diskProvider.DeleteFolder(directoryInfo.FullName, true);
            }

            return importResults;
        }

        private List<ImportResult> ProcessFile(FileInfo fileInfo, ImportMode importMode, DownloadClientItem downloadClientItem)
        {
            var series = _parsingService.GetSeries(Path.GetFileNameWithoutExtension(fileInfo.Name));

            if (series == null)
            {
                _logger.Debug("Unknown Series for file: {0}", fileInfo.Name);

                return new List<ImportResult>
                       {
                           UnknownSeriesResult(string.Format("Unknown Series for file: {0}", fileInfo.Name), fileInfo.FullName)
                       };
            }

            return ProcessFile(fileInfo, importMode, series, downloadClientItem);
        }

        private List<ImportResult> ProcessFile(FileInfo fileInfo, ImportMode importMode, Series series, DownloadClientItem downloadClientItem)
        {
            if (Path.GetFileNameWithoutExtension(fileInfo.Name).StartsWith("._"))
            {
                _logger.Debug("[{0}] starts with '._', skipping", fileInfo.FullName);

                return new List<ImportResult>
                       {
                           new ImportResult(new ImportDecision(new LocalEpisode { Path = fileInfo.FullName }, new Rejection("Invalid video file, filename starts with '._'")), "Invalid video file, filename starts with '._'")
                       };
            }

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

            var decisions = _importDecisionMaker.GetImportDecisions(new List<string>() { fileInfo.FullName }, series, downloadClientItem, null, true);

            return _importApprovedEpisodes.Import(decisions, true, downloadClientItem, importMode);
        }

        private string GetCleanedUpFolderName(string folder)
        {
            folder = folder.Replace("_UNPACK_", "")
                           .Replace("_FAILED_", "");

            return folder;
        }

        private ImportResult FileIsLockedResult(string videoFile)
        {
            _logger.Debug("[{0}] is currently locked by another process, skipping", videoFile);
            return new ImportResult(new ImportDecision(new LocalEpisode { Path = videoFile }, new Rejection("Locked file, try again later")), "Locked file, try again later");
        }

        private ImportResult UnknownSeriesResult(string message, string videoFile = null)
        {
            var localEpisode = videoFile == null ? null : new LocalEpisode { Path = videoFile };

            return new ImportResult(new ImportDecision(localEpisode, new Rejection("Unknown Series")), message);
        }
    }
}
