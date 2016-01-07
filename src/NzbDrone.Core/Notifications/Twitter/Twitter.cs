using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Twitter
{
    class Twitter : NotificationBase<TwitterSettings>
    {

        private readonly ITwitterService _twitterService;

        public Twitter(ITwitterService twitterService)
        {
            _twitterService = twitterService;
        }

        public override string Link
        {
            get { return "https://twitter.com/"; }
        }

        public override void OnGrab(GrabMessage grabMessage)
        {
            _twitterService.SendNotification(grabMessage.Message, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _twitterService.SendNotification(message.Message, Settings);
        }

        public override void OnRename(Series series)
        {
        }

        public override object ConnectData(string stage, IDictionary<string, object> query)
        {
            if (stage == "step1")
            {
                return new 
                {
                    nextStep = "step2",
                    action = "openWindow",
                    url = _twitterService.GetOAuthRedirect(query["consumerKey"].ToString(), query["consumerSecret"].ToString(), query["callbackUrl"].ToString())
                };
            }
            else if (stage == "step2")
            {
                return new
                {
                    action = "updateFields",
                    fields = _twitterService.GetOAuthToken(query["consumerKey"].ToString(), query["consumerSecret"].ToString(), query["oauth_token"].ToString(), query["oauth_verifier"].ToString())
                };
            }
            return new {};
        }

        public override string Name
        {
            get
            {
                return "Twitter";
            }
        }

        public override bool SupportsOnRename
        {
            get
            {
                return false;
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_twitterService.Test(Settings));

            return new ValidationResult(failures);
        }
    }

}
