using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Indexers
{
    class Omgwtfnzbs : IndexerBase
    {
        public Omgwtfnzbs(HttpProvider httpProvider, IConfigService configService)
            : base(httpProvider, configService)
        {
        }

        public override string Name
        {
            get { return "omgwtfnzbs"; }
        }

        protected override string[] Urls
        {
            get
            { 
                return new string[]
                {
                    String.Format("http://rss.omgwtfnzbs.org/rss-search.php?catid=19,20&user={0}&api={1}&eng=1",
                                    _configService.OmgwtfnzbsUsername, _configService.OmgwtfnzbsApiKey)
                };
            }
        }

        public override bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_configService.OmgwtfnzbsUsername) &&
                       !string.IsNullOrWhiteSpace(_configService.OmgwtfnzbsApiKey);
            }
        }

        protected override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&search={1}+S{2:00}E{3:00}", url, seriesTitle, seasonNumber, episodeNumber));
            }

            return searchUrls;
        }

        protected override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&search={1}+{2:yyyy MM dd}", url, seriesTitle, date));
            }

            return searchUrls;
        }

        protected override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&search={1}+S{2:00}", url, seriesTitle, seasonNumber));
            }

            return searchUrls;
        }

        protected override IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&search={1}+S{2:00}E{3}", url, seriesTitle, seasonNumber, episodeWildcard));
            }

            return searchUrls;
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override string NzbInfoUrl(SyndicationItem item)
        {
            //Todo: Me thinks I need to parse details to get this...
            var match = Regex.Match(item.Summary.Text, @"(?:\<b\>View NZB\:\<\/b\>\s\<a\shref\=\"")(?<URL>.+)(?:\""\starget)",
                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if(match.Success)
            {
                return match.Groups["URL"].Value;
            }

            return String.Empty;
        }

        protected override EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            if (currentResult != null)
            {
                var sizeString = Regex.Match(item.Summary.Text, @"Size:\<\/b\>\s\d+\.\d{1,2}\s\w{2}\<br \/\>", RegexOptions.IgnoreCase | RegexOptions.Compiled).Value;
                currentResult.Size = Parser.GetReportSize(sizeString);
            }

            return currentResult;
        }
    }
}
