using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.SeasonPass
{
    public class SeasonPassResource
    {
        public List<Core.Tv.Series> Series { get; set; }
        public MonitoringOptions MonitoringOptions { get; set; }
    }
}
