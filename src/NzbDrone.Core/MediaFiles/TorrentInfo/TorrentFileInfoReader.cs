using System.Text;
using MonoTorrent;
using NLog;

namespace NzbDrone.Core.MediaFiles.TorrentInfo
{
    public interface ITorrentFileInfoReader
    {
        string GetHashFromTorrentFile(byte[] fileContents);
    }

    public class TorrentFileInfoReader : ITorrentFileInfoReader
    {
        private readonly Logger _logger;

        public TorrentFileInfoReader(Logger logger)
        {
            _logger = logger;
        }

        public string GetHashFromTorrentFile(byte[] fileContents)
        {
            try
            {
                return Torrent.Load(fileContents).InfoHash.ToHex();
            }
            catch
            {
                _logger.Trace("Invalid torrent file contents: {0}", Encoding.ASCII.GetString(fileContents));
                throw;
            }
        }
    }
}
