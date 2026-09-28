using NzbDrone.Core.Statistics;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Statistics;

public class StatisticsResource : RestResource
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
    public required List<QualityProfileStatisticsResource> QualityProfileStatistics { get; set; }
    public required List<QualityStatisticsResource> QualityStatistics { get; set; }
    public required List<TagStatisticsResource> TagStatistics { get; set; }
}

public static class StatisticsResourceMapper
{
    public static StatisticsResource MapToResource(this LibraryStatistics model)
    {
        return new StatisticsResource
        {
            SeriesCount = model.SeriesCount,
            MonitoredSeriesCount = model.MonitoredSeriesCount,
            CompletedSeriesCount = model.CompletedSeriesCount,
            ContinuingSeriesCount = model.ContinuingSeriesCount,
            EndedSeriesCount = model.EndedSeriesCount,
            UpcomingSeriesCount = model.UpcomingSeriesCount,
            DeletedSeriesCount = model.DeletedSeriesCount,
            StandardSeriesCount = model.StandardSeriesCount,
            DailySeriesCount = model.DailySeriesCount,
            AnimeSeriesCount = model.AnimeSeriesCount,
            SeasonCount = model.SeasonCount,
            CompletedSeasonCount = model.CompletedSeasonCount,
            TotalEpisodeCount = model.TotalEpisodeCount,
            MonitoredEpisodeCount = model.MonitoredEpisodeCount,
            DownloadedEpisodeCount = model.DownloadedEpisodeCount,
            MissingEpisodeCount = model.MissingEpisodeCount,
            UnairedEpisodeCount = model.UnairedEpisodeCount,
            EpisodeFileCount = model.EpisodeFileCount,
            SizeOnDisk = model.SizeOnDisk,
            QualityProfileStatistics = model.QualityProfileStatistics.ConvertAll(QualityProfileStatisticsResourceMapper.MapToResource),
            QualityStatistics = model.QualityStatistics.ConvertAll(QualityStatisticsResourceMapper.MapToResource),
            TagStatistics = model.TagStatistics.ConvertAll(TagStatisticsResourceMapper.MapToResource)
        };
    }
}
