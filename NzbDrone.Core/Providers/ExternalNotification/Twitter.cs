using System;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public class Twitter : ExternalNotificationBase
    {
        private readonly TwitterProvider _twitterProvider;

        public Twitter(ConfigProvider configProvider, TwitterProvider twitterProvider)
            : base(configProvider)
        {
            _twitterProvider = twitterProvider;
        }

        public override string Name
        {
            get { return "Twitter"; }
        }

        public override void OnGrab(string message)
        {
            if (_configProvider.TwitterNotifyOnGrab)
            {
                _logger.Trace("Sending Notification to Twitter (On Grab)");
                _twitterProvider.SendTweet("Download Started: " + message);
            }
        }

        public override void OnDownload(string message, Series series)
        {
            if (_configProvider.TwitterNotifyOnDownload)
            {
                _logger.Trace("Sending Notification to Twitter (On Grab)");
                _twitterProvider.SendTweet("Download Completed: " + message);
            }
        }

        public override void OnRename(string message, Series series)
        {

        }

        public override void AfterRename(string message, Series series)
        {

        }
    }
}
