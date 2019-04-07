using System;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Extensions;
using Nancy.ModelBinding;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Authentication
{
    public class AuthenticationModule : NancyModule
    {
        private readonly IUserService _userService;
        private readonly IConfigFileProvider _configFileProvider;

        public AuthenticationModule(IUserService userService, IConfigFileProvider configFileProvider)
        {
            _userService = userService;
            _configFileProvider = configFileProvider;
            Post["/login"] = x => Login(this.Bind<LoginResource>());
            Get["/logout"] = x => Logout();
        }

        private Response Login(LoginResource resource)
        {
            var user = _userService.FindUser(resource.Username, resource.Password);

            if (user == null)
            {
                var returnUrl = (string)Request.Query.returnUrl;
                return Context.GetRedirect($"~/login?returnUrl={returnUrl}&loginFailed=true");
            }

            DateTime? expiry = null;

            if (resource.RememberMe)
            {
                expiry = DateTime.UtcNow.AddDays(7);
            }

            return this.LoginAndRedirect(user.Identifier, expiry, _configFileProvider.UrlBase + "/");
        }

        private Response Logout()
        {
            return this.LogoutAndRedirect(_configFileProvider.UrlBase + "/");
        }
    }
}
