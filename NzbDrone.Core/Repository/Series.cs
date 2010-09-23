using System;

namespace NzbDrone.Core.Repository
{
    public class Series
    {
        public int Id
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
