using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.DownloadStation.Responses;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Proxies
{
    public interface IDiskStationProxy
    {
        DiskStationApiInfo GetApiInfo(DownloadStationSettings settings);
    }

    public abstract class DiskStationProxyBase : IDiskStationProxy
    {
        protected readonly Logger _logger;

        private readonly IHttpClient _httpClient;
        private readonly ICached<DiskStationApiInfo> _infoCache;
        private readonly ICached<string> _sessionCache;
        private readonly DiskStationApi _apiType;
        private readonly string _apiName;

        private static readonly DiskStationApiInfo _apiInfo;

        static DiskStationProxyBase()
        {
            _apiInfo = new DiskStationApiInfo()
            {
                Type = DiskStationApi.Info,
                Name = "SYNO.API.Info",
                Path = "query.cgi",
                MaxVersion = 1,
                MinVersion = 1,
                NeedsAuthentication = false
            };
        }

        public DiskStationProxyBase(DiskStationApi apiType,
                                    string apiName,
                                    IHttpClient httpClient,
                                    ICacheManager cacheManager,
                                    Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _infoCache = cacheManager.GetCache<DiskStationApiInfo>(typeof(DiskStationProxyBase), "apiInfo");
            _sessionCache = cacheManager.GetCache<string>(typeof(DiskStationProxyBase), "sessions");
            _apiType = apiType;
            _apiName = apiName;
        }

        private string GenerateSessionCacheKey(DownloadStationSettings settings)
        {
            return $"{settings.Username}@{settings.Host}:{settings.Port}";
        }

        protected DiskStationResponse<T> ProcessRequest<T>(HttpRequestBuilder requestBuilder,
                                                         string operation,
                                                         DownloadStationSettings settings)
            where T : new()
        {
            return ProcessRequest<T>(requestBuilder, operation, _apiType, settings);
        }

        private DiskStationResponse<T> ProcessRequest<T>(HttpRequestBuilder requestBuilder,
                                                         string operation,
                                                         DiskStationApi api,
                                                         DownloadStationSettings settings)
            where T : new()
        {
            var request = requestBuilder.Build();
            HttpResponse response;

            try
            {
                response = _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                throw new DownloadClientException("Unable to connect to Diskstation, please check your settings", ex);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.TrustFailure)
                {
                    throw new DownloadClientUnavailableException("Unable to connect to Diskstation, certificate validation failed.", ex);
                }

                throw new DownloadClientUnavailableException("Unable to connect to Diskstation, please check your settings", ex);
            }

            _logger.Debug("Trying to {0}", operation);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = Json.Deserialize<DiskStationResponse<T>>(response.Content);

                if (responseContent.Success)
                {
                    return responseContent;
                }
                else
                {
                    var msg = $"Failed to {operation}. Reason: {responseContent.Error.GetMessage(api)}";
                    _logger.Error(msg);

                    if (responseContent.Error.SessionError)
                    {
                        _sessionCache.Remove(GenerateSessionCacheKey(settings));

                        if (responseContent.Error.Code == 105)
                        {
                            throw new DownloadClientAuthenticationException(msg);
                        }
                    }

                    throw new DownloadClientException(msg);
                }
            }
            else
            {
                throw new HttpException(request, response);
            }
        }

        private string AuthenticateClient(DownloadStationSettings settings)
        {
            var authInfo = GetApiInfo(DiskStationApi.Auth, settings);

            var requestBuilder = BuildRequest(settings, authInfo, "login", authInfo.MaxVersion >= 7 ? 6 : 2);
            requestBuilder.AddQueryParam("account", settings.Username);
            requestBuilder.AddQueryParam("passwd", settings.Password);
            requestBuilder.AddQueryParam("format", "sid");
            requestBuilder.AddQueryParam("session", "DownloadStation");

            var authResponse = ProcessRequest<DiskStationAuthResponse>(requestBuilder, "login", DiskStationApi.Auth, settings);

            return authResponse.Data.SId;
        }

        protected HttpRequestBuilder BuildRequest(DownloadStationSettings settings, string methodName, int apiVersion, HttpMethod httpVerb = null)
        {
            httpVerb ??= HttpMethod.Get;

            var info = GetApiInfo(_apiType, settings);

            return BuildRequest(settings, info, methodName, apiVersion, httpVerb);
        }

        private HttpRequestBuilder BuildRequest(DownloadStationSettings settings, DiskStationApiInfo apiInfo, string methodName, int apiVersion, HttpMethod httpVerb = null)
        {
            httpVerb ??= HttpMethod.Get;

            var requestBuilder = new HttpRequestBuilder(settings.UseSsl, settings.Host, settings.Port).Resource($"webapi/{apiInfo.Path}");
            requestBuilder.Method = httpVerb;
            requestBuilder.LogResponseContent = true;
            requestBuilder.SuppressHttpError = true;
            requestBuilder.AllowAutoRedirect = false;
            requestBuilder.Headers.ContentType = "application/json";

            if (apiVersion < apiInfo.MinVersion || apiVersion > apiInfo.MaxVersion)
            {
                throw new ArgumentOutOfRangeException(nameof(apiVersion));
            }

            if (httpVerb == HttpMethod.Post)
            {
                if (apiInfo.NeedsAuthentication)
                {
                    if (_apiType == DiskStationApi.DownloadStation2Task)
                    {
                        requestBuilder.AddQueryParam("_sid", _sessionCache.Get(GenerateSessionCacheKey(settings), () => AuthenticateClient(settings), TimeSpan.FromHours(6)));
                    }
                    else
                    {
                        requestBuilder.AddFormParameter("_sid", _sessionCache.Get(GenerateSessionCacheKey(settings), () => AuthenticateClient(settings), TimeSpan.FromHours(6)));
                    }
                }

                requestBuilder.AddFormParameter("api", apiInfo.Name);
                requestBuilder.AddFormParameter("version", apiVersion);
                requestBuilder.AddFormParameter("method", methodName);
            }
            else
            {
                if (apiInfo.NeedsAuthentication)
                {
                    requestBuilder.AddQueryParam("_sid", _sessionCache.Get(GenerateSessionCacheKey(settings), () => AuthenticateClient(settings), TimeSpan.FromHours(6)));
                }

                requestBuilder.AddQueryParam("api", apiInfo.Name);
                requestBuilder.AddQueryParam("version", apiVersion);
                requestBuilder.AddQueryParam("method", methodName);
            }

            return requestBuilder;
        }

        private string GenerateInfoCacheKey(DownloadStationSettings settings, DiskStationApi api)
        {
            return $"{settings.Host}:{settings.Port}->{api}";
        }

        private void UpdateApiInfo(DownloadStationSettings settings)
        {
            var apis = new Dictionary<string, DiskStationApi>()
            {
                { "SYNO.API.Auth", DiskStationApi.Auth },
                { _apiName, _apiType }
            };

            var requestBuilder = BuildRequest(settings, _apiInfo, "query", _apiInfo.MinVersion);
            requestBuilder.AddQueryParam("query", string.Join(",", apis.Keys));

            var infoResponse = ProcessRequest<DiskStationApiInfoResponse>(requestBuilder, "get api info", _apiInfo.Type, settings);

            foreach (var data in infoResponse.Data)
            {
                if (apis.ContainsKey(data.Key))
                {
                    data.Value.Name = data.Key;
                    data.Value.Type = apis[data.Key];
                    data.Value.NeedsAuthentication = apis[data.Key] != DiskStationApi.Auth;

                    _infoCache.Set(GenerateInfoCacheKey(settings, apis[data.Key]), data.Value, TimeSpan.FromHours(1));
                }
            }
        }

        private DiskStationApiInfo GetApiInfo(DiskStationApi api, DownloadStationSettings settings)
        {
            if (api == DiskStationApi.Info)
            {
                return _apiInfo;
            }

            var key = GenerateInfoCacheKey(settings, api);
            var info = _infoCache.Find(key);

            if (info == null)
            {
                UpdateApiInfo(settings);
                info = _infoCache.Find(key);

                if (info == null)
                {
                    if (api == DiskStationApi.DownloadStation2Task)
                    {
                        _logger.Warn("Info of {0} not found on {1}:{2}", api, settings.Host, settings.Port);
                    }
                    else
                    {
                        throw new DownloadClientException("Info of {0} not found on {1}:{2}", api, settings.Host, settings.Port);
                    }
                }
            }

            return info;
        }

        public DiskStationApiInfo GetApiInfo(DownloadStationSettings settings)
        {
            return GetApiInfo(_apiType, settings);
        }
    }
}
