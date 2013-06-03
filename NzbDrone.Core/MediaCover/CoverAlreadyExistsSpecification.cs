using NLog;
using NzbDrone.Common;

namespace NzbDrone.Core.MediaCover
{
    public interface ICoverExistsSpecification
    {
        bool AlreadyExists(string url, string path);
    }

    public class CoverAlreadyExistsSpecification : ICoverExistsSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        public CoverAlreadyExistsSpecification(IDiskProvider diskProvider, IHttpProvider httpProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public bool AlreadyExists(string url, string path)
        {
            if (!_diskProvider.FileExists(path))
            {
                return false;
            }

            var headers = _httpProvider.DownloadHeader(url);

            string sizeString;

            if (headers.TryGetValue(headers[HttpProvider.ContentLenghtHeader], out sizeString))
            {
                int size;
                int.TryParse(sizeString, out size);
                var fileSize = _diskProvider.GetFileSize(path);

                return fileSize == size;
            }

            _logger.Warn("Couldn't read content length header {0}", headers[HttpProvider.ContentLenghtHeader]);

            return false;
        }
    }
}