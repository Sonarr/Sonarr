using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NzbsOrg : IndexerBase
    {
        public NzbsOrg(HttpProvider httpProvider, ConfigProvider configProvider)
            : base(httpProvider, configProvider)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                                   {
                                       string.Format("http://nzbs.org/rss.php?type=1&i={0}&h={1}&num=50&dl=1", _configProvider.NzbsOrgUId, _configProvider.NzbsOrgHash)
                                   };
            }
        }

        public override string Name
        {
            get { return "Nzbs.org"; }
        }


        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Id;
        }

        protected override IList<string> GetSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                searchUrls.Add(String.Format("{0}&action=search&q={1}+s{2:00}e{3:00}", url, GetQueryTitle(seriesTitle), seasonNumber, episodeNumber));
            }

            return searchUrls;
        }

    }
}