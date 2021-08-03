using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Authentication.Forms;
using Nancy.Routing.Trie.Nodes;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using Sonarr.Http.Extensions;

namespace Sonarr.Http.Authentication
{
    public interface IAuthenticationService : IUserValidator, IUserMapper
    {
        void SetContext(NancyContext context);

        void LogUnauthorized(NancyContext context);
        User Login(NancyContext context, string username, string password);
        void Logout(NancyContext context);
        bool IsAuthenticated(NancyContext context);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private const string AnonymousUser = "Anonymous";
        private static readonly Logger _authLogger = LogManager.GetLogger("Auth");
        private readonly IUserService _userService;

        private static string API_KEY;
        private static AuthenticationType AUTH_METHOD;

        [ThreadStatic]
        private static NancyContext _context;

        public AuthenticationService(IConfigFileProvider configFileProvider, IUserService userService)
        {
            _userService = userService;
            API_KEY = configFileProvider.ApiKey;
            AUTH_METHOD = configFileProvider.AuthenticationMethod;
        }

        public void SetContext(NancyContext context)
        {
            // Validate and GetUserIdentifier don't have access to the NancyContext so get it from the pipeline earlier
            _context = context;
        }

        public User Login(NancyContext context, string username, string password)
        {
            if (AUTH_METHOD == AuthenticationType.None)
            {
                return null;
            }

            var user = _userService.FindUser(username, password);

            if (user != null)
            {
                LogSuccess(context, username);

                return user;
            }

            LogFailure(context, username);

            return null;
        }

        public void Logout(NancyContext context)
        {
            if (AUTH_METHOD == AuthenticationType.None)
            {
                return;
            }

            if (context.CurrentUser != null)
            {
                LogLogout(context, context.CurrentUser.Identity.Name);
            }
        }

        public ClaimsPrincipal Validate(string username, string password)
        {
            if (AUTH_METHOD == AuthenticationType.None)
            {
                return new ClaimsPrincipal(new GenericIdentity(AnonymousUser));
            }

            var user = _userService.FindUser(username, password);

            if (user != null)
            {
                if (AUTH_METHOD != AuthenticationType.Basic)
                {
                    // Don't log success for basic auth
                    LogSuccess(_context, username);
                }

                return new ClaimsPrincipal(new GenericIdentity(user.Username));
            }

            LogFailure(_context, username);

            return null;
        }

        public ClaimsPrincipal GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            if (AUTH_METHOD == AuthenticationType.None)
            {
                return new ClaimsPrincipal(new GenericIdentity(AnonymousUser));
            }

            var user = _userService.FindUser(identifier);

            if (user != null)
            {
                return new ClaimsPrincipal(new GenericIdentity(user.Username));
            }

            LogInvalidated(_context);

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

            if (context.Request.IsBundledJsRequest())
            {
                return true;
            }
            
            if (context.Request.IsFavIconRequest())
            {
                return true;
            }
            
            if (context.Request.IsPingRequest())
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
            if (context.CurrentUser != null)
            {
                return true;
            }

            return false;
        }

        private bool ValidApiKey(string apiKey)
        {
            if (API_KEY.Equals(apiKey))
            {
                return true;
            }

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

        public void LogUnauthorized(NancyContext context)
        {
            _authLogger.Info("Auth-Unauthorized ip {0} url '{1}'", context.GetRemoteIP(), context.Request.Url.ToString());
        }

        private void LogInvalidated(NancyContext context)
        {
            _authLogger.Info("Auth-Invalidated ip {0}", context.GetRemoteIP());
        }

        private void LogFailure(NancyContext context, string username)
        {
            _authLogger.Warn("Auth-Failure ip {0} username '{1}'", context.GetRemoteIP(), username);
        }

        private void LogSuccess(NancyContext context, string username)
        {
            _authLogger.Info("Auth-Success ip {0} username '{1}'", context.GetRemoteIP(), username);
        }

        private void LogLogout(NancyContext context, string username)
        {
            _authLogger.Info("Auth-Logout ip {0} username '{1}'", context.GetRemoteIP(), username);
        }
    }
}
