using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using NzbDrone.Core.Download.Clients.Hadouken.Models;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Download.Clients.Hadouken
{
    public sealed class HadoukenProxy : IHadoukenProxy
    {
        private static int _callId;
        private readonly Logger _logger;

        public HadoukenProxy(Logger logger)
        {
            _logger = logger;
        }

        public HadoukenSystemInfo GetSystemInfo(HadoukenSettings settings)
        {
            return ProcessRequest<HadoukenSystemInfo>(settings, "core.getSystemInfo").Result;
        }

        public IDictionary<string, HadoukenTorrent> GetTorrents(HadoukenSettings settings)
        {
            return ProcessRequest<Dictionary<string, HadoukenTorrent>>(settings, "session.getTorrents").Result;
        }

        public IDictionary<string, object> GetConfig(HadoukenSettings settings)
        {
            return ProcessRequest<IDictionary<string, object>>(settings, "config.get").Result;
        }

        public string AddTorrentFile(HadoukenSettings settings, byte[] fileContent)
        {
            return ProcessRequest<string>(settings, "session.addTorrentFile", Convert.ToBase64String(fileContent)).Result;
        }

        public void AddTorrentUri(HadoukenSettings settings, string torrentUrl)
        {
            ProcessRequest<string>(settings, "session.addTorrentUri", torrentUrl);
        }

        public void RemoveTorrent(HadoukenSettings settings, string downloadId, bool deleteData)
        {
            ProcessRequest<bool>(settings, "session.removeTorrent", downloadId, deleteData);
        }

        private HadoukenResponse<TResult> ProcessRequest<TResult>(HadoukenSettings settings,
            string method,
            params object[] parameters)
        {
            var client = BuildClient(settings);
            return ProcessRequest<TResult>(client, method, parameters);
        }

        private HadoukenResponse<TResult> ProcessRequest<TResult>(IRestClient client, string method, params object[] parameters)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "api";
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Accept-Encoding", "gzip,deflate");

            var data = new Dictionary<String, Object>();
            data.Add("id", GetCallId());
            data.Add("method", method);

            if (parameters != null)
            {
                data.Add("params", parameters);
            }

            request.AddBody(data);

            _logger.Debug("Url: {0} Method: {1}", client.BuildUri(request), method);
            return client.ExecuteAndValidate<HadoukenResponse<TResult>>(request);
        }

        private IRestClient BuildClient(HadoukenSettings settings)
        {
            var protocol = settings.UseSsl ? "https" : "http";
            var url = string.Format(@"{0}://{1}:{2}", protocol, settings.Host, settings.Port);

            var restClient = RestClientFactory.BuildClient(url);
            restClient.Timeout = 4000;

            if (settings.AuthenticationType == (int) AuthenticationType.Basic)
            {
                var basicData = Encoding.UTF8.GetBytes(string.Format("{0}:{1}", settings.Username, settings.Password));
                var basicHeader = Convert.ToBase64String(basicData);

                restClient.AddDefaultHeader("Authorization", string.Format("Basic {0}", basicHeader));
            }
            else if (settings.AuthenticationType == (int) AuthenticationType.Token)
            {
                restClient.AddDefaultHeader("Authorization", string.Format("Token {0}", settings.Token));
            }

            return restClient;
        }

        private int GetCallId()
        {
            return System.Threading.Interlocked.Increment(ref _callId);
        }
    }
}