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
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseService
    {
        void Add(DownloadDecision decision, PendingReleaseReason reason);
        void AddMany(List<Tuple<DownloadDecision, PendingReleaseReason>> decisions);
        List<ReleaseInfo> GetPending();
        List<RemoteEpisode> GetPendingRemoteEpisodes(int seriesId);
        List<Queue.Queue> GetPendingQueue();
        Queue.Queue FindPendingQueueItem(int queueId);
        void RemovePendingQueueItems(int queueId);
        RemoteEpisode OldestPendingRelease(int seriesId, int[] episodeIds);
    }

    public class PendingReleaseService : IPendingReleaseService,
                                         IHandle<SeriesDeletedEvent>,
                                         IHandle<EpisodeGrabbedEvent>,
                                         IHandle<RssSyncCompleteEvent>
    {
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IPendingReleaseRepository _repository;
        private readonly ISeriesService _seriesService;
        private readonly IParsingService _parsingService;
        private readonly IDelayProfileService _delayProfileService;
        private readonly ITaskManager _taskManager;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public PendingReleaseService(IIndexerStatusService indexerStatusService,
                                    IPendingReleaseRepository repository,
                                    ISeriesService seriesService,
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
            _parsingService = parsingService;
            _delayProfileService = delayProfileService;
            _taskManager = taskManager;
            _configService = configService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }


        public void Add(DownloadDecision decision, PendingReleaseReason reason)
        {
            var alreadyPending = _repository.AllBySeriesId(decision.RemoteEpisode.Series.Id);

            alreadyPending = IncludeRemoteEpisodes(alreadyPending);

            Add(alreadyPending, decision, reason);
        }

        public void AddMany(List<Tuple<DownloadDecision, PendingReleaseReason>> decisions)
        {
            var alreadyPending = decisions.Select(v => v.Item1.RemoteEpisode.Series.Id).Distinct().SelectMany(_repository.AllBySeriesId).ToList();

            alreadyPending = IncludeRemoteEpisodes(alreadyPending);

            foreach (var pair in decisions)
            {
                Add(alreadyPending, pair.Item1, pair.Item2);
            }
        }

        private void Add(List<PendingRelease> alreadyPending, DownloadDecision decision, PendingReleaseReason reason)
        {
            var episodeIds = decision.RemoteEpisode.Episodes.Select(e => e.Id);

            var existingReports = alreadyPending.Where(r => r.RemoteEpisode.Episodes.Select(e => e.Id)
                                                             .Intersect(episodeIds)
                                                             .Any());

            var matchingReports = existingReports.Where(MatchingReleasePredicate(decision.RemoteEpisode.Release)).ToList();

            if (matchingReports.Any())
            {
                var matchingReport = matchingReports.First();

                if (matchingReport.Reason != reason)
                {
                    _logger.Debug("The release {0} is already pending with reason {1}, changing to {2}", decision.RemoteEpisode, matchingReport.Reason, reason);
                    matchingReport.Reason = reason;
                    _repository.Update(matchingReport);
                }
                else
                {
                    _logger.Debug("The release {0} is already pending with reason {1}, not adding again", decision.RemoteEpisode, reason);
                }

                if (matchingReports.Count() > 1)
                {
                    _logger.Debug("The release {0} had {1} duplicate pending, removing duplicates.", decision.RemoteEpisode, matchingReports.Count() - 1);

                    foreach (var duplicate in matchingReports.Skip(1))
                    {
                        _repository.Delete(duplicate.Id);
                        alreadyPending.Remove(duplicate);
                    }
                }

                return;
            }

            _logger.Debug("Adding release {0} to pending releases with reason {1}", decision.RemoteEpisode, reason);
            Insert(decision, reason);
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
            var blockedIndexers = new HashSet<int>(_indexerStatusService.GetBlockedProviders().Select(v => v.ProviderId));

            return releases.Where(release => !blockedIndexers.Contains(release.IndexerId)).ToList();
        }

        public List<RemoteEpisode> GetPendingRemoteEpisodes(int seriesId)
        {
            return IncludeRemoteEpisodes(_repository.AllBySeriesId(seriesId)).Select(v => v.RemoteEpisode).ToList();
        }

        public List<Queue.Queue> GetPendingQueue()
        {
            var queued = new List<Queue.Queue>();

            var nextRssSync = new Lazy<DateTime>(() => _taskManager.GetNextExecution(typeof(RssSyncCommand)));

            var pendingReleases = IncludeRemoteEpisodes(_repository.WithoutFallback());
            foreach (var pendingRelease in pendingReleases)
            {
                foreach (var episode in pendingRelease.RemoteEpisode.Episodes)
                {
                    var ect = pendingRelease.Release.PublishDate.AddMinutes(GetDelay(pendingRelease.RemoteEpisode));

                    if (ect < nextRssSync.Value)
                    {
                        ect = nextRssSync.Value;
                    }
                    else
                    {
                        ect = ect.AddMinutes(_configService.RssSyncInterval);
                    }

                    var timeleft = ect.Subtract(DateTime.UtcNow);

                    if (timeleft.TotalSeconds < 0)
                    {
                        timeleft = TimeSpan.Zero;
                    }

                    var queue = new Queue.Queue
                                {
                                    Id = GetQueueId(pendingRelease, episode),
                                    Series = pendingRelease.RemoteEpisode.Series,
                                    Episode = episode,
                                    Quality = pendingRelease.RemoteEpisode.ParsedEpisodeInfo.Quality,
                                    Title = pendingRelease.Title,
                                    Size = pendingRelease.RemoteEpisode.Release.Size,
                                    Sizeleft = pendingRelease.RemoteEpisode.Release.Size,
                                    RemoteEpisode = pendingRelease.RemoteEpisode,
                                    Timeleft = timeleft,
                                    EstimatedCompletionTime = ect,
                                    Status = pendingRelease.Reason.ToString(),
                                    Protocol = pendingRelease.RemoteEpisode.Release.DownloadProtocol,
                                    Indexer = pendingRelease.RemoteEpisode.Release.Indexer
                                };

                    queued.Add(queue);
                }
            }

            //Return best quality release for each episode
            var deduped = queued.GroupBy(q => q.Episode.Id).Select(g =>
            {
                var series = g.First().Series;

                return g.OrderByDescending(e => e.Quality, new QualityModelComparer(series.Profile))
                        .ThenBy(q => PrioritizeDownloadProtocol(q.Series, q.Protocol))
                        .First();
            });

            return deduped.ToList();
        }

        public Queue.Queue FindPendingQueueItem(int queueId)
        {
            return GetPendingQueue().SingleOrDefault(p => p.Id == queueId);
        }

        public void RemovePendingQueueItems(int queueId)
        {
            var targetItem = FindPendingRelease(queueId);
            var seriesReleases = _repository.AllBySeriesId(targetItem.SeriesId);

            var releasesToRemove = seriesReleases.Where(
                c => c.ParsedEpisodeInfo.SeasonNumber == targetItem.ParsedEpisodeInfo.SeasonNumber &&
                     c.ParsedEpisodeInfo.EpisodeNumbers.SequenceEqual(targetItem.ParsedEpisodeInfo.EpisodeNumbers));

            _repository.DeleteMany(releasesToRemove.Select(c => c.Id));
        }

        public RemoteEpisode OldestPendingRelease(int seriesId, int[] episodeIds)
        {
            var seriesReleases = GetPendingReleases(seriesId);

            return seriesReleases.Select(r => r.RemoteEpisode)
                                 .Where(r => r.Episodes.Select(e => e.Id).Intersect(episodeIds).Any())
                                 .OrderByDescending(p => p.Release.AgeHours)
                                 .FirstOrDefault();
        }

        private List<PendingRelease> GetPendingReleases()
        {
            return IncludeRemoteEpisodes(_repository.All().ToList());
        }

        private List<PendingRelease> GetPendingReleases(int seriesId)
        {
            return IncludeRemoteEpisodes(_repository.AllBySeriesId(seriesId).ToList());
        }

        private List<PendingRelease> IncludeRemoteEpisodes(List<PendingRelease> releases)
        {
            var result = new List<PendingRelease>();
            var seriesMap = _seriesService.GetSeries(releases.Select(v => v.SeriesId).Distinct())
                                          .ToDictionary(v => v.Id);

            foreach (var release in releases)
            {
                var series = seriesMap.GetValueOrDefault(release.SeriesId);

                // Just in case the series was removed, but wasn't cleaned up yet (housekeeper will clean it up)
                if (series == null) return null;

                var episodes = _parsingService.GetEpisodes(release.ParsedEpisodeInfo, series, true);

                release.RemoteEpisode = new RemoteEpisode
                {
                    Series = series,
                    Episodes = episodes,
                    ParsedEpisodeInfo = release.ParsedEpisodeInfo,
                    Release = release.Release
                };

                result.Add(release);
            }

            return result;
        }

        private void Insert(DownloadDecision decision, PendingReleaseReason reason)
        {
            _repository.Insert(new PendingRelease
            {
                SeriesId = decision.RemoteEpisode.Series.Id,
                ParsedEpisodeInfo = decision.RemoteEpisode.ParsedEpisodeInfo,
                Release = decision.RemoteEpisode.Release,
                Title = decision.RemoteEpisode.Release.Title,
                Added = DateTime.UtcNow,
                Reason = reason
            });

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

        private int GetDelay(RemoteEpisode remoteEpisode)
        {
            var delayProfile = _delayProfileService.AllForTags(remoteEpisode.Series.Tags).OrderBy(d => d.Order).First();
            var delay = delayProfile.GetProtocolDelay(remoteEpisode.Release.DownloadProtocol);
            var minimumAge = _configService.MinimumAge;

            return new[] { delay, minimumAge }.Max();
        }

        private void RemoveGrabbed(RemoteEpisode remoteEpisode)
        {
            var pendingReleases = GetPendingReleases(remoteEpisode.Series.Id);
            var episodeIds = remoteEpisode.Episodes.Select(e => e.Id);

            var existingReports = pendingReleases.Where(r => r.RemoteEpisode.Episodes.Select(e => e.Id)
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
                                                                        existingReport.RemoteEpisode.ParsedEpisodeInfo.Quality);

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
                var matching = pending.Where(MatchingReleasePredicate(rejectedRelease.RemoteEpisode.Release));

                foreach (var pendingRelease in matching)
                {
                    _logger.Debug("Removing previously pending release, as it has now been rejected.");
                    Delete(pendingRelease);
                }
            }
        }

        private PendingRelease FindPendingRelease(int queueId)
        {
            return GetPendingReleases().First(p => p.RemoteEpisode.Episodes.Any(e => queueId == GetQueueId(p, e)));
        }

        private int GetQueueId(PendingRelease pendingRelease, Episode episode)
        {
            return HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", pendingRelease.Id, episode.Id));
        }

        private int PrioritizeDownloadProtocol(Series series, DownloadProtocol downloadProtocol)
        {
            var delayProfile = _delayProfileService.BestForTags(series.Tags);

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

        public void Handle(EpisodeGrabbedEvent message)
        {
            RemoveGrabbed(message.Episode);
        }

        public void Handle(RssSyncCompleteEvent message)
        {
            RemoveRejected(message.ProcessedDecisions.Rejected);
        }
    }
}
