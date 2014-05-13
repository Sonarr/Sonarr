using System;
using MonoTorrent;

namespace NzbDrone.Core.MediaFiles.TorrentInfo
{
    public interface ITorrentFileInfoReader 
    {
        String GetHashFromTorrentFile(Byte[] fileContents);
    }

    public class TorrentFileInfoReader: ITorrentFileInfoReader
    {
        public String GetHashFromTorrentFile(byte[] fileContents)
        {
            return Torrent.Load(fileContents).InfoHash.ToHex();
        }
    }
}
