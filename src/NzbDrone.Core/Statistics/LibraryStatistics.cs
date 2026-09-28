using System.Collections.Generic;

namespace NzbDrone.Core.Statistics;

public class LibraryStatistics
{
    public int SeriesCount { get; set; }
    public int MonitoredSeriesCount { get; set; }
    public int CompletedSeriesCount { get; set; }
    public int ContinuingSeriesCount { get; set; }
    public int EndedSeriesCount { get; set; }
    public int UpcomingSeriesCount { get; set; }
    public int DeletedSeriesCount { get; set; }
    public int StandardSeriesCount { get; set; }
    public int DailySeriesCount { get; set; }
    public int AnimeSeriesCount { get; set; }
    public int SeasonCount { get; set; }
    public int CompletedSeasonCount { get; set; }
    public int TotalEpisodeCount { get; set; }
    public int MonitoredEpisodeCount { get; set; }
    public int DownloadedEpisodeCount { get; set; }
    public int MissingEpisodeCount { get; set; }
    public int UnairedEpisodeCount { get; set; }
    public int EpisodeFileCount { get; set; }
    public long SizeOnDisk { get; set; }
    public List<QualityProfileStatistics> QualityProfileStatistics { get; set; }
    public List<QualityStatistics> QualityStatistics { get; set; }
    public List<TagStatistics> TagStatistics { get; set; }
}
