using System;
using System.Collections.Generic;
using FluentValidation;
using Nancy;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using Omu.ValueInjecter;
using System.Linq;
using Nancy.ModelBinding;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.Cache;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace NzbDrone.Api.Indexers
{
    public class ReleaseModule : NzbDroneRestModule<ReleaseResource>
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IDownloadService _downloadService;
        private readonly Logger _logger;

        private readonly ICached<RemoteEpisode> _remoteEpisodeCache;

        public ReleaseModule(IFetchAndParseRss rssFetcherAndParser,
                             ISearchForNzb nzbSearchService,
                             IMakeDownloadDecision downloadDecisionMaker,
                             IPrioritizeDownloadDecision prioritizeDownloadDecision,
                             IDownloadService downloadService,
                             ICacheManager cacheManager,
                             Logger logger)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _nzbSearchService = nzbSearchService;
            _downloadDecisionMaker = downloadDecisionMaker;
            _prioritizeDownloadDecision = prioritizeDownloadDecision;
            _downloadService = downloadService;
            _logger = logger;

            GetResourceAll = GetReleases;
            Post["/"] = x => DownloadRelease(this.Bind<ReleaseResource>());

            PostValidator.RuleFor(s => s.DownloadAllowed).Equal(true);
            PostValidator.RuleFor(s => s.Guid).NotEmpty();

            _remoteEpisodeCache = cacheManager.GetCache<RemoteEpisode>(GetType(), "remoteEpisodes");
        }

        private Response DownloadRelease(ReleaseResource release)
        {
            var remoteEpisode = _remoteEpisodeCache.Find(release.Guid);

            if (remoteEpisode == null)
            {
                _logger.Debug("Couldn't find requested release in cache, cache timeout probably expired.");

                return new NotFoundResponse();
            }

            try
            {
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
            if (Request.Query.episodeId != null)
            {
                return GetEpisodeReleases(Request.Query.episodeId);
            }

            return GetRss();
        }

        private List<ReleaseResource> GetEpisodeReleases(int episodeId)
        {
            try
            {
                var decisions = _nzbSearchService.EpisodeSearch(episodeId);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Episode search failed: " + ex.Message, ex);
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

        private List<ReleaseResource> MapDecisions(IEnumerable<DownloadDecision> decisions)
        {
            var result = new List<ReleaseResource>();

            foreach (var downloadDecision in decisions)
            {
                _remoteEpisodeCache.Set(downloadDecision.RemoteEpisode.Release.Guid, downloadDecision.RemoteEpisode, TimeSpan.FromMinutes(30));

                var release = new ReleaseResource();

                release.InjectFrom(downloadDecision.RemoteEpisode.Release);
                release.InjectFrom(downloadDecision.RemoteEpisode.ParsedEpisodeInfo);
                release.InjectFrom(downloadDecision);
                release.Rejections = downloadDecision.Rejections.Select(r => r.Reason).ToList();
                release.DownloadAllowed = downloadDecision.RemoteEpisode.DownloadAllowed;

                release.ReleaseWeight = result.Count;

                if (downloadDecision.RemoteEpisode.Series != null)
                {
                    release.QualityWeight = downloadDecision.RemoteEpisode
                                                            .Series
                                                            .Profile
                                                            .Value
                                                            .Items
                                                            .FindIndex(v => v.Quality == release.Quality.Quality) * 100;
                }

                release.QualityWeight += release.Quality.Revision.Real * 10;
                release.QualityWeight += release.Quality.Revision.Version;
                
                var torrentRelease = downloadDecision.RemoteEpisode.Release as TorrentInfo;
                var filehosterRelease = downloadDecision.RemoteEpisode.Release as FilehosterInfo;
                if (torrentRelease != null)
                {
                    release.Protocol = DownloadProtocol.Torrent;
                    release.Seeders = torrentRelease.Seeders;
                    //TODO: move this up the chains
                    release.Leechers = torrentRelease.Peers - torrentRelease.Seeders;
                }
                else if (filehosterRelease != null)
                {
                    release.Protocol = DownloadProtocol.Filehoster;
                }
                else
                {
                    release.Protocol = DownloadProtocol.Usenet;
                }

                result.Add(release);
            }

            return result;
        }
    }
}