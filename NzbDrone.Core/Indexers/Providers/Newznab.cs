using System.Linq;
using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Indexers.Providers
{
    public class Newznab : IndexerBase
    {
        private readonly NewznabProvider _newznabProvider;

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
                searchUrls.Add(String.Format("{0}&limit=100&q={1}&season={2}&ep={3}", url, seriesTitle, seasonNumber, episodeNumber));
            }

            return searchUrls;
        }

        protected override IList<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            var searchUrls = new List<string>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&limit=100&q={1}&season={2:yyyy}&ep={2:MM/dd}", url, seriesTitle, date));
            }

            return searchUrls;
        }

        protected override IList<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            var searchUrls = new List<string>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&limit=100&q={1}&season={2}", url, seriesTitle, seasonNumber));
            }

            return searchUrls;
        }

        protected override IList<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            var searchUrls = new List<string>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&limit=100&q={1}+S{2:00}E{3}", url, seriesTitle, seasonNumber, episodeWildcard));
            }

            return searchUrls;
        }

        public override string Name
        {
            get { return "Newznab"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override string NzbInfoUrl(SyndicationItem item)
        {
            return item.Id;
        }

        protected override EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            if (currentResult != null)
            {
                if (item.Links.Count > 1)
                    currentResult.Size = item.Links[1].Length;

                currentResult.Indexer = GetName(item);
            }

            return currentResult;
        }

        private string[] GetUrls()
        {
            var urls = new List<string>();
            var newznabIndexers = _newznabProvider.Enabled();

            foreach (var newznabDefinition in newznabIndexers)
            {
                if (!String.IsNullOrWhiteSpace(newznabDefinition.ApiKey))
                    urls.Add(String.Format("{0}/api?t=tvsearch&cat=5030,5040,5070,5090&apikey={1}", newznabDefinition.Url,
                                        newznabDefinition.ApiKey));

                else
                    urls.Add(String.Format("{0}/api?t=tvsearch&cat=5030,5040,5070,5090s", newznabDefinition.Url));
            }

            return urls.ToArray();
        }

        private string GetName(SyndicationItem item)
        {
            var hostname = item.Links[0].Uri.DnsSafeHost.ToLower();
            return String.Format("{0}_{1}", Name, hostname);
        }

        public override string GetQueryTitle(string title)
        {
            title = RemoveThe.Replace(title, string.Empty);
            
            //remove any repeating whitespace
            var cleanTitle = TitleSearchRegex.Replace(title, "%20");

            cleanTitle = Regex.Replace(cleanTitle, @"(%20){1,100}", "%20");

            //Trim %20 from start then then the end
            cleanTitle = Regex.Replace(cleanTitle, "^(%20)", "");
            cleanTitle = Regex.Replace(cleanTitle, "(%20)$", "");

            return cleanTitle;
        }
    }
}