using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Search;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Indexer
{
    public class Newzbin : IndexerBase
    {
        [Inject]
        public Newzbin(HttpProvider httpProvider, ConfigProvider configProvider)
            : base(httpProvider, configProvider)
        {
        }

        private const string UrlParams = "feed=rss&hauth=1&ps_rb_language=4096";

        protected override string[] Urls
        {
            get
            {
                return new[]
                                   {
                                       "http://www.newzbin.com/browse/category/p/tv?" + UrlParams
                                   };
            }
        }




        protected override NetworkCredential Credentials
        {
            get { return new NetworkCredential(_configProvider.NewzbinUsername, _configProvider.NewzbinPassword); }
        }

        protected override IList<string> GetSearchUrls(SearchModel searchModel)
        {
            if (searchModel.SearchType == SearchType.EpisodeSearch)
            {
                return new List<string>
                           {
                               String.Format(
                                   @"http://www.newzbin.com/search/query/?q={0}+{1}x{2:00}&fpn=p&searchaction=Go&category=8&{3}",
                                   searchModel.SeriesTitle, searchModel.SeasonNumber,
                                   searchModel.EpisodeNumber, UrlParams)
                           };
            }

            return new List<string>
                           {
                               String.Format(
                                   @"http://www.newzbin.com/search/query/?q={0}+Season+{1}&fpn=p&searchaction=Go&category=8&{2}",
                                   searchModel.SeriesTitle, searchModel.SeasonNumber, UrlParams)
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
            }
            return currentResult;
        }

    }
}