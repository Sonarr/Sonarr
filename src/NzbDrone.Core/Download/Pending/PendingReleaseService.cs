using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseService
    {
        void Add(DownloadDecision decision);
        void RemoveGrabbed(List<DownloadDecision> grabbed);
        void RemoveRejected(List<DownloadDecision> rejected);
        List<ReleaseInfo> GetPending();
        List<RemoteEpisode> GetPendingRemoteEpisodes(Int32 seriesId);
        List<Queue.Queue> GetPendingQueue();
    }

    public class PendingReleaseService : IPendingReleaseService, IHandle<SeriesDeletedEvent>
    {
        private readonly IPendingReleaseRepository _repository;
        private readonly ISeriesService _seriesService;
        private readonly IParsingService _parsingService;
        private readonly IDownloadService _downloadService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public PendingReleaseService(IPendingReleaseRepository repository,
                                    ISeriesService seriesService,
                                    IParsingService parsingService,
                                    IDownloadService downloadService,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _repository = repository;
            _seriesService = seriesService;
            _parsingService = parsingService;
            _downloadService = downloadService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void Add(DownloadDecision decision)
        {
            var alreadyPending = GetPendingReleases();

            var episodeIds = decision.RemoteEpisode.Episodes.Select(e => e.Id);

            var existingReports = alreadyPending.Where(r => r.RemoteEpisode.Episodes.Select(e => e.Id)
                                                             .Intersect(episodeIds)
                                                             .Any());

            if (existingReports.Any(MatchingReleasePredicate(decision)))
            {
                _logger.Debug("This release is already pending, not adding again");
                return;
            }

            _logger.Debug("Adding release to pending releases");
            Insert(decision);
        }

        public void RemoveGrabbed(List<DownloadDecision> grabbed)
        {
            _logger.Debug("Removing grabbed releases from pending");
            var alreadyPending = GetPendingReleases();

            foreach (var decision in grabbed)
            {
                var decisionLocal = decision;
                var episodeIds = decisionLocal.RemoteEpisode.Episodes.Select(e => e.Id);


                var existingReports = alreadyPending.Where(r => r.RemoteEpisode.Episodes.Select(e => e.Id)
                                                                 .Intersect(episodeIds)
                                                                 .Any());

                foreach (var existingReport in existingReports)
                {
                    _logger.Debug("Removing previously pending release, as it was grabbed.");
                    Delete(existingReport);
                }
            }
        }

        public void RemoveRejected(List<DownloadDecision> rejected)
        {
            _logger.Debug("Removing failed releases from pending");
            var pending = GetPendingReleases();

            foreach (var rejectedRelease in rejected)
            {
                var matching = pending.SingleOrDefault(MatchingReleasePredicate(rejectedRelease));

                if (matching != null)
                {
                    _logger.Debug("Removing previously pending release, as it has now been rejected.");
                    Delete(matching);
                }
            }
        }

        public List<ReleaseInfo> GetPending()
        {
            return _repository.All().Select(p => p.Release).ToList();
        }

        public List<RemoteEpisode> GetPendingRemoteEpisodes(int seriesId)
        {
            return _repository.AllBySeriesId(seriesId).Select(GetRemoteEpisode).ToList();
        }

        public List<Queue.Queue> GetPendingQueue()
        {
            var queued = new List<Queue.Queue>();

            foreach (var pendingRelease in GetPendingReleases())
            {
                foreach (var episode in pendingRelease.RemoteEpisode.Episodes)
                {
                    var queue = new Queue.Queue
                                {
                                    Id = episode.Id ^ (pendingRelease.Id << 16),
                                    Series = pendingRelease.RemoteEpisode.Series,
                                    Episode = episode,
                                    Quality = pendingRelease.RemoteEpisode.ParsedEpisodeInfo.Quality,
                                    Title = pendingRelease.Title,
                                    Size = pendingRelease.RemoteEpisode.Release.Size,
                                    Sizeleft = pendingRelease.RemoteEpisode.Release.Size,
                                    Timeleft =
                                        pendingRelease.Release.PublishDate.AddHours(
                                            pendingRelease.RemoteEpisode.Series.Profile.Value.GrabDelay)
                                                      .Subtract(DateTime.UtcNow),
                                    Status = "Pending",
                                    RemoteEpisode = pendingRelease.RemoteEpisode
                                };
                    queued.Add(queue);
                }
            }

            return queued;
        }

        private List<PendingRelease> GetPendingReleases()
        {
            var result = new List<PendingRelease>();

            foreach (var release in _repository.All())
            {
                var remoteEpisode = GetRemoteEpisode(release);

                if (remoteEpisode == null) continue;

                release.RemoteEpisode = remoteEpisode;

                result.Add(release);
            }

            return result;
        }

        private RemoteEpisode GetRemoteEpisode(PendingRelease release)
        {
            var series = _seriesService.GetSeries(release.SeriesId);

            //Just in case the series was removed, but wasn't cleaned up yet (housekeeper will clean it up)
            if (series == null) return null;

            var episodes = _parsingService.GetEpisodes(release.ParsedEpisodeInfo, series, true);

            return new RemoteEpisode
            {
                Series = series,
                Episodes = episodes,
                ParsedEpisodeInfo = release.ParsedEpisodeInfo,
                Release = release.Release
            };
        }

        private void Insert(DownloadDecision decision)
        {
            _repository.Insert(new PendingRelease
            {
                SeriesId = decision.RemoteEpisode.Series.Id,
                ParsedEpisodeInfo = decision.RemoteEpisode.ParsedEpisodeInfo,
                Release = decision.RemoteEpisode.Release,
                Title = decision.RemoteEpisode.Release.Title,
                Added = DateTime.UtcNow
            });

            _eventAggregator.PublishEvent(new PendingReleasesUpdatedEvent());
        }

        private void Delete(PendingRelease pendingRelease)
        {
            _repository.Delete(pendingRelease);
            _eventAggregator.PublishEvent(new PendingReleasesUpdatedEvent());
        }

        private Func<PendingRelease, bool> MatchingReleasePredicate(DownloadDecision decision)
        {
            return p => p.Title == decision.RemoteEpisode.Release.Title &&
                   p.Release.PublishDate == decision.RemoteEpisode.Release.PublishDate &&
                   p.Release.Indexer == decision.RemoteEpisode.Release.Indexer;
        }

        public void Handle(SeriesDeletedEvent message)
        {
            _repository.DeleteBySeriesId(message.Series.Id);
        }
    }
}
