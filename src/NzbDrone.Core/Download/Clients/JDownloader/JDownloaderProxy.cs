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


        #region Hidden
        //public JDownloaderSubscribeResponse Subscribe(JDownloaderSettings settings)
        //{
        //    return Subscribe(subscribe, settings);
        //}

        //public JDownloaderSubscribeResponse Subscribe(IEnumerable<string> subscribeTo, JDownloaderSettings settings)
        //{
        //    var request = new RestRequest("events/subscribe");

        //    request.AddParameter("", string.Join(",", subscribeTo.Select(a => "\"" + a + "\"")))
        //           .AddParameter("", "");

        //    var response = ProcessRequest(request, settings);

        //    return JObject.Parse(response)["data"].ToObject<JDownloaderSubscribeResponse>();
        //}
        //public async Task<JObject> AddLinks(IEnumerable<string> links, string package)
        //{
        //    //http://localhost:3128/linkgrabberv2/addLinks?{%22links%22:%22http://uploaded.net/file/vdugb2lw%20http://www.filestube.to/5OtlmDPstDFTFpUMb4qnHK%22,%22packageName%22:%22Test%22,%22autostart%22:false}

        //    var request = new RestRequest("linkgrabberv2/addLinks", Method.POST);
        //    string links_str = string.Join(" ", links);
        //    request.AddParameter("",
        //        @"{""links"":""" + links_str + @""",""packageName"":""" + package + @""",""autostart"":false}");

        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    return JObject.Parse(response.Content);
        //}

        //public async Task<List<JDownloderLinkCheckResponse>> AddLinksAndWait(params string[] ids)
        //{
        //    if (!ids.Any()) return null;

        //    string package = CreateUniquePackageName();

        //    await AddLinks(ids, package);

        //    List<JDownloderLinkCheckResponse> check = null;
        //    int i = 0;
        //    while (i*100 < 5000 &&
        //           (check == null || check.Count < ids.Length))
        //    {
        //        if (i != 0) await Task.Delay(100);

        //        await WaitIdle();
        //        check = await QueryLinks(package);

        //        i++;
        //    }
        //    if (check != null)
        //    {
        //        IEnumerable<JDownloderLinkCheckResponse> toDelete = check.Where(a => a.availability != "ONLINE");
        //        await DeleteLinks(toDelete.Select(a => a.uuid));
        //        return check.Where(a => a.availability == "ONLINE").ToList();
        //    }
        //    return null;
        //}

        //private string CreateUniquePackageName()
        //{
        //    lock (Lock)
        //    {
        //        JDownloaderQueryPackageResponse packages = QueryPackagesSync();
        //        int packageId = 1;
        //        if (packages != null)
        //        {
        //            string name;
        //            do
        //            {
        //                name = packageDownload + " (" + packageId + ")";
        //                packageId++;
        //            } while (packages.data.Any(a => a.name == name));
        //            return name;
        //        }
        //        return packageDownload + " (1)";
        //    }
        //}


        //public async Task WaitIdle()
        //{
        //    while ((await GetState()).data.FirstOrDefault(a => a.eventName == "linkGrabberState").eventData.data !=
        //           "IDLE") await Task.Delay(100);
        //}

        //private async Task<JDownloaderQueryPackageResponse> QueryPackages()
        //{
        //    var request = new RestRequest("linkgrabberv2/queryPackages");
        //    request.AddParameter("", @"{}");
        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    return Json.Deserialize<JDownloaderQueryPackageResponse>(response.Content);
        //}

        //private JDownloaderQueryPackageResponse QueryPackagesSync()
        //{
        //    var request = new RestRequest("linkgrabberv2/queryPackages");
        //    request.AddParameter("", @"{}");
        //    IRestResponse response = client.Execute(request);
        //    return Json.Deserialize<JDownloaderQueryPackageResponse>(response.Content);
        //}

        //private async Task<JDownloaderQueryPackageResponse> QueryDownloadPackages()
        //{
        //    var request = new RestRequest("downloadsV2/queryPackages");
        //    request.AddParameter("", @"{}");
        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    return Json.Deserialize<JDownloaderQueryPackageResponse>(response.Content);
        //}

        //public async Task<List<JDownloaderQueryPackageItem>> QueryPackages(params string[] packages)
        //{
        //    IEnumerable<JDownloaderQueryPackageItem> p = (await QueryPackages()).data.Where(a => packages.Contains(a.name));
        //    return p.ToList();
        //}

        //public async Task<List<JDownloaderQueryPackageItem>> QueryDownloadPackages(params string[] packages)
        //{
        //    IEnumerable<JDownloaderQueryPackageItem> p =
        //        (await QueryDownloadPackages()).data.Where(a => packages.Contains(a.name));
        //    return p.ToList();
        //}

        //public Task<List<JDownloaderQueryPackageItem>> QueryDefaultDownloadPackages()
        //{
        //    return QueryDownloadPackages(packageDownload);
        //}

        //internal async Task<List<JDownloderLinkCheckResponse>> QueryDownloadLinks()
        //{
        //    List<JDownloaderQueryPackageItem> p = await QueryDefaultDownloadPackages();
        //    if (p != null) return await QueryDownloadLinks(p.Select(a => a.uuid));
        //    return null;
        //}

        //internal Task<List<JDownloderLinkCheckResponse>> QueryDownloadLinks(params long[] ids)
        //{
        //    return QueryDownloadLinks(ids.ToList());
        //}

        //public async Task<List<JDownloderLinkCheckResponse>> QueryDownloadLinks(IEnumerable<long> packageUuid)
        //{
        //    var request = new RestRequest("downloadsV2/queryLinks");

        //    request.AddParameter("",
        //        @"{""packageUUIDs"":[" + string.Join(",", packageUuid) + @"]" + queryLinksParameters + "}");
        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    return JObject.Parse(response.Content)["data"].ToObject<List<JDownloderLinkCheckResponse>>();
        //}

        //internal Task<List<JDownloderLinkCheckResponse>> QueryLinks()
        //{
        //    return QueryLinks(packageDownload, packageOffline, packageVerschiedene, packageVarious);
        //}

        //internal Task<List<JDownloderLinkCheckResponse>> QueryLinks(string package)
        //{
        //    return QueryLinks(packageDownload, packageOffline, packageVerschiedene, packageVarious, package);
        //}

        //internal async Task<List<JDownloderLinkCheckResponse>> QueryLinks(params string[] packages)
        //{
        //    List<JDownloaderQueryPackageItem> p = await QueryPackages(packages);
        //    if (p != null) return await QueryLinks(p.Select(a => a.uuid));
        //    return null;
        //}

        //public async Task<List<JDownloderLinkCheckResponse>> QueryLinks(IEnumerable<long> packageUuid)
        //{
        //    //http://localhost:3128/linkcollector/queryLinks?{%22packageUUIDs%22:[1393787299279],%22size%22:true,%22host%22:true,%22availability%22:true,%22url%22:true}

        //    //http://localhost:3128/linkgrabberv2/queryLinks?{%22size%22:true,%22host%22:true,%22availability%22:true,%22variants%22:true,%22url%22:true}

        //    var ids = packageUuid as IList<long> ?? packageUuid.ToList();

        //    if (!ids.Any()) return null;

        //    var request = new RestRequest("linkgrabberv2/queryLinks");
        //    request.AddParameter("",
        //        @"{""packageUUIDs"":[" + string.Join(",", ids) +
        //        @"]" + queryLinksParameters + "}");
        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    string text = response.Content;
        //    return JObject.Parse(text)["data"].ToObject<List<JDownloderLinkCheckResponse>>();
        //}

        //public Task<JObject> DownloadLink(LinkDescription selected, IEnumerable<LinkDescription> delete,
        //    bool deletePackage = false)
        //{
        //    return DownloadLinks(new List<LinkDescription> {selected}, delete, deletePackage);
        //}

        //public async Task<JObject> DownloadLinks(IEnumerable<LinkDescription> list, IEnumerable<LinkDescription> delete,
        //    bool deletePackage = false)
        //{
        //    IList<LinkDescription> downloadLinkDtos = list as IList<LinkDescription> ?? list.ToList();
        //    LinkDescription first = downloadLinkDtos.FirstOrDefault();
        //    delete = delete ?? new List<LinkDescription>();
        //    if (first == null) await DeleteLinks(delete.ToArray());
        //    else
        //    {
        //        IEnumerable<LinkDescription> ids = delete.Where(a => !downloadLinkDtos.Contains(a));
        //        await DeleteLinks(ids.ToArray());
        //        await RenamePackages(downloadLinkDtos.Select(a => a.PackageId), packageDownload);
        //        await DownloadLinksInternal(downloadLinkDtos);
        //        foreach (LinkDescription d in downloadLinkDtos)
        //        {
        //            if (d.Episode != null) d.Episode.DownloadId = d.Id;
        //        }
        //        if (deletePackage) await DeletePackages();
        //        return await Start();
        //    }
        //    return null;
        //}

        //private async Task RenamePackages(IEnumerable<long> packageIds, string package)
        //{
        //    packageIds = packageIds.Distinct();

        //    foreach (long id in packageIds) await RenamePackages(id, package);
        //}

        //private async Task<JObject> RenamePackages(long id, string package)
        //{
        //    var request = new RestRequest("linkgrabberv2/renamePackage");
        //    request.AddParameter("", id.ToString())
        //           .AddParameter("", "\"" + package + "\"");
        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    return JObject.Parse(response.Content);
        //}

        //private async Task<JObject> DownloadLinksInternal(IEnumerable<LinkDescription> list)
        //{
        //    string ids = string.Join(",", list.Select(a => a.Id));
        //    var request = new RestRequest("linkgrabberv2/moveToDownloadlist");
        //    request.AddParameter("", "[" + ids + "]")
        //           .AddParameter("", "[]");
        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    return JObject.Parse(response.Content);
        //}

        //public async Task<JDownloaderGetDownloadStateResponse> GetDownloadState()
        //{
        //    var request = new RestRequest("downloads/getJDState");
        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    return Json.Deserialize<JDownloaderGetDownloadStateResponse>(response.Content);
        //}

        //private async Task<JObject> Start()
        //{
        //    //http://localhost:3128/downloads/start
        //    var request = new RestRequest("downloads/start");
        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    DownloadChecker.Start();
        //    return JObject.Parse(response.Content);
        //}

        //private async Task<JObject> DownloadPackage()
        //{
        //    List<JDownloaderQueryPackageItem> p = await QueryPackages(packageDownload);
        //    if (p == null) return null;

        //    var request = new RestRequest("linkgrabberv2/moveToDownloadlist");
        //    string ids = string.Join(",", p.Select(a => a.uuid));
        //    request.AddParameter("", "[]")
        //           .AddParameter("", "[" + ids + "]");
        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    return JObject.Parse(response.Content);
        //}

        //public async Task<JObject> DeletePackages()
        //{
        //    List<JDownloaderQueryPackageItem> p = await QueryPackages(packageDownload);
        //    if (p != null) return await DeletePackages(p.Select(a => a.uuid));
        //    return null;
        //}

        //private async Task<JObject> DeletePackages(IEnumerable<long> packages)
        //{
        //    //http://localhost:3128/linkcollector/removeLinks?[]&[1393787299804]

        //    var request = new RestRequest("linkcollector/removeLinks");
        //    string ids = string.Join(",", packages);
        //    request.AddParameter("", "[]")
        //           .AddParameter("", @"[" + ids + "]");
        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    return JObject.Parse(response.Content);
        //}

        //public Task<JObject> DeleteLinks(params LinkDescription[] ids)
        //{
        //    return DeleteLinks(ids.Where(a => a != null).Select(a => a.Id));
        //}

        //public async Task<JObject> DeleteLinks(IEnumerable<long> ids)
        //{
        //    IList<long> values = ids as IList<long> ?? ids.ToList();
        //    if (values.Any())
        //    {
        //        string idsString = string.Join(",", values);
        //        Debug.WriteLine("Delete: " + idsString);
        //        IRestRequest request = new RestRequest("linkgrabberv2/removeLinks")
        //            .AddParameter("", "[" + idsString + "]")
        //            .AddParameter("", "");
        //        IRestResponse response = await client.ExecuteTaskAsync(request);
        //        return JObject.Parse(response.Content);
        //    }
        //    return null;
        //}

        //public async Task<JObject> DeleteDownloadLinks(params long[] ids)
        //{
        //    string idsString = string.Join(",", ids.Select(a => a.ToString()));
        //    IRestRequest request = new RestRequest("downloads/removeLinks")
        //        .AddParameter("", "[" + idsString + "]");

        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    return JObject.Parse(response.Content);
        //}

        //public async Task<JDownloaderDownloadSpeedResponse> GetDownloadSpeed()
        //{
        //    var request = new RestRequest("downloads/speed");
        //    IRestResponse response = await client.ExecuteTaskAsync(request);
        //    return Json.Deserialize<JDownloaderDownloadSpeedResponse>(response.Content);
        //}
        #endregion

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
