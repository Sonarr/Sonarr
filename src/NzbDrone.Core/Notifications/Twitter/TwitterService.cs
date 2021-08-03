using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.OAuth;

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
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public TwitterService(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private NameValueCollection OAuthQuery(OAuthRequest oAuthRequest)
        {
            var auth = oAuthRequest.GetAuthorizationHeader();
            var request = new Common.Http.HttpRequest(oAuthRequest.RequestUrl);
            request.Headers.Add("Authorization", auth);
            var response = _httpClient.Get(request);

            return HttpUtility.ParseQueryString(response.Content);
        }

        public OAuthToken GetOAuthToken(string consumerKey, string consumerSecret, string oauthToken, string oauthVerifier)
        {
            // Creating a new instance with a helper method
            var oAuthRequest = OAuthRequest.ForAccessToken(consumerKey, consumerSecret, oauthToken, "", oauthVerifier);
            oAuthRequest.RequestUrl = "https://api.twitter.com/oauth/access_token";
            var qscoll = OAuthQuery(oAuthRequest);

            return new OAuthToken
            {
                AccessToken = qscoll["oauth_token"],
                AccessTokenSecret = qscoll["oauth_token_secret"]
            };
        }

        public string GetOAuthRedirect(string consumerKey, string consumerSecret, string callbackUrl)
        {
            // Creating a new instance with a helper method
            var oAuthRequest = OAuthRequest.ForRequestToken(consumerKey, consumerSecret, callbackUrl);
            oAuthRequest.RequestUrl = "https://api.twitter.com/oauth/request_token";
            var qscoll = OAuthQuery(oAuthRequest);

            return string.Format("https://api.twitter.com/oauth/authorize?oauth_token={0}", qscoll["oauth_token"]);
        }

        public void SendNotification(string message, TwitterSettings settings)
        {
            try
            {
                var oAuth = new TinyTwitter.OAuthInfo
                {
                    ConsumerKey = settings.ConsumerKey,
                    ConsumerSecret = settings.ConsumerSecret,
                    AccessToken = settings.AccessToken,
                    AccessSecret = settings.AccessTokenSecret
                };

                var twitter = new TinyTwitter.TinyTwitter(oAuth);

                if (settings.DirectMessage)
                {
                    twitter.DirectMessage(message, settings.Mention);
                }
                else
                {
                    if (settings.Mention.IsNotNullOrWhiteSpace())
                    {
                        message += string.Format(" @{0}", settings.Mention);
                    }

                    twitter.UpdateStatus(message);
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
