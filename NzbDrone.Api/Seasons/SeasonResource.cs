using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Seasons
{
    public class SeasonResource : RestResource
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public Boolean Monitored { get; set; }
    }
}
