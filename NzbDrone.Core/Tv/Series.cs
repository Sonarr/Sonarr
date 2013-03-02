using System.Collections.Generic;
using System.Linq;
using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Model;
using NzbDrone.Core.Qualities;

using Sqo.Attributes;

namespace NzbDrone.Core.Tv
{
    public enum  SeriesType
    {
        Standard =0,
        Daily =1,
        Anime = 2,
    }

    public class Series : ModelBase
    {
        public int TvDbId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string Status { get; set; }

        [Text]
        public string Overview { get; set; }

        public String AirTime { get; set; }
        public string Language { get; set; }
        public string Path { get; set; }
        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public bool SeasonFolder { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public DateTime? LastDiskSync { get; set; }
        public int Runtime { get; set; }
        public string BannerUrl { get; set; }
        public SeriesType SeriesType { get; set; }
        public BacklogSettingType BacklogSetting { get; set; }
        public string Network { get; set; }
        public DateTime? CustomStartDate { get; set; }
        public bool UseSceneNumbering { get; set; }
        public int TvRageId { get; set; }
        public string TvRageTitle { get; set; }

        //Todo: This should be a double since there are timezones that aren't on a full hour offset
        public int UtcOffset { get; set; }

        public DateTime? FirstAired { get; set; }
        public QualityProfile QualityProfile { get; set; }
        public int EpisodeCount { get; set; }
        public int EpisodeFileCount { get; set; }
        public int SeasonCount { get; set; }
        public DateTime? NextAiring { get; set; }
    }
}