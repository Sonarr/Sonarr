using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Rest;
using NLog;
using RestSharp;
using Newtonsoft.Json.Linq;
using RestSharp.Deserializers;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public interface IPutioProxy
    {
        List<PutioTorrent> GetTorrents(PutioSettings settings);
        void AddTorrentFromUrl(string torrentUrl, PutioSettings settings);
        void AddTorrentFromData(byte[] torrentData, PutioSettings settings);
        void RemoveTorrent(string hash, PutioSettings settings);
        Dictionary<String, Object> GetAccountSettings(PutioSettings settings);
    }

    public class PutioProxy: IPutioProxy
    {        
        private readonly Logger _logger;

        public PutioProxy(Logger logger)
        {
            _logger = logger;
        }
        
        public List<PutioTorrent> GetTorrents(PutioSettings settings)
        {
            var result = GetTorrentStatus(settings);
            var json = result["transfers"].ToString();
            List<PutioTorrent> transfers = JsonConvert.DeserializeObject<List<PutioTorrent>>(json);            
            return transfers;
        }

        public void AddTorrentFromUrl(string torrentUrl, PutioSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("url", torrentUrl);
            ProcessRequest(Method.POST, "transfers/add", arguments, settings);
        }

        public void AddTorrentFromData(byte[] torrentData, PutioSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("metainfo", Convert.ToBase64String(torrentData));
            ProcessRequest(Method.POST, "transfers/add", arguments, settings);
        }

        public void RemoveTorrent(string hashString, PutioSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("transfer_ids", new string[] { hashString });
            ProcessRequest(Method.POST, "torrents/cancel", arguments, settings);
        }

        public Dictionary<String, Object> GetAccountSettings(PutioSettings settings)
        {
            var result = ProcessRequest(Method.GET, "account/settings", null, settings);
            return result;
        }

        private Dictionary<String, Object> GetTorrentStatus(PutioSettings settings)
        {
            var result = ProcessRequest(Method.GET, "transfers/list", null, settings);
            return result;
        }

        public Dictionary<String, Object> ProcessRequest(Method method, string resource, Dictionary<String, Object> arguments, PutioSettings settings)
        {
            var client = BuildClient(settings);

            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;
            request.AddQueryParameter("oauth_token", settings.OAuthToken);

            if (arguments != null)
            {
                foreach (KeyValuePair<string, object> e in arguments)
                {
                    request.AddParameter(e.Key, e.Value);
                }
            }

            _logger.Debug("Method: {0} Url: {1}", method, client.BuildUri(request));

            var json = new JsonDeserializer();
            var restResponse = client.Execute(request);

            Dictionary<string, object> output =
                json.Deserialize<Dictionary<string, object>>(restResponse);

            if (output == null)
            {
                throw new PutioException("Unexpected response");
            }
            else if ((string) output["status"] != "OK")
            {
                throw new PutioException((string) output["error_message"]);
            }

            return output;
        }

        private IRestClient BuildClient(PutioSettings settings)
        {
            var restClient = RestClientFactory.BuildClient(settings.Url);
            restClient.FollowRedirects = false;
            return restClient;
        }
    }
}
