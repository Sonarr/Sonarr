using System;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Extensions;
using Nancy.ModelBinding;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Twitter;

namespace NzbDrone.Api.Notifications.Twitter
{
    public class TwitterModule : NancyModule
    {
        private readonly ITwitterService _twitterService;
        private readonly IConfigFileProvider _configFileProvider;

        public TwitterModule(ITwitterService twitterService, IConfigFileProvider configFileProvider)
        {
            _twitterService = twitterService;
            _configFileProvider = configFileProvider;
            //Post["/login"] = x => Step1();
            Get["/twitter/step1"] = x => Step1();
        }

        private Response Step1()
        {
            // ?ApiKey=blah
            //_twitterService.
            //return notifiers.twitter_notifier._get_authorization()
            return Context.GetRedirect("~/login?returnUrl=" + (string)Request.Query.returnUrl).WithHeader("Cache-Control", "max-age=0,no-cache,no-store");
            //return this.LoginAndRedirect(user.Identifier, expiry);
        }

        private Response Logout()
        {
            return this.LogoutAndRedirect(_configFileProvider.UrlBase + "/");
        }
    }
}