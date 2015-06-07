namespace NzbDrone.Api.Series
{
    public class SeasonResource
    {
        public int SeasonNumber { get; set; }
        public bool Monitored { get; set; }
        public SeasonStatisticsResource Statistics { get; set; }
    }
}
