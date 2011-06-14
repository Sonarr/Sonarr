using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NzbMatrix : IndexerBase
    {
          [Inject]
        public NzbMatrix(HttpProvider httpProvider, ConfigProvider configProvider) : base(httpProvider, configProvider)
        {
        }

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

        public override string Name
        {
            get { return "NzbMatrix"; }
        }


        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override IList<string> GetSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&term={1}+s{2:00}e{3:00}", url, GetQueryTitle(seriesTitle), seasonNumber, episodeNumber));
            }

            return searchUrls;
        }

    }
}