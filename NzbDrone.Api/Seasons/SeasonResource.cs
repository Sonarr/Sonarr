using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Api.REST;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Seasons
{
    public class SeasonResource : RestResource
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public Boolean Monitored { get; set; }
    }
}
