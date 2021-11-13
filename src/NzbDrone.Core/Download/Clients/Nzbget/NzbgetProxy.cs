using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public interface INzbgetProxy
    {
        string GetBaseUrl(NzbgetSettings settings, string relativePath = null);
        string DownloadNzb(byte[] nzbData, string title, string category, int priority, bool addpaused, NzbgetSettings settings);
        NzbgetGlobalStatus GetGlobalStatus(NzbgetSettings settings);
        List<NzbgetQueueItem> GetQueue(NzbgetSettings settings);
        List<NzbgetHistoryItem> GetHistory(NzbgetSettings settings);
        string GetVersion(NzbgetSettings settings);
        Dictionary<string, string> GetConfig(NzbgetSettings settings);
        void RemoveItem(string id, NzbgetSettings settings);
        void RetryDownload(string id, NzbgetSettings settings);
    }

    public class NzbgetProxy : INzbgetProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private readonly ICached<string> _versionCache;

        public NzbgetProxy(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _versionCache = cacheManager.GetCache<string>(GetType(), "versions");
        }

        public string GetBaseUrl(NzbgetSettings settings, string relativePath = null)
        {
            var baseUrl = HttpRequestBuilder.BuildBaseUrl(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase);
            baseUrl = HttpUri.CombinePath(baseUrl, relativePath);

            return baseUrl;
        }

        private bool HasVersion(int minimumVersion, NzbgetSettings settings)
        {
            var versionString = _versionCache.Find(GetBaseUrl(settings)) ?? GetVersion(settings);

            var version = int.Parse(versionString.Split(new[] { '.', '-' })[0]);

            return version >= minimumVersion;
        }

        public string DownloadNzb(byte[] nzbData, string title, string category, int priority, bool addpaused, NzbgetSettings settings)
        {
            if (HasVersion(16, settings))
            {
                var droneId = Guid.NewGuid().ToString().Replace("-", "");
                var response = ProcessRequest<int>(settings, "append", title, nzbData, category, priority, false, addpaused, string.Empty, 0, "all", new string[] { "drone", droneId });
                if (response <= 0)
                {
                    return null;
                }

                return droneId;
            }
            else if (HasVersion(13, settings))
            {
                return DownloadNzbLegacy13(nzbData, title, category, priority, settings);
            }
            else
            {
                return DownloadNzbLegacy12(nzbData, title, category, priority, settings);
            }
        }

        private string DownloadNzbLegacy13(byte[] nzbData, string title, string category, int priority, NzbgetSettings settings)
        {
            var response = ProcessRequest<int>(settings, "append", title, nzbData, category, priority, false, false, string.Empty, 0, "all");
            if (response <= 0)
            {
                return null;
            }

            var queue = GetQueue(settings);
            var item = queue.FirstOrDefault(q => q.NzbId == response);

            if (item == null)
            {
                return null;
            }

            var droneId = Guid.NewGuid().ToString().Replace("-", "");
            var editResult = EditQueue("GroupSetParameter", 0, "drone=" + droneId, item.NzbId, settings);
            if (editResult)
            {
                _logger.Debug("Nzbget download drone parameter set to: {0}", droneId);
            }

            return droneId;
        }

        private string DownloadNzbLegacy12(byte[] nzbData, string title, string category, int priority, NzbgetSettings settings)
        {
            var response = ProcessRequest<bool>(settings, "append", title, category, priority, false, nzbData);
            if (!response)
            {
                return null;
            }

            var queue = GetQueue(settings);
            var item = queue.FirstOrDefault(q => q.NzbName == title.Substring(0, title.Length - 4));

            if (item == null)
            {
                return null;
            }

            var droneId = Guid.NewGuid().ToString().Replace("-", "");
            var editResult = EditQueue("GroupSetParameter", 0, "drone=" + droneId, item.LastId, settings);

            if (editResult)
            {
                _logger.Debug("Nzbget download drone parameter set to: {0}", droneId);
            }

            return droneId;
        }

        public NzbgetGlobalStatus GetGlobalStatus(NzbgetSettings settings)
        {
            return ProcessRequest<NzbgetGlobalStatus>(settings, "status");
        }

        public List<NzbgetQueueItem> GetQueue(NzbgetSettings settings)
        {
            return ProcessRequest<List<NzbgetQueueItem>>(settings, "listgroups");
        }

        public List<NzbgetHistoryItem> GetHistory(NzbgetSettings settings)
        {
            return ProcessRequest<List<NzbgetHistoryItem>>(settings, "history");
        }

        public string GetVersion(NzbgetSettings settings)
        {
            var response = ProcessRequest<string>(settings, "version");

            _versionCache.Set(GetBaseUrl(settings), response, TimeSpan.FromDays(1));

            return response;
        }

        public Dictionary<string, string> GetConfig(NzbgetSettings settings)
        {
            return ProcessRequest<List<NzbgetConfigItem>>(settings, "config").ToDictionary(v => v.Name, v => v.Value);
        }

        public void RemoveItem(string id, NzbgetSettings settings)
        {
            var queue = GetQueue(settings);
            var history = GetHistory(settings);

            int nzbId;
            NzbgetQueueItem queueItem;
            NzbgetHistoryItem historyItem;

            if (id.Length < 10 && int.TryParse(id, out nzbId))
            {
                // Download wasn't grabbed by Sonarr, so the id is the NzbId reported by nzbget.
                queueItem = queue.SingleOrDefault(h => h.NzbId == nzbId);
                historyItem = history.SingleOrDefault(h => h.Id == nzbId);
            }
            else
            {
                queueItem = queue.SingleOrDefault(h => h.Parameters.Any(p => p.Name == "drone" && id == (p.Value as string)));
                historyItem = history.SingleOrDefault(h => h.Parameters.Any(p => p.Name == "drone" && id == (p.Value as string)));
            }

            if (queueItem != null)
            {
                if (!EditQueue("GroupFinalDelete", 0, "", queueItem.NzbId, settings))
                {
                    _logger.Warn("Failed to remove item from nzbget queue, {0} [{1}]", queueItem.NzbName, queueItem.NzbId);
                }
            }
            else if (historyItem != null)
            {
                if (!EditQueue("HistoryDelete", 0, "", historyItem.Id, settings))
                {
                    _logger.Warn("Failed to remove item from nzbget history, {0} [{1}]", historyItem.Name, historyItem.Id);
                }
            }
            else
            {
                _logger.Warn("Unable to remove item from nzbget, Unknown ID: {0}", id);
                return;
            }
        }

        public void RetryDownload(string id, NzbgetSettings settings)
        {
            var history = GetHistory(settings);
            var item = history.SingleOrDefault(h => h.Parameters.SingleOrDefault(p => p.Name == "drone" && id == (p.Value as string)) != null);

            if (item == null)
            {
                _logger.Warn("Unable to return item to queue, Unknown ID: {0}", id);
                return;
            }

            if (!EditQueue("HistoryRedownload", 0, "", item.Id, settings))
            {
                _logger.Warn("Failed to return item to queue from history, {0} [{1}]", item.Name, item.Id);
            }
        }

        private bool EditQueue(string command, int offset, string editText, int id, NzbgetSettings settings)
        {
            return ProcessRequest<bool>(settings, "editqueue", command, offset, editText, id);
        }

        private T ProcessRequest<T>(NzbgetSettings settings, string method, params object[] parameters)
        {
            var baseUrl = GetBaseUrl(settings, "jsonrpc");

            var requestBuilder = new JsonRpcRequestBuilder(baseUrl, method, parameters);
            requestBuilder.LogResponseContent = true;
            requestBuilder.NetworkCredential = new BasicNetworkCredential(settings.Username, settings.Password);

            var httpRequest = requestBuilder.Build();

            HttpResponse response;
            try
            {
                response = _httpClient.Execute(httpRequest);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new DownloadClientAuthenticationException("Authentication failed for NzbGet, please check your settings", ex);
                }

                throw new DownloadClientException("Unable to connect to NzbGet. " + ex.Message, ex);
            }
            catch (WebException ex)
            {
                throw new DownloadClientUnavailableException("Unable to connect to NzbGet. " + ex.Message, ex);
            }

            var result = Json.Deserialize<JsonRpcResponse<T>>(response.Content);

            if (result.Error != null)
            {
                throw new DownloadClientException("Error response received from nzbget: {0}", result.Error.ToString());
            }

            return result.Result;
        }
    }
}
