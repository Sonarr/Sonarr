using System;
using System.IO;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Twitter
{
    public interface ITwitterService
    {
        void SendNotification(string message, TwitterSettings settings);
        ValidationFailure Test(TwitterSettings settings);
        string GetOAuthRedirect(string consumerKey, string consumerSecret, string callbackUrl);
        OAuthToken GetOAuthToken(string consumerKey, string consumerSecret, string oauthToken, string oauthVerifier);
    }

    public class TwitterService : ITwitterService
    {
        private readonly ITwitterProxy _twitterProxy;
        private readonly Logger _logger;

        public TwitterService(ITwitterProxy twitterProxy, Logger logger)
        {
            _twitterProxy = twitterProxy;
            _logger = logger;
        }

        public OAuthToken GetOAuthToken(string consumerKey, string consumerSecret, string oauthToken, string oauthVerifier)
        {
            var qscoll = _twitterProxy.GetOAuthToken(consumerKey, consumerSecret, oauthToken, oauthVerifier);

            return new OAuthToken
            {
                AccessToken = qscoll["oauth_token"],
                AccessTokenSecret = qscoll["oauth_token_secret"]
            };
        }

        public string GetOAuthRedirect(string consumerKey, string consumerSecret, string callbackUrl)
        {
            return _twitterProxy.GetOAuthRedirect(consumerKey, consumerSecret, callbackUrl);
        }

        public void SendNotification(string message, TwitterSettings settings)
        {
            try
            {
                if (settings.DirectMessage)
                {
                    _twitterProxy.DirectMessage(message, settings);
                }
                else
                {
                    if (settings.Mention.IsNotNullOrWhiteSpace())
                    {
                        message += string.Format(" @{0}", settings.Mention);
                    }

                    _twitterProxy.UpdateStatus(message, settings);
                }
            }
            catch (WebException ex)
            {
                using (var response = ex.Response)
                {
                    var httpResponse = (HttpWebResponse)response;

                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null)
                        {
                            _logger.Trace("Status Code: {0}", httpResponse.StatusCode);
                            throw new TwitterException("Error received from Twitter: " + httpResponse.StatusCode, ex);
                        }

                        using (var reader = new StreamReader(responseStream))
                        {
                            var responseBody = reader.ReadToEnd();
                            _logger.Trace("Reponse: {0} Status Code: {1}", responseBody, httpResponse.StatusCode);
                            throw new TwitterException("Error received from Twitter: " + responseBody, ex);
                        }
                    }
                }
            }
        }

        public ValidationFailure Test(TwitterSettings settings)
        {
            try
            {
                var body = "Sonarr: Test Message @ " + DateTime.Now;

                SendNotification(body, settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("Host", "Unable to send test message");
            }

            return null;
        }
    }
}
