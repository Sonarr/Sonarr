using System;
using System.Net;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.Sabnzbd.Responses;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public interface ISabnzbdProxy
    {
        string GetBaseUrl(SabnzbdSettings settings, string relativePath = null);
        SabnzbdAddResponse DownloadNzb(byte[] nzbData, string filename, string category, int priority, SabnzbdSettings settings);
        void RemoveFrom(string source, string id, bool deleteData, SabnzbdSettings settings);
        string GetVersion(SabnzbdSettings settings);
        SabnzbdConfig GetConfig(SabnzbdSettings settings);
        SabnzbdFullStatus GetFullStatus(SabnzbdSettings settings);
        SabnzbdQueue GetQueue(int start, int limit, SabnzbdSettings settings);
        SabnzbdHistory GetHistory(int start, int limit, string category, SabnzbdSettings settings);
        string RetryDownload(string id, SabnzbdSettings settings);
    }

    public class SabnzbdProxy : ISabnzbdProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public SabnzbdProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public string GetBaseUrl(SabnzbdSettings settings, string relativePath = null)
        {
            var baseUrl = HttpRequestBuilder.BuildBaseUrl(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase);
            baseUrl = HttpUri.CombinePath(baseUrl, relativePath);

            return baseUrl;
        }

        public SabnzbdAddResponse DownloadNzb(byte[] nzbData, string filename, string category, int priority, SabnzbdSettings settings)
        {
            var request = BuildRequest("addfile", settings).Post();

            request.AddQueryParam("cat", category);
            request.AddQueryParam("priority", priority);

            request.AddFormUpload("name", filename, nzbData, "application/x-nzb");

            SabnzbdAddResponse response;

            if (!Json.TryDeserialize<SabnzbdAddResponse>(ProcessRequest(request, settings), out response))
            {
                response = new SabnzbdAddResponse();
                response.Status = true;
            }

            return response;
        }

        public void RemoveFrom(string source, string id, bool deleteData, SabnzbdSettings settings)
        {
            var request = BuildRequest(source, settings);
            request.AddQueryParam("name", "delete");
            request.AddQueryParam("del_files", deleteData ? 1 : 0);
            request.AddQueryParam("value", id);

            ProcessRequest(request, settings);
        }

        public string GetVersion(SabnzbdSettings settings)
        {
            var request = BuildRequest("version", settings);

            SabnzbdVersionResponse response;

            if (!Json.TryDeserialize<SabnzbdVersionResponse>(ProcessRequest(request, settings), out response))
            {
                response = new SabnzbdVersionResponse();
            }

            return response.Version;
        }

        public SabnzbdConfig GetConfig(SabnzbdSettings settings)
        {
            var request = BuildRequest("get_config", settings);

            var response = Json.Deserialize<SabnzbdConfigResponse>(ProcessRequest(request, settings));

            return response.Config;
        }

        public SabnzbdFullStatus GetFullStatus(SabnzbdSettings settings)
        {
            var request = BuildRequest("fullstatus", settings);
            request.AddQueryParam("skip_dashboard", "1");

            var response = Json.Deserialize<SabnzbdFullStatusResponse>(ProcessRequest(request, settings));

            return response.Status;
        }

        public SabnzbdQueue GetQueue(int start, int limit, SabnzbdSettings settings)
        {
            var request = BuildRequest("queue", settings);
            request.AddQueryParam("start", start);
            request.AddQueryParam("limit", limit);

            var response = ProcessRequest(request, settings);

            return Json.Deserialize<SabnzbdQueue>(JObject.Parse(response).SelectToken("queue").ToString());
        }

        public SabnzbdHistory GetHistory(int start, int limit, string category, SabnzbdSettings settings)
        {
            var request = BuildRequest("history", settings);
            request.AddQueryParam("start", start);
            request.AddQueryParam("limit", limit);

            if (category.IsNotNullOrWhiteSpace())
            {
                request.AddQueryParam("category", category);
            }

            var response = ProcessRequest(request, settings);

            return Json.Deserialize<SabnzbdHistory>(JObject.Parse(response).SelectToken("history").ToString());
        }

        public string RetryDownload(string id, SabnzbdSettings settings)
        {
            var request = BuildRequest("retry", settings);
            request.AddQueryParam("value", id);

            SabnzbdRetryResponse response;

            if (!Json.TryDeserialize<SabnzbdRetryResponse>(ProcessRequest(request, settings), out response))
            {
                response = new SabnzbdRetryResponse();
                response.Status = true;
            }

            return response.Id;
        }

        private HttpRequestBuilder BuildRequest(string mode, SabnzbdSettings settings)
        {
            var baseUrl = GetBaseUrl(settings, "api");

            var requestBuilder = new HttpRequestBuilder(baseUrl)
                .Accept(HttpAccept.Json)
                .AddQueryParam("mode", mode);

            requestBuilder.LogResponseContent = true;

            if (settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddSuffixQueryParam("apikey", settings.ApiKey);
            }
            else
            {
                requestBuilder.AddSuffixQueryParam("ma_username", settings.Username);
                requestBuilder.AddSuffixQueryParam("ma_password", settings.Password);
            }

            requestBuilder.AddSuffixQueryParam("output", "json");

            return requestBuilder;
        }

        private string ProcessRequest(HttpRequestBuilder requestBuilder, SabnzbdSettings settings)
        {
            var httpRequest = requestBuilder.Build();

            HttpResponse response;

            _logger.Debug("Url: {0}", httpRequest.Url);

            try
            {
                response = _httpClient.Execute(httpRequest);
            }
            catch (HttpException ex)
            {
                throw new DownloadClientException("Unable to connect to SABnzbd, {0}", ex, ex.Message);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.TrustFailure)
                {
                    throw new DownloadClientUnavailableException("Unable to connect to SABnzbd, certificate validation failed.", ex);
                }

                throw new DownloadClientUnavailableException("Unable to connect to SABnzbd, {0}", ex, ex.Message);
            }

            CheckForError(response);

            return response.Content;
        }

        private void CheckForError(HttpResponse response)
        {
            SabnzbdJsonError result;

            if (!Json.TryDeserialize<SabnzbdJsonError>(response.Content, out result))
            {
                //Handle plain text responses from SAB
                result = new SabnzbdJsonError();

                if (response.Content.StartsWith("error", StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Status = "false";
                    result.Error = response.Content.Replace("error: ", "");
                }
                else
                {
                    result.Status = "true";
                }

                result.Error = response.Content.Replace("error: ", "");
            }

            if (result.Failed)
            {
                throw new DownloadClientException("Error response received from SABnzbd: {0}", result.Error);
            }
        }
    }
}
