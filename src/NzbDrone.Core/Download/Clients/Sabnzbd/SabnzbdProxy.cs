using System;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Rest;
using NzbDrone.Core.Download.Clients.Sabnzbd.Responses;
using RestSharp;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public interface ISabnzbdProxy
    {
        SabnzbdAddResponse DownloadNzb(byte[] nzbData, string filename, string category, int priority, SabnzbdSettings settings);
        void RemoveFrom(string source, string id,bool deleteData, SabnzbdSettings settings);
        string ProcessRequest(IRestRequest restRequest, string action, SabnzbdSettings settings);
        SabnzbdVersionResponse GetVersion(SabnzbdSettings settings);
        SabnzbdConfig GetConfig(SabnzbdSettings settings);
        SabnzbdQueue GetQueue(int start, int limit, SabnzbdSettings settings);
        SabnzbdHistory GetHistory(int start, int limit, SabnzbdSettings settings);
        string RetryDownload(string id, SabnzbdSettings settings);
    }

    public class SabnzbdProxy : ISabnzbdProxy
    {
        private readonly Logger _logger;

        public SabnzbdProxy(Logger logger)
        {
            _logger = logger;
        }

        public SabnzbdAddResponse DownloadNzb(byte[] nzbData, string filename, string category, int priority, SabnzbdSettings settings)
        {
            var request = new RestRequest(Method.POST);
            var action = string.Format("mode=addfile&cat={0}&priority={1}", Uri.EscapeDataString(category), priority);

            request.AddFile("name", nzbData, filename, "application/x-nzb");

            SabnzbdAddResponse response;

            if (!Json.TryDeserialize<SabnzbdAddResponse>(ProcessRequest(request, action, settings), out response))
            {
                response = new SabnzbdAddResponse();
                response.Status = true;
            }

            return response;
        }

        public void RemoveFrom(string source, string id, bool deleteData, SabnzbdSettings settings)
        {
            var request = new RestRequest();

            var action = string.Format("mode={0}&name=delete&del_files={1}&value={2}", source, deleteData ? 1 : 0, id);

            ProcessRequest(request, action, settings);
        }

        public string ProcessRequest(IRestRequest restRequest, string action, SabnzbdSettings settings)
        {
            var client = BuildClient(action, settings);
            var response = client.Execute(restRequest);
            _logger.Trace("Response: {0}", response.Content);

            CheckForError(response);

            return response.Content;
        }

        public SabnzbdVersionResponse GetVersion(SabnzbdSettings settings)
        {
            var request = new RestRequest();
            var action = "mode=version";

            SabnzbdVersionResponse response;

            if (!Json.TryDeserialize<SabnzbdVersionResponse>(ProcessRequest(request, action, settings), out response))
            {
                response = new SabnzbdVersionResponse();
            }

            return response;
        }

        public SabnzbdConfig GetConfig(SabnzbdSettings settings)
        {
            var request = new RestRequest();
            var action = "mode=get_config";

            var response = Json.Deserialize<SabnzbdConfigResponse>(ProcessRequest(request, action, settings));

            return response.Config;
        }

        public SabnzbdQueue GetQueue(int start, int limit, SabnzbdSettings settings)
        {
            var request = new RestRequest();
            var action = string.Format("mode=queue&start={0}&limit={1}", start, limit);

            var response = ProcessRequest(request, action, settings);
            return Json.Deserialize<SabnzbdQueue>(JObject.Parse(response).SelectToken("queue").ToString());
        }

        public SabnzbdHistory GetHistory(int start, int limit, SabnzbdSettings settings)
        {
            var request = new RestRequest();
            var action = string.Format("mode=history&start={0}&limit={1}", start, limit);

            var response = ProcessRequest(request, action, settings);
            return Json.Deserialize<SabnzbdHistory>(JObject.Parse(response).SelectToken("history").ToString());
        }

        public string RetryDownload(string id, SabnzbdSettings settings)
        {
            var request = new RestRequest();
            var action = string.Format("mode=retry&value={0}", id);

            SabnzbdRetryResponse response;

            if (!Json.TryDeserialize<SabnzbdRetryResponse>(ProcessRequest(request, action, settings), out response))
            {
                response = new SabnzbdRetryResponse();
                response.Status = true;
            }

            return response.Id;
        }

        private IRestClient BuildClient(string action, SabnzbdSettings settings)
        {
            var protocol = settings.UseSsl ? "https" : "http";

            var authentication = settings.ApiKey.IsNullOrWhiteSpace() ?
                                 string.Format("ma_username={0}&ma_password={1}", settings.Username, Uri.EscapeDataString(settings.Password)) :
                                 string.Format("apikey={0}", settings.ApiKey);

            var url = string.Format(@"{0}://{1}:{2}/api?{3}&{4}&output=json",
                                 protocol,
                                 settings.Host,
                                 settings.Port,
                                 action,
                                 authentication);

            _logger.Debug("Url: " + url);

            return RestClientFactory.BuildClient(url);
        }

        private void CheckForError(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new DownloadClientException("Unable to connect to SABnzbd, please check your settings", response.ErrorException);
            }

            SabnzbdJsonError result;

            if (!Json.TryDeserialize<SabnzbdJsonError>(response.Content, out result))
            {
                //Handle plain text responses from SAB
                result = new SabnzbdJsonError();

                if (response.Content.StartsWith("error", StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Status = "false";
                    result.Error = response.Content.Replace("error: ", "");
                }

                else
                {
                    result.Status = "true";
                }

                result.Error = response.Content.Replace("error: ", "");
            }

            if (result.Failed)
                throw new DownloadClientException("Error response received from SABnzbd: {0}", result.Error);
        }
    }
}
