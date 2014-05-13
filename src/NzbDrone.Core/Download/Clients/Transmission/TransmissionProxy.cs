using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Common;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Rest;
using NLog;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public interface ITransmissionProxy
    {
        List<TransmissionTorrent> GetTorrents(TransmissionSettings settings);
        void AddTorrentFromUrl(String torrentUrl, TransmissionSettings settings);
        void AddTorrentFromData(Byte[] torrentData, TransmissionSettings settings);
        Dictionary<String, Object> GetConfig(TransmissionSettings settings);
        String GetVersion(TransmissionSettings settings);
        void RemoveTorrent(String hash, Boolean removeData, TransmissionSettings settings);
    }

    public class TransmissionProxy: ITransmissionProxy
    {        
        private readonly Logger _logger;

        public TransmissionProxy(Logger logger)
        {
            _logger = logger;
        }
        
        public List<TransmissionTorrent> GetTorrents(TransmissionSettings settings)
        {
            var result = GetTorrentStatus(settings);

            var torrents = ((JArray)result.Arguments["torrents"]).ToObject<List<TransmissionTorrent>>();

            return torrents;
        }

        public void AddTorrentFromUrl(String torrentUrl, TransmissionSettings settings)
        {
            var arguments = new Dictionary<String, Object>();
            arguments.Add("filename", torrentUrl);
            arguments.Add("download-dir", String.Empty);

            ProcessRequest("torrent-add", arguments, settings);
        }

        public void AddTorrentFromData(Byte[] torrentData, TransmissionSettings settings)
        {
            var arguments = new Dictionary<String, Object>();
            arguments.Add("metainfo", Convert.ToBase64String(torrentData));
            arguments.Add("download-dir", String.Empty);

            ProcessRequest("torrent-add", arguments, settings);
        }

        public String GetVersion(TransmissionSettings settings)
        {
            // Gets the transmission version.
            var config = GetConfig(settings);

            var version = config["version"];

            return version.ToString();
        }

        public Dictionary<String, Object> GetConfig(TransmissionSettings settings)
        {
            // Gets the transmission version.
            var result = GetSessionVariables(settings);

            return result.Arguments;
        }

        public void RemoveTorrent(String hashString, Boolean removeData, TransmissionSettings settings)
        {
            var arguments = new Dictionary<String, Object>();
            arguments.Add("ids", new String[] { hashString });
            arguments.Add("delete-local-data", removeData);

            ProcessRequest("torrent-remove", arguments, settings);
        }

        private TransmissionResponse GetSessionVariables(TransmissionSettings settings)
        {
            // Retrieve transmission information such as the default download directory, bandwith throttling and seed ratio.

            return ProcessRequest("session-get", null, settings);
        }

        private TransmissionResponse GetSessionStatistics(TransmissionSettings settings)
        {
            return ProcessRequest("session-stats", null, settings);
        }

        private TransmissionResponse GetTorrentStatus(TransmissionSettings settings)
        {
            return GetTorrentStatus(null, settings);
        }

        private TransmissionResponse GetTorrentStatus(IEnumerable<String> hashStrings, TransmissionSettings settings)
        {
            var fields = new String[]{
                "id",
                "hashString", // Unique torrent ID. Use this instead of the client id?
                "name",
                "downloadDir",
                "status",
                "totalSize",
                "leftUntilDone",
                "isFinished",
                "eta",
                "errorString"
            };
            
            var arguments = new Dictionary<String, Object>();
            arguments.Add("fields", fields);

            if (hashStrings != null)
            {
                arguments.Add("ids", hashStrings);
            }

            var result = ProcessRequest("torrent-get", arguments, settings);

            return result;
        }

        protected String GetSessionId(IRestClient client, TransmissionSettings settings)
        {
            var sessionIdRequest = new RestRequest();
            sessionIdRequest.RequestFormat = DataFormat.Json;
            var sessionIdResponse = client.Execute(sessionIdRequest);
            var sessionId = (String)sessionIdResponse.Headers.Single(o => o.Name == "X-Transmission-Session-Id").Value;

            return sessionId;
        }

        public TransmissionResponse ProcessRequest(String action, Object arguments, TransmissionSettings settings)
        {
            var client = BuildClient(settings);

            var sessionId = GetSessionId(client, settings);

            var request = new RestRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-Transmission-Session-Id", sessionId);

            if (!settings.Username.IsNullOrWhiteSpace())
            {
                request.Credentials = new NetworkCredential(settings.Username, settings.Password);
            }

            var data = new Dictionary<String, Object>();
            data.Add("method", action);

            if (arguments != null)
            {
                data.Add("arguments", arguments);
            }

            request.AddBody(data);

            var transmissionResponse = client.ExecuteAndValidate<TransmissionResponse>(request);

            if (transmissionResponse.Result != "success")
            {
                throw new TransmissionException(transmissionResponse.Result);
            }

            return transmissionResponse;
        }

        private IRestClient BuildClient(TransmissionSettings settings)
        {
            var url = String.Format(@"http://{0}:{1}/transmission/rpc",
                                 settings.Host,
                                 settings.Port);

            return RestClientFactory.BuildClient(url);
        }
    }
}
