using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Movies
{
    public interface IMovieService
    {
        Movie GetMovie(int movieId);
        List<Movie> GetMovies(IEnumerable<int> movieIds);
        Movie AddMovie(Movie newMovie);
        Movie FindByImdbId(string imdbId);
        Movie FindByTmdbId(string tmdbId);
        Movie FindByTitle(string title);
        Movie FindByTitle(string title, int year);
        void DeleteMove(int movieId, bool deleteFiles);
        List<Movie> GetAllMovies();
        Movie UpdateMovie(Movie movie);
        bool MoviePathExists(string folder);
        void RemoveAddOptions(Movie movie);
        PagingSpec<Movie> MoviesWithoutFile(PagingSpec<Movie> pagingSpec);

    }

    public class MovieService : IMovieService,
                                IHandle<MovieFileAddedEvent>,
                                IHandle<MovieFileDeletedEvent>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly Logger _logger;

        public MovieService(IMovieRepository movieRepository,
                            IConfigService configService,
                            IEventAggregator eventAggregator,
                            IBuildFileNames fileNameBuilder,
                            Logger logger)
        {
            _movieRepository = movieRepository;
            _configService = configService;
            _eventAggregator = eventAggregator;
            _fileNameBuilder = fileNameBuilder;
            _logger = logger;
        }

        public Movie GetMovie(int movieId)
        {
            return _movieRepository.Get(movieId);
        }

        public List<Movie> GetMovies(IEnumerable<int> movieIds)
        {
            return _movieRepository.Get(movieIds).ToList();
        }

        public Movie AddMovie(Movie newMovie)
        {
            Ensure.That(newMovie, () => newMovie).IsNotNull();

            if (String.IsNullOrEmpty(newMovie.Path))
            {
                var folderName = _fileNameBuilder.GetMovieFolder(newMovie);
                newMovie.Path = Path.Combine(newMovie.RootFolderPath, folderName);
            }

            _logger.Info("Adding Movies {0} Path[{1}]", newMovie, newMovie.Path);
            newMovie.CleanTitle = Parser.Parser.CleanSeriesTitle(newMovie.OriginalTitle);
            newMovie.Monitored = true;
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
            return _movieRepository.FindByTitle(Parser.Parser.CleanMovieTitle(title));
        }

        public Movie FindByTitle(string title, int year)
        {
            return _movieRepository.FindByTitle(Parser.Parser.CleanMovieTitle(title), year);
        }

        public void DeleteMove(int movieId, bool deleteFiles)
        {
            var movie = _movieRepository.Get(movieId);
            _movieRepository.Delete(movieId);

            _eventAggregator.PublishEvent(new MovieDeletedEvent(movie, deleteFiles));
        }

        public List<Movie> GetAllMovies()
        {
            return _movieRepository.All().ToList();
        }

        public Movie GetMovieByFileId(int movieId)
        {
            return _movieRepository.GetMovieByFileId(movieId);
        }

        public Movie UpdateMovie(Movie movie)
        {
            var storedMovie = GetMovie(movie.Id);
            var updatedMovie = _movieRepository.Update(movie);
            _eventAggregator.PublishEvent(new MovieEditedEvent(updatedMovie, storedMovie));
            return updatedMovie;
        }

        public bool MoviePathExists(string folder)
        {
            return _movieRepository.MoviePathExists(folder);
        }

        public void RemoveAddOptions(Movie movie)
        {
            _movieRepository.SetFields(movie, s => s.AddOptions);
        }

        public void Handle(MovieFileAddedEvent message)
        {
            _movieRepository.SetFileId(message.MovieFile.MovieId, message.MovieFile.Id);
            _logger.Debug("Linking [{0}] > [{1}]", message.MovieFile.RelativePath, message.MovieFile.Movie);
        }

        public void Handle(MovieFileDeletedEvent message)
        {
            Movie movie = GetMovieByFileId(message.MovieFile.Id);
            _logger.Debug("Detaching movie {0} from file.", movie.Id);
            movie.MovieFileId = 0;
            if (message.Reason != DeleteMediaFileReason.Upgrade && _configService.AutoUnmonitorPreviouslyDownloadedEpisodes)
            {
                movie.Monitored = false;
            }

            UpdateMovie(movie);
        }

        public PagingSpec<Movie> MoviesWithoutFile(PagingSpec<Movie> pagingSpec)
        {
            var movieResult = _movieRepository.MoviesWithoutFile(pagingSpec);

            return movieResult;
        }

    }
}
