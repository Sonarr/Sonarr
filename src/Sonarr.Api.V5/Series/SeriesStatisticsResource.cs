using NzbDrone.Core.SeriesStats;

namespace Sonarr.Api.V5.Series;

public class SeriesStatisticsResource
{
    public int SeasonCount { get; set; }
    public int EpisodeFileCount { get; set; }
    public int EpisodeCount { get; set; }
    public int TotalEpisodeCount { get; set; }
    public long SizeOnDisk { get; set; }
    public List<string>? ReleaseGroups { get; set; }

    public decimal PercentOfEpisodes
    {
        get
        {
            if (EpisodeCount == 0)
            {
                return 0;
            }

            return (decimal)EpisodeFileCount / (decimal)EpisodeCount * 100;
        }
    }
}

public static class SeriesStatisticsResourceMapper
{
    public static SeriesStatisticsResource ToResource(this SeriesStatistics model, List<SeasonResource>? seasons)
    {
        return new SeriesStatisticsResource
        {
            SeasonCount = seasons?.Count(s => s.SeasonNumber > 0) ?? 0,
            EpisodeFileCount = model.EpisodeFileCount,
            EpisodeCount = model.EpisodeCount,
            TotalEpisodeCount = model.TotalEpisodeCount,
            SizeOnDisk = model.SizeOnDisk,
            ReleaseGroups = model.ReleaseGroups
        };
    }
}
