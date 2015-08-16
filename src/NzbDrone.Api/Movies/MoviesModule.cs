using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.Validation;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Api.Movies
{
    public class MoviesModule : NzbDroneRestModuleWithSignalR<MoviesResource, Movie>,
                                IHandle<MovieImportedEvent>,
                                IHandle<MovieFileDeletedEvent>,
                                IHandle<MovieUpdatedEvent>,
                                IHandle<MovieDeletedEvent>,
                                IHandle<MediaCoversUpdatedEvent>,
                                IHandle<MovieDownloadedEvent>,
                                IHandle<MovieRenamedEvent>,
                                IHandle<MovieGrabbedEvent>
    {
        private readonly IMovieService _movieService;
        private readonly IMapCoversToLocal _coverMapper;

        public MoviesModule(IBroadcastSignalRMessage signalRBroadcaster, 
                            IMovieService movieService, 
                            IMapCoversToLocal coverMapper
            )
            : base(signalRBroadcaster, "movies")
        {
            _movieService = movieService;
            _coverMapper = coverMapper;
            GetResourceAll = GetAllMovies;
            GetResourceById = GetMovie;
            CreateResource = AddMovie;
            UpdateResource = UpdateMovie;
            DeleteResource = DeleteMovie;

            SharedValidator.RuleFor(s => s.ProfileId).ValidId();

            PutValidator.RuleFor(s => s.Path).IsValidPath();

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => String.IsNullOrEmpty(s.RootFolderPath));
            PostValidator.RuleFor(s => s.RootFolderPath).IsValidPath().When(s => String.IsNullOrEmpty(s.Path));
            PostValidator.RuleFor(s => s.Title).NotEmpty();
        }

        private void DeleteMovie(int id)
        {
            var deleteFiles = false;
            var deleteFilesQuery = Request.Query.deleteFiles;

            if (deleteFilesQuery.HasValue)
            {
                deleteFiles = Convert.ToBoolean(deleteFilesQuery.Value);
            }
            _movieService.DeleteMove(id,deleteFiles);
        }

        private void UpdateMovie(MoviesResource moviesResource)
        {
            GetNewId<Movie>(_movieService.UpdateMovie, moviesResource);
        }

        private int AddMovie(MoviesResource moviesResource)
        {
            return GetNewId<Movie>(_movieService.AddMovie, moviesResource);
        }

        private MoviesResource GetMovie(int id)
        {
            var movie = _movieService.GetMovie(id);
            return GetMovieResource(movie);
        }

        private MoviesResource GetMovieResource(Movie movie)
        {
            if (movie == null) return null;
            var resource = movie.InjectTo<MoviesResource>();
            MapCoversToLocal(resource);

            return resource;
        }

        private void MapCoversToLocal(params MoviesResource[] resource)
        {
            foreach (var moviesResource in resource)
            {
                _coverMapper.ConvertToLocalUrls(moviesResource.Id,moviesResource.Images);
            }
        }

        private List<MoviesResource> GetAllMovies()
        {
            var movieResources = ToListResource(_movieService.GetAllMovies);

            MapCoversToLocal(movieResources.ToArray());

            return movieResources;
        }

        public void Handle(MovieImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.ImportedMovie.MovieId);
        }

        public void Handle(MovieFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade) return;

            BroadcastResourceChange(ModelAction.Updated, message.MovieFile.MovieId);
        }

        public void Handle(MovieUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Movie.Id);
        }

        public void Handle(MovieEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Movie.Id);
        }

        public void Handle(MovieDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.Movie.InjectTo<MoviesResource>());
        }

        public void Handle(MediaCoversUpdatedEvent message)
        {
            if (message.CoverOrigin == MediaCoverOrigin.Movie)
                BroadcastResourceChange(ModelAction.Updated, message.Movie.Id);
        }

        public void Handle(MovieDownloadedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Movie.Movie.Id);
        }

        public void Handle(MovieRenamedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Movie.Id);
        }

        public void Handle(MovieGrabbedEvent message)
        {
            var resource = message.Movie.InjectTo<MoviesResource>();
            BroadcastResourceChange(ModelAction.Updated, resource);
        }
    }
}
