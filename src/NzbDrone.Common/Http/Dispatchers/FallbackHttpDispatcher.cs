using System;
using System.Net;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Http.Dispatchers
{
    public class FallbackHttpDispatcher : IHttpDispatcher
    {
        private readonly Logger _logger;
        private readonly ICached<bool> _curlTLSFallbackCache;
        private readonly ManagedHttpDispatcher _managedDispatcher;
        private readonly CurlHttpDispatcher _curlDispatcher;

        public FallbackHttpDispatcher(ICached<bool> curlTLSFallbackCache, Logger logger)
        {
            _logger = logger;
            _curlTLSFallbackCache = curlTLSFallbackCache;
            _managedDispatcher = new ManagedHttpDispatcher();
            _curlDispatcher = new CurlHttpDispatcher();
        }

        public HttpResponse GetResponse(HttpRequest request, CookieContainer cookies)
        {
            if (OsInfo.IsMonoRuntime && request.Url.Scheme == "https")
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

                if (CurlHttpDispatcher.CheckAvailability())
                {
                    return _curlDispatcher.GetResponse(request, cookies);
                }

                _logger.Trace("Curl not available, using default WebClient.");
            }

            return _managedDispatcher.GetResponse(request, cookies);
        }
    }
}
