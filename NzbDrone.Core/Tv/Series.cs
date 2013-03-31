using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MetadataSource.Trakt;
using NzbDrone.Core.Model;
using NzbDrone.Core.Qualities;


namespace NzbDrone.Core.Tv
{
    public enum SeriesTypes
    {
        Standard = 0,
        Daily = 1,
        Anime = 2,
    }

    public class Series : ModelBase
    {
        public Series()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public int TvDbId { get; set; }
        public int TvRageId { get; set; }
        public string ImdbId { get; set; }

        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public SeriesStatusType Status { get; set; }
        public string Overview { get; set; }
        public String AirTime { get; set; }
        public string Path { get; set; }
        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public bool SeasonFolder { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public DateTime? LastDiskSync { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public SeriesTypes SeriesType { get; set; }
        public BacklogSettingType BacklogSetting { get; set; }
        public string Network { get; set; }
        public DateTime? CustomStartDate { get; set; }
        public bool UseSceneNumbering { get; set; }

        public string TitleSlug { get; set; }

        //Todo: This should be a double since there are timezones that aren't on a full hour offset
        public int UtcOffset { get; set; }

        public DateTime? FirstAired { get; set; }
        public QualityProfile QualityProfile { get; set; }
        public DateTime? NextAiring { get; set; }
    }
}