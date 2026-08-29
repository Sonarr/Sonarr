using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Options;
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
        string RequestPasswordReset(HttpRequest request);
        bool ResetPassword(HttpRequest request, string token, string username, string password);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private const int MaxFailedAttempts = 5;
        private const int ResetTokenLength = 20;
        private const int MinimumConfiguredResetTokenLength = 20;
        private const string ResetTokenKey = "passwordResetToken";

        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan ResetRequestThrottle = TimeSpan.FromMinutes(1);

        private static readonly Logger _authLogger = LogManager.GetLogger("Auth");
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IUserService _userService;
        private readonly ICached<FailedLoginAttempts> _failedAttempts;
        private readonly ICached<string> _resetToken;
        private readonly ICached<string> _resetRequests;
        private string _configuredResetTokenHash;

        public AuthenticationService(IConfigFileProvider configFileProvider, IUserService userService, ICacheManager cacheManager, IOptions<AuthOptions> authOptions)
        {
            _configFileProvider = configFileProvider;
            _userService = userService;
            _failedAttempts = cacheManager.GetCache<FailedLoginAttempts>(GetType(), "failedLoginAttempts");
            _resetToken = cacheManager.GetCache<string>(GetType(), "passwordResetToken");
            _resetRequests = cacheManager.GetCache<string>(GetType(), "passwordResetRequests");
            _configuredResetTokenHash = GetConfiguredResetTokenHash(authOptions.Value.ResetToken);
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

        public string RequestPasswordReset(HttpRequest request)
        {
            if (_configFileProvider.EffectiveAuthenticationMethod() != AuthenticationType.Forms)
            {
                return null;
            }

            _resetRequests.ClearExpired();

            var remoteIP = request.GetRemoteIP();

            if (_resetRequests.Find(remoteIP) != null)
            {
                _authLogger.Debug("Auth-ResetThrottled ip {0}", remoteIP);

                return null;
            }

            _resetRequests.Set(remoteIP, remoteIP, ResetRequestThrottle);

            if (_resetToken.Find(ResetTokenKey) != null)
            {
                _authLogger.Info("Auth-ResetTokenActive ip {0} a password reset token has already been issued, restart {1} to issue another", remoteIP, BuildInfo.AppName);

                return null;
            }

            var token = SecretGenerator.Generate(ResetTokenLength);

            _resetToken.Set(ResetTokenKey, HashToken(token), ResetTokenLifetime);

            _authLogger.Warn("Auth-ResetRequested ip {0} token: {1} valid for {2} minutes", remoteIP, token, ResetTokenLifetime.TotalMinutes);

            return token;
        }

        public bool ResetPassword(HttpRequest request, string token, string username, string password)
        {
            if (_configFileProvider.EffectiveAuthenticationMethod() != AuthenticationType.Forms)
            {
                return false;
            }

            var remoteIP = request.GetRemoteIP();

            if (IsLockedOut(remoteIP))
            {
                LogLockout(remoteIP, username);

                return false;
            }

            if (username.IsNullOrWhiteSpace())
            {
                RecordFailedAttempt(remoteIP, username);

                return false;
            }

            var expected = _resetToken.Find(ResetTokenKey);
            var isMatchingToken = expected != null && IsMatchingToken(expected, token);
            var isMatchingConfiguredToken = _configuredResetTokenHash != null && IsMatchingToken(_configuredResetTokenHash, token);

            if (!isMatchingToken && !isMatchingConfiguredToken)
            {
                RecordFailedAttempt(remoteIP, username);

                return false;
            }

            if (isMatchingToken)
            {
                _resetToken.Remove(ResetTokenKey);
            }

            if (isMatchingConfiguredToken)
            {
                _configuredResetTokenHash = null;

                _authLogger.Info("Auth-ResetTokenDisabled ip {0} the configured password reset token has been used and is now disabled, remove the SONARR__AUTH__RESETTOKEN environment variable", remoteIP);
            }

            _userService.Upsert(username, password);

            _failedAttempts.Remove(remoteIP);

            _authLogger.Warn("Auth-PasswordReset ip {0} username '{1}'", remoteIP, username);

            return true;
        }

        private static string GetConfiguredResetTokenHash(string configuredResetToken)
        {
            if (configuredResetToken.IsNullOrWhiteSpace())
            {
                return null;
            }

            if (configuredResetToken.Length < MinimumConfiguredResetTokenLength)
            {
                _authLogger.Error("Configured password reset token is shorter than {0} characters and will be ignored", MinimumConfiguredResetTokenLength);

                return null;
            }

            return HashToken(configuredResetToken);
        }

        private static string HashToken(string token)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }

        private static bool IsMatchingToken(string expectedHash, string token)
        {
            if (token.IsNullOrWhiteSpace())
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedHash),
                Encoding.UTF8.GetBytes(HashToken(token)));
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
