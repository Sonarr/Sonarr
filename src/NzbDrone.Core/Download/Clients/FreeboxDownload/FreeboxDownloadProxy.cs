using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.FreeboxDownload.Responses;

namespace NzbDrone.Core.Download.Clients.FreeboxDownload
{
    public interface IFreeboxDownloadProxy
    {
        void Authenticate(FreeboxDownloadSettings settings);
        string AddTaskFromUrl(string url, string directory, bool addPaused, bool addFirst, double? seedRatio, FreeboxDownloadSettings settings);
        string AddTaskFromFile(string fileName, byte[] fileContent, string directory, bool addPaused, bool addFirst, double? seedRatio, FreeboxDownloadSettings settings);
        void DeleteTask(string id, bool deleteData, FreeboxDownloadSettings settings);
        FreeboxDownloadConfiguration GetDownloadConfiguration(FreeboxDownloadSettings settings);
        List<FreeboxDownloadTask> GetTasks(FreeboxDownloadSettings settings);
    }

    public class FreeboxDownloadProxy : IFreeboxDownloadProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private ICached<string> _authSessionTokenCache;

        public FreeboxDownloadProxy(ICacheManager cacheManager, IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _authSessionTokenCache = cacheManager.GetCache<string>(GetType(), "authSessionToken");
        }

        public void Authenticate(FreeboxDownloadSettings settings)
        {
            var request = BuildRequest(settings).Resource("/login").Build();

            var response = ProcessRequest<FreeboxLogin>(request, settings);

            if (response.Result.LoggedIn == false)
            {
                throw new DownloadClientAuthenticationException("Not logged");
            }
        }

        public string AddTaskFromUrl(string url, string directory, bool addPaused, bool addFirst, double? seedRatio, FreeboxDownloadSettings settings)
        {
            var request = BuildRequest(settings).Resource("/downloads/add").Post();
            request.Headers.ContentType = "application/x-www-form-urlencoded";

            request.AddFormParameter("download_url", System.Web.HttpUtility.UrlPathEncode(url));

            if (!directory.IsNullOrWhiteSpace())
            {
                request.AddFormParameter("download_dir", directory);
            }

            var response = ProcessRequest<FreeboxDownloadTask>(request.Build(), settings);

            SetTorrentSettings(response.Result.Id, addPaused, addFirst, seedRatio, settings);

            return response.Result.Id;
        }

        public string AddTaskFromFile(string fileName, byte[] fileContent, string directory, bool addPaused, bool addFirst, double? seedRatio, FreeboxDownloadSettings settings)
        {
            var request = BuildRequest(settings).Resource("/downloads/add").Post();

            request.AddFormUpload("download_file", fileName, fileContent, "multipart/form-data");

            if (directory.IsNotNullOrWhiteSpace())
            {
                request.AddFormParameter("download_dir", directory);
            }

            var response = ProcessRequest<FreeboxDownloadTask>(request.Build(), settings);

            SetTorrentSettings(response.Result.Id, addPaused, addFirst, seedRatio, settings);

            return response.Result.Id;
        }

        public void DeleteTask(string id, bool deleteData, FreeboxDownloadSettings settings)
        {
            var uri = "/downloads/" + id;

            if (deleteData == true)
            {
                uri += "/erase";
            }

            var request = BuildRequest(settings).Resource(uri).Build();

            request.Method = HttpMethod.Delete;

            ProcessRequest<string>(request, settings);
        }

        public FreeboxDownloadConfiguration GetDownloadConfiguration(FreeboxDownloadSettings settings)
        {
            var request = BuildRequest(settings).Resource("/downloads/config/").Build();

            return ProcessRequest<FreeboxDownloadConfiguration>(request, settings).Result;
        }

        public List<FreeboxDownloadTask> GetTasks(FreeboxDownloadSettings settings)
        {
            var request = BuildRequest(settings).Resource("/downloads/").Build();

            return ProcessRequest<List<FreeboxDownloadTask>>(request, settings).Result;
        }

        private static string BuildCachedHeaderKey(FreeboxDownloadSettings settings)
        {
            return $"{settings.Host}:{settings.AppId}:{settings.AppToken}";
        }

        private void SetTorrentSettings(string id, bool addPaused, bool addFirst, double? seedRatio, FreeboxDownloadSettings settings)
        {
            var request = BuildRequest(settings).Resource("/downloads/" + id).Build();

            request.Method = HttpMethod.Put;

            var body = new Dictionary<string, object> { };

            if (addPaused)
            {
                body.Add("status", FreeboxDownloadTaskStatus.Stopped.ToString().ToLower());
            }

            if (addFirst)
            {
                body.Add("queue_pos", "1");
            }

            if (seedRatio != null)
            {
                // 0 means unlimited seeding
                body.Add("stop_ratio", seedRatio);
            }

            if (body.Count == 0)
            {
                return;
            }

            request.SetContent(body.ToJson());

            ProcessRequest<FreeboxDownloadTask>(request, settings);
        }

        private string GetSessionToken(HttpRequestBuilder requestBuilder, FreeboxDownloadSettings settings, bool force = false)
        {
            var sessionToken = _authSessionTokenCache.Find(BuildCachedHeaderKey(settings));

            if (sessionToken == null || force)
            {
                _authSessionTokenCache.Remove(BuildCachedHeaderKey(settings));

                _logger.Debug($"Client needs a new Session Token to reach the API with App ID '{settings.AppId}'");

                // Obtaining a Session Token (from official documentation):
                // To protect the app_token secret, it will never be used directly to authenticate the
                // application, instead the API will provide a challenge the app will combine to its
                // app_token to open a session and get a session_token.
                // The validity of the session_token is limited in time and the app will have to renew
                // this session_token once in a while.

                // Retrieving the 'challenge' value (it changes frequently and have a limited time validity)
                // needed to build password
                var challengeRequest = requestBuilder.Resource("/login").Build();
                challengeRequest.Method = HttpMethod.Get;

                var challenge = ProcessRequest<FreeboxLogin>(challengeRequest, settings).Result.Challenge;

                // The password is computed using the 'challenge' value and the 'app_token' ('App Token' setting)
                var enc = System.Text.Encoding.ASCII;
                var hmac = new HMACSHA1(enc.GetBytes(settings.AppToken));
                hmac.Initialize();
                var buffer = enc.GetBytes(challenge);
                var password = System.BitConverter.ToString(hmac.ComputeHash(buffer)).Replace("-", "").ToLower();

                // Both 'app_id' ('App ID' setting) and computed password are set to get a Session Token
                var sessionRequest = requestBuilder.Resource("/login/session").Post().Build();
                var body = new Dictionary<string, object>
                {
                    { "app_id", settings.AppId },
                    { "password", password }
                };
                sessionRequest.SetContent(body.ToJson());

                sessionToken = ProcessRequest<FreeboxLogin>(sessionRequest, settings).Result.SessionToken;

                _authSessionTokenCache.Set(BuildCachedHeaderKey(settings), sessionToken);

                _logger.Debug($"New Session Token stored in cache for App ID '{settings.AppId}', ready to reach API");
            }

            return sessionToken;
        }

        private HttpRequestBuilder BuildRequest(FreeboxDownloadSettings settings, bool authentication = true)
        {
            var requestBuilder = new HttpRequestBuilder(settings.UseSsl, settings.Host, settings.Port, settings.ApiUrl)
            {
                LogResponseContent = true
            };

            requestBuilder.Headers.ContentType = "application/json";

            if (authentication == true)
            {
                requestBuilder.SetHeader("X-Fbx-App-Auth", GetSessionToken(requestBuilder, settings));
            }

            return requestBuilder;
        }

        private FreeboxResponse<T> ProcessRequest<T>(HttpRequest request, FreeboxDownloadSettings settings)
        {
            request.LogResponseContent = true;
            request.SuppressHttpError = true;

            HttpResponse response;

            try
            {
                response = _httpClient.Execute(request);
            }
            catch (HttpRequestException ex)
            {
                throw new DownloadClientUnavailableException($"Unable to reach Freebox API. Verify 'Host', 'Port' or 'Use SSL' settings. (Error: {ex.Message})", ex);
            }
            catch (WebException ex)
            {
                throw new DownloadClientUnavailableException("Unable to connect to Freebox API, please check your settings", ex);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _authSessionTokenCache.Remove(BuildCachedHeaderKey(settings));

                var responseContent = Json.Deserialize<FreeboxResponse<FreeboxLogin>>(response.Content);

                var msg = $"Authentication to Freebox API failed. Reason: {responseContent.GetErrorDescription()}";
                _logger.Error(msg);
                throw new DownloadClientAuthenticationException(msg);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new FreeboxDownloadException("Unable to reach Freebox API. Verify 'API URL' setting for base URL and version.");
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = Json.Deserialize<FreeboxResponse<T>>(response.Content);

                if (responseContent.Success)
                {
                    return responseContent;
                }
                else
                {
                    var msg = $"Freebox API returned error: {responseContent.GetErrorDescription()}";
                    _logger.Error(msg);
                    throw new DownloadClientException(msg);
                }
            }
            else
            {
                throw new DownloadClientException("Unable to connect to Freebox, please check your settings.");
            }
        }
    }
}
