using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Model;
using Rss;

namespace NzbDrone.Core.Providers
{
    public class RssProvider : IRssProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region IRssProvider Members
        public IEnumerable<RssItem> GetFeed(FeedInfoModel feedInfo)
        {
            RssFeed feed = null;
            try
            {
                Logger.Info("INFO: Downloading feed {0} from {1}", feedInfo.Name, feedInfo.Url);
                feed = RssFeed.Read(feedInfo.Url);
            }
            catch (Exception e)
            {
                Logger.ErrorException(String.Format("ERROR: Could not download feed {0} from {1}", feedInfo.Name, feedInfo.Url), e);
            }
            if (feed == null || feed.Channels == null || feed.Channels.Count == 0)
                return Enumerable.Empty<RssItem>();
            return feed.Channels[0].Items.Cast<RssItem>();
        }
        #endregion
    }
}
