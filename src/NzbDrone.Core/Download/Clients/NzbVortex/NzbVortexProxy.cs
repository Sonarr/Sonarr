using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.NzbVortex.Responses;

namespace NzbDrone.Core.Download.Clients.NzbVortex
{
    public interface INzbVortexProxy
    {
        string DownloadNzb(byte[] nzbData, string filename, int priority, NzbVortexSettings settings);
        void Remove(int id, bool deleteData, NzbVortexSettings settings);
        NzbVortexVersionResponse GetVersion(NzbVortexSettings settings);
        NzbVortexApiVersionResponse GetApiVersion(NzbVortexSettings settings);
        List<NzbVortexGroup> GetGroups(NzbVortexSettings settings);
        List<NzbVortexQueueItem> GetQueue(int doneLimit, NzbVortexSettings settings);
        List<NzbVortexFile> GetFiles(int id, NzbVortexSettings settings);
    }

    public class NzbVortexProxy : INzbVortexProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private readonly ICached<string> _authSessionIdCache;

        public NzbVortexProxy(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _authSessionIdCache = cacheManager.GetCache<string>(GetType(), "authCache");
        }

        public string DownloadNzb(byte[] nzbData, string filename, int priority, NzbVortexSettings settings)
        {
            var requestBuilder = BuildRequest(settings).Resource("nzb/add")
                                                       .Post()
                                                       .AddQueryParam("priority", priority.ToString());

            if (settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("groupname", settings.TvCategory);
            }

            requestBuilder.AddFormUpload("name", filename, nzbData, "application/x-nzb");

            var response = ProcessRequest<NzbVortexAddResponse>(requestBuilder, true, settings);

            return response.Id;
        }

        public void Remove(int id, bool deleteData, NzbVortexSettings settings)
        {
            var requestBuilder = BuildRequest(settings).Resource(string.Format("nzb/{0}/{1}", id, deleteData ? "cancelDelete" : "cancel"));

            ProcessRequest<NzbVortexResponseBase>(requestBuilder, true, settings);
        }

        public NzbVortexVersionResponse GetVersion(NzbVortexSettings settings)
        {
            var requestBuilder = BuildRequest(settings).Resource("app/appversion");

            var response = ProcessRequest<NzbVortexVersionResponse>(requestBuilder, false, settings);

            return response;
        }

        public NzbVortexApiVersionResponse GetApiVersion(NzbVortexSettings settings)
        {
            var requestBuilder = BuildRequest(settings).Resource("app/apilevel");

            var response = ProcessRequest<NzbVortexApiVersionResponse>(requestBuilder, false, settings);

            return response;
        }

        public List<NzbVortexGroup> GetGroups(NzbVortexSettings settings)
        {
            var request = BuildRequest(settings).Resource("group");
            var response = ProcessRequest<NzbVortexGroupResponse>(request, true, settings);

            return response.Groups;
        }

        public List<NzbVortexQueueItem> GetQueue(int doneLimit, NzbVortexSettings settings)
        {
            var requestBuilder = BuildRequest(settings).Resource("nzb");

            if (settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("groupName", settings.TvCategory);
            }

            requestBuilder.AddQueryParam("limitDone", doneLimit.ToString());

            var response = ProcessRequest<NzbVortexQueueResponse>(requestBuilder, true, settings);

            return response.Items;
        }

        public List<NzbVortexFile> GetFiles(int id, NzbVortexSettings settings)
        {
            var requestBuilder = BuildRequest(settings).Resource(string.Format("file/{0}", id));

            var response = ProcessRequest<NzbVortexFilesResponse>(requestBuilder, true, settings);

            return response.Files;
        }

        private HttpRequestBuilder BuildRequest(NzbVortexSettings settings)
        {
            var baseUrl = HttpRequestBuilder.BuildBaseUrl(true, settings.Host, settings.Port, settings.UrlBase);
            baseUrl = HttpUri.CombinePath(baseUrl, "api");
            var requestBuilder = new HttpRequestBuilder(baseUrl);
            requestBuilder.LogResponseContent = true;

            return requestBuilder;
        }

        private T ProcessRequest<T>(HttpRequestBuilder requestBuilder, bool requiresAuthentication, NzbVortexSettings settings)
            where T : NzbVortexResponseBase, new()
        {
            if (requiresAuthentication)
            {
                AuthenticateClient(requestBuilder, settings);
            }

            HttpResponse response = null;
            try
            {
                response = _httpClient.Execute(requestBuilder.Build());

                var result = Json.Deserialize<T>(response.Content);

                if (result.Result == NzbVortexResultType.NotLoggedIn)
                {
                    _logger.Debug("Not logged in response received, reauthenticating and retrying");
                    AuthenticateClient(requestBuilder, settings, true);

                    response = _httpClient.Execute(requestBuilder.Build());

                    result = Json.Deserialize<T>(response.Content);

                    if (result.Result == NzbVortexResultType.NotLoggedIn)
                    {
                        throw new DownloadClientException("Unable to connect to remain authenticated to NzbVortex");
                    }
                }

                return result;
            }
            catch (JsonException ex)
            {
                throw new DownloadClientException("NzbVortex response could not be processed {0}: {1}", ex.Message, response.Content);
            }
            catch (HttpException ex)
            {
                throw new DownloadClientException("Unable to connect to NZBVortex, please check your settings", ex);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.TrustFailure)
                {
                    throw new DownloadClientUnavailableException("Unable to connect to NZBVortex, certificate validation failed.", ex);
                }

                throw new DownloadClientUnavailableException("Unable to connect to NZBVortex, please check your settings", ex);
            }
        }

        private void AuthenticateClient(HttpRequestBuilder requestBuilder, NzbVortexSettings settings, bool reauthenticate = false)
        {
            var authKey = string.Format("{0}:{1}", requestBuilder.BaseUrl, settings.ApiKey);

            var sessionId = _authSessionIdCache.Find(authKey);

            if (sessionId == null || reauthenticate)
            {
                _authSessionIdCache.Remove(authKey);

                var nonceRequest = BuildRequest(settings).Resource("auth/nonce").Build();
                var nonceResponse = _httpClient.Execute(nonceRequest);

                var nonce = Json.Deserialize<NzbVortexAuthNonceResponse>(nonceResponse.Content).AuthNonce;

                var cnonce = Guid.NewGuid().ToString();

                var hashString = string.Format("{0}:{1}:{2}", nonce, cnonce, settings.ApiKey);
                var hash = Convert.ToBase64String(hashString.SHA256Hash().HexToByteArray());

                var authRequest = BuildRequest(settings).Resource("auth/login")
                                                        .AddQueryParam("nonce", nonce)
                                                        .AddQueryParam("cnonce", cnonce)
                                                        .AddQueryParam("hash", hash)
                                                        .Build();
                var authResponse = _httpClient.Execute(authRequest);
                var authResult = Json.Deserialize<NzbVortexAuthResponse>(authResponse.Content);

                if (authResult.LoginResult == NzbVortexLoginResultType.Failed)
                {
                    throw new NzbVortexAuthenticationException("Authentication failed, check your API Key");
                }

                sessionId = authResult.SessionId;

                _authSessionIdCache.Set(authKey, sessionId);
            }

            requestBuilder.AddQueryParam("sessionid", sessionId);
        }
    }
}
