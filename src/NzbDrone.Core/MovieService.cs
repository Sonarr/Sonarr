using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Movies
{
    public interface IMovieService
    {
        Movie GetMovie(int movieId);
        Movie AddMovie(Movie newMovie);
        Movie FindByImdbId(string imdbId);
        Movie FindByTmdbId(string tmdbId);
        Movie FindByTitle(string title);
        void DeleteMove(int movieId, bool deleteFiles);
        List<Movie> GetAllMovies();
        Movie UpdateMovie(Movie movie);
        bool MoviePathExists(string folder);
    }

    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public MovieService(IMovieRepository movieRepository,IConfigService configService,IEventAggregator eventAggregator
            ,Logger logger)
        {
            _movieRepository = movieRepository;
            _configService = configService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public Movie GetMovie(int movieId)
        {
            return _movieRepository.Get(movieId);
        }

        public Movie AddMovie(Movie newMovie)
        {
            Ensure.That(newMovie, () => newMovie).IsNotNull();

            if (String.IsNullOrEmpty(newMovie.Path))
            {
                var folderName = FileNameBuilder.CleanFilename(newMovie.Title);
                newMovie.Path = Path.Combine(newMovie.RootFolderPath, folderName);
            }

            _logger.Info("Adding Movie {0} Path[{1}]",newMovie,newMovie.Path);
            newMovie.CleanTitle = Parser.Parser.CleanSeriesTitle(newMovie.Title);
            newMovie.TagLine = "";
            _movieRepository.Insert(newMovie);

            _eventAggregator.PublishEvent(new MovieAddedEvent(newMovie));

            return newMovie;
        }

        public Movie FindByImdbId(string imdbId)
        {
            return _movieRepository.FindByImdbId(imdbId);
        }

        public Movie FindByTmdbId(string tmdbId)
        {
            return _movieRepository.FindByTmdbId(tmdbId);
        }

        public Movie FindByTitle(string title)
        {
            return _movieRepository.FindByTitle(Parser.Parser.CleanSeriesTitle(title));
        }

        public void DeleteMove(int movieId, bool deleteFiles)
        {
            var movie = _movieRepository.Get(movieId);
            _movieRepository.Delete(movieId);

            _eventAggregator.PublishEvent(new MovieDeletedEvent(movie,deleteFiles));
        }

        public List<Movie> GetAllMovies()
        {
            return _movieRepository.All().ToList();
        }

        public Movie UpdateMovie(Movie movie)
        {
            return _movieRepository.Update(movie);
        }

        public bool MoviePathExists(string folder)
        {
            return _movieRepository.MoviePathExists(folder);
        }
    }
}
