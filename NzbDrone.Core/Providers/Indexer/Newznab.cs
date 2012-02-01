using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Indexer
{
    public class Newznab : IndexerBase
    {
        private readonly NewznabProvider _newznabProvider;

        [Inject]
        public Newznab(HttpProvider httpProvider, ConfigProvider configProvider, NewznabProvider newznabProvider)
            : base(httpProvider, configProvider)
        {
            _newznabProvider = newznabProvider;
        }

        protected override string[] Urls
        {
            get { return GetUrls(); }
        }

        public override bool IsConfigured
        {
            get { return true; }
        }

        protected override IList<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            var searchUrls = new List<string>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&limit=100&q={1}&season{2}&ep{3}", url, seriesTitle, seasonNumber, episodeNumber));
            }

            return searchUrls;
        }

        protected override IList<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            var searchUrls = new List<string>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&limit=100&q={1}+{2:yyyy MM dd}", url, seriesTitle, date));
            }

            return searchUrls;
        }

        protected override IList<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            return new List<string>();
        }

        protected override IList<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            return new List<string>();
        }

        public override string Name
        {
            get { return "Newznab"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Id;
        }


        protected override EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            if (currentResult != null)
            {
                var sizeString = Regex.Match(item.Summary.Text, @">\d+\.\d{1,2} \w{2}</a>", RegexOptions.IgnoreCase).Value;

                currentResult.Size = Parser.GetReportSize(sizeString);
            }
            return currentResult;
        }

        private string[] GetUrls()
        {
            var urls = new List<string>();
            var newznzbIndexers = _newznabProvider.Enabled();

            foreach (var newznabDefinition in newznzbIndexers)
            {
                if (!String.IsNullOrWhiteSpace(newznabDefinition.ApiKey))
                    urls.Add(String.Format("{0}/api?t=tvsearch&cat=5030,5040&apikey={1}", newznabDefinition.Url,
                                        newznabDefinition.ApiKey));

                else
                    urls.Add(String.Format("{0}/api?t=tvsearch&cat=5030,5040", newznabDefinition.Url));
            }

            return urls.ToArray();
        }
    }
}