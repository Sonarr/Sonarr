using System.Collections.Generic;
using System.ServiceModel.Syndication;
using Ninject;
using NzbDrone.Core.Model.Search;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NzbsRUs : IndexerBase
    {
          [Inject]
        public NzbsRUs(HttpProvider httpProvider, ConfigProvider configProvider) : base(httpProvider, configProvider)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                           {
                               string.Format(
                                   "http://www.nzbsrus.com/rssfeed.php?cat=91,75&i={0}&h={1}",
                                   _configProvider.NzbsrusUId,
                                   _configProvider.NzbsrusHash)
                           };
            }
        }

        public override string Name
        {
            get { return "NzbsRUs"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override IList<string> GetSearchUrls(SearchModel searchModel)
        {
            return new List<string>();
        }
    }
}