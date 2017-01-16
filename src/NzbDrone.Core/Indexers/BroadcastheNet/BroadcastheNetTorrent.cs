namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetTorrent
    {
        public string GroupName { get; set; }
        public int GroupID { get; set; }
        public int TorrentID { get; set; }
        public int SeriesID { get; set; }
        public string Series { get; set; }
        public string SeriesBanner { get; set; }
        public string SeriesPoster { get; set; }
        public string YoutubeTrailer { get; set; }
        public string Category { get; set; }
        public int? Snatched { get; set; }
        public int? Seeders { get; set; }
        public int? Leechers { get; set; }
        public string Source { get; set; }
        public string Container { get; set; }
        public string Codec { get; set; }
        public string Resolution { get; set; }
        public string Origin { get; set; }
        public string ReleaseName { get; set; }
        public long Size { get; set; }
        public long Time { get; set; }
        public int? TvdbID { get; set; }
        public int? TvrageID { get; set; }
        public string ImdbID { get; set; }
        public string InfoHash { get; set; }
        public string DownloadURL { get; set; }
    }
}
