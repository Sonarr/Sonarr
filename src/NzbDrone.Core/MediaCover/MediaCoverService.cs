using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaCover
{
    public interface IMapCoversToLocal
    {
        void ConvertToLocalUrls(int seriesId, IEnumerable<MediaCover> covers);
        string GetCoverPath(int seriesId, MediaCoverTypes mediaCoverTypes, MediaCoverOrigin coverOrigin, int? height = null);
    }

    public class MediaCoverService :
        IHandleAsync<SeriesUpdatedEvent>,
        IHandleAsync<SeriesDeletedEvent>,
        IHandleAsync<MovieUpdatedEvent>,
        IHandleAsync<MovieDeletedEvent>,
        IMapCoversToLocal
    {
        private readonly IImageResizer _resizer;
        private readonly IHttpClient _httpClient;
        private readonly IDiskProvider _diskProvider;
        private readonly ICoverExistsSpecification _coverExistsSpecification;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        private readonly string _coverRootFolder;

        public MediaCoverService(IImageResizer resizer,
                                 IHttpClient httpClient,
                                 IDiskProvider diskProvider,
                                 IAppFolderInfo appFolderInfo,
                                 ICoverExistsSpecification coverExistsSpecification,
                                 IConfigFileProvider configFileProvider,
                                 IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _resizer = resizer;
            _httpClient = httpClient;
            _diskProvider = diskProvider;
            _coverExistsSpecification = coverExistsSpecification;
            _configFileProvider = configFileProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;

            _coverRootFolder = appFolderInfo.GetMediaCoverPath();
        }

        public string GetCoverPath(int id, MediaCoverTypes coverTypes, MediaCoverOrigin coverOrigin, int? height = null)
        {
            var heightSuffix = height.HasValue ? "-" + height.ToString() : "";

            var path = coverOrigin == MediaCoverOrigin.Series ? GetSeriesCoverPath(id) : GetMovieCoverPath(id);

            return Path.Combine(path, coverTypes.ToString().ToLower() + heightSuffix + ".jpg");
        }

        public void ConvertToLocalUrls(int seriesId, IEnumerable<MediaCover> covers)
        {
            foreach (var mediaCover in covers)
            {
                var filePath = GetCoverPath(seriesId, mediaCover.CoverType, mediaCover.CoverOrigin);

                var movie = mediaCover.CoverOrigin == MediaCoverOrigin.Movie ? "/movies/" : "";

                mediaCover.Url = _configFileProvider.UrlBase + @"/MediaCover/" + movie + seriesId + "/" + mediaCover.CoverType.ToString().ToLower() + ".jpg";

                if (_diskProvider.FileExists(filePath))
                {
                    var lastWrite = _diskProvider.FileGetLastWrite(filePath);
                    mediaCover.Url += "?lastWrite=" + lastWrite.Ticks;
                }
            }
        }

        private string GetSeriesCoverPath(int seriesId)
        {
            return Path.Combine(_coverRootFolder, seriesId.ToString());
        }

        private string GetMovieCoverPath(int movieId)
        {
            return Path.Combine(_coverRootFolder, "movies", movieId.ToString());
        }

        private void EnsureCovers(Movie movie)
        {
            foreach (var cover in movie.Images)
            {
                var fileName = GetCoverPath(movie.Id, cover.CoverType, MediaCoverOrigin.Movie);
                var alreadyExists = false;
                try
                {
                    alreadyExists = _coverExistsSpecification.AlreadyExists(cover.Url, fileName);
                    if (!alreadyExists)
                    {
                        DownloadCover(movie.Id, cover);
                    }
                }
                catch (WebException e)
                {
                    _logger.Warn(string.Format("Couldn't download media cover for {0}. {1}", movie, e.Message));
                }
                catch (Exception e)
                {
                    _logger.ErrorException("Couldn't download media cover for " + movie, e);
                }

                EnsureResizedCovers(movie.Id, cover, !alreadyExists);
            }
        }

        private void EnsureCovers(Series series)
        {
            foreach (var cover in series.Images)
            {
                var fileName = GetCoverPath(series.Id, cover.CoverType, MediaCoverOrigin.Series);
                var alreadyExists = false;
                try
                {
                    alreadyExists = _coverExistsSpecification.AlreadyExists(cover.Url, fileName);
                    if (!alreadyExists)
                    {
                        DownloadCover(series.Id, cover);
                    }
                }
                catch (WebException e)
                {
                    _logger.Warn(string.Format("Couldn't download media cover for {0}. {1}", series, e.Message));
                }
                catch (Exception e)
                {
                    _logger.ErrorException("Couldn't download media cover for " + series, e);
                }

                EnsureResizedCovers(series.Id, cover, !alreadyExists);
            }
        }

        private void DownloadCover(int id, MediaCover cover)
        {
            var fileName = GetCoverPath(id, cover.CoverType, cover.CoverOrigin);

            _logger.Info("Downloading {0} for {1} {2} {3}", cover.CoverType, id, cover.CoverOrigin == MediaCoverOrigin.Series ? "serie":"movie", cover.Url);
            _httpClient.DownloadFile(cover.Url, fileName);
        }

        private void EnsureResizedCovers(int id, MediaCover cover, bool forceResize)
        {
            int[] heights;

            switch (cover.CoverType)
            {
                default:
                    return;

                case MediaCoverTypes.Poster:
                case MediaCoverTypes.Headshot:
                    heights = new[] { 500, 250 };
                    break;

                case MediaCoverTypes.Banner:
                    heights = new[] { 70, 35 };
                    break;

                case MediaCoverTypes.Fanart:
                case MediaCoverTypes.Screenshot:
                    heights = new[] { 360, 180 };
                    break;
            }

            foreach (var height in heights)
            {
                var mainFileName = GetCoverPath(id, cover.CoverType, cover.CoverOrigin);
                var resizeFileName = GetCoverPath(id, cover.CoverType, cover.CoverOrigin, height);

                if (forceResize || !_diskProvider.FileExists(resizeFileName) || _diskProvider.GetFileSize(resizeFileName) == 0)
                {
                    _logger.Debug("Resizing {0}-{1} for {2}", cover.CoverType, height, id);

                    try
                    {
                        _resizer.Resize(mainFileName, resizeFileName, height);
                    }
                    catch
                    {
                        _logger.Debug("Couldn't resize media cover {0}-{1} for {2}, using full size image instead.", cover.CoverType, height, id);
                    }
                }
            }
        }

        public void HandleAsync(SeriesUpdatedEvent message)
        {
            EnsureCovers(message.Series);
            _eventAggregator.PublishEvent(new MediaCoversUpdatedEvent(message.Series));
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            var path = GetSeriesCoverPath(message.Series.Id);
            if (_diskProvider.FolderExists(path))
            {
                _diskProvider.DeleteFolder(path, true);
            }
        }

        public void HandleAsync(MovieUpdatedEvent message)
        {
            EnsureCovers(message.Movie);
            _eventAggregator.PublishEvent(new MediaCoversUpdatedEvent(message.Movie));
        }

        public void HandleAsync(MovieDeletedEvent message)
        {
            var path = GetMovieCoverPath(message.Movie.Id);
            if (_diskProvider.FolderExists(path))
            {
                _diskProvider.DeleteFolder(path, true);
            }
        }
    }
}
