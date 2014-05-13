using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetTorrent
    {
        public String GroupName { get; set; }
        public Int32 GroupID { get; set; }
        public Int32 TorrentID { get; set; }
        public Int32 SeriesID { get; set; }
        public String Series { get; set; }
        public String SeriesBanner { get; set; }
        public String SeriesPoster { get; set; }
        public String YoutubeTrailer { get; set; }
        public String Category { get; set; }
        public Int32? Snatched { get; set; }
        public Int32? Seeders { get; set; }
        public Int32? Leechers { get; set; }
        public String Source { get; set; }
        public String Container { get; set; }
        public String Codec { get; set; }
        public String Resolution { get; set; }
        public String Origin { get; set; }
        public String ReleaseName { get; set; }
        public Int64 Size { get; set; }
        public Int64 Time { get; set; }
        public Int32? TvdbID { get; set; }
        public Int32? TvrageID { get; set; }
        public String ImdbID { get; set; }
        public String InfoHash { get; set; }
        public String DownloadURL { get; set; }
    }
}
