using System.Collections.Generic;

namespace NzbDrone.Core.SeriesStats
{
    public interface ISeriesStatisticsService
    {
        List<SeriesStatistics> SeriesStatistics();
    }

    public class SeriesStatisticsService : ISeriesStatisticsService
    {
        private readonly SeriesStatisticsRepository _seriesStatisticsRepository;

        public SeriesStatisticsService(SeriesStatisticsRepository seriesStatisticsRepository)
        {
            _seriesStatisticsRepository = seriesStatisticsRepository;
        }

        public List<SeriesStatistics> SeriesStatistics()
        {
            return _seriesStatisticsRepository.SeriesStatistics();
        }
    }
}
