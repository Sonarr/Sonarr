using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MediaFiles.Commands.Movies;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.MediaFiles.Movies
{
    public interface IRenameMovieFileService
    {
        List<RenameMovieFilePreview> GetRenamePreviews(int movieId);
    }

    public class RenameMovieFileService : IRenameMovieFileService,
                                          IExecute<RenameMovieFilesCommand>,
                                          IExecute<RenameMovieCommand>
    {
        private readonly IMovieService _movieService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveFiles _fileMover;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly Logger _logger;

        public RenameMovieFileService(IMovieService movieService,
                                      IMediaFileService mediaFileService,
                                      IMoveFiles fileMover,
                                      IEventAggregator eventAggregator,
                                      IBuildFileNames filenameBuilder,
                                      Logger logger)
        {
            _movieService = movieService;
            _mediaFileService = mediaFileService;
            _fileMover = fileMover;
            _eventAggregator = eventAggregator;
            _filenameBuilder = filenameBuilder;
            _logger = logger;
        }

        public List<RenameMovieFilePreview> GetRenamePreviews(int movieId)
        {
            var movie = _movieService.GetMovie(movieId);
            var files = _mediaFileService.GetFileByMovie(movieId);

            return GetPreviews(movie, files).ToList();
        }

        private IEnumerable<RenameMovieFilePreview> GetPreviews(Movie movie, List<MovieFile> files)
        {
            foreach (var f in files)
            {
                var file = f;
                var movieFilePath = Path.Combine(movie.Path, file.RelativePath);

                if (f.Id != movie.MovieFileId)
                {
                    _logger.Warn("File ({0}) is not linked to this movie", movieFilePath);
                    continue;
                }

                var newName = _filenameBuilder.BuildFileName(movie, file);
                var newPath = _filenameBuilder.BuildFilePath(movie, newName, Path.GetExtension(movieFilePath));

                if (!movieFilePath.PathEquals(newPath, StringComparison.Ordinal))
                {
                    yield return new RenameMovieFilePreview
                                 {
                                     MovieId = movie.Id,
                                     MovieFileId = file.Id,
                                     ExistingPath = file.RelativePath,
                                     NewPath = movie.Path.GetRelativePath(newPath)
                                 };
                }
            }
        }

        private void RenameFiles(List<MovieFile> movieFiles, Movie movie)
        {
            var renamed = new List<MovieFile>();

            foreach (var movieFile in movieFiles)
            {
                var movieFilePath = Path.Combine(movie.Path, movieFile.RelativePath);

                try
                {
                    _logger.Debug("Renaming movie file: {0}", movieFile);
                    _fileMover.MoveFile(movieFile, movie);

                    _mediaFileService.Update(movieFile);
                    renamed.Add(movieFile);

                    _logger.Debug("Renamed episode file: {0}", movieFile);
                }
                catch (SameFilenameException ex)
                {
                    _logger.Debug("File not renamed, source and destination are the same: {0}", ex.Filename);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Failed to rename file: " + movieFilePath, ex);
                }
            }

            if (renamed.Any())
            {
                _eventAggregator.PublishEvent(new MovieRenamedEvent(movie));
            }
        }

        public void Execute(RenameMovieFilesCommand message)
        {
            var movie = _movieService.GetMovie(message.MovieId);
            var movieFile = _mediaFileService.GetMovieFiles(message.Files);

            _logger.ProgressInfo("Renaming {0} files for {1}", movieFile.Count, movie.Title);
            RenameFiles(movieFile, movie);
            _logger.ProgressInfo("Selected movie files renamed for {0}", movie.Title);
        }

        public void Execute(RenameMovieCommand message)
        {
            _logger.Debug("Renaming all files for selected movies");
            var moviesToRename = _movieService.GetMovies(message.MovieIds);

            foreach (var movie in moviesToRename)
            {
                var movieFiles = _mediaFileService.GetFileByMovie(movie.Id);
                _logger.ProgressInfo("Renaming all files in series: {0}", movie.Title);
                RenameFiles(movieFiles, movie);
                _logger.ProgressInfo("All episode files renamed for {0}", movie.Title);
            }
        }
    }
}
