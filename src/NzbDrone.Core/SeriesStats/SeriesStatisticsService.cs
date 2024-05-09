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
        private readonly ISeriesStatisticsRepository _seriesStatisticsRepository;

        public SeriesStatisticsService(ISeriesStatisticsRepository seriesStatisticsRepository)
        {
            _seriesStatisticsRepository = seriesStatisticsRepository;
        }

        public List<SeriesStatistics> SeriesStatistics()
        {
            var seasonStatistics = _seriesStatisticsRepository.SeriesStatistics();

            return seasonStatistics.GroupBy(s => s.SeriesId).Select(s => MapSeriesStatistics(s.ToList())).ToList();
        }

        public SeriesStatistics SeriesStatistics(int seriesId)
        {
            var stats = _seriesStatisticsRepository.SeriesStatistics(seriesId);

            if (stats == null || stats.Count == 0)
            {
                return new SeriesStatistics();
            }

            return MapSeriesStatistics(stats);
        }

        private SeriesStatistics MapSeriesStatistics(List<SeasonStatistics> seasonStatistics)
        {
            var seriesStatistics = new SeriesStatistics
            {
                SeasonStatistics = seasonStatistics,
                SeriesId = seasonStatistics.First().SeriesId,
                EpisodeFileCount = seasonStatistics.Sum(s => s.EpisodeFileCount),
                EpisodeCount = seasonStatistics.Sum(s => s.EpisodeCount),
                TotalEpisodeCount = seasonStatistics.Sum(s => s.TotalEpisodeCount),
                SizeOnDisk = seasonStatistics.Sum(s => s.SizeOnDisk),
                ReleaseGroups = seasonStatistics.SelectMany(s => s.ReleaseGroups).Distinct().ToList()
            };

            var nextAiring = seasonStatistics.Where(s => s.NextAiring != null).MinBy(s => s.NextAiring);
            var previousAiring = seasonStatistics.Where(s => s.PreviousAiring != null).MaxBy(s => s.PreviousAiring);
            var lastAired = seasonStatistics.Where(s => s.SeasonNumber > 0 && s.LastAired != null).MaxBy(s => s.LastAired);

            seriesStatistics.NextAiring = nextAiring?.NextAiring;
            seriesStatistics.PreviousAiring = previousAiring?.PreviousAiring;
            seriesStatistics.LastAired = lastAired?.LastAired;

            return seriesStatistics;
        }
    }
}
