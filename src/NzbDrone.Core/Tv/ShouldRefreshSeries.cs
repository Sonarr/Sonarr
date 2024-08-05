using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Tv
{
    public interface ICheckIfSeriesShouldBeRefreshed
    {
        bool ShouldRefresh(Series series);
    }

    public class ShouldRefreshSeries : ICheckIfSeriesShouldBeRefreshed
    {
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public ShouldRefreshSeries(IEpisodeService episodeService, Logger logger)
        {
            _episodeService = episodeService;
            _logger = logger;
        }

        public bool ShouldRefresh(Series series)
        {
            if (series.LastInfoSync < DateTime.UtcNow.AddDays(-30))
            {
                _logger.Trace("Series {0} last updated more than 30 days ago, should refresh.", series.Title);
                return true;
            }

            var episodes = _episodeService.GetEpisodeBySeries(series.Id);

            var atLeastOneAiredEpisodeWithoutTitle = episodes.Any(e =>
                e.SeasonNumber > 0 &&
                e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(DateTime.UtcNow) &&
                e.Title.Equals("TBA", StringComparison.Ordinal));

            if (atLeastOneAiredEpisodeWithoutTitle)
            {
                _logger.Trace("Series {0} with at least one aired episode with TBA title, should refresh.", series.Title);
                return true;
            }

            if (series.LastInfoSync >= DateTime.UtcNow.AddHours(-6))
            {
                _logger.Trace("Series {0} last updated less than 6 hours ago, should not be refreshed.", series.Title);
                return false;
            }

            if (series.Status != SeriesStatusType.Ended)
            {
                _logger.Trace("Series {0} is not ended, should refresh.", series.Title);
                return true;
            }

            var lastEpisode = episodes.MaxBy(e => e.AirDateUtc);

            if (lastEpisode != null && lastEpisode.AirDateUtc > DateTime.UtcNow.AddDays(-30))
            {
                _logger.Trace("Last episode in {0} aired less than 30 days ago, should refresh.", series.Title);
                return true;
            }

            _logger.Trace("Series {0} ended long ago, should not be refreshed.", series.Title);
            return false;
        }
    }
}
