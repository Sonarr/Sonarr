using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Feed
{
    class NzbsOrgFeedProvider : FeedProviderBase
    {
        public NzbsOrgFeedProvider(ISeriesProvider seriesProvider, ISeasonProvider seasonProvider, IEpisodeProvider episodeProvider, IConfigProvider configProvider, IHttpProvider httpProvider) : base(seriesProvider, seasonProvider, episodeProvider, configProvider, httpProvider)
        {
        }

        protected override string[] URL
        {
            get
            {
                return new[] { string.Format("http://nzbs.org/rss.php?type=1&i={0}&h={1}", _configProvider.NzbsOrgUId, _configProvider.NzbsOrgHash) };
            }
        }

        protected override string Name
        {
            get { return "Nzbs.Org"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Id.Replace("action=view", "action=getnzb");
        }
    }
}
