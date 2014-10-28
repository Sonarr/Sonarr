using System;
using System.Linq;
using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Security;
using NzbDrone.Api.Extensions;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Authentication
{
    public interface IAuthenticationService : IUserValidator
    {
        bool IsAuthenticated(NancyContext context);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfigFileProvider _configFileProvider;
        private static readonly NzbDroneUser AnonymousUser = new NzbDroneUser { UserName = "Anonymous" };
        private static String API_KEY;

        public AuthenticationService(IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;
            API_KEY = configFileProvider.ApiKey;
        }

        public IUserIdentity Validate(string username, string password)
        {
            if (!Enabled)
            {
                return AnonymousUser;
            }

            if (_configFileProvider.Username.Equals(username) &&
                _configFileProvider.Password.Equals(password))
            {
                return new NzbDroneUser { UserName = username };
            }

            return null;
        }

        private bool Enabled
        {
            get
            {
                return _configFileProvider.AuthenticationEnabled;
            }
        }

        public bool IsAuthenticated(NancyContext context)
        {
            var apiKey = GetApiKey(context);

            if (context.Request.IsApiRequest())
            {
                return ValidApiKey(apiKey);
            }

            if (context.Request.IsFeedRequest())
            {
                if (!Enabled)
                {
                    return true;
                }

                if (ValidUser(context) || ValidApiKey(apiKey))
                {
                    return true;
                }

                return false;
            }

            if (!Enabled)
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
