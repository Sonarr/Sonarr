using System;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaCover
{
    public class MediaCoverService :
        IHandleAsync<SeriesUpdatedEvent>,
        IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IHttpProvider _httpProvider;
        private readonly IDiskProvider _diskProvider;
        private readonly ICoverExistsSpecification _coverExistsSpecification;
        private readonly Logger _logger;

        private readonly string _coverRootFolder;

        public MediaCoverService(IHttpProvider httpProvider, IDiskProvider diskProvider, IEnvironmentProvider environmentProvider,
            ICoverExistsSpecification coverExistsSpecification, Logger logger)
        {
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
            _coverExistsSpecification = coverExistsSpecification;
            _logger = logger;

            _coverRootFolder = environmentProvider.GetMediaCoverPath();
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

                _logger.Info("Downloading {0} for {1} {2}", cover.CoverType, series.Title, cover.Url);
                _httpProvider.DownloadFile(cover.Url, fileName);
            }
            catch (Exception e)
            {
                _logger.ErrorException("Couldn't download media cover for " + series.TvdbId, e);
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
    }
}
