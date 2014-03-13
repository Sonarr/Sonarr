using System;
using System.IO;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.Sabnzbd.Responses;
using NzbDrone.Core.Instrumentation.Extensions;
using RestSharp;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public interface ISabnzbdProxy
    {
        SabnzbdAddResponse DownloadNzb(Stream nzb, string name, string category, int priority, SabnzbdSettings settings);
        void RemoveFrom(string source, string id, SabnzbdSettings settings);
        string ProcessRequest(IRestRequest restRequest, string action, SabnzbdSettings settings);
        SabnzbdVersionResponse GetVersion(SabnzbdSettings settings);
        SabnzbdCategoryResponse GetCategories(SabnzbdSettings settings);
        SabnzbdQueue GetQueue(int start, int limit, SabnzbdSettings settings);
        SabnzbdHistory GetHistory(int start, int limit, SabnzbdSettings settings);
    }

    public class SabnzbdProxy : ISabnzbdProxy
    {
        private readonly Logger _logger;

        public SabnzbdProxy(Logger logger)
        {
            _logger = logger;
        }

        public SabnzbdAddResponse DownloadNzb(Stream nzb, string title, string category, int priority, SabnzbdSettings settings)
        {
            var request = new RestRequest(Method.POST);
            var action = String.Format("mode=addfile&cat={0}&priority={1}", category, priority);

            request.AddFile("name", ReadFully(nzb), title, "application/x-nzb");

            SabnzbdAddResponse response;

            if (!Json.TryDeserialize<SabnzbdAddResponse>(ProcessRequest(request, action, settings), out response))
            {
                response = new SabnzbdAddResponse();
                response.Status = true;
            }

            return response;
        }

        public void RemoveFrom(string source, string id, SabnzbdSettings settings)
        {
            var request = new RestRequest();
            var action = String.Format("mode={0}&name=delete&del_files=1&value={1}", source, id);

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

        public SabnzbdCategoryResponse GetCategories(SabnzbdSettings settings)
        {
            var request = new RestRequest();
            var action = "mode=get_cats";

            var response = Json.Deserialize<SabnzbdCategoryResponse>(ProcessRequest(request, action, settings));

            return response;
        }

        public SabnzbdQueue GetQueue(int start, int limit, SabnzbdSettings settings)
        {
            var request = new RestRequest();
            var action = String.Format("mode=queue&start={0}&limit={1}", start, limit);

            var response = ProcessRequest(request, action, settings);
            return Json.Deserialize<SabnzbdQueue>(JObject.Parse(response).SelectToken("queue").ToString());
        }

        public SabnzbdHistory GetHistory(int start, int limit, SabnzbdSettings settings)
        {
            var request = new RestRequest();
            var action = String.Format("mode=history&start={0}&limit={1}", start, limit);

            var response = ProcessRequest(request, action, settings);
            return Json.Deserialize<SabnzbdHistory>(JObject.Parse(response).SelectToken("history").ToString());
        }

        private IRestClient BuildClient(string action, SabnzbdSettings settings)
        {
            var protocol = settings.UseSsl ? "https" : "http";

            var authentication = settings.ApiKey.IsNullOrWhiteSpace() ?
                                 String.Format("ma_username={0}&ma_password={1}", settings.Username, settings.Password) :
                                 String.Format("apikey={0}", settings.ApiKey);

            var url = string.Format(@"{0}://{1}:{2}/api?{3}&{4}&output=json",
                                 protocol,
                                 settings.Host,
                                 settings.Port,
                                 action,
                                 authentication);

            _logger.CleansedDebug(url);

            return new RestClient(url);
        }

        private void CheckForError(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new DownloadClientException("Unable to connect to SABnzbd, please check your settings");
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

        //TODO: Find a better home for this
        private byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
