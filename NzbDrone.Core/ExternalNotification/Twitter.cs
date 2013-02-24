using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.ExternalNotification
{
    public class Twitter : ExternalNotificationBase
    {
        private readonly TwitterProvider _twitterProvider;

        public Twitter(IExternalNotificationRepository repository, TwitterProvider twitterProvider, Logger logger)
            : base(repository, logger)
        {
            _twitterProvider = twitterProvider;
        }

        public override string Name
        {
            get { return "Twitter"; }
        }

        protected override void OnGrab(string message)
        {
            _twitterProvider.SendTweet("Download Started: " + message);
        }

        protected override void OnDownload(string message, Series series)
        {
            _twitterProvider.SendTweet("Download Completed: " + message);
        }

    }
}
