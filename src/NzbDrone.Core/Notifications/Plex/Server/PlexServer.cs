using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Notifications.Plex.PlexTv;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Plex.Server
{
    public class PlexServer : NotificationBase<PlexServerSettings>
    {
        private readonly IPlexServerService _plexServerService;
        private readonly IPlexTvService _plexTvService;

        public PlexServer(IPlexServerService plexServerService, IPlexTvService plexTvService)
        {
            _plexServerService = plexServerService;
            _plexTvService = plexTvService;
        }

        public override string Link => "https://www.plex.tv/";
        public override string Name => "Plex Media Server";

        public override void OnDownload(DownloadMessage message)
        {
            UpdateIfEnabled(message.Series);
        }

        public override void OnRename(Series series)
        {
            UpdateIfEnabled(series);
        }

        private void UpdateIfEnabled(Series series)
        {
            if (Settings.UpdateLibrary)
            {
                _plexServerService.UpdateLibrary(series, Settings);
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_plexServerService.Test(Settings));

            return new ValidationResult(failures);
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

                return _plexTvService.GetSignInUrl(query["callbackUrl"]);
            }
            else if (action == "getOAuthToken")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                if (query["pinId"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam pinId invalid.");
                }

                var authToken = _plexTvService.GetAuthToken(Convert.ToInt32(query["pinId"]));

                return new
                       {
                           authToken
                       };
            }

            return new { };
        }
    }
}
