using System;
using System.IO;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using RestSharp;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public interface ISabCommunicationProxy
    {
        SabAddResponse DownloadNzb(Stream nzb, string name, string category, int priority);
        void RemoveFrom(string source, string id);
        string ProcessRequest(IRestRequest restRequest, string action);
    }

    public class SabCommunicationProxy : ISabCommunicationProxy
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public SabCommunicationProxy(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public SabAddResponse DownloadNzb(Stream nzb, string title, string category, int priority)
        {
            var request = new RestRequest(Method.POST);
            var action = String.Format("mode=addfile&cat={0}&priority={1}", category, priority);

            request.AddFile("name", ReadFully(nzb), title, "application/x-nzb");
            
            var response = Json.TryDeserialize<SabAddResponse>(ProcessRequest(request, action));

            if (response == null)
            {
                response = new SabAddResponse();
                response.Status = true;
            }

            return response;
        }

        public void RemoveFrom(string source, string id)
        {
            var request = new RestRequest();
            var action = String.Format("mode={0}&name=delete&del_files=1&value={1}", source, id);

            ProcessRequest(request, action);
        }

        public string ProcessRequest(IRestRequest restRequest, string action)
        {
            var client = BuildClient(action);
            var response = client.Execute(restRequest);
            _logger.Trace("Response: {0}", response.Content);

            CheckForError(response);

            return response.Content;
        }

        private IRestClient BuildClient(string action)
        {
            var protocol = _configService.SabUseSsl ? "https" : "http";

            var url = string.Format(@"{0}://{1}:{2}/api?{3}&apikey={4}&ma_username={5}&ma_password={6}&output=json",
                                 protocol,
                                 _configService.SabHost,
                                 _configService.SabPort,
                                 action,
                                 _configService.SabApiKey,
                                 _configService.SabUsername,
                                 _configService.SabPassword);

            _logger.Trace(url);

            return new RestClient(url);
        }

        private void CheckForError(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new ApplicationException("Unable to connect to SABnzbd, please check your settings");
            }

            var result = Json.TryDeserialize<SabJsonError>(response.Content);

            if (result == null)
            {
                //Handle plain text responses from SAB
                result = new SabJsonError();

                if (response.Content.StartsWith("error", StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Status = false;
                    result.Error = response.Content.Replace("error: ", "");
                }

                else
                {
                    result.Status = true;
                }

                result.Error = response.Content.Replace("error: ", "");
            }
            
            if (!result.Status)
                throw new ApplicationException(result.Error);
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
