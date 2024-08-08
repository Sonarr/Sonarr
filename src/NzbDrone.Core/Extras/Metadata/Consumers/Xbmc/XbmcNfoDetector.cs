using System.Text.RegularExpressions;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Xbmc
{
    public interface IDetectXbmcNfo
    {
        bool IsXbmcNfoFile(string path);
    }

    public class XbmcNfoDetector : IDetectXbmcNfo
    {
        private readonly IDiskProvider _diskProvider;

        private readonly Regex _regex = new Regex("<(movie|tvshow|episodedetails|artist|album|musicvideo)>", RegexOptions.Compiled);

        public XbmcNfoDetector(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public bool IsXbmcNfoFile(string path)
        {
            // Lets make sure we're not reading huge files.
            if (_diskProvider.GetFileSize(path) > 10.Megabytes())
            {
                return false;
            }

            // Check if it contains some of the kodi/xbmc xml tags
            var content = _diskProvider.ReadAllText(path);

            return _regex.IsMatch(content);
        }
    }
}
