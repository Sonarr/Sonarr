using System;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Tvdb.Offline
{
    public class Series
    {
        [SubSonicPrimaryKey(false)]
        public virtual int SeriesId { get; set; }

        public string Title { get; set; }

        [SubSonicNullString]
        public string CleanTitle { get; set; }

        [SubSonicNullString]
        public string Status { get; set; }

        public Boolean? Active { get; set; }

        [SubSonicNullString]
        public string Overview { get; set; }

        [SubSonicNullString]
        public string AirsDayOfWeek { get; set; }

        public int? WeekDay { get; set; }

        public String AirTimes { get; set; }

        public int? RateCount { get; set; }

        public decimal? Rating { get; set; }

        [SubSonicIgnore]
        public String Path { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}:{1} {2}]", SeriesId, Title, Path);
        }
    }
}