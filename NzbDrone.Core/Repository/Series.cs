using System;
using System.ComponentModel;
using Eloquera.Client;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [PrimaryKey("SeriesId", autoIncrement = false)]
    public class Series
    {
        [ID]
        public long Id;

        public virtual int SeriesId { get; set; }

        public string Title { get; set; }

        public string CleanTitle { get; set; }

        public string Status { get; set; }

        public string Overview { get; set; }

        [DisplayName("Air on")]
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

        public int UtcOffset { get; set; }

        public DateTime? FirstAired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Series"/> is hidden.
        /// </summary>
        /// <value><c>true</c> if hidden; otherwise, <c>false</c>.</value>
        /// <remarks>This field will be used for shows that are pending delete or 
        /// new series that haven't been successfully imported</remarks>
        [PetaPoco.Ignore]
        public bool Hidden { get; set; }

        [ResultColumn]
        public QualityProfile QualityProfile { get; set; }

        [ResultColumn]
        public int EpisodeCount { get; set; }

        [ResultColumn]
        public int EpisodeFileCount { get; set; }

        [ResultColumn]
        public int SeasonCount { get; set; }

        [ResultColumn]
        public DateTime? NextAiring { get; set; }
    }
}