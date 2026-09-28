using NzbDrone.Core.Qualities;
using NzbDrone.Core.Statistics;

namespace Sonarr.Api.V5.Statistics;

public class QualityStatisticsResource
{
    public required Quality Quality { get; set; }
    public int EpisodeFileCount { get; set; }
    public long SizeOnDisk { get; set; }
}

public static class QualityStatisticsResourceMapper
{
    public static QualityStatisticsResource MapToResource(this QualityStatistics model)
    {
        return new QualityStatisticsResource
        {
            Quality = model.Quality,
            EpisodeFileCount = model.EpisodeFileCount,
            SizeOnDisk = model.SizeOnDisk
        };
    }
}
