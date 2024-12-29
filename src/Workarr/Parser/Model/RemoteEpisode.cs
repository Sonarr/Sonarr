using Workarr.CustomFormats;
using Workarr.DataAugmentation.Scene;
using Workarr.Download.Clients;
using Workarr.Languages;
using Workarr.Tv;

namespace Workarr.Parser.Model
{
    public class RemoteEpisode
    {
        public ReleaseInfo Release { get; set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }
        public SceneMapping SceneMapping { get; set; }
        public int MappedSeasonNumber { get; set; }
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public bool EpisodeRequested { get; set; }
        public bool DownloadAllowed { get; set; }
        public TorrentSeedConfiguration SeedConfiguration { get; set; }
        public List<CustomFormat> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public SeriesMatchType SeriesMatchType { get; set; }
        public List<Language> Languages { get; set; }
        public ReleaseSourceType ReleaseSource { get; set; }

        public RemoteEpisode()
        {
            Episodes = new List<Episode>();
            CustomFormats = new List<CustomFormat>();
            Languages = new List<Language>();
        }

        public bool IsRecentEpisode()
        {
            return Episodes.Any(e => e.AirDateUtc >= DateTime.UtcNow.Date.AddDays(-14));
        }

        public override string ToString()
        {
            return Release.Title;
        }
    }

    public enum ReleaseSourceType
    {
        Unknown = 0,
        Rss = 1,
        Search = 2,
        UserInvokedSearch = 3,
        InteractiveSearch = 4,
        ReleasePush = 5
    }
}
