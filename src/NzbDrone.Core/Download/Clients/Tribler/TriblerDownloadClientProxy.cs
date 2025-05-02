using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Indexers.Tribler;

namespace NzbDrone.Core.Download.Clients.Tribler
{
    public interface ITriblerDownloadClientProxy
    {
        ICollection<Download> GetDownloads(TriblerDownloadSettings settings);
        ICollection<File> GetDownloadFiles(TriblerDownloadSettings settings, Download downloadItem);
        GetTriblerSettingsResponse GetConfig(TriblerDownloadSettings settings);
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

        private HttpRequestBuilder getRequestBuilder(TriblerDownloadSettings settings, string relativePath = null)
        {
            var requestBuilder = new HttpRequestBuilder(GetBaseUrl(settings, relativePath))
                .Accept(HttpAccept.Json);

            requestBuilder.Headers.Add("X-Api-Key", settings.ApiKey);

            requestBuilder.LogResponseContent = true;

            return requestBuilder;
        }

        public string GetBaseUrl(TriblerDownloadSettings settings, string relativePath = null)
        {
            var baseUrl = HttpRequestBuilder.BuildBaseUrl(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase);
            baseUrl = HttpUri.CombinePath(baseUrl, relativePath);

            return baseUrl;
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

        public GetTriblerSettingsResponse GetConfig(TriblerDownloadSettings settings)
        {
            var configRequest = getRequestBuilder(settings, "api/settings");
            return ProcessRequest<GetTriblerSettingsResponse>(configRequest);
        }

        public ICollection<File> GetDownloadFiles(TriblerDownloadSettings settings, Download downloadItem)
        {
            var filesRequest = getRequestBuilder(settings, "api/downloads/" + downloadItem.Infohash + "/files");
            return ProcessRequest<GetFilesResponse>(filesRequest).Files;
        }

        public ICollection<Download> GetDownloads(TriblerDownloadSettings settings)
        {
            var downloadRequest = getRequestBuilder(settings, "api/downloads");
            var downloads = ProcessRequest<DownloadsResponse>(downloadRequest);
            return downloads.Downloads;
        }

        public void RemoveDownload(TriblerDownloadSettings settings, DownloadClientItem item, bool deleteData)
        {
            var deleteDownloadRequestObject = new RemoveDownloadRequest
            {
                RemoveData = deleteData
            };

            var deleteRequestBuilder = getRequestBuilder(settings, "api/downloads/" + item.DownloadId.ToLower());
            deleteRequestBuilder.Method = System.Net.Http.HttpMethod.Delete;

            var deleteRequest = deleteRequestBuilder.Build();
            deleteRequest.SetContent(Json.ToJson(deleteDownloadRequestObject));

            ProcessRequest<DeleteDownloadResponse>(deleteRequest);
        }

        public string AddFromMagnetLink(TriblerDownloadSettings settings, AddDownloadRequest downloadRequest)
        {
            var addDownloadRequestBuilder = getRequestBuilder(settings, "api/downloads");
            addDownloadRequestBuilder.Method = System.Net.Http.HttpMethod.Put;

            var addDownloadRequest = addDownloadRequestBuilder.Build();
            addDownloadRequest.SetContent(Json.ToJson(downloadRequest));

            return ProcessRequest<AddDownloadResponse>(addDownloadRequest).Infohash;
        }
    }
}
