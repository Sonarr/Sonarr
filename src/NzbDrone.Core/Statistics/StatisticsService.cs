namespace NzbDrone.Core.Statistics;

public interface IStatisticsService
{
    LibraryStatistics GetLibraryStatistics(StatisticsFilter filter = null);
}

public class StatisticsService : IStatisticsService
{
    private readonly IStatisticsRepository _statisticsRepository;

    public StatisticsService(IStatisticsRepository statisticsRepository)
    {
        _statisticsRepository = statisticsRepository;
    }

    public LibraryStatistics GetLibraryStatistics(StatisticsFilter filter = null)
    {
        return _statisticsRepository.GetLibraryStatistics(filter);
    }
}
