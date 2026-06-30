using NzbDrone.Core.Statistics;

namespace Sonarr.Api.V5.Statistics;

public class QualityProfileStatisticsResource
{
    public int QualityProfileId { get; set; }
    public required string Name { get; set; }
    public int SeriesCount { get; set; }
    public int EpisodeFileCount { get; set; }
    public long SizeOnDisk { get; set; }
}

public static class QualityProfileStatisticsResourceMapper
{
    public static QualityProfileStatisticsResource MapToResource(this QualityProfileStatistics model)
    {
        return new QualityProfileStatisticsResource
        {
            QualityProfileId = model.QualityProfileId,
            Name = model.Name,
            SeriesCount = model.SeriesCount,
            EpisodeFileCount = model.EpisodeFileCount,
            SizeOnDisk = model.SizeOnDisk
        };
    }
}
