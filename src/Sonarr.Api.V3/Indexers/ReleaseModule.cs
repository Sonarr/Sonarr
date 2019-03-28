using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Nancy;
using Nancy.ModelBinding;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Sonarr.Api.V3.Indexers
{
    public class ReleaseModule : ReleaseModuleBase
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IDownloadService _downloadService;
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;
        private ResourceValidator<ReleaseResource> _releaseValidator;

        private readonly ICached<RemoteEpisode> _remoteEpisodeCache;

        public ReleaseModule(IFetchAndParseRss rssFetcherAndParser,
                             ISearchForNzb nzbSearchService,
                             IMakeDownloadDecision downloadDecisionMaker,
                             IPrioritizeDownloadDecision prioritizeDownloadDecision,
                             IDownloadService downloadService,
                             ISeriesService seriesService,
                             IEpisodeService episodeService,
                             IParsingService parsingService,
                             ICacheManager cacheManager,
                             Logger logger)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _nzbSearchService = nzbSearchService;
            _downloadDecisionMaker = downloadDecisionMaker;
            _prioritizeDownloadDecision = prioritizeDownloadDecision;
            _downloadService = downloadService;
            _seriesService = seriesService;
            _episodeService = episodeService;
            _parsingService = parsingService;
            _logger = logger;

            _releaseValidator = new ResourceValidator<ReleaseResource>();
            _releaseValidator.RuleFor(s => s.DownloadAllowed).Equal(true);
            _releaseValidator.RuleFor(s => s.IndexerId).ValidId();
            _releaseValidator.RuleFor(s => s.Guid).NotEmpty();

            GetResourceAll = GetReleases;
            Post["/"] = x => DownloadRelease(this.Bind<ReleaseResource>());

            _remoteEpisodeCache = cacheManager.GetCache<RemoteEpisode>(GetType(), "remoteEpisodes");
        }

        private Response DownloadRelease(ReleaseResource release)
        {
            var validationFailures = _releaseValidator.Validate(release).Errors;

            if (validationFailures.Any())
            {
                throw new ValidationException(validationFailures);
            }

            var remoteEpisode = _remoteEpisodeCache.Find(GetCacheKey(release));

            if (remoteEpisode == null)
            {
                _logger.Debug("Couldn't find requested release in cache, cache timeout probably expired.");

                throw new NzbDroneClientException(HttpStatusCode.NotFound, "Couldn't find requested release in cache, try searching again");
            }

            try
            {
                if (remoteEpisode.Series == null)
                {
                    if (release.EpisodeId.HasValue)
                    {
                        var episode = _episodeService.GetEpisode(release.EpisodeId.Value);

                        remoteEpisode.Series = _seriesService.GetSeries(episode.SeriesId);
                        remoteEpisode.Episodes = new List<Episode> { episode };
                    }
                    else if (release.SeriesId.HasValue)
                    {
                        var series = _seriesService.GetSeries(release.SeriesId.Value);
                        var episodes = _parsingService.GetEpisodes(remoteEpisode.ParsedEpisodeInfo, series, true);

                        if (episodes.Empty())
                        {
                            throw new NzbDroneClientException(HttpStatusCode.NotFound, "Unable to parse episodes in the release");
                        }

                        remoteEpisode.Series = series;
                        remoteEpisode.Episodes = episodes;
                    }
                    else
                    {
                            throw new NzbDroneClientException(HttpStatusCode.NotFound, "Unable to find matching series and episodes");
                    }
                }

                _downloadService.DownloadReport(remoteEpisode);
            }
            catch (ReleaseDownloadException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Getting release from indexer failed");
            }

            return release.AsResponse();
        }

        private List<ReleaseResource> GetReleases()
        {
            if (Request.Query.episodeId.HasValue)
            {
                return GetEpisodeReleases(Request.Query.episodeId);
            }

            if (Request.Query.seriesId.HasValue && Request.Query.seasonNumber.HasValue)
            {
                return GetSeasonReleases(Request.Query.seriesId, Request.Query.seasonNumber);
            }

            return GetRss();
        }

        private List<ReleaseResource> GetEpisodeReleases(int episodeId)
        {
            try
            {
                var decisions = _nzbSearchService.EpisodeSearch(episodeId, true, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Episode search failed: " + ex.Message);
            }

            return new List<ReleaseResource>();
        }

        private List<ReleaseResource> GetSeasonReleases(int seriesId, int seasonNumber)
        {
            try
            {
                var decisions = _nzbSearchService.SeasonSearch(seriesId, seasonNumber, false, true, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Season search failed: " + ex.Message);
            }

            return new List<ReleaseResource>();
        }

        private List<ReleaseResource> GetRss()
        {
            var reports = _rssFetcherAndParser.Fetch();
            var decisions = _downloadDecisionMaker.GetRssDecision(reports);
            var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

            return MapDecisions(prioritizedDecisions);
        }

        protected override ReleaseResource MapDecision(DownloadDecision decision, int initialWeight)
        {
            var resource = base.MapDecision(decision, initialWeight);
            _remoteEpisodeCache.Set(GetCacheKey(resource), decision.RemoteEpisode, TimeSpan.FromMinutes(30));

            return resource;
        }

        private string GetCacheKey(ReleaseResource resource)
        {
            return string.Concat(resource.IndexerId, "_", resource.Guid);
        }
    }
}
