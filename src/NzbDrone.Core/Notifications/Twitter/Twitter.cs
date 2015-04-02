using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using System;
using OAuth;
using System.Net;
using System.IO;

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

        public override object ConnectData(string stage, IDictionary<string, object> query)
        {
            if (stage == "step1")
            {
                return new 
                {
                    nextStep = "step2",
                    action = "openwindow",
                    url = _TwitterService.GetOAuthRedirect(
                        Settings.ConsumerKey, 
                        Settings.ConsumerSecret,
                        "http://localhost:8989/Content/oauthLand.html" /* FIXME - how do I get http host and such */
                    )
                };
            }
            else if (stage == "step2")
            {
                return new
                {
                    action = "updatefields",
                    fields = _TwitterService.GetOAuthToken(
                        Settings.ConsumerKey, Settings.ConsumerSecret,
                        query["oauth_token"].ToString(),
                        query["oauth_verifier"].ToString()
                    )
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
