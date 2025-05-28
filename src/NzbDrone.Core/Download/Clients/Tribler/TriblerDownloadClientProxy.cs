using System.Collections.Generic;
using System.Net.Http;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Indexers.Tribler;

namespace NzbDrone.Core.Download.Clients.Tribler
{
    public interface ITriblerDownloadClientProxy
    {
        List<Download> GetDownloads(TriblerDownloadSettings settings);
        List<File> GetDownloadFiles(TriblerDownloadSettings settings, Download downloadItem);
        TriblerSettingsResponse GetConfig(TriblerDownloadSettings settings);
        void RemoveDownload(TriblerDownloadSettings settings, DownloadClientItem item, bool deleteData);
        string AddFromMagnetLink(TriblerDownloadSettings settings, AddDownloadRequest downloadRequest);
    }

    public class TriblerDownloadClientProxy : ITriblerDownloadClientProxy
    {
        protected readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public TriblerDownloadClientProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private HttpRequestBuilder GetRequestBuilder(TriblerDownloadSettings settings, string relativePath = null)
        {
            var baseUrl = HttpRequestBuilder.BuildBaseUrl(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase);
            baseUrl = HttpUri.CombinePath(baseUrl, relativePath);

            var requestBuilder = new HttpRequestBuilder(baseUrl)
                .Accept(HttpAccept.Json);

            requestBuilder.Headers.Add("X-Api-Key", settings.ApiKey);
            requestBuilder.LogResponseContent = true;

            return requestBuilder;
        }

        private T ProcessRequest<T>(HttpRequestBuilder requestBuilder)
            where T : new()
        {
            return ProcessRequest<T>(requestBuilder.Build());
        }

        private T ProcessRequest<T>(HttpRequest requestBuilder)
            where T : new()
        {
            var httpRequest = requestBuilder;

            _logger.Debug("Url: {0}", httpRequest.Url);

            try
            {
                var response = _httpClient.Execute(httpRequest);
                return Json.Deserialize<T>(response.Content);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new DownloadClientAuthenticationException("Unauthorized - AuthToken is invalid", ex);
                }

                throw new DownloadClientUnavailableException("Unable to connect to Tribler. Status Code: {0}", ex.Response.StatusCode, ex);
            }
        }

        public TriblerSettingsResponse GetConfig(TriblerDownloadSettings settings)
        {
            var configRequest = GetRequestBuilder(settings, "api/settings");
            return ProcessRequest<TriblerSettingsResponse>(configRequest);
        }

        public List<File> GetDownloadFiles(TriblerDownloadSettings settings, Download downloadItem)
        {
            var filesRequest = GetRequestBuilder(settings, "api/downloads/" + downloadItem.Infohash + "/files");
            return ProcessRequest<GetFilesResponse>(filesRequest).Files;
        }

        public List<Download> GetDownloads(TriblerDownloadSettings settings)
        {
            var downloadRequest = GetRequestBuilder(settings, "api/downloads");
            var downloads = ProcessRequest<DownloadsResponse>(downloadRequest);
            return downloads.Downloads;
        }

        public void RemoveDownload(TriblerDownloadSettings settings, DownloadClientItem item, bool deleteData)
        {
            var deleteDownloadRequestObject = new RemoveDownloadRequest
            {
                RemoveData = deleteData
            };

            var deleteRequestBuilder = GetRequestBuilder(settings, "api/downloads/" + item.DownloadId.ToLower());
            deleteRequestBuilder.Method = HttpMethod.Delete;

            var deleteRequest = deleteRequestBuilder.Build();
            deleteRequest.SetContent(Json.ToJson(deleteDownloadRequestObject));

            ProcessRequest<DeleteDownloadResponse>(deleteRequest);
        }

        public string AddFromMagnetLink(TriblerDownloadSettings settings, AddDownloadRequest downloadRequest)
        {
            var addDownloadRequestBuilder = GetRequestBuilder(settings, "api/downloads");
            addDownloadRequestBuilder.Method = HttpMethod.Put;

            var addDownloadRequest = addDownloadRequestBuilder.Build();
            addDownloadRequest.SetContent(Json.ToJson(downloadRequest));

            return ProcessRequest<AddDownloadResponse>(addDownloadRequest).Infohash;
        }
    }
}
