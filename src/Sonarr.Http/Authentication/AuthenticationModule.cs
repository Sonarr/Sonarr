using System;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Extensions;
using Nancy.ModelBinding;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Authentication
{
    public class AuthenticationModule : NancyModule
    {
        private readonly IAuthenticationService _authService;
        private readonly IConfigFileProvider _configFileProvider;

        public AuthenticationModule(IAuthenticationService authService, IConfigFileProvider configFileProvider)
        {
            _authService = authService;
            _configFileProvider = configFileProvider;
            Post("/login",  x => Login(this.Bind<LoginResource>()));
            Get("/logout",  x => Logout());
        }

        private Response Login(LoginResource resource)
        {
            var user = _authService.Login(Context, resource.Username, resource.Password);

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
            _authService.Logout(Context);

            return this.LogoutAndRedirect(_configFileProvider.UrlBase + "/");
        }
    }
}
