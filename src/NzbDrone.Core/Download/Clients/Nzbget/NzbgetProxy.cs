using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public interface INzbgetProxy
    {
        bool AddNzb(NzbgetSettings settings, params object[] parameters);
        List<NzbgetQueueItem> GetQueue(NzbgetSettings settings);
        VersionResponse GetVersion(NzbgetSettings settings);
    }

    public class NzbgetProxy : INzbgetProxy
    {
        private readonly Logger _logger;

        public NzbgetProxy(Logger logger)
        {
            _logger = logger;
        }

        public bool AddNzb(NzbgetSettings settings, params object[] parameters)
        {
            var request = BuildRequest(new JsonRequest("appendurl", parameters));

            return Json.Deserialize<EnqueueResponse>(ProcessRequest(request, settings)).Result;
        }

        public List<NzbgetQueueItem> GetQueue(NzbgetSettings settings)
        {
            var request = BuildRequest(new JsonRequest("listgroups"));

            return Json.Deserialize<NzbgetQueue>(ProcessRequest(request, settings)).QueueItems;
        }

        public VersionResponse GetVersion(NzbgetSettings settings)
        {
            var request = BuildRequest(new JsonRequest("version"));

            return Json.Deserialize<VersionResponse>(ProcessRequest(request, settings));
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
