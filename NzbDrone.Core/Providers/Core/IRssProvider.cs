using System.Collections.Generic;
using NzbDrone.Core.Model;
using Rss;

namespace NzbDrone.Core.Providers.Core
{
    public interface IRssProvider
    {
        IEnumerable<RssItem> GetFeed(FeedInfoModel feedInfo);
    }
}
