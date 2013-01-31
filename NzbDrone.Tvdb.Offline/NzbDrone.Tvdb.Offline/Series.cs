using System;

namespace NzbDrone.Tvdb.Offline
{
    public class Series
    {
        public virtual int SeriesId { get; set; }

        public string Title { get; set; }

        public string CleanTitle { get; set; }

        public string Status { get; set; }

        public Boolean? Active { get; set; }

        public string Overview { get; set; }

        public string AirsDayOfWeek { get; set; }

        public int? WeekDay { get; set; }

        public String AirTimes { get; set; }

        public int? RateCount { get; set; }

        public decimal? Rating { get; set; }

        public String Path { get; set; }
    }
}