using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public interface INzbGetCommunicationProxy
    {
        bool AddNzb(params object[] parameters);
        List<NzbGetQueueItem> GetQueue();
        string GetVersion();
    }

    public class NzbGetCommunicationProxy : INzbGetCommunicationProxy
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public NzbGetCommunicationProxy(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public bool AddNzb(params object[] parameters)
        {
            var request = BuildRequest(new JsonRequest("appendurl", parameters));

            return Json.Deserialize<EnqueueResponse>(ProcessRequest(request)).Result;
        }

        public List<NzbGetQueueItem> GetQueue()
        {
            var request = BuildRequest(new JsonRequest("listgroups"));      

            return Json.Deserialize<NzbGetQueue>(ProcessRequest(request)).QueueItems;
        }

        public string GetVersion()
        {
            var request = BuildRequest(new JsonRequest("version"));

            return ProcessRequest(request);
        }

        private string ProcessRequest(IRestRequest restRequest)
        {
            var client = BuildClient();
            var response = client.Execute(restRequest);
            _logger.Trace("Response: {0}", response.Content);

            CheckForError(response);

            return response.Content;
        }

        private IRestClient BuildClient()
        {
            var url = String.Format("http://{0}:{1}/jsonrpc",
                                 _configService.NzbgetHost,
                                 _configService.NzbgetPort);

            var client = new RestClient(url);
            client.Authenticator = new HttpBasicAuthenticator(_configService.NzbgetUsername, _configService.NzbgetPassword);

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
                throw new ApplicationException("Unable to connect to NzbGet, please check your settings");
            }

            var result = Json.Deserialize<JsonError>(response.Content);

            if (result.Error != null)
                throw new ApplicationException(result.Error.ToString());
        }
    }
}
