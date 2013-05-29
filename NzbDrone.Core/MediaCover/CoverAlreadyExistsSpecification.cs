using NzbDrone.Common;

namespace NzbDrone.Core.MediaCover
{
    public interface ICoverExistsSpecification
    {
        bool AlreadyExists(string url, string paht);
    }

    public class CoverAlreadyExistsSpecification : ICoverExistsSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IHttpProvider _httpProvider;

        public CoverAlreadyExistsSpecification(IDiskProvider diskProvider, IHttpProvider httpProvider)
        {
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
        }

        public bool AlreadyExists(string url, string path)
        {
            if (!_diskProvider.FileExists(path))
            {
                return false;
            }

            var headers = _httpProvider.DownloadHeader(url);

            string sizeString = null;

            if (headers.TryGetValue(HttpProvider.ContentLenghtHeader, out sizeString))
            {
                int size;
                int.TryParse(sizeString, out size);
                var fileSize = _diskProvider.GetFileSize(path);

                if (fileSize != size)
                {
                    return false;
                }
            }

            return true;
        }
    }
}