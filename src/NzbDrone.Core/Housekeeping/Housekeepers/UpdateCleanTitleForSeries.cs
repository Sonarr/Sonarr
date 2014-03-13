using System.Linq;
using NLog;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class UpdateCleanTitleForSeries : IHousekeepingTask
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly Logger _logger;

        public UpdateCleanTitleForSeries(ISeriesRepository seriesRepository, Logger logger)
        {
            _seriesRepository = seriesRepository;
            _logger = logger;
        }

        public void Clean()
        {
            _logger.Debug("Updating CleanTitle for all series");

            var series = _seriesRepository.All().ToList();

            series.ForEach(s =>
            {
                s.CleanTitle = s.CleanTitle.CleanSeriesTitle();
                _seriesRepository.Update(s);
            });
        }
    }
}
