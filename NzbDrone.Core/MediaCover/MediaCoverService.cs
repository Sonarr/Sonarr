using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaCover
{
    public class MediaCoverService :
        IHandleAsync<SeriesUpdatedEvent>,
        IHandleAsync<SeriesDeletedEvent>,
        IMapCoversToLocal
    {
        private readonly IHttpProvider _httpProvider;
        private readonly IDiskProvider _diskProvider;
        private readonly ICoverExistsSpecification _coverExistsSpecification;
        private readonly Logger _logger;

        private readonly string _coverRootFolder;

        public MediaCoverService(IHttpProvider httpProvider, IDiskProvider diskProvider, IAppFolderInfo appFolderInfo,
            ICoverExistsSpecification coverExistsSpecification, Logger logger)
        {
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
            _coverExistsSpecification = coverExistsSpecification;
            _logger = logger;

            _coverRootFolder = appFolderInfo.GetMediaCoverPath();
        }

        public void HandleAsync(SeriesUpdatedEvent message)
        {
            EnsureCovers(message.Series);
        }

        private void EnsureCovers(Series series)
        {
            foreach (var cover in series.Images)
            {
                var fileName = GetCoverPath(series.Id, cover.CoverType);
                if (!_coverExistsSpecification.AlreadyExists(cover.Url, fileName))
                {
                    DownloadCover(series, cover);
                }
            }
        }

        private void DownloadCover(Series series, MediaCover cover)
        {
            try
            {
                var fileName = GetCoverPath(series.Id, cover.CoverType);

                _logger.Info("Downloading {0} for {1} {2}", cover.CoverType, series, cover.Url);
                _httpProvider.DownloadFile(cover.Url, fileName);
            }
            catch (WebException e)
            {
                _logger.WarnException("Couldn't download media cover for " + series, e);
            }
            catch (Exception e)
            {
                _logger.ErrorException("Couldn't download media cover for " + series, e);
            }
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            var path = GetSeriesCoverPath(message.Series.Id);
            if (_diskProvider.FolderExists(path))
            {
                _diskProvider.DeleteFolder(path, true);
            }
        }

        private string GetCoverPath(int seriesId, MediaCoverTypes coverTypes)
        {
            return Path.Combine(GetSeriesCoverPath(seriesId), coverTypes.ToString().ToLower() + ".jpg");
        }

        private string GetSeriesCoverPath(int seriesId)
        {
            return Path.Combine(_coverRootFolder, seriesId.ToString());
        }

        public void ConvertToLocalUrls(int seriesId, IEnumerable<MediaCover> covers)
        {
            foreach (var mediaCover in covers)
            {
                var filePath = GetCoverPath(seriesId, mediaCover.CoverType);

                mediaCover.Url = @"/MediaCover/" + seriesId + "/" + mediaCover.CoverType.ToString().ToLower() + ".jpg";

                if (_diskProvider.FileExists(filePath))
                {
                    var lastWrite = _diskProvider.GetLastFileWrite(filePath);
                    mediaCover.Url += "?lastWrite=" + lastWrite.Ticks;
                }
            }
        }
    }

    public interface IMapCoversToLocal
    {
        void ConvertToLocalUrls(int seriesId, IEnumerable<MediaCover> covers);
    }
}
