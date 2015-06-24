using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Download.Clients.JDownloader
{

    public interface IJDownloaderProxy
    {
        int GetVersion(JDownloaderSettings settings);
        List<JDownloaderLinkCheckResponse> CheckLinks(IList<string> releases, JDownloaderSettings settings);
        string Download(string package, string downloadUrl, int priority, JDownloaderSettings settings);

        void RemoveDownload(string downloadId, bool deleteData, JDownloaderSettings settings);

        JDownloaderQueryPackageResponse QueryAllDownloadPackages(JDownloaderSettings settings);
    }
    public class JDownloaderProxy : IJDownloaderProxy
    {
        private readonly Logger _logger;


        public JDownloaderProxy(Logger logger)
        {
            this._logger = logger;
        }


        public int GetVersion(JDownloaderSettings settings)
        {
            var request = new RestRequest("jd/version");
            var response = ProcessRequest(request, settings);
            return Convert.ToInt32(JObject.Parse(response)["data"]);
        }
        public List<JDownloaderLinkCheckResponse> CheckLinks(IList<string> downloadUrls, JDownloaderSettings settings)
        {
            if (downloadUrls.Any(a => a.Contains("?")))
                _logger.Warn("Download URL's can not contain a '?' when checking links with JDownloader. They will be ignored.");

            string package = Guid.NewGuid().ToString();
            AddLinks(package, downloadUrls, settings);

            var startTime = DateTime.Now;
            JDownloaderQueryPackageItem packageItem = null;
            while ((DateTime.Now - startTime).TotalSeconds <= settings.LinkCheckerTimeout)
            {
                packageItem = QueryPackage(package, settings).FirstOrDefault();
                Thread.Sleep(500);

                if (packageItem != null && packageItem.childCount == downloadUrls.Count)
                    break;
            }
            if (packageItem != null)
            {
                var result = QueryLinks(packageItem.uuid, settings);
                if (packageItem.childCount != downloadUrls.Count)
                {
                    RemovePackage(packageItem.uuid, settings);
                    WaitAndRemove(packageItem.uuid, settings);
                }
                else
                {
                    RemovePackage(packageItem.uuid, settings);
                }
                return result;
            }
            return new List<JDownloaderLinkCheckResponse>();
        }

        private void WaitAndRemove(long packageId, JDownloaderSettings settings)
        {
            Task.Factory.StartNew(() =>
                                  {
                                      WaitWhileIdle(settings);
                                      RemovePackage(packageId, settings);
                                  });
        }

        public void WaitWhileIdle(JDownloaderSettings settings)
        {
            var jDownloaderEvent = (GetState(settings)).data.FirstOrDefault(a => a.eventName == "linkGrabberState");
            while (jDownloaderEvent != null &&
                   jDownloaderEvent.eventData.data != "IDLE")
            {
                Thread.Sleep(500);
                jDownloaderEvent = (GetState(settings)).data.FirstOrDefault(a => a.eventName == "linkGrabberState");
            }
        }

        private void AddLinks(string packageName, IEnumerable<string> links, JDownloaderSettings settings, bool autostart = false, string priority = "DEFAULT")
        {
            var requestContent = new
            {
                autostart,
                links = string.Join(" ", links.Where(a => !a.Contains("?"))),
                packageName,
                //priority
            };
            var request = BuildPostRequest("linkgrabberv2/addLinks", requestContent);
            var response = ProcessRequest(request, settings);
        }
        public string Download(string package, string downloadUrl, int priority, JDownloaderSettings settings)
        {
            AddLinks(package, new List<string>() { downloadUrl }, settings, false, ((JDownloaderPriority)priority).ToString().ToUpper());
            WaitWhileIdle(settings);

            var packageId = DownloadPackage(package, settings);

            while (packageId == null)
            {
                Thread.Sleep(500);
                packageId = DownloadPackage(package, settings);
            }
            Start(settings);
            return packageId;
        }
        private void Start(JDownloaderSettings settings)
        {
            var request = new RestRequest("downloads/start");
            var response = ProcessRequest(request, settings);
        }
        private string DownloadPackage(string package, JDownloaderSettings settings)
        {
            JDownloaderQueryPackageItem p = QueryPackage(package, settings).FirstOrDefault();
            if (p == null) return null;

            var request = new RestRequest("linkgrabberv2/moveToDownloadlist");
            request.AddParameter("", "[]")
                   .AddParameter("", "[" + p.uuid + "]");

            var response = ProcessRequest(request, settings);
            return p.uuid.ToString();
        }

        public void RemoveDownload(string downloadId, bool deleteData, JDownloaderSettings settings)
        {
            IRestRequest request = new RestRequest("downloadsV2/removeLinks")
                .AddParameter("", "")
                .AddParameter("", "[" + downloadId + "]");

            var response = ProcessRequest(request, settings);
        }


        public List<JDownloaderQueryPackageItem> QueryPackage(string package, JDownloaderSettings settings)
        {
            IEnumerable<JDownloaderQueryPackageItem> p = QueryAllPackages(settings).data.Where(a => package == a.name);
            return p.ToList();
        }

        private JDownloaderQueryPackageResponse QueryAllPackages(JDownloaderSettings settings)
        {
            var request = BuildPostRequest("linkgrabberv2/queryPackages", new
            {
                bytesTotal = true,
                comment = true,
                status = true,
                enabled = true,
                maxResults = -1,
                startAt = 0,
                childCount = true,
                hosts = true,
                saveTo = true,
                availableOfflineCount = true,
                availableOnlineCount = true,
                availableTempUnknownCount = true,
                availableUnknownCount = true
            });
            var response = ProcessRequest(request, settings);
            return Json.Deserialize<JDownloaderQueryPackageResponse>(response);
        }

        public List<JDownloaderQueryPackageItem> QueryDownloadPackage(string package, JDownloaderSettings settings)
        {
            IEnumerable<JDownloaderQueryPackageItem> p = QueryAllDownloadPackages(settings).data.Where(a => package == a.name);
            return p.ToList();
        }
        public JDownloaderQueryPackageResponse QueryAllDownloadPackages(JDownloaderSettings settings)
        {
            var request = BuildPostRequest("downloadsV2/queryPackages", new
            {
                bytesTotal = true,
                comment = true,
                status = true,
                enabled = true,
                maxResults = -1,
                startAt = 0,
                childCount = true,
                hosts = true,
                saveTo = true,
                availableOfflineCount = true,
                availableOnlineCount = true,
                availableTempUnknownCount = true,
                availableUnknownCount = true,
                speed = true,
                finished = true,
                eta = true,
                bytesLoaded = true,
                running = true
            });
            var response = ProcessRequest(request, settings);
            return Json.Deserialize<JDownloaderQueryPackageResponse>(response);
        }

        public List<JDownloaderLinkCheckResponse> QueryLinks(long packageUuid, JDownloaderSettings settings)
        {
            var request = BuildPostRequest("linkgrabberv2/queryLinks", new
            {
                packageUUIDs = new []{packageUuid},
                bytesTotal = true,
                comment = true,
                status = true,
                enabled = true,
                maxResults = -1,
                startAt = 0,
                host = true,
                url = true,
                availability = true,
                variantIcon = true,
                variantName = true,
                variantID = true,
                variants = true,
                priority = true
            });
            var response = ProcessRequest(request, settings);
            return JObject.Parse(response)["data"].ToObject<List<JDownloaderLinkCheckResponse>>();
        }
        private void RemovePackage(long uuid, JDownloaderSettings settings)
        {
            var request = new RestRequest("linkcollector/removeLinks");
            request.AddParameter("", "[]")
                   .AddParameter("", @"[" + uuid + "]");
            var response = ProcessRequest(request, settings);
        }

        public JDownloaderGetStateResponse GetState(JDownloaderSettings settings)
        {
            var request = new RestRequest("polling/poll");
            request.AddParameter("", @"{""linkGrabberState"":true}");
            var response = ProcessRequest(request, settings);
            return Json.Deserialize<JDownloaderGetStateResponse>(response);
        }

        private string ProcessRequest(IRestRequest restRequest, JDownloaderSettings settings)
        {
            var client = BuildClient(settings);
            var response = client.Execute(restRequest);
            //_logger.Trace("Response: {0}", response.Content);

            CheckForError(response);

            return response.Content;
        }
        private IRestRequest BuildPostRequest(string resource, object parameter)
        {
            var request = new RestRequest(resource, Method.POST);
            request.AddParameter("", JObject.FromObject(parameter).ToString());
            return request;
        }
        private IRestClient BuildClient(JDownloaderSettings settings)
        {
            var protocol = "http";

            var url = String.Format("{0}://{1}:{2}",
                                 protocol,
                                 settings.Host,
                                 settings.Port);

            //_logger.Debug("Url: " + url);

            var client = RestClientFactory.BuildClient(url);
            return client;
        }

        private void CheckForError(IRestResponse response)
        {
            if (response.ErrorException != null)
            {
                throw new DownloadClientException("Unable to connect to JDownloader. " + response.ErrorException.Message, response.ErrorException);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new DownloadClientException("Authentication failed for JDownloader, please check your settings", response.ErrorException);
            }

            //var result = Json.Deserialize<JDownloaderError>(response.Content);

            //if (result.type != null)
            //    throw new DownloadClientException("Error response received from JDownloader: {0}, {1}", result.type.ToString(), result.data);
        }








    }
}
