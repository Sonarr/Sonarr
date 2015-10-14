using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Rest;
using NzbDrone.Core.Download.Clients.NzbVortex.Responses;
using RestSharp;

namespace NzbDrone.Core.Download.Clients.NzbVortex
{
    public interface INzbVortexProxy
    {
        string DownloadNzb(byte[] nzbData, string filename, int priority, NzbVortexSettings settings);
        void Remove(int id, bool deleteData, NzbVortexSettings settings);
        NzbVortexVersionResponse GetVersion(NzbVortexSettings settings);
        NzbVortexApiVersionResponse GetApiVersion(NzbVortexSettings settings);
        List<NzbVortexGroup> GetGroups(NzbVortexSettings settings);
        NzbVortexQueue GetQueue(int doneLimit, NzbVortexSettings settings);
        NzbVortexFiles GetFiles(int id, NzbVortexSettings settings);
    }

    public class NzbVortexProxy : INzbVortexProxy
    {
        private readonly ICached<string> _authCache;
        private readonly Logger _logger;

        public NzbVortexProxy(ICacheManager cacheManager, Logger logger)
        {
            _authCache = cacheManager.GetCache<string>(GetType(), "authCache");
            _logger = logger;
        }

        public string DownloadNzb(byte[] nzbData, string filename, int priority, NzbVortexSettings settings)
        {
            var request = BuildRequest("/nzb/add", Method.POST, true, settings);

            request.AddFile("name", nzbData, filename, "application/x-nzb");
            request.AddQueryParameter("priority", priority.ToString());

            if (settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                request.AddQueryParameter("groupname", settings.TvCategory);                
            }

            var response = ProcessRequest<NzbVortexAddResponse>(request, settings);

            return response.Id;
        }

        public void Remove(int id, bool deleteData, NzbVortexSettings settings)
        {
            var request = BuildRequest(string.Format("nzb/{0}/cancel", id), Method.GET, true, settings);

            if (deleteData)
            {
                request.Resource += "Delete";
            }

            ProcessRequest(request, settings);
        }

        public NzbVortexVersionResponse GetVersion(NzbVortexSettings settings)
        {
            var request = BuildRequest("app/appversion", Method.GET, false, settings);
            var response = ProcessRequest<NzbVortexVersionResponse>(request, settings);

            return response;
        }

        public NzbVortexApiVersionResponse GetApiVersion(NzbVortexSettings settings)
        {
            var request = BuildRequest("app/apilevel", Method.GET, false, settings);
            var response = ProcessRequest<NzbVortexApiVersionResponse>(request, settings);

            return response;
        }

        public List<NzbVortexGroup> GetGroups(NzbVortexSettings settings)
        {
            var request = BuildRequest("group", Method.GET, true, settings);
            var response = ProcessRequest<NzbVortexGroupResponse>(request, settings);

            return response.Groups;
        }

        public NzbVortexQueue GetQueue(int doneLimit, NzbVortexSettings settings)
        {
            var request = BuildRequest("nzb", Method.GET, true, settings);

            if (settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                request.AddQueryParameter("groupName", settings.TvCategory);
            }

            request.AddQueryParameter("limitDone", doneLimit.ToString());

            var response = ProcessRequest<NzbVortexQueue>(request, settings);

            return response;
        }

        public NzbVortexFiles GetFiles(int id, NzbVortexSettings settings)
        {
            var request = BuildRequest(string.Format("file/{0}", id), Method.GET, true, settings);
            var response = ProcessRequest<NzbVortexFiles>(request, settings);

            return response;
        }

        private string GetSessionId(bool force, NzbVortexSettings settings)
        {
            var authCacheKey = string.Format("{0}_{1}_{2}", settings.Host, settings.Port, settings.ApiKey);

            if (force)
            {
                _authCache.Remove(authCacheKey);
            }

            var sessionId = _authCache.Get(authCacheKey, () => Authenticate(settings));

            return sessionId;
        }

        private string Authenticate(NzbVortexSettings settings)
        {
            var nonce = GetNonce(settings);
            var cnonce = Guid.NewGuid().ToString();
            var hashString = string.Format("{0}:{1}:{2}", nonce, cnonce, settings.ApiKey);
            var sha256 = hashString.SHA256Hash();
            var base64 = Convert.ToBase64String(sha256.HexToByteArray());
            var request = BuildRequest("auth/login", Method.GET, false, settings);

            request.AddQueryParameter("nonce", nonce);
            request.AddQueryParameter("cnonce", cnonce);
            request.AddQueryParameter("hash", base64);

            var response = ProcessRequest(request, settings);
            var result = Json.Deserialize<NzbVortexAuthResponse>(response);

            if (result.LoginResult == NzbVortexLoginResultType.Failed)
            {
                throw new NzbVortexAuthenticationException("Authentication failed, check your API Key");
            }

            return result.SessionId;
        }

        private string GetNonce(NzbVortexSettings settings)
        {
            var request = BuildRequest("auth/nonce", Method.GET, false, settings);
            
            return ProcessRequest<NzbVortexAuthNonceResponse>(request, settings).AuthNonce;
        }

        private IRestClient BuildClient(NzbVortexSettings settings)
        {
            var url = string.Format(@"https://{0}:{1}/api", settings.Host, settings.Port);

            return RestClientFactory.BuildClient(url);
        }

        private IRestRequest BuildRequest(string resource, Method method, bool requiresAuthentication, NzbVortexSettings settings)
        {
            var request = new RestRequest(resource, method);

            if (requiresAuthentication)
            {
                request.AddQueryParameter("sessionid", GetSessionId(false, settings));
            }

            return request;
        }

        private T ProcessRequest<T>(IRestRequest request, NzbVortexSettings settings) where T : new()
        {
            return Json.Deserialize<T>(ProcessRequest(request, settings));
        }

        private string ProcessRequest(IRestRequest request, NzbVortexSettings settings)
        {
            var client = BuildClient(settings);

            try
            {
                return ProcessRequest(client, request).Content;
            }
            catch (NzbVortexNotLoggedInException ex)
            {
                _logger.Warn("Not logged in response received, reauthenticating and retrying");
                request.AddQueryParameter("sessionid", GetSessionId(true, settings));

                return ProcessRequest(client, request).Content;
            }
        }

        private IRestResponse ProcessRequest(IRestClient client, IRestRequest request)
        {
            _logger.Debug("URL: {0}/{1}", client.BaseUrl, request.Resource);
            var response = client.Execute(request);

            _logger.Trace("Response: {0}", response.Content);
            CheckForError(response);

            return response;
        }

        private void CheckForError(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new DownloadClientException("Unable to connect to NZBVortex, please check your settings", response.ErrorException);
            }

            NzbVortexResponseBase result;

            if (Json.TryDeserialize<NzbVortexResponseBase>(response.Content, out result))
            {
                if (result.Result == NzbVortexResultType.NotLoggedIn)
                {
                    throw new NzbVortexNotLoggedInException();
                }
            }

            else
            {
                throw new DownloadClientException("Response could not be processed: {0}", response.Content);
            }
        }
    }
}
