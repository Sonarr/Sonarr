using System;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Extensions;
using Nancy.ModelBinding;
using NzbDrone.Core.Authentication;

namespace NzbDrone.Api.Authentication
{
    public class LoginModule : NancyModule
    {
        private readonly IUserService _userService;

        public LoginModule(IUserService userService)
        {
            _userService = userService;
            Post["/login"] = x => Login(this.Bind<LoginResource>());
        }

        private Response Login(LoginResource resource)
        {
            var user = _userService.FindUser(resource.Username, resource.Password);

            if (user == null)
            {
                return Context.GetRedirect("~/login?returnUrl=" + (string)Request.Query.returnUrl);
            }

            DateTime? expiry = null;

            if (resource.RememberMe)
            {
                expiry = DateTime.UtcNow.AddDays(7);
            }

            return this.LoginAndRedirect(user.Identifier, expiry);
        }
    }
}
