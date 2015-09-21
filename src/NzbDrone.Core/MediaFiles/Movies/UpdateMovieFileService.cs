using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.Movies
{
    public interface IUpdateMovieFileService
    {
        void ChangeFileDateForFile(MovieFile movieFile, Movie movie);
    }

    public class UpdateMovieFileService : IUpdateMovieFileService,
                                          IHandle<MovieScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        public UpdateMovieFileService(IDiskProvider diskProvider,
                                      IConfigService configService,
                                      IMovieService movieService,
                                      Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _movieService = movieService;
            _logger = logger;
        }

        public void ChangeFileDateForFile(MovieFile movieFile, Movie movie)
        {
            ChangeFileDate(movieFile, movie);
        }

        private bool ChangeFileDate(MovieFile movieFile, Movie movie)
        {
            var movieFilePath = Path.Combine(movie.Path, movieFile.RelativePath);

            switch (_configService.FileDate)
            {
                case FileDateType.LocalAirDate:
                    {
                        var airDate = movie.ReleaseDate;

                        if (airDate.ToString().IsNullOrWhiteSpace())
                        {
                            return false;
                        }

                        return ChangeFileDateToLocalAirDate(movieFilePath, airDate.ToString());
                    }
            }

            return false;
        }

        public void Handle(MovieScannedEvent message)
        {
            if (_configService.FileDate == FileDateType.None)
            {
                return;
            }

            var movie = _movieService.GetMovie(message.Movie.Id);


            if (movie.MovieFileId > 0)
            {
                if (ChangeFileDate(movie.MovieFile.Value, message.Movie))
                {
                    _logger.ProgressDebug("Changed file date for file {1} in {2}", movie.MovieFile.Value.RelativePath, message.Movie.Title);
                }
                _logger.ProgressDebug("No file dates changed for {0}", message.Movie.Title);
            }
        }

        private bool ChangeFileDateToLocalAirDate(string filePath, string fileDate)
        {
            DateTime airDate;

            if (DateTime.TryParse(fileDate, out airDate))
            {
                // avoiding false +ve checks and set date skewing by not using UTC (Windows)
                DateTime oldDateTime = _diskProvider.FileGetLastWrite(filePath);

                if (!DateTime.Equals(airDate, oldDateTime))
                {
                    try
                    {
                        _diskProvider.FileSetLastWriteTime(filePath, airDate);
                        _logger.Debug("Date of file [{0}] changed from '{1}' to '{2}'", filePath, oldDateTime, airDate);

                        return true;
                    }

                    catch (Exception ex)
                    {
                        _logger.WarnException("Unable to set date of file [" + filePath + "]", ex);
                    }
                }
            }

            else
            {
                _logger.Debug("Could not create valid date to change file [{0}]", filePath);
            }

            return false;
        }
    }
}
