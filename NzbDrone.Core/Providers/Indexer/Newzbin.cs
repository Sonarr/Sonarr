using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;

namespace NzbDrone.Core.Providers.Indexer
{
    public class Newzbin : IndexerBase
    {
        public Newzbin(HttpProvider httpProvider, ConfigProvider configProvider) : base(httpProvider, configProvider)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                                   {
                                       "http://www.newzbin.com/browse/category/p/tv?feed=rss&hauth=1"
                                   };
            }
        }




        protected override NetworkCredential Credentials
        {
            get { return new NetworkCredential(_configProvider.NewzbinUsername, _configProvider.NewzbinPassword); }
        }

        protected override IList<string> GetSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            return new List<string>();
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
            }
            return currentResult;
        }

    }
}