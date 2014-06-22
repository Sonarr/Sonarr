using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public interface INzbgetProxy
    {
        string DownloadNzb(Stream nzb, string title, string category, int priority, NzbgetSettings settings);
        NzbgetGlobalStatus GetGlobalStatus(NzbgetSettings settings);
        List<NzbgetQueueItem> GetQueue(NzbgetSettings settings);
        List<NzbgetPostQueueItem> GetPostQueue(NzbgetSettings settings);
        List<NzbgetHistoryItem> GetHistory(NzbgetSettings settings);
        String GetVersion(NzbgetSettings settings);
        Dictionary<String, String> GetConfig(NzbgetSettings settings);
        void RemoveFromHistory(string id, NzbgetSettings settings);
        void RetryDownload(string id, NzbgetSettings settings);
    }

    public class NzbgetProxy : INzbgetProxy
    {
        private readonly Logger _logger;

        public NzbgetProxy(Logger logger)
        {
            _logger = logger;
        }

        public string DownloadNzb(Stream nzb, string title, string category, int priority, NzbgetSettings settings)
        {
            var parameters = new object[] { title, category, priority, false, Convert.ToBase64String(nzb.ToBytes()) };
            var request = BuildRequest(new JsonRequest("append", parameters));

            var response = Json.Deserialize<NzbgetResponse<Boolean>>(ProcessRequest(request, settings));
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

        public List<NzbgetPostQueueItem> GetPostQueue(NzbgetSettings settings)
        {
            var request = BuildRequest(new JsonRequest("postqueue"));

            return Json.Deserialize<NzbgetResponse<List<NzbgetPostQueueItem>>>(ProcessRequest(request, settings)).Result;
        }

        public List<NzbgetHistoryItem> GetHistory(NzbgetSettings settings)
        {
            var request = BuildRequest(new JsonRequest("history"));

            return Json.Deserialize<NzbgetResponse<List<NzbgetHistoryItem>>>(ProcessRequest(request, settings)).Result;
        }

        public String GetVersion(NzbgetSettings settings)
        {
            var request = BuildRequest(new JsonRequest("version"));

            return Json.Deserialize<NzbgetResponse<String>>(ProcessRequest(request, settings)).Version;
        }

        public Dictionary<String, String> GetConfig(NzbgetSettings settings)
        {
            var request = BuildRequest(new JsonRequest("config"));

            return Json.Deserialize<NzbgetResponse<List<NzbgetConfigItem>>>(ProcessRequest(request, settings)).Result.ToDictionary(v => v.Name, v => v.Value);
        }


        public void RemoveFromHistory(string id, NzbgetSettings settings)
        {
            var history = GetHistory(settings);
            var item = history.SingleOrDefault(h => h.Parameters.Any(p => p.Name == "drone" && id == (p.Value as string)));

            if (item == null)
            {
                _logger.Warn("Unable to remove item from nzbget's history, Unknown ID: {0}", id);
                return;
            }

            if (!EditQueue("HistoryDelete", 0, "", item.Id, settings))
            {
                _logger.Warn("Failed to remove item from nzbget history, {0} [{1}]", item.Name, item.Id);
            }
        }

        public void RetryDownload(string id, NzbgetSettings settings)
        {
            var history = GetHistory(settings);
            var item = history.SingleOrDefault(h => h.Parameters.SingleOrDefault(p => p.Name == "drone") != null);

            if (item == null)
            {
                _logger.Warn("Unable to return item to queue, Unknown ID: {0}", id);
                return;
            }

            if (!EditQueue("HistoryReturn", 0, "", item.Id, settings))
            {
                _logger.Warn("Failed to return item to queue from history, {0} [{1}]", item.Name, item.Id);
            }
        }

        private bool EditQueue(string command, int offset, string editText, int id, NzbgetSettings settings)
        {
            var parameters = new object[] { command, offset, editText, id };
            var request = BuildRequest(new JsonRequest("editqueue", parameters));
            var response = Json.Deserialize<NzbgetResponse<Boolean>>(ProcessRequest(request, settings));

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

            var url = String.Format("{0}://{1}:{2}/jsonrpc",
                                 protocol,
                                 settings.Host,
                                 settings.Port);

            _logger.Debug("Url: " + url);

            var client = new RestClient(url);
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
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new DownloadClientException("Unable to connect to NzbGet, please check your settings");
            }

            var result = Json.Deserialize<JsonError>(response.Content);

            if (result.Error != null)
                throw new DownloadClientException("Error response received from nzbget: {0}", result.Error.ToString());
        }
    }
}
