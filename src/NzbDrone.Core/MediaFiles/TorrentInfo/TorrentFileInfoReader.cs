using MonoTorrent;

namespace NzbDrone.Core.MediaFiles.TorrentInfo
{
    public interface ITorrentFileInfoReader
    {
        string GetHashFromTorrentFile(byte[] fileContents);
    }

    public class TorrentFileInfoReader : ITorrentFileInfoReader
    {
        public string GetHashFromTorrentFile(byte[] fileContents)
        {
            return Torrent.Load(fileContents).InfoHash.ToHex();
        }
    }
}
