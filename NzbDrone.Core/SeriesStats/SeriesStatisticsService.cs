using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.SeriesStats
{
    public interface ISeriesStatisticsService
    {
        List<SeriesStatistics> SeriesStatistics();
        SeriesStatistics SeriesStatistics(int seriesId);
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

        public SeriesStatistics SeriesStatistics(int seriesId)
        {
            var stats = _seriesStatisticsRepository.SeriesStatistics(seriesId);

            if (stats == null) return new SeriesStatistics();

            return stats;
        }
    }
}
