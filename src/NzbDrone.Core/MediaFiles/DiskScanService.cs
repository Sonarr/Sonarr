using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDiskScanService
    {
        void Scan(Series series);
        string[] GetVideoFiles(string path, bool allDirectories = true);
        string[] GetNonVideoFiles(string path, bool allDirectories = true);
        List<string> FilterFiles(Series series, IEnumerable<string> files);
    }

    public class DiskScanService :
        IDiskScanService,
        IHandle<SeriesUpdatedEvent>,
        IExecute<RescanSeriesCommand>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedEpisodes _importApprovedEpisodes;
        private readonly IConfigService _configService;
        private readonly ISeriesService _seriesService;
        private readonly IMediaFileTableCleanupService _mediaFileTableCleanupService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public DiskScanService(IDiskProvider diskProvider,
                               IMakeImportDecision importDecisionMaker,
                               IImportApprovedEpisodes importApprovedEpisodes,
                               IConfigService configService,
                               ISeriesService seriesService,
                               IMediaFileTableCleanupService mediaFileTableCleanupService,
                               IEventAggregator eventAggregator,
                               Logger logger)
        {
            _diskProvider = diskProvider;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedEpisodes = importApprovedEpisodes;
            _configService = configService;
            _seriesService = seriesService;
            _mediaFileTableCleanupService = mediaFileTableCleanupService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private static readonly Regex ExcludedSubFoldersRegex = new Regex(@"(?:\\|\/|^)(extras|@eadir|extrafanart|plex\sversions|\..+)(?:\\|\/)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ExcludedFilesRegex = new Regex(@"^\._|Thumbs\.db", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void Scan(Series series)
        {
            var rootFolder = _diskProvider.GetParentFolder(series.Path);

            if (!_diskProvider.FolderExists(rootFolder))
            {
                _logger.Warn("Series' root folder ({0}) doesn't exist.", rootFolder);
                _eventAggregator.PublishEvent(new SeriesScanSkippedEvent(series, SeriesScanSkippedReason.RootFolderDoesNotExist));
                return;
            }

            if (_diskProvider.GetDirectories(rootFolder).Empty())
            {
                _logger.Warn("Series' root folder ({0}) is empty.", rootFolder);
                _eventAggregator.PublishEvent(new SeriesScanSkippedEvent(series, SeriesScanSkippedReason.RootFolderIsEmpty));
                return; 
            }

            _logger.ProgressInfo("Scanning disk for {0}", series.Title);
            
            if (!_diskProvider.FolderExists(series.Path))
            {
                if (_configService.CreateEmptySeriesFolders &&
                    _diskProvider.FolderExists(rootFolder))
                {
                    _logger.Debug("Creating missing series folder: {0}", series.Path);
                    _diskProvider.CreateFolder(series.Path);
                    SetPermissions(series.Path);
                }
                else
                {
                    _logger.Debug("Series folder doesn't exist: {0}", series.Path);
                }

                _eventAggregator.PublishEvent(new SeriesScanSkippedEvent(series, SeriesScanSkippedReason.SeriesFolderDoesNotExist));
                return;
            }

            var videoFilesStopwatch = Stopwatch.StartNew();
            var mediaFileList = FilterFiles(series, GetVideoFiles(series.Path)).ToList();

            videoFilesStopwatch.Stop();
            _logger.Trace("Finished getting episode files for: {0} [{1}]", series, videoFilesStopwatch.Elapsed);

            _logger.Debug("{0} Cleaning up media files in DB", series);
            _mediaFileTableCleanupService.Clean(series, mediaFileList);
            
            var decisionsStopwatch = Stopwatch.StartNew();
            var decisions = _importDecisionMaker.GetImportDecisions(mediaFileList, series);
            decisionsStopwatch.Stop();
            _logger.Trace("Import decisions complete for: {0} [{1}]", series, decisionsStopwatch.Elapsed);

            _importApprovedEpisodes.Import(decisions, false);

            _logger.Info("Completed scanning disk for {0}", series.Title);
            _eventAggregator.PublishEvent(new SeriesScannedEvent(series));
        }

        public string[] GetVideoFiles(string path, bool allDirectories = true)
        {
            _logger.Debug("Scanning '{0}' for video files", path);

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filesOnDisk = _diskProvider.GetFiles(path, searchOption);

            var mediaFileList = filesOnDisk.Where(file => MediaFileExtensions.Extensions.Contains(Path.GetExtension(file).ToLower()))
                                           .ToList();

            _logger.Debug("{0} video files were found in {1}", mediaFileList.Count, path);
            return mediaFileList.ToArray();
        }

        public string[] GetNonVideoFiles(string path, bool allDirectories = true)
        {
            _logger.Debug("Scanning '{0}' for non-video files", path);

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filesOnDisk = _diskProvider.GetFiles(path, searchOption);

            var mediaFileList = filesOnDisk.Where(file => !MediaFileExtensions.Extensions.Contains(Path.GetExtension(file).ToLower()))
                                           .ToList();

            _logger.Debug("{0} non-video files were found in {1}", mediaFileList.Count, path);
            return mediaFileList.ToArray();
        }

        public List<string> FilterFiles(Series series, IEnumerable<string> files)
        {
            return files.Where(file => !ExcludedSubFoldersRegex.IsMatch(series.Path.GetRelativePath(file)))
                        .Where(file => !ExcludedFilesRegex.IsMatch(Path.GetFileName(file)))
                        .ToList();
        }

        private void SetPermissions(string path)
        {
            if (!_configService.SetPermissionsLinux)
            {
                return;
            }

            try
            {
                var permissions = _configService.FolderChmod;
                _diskProvider.SetPermissions(path, permissions, _configService.ChownUser, _configService.ChownGroup);
            }

            catch (Exception ex)
            {

                _logger.Warn(ex, "Unable to apply permissions to: " + path);
                _logger.Debug(ex, ex.Message);
            }
        }       

        public void Handle(SeriesUpdatedEvent message)
        {
            Scan(message.Series);
        }

        public void Execute(RescanSeriesCommand message)
        {
            if (message.SeriesId.HasValue)
            {
                var series = _seriesService.GetSeries(message.SeriesId.Value);
                Scan(series);
            }

            else
            {
                var allSeries = _seriesService.GetAllSeries();

                foreach (var series in allSeries)
                {
                    Scan(series);
                }
            }
        }
    }
}