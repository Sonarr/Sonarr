using System;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Series
    {
        [SubSonicPrimaryKey]
        public int TvdbId
        {
            get;
            set;
        }

        public string SeriesName
        {
            get;
            set;
        }

        public string Status
        {
            get;
            set;
        }

        [SubSonicLongString]
        public string Overview
        {
            get;
            set;
        }

        public DayOfWeek? AirsDayOfWeek
        {
            get;
            set;
        }

        public string AirTimes
        {
            get;
            set;
        }

        public string Language
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

    }
}
