using System;
using Microsoft.AspNetCore.Http;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using Sonarr.Http.Extensions;

namespace Sonarr.Http.Authentication
{
    public interface IAuthenticationService
    {
        void LogUnauthorized(HttpRequest context);
        User Login(HttpRequest request, string username, string password);
        DateTime? GetLockoutEndTime(HttpRequest request);
        void Logout(HttpContext context);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

        private static readonly Logger _authLogger = LogManager.GetLogger("Auth");
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IUserService _userService;
        private readonly ICached<FailedLoginAttempts> _failedAttempts;

        public AuthenticationService(IConfigFileProvider configFileProvider, IUserService userService, ICacheManager cacheManager)
        {
            _configFileProvider = configFileProvider;
            _userService = userService;
            _failedAttempts = cacheManager.GetCache<FailedLoginAttempts>(GetType(), "failedLoginAttempts");
        }

        public User Login(HttpRequest request, string username, string password)
        {
            if (_configFileProvider.EffectiveAuthenticationMethod() != AuthenticationType.Forms)
            {
                return null;
            }

            var remoteIP = request.GetRemoteIP();

            if (IsLockedOut(remoteIP))
            {
                LogLockout(remoteIP, username);

                return null;
            }

            var user = _userService.FindUser(username, password);

            if (user != null)
            {
                RecordSuccessfulAttempt(remoteIP, username);

                return user;
            }

            RecordFailedAttempt(remoteIP, username);

            return null;
        }

        public DateTime? GetLockoutEndTime(HttpRequest request)
        {
            var attempts = _failedAttempts.Find(request.GetRemoteIP());

            if (attempts == null || attempts.LockedUntilUtc <= DateTime.UtcNow)
            {
                return null;
            }

            return attempts.LockedUntilUtc;
        }

        public void Logout(HttpContext context)
        {
            if (_configFileProvider.AuthenticationMethod == AuthenticationType.None)
            {
                return;
            }

            if (context.User != null)
            {
                LogLogout(context.Request, context.User.FindFirst("user")?.Value ?? context.User.Identity?.Name);
            }
        }

        private bool IsLockedOut(string remoteIP)
        {
            _failedAttempts.ClearExpired();

            var attempts = _failedAttempts.Find(remoteIP);

            return attempts != null && attempts.LockedUntilUtc > DateTime.UtcNow;
        }

        private void RecordFailedAttempt(string remoteIP, string username)
        {
            var attempts = _failedAttempts.Find(remoteIP) ?? new FailedLoginAttempts();

            attempts.Count++;

            if (attempts.Count >= MaxFailedAttempts)
            {
                attempts.LockedUntilUtc = DateTime.UtcNow + LockoutDuration;
            }

            _failedAttempts.Set(remoteIP, attempts, LockoutDuration);

            LogFailure(remoteIP, username);
        }

        public void LogUnauthorized(HttpRequest context)
        {
            _authLogger.Info("Auth-Unauthorized ip {0} url '{1}'", context.GetRemoteIP(), context.Path);
        }

        private void LogInvalidated(HttpRequest context)
        {
            _authLogger.Info("Auth-Invalidated ip {0}", context.GetRemoteIP());
        }

        private void LogFailure(string remoteIP, string username)
        {
            _authLogger.Warn("Auth-Failure ip {0} username '{1}'", remoteIP, username);
        }

        private void LogLockout(string remoteIP, string username)
        {
            _authLogger.Warn("Auth-Lockout ip {0} username '{1}'", remoteIP, username);
        }

        private void RecordSuccessfulAttempt(string remoteIP, string username)
        {
            _failedAttempts.Remove(remoteIP);

            _authLogger.Debug("Auth-Success ip {0} username '{1}'", remoteIP, username);
        }

        private void LogLogout(HttpRequest context, string username)
        {
            _authLogger.Info("Auth-Logout ip {0} username '{1}'", context.GetRemoteIP(), username);
        }

        private class FailedLoginAttempts
        {
            public int Count { get; set; }
            public DateTime LockedUntilUtc { get; set; }
        }
    }
}
