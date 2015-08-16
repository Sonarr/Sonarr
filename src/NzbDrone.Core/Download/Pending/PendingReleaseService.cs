using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseService
    {
        void Add(DownloadDecision decision);
        List<ReleaseInfo> GetPending();
        List<Queue.Queue> GetPendingQueue();
        Queue.Queue FindPendingQueueItem(int queueId);
        void RemovePendingQueueItems(int queueId);
        RemoteItem OldestPendingRelease(RemoteItem item);
    }

    public class PendingReleaseService : IPendingReleaseService,
                                         IHandle<SeriesDeletedEvent>,
                                         IHandle<MovieDeletedEvent>,
                                         IHandle<EpisodeGrabbedEvent>,
                                         IHandle<MovieGrabbedEvent>,
                                         IHandle<RssSyncCompleteEvent>
    {
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IPendingReleaseRepository _repository;
        private readonly ISeriesService _seriesService;
        private readonly IMovieService _movieService;
        private readonly IParsingService _parsingService;
        private readonly IDelayProfileService _delayProfileService;
        private readonly ITaskManager _taskManager;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public PendingReleaseService(IIndexerStatusService indexerStatusService,
                                    IPendingReleaseRepository repository,
                                    ISeriesService seriesService,
                                    IMovieService movieService,
                                    IParsingService parsingService,
                                    IDelayProfileService delayProfileService,
                                    ITaskManager taskManager,
                                    IConfigService configService,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _indexerStatusService = indexerStatusService;
            _repository = repository;
            _seriesService = seriesService;
            _movieService = movieService;
            _parsingService = parsingService;
            _delayProfileService = delayProfileService;
            _taskManager = taskManager;
            _configService = configService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public RemoteItem OldestPendingRelease(RemoteItem item)
        {

            if (item is RemoteEpisode)
            {
                var episodeIds = (item as RemoteEpisode).Episodes.Select(e => e.Id);
                return OldestPendingSeriesRelease(item.Media.Id, episodeIds);
            }
            else if (item is RemoteMovie)
            {
                return OldestPendingMovieRelease(item.Media.Id);
            }

            return null;
        }

        public void Add(DownloadDecision decision)
        {
            var alreadyPending = GetPendingReleases();
            IEnumerable<int> decisionIds;
            IEnumerable<PendingRelease> existingReports = null;

            if (decision.RemoteItem is RemoteEpisode)
            {
                var episodes = decision.RemoteItem as RemoteEpisode;
                decisionIds = episodes.Episodes.Select(e => e.Id);

                existingReports = alreadyPending.Where(r => r.SeriesId == decision.RemoteItem.Media.Id && (r.RemoteItem as RemoteEpisode).Episodes.Select(e => e.Id)
                                    .Intersect(decisionIds)
                                    .Any());
            }
            else if (decision.RemoteItem is RemoteMovie)
            {
                existingReports = alreadyPending.Where((r => r.MovieId == decision.RemoteItem.Media.Id));
            }

            if (existingReports.Any(MatchingReleasePredicate(decision.RemoteItem.Release)))
            {
                _logger.Debug("This release is already pending, not adding again");
                return;
            }

            _logger.Debug("Adding release to pending releases");
            Insert(decision);
        }

        public List<ReleaseInfo> GetPending()
        {
            var releases = _repository.All().Select(p => p.Release).ToList();

            if (releases.Any())
            {
                releases = FilterBlockedIndexers(releases);
            }

            return releases;
        }

        private List<ReleaseInfo> FilterBlockedIndexers(List<ReleaseInfo> releases)
        {
            var blockedIndexers = new HashSet<int>(_indexerStatusService.GetBlockedIndexers().Select(v => v.IndexerId));

            return releases.Where(release => !blockedIndexers.Contains(release.IndexerId)).ToList();
        }

        private List<RemoteEpisode> GetPendingRemoteEpisodes(int seriesId)
        {
            return _repository.AllBySeriesId(seriesId).Select(GetRemoteEpisode).ToList();
        }

        private List<RemoteMovie> GetPendingRemoteMovies(int movieId)
        {
            return _repository.AllByMovieId(movieId).Select(GetRemoteMovie).ToList();
        }

        public List<Queue.Queue> GetPendingQueue()
        {
            var queued = new List<Queue.Queue>();

            var nextRssSync = new Lazy<DateTime>(() => _taskManager.GetNextExecution(typeof(RssSyncCommand)));

            foreach (var pendingRelease in GetPendingReleases())
            {
                if (pendingRelease.SeriesId > 0)
                {
                    var remoteEpisode = pendingRelease.RemoteItem as RemoteEpisode;
                    foreach (var episode in remoteEpisode.Episodes)
                    {
                        var ect = pendingRelease.Release.PublishDate.AddMinutes(GetDelay(pendingRelease.RemoteItem));

                        ect = ect < nextRssSync.Value ? nextRssSync.Value : ect.AddMinutes(_configService.RssSyncInterval);

                        var queue = new SeriesQueue
                        {
                            Id = GetQueueId(pendingRelease, episode.Id),
                            Media = pendingRelease.RemoteItem.Media,
                            Episode = episode,
                            Quality = pendingRelease.RemoteItem.ParsedInfo.Quality,
                            Title = pendingRelease.Title,
                            Size = pendingRelease.RemoteItem.Release.Size,
                            Sizeleft = pendingRelease.RemoteItem.Release.Size,
                            RemoteItem = pendingRelease.RemoteItem,
                            Timeleft = ect.Subtract(DateTime.UtcNow),
                            EstimatedCompletionTime = ect,
                            Status = "Pending",
                            Protocol = pendingRelease.RemoteItem.Release.DownloadProtocol
                        };
                        queued.Add(queue);
                    }
                }
                else if (pendingRelease.MovieId > 0)
                {
                    var ect = pendingRelease.Release.PublishDate.AddMinutes(GetDelay(pendingRelease.RemoteItem));
                    var remoteMovie = pendingRelease.RemoteItem as RemoteMovie;

                    ect = ect < nextRssSync.Value ? nextRssSync.Value : ect.AddMinutes(_configService.RssSyncInterval);

                    var queue = new MovieQueue
                    {
                        Id = GetQueueId(pendingRelease),
                        Media = pendingRelease.RemoteItem.Media,
                        Quality = pendingRelease.RemoteItem.ParsedInfo.Quality,
                        Title = pendingRelease.Title,
                        Size = pendingRelease.RemoteItem.Release.Size,
                        Sizeleft = pendingRelease.RemoteItem.Release.Size,
                        RemoteItem = pendingRelease.RemoteItem,
                        Timeleft = ect.Subtract(DateTime.UtcNow),
                        EstimatedCompletionTime = ect,
                        Status = "Pending",
                        Protocol = pendingRelease.RemoteItem.Release.DownloadProtocol
                    };
                    queued.Add(queue);
                }
            }

            //Return best quality release for each episode
            var dedupedSeries = queued.OfType<SeriesQueue>().GroupBy(q => q.Episode.Id).Select(g =>
            {
                var series = g.First().Media;

                return g.OrderByDescending(e => e.Quality, new QualityModelComparer(series.Profile))
                        .ThenBy(q => PrioritizeDownloadProtocol(q.Media.Tags, q.Protocol))
                        .First();
            });

            var dedupedMovies = queued.OfType<MovieQueue>().GroupBy(q => q.Media.Id).Select(g =>
            {
                var movie = g.First().Media;

                return g.OrderByDescending(e => e.Quality, new QualityModelComparer(movie.Profile))
                    .ThenBy(q => PrioritizeDownloadProtocol(q.Media.Tags, q.Protocol))
                    .First();
            });

            return dedupedSeries.Union(dedupedMovies.OfType<Queue.Queue>()).ToList();
        }

        public Queue.Queue FindPendingQueueItem(int queueId)
        {
            return GetPendingQueue().SingleOrDefault(p => p.Id == queueId);
        }

        private void RemovePendingQueueEpisodes(PendingRelease release)
        {
            var seriesReleases = _repository.AllBySeriesId(release.SeriesId);
            var targetParsedEpisodeInfo = release.RemoteItem.ParsedInfo as ParsedEpisodeInfo;

            var releasesToRemove = seriesReleases.Where(c =>
                {
                    var parsedEpisodeInfo = c.ParsedInfo as ParsedEpisodeInfo;
                    return parsedEpisodeInfo.SeasonNumber == targetParsedEpisodeInfo.SeasonNumber &&
                           parsedEpisodeInfo.EpisodeNumbers.SequenceEqual(targetParsedEpisodeInfo.EpisodeNumbers);
                });

            _repository.DeleteMany(releasesToRemove.Select(c => c.Id));
        }

        public void RemovePendingQueueItems(int queueId)
        {
            var targetItem = FindPendingRelease(queueId);

            if (targetItem.RemoteItem is RemoteEpisode)
            {
                RemovePendingQueueEpisodes(targetItem);
            }
            else if (targetItem.RemoteItem is RemoteMovie)
            {
                _repository.DeleteByMovieId(targetItem.MovieId);
            }
        }

        private RemoteEpisode OldestPendingSeriesRelease(int seriesId, IEnumerable<int> episodeIds)
        {
            return GetPendingRemoteEpisodes(seriesId).Where(r => r.Episodes.Select(e => e.Id).Intersect(episodeIds).Any())
                                                     .OrderByDescending(p => p.Release.AgeHours)
                                                     .FirstOrDefault();
        }

        private RemoteMovie OldestPendingMovieRelease(int movieId)
        {
            return GetPendingRemoteMovies(movieId).OrderByDescending(p => p.Release.AgeHours)
                                                  .FirstOrDefault();
        }

        private List<PendingRelease> GetPendingReleases()
        {
            var result = new List<PendingRelease>();

            foreach (var release in _repository.All())
            {
                if (release.SeriesId > 0)
                {
                    var remoteEpisode = GetRemoteEpisode(release);

                    if (remoteEpisode == null) continue;

                    release.RemoteItem = remoteEpisode;

                    result.Add(release);
                }
                else if (release.MovieId > 0)
                {
                    var remoteMovie = GetRemoteMovie(release);

                    if (remoteMovie == null) continue;

                    release.RemoteItem = remoteMovie;

                    result.Add(release);
                }
            }

            return result;
        }

        private RemoteEpisode GetRemoteEpisode(PendingRelease release)
        {
            var series = _seriesService.GetSeries(release.SeriesId);

            //Just in case the series was removed, but wasn't cleaned up yet (housekeeper will clean it up)
            if (series == null) return null;

            var episodes = _parsingService.GetEpisodes(release.ParsedInfo as ParsedEpisodeInfo, series, true);

            return new RemoteEpisode
            {
                Series = series,
                Episodes = episodes,
                ParsedInfo = release.ParsedInfo,
                Release = release.Release
            };
        }

        private RemoteMovie GetRemoteMovie(PendingRelease release)
        {
            var movie = _movieService.GetMovie(release.MovieId);

            //Just in case the movie was removed, but wasn't cleaned up yet (housekeeper will clean it up)
            if (movie == null) return null;

            return new RemoteMovie
            {
                Movie = movie,
                ParsedInfo = release.ParsedInfo,
                Release = release.Release
            };
        }

        private void Insert(DownloadDecision decision)
        {
            //TODO: Can we avoid having SeriesId here and use only ParsedInfo type?
            if (decision.RemoteItem is RemoteEpisode)
            {
                _repository.Insert(new PendingRelease
                {
                    SeriesId = decision.RemoteItem.Media.Id,
                    ParsedInfo = decision.RemoteItem.ParsedInfo,
                    Release = decision.RemoteItem.Release,
                    Title = decision.RemoteItem.Release.Title,
                    Added = DateTime.UtcNow
                });
            }

            else if (decision.RemoteItem is RemoteMovie)
            {
                _repository.Insert(new PendingRelease
                {
                    MovieId = decision.RemoteItem.Media.Id,
                    ParsedInfo = decision.RemoteItem.ParsedInfo,
                    Release = decision.RemoteItem.Release,
                    Title = decision.RemoteItem.Release.Title,
                    Added = DateTime.UtcNow
                });
            }

            _eventAggregator.PublishEvent(new PendingReleasesUpdatedEvent());
        }

        private void Delete(PendingRelease pendingRelease)
        {
            _repository.Delete(pendingRelease);
            _eventAggregator.PublishEvent(new PendingReleasesUpdatedEvent());
        }

        private static Func<PendingRelease, bool> MatchingReleasePredicate(ReleaseInfo release)
        {
            return p => p.Title == release.Title &&
                   p.Release.PublishDate == release.PublishDate &&
                   p.Release.Indexer == release.Indexer;
        }

        private int GetDelay(RemoteItem remoteItem)
        {
            var delayProfile = _delayProfileService.AllForTags(remoteItem.Media.Tags).OrderBy(d => d.Order).First();
            var delay = delayProfile.GetProtocolDelay(remoteItem.Release.DownloadProtocol);
            var minimumAge = _configService.MinimumAge;

            return new[] { delay, minimumAge }.Max();
        }

        private void RemoveGrabbed(RemoteEpisode remoteEpisode)
        {
            var pendingReleases = GetPendingReleases();
            var episodeIds = remoteEpisode.Episodes.Select(e => e.Id);

            var existingReports = pendingReleases.Where(r => r.SeriesId > 0 && (r.RemoteItem as RemoteEpisode).Episodes.Select(e => e.Id)
                                                             .Intersect(episodeIds)
                                                             .Any())
                                                             .ToList();

            if (existingReports.Empty())
            {
                return;
            }

            var profile = remoteEpisode.Series.Profile.Value;

            foreach (var existingReport in existingReports)
            {
                var compare = new QualityModelComparer(profile).Compare(remoteEpisode.ParsedEpisodeInfo.Quality,
                                                                        existingReport.RemoteItem.ParsedInfo.Quality);

                //Only remove lower/equal quality pending releases
                //It is safer to retry these releases on the next round than remove it and try to re-add it (if its still in the feed)
                if (compare >= 0)
                {
                    _logger.Debug("Removing previously pending release, as it was grabbed.");
                    Delete(existingReport);
                }
            }
        }

        private void RemoveGrabbed(RemoteMovie remoteMovie)
        {
            var pendingReleases = GetPendingReleases();

            var existingReports = pendingReleases.Where(r => r.MovieId > 0 && r.RemoteItem.Media.Id == remoteMovie.Movie.Id);

            if (existingReports.Empty())
            {
                return;
            }

            var profile = remoteMovie.Movie.Profile.Value;

            foreach (var existingReport in existingReports)
            {
                var compare = new QualityModelComparer(profile).Compare(remoteMovie.ParsedMovieInfo.Quality,
                                                                        existingReport.RemoteItem.ParsedInfo.Quality);

                //Only remove lower/equal quality pending releases
                //It is safer to retry these releases on the next round than remove it and try to re-add it (if its still in the feed)
                if (compare >= 0)
                {
                    _logger.Debug("Removing previously pending release, as it was grabbed.");
                    Delete(existingReport);
                }
            }
        }

        private void RemoveRejected(List<DownloadDecision> rejected)
        {
            _logger.Debug("Removing failed releases from pending");
            var pending = GetPendingReleases();

            foreach (var rejectedRelease in rejected)
            {
                ReleaseInfo release = rejectedRelease.RemoteItem.Release;
                var matching = pending.SingleOrDefault(MatchingReleasePredicate(release));

                if (matching != null)
                {
                    _logger.Debug("Removing previously pending release, as it has now been rejected.");
                    Delete(matching);
                }
            }
        }

        private bool ItemHasQueueId(PendingRelease item, int queueId)
        {
            var remoteEpisode = item.RemoteItem as RemoteEpisode;
            var remoteMovie = item.RemoteItem as RemoteMovie;

            if (remoteEpisode != null)
            {
                return remoteEpisode.Episodes.Any(episode => GetQueueId(item, episode.Id) == queueId);
            }
            else if (remoteMovie != null)
            {
                return GetQueueId(item, 0) == queueId;
            }

            return false;
        }

        private PendingRelease FindPendingRelease(int queueId)
        {
            return GetPendingReleases().First(p => ItemHasQueueId(p, queueId));
        }

        private int GetQueueId(PendingRelease pendingRelease)
        {
            return GetQueueId(pendingRelease, 0);
        }

        private int GetQueueId(PendingRelease pendingRelease, int id)
        {
            return HashConverter.GetHashInt31(String.Format("pending-{0}-ep{1}", pendingRelease.Id, id));
        }

        private int PrioritizeDownloadProtocol(HashSet<int> tags, DownloadProtocol downloadProtocol)
        {
            var delayProfile = _delayProfileService.BestForTags(tags);

            if (downloadProtocol == delayProfile.PreferredProtocol)
            {
                return 0;
            }

            return 1;
        }

        public void Handle(SeriesDeletedEvent message)
        {
            _repository.DeleteBySeriesId(message.Series.Id);
        }

        public void Handle(MovieDeletedEvent message)
        {
            _repository.DeleteByMovieId(message.Movie.Id);
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            RemoveGrabbed(message.Episode);
        }

        public void Handle(MovieGrabbedEvent message)
        {
            RemoveGrabbed(message.Movie);
        }

        public void Handle(RssSyncCompleteEvent message)
        {
            RemoveRejected(message.ProcessedDecisions.Rejected);
        }
    }
}
