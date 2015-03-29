using FluentValidation.Results;
//using Tweetinvi;
//using TinyTwitter;
using NzbDrone.Common.Extensions;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OAuth;
using System.Net;
using System.Collections.Specialized;

namespace NzbDrone.Core.Notifications.Twitter
{
    public interface ITwitterService
    {
        void SendNotification(string message, String accessToken, String accessTokenSecret, String consumerKey, String consumerSecret);
        ValidationFailure Test(TwitterSettings settings);
        string GetOAuthRedirect(string consumerKey, string consumerSecret, string callback);
        object GetOAuthToken(string consumerKey, string consumerSecret, string oauthToken, string oauthVerifier);
    }

    public class TwitterService : ITwitterService
    {
        private readonly Logger _logger;

        public TwitterService(Logger logger)
        {
            _logger = logger;

            var logo = typeof(TwitterService).Assembly.GetManifestResourceBytes("NzbDrone.Core.Resources.Logo.64.png");
        }

        public object GetOAuthToken(string consumerKey, string consumerSecret, string oauthToken, string oauthVerifier)
        {
            // Creating a new instance with a helper method
            OAuthRequest client = OAuthRequest.ForAccessToken(
                consumerKey, 
                consumerSecret,
                oauthToken,
                "",
                oauthVerifier
            );
            client.RequestUrl = "https://api.twitter.com/oauth/access_token";
            // Using HTTP header authorization
            string auth = client.GetAuthorizationHeader();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(client.RequestUrl);

            request.Headers.Add("Authorization", auth);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            System.Collections.Specialized.NameValueCollection qscoll;
            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8")))
            {
                string responseText = reader.ReadToEnd();
                qscoll = System.Web.HttpUtility.ParseQueryString(responseText);
            }

            return new
            {
                AccessToken = qscoll["oauth_token"],
                AccessTokenSecret = qscoll["oauth_token_secret"]
            };
        }

        public string GetOAuthRedirect(string consumerKey, string consumerSecret, string callback)
        {
            // Creating a new instance with a helper method
            OAuthRequest client = OAuthRequest.ForRequestToken(consumerKey, consumerSecret, callback);
            client.RequestUrl = "https://api.twitter.com/oauth/request_token";
            // Using HTTP header authorization
            string auth = client.GetAuthorizationHeader();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(client.RequestUrl);

            request.Headers.Add("Authorization", auth);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            System.Collections.Specialized.NameValueCollection qscoll;
            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8")))
            {
                string responseText = reader.ReadToEnd();
                qscoll = System.Web.HttpUtility.ParseQueryString(responseText);
            }
            return "https://api.twitter.com/oauth/authorize?oauth_token=" + qscoll["oauth_token"];
        }

        public void SendNotification(string message, String accessToken, String accessTokenSecret, String consumerKey, String consumerSecret)
        {
            try
            {
                var oauth = new TinyTwitter.OAuthInfo
                {
                    AccessToken = accessToken,
                    AccessSecret = accessTokenSecret,
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret
                };
                var twitter = new TinyTwitter.TinyTwitter(oauth);
                twitter.UpdateStatus(message);
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (System.IO.Stream data = response.GetResponseStream())
                    using (var reader = new System.IO.StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
                throw e;
            }
            return;
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
