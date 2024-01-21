using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download.Aggregation;
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
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IRemoteEpisodeAggregationService _aggregationService;
        private readonly IDownloadClientFactory _downloadClientFactory;
        private readonly IIndexerFactory _indexerFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public PendingReleaseService(IIndexerStatusService indexerStatusService,
                                    IPendingReleaseRepository repository,
                                    ISeriesService seriesService,
                                    IParsingService parsingService,
                                    IDelayProfileService delayProfileService,
                                    ITaskManager taskManager,
                                    IConfigService configService,
                                    ICustomFormatCalculationService formatCalculator,
                                    IRemoteEpisodeAggregationService aggregationService,
                                    IDownloadClientFactory downloadClientFactory,
                                    IIndexerFactory indexerFactory,
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
            _formatCalculator = formatCalculator;
            _aggregationService = aggregationService;
            _downloadClientFactory = downloadClientFactory;
            _indexerFactory = indexerFactory;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void Add(DownloadDecision decision, PendingReleaseReason reason)
        {
            AddMany(new List<Tuple<DownloadDecision, PendingReleaseReason>> { Tuple.Create(decision, reason) });
        }

        public void AddMany(List<Tuple<DownloadDecision, PendingReleaseReason>> decisions)
        {
            foreach (var seriesDecisions in decisions.GroupBy(v => v.Item1.RemoteEpisode.Series.Id))
            {
                var series = seriesDecisions.First().Item1.RemoteEpisode.Series;
                var alreadyPending = _repository.AllBySeriesId(series.Id);

                alreadyPending = IncludeRemoteEpisodes(alreadyPending, seriesDecisions.ToDictionaryIgnoreDuplicates(v => v.Item1.RemoteEpisode.Release.Title, v => v.Item1.RemoteEpisode));
                var alreadyPendingByEpisode = CreateEpisodeLookup(alreadyPending);

                foreach (var pair in seriesDecisions)
                {
                    var decision = pair.Item1;
                    var reason = pair.Item2;

                    var episodeIds = decision.RemoteEpisode.Episodes.Select(e => e.Id);

                    var existingReports = episodeIds.SelectMany(v => alreadyPendingByEpisode[v])
                                                    .Distinct().ToList();

                    var matchingReports = existingReports.Where(MatchingReleasePredicate(decision.RemoteEpisode.Release)).ToList();

                    if (matchingReports.Any())
                    {
                        var matchingReport = matchingReports.First();

                        if (matchingReport.Reason != reason)
                        {
                            if (matchingReport.Reason == PendingReleaseReason.DownloadClientUnavailable)
                            {
                                _logger.Debug("The release {0} is already pending with reason {1}, not changing reason", decision.RemoteEpisode, matchingReport.Reason);
                            }
                            else
                            {
                                _logger.Debug("The release {0} is already pending with reason {1}, changing to {2}", decision.RemoteEpisode, matchingReport.Reason, reason);
                                matchingReport.Reason = reason;
                                _repository.Update(matchingReport);
                            }
                        }
                        else
                        {
                            _logger.Debug("The release {0} is already pending with reason {1}, not adding again", decision.RemoteEpisode, reason);
                        }

                        if (matchingReports.Count > 1)
                        {
                            _logger.Debug("The release {0} had {1} duplicate pending, removing duplicates.", decision.RemoteEpisode, matchingReports.Count - 1);

                            foreach (var duplicate in matchingReports.Skip(1))
                            {
                                _repository.Delete(duplicate.Id);
                                alreadyPending.Remove(duplicate);
                                alreadyPendingByEpisode = CreateEpisodeLookup(alreadyPending);
                            }
                        }

                        continue;
                    }

                    _logger.Debug("Adding release {0} to pending releases with reason {1}", decision.RemoteEpisode, reason);
                    Insert(decision, reason);
                }
            }
        }

        public List<ReleaseInfo> GetPending()
        {
            var releases = _repository.All().Select(p =>
            {
                var release = p.Release;

                release.PendingReleaseReason = p.Reason;

                return release;
            }).ToList();

            if (releases.Any())
            {
                releases = FilterBlockedIndexers(releases);
            }

            return releases;
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
                if (pendingRelease.RemoteEpisode.Episodes.Empty())
                {
                    var noEpisodeItem = GetQueueItem(pendingRelease, nextRssSync, null);

                    noEpisodeItem.ErrorMessage = "Unable to find matching episode(s)";

                    queued.Add(noEpisodeItem);

                    continue;
                }

                foreach (var episode in pendingRelease.RemoteEpisode.Episodes)
                {
                    queued.Add(GetQueueItem(pendingRelease, nextRssSync, episode));
                }
            }

            // Return best quality release for each episode
            var deduped = queued.Where(q => q.Episode != null).GroupBy(q => q.Episode.Id).Select(g =>
            {
                var series = g.First().Series;

                return g.OrderByDescending(e => e.Quality, new QualityModelComparer(series.QualityProfile))
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
                                 .MaxBy(p => p.Release.AgeHours);
        }

        private ILookup<int, PendingRelease> CreateEpisodeLookup(IEnumerable<PendingRelease> alreadyPending)
        {
            return alreadyPending.SelectMany(v => v.RemoteEpisode.Episodes
                                                   .Select(d => new { Episode = d, PendingRelease = v }))
                                 .ToLookup(v => v.Episode.Id, v => v.PendingRelease);
        }

        private List<ReleaseInfo> FilterBlockedIndexers(List<ReleaseInfo> releases)
        {
            var blockedIndexers = new HashSet<int>(_indexerStatusService.GetBlockedProviders().Select(v => v.ProviderId));

            return releases.Where(release => !blockedIndexers.Contains(release.IndexerId)).ToList();
        }

        private List<PendingRelease> GetPendingReleases()
        {
            return IncludeRemoteEpisodes(_repository.All().ToList());
        }

        private List<PendingRelease> GetPendingReleases(int seriesId)
        {
            return IncludeRemoteEpisodes(_repository.AllBySeriesId(seriesId).ToList());
        }

        private List<PendingRelease> IncludeRemoteEpisodes(List<PendingRelease> releases, Dictionary<string, RemoteEpisode> knownRemoteEpisodes = null)
        {
            var result = new List<PendingRelease>();

            var seriesMap = new Dictionary<int, Series>();

            if (knownRemoteEpisodes != null)
            {
                foreach (var series in knownRemoteEpisodes.Values.Select(v => v.Series))
                {
                    if (!seriesMap.ContainsKey(series.Id))
                    {
                        seriesMap[series.Id] = series;
                    }
                }
            }

            foreach (var series in _seriesService.GetSeries(releases.Select(v => v.SeriesId).Distinct().Where(v => !seriesMap.ContainsKey(v))))
            {
                seriesMap[series.Id] = series;
            }

            foreach (var release in releases)
            {
                var series = seriesMap.GetValueOrDefault(release.SeriesId);

                // Just in case the series was removed, but wasn't cleaned up yet (housekeeper will clean it up)
                if (series == null)
                {
                    return null;
                }

                // Languages will be empty if added before upgrading to v4, reparsing the languages if they're empty will set it to Unknown or better.
                if (release.ParsedEpisodeInfo.Languages.Empty())
                {
                    release.ParsedEpisodeInfo.Languages = LanguageParser.ParseLanguages(release.Title);
                }

                release.RemoteEpisode = new RemoteEpisode
                {
                    Series = series,
                    SeriesMatchType = release.AdditionalInfo?.SeriesMatchType ?? SeriesMatchType.Unknown,
                    ReleaseSource = release.AdditionalInfo?.ReleaseSource ?? ReleaseSourceType.Unknown,
                    ParsedEpisodeInfo = release.ParsedEpisodeInfo,
                    Release = release.Release
                };

                if (knownRemoteEpisodes != null && knownRemoteEpisodes.TryGetValue(release.Release.Title, out var knownRemoteEpisode))
                {
                    release.RemoteEpisode.MappedSeasonNumber = knownRemoteEpisode.MappedSeasonNumber;
                    release.RemoteEpisode.Episodes = knownRemoteEpisode.Episodes;
                }
                else if (ValidateParsedEpisodeInfo.ValidateForSeriesType(release.ParsedEpisodeInfo, series))
                {
                    try
                    {
                        var remoteEpisode = _parsingService.Map(release.ParsedEpisodeInfo, series);

                        release.RemoteEpisode.MappedSeasonNumber = remoteEpisode.MappedSeasonNumber;
                        release.RemoteEpisode.Episodes = remoteEpisode.Episodes;
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.Debug(ex, ex.Message);

                        release.RemoteEpisode.MappedSeasonNumber = release.ParsedEpisodeInfo.SeasonNumber;
                        release.RemoteEpisode.Episodes = new List<Episode>();
                    }
                }
                else
                {
                    release.RemoteEpisode.MappedSeasonNumber = release.ParsedEpisodeInfo.SeasonNumber;
                    release.RemoteEpisode.Episodes = new List<Episode>();
                }

                _aggregationService.Augment(release.RemoteEpisode);
                release.RemoteEpisode.CustomFormats = _formatCalculator.ParseCustomFormat(release.RemoteEpisode, release.Release.Size);

                result.Add(release);
            }

            return result;
        }

        private Queue.Queue GetQueueItem(PendingRelease pendingRelease, Lazy<DateTime> nextRssSync, Episode episode)
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

            string downloadClientName = null;
            var indexer = _indexerFactory.Find(pendingRelease.Release.IndexerId);

            if (indexer is { DownloadClientId: > 0 })
            {
                var downloadClient = _downloadClientFactory.Find(indexer.DownloadClientId);

                downloadClientName = downloadClient?.Name;
            }

            var queue = new Queue.Queue
            {
                Id = GetQueueId(pendingRelease, episode),
                Series = pendingRelease.RemoteEpisode.Series,
                Episode = episode,
                Languages = pendingRelease.RemoteEpisode.Languages,
                Quality = pendingRelease.RemoteEpisode.ParsedEpisodeInfo.Quality,
                Title = pendingRelease.Title,
                Size = pendingRelease.RemoteEpisode.Release.Size,
                Sizeleft = pendingRelease.RemoteEpisode.Release.Size,
                RemoteEpisode = pendingRelease.RemoteEpisode,
                Timeleft = timeleft,
                EstimatedCompletionTime = ect,
                Added = pendingRelease.Added,
                Status = pendingRelease.Reason.ToString(),
                Protocol = pendingRelease.RemoteEpisode.Release.DownloadProtocol,
                Indexer = pendingRelease.RemoteEpisode.Release.Indexer,
                DownloadClient = downloadClientName
            };

            return queue;
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
                Reason = reason,
                AdditionalInfo = new PendingReleaseAdditionalInfo
                {
                    SeriesMatchType = decision.RemoteEpisode.SeriesMatchType,
                    ReleaseSource = decision.RemoteEpisode.ReleaseSource
                }
            });

            _eventAggregator.PublishEvent(new PendingReleasesUpdatedEvent());
        }

        private void Delete(PendingRelease pendingRelease)
        {
            _repository.Delete(pendingRelease);
            _eventAggregator.PublishEvent(new PendingReleasesUpdatedEvent());
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

            var profile = remoteEpisode.Series.QualityProfile;

            foreach (var existingReport in existingReports)
            {
                var compare = new QualityModelComparer(profile).Compare(remoteEpisode.ParsedEpisodeInfo.Quality,
                                                                        existingReport.RemoteEpisode.ParsedEpisodeInfo.Quality);

                // Only remove lower/equal quality pending releases
                // It is safer to retry these releases on the next round than remove it and try to re-add it (if its still in the feed)
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
            return HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", pendingRelease.Id, episode?.Id ?? 0));
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
            _repository.DeleteBySeriesIds(message.Series.Select(m => m.Id).ToList());
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            RemoveGrabbed(message.Episode);
        }

        public void Handle(RssSyncCompleteEvent message)
        {
            RemoveRejected(message.ProcessedDecisions.Rejected);
        }

        private static Func<PendingRelease, bool> MatchingReleasePredicate(ReleaseInfo release)
        {
            return p => p.Title == release.Title &&
                        p.Release.PublishDate == release.PublishDate &&
                        p.Release.Indexer == release.Indexer;
        }
    }
}
