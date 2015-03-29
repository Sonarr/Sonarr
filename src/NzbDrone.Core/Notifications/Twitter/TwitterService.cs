using FluentValidation.Results;
//using Tweetinvi;
//using TinyTwitter;
using NzbDrone.Common.Extensions;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NzbDrone.Core.Notifications.Twitter
{
    public interface ITwitterService
    {
        void SendNotification(string message, String accessToken, String accessTokenSecret, String consumerKey, String consumerSecret);
        ValidationFailure Test(TwitterSettings settings);
    }

    public class TwitterService : ITwitterService
    {
        private readonly Logger _logger;

        public TwitterService(Logger logger)
        {
            _logger = logger;

            var logo = typeof(TwitterService).Assembly.GetManifestResourceBytes("NzbDrone.Core.Resources.Logo.64.png");
        }

        public void SendNotification(string message, String accessToken, String accessTokenSecret, String consumerKey, String consumerSecret)
        {
            //_logger.Debug("Sending Notification to: {0}:{1}", hostname, port);

            var oauth = new TinyTwitter.OAuthInfo
            {
                AccessToken = accessToken,
                AccessSecret = accessTokenSecret,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret
            };

            var twitter = new TinyTwitter.TinyTwitter(oauth);

            // Update status, i.e, post a new tweet
            twitter.UpdateStatus(message);

            /*var credentials = TwitterCredentials.CreateCredentials(accessToken, accessTokenSecret, consumerKey, consumerSecret);
            TwitterCredentials.ExecuteOperationWithCredentials(credentials, () =>
            {
                    Tweet.PublishTweet(message);
            });*/
        }

        public ValidationFailure Test(TwitterSettings settings)
        {
            try
            {
                string body = "This is a test message from Sonarr @ " + DateTime.Now.ToString();
                SendNotification(body, settings.AccessToken, settings.AccessTokenSecret, settings.ConsumerKey, settings.ConsumerSecret);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to send test message: " + ex.Message, ex);
                return new ValidationFailure("Host", "Unable to send test message");
            }
            return null;
        }
    }
}
