using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public interface INzbgetProxy
    {
        string DownloadNzb(byte[] nzbData, string title, string category, int priority, NzbgetSettings settings);
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
        private readonly Logger _logger;

        public NzbgetProxy(Logger logger)
        {
            _logger = logger;
        }

        public string DownloadNzb(byte[] nzbData, string title, string category, int priority, NzbgetSettings settings)
        {
            var parameters = new object[] { title, category, priority, false, Convert.ToBase64String(nzbData) };
            var request = BuildRequest(new JsonRequest("append", parameters));

            var response = Json.Deserialize<NzbgetResponse<bool>>(ProcessRequest(request, settings));
            _logger.Trace("Response: [{0}]", response.Result);

            if (!response.Result)
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
            var request = BuildRequest(new JsonRequest("status"));

            return Json.Deserialize<NzbgetResponse<NzbgetGlobalStatus>>(ProcessRequest(request, settings)).Result;
        }

        public List<NzbgetQueueItem> GetQueue(NzbgetSettings settings)
        {
            var request = BuildRequest(new JsonRequest("listgroups"));

            return Json.Deserialize<NzbgetResponse<List<NzbgetQueueItem>>>(ProcessRequest(request, settings)).Result;
        }

        public List<NzbgetHistoryItem> GetHistory(NzbgetSettings settings)
        {
            var request = BuildRequest(new JsonRequest("history"));

            return Json.Deserialize<NzbgetResponse<List<NzbgetHistoryItem>>>(ProcessRequest(request, settings)).Result;
        }

        public string GetVersion(NzbgetSettings settings)
        {
            var request = BuildRequest(new JsonRequest("version"));

            return Json.Deserialize<NzbgetResponse<string>>(ProcessRequest(request, settings)).Result;
        }

        public Dictionary<string, string> GetConfig(NzbgetSettings settings)
        {
            var request = BuildRequest(new JsonRequest("config"));

            return Json.Deserialize<NzbgetResponse<List<NzbgetConfigItem>>>(ProcessRequest(request, settings)).Result.ToDictionary(v => v.Name, v => v.Value);
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
            var parameters = new object[] { command, offset, editText, id };
            var request = BuildRequest(new JsonRequest("editqueue", parameters));
            var response = Json.Deserialize<NzbgetResponse<bool>>(ProcessRequest(request, settings));

            return response.Result;
        }

        private string ProcessRequest(IRestRequest restRequest, NzbgetSettings settings)
        {
            var client = BuildClient(settings);
            var response = client.Execute(restRequest);
            _logger.Trace("Response: {0}", response.Content);

            CheckForError(response);

            return response.Content;
        }

        private IRestClient BuildClient(NzbgetSettings settings)
        {
            var protocol = settings.UseSsl ? "https" : "http";

            var url = string.Format("{0}://{1}:{2}/jsonrpc",
                                 protocol,
                                 settings.Host,
                                 settings.Port);

            _logger.Debug("Url: " + url);

            var client = RestClientFactory.BuildClient(url);
            client.Authenticator = new HttpBasicAuthenticator(settings.Username, settings.Password);

            return client;
        }

        private IRestRequest BuildRequest(JsonRequest jsonRequest)
        {
            var request = new RestRequest(Method.POST);

            request.JsonSerializer = new JsonNetSerializer();
            request.RequestFormat = DataFormat.Json;
            request.AddBody(jsonRequest);

            return request;
        }

        private void CheckForError(IRestResponse response)
        {
            if (response.ErrorException != null)
            {
                throw new DownloadClientException("Unable to connect to NzbGet. " + response.ErrorException.Message, response.ErrorException);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new DownloadClientException("Authentication failed for NzbGet, please check your settings", response.ErrorException);
            }

            var result = Json.Deserialize<JsonError>(response.Content);

            if (result.Error != null)
                throw new DownloadClientException("Error response received from nzbget: {0}", result.Error.ToString());
        }
    }
}
