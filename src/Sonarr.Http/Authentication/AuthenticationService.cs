using System;
using System.Linq;
using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Authentication.Forms;
using Nancy.Security;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using Sonarr.Http.Extensions;

namespace Sonarr.Http.Authentication
{
    public interface IAuthenticationService : IUserValidator, IUserMapper
    {
        bool IsAuthenticated(NancyContext context);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private static readonly NzbDroneUser AnonymousUser = new NzbDroneUser { UserName = "Anonymous" };
        
        private static string API_KEY;
        private static AuthenticationType AUTH_METHOD;

        public AuthenticationService(IConfigFileProvider configFileProvider, IUserService userService)
        {
            _userService = userService;
            API_KEY = configFileProvider.ApiKey;
            AUTH_METHOD = configFileProvider.AuthenticationMethod;
        }

        public IUserIdentity Validate(string username, string password)
        {
            if (AUTH_METHOD == AuthenticationType.None)
            {
                return AnonymousUser;
            }

            var user = _userService.FindUser(username, password);

            if (user != null)
            {
                return new NzbDroneUser { UserName = user.Username };
            }

            return null;
        }

        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            if (AUTH_METHOD == AuthenticationType.None)
            {
                return AnonymousUser;
            }

            var user = _userService.FindUser(identifier);

            if (user != null)
            {
                return new NzbDroneUser { UserName = user.Username };
            }

            return null;
        }

        public bool IsAuthenticated(NancyContext context)
        {
            var apiKey = GetApiKey(context);

            if (context.Request.IsApiRequest())
            {
                return ValidApiKey(apiKey);
            }

            if (AUTH_METHOD == AuthenticationType.None)
            {
                return true;
            }

            if (context.Request.IsFeedRequest())
            {
                if (ValidUser(context) || ValidApiKey(apiKey))
                {
                    return true;
                }

                return false;
            }

            if (context.Request.IsLoginRequest())
            {
                return true;
            }

            if (context.Request.IsContentRequest())
            {
                return true;
            }

            if (ValidUser(context))
            {
                return true;
            }

            return false;
        }

        private bool ValidUser(NancyContext context)
        {
            if (context.CurrentUser != null) return true;

            return false;
        }

        private bool ValidApiKey(string apiKey)
        {
            if (API_KEY.Equals(apiKey)) return true;

            return false;
        }

        private string GetApiKey(NancyContext context)
        {
            var apiKeyHeader = context.Request.Headers["X-Api-Key"].FirstOrDefault();
            var apiKeyQueryString = context.Request.Query["ApiKey"];

            if (!apiKeyHeader.IsNullOrWhiteSpace())
            {
                return apiKeyHeader;
            }

            if (apiKeyQueryString.HasValue)
            {
                return apiKeyQueryString.Value;
            }

            return context.Request.Headers.Authorization;
        }
    }
}
