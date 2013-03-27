using System.Linq;
using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Indexers
{
    public class NzbIndex : IndexerBase
    {
        public NzbIndex(HttpProvider httpProvider, IConfigService configService)
            : base(httpProvider, configService)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                           {
                               String.Format("http://www.nzbindex.nl/rss/alt.binaries.teevee/?sort=agedesc&minsize=100&complete=1&max=50&more=1&q=%23a.b.teevee"),
                               String.Format("http://www.nzbindex.nl/rss/alt.binaries.hdtv/?sort=agedesc&minsize=100&complete=1&max=50&more=1&q=")
                           };
            }
        }

        public override bool IsConfigured
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get { return "NzbIndex"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[1].Uri.ToString();
        }

        protected override string NzbInfoUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}+{1}+s{2:00}e{3:00}", url, seriesTitle, seasonNumber, episodeNumber));
            }

            return searchUrls;
        }

        protected override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}+{1}+s{2:00}", url, seriesTitle, seasonNumber));
            }

            return searchUrls;
        }

        protected override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}+{1}+{2:yyyy MM dd}", url, seriesTitle, date));
            }

            return searchUrls;
        }

        protected override IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}+{1}+S{2:00}E{3}", url, seriesTitle, seasonNumber, episodeWildcard));
            }

            return searchUrls;
        }

        protected override EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            if (currentResult != null)
            {
                var sizeString = Regex.Match(item.Summary.Text, @"<b>\d+\.\d{1,2}\s\w{2}</b><br\s/>", RegexOptions.IgnoreCase | RegexOptions.Compiled).Value;
                currentResult.Size = Parser.GetReportSize(sizeString);
            }

            return currentResult;
        }

        public override bool EnabledByDefault
        {
            get { return true; }
        }

        protected override string TitlePreParser(SyndicationItem item)
        {
            var title = Parser.ParseHeader(item.Title.Text);

            if (String.IsNullOrWhiteSpace(title))
                return item.Title.Text;

            return title;
        }
    }
}