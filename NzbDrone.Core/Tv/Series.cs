using System.Linq;
using System;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Tv
{
    public class Series
    {
        public virtual int SeriesId { get; set; }

        public string Title { get; set; }

        public string CleanTitle { get; set; }

        public string Status { get; set; }

        public string Overview { get; set; }

        public DayOfWeek? AirsDayOfWeek { get; set; }

        [Column("AirTimes")]
        public String AirTime { get; set; }

        public string Language { get; set; }

        public string Path { get; set; }

        public bool Monitored { get; set; }

        public virtual int QualityProfileId { get; set; }

        public bool SeasonFolder { get; set; }

        public DateTime? LastInfoSync { get; set; }

        public DateTime? LastDiskSync { get; set; }

        public int Runtime { get; set; }

        public string BannerUrl { get; set; }

        public bool IsDaily { get; set; }

        public BacklogSettingType BacklogSetting { get; set; }

        public string Network { get; set; }

        public DateTime? CustomStartDate { get; set; }

        public bool UseSceneNumbering { get; set; }

        public int TvRageId { get; set; }

        public string TvRageTitle { get; set; }

        //Todo: This should be a double since there are timezones that aren't on a full hour offset
        public int UtcOffset { get; set; }

        public DateTime? FirstAired { get; set; }

        public bool Hidden { get; set; }

        public QualityProfile QualityProfile { get; set; }

        public int EpisodeCount { get; set; }

        public int EpisodeFileCount { get; set; }

        public int SeasonCount { get; set; }

        public DateTime? NextAiring { get; set; }
    }
}