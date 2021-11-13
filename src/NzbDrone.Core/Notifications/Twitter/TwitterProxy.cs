using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.OAuth;

namespace NzbDrone.Core.Notifications.Twitter
{
    public interface ITwitterProxy
    {
        NameValueCollection GetOAuthToken(string consumerKey, string consumerSecret, string oauthToken, string oauthVerifier);
        string GetOAuthRedirect(string consumerKey, string consumerSecret, string callbackUrl);
        void UpdateStatus(string message, TwitterSettings settings);
        void DirectMessage(string message, TwitterSettings settings);
    }

    public class TwitterProxy : ITwitterProxy
    {
        private readonly IHttpClient _httpClient;

        public TwitterProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string GetOAuthRedirect(string consumerKey, string consumerSecret, string callbackUrl)
        {
            // Creating a new instance with a helper method
            var oAuthRequest = OAuthRequest.ForRequestToken(consumerKey, consumerSecret, callbackUrl);
            oAuthRequest.RequestUrl = "https://api.twitter.com/oauth/request_token";
            var qscoll = HttpUtility.ParseQueryString(ExecuteRequest(GetRequest(oAuthRequest, new Dictionary<string, string>())).Content);

            return string.Format("https://api.twitter.com/oauth/authorize?oauth_token={0}", qscoll["oauth_token"]);
        }

        public NameValueCollection GetOAuthToken(string consumerKey, string consumerSecret, string oauthToken, string oauthVerifier)
        {
            // Creating a new instance with a helper method
            var oAuthRequest = OAuthRequest.ForAccessToken(consumerKey, consumerSecret, oauthToken, "", oauthVerifier);
            oAuthRequest.RequestUrl = "https://api.twitter.com/oauth/access_token";

            return HttpUtility.ParseQueryString(ExecuteRequest(GetRequest(oAuthRequest, new Dictionary<string, string>())).Content);
        }

        public void UpdateStatus(string message, TwitterSettings settings)
        {
            var oAuthRequest = OAuthRequest.ForProtectedResource("POST", settings.ConsumerKey, settings.ConsumerSecret, settings.AccessToken, settings.AccessTokenSecret);

            oAuthRequest.RequestUrl = "https://api.twitter.com/1.1/statuses/update.json";

            var customParams = new Dictionary<string, string>
            {
                { "status", message.EncodeRFC3986() }
            };

            var request = GetRequest(oAuthRequest, customParams);

            request.Headers.ContentType = "application/x-www-form-urlencoded";
            request.SetContent(Encoding.ASCII.GetBytes(GetCustomParametersString(customParams)));

            ExecuteRequest(request);
        }

        public void DirectMessage(string message, TwitterSettings settings)
        {
            var oAuthRequest = OAuthRequest.ForProtectedResource("POST", settings.ConsumerKey, settings.ConsumerSecret, settings.AccessToken, settings.AccessTokenSecret);

            oAuthRequest.RequestUrl = "https://api.twitter.com/1.1/direct_messages/new.json";

            var customParams = new Dictionary<string, string>
            {
                { "text", message.EncodeRFC3986() },
                { "screenname", settings.Mention.EncodeRFC3986() }
            };

            var request = GetRequest(oAuthRequest, customParams);

            request.Headers.ContentType = "application/x-www-form-urlencoded";
            request.SetContent(Encoding.ASCII.GetBytes(GetCustomParametersString(customParams)));

            ExecuteRequest(request);
        }

        private string GetCustomParametersString(Dictionary<string, string> customParams)
        {
            return customParams.Select(x => string.Format("{0}={1}", x.Key, x.Value)).Join("&");
        }

        private HttpRequest GetRequest(OAuthRequest oAuthRequest, Dictionary<string, string> customParams)
        {
            var auth = oAuthRequest.GetAuthorizationHeader(customParams);
            var request = new HttpRequest(oAuthRequest.RequestUrl);

            request.Headers.Add("Authorization", auth);

            request.Method = oAuthRequest.Method == "POST" ? HttpMethod.Post : HttpMethod.Get;

            return request;
        }

        private HttpResponse ExecuteRequest(HttpRequest request)
        {
            return _httpClient.Execute(request);
        }
    }
}
