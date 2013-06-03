using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Serializer;

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

            var headers = _httpProvider.GetHeader(url);

            string sizeString;

            if (headers.TryGetValue(HttpProvider.ContentLenghtHeader, out sizeString))
            {
                int size;
                int.TryParse(sizeString, out size);
                var fileSize = _diskProvider.GetFileSize(path);

                return fileSize == size;
            }

            _logger.Warn("Couldn't find content-length header {0}", headers.ToJson());

            return false;
        }
    }
}