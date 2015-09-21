using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Movies.Commands;

namespace NzbDrone.Core.Movies
{
    public class MoveMovieService : IExecute<MoveMovieCommand>
    {
        private readonly IMovieService _movieService;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public MoveMovieService(IMovieService movieService,
                                IBuildFileNames filenameBuilder,
                                IDiskTransferService diskTransferService,
                                IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _movieService = movieService;
            _filenameBuilder = filenameBuilder;
            _diskTransferService = diskTransferService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void Execute(MoveMovieCommand message)
        {
            var movie = _movieService.GetMovie(message.MovieId);
            var source = message.SourcePath;
            var destination = message.DestinationPath;

            if (!message.DestinationRootFolder.IsNullOrWhiteSpace())
            {
                _logger.Debug("Buiding destination path using root folder: {0} and the movie title", message.DestinationRootFolder);
                destination = Path.Combine(message.DestinationRootFolder, _filenameBuilder.GetMovieFolder(movie));
            }

            _logger.ProgressInfo("Moving {0} from '{1}' to '{2}'", movie.Title, source, destination);

            //TODO: Move to transactional disk operations
            try
            {
                _diskTransferService.TransferFolder(source, destination, TransferMode.Move);
            }
            catch (IOException ex)
            {
                var errorMessage = String.Format("Unable to move series from '{0}' to '{1}'", source, destination);

                _logger.ErrorException(errorMessage, ex);
                throw;
            }

            _logger.ProgressInfo("{0} moved successfully to {1}", movie.Title, movie.Path);

            //Update the series path to the new path
            movie.Path = destination;
            movie = _movieService.UpdateMovie(movie);

            _eventAggregator.PublishEvent(new MovieMovedEvent(movie, source, destination));
        }
    }
}
