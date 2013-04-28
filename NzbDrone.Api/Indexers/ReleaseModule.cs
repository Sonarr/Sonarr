using System;
using System.Collections.Generic;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.REST;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser;
using Omu.ValueInjecter;
using System.Linq;

namespace NzbDrone.Api.Indexers
{
    public class ReleaseModule : NzbDroneRestModule<ReleaseResource>
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;

        public ReleaseModule(IFetchAndParseRss rssFetcherAndParser, IMakeDownloadDecision downloadDecisionMaker)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _downloadDecisionMaker = downloadDecisionMaker;
            GetResourceAll = GetRss;
        }

        private List<ReleaseResource> GetRss()
        {
            var reports = _rssFetcherAndParser.Fetch();
            var decisions = _downloadDecisionMaker.GetRssDecision(reports);

            var result = new List<ReleaseResource>();

            foreach (var downloadDecision in decisions)
            {
                var release = new ReleaseResource();

                release.InjectFrom(downloadDecision.RemoteEpisode.Report);
                release.InjectFrom(downloadDecision.RemoteEpisode.ParsedEpisodeInfo);
                release.InjectFrom(downloadDecision);
                release.Rejections = downloadDecision.Rejections.ToList();

                result.Add(release);
            }

            return result;
        }
    }

    public class ReleaseResource : RestResource
    {
        public Int32 Age { get; set; }
        public Int64 Size { get; set; }
        public String Indexer { get; set; }
        public String NzbInfoUrl { get; set; }
        public String NzbUrl { get; set; }
        public String ReleaseGroup { get; set; }
        public String Title { get; set; }
        public Boolean FullSeason { get; set; }
        public Boolean SceneSource { get; set; }
        public Int32 SeasonNumber { get; set; }
        public Language Language { get; set; }
        public DateTime? AirDate { get; set; }
        public String OriginalString { get; set; }
        public String SeriesTitle { get; set; }
        public int[] EpisodeNumbers { get; set; }
        public Boolean Approved { get; set; }
        public List<string> Rejections { get; set; }
    }
}