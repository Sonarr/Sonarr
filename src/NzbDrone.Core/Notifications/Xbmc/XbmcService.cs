using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public interface IXbmcService
    {
        void Notify(XbmcSettings settings, string title, string message);
        void Update(XbmcSettings settings, Series series);
        void Clean(XbmcSettings settings);
        ValidationFailure Test(XbmcSettings settings, string message);
    }

    public class XbmcService : IXbmcService
    {
        private readonly IXbmcJsonApiProxy _proxy;
        private readonly IEnumerable<IApiProvider> _apiProviders;
        private readonly Logger _logger;

        private readonly ICached<XbmcVersion> _xbmcVersionCache;

        public XbmcService(IXbmcJsonApiProxy proxy,
                           IEnumerable<IApiProvider> apiProviders,
                           ICacheManager cacheManager,
                           Logger logger)
        {
            _proxy = proxy;
            _apiProviders = apiProviders;
            _logger = logger;

            _xbmcVersionCache = cacheManager.GetCache<XbmcVersion>(GetType());
        }

        public void Notify(XbmcSettings settings, string title, string message)
        {
            var provider = GetApiProvider(settings);
            provider.Notify(settings, title, message);
        }

        public void Update(XbmcSettings settings, Series series)
        {
            var provider = GetApiProvider(settings);
            provider.Update(settings, series);
        }

        public void Clean(XbmcSettings settings)
        {
            var provider = GetApiProvider(settings);
            provider.Clean(settings);
        }

        private XbmcVersion GetJsonVersion(XbmcSettings settings)
        {
            return _xbmcVersionCache.Get(settings.Address, () =>
            {
                var response = _proxy.GetJsonVersion(settings);

                _logger.Debug("Getting version from response: " + response);
                var result = Json.Deserialize<XbmcJsonResult<JObject>>(response);

                var versionObject = result.Result.Property("version");

                if (versionObject.Value.Type == JTokenType.Integer)
                {
                    return new XbmcVersion((int)versionObject.Value);
                }

                if (versionObject.Value.Type == JTokenType.Object)
                {
                    return Json.Deserialize<XbmcVersion>(versionObject.Value.ToString());
                }

                throw new InvalidCastException("Unknown Version structure!: " + versionObject);
            }, TimeSpan.FromHours(12));
        }

        private IApiProvider GetApiProvider(XbmcSettings settings)
        {
            var version = GetJsonVersion(settings);
            var apiProvider = _apiProviders.SingleOrDefault(a => a.CanHandle(version));

            if (apiProvider == null)
            {
                var message = string.Format("Invalid API Version: {0} for {1}", version, settings.Address);
                throw new InvalidXbmcVersionException(message);
            }

            return apiProvider;
        }

        public ValidationFailure Test(XbmcSettings settings, string message)
        {
            _xbmcVersionCache.Clear();

            try
            {
                _logger.Debug("Determining version of Host: {0}", settings.Address);
                var version = GetJsonVersion(settings);
                _logger.Debug("Version is: {0}", version);

                if (version == new XbmcVersion(0))
                {
                    throw new InvalidXbmcVersionException("Version received from XBMC is invalid, please correct your settings.");
                }

                Notify(settings, "Test Notification", message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("Host", "Unable to send test message");
            }

            return null;
        }
    }
}