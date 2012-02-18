using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Indexer
{
    public class Newzbin : IndexerBase
    {

        private const string ROOT_DOMAIN = "https://www.newzbin2.es";

        [Inject]
        public Newzbin(HttpProvider httpProvider, ConfigProvider configProvider)
            : base(httpProvider, configProvider)
        {
        }

        private const string URL_PARAMS = "feed=rss&hauth=1&ps_rb_language=4096";

        protected override string[] Urls
        {
            get
            {
                return new[]
                                   {
                                       ROOT_DOMAIN + "/browse/category/p/tv?" + URL_PARAMS
                                   };
            }
        }

        public override bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_configProvider.NewzbinUsername) &&
                       !string.IsNullOrWhiteSpace(_configProvider.NewzbinPassword);
            }
        }


        protected override NetworkCredential Credentials
        {
            get { return new NetworkCredential(_configProvider.NewzbinUsername, _configProvider.NewzbinPassword); }
        }

        protected override IList<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            return new List<string>
                           {
                               String.Format(
                                   ROOT_DOMAIN + @"/search/query/?q={0}+{1}x{2:00}&fpn=p&searchaction=Go&category=8&{3}",
                                   seriesTitle, seasonNumber,episodeNumber, URL_PARAMS)
                           };
        }

        protected override IList<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            return new List<string>
                           {
                               String.Format(
                                   ROOT_DOMAIN + @"/search/query/?q={0}+Season+{1}&fpn=p&searchaction=Go&category=8&{2}",
                                   seriesTitle, seasonNumber, URL_PARAMS)
                           };
        }

        protected override IList<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            return new List<string>
                           {
                               String.Format(
                                   ROOT_DOMAIN + @"/search/query/?q={0}+{1:yyyy-MM-dd}&fpn=p&searchaction=Go&category=8&{2}",
                                   seriesTitle, date, URL_PARAMS)
                           };
        }

        protected override IList<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            return new List<string>
                           {
                               String.Format(
                                   ROOT_DOMAIN + @"/search/query/?q={0}+{1}x{2}&fpn=p&searchaction=Go&category=8&{3}",
                                   seriesTitle, seasonNumber, episodeWildcard, URL_PARAMS)
                           };
        }


        public override string Name
        {
            get { return "Newzbin"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Id + "nzb";
        }

        protected override EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            if (currentResult != null)
            {
                var quality = Parser.ParseQuality(item.Summary.Text);
                currentResult.Quality = quality;

                var languageString = Regex.Match(item.Summary.Text, @"Language - \w*", RegexOptions.IgnoreCase).Value;
                currentResult.Language = Parser.ParseLanguage(languageString);

                var sizeString = Regex.Match(item.Summary.Text, @"\(Size: \d*\,?\d+\.\d{1,2}\w{2}\)", RegexOptions.IgnoreCase).Value;
                currentResult.Size = Parser.GetReportSize(sizeString);
            }

            return currentResult;
        }

    }
}