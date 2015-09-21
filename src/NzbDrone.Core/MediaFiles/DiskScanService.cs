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
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Commands.Series;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.Imports;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDiskScanService
    {
        void Scan(Media media);
        string[] GetVideoFiles(string path, bool allDirectories = true);
    }

    public class DiskScanService :
        IDiskScanService,
        IHandle<SeriesUpdatedEvent>,
        IHandle<MovieUpdatedEvent>,
        IExecute<RescanSeriesCommand>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedItems _importApprovedItems;
        private readonly IConfigService _configService;
        private readonly ISeriesService _seriesService;
        private readonly IMovieService _movieService;
        private readonly IMediaFileTableCleanupService _mediaFileTableCleanupService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public DiskScanService(IDiskProvider diskProvider,
                               IMakeImportDecision importDecisionMaker,
                               IImportApprovedItems importApprovedItems,
                               IConfigService configService,
                               ISeriesService seriesService,
                                IMovieService movieService,
                               IMediaFileTableCleanupService mediaFileTableCleanupService,
                               IEventAggregator eventAggregator,
                               Logger logger)
        {
            _diskProvider = diskProvider;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedItems = importApprovedItems;
            _configService = configService;
            _seriesService = seriesService;
            _movieService = movieService;
            _mediaFileTableCleanupService = mediaFileTableCleanupService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private static readonly Regex ExcludedSubFoldersRegex = new Regex(@"(?:\\|\/|^)(extras|@eadir|\..+)(?:\\|\/)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ExcludedFilesRegex = new Regex(@"^\._", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void Scan(Media media)
        {
            var rootFolder = _diskProvider.GetParentFolder(media.Path);

            if (!_diskProvider.FolderExists(rootFolder))
            {
                _logger.Warn("Movie' root folder ({0}) doesn't exist.", rootFolder);
                _eventAggregator.PublishEvent(GetScanSkippedEvent(media, ScanSkippedReason.RootFolderDoesNotExist));
                return;
            }

            if (_diskProvider.GetDirectories(rootFolder).Empty())
            {
                _logger.Warn("Movie' root folder ({0}) is empty.", rootFolder);
                _eventAggregator.PublishEvent(GetScanSkippedEvent(media, ScanSkippedReason.RootFolderIsEmpty));
                return;
            }

            _logger.ProgressInfo("Scanning disk for {0}", media.Title);


            if (!_diskProvider.FolderExists(media.Path))
            {
                if (CanCreateFolder(media) && _diskProvider.FolderExists(rootFolder))
                {
                    _logger.Debug("Creating missing media folder: {0}", media.Path);
                    _diskProvider.CreateFolder(media.Path);
                    SetPermissions(media.Path);
                }
                else
                {
                    _logger.Debug("Media folder doesn't exist: {0}", media.Path);
                }

                _eventAggregator.PublishEvent(GetScanSkippedEvent(media, ScanSkippedReason.MediaFolderDoesNotExist));
                return;
            }

            var videoFilesStopwatch = Stopwatch.StartNew();
            var mediaFileList = FilterFiles(media.Path, GetVideoFiles(media.Path)).ToList();

            videoFilesStopwatch.Stop();
            _logger.Trace("Finished getting media files for: {0} [{1}]", media, videoFilesStopwatch.Elapsed);

            _logger.Debug("{0} Cleaning up media files in DB", media);
            _mediaFileTableCleanupService.Clean(media, mediaFileList);

            var decisionsStopwatch = Stopwatch.StartNew();
            var decisions = _importDecisionMaker.GetImportDecisions(mediaFileList, media);
            decisionsStopwatch.Stop();
            _logger.Trace("Import decisions complete for: {0} [{1}]", media, decisionsStopwatch.Elapsed);

            _importApprovedItems.Import(decisions, false);

            _logger.Info("Completed scanning disk for {0}", media.Title);
            _eventAggregator.PublishEvent(GetScannedEvent(media));
        }

        private bool CanCreateFolder(Media media)
        {
            if (media is Tv.Series)
            {
                return _configService.CreateEmptySeriesFolders;
            }
            else if (media is Movie)
            {
                return _configService.CreateEmptyMovieFolders;
            }
            return false;
        }

        private IEvent GetScanSkippedEvent(Media media, ScanSkippedReason reason)
        {
            if (media is Tv.Series)
            {
                return new SeriesScanSkippedEvent(media as Tv.Series, reason);
            }
            else if (media is Movie)
            {
                return new MovieScanSkippedEvent(media as Movie, reason);
            }
            return null;
        }

        private IEvent GetScannedEvent(Media media)
        {
            if (media is Tv.Series)
            {
                return new SeriesScannedEvent(media as Tv.Series);
            }
            else if (media is Movie)
            {
                return new MovieScannedEvent(media as Movie);
            }
            return null;
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

        private IEnumerable<string> FilterFiles(string FilePath, IEnumerable<string> videoFiles)
        {
            return videoFiles.Where(file => !ExcludedSubFoldersRegex.IsMatch(FilePath.GetRelativePath(file)))
                             .Where(file => !ExcludedFilesRegex.IsMatch(Path.GetFileName(file)));
        }

        private void SetPermissions(String path)
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

                _logger.WarnException("Unable to apply permissions to: " + path, ex);
                _logger.DebugException(ex.Message, ex);
            }
        }

        public void Handle(SeriesUpdatedEvent message)
        {
            Scan(message.Series);
        }

        public void Handle(MovieUpdatedEvent message)
        {
            Scan(message.Movie);
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