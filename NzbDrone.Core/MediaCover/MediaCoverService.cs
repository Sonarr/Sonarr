using System;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaCover
{
    public class MediaCoverService : IHandleAsync<SeriesUpdatedEvent>
    {
        private readonly HttpProvider _httpProvider;
        private readonly DiskProvider _diskProvider;
        private readonly Logger _logger;

        private readonly string _coverRootFolder;

        private const string COVER_URL_PREFIX = "http://www.thetvdb.com/banners/";

        public MediaCoverService(HttpProvider httpProvider, DiskProvider diskProvider, EnvironmentProvider environmentProvider, Logger logger)
        {
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
            _logger = logger;

            _coverRootFolder = environmentProvider.GetMediaCoverPath();
        }

        public void HandleAsync(SeriesUpdatedEvent message)
        {
            EnsureCovers(message.Series);
        }

        private void EnsureCovers(Series series)
        {
            foreach (var cover in series.Covers)
            {
                var fileName = GetCoverPath(series.Id, cover.CoverType);
                if (!_diskProvider.FileExists(fileName))
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

                _logger.Info("Downloading {0} for {1}", cover.CoverType, series.Title);
                _httpProvider.DownloadFile(COVER_URL_PREFIX + cover.Url, fileName);
            }
            catch (Exception e)
            {
                _logger.ErrorException("Couldn't download media cover for " + series.TvDbId, e);
            }
        }

        private string GetCoverPath(int seriesId, MediaCoverTypes coverTypes)
        {
            return Path.Combine(_coverRootFolder, seriesId.ToString("0000"), coverTypes.ToString().ToLower() + ".jpg");
        }
    }
}
