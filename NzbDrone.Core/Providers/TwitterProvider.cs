using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Twitter;
using Twitterizer;

namespace NzbDrone.Core.Providers
{
    public class TwitterProvider
    {
        private readonly IConfigService _configService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string ConsumerKey = "umKU6jBWpFbHTuqQbW2VlQ";
        private const string ConsumerSecret = "e30OXkI6qrZWS35hbUUnrQQ8J2R9XNpccQNWAVK10";

        public TwitterProvider(IConfigService configService)
        {
            _configService = configService;
        }

        public virtual TwitterAuthorizationModel GetAuthorization()
        {
            try
            {
                OAuthTokenResponse requestToken = OAuthUtility.GetRequestToken(ConsumerKey, ConsumerSecret, "oob", null);
                Uri authorizationUri = OAuthUtility.BuildAuthorizationUri(requestToken.Token);

                return new TwitterAuthorizationModel
                            { 
                                Token = requestToken.Token,
                                Url = authorizationUri.ToString() 
                            };
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to get Twitter authorization URL.");
                Logger.TraceException(ex.Message, ex);

                return null;
            }
            
        }

        public virtual bool GetAndSaveAccessToken(string authToken, string verifier)
        {
            try
            {
                Logger.Debug("Attempting to get the AccessToken from Twitter");

                OAuthTokenResponse accessToken = OAuthUtility.GetAccessToken(ConsumerKey, ConsumerSecret, authToken, verifier);

                _configService.TwitterAccessToken = accessToken.Token;
                _configService.TwitterAccessTokenSecret = accessToken.TokenSecret;

                //Send a tweet to test!
                SendTweet("I have just setup tweet notifications for NzbDrone!");

                return true;
            }

            catch (Exception ex)
            {
                Logger.TraceException(ex.Message, ex);
                return false;
            }
        }

        public virtual bool SendTweet(string message)
        {
            try
            {
                Logger.Trace("Sending status update to twitter: {0}", message);

                var accessToken = _configService.TwitterAccessToken;
                var accessTokenSecret = _configService.TwitterAccessTokenSecret;

                //If the access token or access token secret are not configured, log an error and return
                if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(accessTokenSecret))
                {
                    Logger.Warn("Twitter Setup is incomplete, please check your settings");
                    return false;
                }

                var token = new OAuthTokens
                                {
                                    AccessToken = accessToken,
                                    AccessTokenSecret = accessTokenSecret,
                                    ConsumerKey = ConsumerKey,
                                    ConsumerSecret = ConsumerSecret
                                };

                TwitterStatus.Update(token, message + " #NzbDrone");

                return true;
            }

            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
                return false;
            }   
        }
    }
}
