namespace NzbDrone.Core.Statistics;

public class QualityProfileStatistics
{
    public int QualityProfileId { get; set; }
    public string Name { get; set; }
    public int SeriesCount { get; set; }
    public int EpisodeFileCount { get; set; }
    public long SizeOnDisk { get; set; }
}
