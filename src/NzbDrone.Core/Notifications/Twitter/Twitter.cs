using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using System;
using TinyTwitter;

namespace NzbDrone.Core.Notifications.Twitter
{
    class Twitter : NotificationBase<TwitterSettings>
    {

        private readonly ITwitterService _TwitterService;

        public Twitter(ITwitterService TwitterService)
        {
            _TwitterService = TwitterService;
        }

        public override string Link
        {
            get { return "https://twitter.com/"; }
        }

        public override void OnGrab(string message)
        {
            _TwitterService.SendNotification(message, Settings.AccessToken, Settings.AccessTokenSecret, Settings.ConsumerKey, Settings.ConsumerSecret);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _TwitterService.SendNotification(message.Message, Settings.AccessToken, Settings.AccessTokenSecret, Settings.ConsumerKey, Settings.ConsumerSecret);
        }

        public override void AfterRename(Series series)
        {
        }

        public override object ConnectData(string stage)
        {
            if (stage == "step1")
            {
                var oauth = new TinyTwitter.OAuthInfo
                {
                    AccessToken = "",
                    AccessSecret = "",
                    ConsumerKey = Settings.ConsumerKey,
                    ConsumerSecret = Settings.ConsumerSecret
                };
                var builder = new TinyTwitter.TinyTwitter.RequestBuilder(oauth, "GET", "https://api.twitter.com/oauth/request_token");
                using (var response = builder.Execute())
			    using (var stream = response.GetResponseStream())
			    using (var reader = new System.IO.StreamReader(stream))
			    {
				    var content = reader.ReadToEnd();
				    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

				    var tweets = (object[])serializer.DeserializeObject(content);
                    return tweets;
			    }

                return new
                {
                    redirectURL = "https://api.twitter.com/oauth/request_token"
                };
            }
            return new {};
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_TwitterService.Test(Settings));

            return new ValidationResult(failures);
        }
    }

}
