using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NzbsOrgProvider : IndexerProviderBase
    {
        public NzbsOrgProvider(HttpProvider httpProvider, ConfigProvider configProvider, IndexerProvider indexerProvider) : base(httpProvider, configProvider, indexerProvider)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                                   {
                                       string.Format("http://nzbs.org/rss.php?catid=1,14&i={0}&h={1}&num=50&dl=1", _configProvider.NzbsOrgUId, _configProvider.NzbsOrgHash)
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

 }
}