using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Clients.AirDCPP;
using NzbDrone.Core.Indexers.AirDCPP.Requests;
using NzbDrone.Core.Indexers.AirDCPP.Responses;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Indexers.AirDCPP
{
    public class AirDCPPProxy : IAirDCPPProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public AirDCPPProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public AirDCPPSettings Settings { get; set; }

        public HttpRequest PerformSearch(AirDCPPSettings settings, string searchTerm)
        {
            Settings = settings;

            var searchInstanceId = CreateSearchInstance();
            _logger.Trace($"Obtained search instance ID from AirDCPP: {searchInstanceId}");

            // strip illegal file/path characters from search term since we search by directory on AirDCPP
            var legalSearchTerm = RemoveInvalidChars(searchTerm);
            PerformHubSearch(searchInstanceId, legalSearchTerm);

            var searchResultsRequest = BuildRequest().Resource($"search/{searchInstanceId}/results/0/1000");

            if (settings?.Delay > 0)
            {
                Delay(settings.Delay);
            }

            return searchResultsRequest.Build();
        }

        public string DownloadBySearchInstanceAndResultId(AirDCPPClientSettings settings, string id, string title)
        {
            var splitResult = id.Split(':');
            var searchInstanceId = splitResult[0];
            var resultId = splitResult[1];

            var downloadRequest = BuildRequest(settings).Resource($"search/{searchInstanceId}/results/{resultId}/download").Post().Build();

            var query = new HubDownloadQuery
            {
                target_directory = settings.DownloadDirectory
            };

            downloadRequest.SetContent(query.ToJson());

            // first start the download
            ProcessRequest<DownloadResultResponse>(downloadRequest);

            // next, search for the id of the bundle that has now been added to the queue
            string downloadBundleId = string.Empty;

            while (string.IsNullOrEmpty(downloadBundleId))
            {
                var queueResults = GetQueueHistory(settings);
                downloadBundleId = queueResults.Where(result => result.name == title).FirstOrDefault()?.id.ToString();
                Delay(1000);
            }

            return downloadBundleId;
        }

        public List<QueueResult> GetQueueHistory(AirDCPPClientSettings settings)
        {
            var queueRequest = BuildRequest(settings).Resource($"queue/bundles/0/1000").Build();

            return ProcessRequest<List<QueueResult>>(queueRequest);
        }

        public void DeleteItemFromQueueHistory(AirDCPPClientSettings settings, string bundleId, bool deleteData)
        {
            var deleteRequest = BuildRequest(settings).Resource($"queue/bundles/{bundleId}/remove").Build();

            deleteRequest.Method = HttpMethod.POST;

            var query = new RemoveBundleQuery
            {
                remove_finished = deleteData
            };

            deleteRequest.SetContent(query.ToJson());

            ProcessRequest(deleteRequest);
        }

        private string RemoveInvalidChars(string filename)
        {
            return Regex.Replace(filename, @"[^0-9A-Za-z ,]", "");
        }

        private bool Delay(int millisecond)
        {
            var sw = new Stopwatch();
            sw.Start();
            bool flag = false;
            while (!flag)
            {
                if (sw.ElapsedMilliseconds > millisecond)
                {
                    flag = true;
                }
            }

            sw.Stop();
            return true;
        }

        private int CreateSearchInstance()
        {
            var searchInstanceRequest = BuildRequest().Resource("search").Post().Build();

            var result = ProcessRequest<SearchResponse>(searchInstanceRequest);
            return result.id;
        }

        private void PerformHubSearch(int searchInstanceId, string searchTerm)
        {
            _logger.Debug($"Performing hub search for {searchTerm}");
            var searchRequest = BuildRequest().Resource($"search/{searchInstanceId}/hub_search").Post().Build();

            var query = new CustomInfo
            {
                query = new QueryInfo
                {
                    pattern = searchTerm,
                    file_type = "video"
                }
            };

            searchRequest.SetContent(query.ToJson());

            var response = ProcessRequest<SearchHubResponse>(searchRequest);

            if (response.queue_time > 0)
            {
                _logger.Debug($"AirDCPP queue time was > 0: waiting for {response.queue_time}ms");
                Delay(response.queue_time);
            }
        }

        private HttpRequestBuilder BuildRequest()
        {
            var urlBase = (Settings.BaseUrl.IsNotNullOrWhiteSpace() ? $"{Settings.BaseUrl}/" : string.Empty) + "api/v1";
            var requestBuilder = new HttpRequestBuilder(Settings.UseSsl, Settings.Host, Settings.Port, urlBase)
            {
                LogResponseContent = true,
                NetworkCredential = new NetworkCredential(Settings.Username, Settings.Password)
            };
            requestBuilder.SetHeader("Content-Type", "application/json");

            return requestBuilder;
        }

        private HttpRequestBuilder BuildRequest(AirDCPPClientSettings settings)
        {
            var urlBase = (settings.UrlBase.IsNotNullOrWhiteSpace() ? $"{settings.UrlBase}/" : string.Empty) + "api/v1";
            var requestBuilder = new HttpRequestBuilder(settings.UseSsl, settings.Host, settings.Port, urlBase)
            {
                LogResponseContent = true,
                NetworkCredential = new NetworkCredential(settings.Username, settings.Password)
            };
            requestBuilder.SetHeader("Content-Type", "application/json");

            return requestBuilder;
        }

        private void ProcessRequest(HttpRequest request)
        {
            try
            {
                var response = _httpClient.Execute(request);

                if (response.StatusCode != HttpStatusCode.NoContent && response.StatusCode != HttpStatusCode.OK)
                {
                    throw new DownloadClientException($"Unable to process Request with Status Code: {response.StatusCode}");
                }
            }
            catch (HttpException ex)
            {
                throw new DownloadClientException("Unable to connect to AirDCPP, please check your settings", ex);
            }
            catch (WebException ex)
            {
                throw new DownloadClientException("Unable to connect to AirDCPP, please check your settings", ex);
            }
        }

        private T ProcessRequest<T>(HttpRequest request)
            where T : new()
        {
            HttpResponse response = null;
            try
            {
                response = _httpClient.Execute(request);
                return Json.Deserialize<T>(response.Content);
            }
            catch (JsonException ex)
            {
                throw new DownloadClientException("AirDCPP response could not be processed {0}: {1}", ex.Message, response.Content);
            }
            catch (HttpException ex)
            {
                throw new DownloadClientException("Unable to connect to AirDCPP, please check your settings", ex);
            }
            catch (WebException ex)
            {
                throw new DownloadClientException("Unable to connect to AirDCPP, please check your settings", ex);
            }
        }
    }
}