namespace NzbDrone.Core.Statistics;

public class TagStatistics
{
    public int TagId { get; set; }
    public string Label { get; set; }
    public int SeriesCount { get; set; }
    public int EpisodeFileCount { get; set; }
    public long SizeOnDisk { get; set; }
}
