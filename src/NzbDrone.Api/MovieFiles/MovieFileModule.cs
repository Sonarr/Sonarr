using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Api.REST;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Movies;
using NzbDrone.SignalR;
using NzbDrone.Core.Movies;

namespace NzbDrone.Api.MovieFiles
{
    public class MovieFileModule : NzbDroneRestModuleWithSignalR<MovieFileResource, MovieFile>,
                                   IHandle<MovieFileAddedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMovieService _movieService;
        private readonly IQualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public MovieFileModule(IBroadcastSignalRMessage signalRBroadcaster,
                              IMediaFileService mediaFileService,
                              IRecycleBinProvider recycleBinProvider,
                              IMovieService movieService,
                              IQualityUpgradableSpecification qualityUpgradableSpecification,
                              Logger logger)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _recycleBinProvider = recycleBinProvider;
            _movieService = movieService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
            GetResourceById = GetMovieFile;
            GetResourceAll = GetMovieFiles;
            UpdateResource = SetQuality;
            DeleteResource = DeleteEpisodeFile;
        }

        private MovieFileResource GetMovieFile(int id)
        {
            var movieFile = _mediaFileService.GetMovieFile(id);
            var movie = _movieService.GetMovie(movieFile.MovieId);

            return MapToResource(movie, movieFile);
        }

        private List<MovieFileResource> GetMovieFiles()
        {
            var movieId = (int?)Request.Query.MovieId;

            if (movieId == null)
            {
                throw new BadRequestException("movieId is missing");
            }

            var movie = _movieService.GetMovie(movieId.Value);
            return _mediaFileService.GetFileByMovie(movieId.Value).Select(m => MapToResource(movie, m)).ToList();
        }

        private void SetQuality(MovieFileResource movieFileResource)
        {
            var movieFile = _mediaFileService.GetMovieFile(movieFileResource.Id);
            movieFile.Quality = movieFileResource.Quality;
            _mediaFileService.Update(movieFile);
        }

        private void DeleteEpisodeFile(int id)
        {
            var movieFile = _mediaFileService.GetMovieFile(id);
            var movie = _movieService.GetMovie(movieFile.MovieId);
            var fullPath = Path.Combine(movie.Path, movieFile.RelativePath);

            _logger.Info("Deleting episode file: {0}", fullPath);
            _recycleBinProvider.DeleteFile(fullPath);
            _mediaFileService.Delete(movieFile, DeleteMediaFileReason.Manual);
        }

        private MovieFileResource MapToResource(Movie movie, MovieFile movieFile)
        {

            var resource = movieFile.InjectTo<MovieFileResource>();
            resource.Path = Path.Combine(movie.Path, movieFile.RelativePath);

            resource.QualityCutoffNotMet = _qualityUpgradableSpecification.CutoffNotMet(movie.Profile.Value, movieFile.Quality);

            return resource;
        }

        public void Handle(MovieFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.MovieFile.Id);
        }
    }
}