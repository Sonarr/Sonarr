using System;
using System.Collections.Generic;
using System.ComponentModel;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [PrimaryKey("SeriesId", autoIncrement = false)]
    public class Series
    {

        public virtual int SeriesId { get; set; }


        public string Title { get; set; }


        public string CleanTitle { get; set; }


        public string Status { get; set; }


        public string Overview { get; set; }

        [DisplayName("Air on")]
        public DayOfWeek? AirsDayOfWeek { get; set; }


        public String AirTimes { get; set; }


        public string Language { get; set; }

        public string Path { get; set; }

        public bool Monitored { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Series"/> is hidden.
        /// </summary>
        /// <value><c>true</c> if hidden; otherwise, <c>false</c>.</value>
        /// <remarks>This field will be used for shows that are pending delete or 
        /// new series that haven't been successfully imported</remarks>
        [Ignore]
        public bool Hidden { get; set; }

        public virtual int QualityProfileId { get; set; }

        public bool SeasonFolder { get; set; }

        public DateTime? LastInfoSync { get; set; }

        public DateTime? LastDiskSync { get; set; }

        [Ignore]
        public virtual QualityProfile QualityProfile { get; set; }
    }
}