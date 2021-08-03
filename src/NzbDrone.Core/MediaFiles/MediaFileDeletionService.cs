using System;
using System.IO;
using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDeleteMediaFiles
    {
        void DeleteEpisodeFile(Series series, EpisodeFile episodeFile);
    }

    public class MediaFileDeletionService : IDeleteMediaFiles,
                                            IHandleAsync<SeriesDeletedEvent>,
                                            IHandle<EpisodeFileDeletedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly ISeriesService _seriesService;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public MediaFileDeletionService(IDiskProvider diskProvider,
                                        IRecycleBinProvider recycleBinProvider,
                                        IMediaFileService mediaFileService,
                                        ISeriesService seriesService,
                                        IConfigService configService,
                                        IEventAggregator eventAggregator,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _seriesService = seriesService;
            _configService = configService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void DeleteEpisodeFile(Series series, EpisodeFile episodeFile)
        {
            var fullPath = Path.Combine(series.Path, episodeFile.RelativePath);
            var rootFolder = _diskProvider.GetParentFolder(series.Path);

            if (!_diskProvider.FolderExists(rootFolder))
            {
                _logger.Warn("Series' root folder ({0}) doesn't exist.", rootFolder);
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Series' root folder ({0}) doesn't exist.", rootFolder);
            }

            if (_diskProvider.GetDirectories(rootFolder).Empty())
            {
                _logger.Warn("Series' root folder ({0}) is empty.", rootFolder);
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Series' root folder ({0}) is empty.", rootFolder);
            }

            if (_diskProvider.FolderExists(series.Path) && _diskProvider.FileExists(fullPath))
            {
                _logger.Info("Deleting episode file: {0}", fullPath);

                var subfolder = _diskProvider.GetParentFolder(series.Path).GetRelativePath(_diskProvider.GetParentFolder(fullPath));

                try
                {
                    _recycleBinProvider.DeleteFile(fullPath, subfolder);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unable to delete episode file");
                    throw new NzbDroneClientException(HttpStatusCode.InternalServerError, "Unable to delete episode file");
                }
            }

            // Delete the episode file from the database to clean it up even if the file was already deleted
            _mediaFileService.Delete(episodeFile, DeleteMediaFileReason.Manual);

            _eventAggregator.PublishEvent(new DeleteCompletedEvent());
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            if (message.DeleteFiles)
            {
                var series = message.Series;
                var allSeries = _seriesService.GetAllSeries();

                foreach (var s in allSeries)
                {
                    if (s.Id == series.Id)
                    {
                        continue;
                    }

                    if (series.Path.IsParentPath(s.Path))
                    {
                        _logger.Error("Series path: '{0}' is a parent of another series, not deleting files.", series.Path);
                        return;
                    }

                    if (series.Path.PathEquals(s.Path))
                    {
                        _logger.Error("Series path: '{0}' is the same as another series, not deleting files.", series.Path);
                        return;
                    }
                }

                if (_diskProvider.FolderExists(message.Series.Path))
                {
                    _recycleBinProvider.DeleteFolder(message.Series.Path);
                }

                _eventAggregator.PublishEvent(new DeleteCompletedEvent());
            }
        }

        [EventHandleOrder(EventHandleOrder.Last)]
        public void Handle(EpisodeFileDeletedEvent message)
        {
            if (_configService.DeleteEmptyFolders)
            {
                var series = message.EpisodeFile.Series.Value;
                var seriesPath = series.Path;
                var folder = message.EpisodeFile.Path.GetParentPath();

                while (seriesPath.IsParentPath(folder))
                {
                    if (_diskProvider.FolderExists(folder))
                    {
                        _diskProvider.RemoveEmptySubfolders(folder);
                    }

                    folder = folder.GetParentPath();
                }

                _diskProvider.RemoveEmptySubfolders(seriesPath);

                if (_diskProvider.FolderEmpty(seriesPath))
                {
                    _diskProvider.DeleteFolder(seriesPath, true);
                }
            }
        }
    }
}
