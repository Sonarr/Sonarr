using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Twitter
{
    public class Twitter : NotificationBase<TwitterSettings>
    {
        private readonly ITwitterService _twitterService;

        public Twitter(ITwitterService twitterService)
        {
            _twitterService = twitterService;
        }

        public override string Name => "Twitter";
        public override string Link => "https://twitter.com/";

        public override void OnGrab(GrabMessage message)
        {
            _twitterService.SendNotification($"Grabbed: {message.Message}", Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _twitterService.SendNotification($"Imported: {message.Message}", Settings);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            _twitterService.SendNotification($"Episode Deleted: {deleteMessage.Message}", Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            _twitterService.SendNotification($"Series Deleted: {deleteMessage.Message}", Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            _twitterService.SendNotification($"Health Issue: {healthCheck.Message}", Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _twitterService.SendNotification($"Application Updated: {updateMessage.Message}", Settings);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                if (query["callbackUrl"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam callbackUrl invalid.");
                }

                var oauthRedirectUrl = _twitterService.GetOAuthRedirect(Settings.ConsumerKey, Settings.ConsumerSecret, query["callbackUrl"]);
                return new
                {
                    oauthUrl = oauthRedirectUrl
                };
            }
            else if (action == "getOAuthToken")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                if (query["oauth_token"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam oauth_token invalid.");
                }

                if (query["oauth_verifier"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam oauth_verifier invalid.");
                }

                var oauthToken = _twitterService.GetOAuthToken(Settings.ConsumerKey, Settings.ConsumerSecret, query["oauth_token"], query["oauth_verifier"]);
                return new
                {
                    accessToken = oauthToken.AccessToken,
                    accessTokenSecret = oauthToken.AccessTokenSecret
                };
            }

            return new { };
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_twitterService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
