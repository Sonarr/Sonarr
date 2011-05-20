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
    public class NzbMatrixProvider : IndexerProviderBase
    {
        public NzbMatrixProvider(HttpProvider httpProvider, ConfigProvider configProvider, IndexerProvider indexerProvider) : base(httpProvider, configProvider, indexerProvider)
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

    }
}