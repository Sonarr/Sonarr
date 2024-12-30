using System.Collections.Generic;
using Workarr.Tv;

namespace Sonarr.Api.V3.SeasonPass
{
    public class SeasonPassResource
    {
        public List<SeasonPassSeriesResource> Series { get; set; }
        public MonitoringOptions MonitoringOptions { get; set; }
    }
}
