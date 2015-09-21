using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Imports;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDownloadedMediaImportService
    {
        List<ImportResult> ProcessRootFolder(DirectoryInfo directoryInfo);
        List<ImportResult> ProcessPath(string path, Media media = null, DownloadClientItem downloadClientItem = null);
        bool ShouldDeleteFolder(DirectoryInfo directoryInfo, Media media);
    }

    public class DownloadedMediaImportService : IDownloadedMediaImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly ISeriesService _seriesService;
        private readonly IMovieService _movieService;
        private readonly IParsingService _parsingService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedItems _importApprovedItems;
        private readonly IDetectSample _detectSample;
        private readonly Logger _logger;

        public DownloadedMediaImportService(IDiskProvider diskProvider,
                                            IDiskScanService diskScanService,
                                            ISeriesService seriesService,
                                            IMovieService movieService,
                                            IParsingService parsingService,
                                            IMakeImportDecision importDecisionMaker,
                                            IImportApprovedItems importApprovedItems,
                                            IDetectSample detectSample,
                                            Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _seriesService = seriesService;
            _movieService = movieService;
            _parsingService = parsingService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedItems = importApprovedItems;
            _detectSample = detectSample;
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

        public List<ImportResult> ProcessPath(string path, Media media = null, DownloadClientItem downloadClientItem = null)
        {
            if (_diskProvider.FolderExists(path))
            {
                var directoryInfo = new DirectoryInfo(path);

                if (media == null)
                {
                    return ProcessFolder(directoryInfo, downloadClientItem);
                }

                return ProcessFolder(directoryInfo, media, downloadClientItem);
            }

            if (_diskProvider.FileExists(path))
            {
                var fileInfo = new FileInfo(path);

                if (media == null)
                {
                    return ProcessFile(fileInfo, downloadClientItem);
                }

                return ProcessFile(fileInfo, media, downloadClientItem);
            }

            _logger.Error("Import failed, path does not exist or is not accessible by Sonarr: {0}", path);
            return new List<ImportResult>();
        }

        public bool ShouldDeleteFolder(DirectoryInfo directoryInfo, Media media)
        {
            var videoFiles = _diskScanService.GetVideoFiles(directoryInfo.FullName);
            var rarFiles = _diskProvider.GetFiles(directoryInfo.FullName, SearchOption.AllDirectories).Where(f => Path.GetExtension(f) == ".rar");

            foreach (var videoFile in videoFiles)
            {
                ParsedInfo parsedInfo = null;

                if (media is Tv.Series)
                    parsedInfo = Parser.Parser.ParseTitle(Path.GetFileName(videoFile));
                if (media is Movie)
                    parsedInfo = Parser.Parser.ParseMovieTitle(Path.GetFileName(videoFile));

                if (parsedInfo == null)
                {
                    _logger.Warn("Unable to parse file on import: [{0}]", videoFile);
                    return false;
                }

                var size = _diskProvider.GetFileSize(videoFile);
                var quality = QualityParser.ParseQuality(videoFile);

                if (!_detectSample.IsSample(media, quality, videoFile, size,
                    parsedInfo))
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

        private List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, DownloadClientItem downloadClientItem = null)
        {
            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var series = _parsingService.GetSeries(cleanedUpName);
            var movie = _parsingService.GetMovie(cleanedUpName);
           
            if (series == null && movie == null)
            {
                _logger.Debug("Unknown series or movie {0}", cleanedUpName);

                return new List<ImportResult>
                       {
                           UnknownResult("Unknown Series or Movie")
                       };
            }

            Media media = (series != null) ? series as Media : movie as Media;

            return ProcessFolder(directoryInfo, media, downloadClientItem);
        }

        private List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, Media media,
                                                 DownloadClientItem downloadClientItem = null)
        {
            if (media is Tv.Series && _seriesService.SeriesPathExists(directoryInfo.FullName))
            {
                _logger.Warn("Unable to process folder that is mapped to an existing show");
                return new List<ImportResult>();
            }

            if (media is Movie && _movieService.MoviePathExists(directoryInfo.FullName))
            {
                _logger.Warn("Unable to process folder that is mapped to an existing movie");
                return new List<ImportResult>();
            }

            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);

            ParsedInfo folderInfo = null;
            if (media is Tv.Series)
                folderInfo = Parser.Parser.ParseTitle(directoryInfo.Name);
            if (media is Movie)
                folderInfo = Parser.Parser.ParseMovieTitle(directoryInfo.Name);

            if (folderInfo != null)
            {
                _logger.Debug("{0} folder quality: {1}", cleanedUpName, folderInfo.Quality);
            }

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

            var decisions = _importDecisionMaker.GetImportDecisions(videoFiles.ToList(), media, folderInfo, true);
            var importResults = _importApprovedItems.Import(decisions, true, downloadClientItem);

            if ((downloadClientItem == null || !downloadClientItem.IsReadOnly) &&
                importResults.Any(i => i.Result == ImportResultType.Imported) &&
                ShouldDeleteFolder(directoryInfo, media))
            {
                _logger.Debug("Deleting folder after importing valid files");
                _diskProvider.DeleteFolder(directoryInfo.FullName, true);
            }

            return importResults;
        }

        private List<ImportResult> ProcessFile(FileInfo fileInfo, DownloadClientItem downloadClientItem = null)
        {
            var series = _parsingService.GetSeries(Path.GetFileNameWithoutExtension(fileInfo.Name));
            var movie = _parsingService.GetMovie(Path.GetFileNameWithoutExtension(fileInfo.Name));

            if (series == null && movie == null)
            {
                _logger.Debug("Unknown Series or Movie for file: {0}", fileInfo.Name);

                return new List<ImportResult>
                       {
                           UnknownResult(String.Format("Unknown Series or Movie for file: {0}", fileInfo.Name), fileInfo.FullName)
                       };
            }
            Media media = (series != null) ? series as Media : movie as Media;

            return ProcessFile(fileInfo, media, downloadClientItem);
        }

        private List<ImportResult> ProcessFile(FileInfo fileInfo, Media media, DownloadClientItem downloadClientItem = null)
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

            var decisions = _importDecisionMaker.GetImportDecisions(new List<string>() { fileInfo.FullName }, media, null, true);

            return _importApprovedItems.Import(decisions, true, downloadClientItem);
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

        private ImportResult UnknownResult(string message, string videoFile = null)
        {
            var localEpisode = videoFile == null ? null : new LocalEpisode { Path = videoFile };

            return new ImportResult(new ImportDecision(localEpisode, new Rejection("Unknown Series or Movie")), message);
        }
    }
}
