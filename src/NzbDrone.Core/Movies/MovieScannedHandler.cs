using NLog;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies
{
    public class MovieScannedHandler : IHandle<MovieScannedEvent>,
                                       IHandle<MovieScanSkippedEvent>
    {
        private readonly IMovieService _movieService;
        private readonly IManageCommandQueue _commandQueueManager;

        private readonly Logger _logger;

        public MovieScannedHandler(IMovieService movieService,
                                   IManageCommandQueue commandQueueManager,
                                   Logger logger)
        {
            _movieService = movieService;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        private void HandleScanEvents(Movie movie)
        {
            if (!movie.AddOptions)
            {
                //TODO: Perform search of missing movie if recently aired?
                return;
            }

            _logger.Info("[{0}] was recently added, performing post-add actions", movie.Title);

            if (movie.AddOptions)
            {
                _commandQueueManager.Push(new MissingMovieSearchCommand(movie.Id));
            }

            movie.AddOptions = false;
            _movieService.RemoveAddOptions(movie);
        }

        public void Handle(MovieScannedEvent message)
        {
            HandleScanEvents(message.Movie);
        }

        public void Handle(MovieScanSkippedEvent message)
        {
            HandleScanEvents(message.Movie);
        }
    }
}
