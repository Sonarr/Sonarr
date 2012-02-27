using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NzbMatrix : IndexerBase
    {
        [Inject]
        public NzbMatrix(HttpProvider httpProvider, ConfigProvider configProvider)
            : base(httpProvider, configProvider)
        {
        }

        private static readonly Regex TitleSearchRegex = new Regex(@"[\W]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override string[] Urls
        {
            get
            {
                return new[]
                           {
                               string.Format(
                                   "http://rss.nzbmatrix.com/rss.php?page=download&username={0}&apikey={1}&subcat=6,41&english=1&scenename=1&num=50",
                                   _configProvider.NzbMatrixUsername,
                                   _configProvider.NzbMatrixApiKey)
                           };
            }
        }

        public override bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_configProvider.NzbMatrixUsername) &&
                       !string.IsNullOrWhiteSpace(_configProvider.NzbMatrixApiKey);
            }
        }

        protected override IList<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&term={1}+s{2:00}e{3:00}", url, seriesTitle, seasonNumber, episodeNumber));
            }

            return searchUrls;
        }

        protected override IList<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&term={1}+{2:yyyy MM dd}", url, seriesTitle, date));
            }

            return searchUrls;
        }

        protected override IList<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&term={1}+Season", url, seriesTitle));
                searchUrls.Add(String.Format("{0}&term={1}+S{2:00}", url, seriesTitle, seasonNumber));
            }

            return searchUrls;
        }

        protected override IList<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&term={1}+S{2:00}E{3}", url, seriesTitle, seasonNumber, episodeWildcard));
            }

            return searchUrls;
        }

        public override string Name
        {
            get { return "NzbMatrix"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            if (currentResult != null)
            {
                var sizeString = Regex.Match(item.Summary.Text, @"<b>Size:</b> \d+\.\d{1,2} \w{2}<br />", RegexOptions.IgnoreCase).Value;
                currentResult.Size = Parser.GetReportSize(sizeString);
            }

            return currentResult;
        }

        public override string GetQueryTitle(string title)
        {
            //Replace apostrophe with empty string
            title = title.Replace("'", "");
            title = RemoveThe.Replace(title, string.Empty);
            var cleanTitle = TitleSearchRegex.Replace(title, "+").Trim('+', ' ');

            //remove any repeating +s
            cleanTitle = Regex.Replace(cleanTitle, @"\+{1,100}", "+");
            return cleanTitle;
        }
    }
}