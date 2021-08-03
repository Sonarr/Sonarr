using System;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class EpisodeResource
    {
        public int TvdbId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }
        public int? AiredAfterSeasonNumber { get; set; }
        public int? AiredBeforeSeasonNumber { get; set; }
        public int? AiredBeforeEpisodeNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public RatingResource Rating { get; set; }
        public string Overview { get; set; }
        public string Image { get; set; }
    }
}
