using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Config
{
    public class UiConfigResource : RestResource
    {
        //Calendar
        public Int32 FirstDayOfWeek { get; set; }
        public String CalendarWeekColumnHeader { get; set; }

        //Dates
        public String ShortDateFormat { get; set; }
        public String LongDateFormat { get; set; }
        public String TimeFormat { get; set; }
        public Boolean ShowRelativeDates { get; set; }

        public Boolean EnableColorImpairedMode { get; set; }
    }
}
