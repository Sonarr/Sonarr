using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Tv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NzbDrone.Core.Movies
{
    public class RefreshMoviesService : IExecute<RefreshMovieCommand>
    {
        private readonly IProvideMoviesInfo _movieInfo;
        private readonly IMovieService _movieService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskScanService _diskScanService;
        private readonly ICheckIfMovieShouldBeRefreshed _checkIfMovieShouldBeRefreshed;
        private readonly Logger _logger;

        public RefreshMoviesService(IProvideMoviesInfo movieInfo,
                                    IMovieService movieService,
                                    IEventAggregator eventAggregator,
                                    IDiskScanService diskScanService,
                                    ICheckIfMovieShouldBeRefreshed checkIfMovieShouldBeRefreshed,
                                    Logger logger)
        {
            _movieInfo = movieInfo;
            _movieService = movieService;
            _eventAggregator = eventAggregator;
            _diskScanService = diskScanService;
            _checkIfMovieShouldBeRefreshed = checkIfMovieShouldBeRefreshed;
            _logger = logger;
        }

        private void RefreshMovieInfo(Movie oldMovieInfo)
        {
            _logger.ProgressInfo("Updating Info for {0}", oldMovieInfo.Title);

            var movie = new Movie();
            
            try
            {
                movie = _movieInfo.GetMovieInfo(oldMovieInfo.TmdbId);
            }
            catch (MovieNotFoundException)
            {
                _logger.Error("Movie '{0}' (Tmdbid {1}) was not found, it may have been removed from TheTmdb.", oldMovieInfo.Title, oldMovieInfo.TmdbId);
                return;
            }

            var movieInfo = movie;

            oldMovieInfo.ImdbId = movieInfo.ImdbId;
            oldMovieInfo.TmdbId = movieInfo.TmdbId;


            oldMovieInfo.Title = movieInfo.Title;
            oldMovieInfo.CleanTitle = movieInfo.CleanTitle;
            oldMovieInfo.OriginalTitle = movieInfo.OriginalTitle;

            oldMovieInfo.Year = movieInfo.Year;
            oldMovieInfo.Overview = movieInfo.Overview;
            oldMovieInfo.Runtime = movieInfo.Runtime;
            oldMovieInfo.TagLine = movieInfo.TagLine;
            oldMovieInfo.ReleaseDate = movieInfo.ReleaseDate;
            oldMovieInfo.Images = movieInfo.Images;

            oldMovieInfo.LastInfoSync = DateTime.UtcNow;

            try
            {
                oldMovieInfo.Path = new DirectoryInfo(oldMovieInfo.Path).FullName;
                oldMovieInfo.Path = oldMovieInfo.Path.GetActualCasing();
            }
            catch (Exception e)
            {
                _logger.WarnException("Couldn't update movie path for " + oldMovieInfo.Path, e);
            }


            _movieService.UpdateMovie(oldMovieInfo);

            _logger.Debug("Finished series refresh for {0}", oldMovieInfo.Title);
            _eventAggregator.PublishEvent(new MovieUpdatedEvent(oldMovieInfo));
        }

        private List<Season> UpdateSeasons(Series series, Series seriesInfo)
        {
            foreach (var season in seriesInfo.Seasons)
            {
                var existingSeason = series.Seasons.SingleOrDefault(s => s.SeasonNumber == season.SeasonNumber);

                //Todo: Should this should use the previous season's monitored state?
                if (existingSeason == null)
                {
                    if (season.SeasonNumber == 0)
                    {
                        season.Monitored = false;
                        continue;
                    }

                    _logger.Debug("New season ({0}) for series: [{1}] {2}, setting monitored to true", season.SeasonNumber, series.TvdbId, series.Title);
                    season.Monitored = true;
                }

                else
                {
                    season.Monitored = existingSeason.Monitored;
                }
            }

            return seriesInfo.Seasons;
        }

        public void Execute(RefreshMovieCommand message)
        {
            _eventAggregator.PublishEvent(new MovieRefreshStartingEvent());

            if (message.MovieId.HasValue)
            {
                var movie = _movieService.GetMovie(message.MovieId.Value);
                RefreshMovieInfo(movie);
            }
            else
            {
                var allMovies = _movieService.GetAllMovies().OrderBy(c => c.CleanTitle).ToList();

                foreach (var movie in allMovies)
                {
                    if (message.Trigger == CommandTrigger.Manual || _checkIfMovieShouldBeRefreshed.ShouldRefresh(movie))
                    {
                        try
                        {
                            RefreshMovieInfo(movie);
                        }
                        catch (Exception e)
                        {
                            _logger.ErrorException("Couldn't refresh info for {0}".Inject(movie), e);
                        }
                    }

                    else
                    {
                        try
                        {
                            _logger.Info("Skipping refresh of series: {0}", movie.Title);
                            _diskScanService.Scan(movie);
                        }
                        catch (Exception e)
                        {
                            _logger.ErrorException("Couldn't rescan series {0}".Inject(movie), e);
                        }
                    }
                }
            }
        }
    }
}
