using System;
using System.Linq;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using NzbDrone.Core.Configuration;
using Sonarr.Http;

namespace NzbDrone.Host
{
    public class CorsOriginConfigurator(IConfigFileProvider configFileProvider, NLog.Logger logger) : IPostConfigureOptions<CorsOptions>
    {
        private readonly IConfigFileProvider _configFileProvider = configFileProvider;
        private readonly NLog.Logger _logger = logger;

        public void PostConfigure(string name, CorsOptions options) => PostConfigure(options);

        public void PostConfigure(CorsOptions options)
        {
            _logger.Info("Configuring CORS. Allowed origins: {0}", _configFileProvider.AllowedCorsOrigins);
            options.GetPolicy(VersionedApiControllerAttribute.API_CORS_POLICY).IsOriginAllowed = CorsOriginCheck;
            options.GetPolicy("AllowGet").IsOriginAllowed = CorsOriginCheck;
        }

        /// <summary>
        /// Check if the request origin is allowed based on the configured allowed origins in the config file.
        /// </summary>
        /// <param name="requestOrigin">The origin to check against the list of allowed cross-origin domains.</param>
        /// <returns>True if the request origin matches a configured origin (based upon the CORS spec), or if the allowed origins contains '*', false otherwise.</returns>
        protected bool CorsOriginCheck(string requestOrigin)
        {
            var allowedOrigins = _configFileProvider.AllowedCorsOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return allowedOrigins.Any(
                allowedOrigin => allowedOrigin.Equals(CorsConstants.AnyOrigin, StringComparison.Ordinal) ||
                IsSameOrigin(allowedOrigin, requestOrigin));
        }

        /// <summary>
        /// Check if two origins are the same based on scheme, host and port.
        /// </summary>
        /// <param name="configuredOrigin">The first origin to compare.</param>
        /// <param name="requestOrigin">The second origin to compare.</param>
        /// <returns>True if the scheme, host, and port match, false otherwise.</returns>
        protected bool IsSameOrigin(string configuredOrigin, string requestOrigin)
        {
            if (!Uri.TryCreate(configuredOrigin, UriKind.Absolute, out var configuredOriginUri))
            {
                _logger.Warn("Invalid origin configured: {0}", configuredOrigin);
                return false;
            }

            if (!Uri.TryCreate(requestOrigin, UriKind.Absolute, out var requestOriginUri))
            {
                _logger.Debug("Invalid request origin: {0}", requestOrigin);
                return false;
            }

            return Uri.Compare(configuredOriginUri, requestOriginUri, UriComponents.SchemeAndServer, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
