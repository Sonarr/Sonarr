using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

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

    public class PutioProxy : IPutioProxy
    {
        private readonly Logger _logger;
        private readonly IHttpClient _httpClient;

        public PutioProxy(Logger logger, IHttpClient client)
        {
            _logger = logger;
            _httpClient = client;
        }

        public List<PutioTorrent> GetTorrents(PutioSettings settings)
        {
            // var result = ProcessRequest<PutioTransfersResponse>(Method.GET, "transfers/list", null, settings);
            // return result.Transfers;
            return new List<PutioTorrent>();
        }

        public PutioFile GetFile(long fileId, PutioSettings settings)
        {
            // var result = ProcessRequest<PutioFileResponse>(Method.GET, "files/" + fileId, null, settings);
            // return result.File;
            return new PutioFile();
        }

        public void AddTorrentFromUrl(string torrentUrl, PutioSettings settings)
        {
            // var arguments = new Dictionary<string, object>();
            // arguments.Add("url", torrentUrl);
            // ProcessRequest<PutioGenericResponse>(Method.POST, "transfers/add", arguments, settings);
        }

        public void AddTorrentFromData(byte[] torrentData, PutioSettings settings)
        {
            // var arguments = new Dictionary<string, object>();
            // arguments.Add("metainfo", Convert.ToBase64String(torrentData));
            // ProcessRequest<PutioGenericResponse>(Method.POST, "transfers/add", arguments, settings);
        }

        public void RemoveTorrent(string hashString, PutioSettings settings)
        {
            // var arguments = new Dictionary<string, object>();
            // arguments.Add("transfer_ids", new string[] { hashString });
            // ProcessRequest<PutioGenericResponse>(Method.POST, "torrents/cancel", arguments, settings);
        }

        public void GetAccountSettings(PutioSettings settings)
        {
            // ProcessRequest<PutioGenericResponse>(Method.GET, "account/settings", null, settings);
        }

        private HttpRequestBuilder BuildRequest(PutioSettings settings)
        {
            var requestBuilder = new HttpRequestBuilder("https://api.put.io/v2")
            {
                LogResponseContent = true
            };
            requestBuilder.SetHeader("Authorization", "Bearer " + settings.OAuthToken);
            return requestBuilder;
        }

        private string ProcessRequest(HttpRequestBuilder requestBuilder)
        {
            var request = requestBuilder.Build();
            request.LogResponseContent = true;
            request.SuppressHttpErrorStatusCodes = new[] { HttpStatusCode.Forbidden };

            HttpResponse response;
            try
            {
                response = _httpClient.Execute(request);

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new DownloadClientException("Invalid credentials. Check your OAuthToken");
                }
            }
            catch (Exception ex)
            {
                throw new DownloadClientException("Failed to connect to put.io.", ex);
            }

            return response.Content;
        }

        private TResult ProcessRequest<TResult>(HttpRequestBuilder requestBuilder)
            where TResult : new()
        {
            var responseContent = ProcessRequest(requestBuilder);

            return Json.Deserialize<TResult>(responseContent);
        }

    }
}
