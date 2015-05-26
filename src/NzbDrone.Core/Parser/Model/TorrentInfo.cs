using System;
using System.Text;

namespace NzbDrone.Core.Parser.Model
{
    public class TorrentInfo : ReleaseInfo
    {
        public string MagnetUrl { get; set; }
        public string InfoHash { get; set; }
        public Int32? Seeders { get; set; }
        public Int32? Peers { get; set; }

        public static Int32? GetSeeders(ReleaseInfo release)
        {
            var torrentInfo = release as TorrentInfo;

            if (torrentInfo == null)
            {
                return null;
            }
            return torrentInfo.Seeders;
        }

        public override string ToString(string format)
        {
            var stringBuilder = new StringBuilder(base.ToString(format));
            switch (format.ToUpperInvariant())
            {
                case "L": // Long format
                    stringBuilder.AppendLine("MagnetUrl: " + MagnetUrl ?? "Empty");
                    stringBuilder.AppendLine("InfoHash: " + InfoHash ?? "Empty");
                    stringBuilder.AppendLine("Seeders: " + Seeders ?? "Empty");
                    stringBuilder.AppendLine("Peers: " + Peers ?? "Empty");
                    break;
            }

            return stringBuilder.ToString();
        }
    }
}