namespace NzbDrone.Core.Tv
{
    public class AddSeriesOptions : MonitoringOptions
    {
        public bool SearchForMissingEpisodes { get; set; }
        public bool SearchForCutoffUnmetEpisodes { get; set; }
    }
}
