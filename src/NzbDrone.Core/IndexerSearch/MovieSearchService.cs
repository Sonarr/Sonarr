using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.IndexerSearch
{
    public class MovieSearchService : IExecute<MovieSearchCommand>, IExecute<MissingMovieSearchCommand>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly IMovieService _movieService;
        private readonly IQueueService _queueService;
        private readonly Logger _logger;

        public MovieSearchService(ISearchForNzb nzbSearchService,
                                  IProcessDownloadDecisions processDownloadDecisions,
                                  IMovieService movieService,
                                  IQueueService queueService,
                                    Logger logger)
        {
            _nzbSearchService = nzbSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _movieService = movieService;
            _queueService = queueService;
            _logger = logger;
        }

        private void SearchForMissingMovies(List<Movie> movies)
        {
            _logger.ProgressInfo("Performing missing search for {0} movies", movies.Count);
            var downloadedCount = 0;

            foreach (var movie in movies)
            {
                    List<DownloadDecision> decisions;

                    decisions = _nzbSearchService.MovieSearch(movie.Id);

                    var processed = _processDownloadDecisions.ProcessDecisions(decisions);

                    downloadedCount += processed.Grabbed.Count;
            }

            _logger.ProgressInfo("Completed missing search for {0} movies. {1} reports downloaded.", movies.Count, downloadedCount);
        }
        
        public void Execute(MovieSearchCommand message)
        {
            var decisions = _nzbSearchService.MovieSearch(message.MovieId);
            var processed = _processDownloadDecisions.ProcessDecisions(decisions);

            _logger.ProgressInfo("Episode search completed. {0} reports downloaded.", processed.Grabbed.Count);
        }

        public void Execute(MissingMovieSearchCommand message)
        {
            List<Movie> movies;

            if (message.MovieId.HasValue)
            {
                movies = _movieService.GetAllMovies()
                                      .Where(e => e.Id == message.MovieId &&
                                             e.Monitored &&
                                             e.MovieFileId == 0 &&
                                             e.ReleaseDate.Before(DateTime.UtcNow))
                                      .ToList();
            }
            else
            {
                movies = _movieService.MoviesWithoutFile(new PagingSpec<Movie>
                                                         {
                                                             Page = 1,
                                                             PageSize = 100000,
                                                             SortDirection = SortDirection.Ascending,
                                                             SortKey = "Id",
                                                             FilterExpression =
                                                                 v => v.Monitored == true
                                                         }).Records.ToList();
            }

            var queue = _queueService.GetQueue().OfType<MovieQueue>().Select(q => q.Media.Id);
            var missing = movies.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForMissingMovies(missing);
        }
    }
}
