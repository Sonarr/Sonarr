using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Model;
using Rss;

namespace NzbDrone.Core.Providers
{
    public interface IRssProvider
    {
        IEnumerable<RssItem> GetFeed(FeedInfoModel feedInfo);
    }
}
