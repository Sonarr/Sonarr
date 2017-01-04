using System;
using System.Net;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Http.Dispatchers
{
    public class FallbackHttpDispatcher : IHttpDispatcher
    {
        private readonly ManagedHttpDispatcher _managedDispatcher;
        private readonly CurlHttpDispatcher _curlDispatcher;
        private readonly IPlatformInfo _platformInfo;
        private readonly Logger _logger;

        private readonly ICached<bool> _curlTLSFallbackCache;

        public FallbackHttpDispatcher(ManagedHttpDispatcher managedDispatcher, CurlHttpDispatcher curlDispatcher, ICacheManager cacheManager, IPlatformInfo platformInfo, Logger logger)
        {
            _managedDispatcher = managedDispatcher;
            _curlDispatcher = curlDispatcher;
            _platformInfo = platformInfo;
            _curlTLSFallbackCache = cacheManager.GetCache<bool>(GetType(), "curlTLSFallback");
            _logger = logger;
        }

        public HttpResponse GetResponse(HttpRequest request, CookieContainer cookies)
        {
            if (PlatformInfo.IsMono && request.Url.Scheme == "https")
            {
                if (!_curlTLSFallbackCache.Find(request.Url.Host))
                {
                    try
                    {
                        return _managedDispatcher.GetResponse(request, cookies);
                    }
                    catch (Exception ex)
                    {
                        if (ex.ToString().Contains("The authentication or decryption has failed."))
                        {
                            _logger.Debug("https request failed in tls error for {0}, trying curl fallback.", request.Url.Host);

                            _curlTLSFallbackCache.Set(request.Url.Host, true);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (_curlDispatcher.CheckAvailability())
                {
                    return _curlDispatcher.GetResponse(request, cookies);
                }

                _logger.Trace("Curl not available, using default WebClient.");
            }

            return _managedDispatcher.GetResponse(request, cookies);
        }
    }
}
