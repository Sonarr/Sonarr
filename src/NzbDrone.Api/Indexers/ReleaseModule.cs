using System;
using System.Collections.Generic;
using FluentValidation;
using Nancy;
using NLog;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using Omu.ValueInjecter;
using System.Linq;
using Nancy.ModelBinding;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Indexers
{
    public class ReleaseModule : NzbDroneRestModule<ReleaseResource>
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IDownloadService _downloadService;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        public ReleaseModule(IFetchAndParseRss rssFetcherAndParser,
                             ISearchForNzb nzbSearchService,
                             IMakeDownloadDecision downloadDecisionMaker,
                             IPrioritizeDownloadDecision prioritizeDownloadDecision,
                             IDownloadService downloadService,
                             IParsingService parsingService,
                             Logger logger)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _nzbSearchService = nzbSearchService;
            _downloadDecisionMaker = downloadDecisionMaker;
            _prioritizeDownloadDecision = prioritizeDownloadDecision;
            _downloadService = downloadService;
            _parsingService = parsingService;
            _logger = logger;
            GetResourceAll = GetReleases;
            Post["/"] = x=> DownloadRelease(this.Bind<ReleaseResource>());

            PostValidator.RuleFor(s => s.DownloadAllowed).Equal(true);
        }

        private Response DownloadRelease(ReleaseResource release)
        {
            var remoteEpisode = _parsingService.Map(release.InjectTo<ParsedEpisodeInfo>(), release.TvRageId);
            remoteEpisode.Release = release.InjectTo<ReleaseInfo>();
            _downloadService.DownloadReport(remoteEpisode);

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

        private static List<ReleaseResource> MapDecisions(IEnumerable<DownloadDecision> decisions)
        {
            var result = new List<ReleaseResource>();

            foreach (var downloadDecision in decisions)
            {
                var release = new ReleaseResource();

                release.InjectFrom(downloadDecision.RemoteEpisode.Release);
                release.InjectFrom(downloadDecision.RemoteEpisode.ParsedEpisodeInfo);
                release.InjectFrom(downloadDecision);
                release.Rejections = downloadDecision.Rejections.ToList();
                release.DownloadAllowed = downloadDecision.RemoteEpisode.DownloadAllowed;

                release.ReleaseWeight = result.Count;

                if (downloadDecision.RemoteEpisode.Series != null)
                {
                    release.QualityWeight = downloadDecision.RemoteEpisode.Series.QualityProfile.Value.Items.FindIndex(v => v.Quality == release.Quality.Quality) * 2;
                }

                if (!release.Quality.Proper)
                {
                    release.QualityWeight++;
                }

                result.Add(release);
            }

            return result;
        }
    }
}