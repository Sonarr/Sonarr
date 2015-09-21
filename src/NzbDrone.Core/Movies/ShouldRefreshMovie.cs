using System;
using System.Linq;
using NLog;

namespace NzbDrone.Core.Movies
{
    public interface ICheckIfMovieShouldBeRefreshed
    {
        bool ShouldRefresh(Movie movie);
    }

    public class ShouldRefreshMovie : ICheckIfMovieShouldBeRefreshed
    {
        private readonly Logger _logger;

        public ShouldRefreshMovie(Logger logger)
        {
            _logger = logger;
        }

        public bool ShouldRefresh(Movie movie)
        {
            if (movie.LastInfoSync < DateTime.UtcNow.AddDays(-30))
            {
                _logger.Trace("Movie {0} last updated more than 30 days ago, should refresh.", movie.Title);
                return true;
            }

            if (movie.LastInfoSync >= DateTime.UtcNow.AddHours(-6))
            {
                _logger.Trace("Movie {0} last updated less than 6 hours ago, should not be refreshed.", movie.Title);
                return false;
            }

            if (!movie.Images.Any(m => m.CoverType == MediaCover.MediaCoverTypes.Poster) || !movie.Images.Any(m => m.CoverType == MediaCover.MediaCoverTypes.Fanart))
            {
                _logger.Trace("Movie {0} lacks MediaCovers, should refresh.", movie.Title);
                return true;
            }

            _logger.Trace("Movie {0} should not be refreshed.", movie.Title);
            return false;
        }
    }
}
