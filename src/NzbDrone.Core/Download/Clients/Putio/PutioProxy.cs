using System;
using System.Collections.Generic;
using NzbDrone.Core.Rest;
using NLog;
using RestSharp;
using RestSharp.Deserializers;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public interface IPutioProxy
    {
        List<PutioTorrent> GetTorrents(PutioSettings settings);
        PutioFile GetFile(long fileId, PutioSettings settings);
        void AddTorrentFromUrl(string torrentUrl, PutioSettings settings);
        void AddTorrentFromData(byte[] torrentData, PutioSettings settings);
        void RemoveTorrent(string hash, PutioSettings settings);
        void GetAccountSettings(PutioSettings settings);
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
            var result = ProcessRequest<PutioTransfersResponse>(Method.GET, "transfers/list", null, settings);
            return result.Transfers;
        }

        public PutioFile GetFile(long fileId, PutioSettings settings)
        {
            var result = ProcessRequest<PutioFileResponse>(Method.GET, "files/" + fileId, null, settings);
            return result.File;
        }

        public void AddTorrentFromUrl(string torrentUrl, PutioSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("url", torrentUrl);
            ProcessRequest<PutioGenericResponse>(Method.POST, "transfers/add", arguments, settings);
        }

        public void AddTorrentFromData(byte[] torrentData, PutioSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("metainfo", Convert.ToBase64String(torrentData));
            ProcessRequest<PutioGenericResponse>(Method.POST, "transfers/add", arguments, settings);
        }

        public void RemoveTorrent(string hashString, PutioSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("transfer_ids", new string[] { hashString });
            ProcessRequest<PutioGenericResponse>(Method.POST, "torrents/cancel", arguments, settings);
        }

        public void GetAccountSettings(PutioSettings settings)
        {
            ProcessRequest<PutioGenericResponse>(Method.GET, "account/settings", null, settings);
        }

        public TResponseType ProcessRequest<TResponseType>(Method method, string resource, Dictionary<string, object> arguments, PutioSettings settings) where TResponseType : PutioGenericResponse
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

            var restResponse = client.Execute(request);

            var json = new JsonDeserializer();

            TResponseType output = json.Deserialize<TResponseType>(restResponse);

            if (output.Status != "OK")
            {
                throw new PutioException(output.ErrorMessage);
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
