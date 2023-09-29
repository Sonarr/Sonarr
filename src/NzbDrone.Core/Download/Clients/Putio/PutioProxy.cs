using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public interface IPutioProxy
    {
        List<PutioTorrent> GetTorrents(PutioSettings settings);
        void AddTorrentFromUrl(string torrentUrl, PutioSettings settings);
        void AddTorrentFromData(byte[] torrentData, PutioSettings settings);
        void GetAccountSettings(PutioSettings settings);
        public PutioTorrentMetadata GetTorrentMetadata(PutioTorrent torrent, PutioSettings settings);
        public Dictionary<string, PutioTorrentMetadata> GetAllTorrentMetadata(PutioSettings settings);
        public PutioFileListingResponse GetFileListingResponse(long parentId, PutioSettings settings);
    }

    public class PutioProxy : IPutioProxy
    {
        private const string _configPrefix = "sonarr_";
        private readonly Logger _logger;
        private readonly IHttpClient _httpClient;

        public PutioProxy(Logger logger, IHttpClient client)
        {
            _logger = logger;
            _httpClient = client;
        }

        public List<PutioTorrent> GetTorrents(PutioSettings settings)
        {
            var result = Execute<PutioTransfersResponse>(BuildRequest(HttpMethod.Get, "transfers/list", settings));
            return result.Resource.Transfers;
        }

        public void AddTorrentFromUrl(string torrentUrl, PutioSettings settings)
        {
            var request = BuildRequest(HttpMethod.Post, "transfers/add", settings);
            request.AddFormParameter("url", torrentUrl);

            if (settings.SaveParentId.IsNotNullOrWhiteSpace())
            {
                request.AddFormParameter("save_parent_id", settings.SaveParentId);
            }

            Execute<PutioGenericResponse>(request);
        }

        public void AddTorrentFromData(byte[] torrentData, PutioSettings settings)
        {
            // var arguments = new Dictionary<string, object>();
            // arguments.Add("metainfo", Convert.ToBase64String(torrentData));
            // ProcessRequest<PutioGenericResponse>(Method.POST, "transfers/add", arguments, settings);
        }

        public void GetAccountSettings(PutioSettings settings)
        {
            Execute<PutioGenericResponse>(BuildRequest(HttpMethod.Get, "account/settings", settings));
        }

        public PutioTorrentMetadata GetTorrentMetadata(PutioTorrent torrent, PutioSettings settings)
        {
            var metadata = Execute<PutioConfigResponse>(BuildRequest(HttpMethod.Get, "config/" + _configPrefix + torrent.Id, settings));
            if (metadata.Resource.Value != null)
            {
                _logger.Debug("Found metadata for torrent: {0} {1}", torrent.Id, metadata.Resource.Value);
                return metadata.Resource.Value;
            }

            return new PutioTorrentMetadata
            {
                Id = torrent.Id,
                Downloaded = false
            };
        }

        public PutioFileListingResponse GetFileListingResponse(long parentId, PutioSettings settings)
        {
            var request = BuildRequest(HttpMethod.Get, "files/list", settings);
            request.AddQueryParam("parent_id", parentId);
            request.AddQueryParam("per_page", 1000);

            try
            {
                var response = Execute<PutioFileListingResponse>(request);
                return response.Resource;
            }
            catch (DownloadClientException ex)
            {
                _logger.Error(ex, "Failed to get file listing response");
                throw;
            }
        }

        public Dictionary<string, PutioTorrentMetadata> GetAllTorrentMetadata(PutioSettings settings)
        {
            var metadata = Execute<PutioAllConfigResponse>(BuildRequest(HttpMethod.Get, "config", settings));
            var result = new Dictionary<string, PutioTorrentMetadata>();

            foreach (var item in metadata.Resource.Config)
            {
                if (item.Key.StartsWith(_configPrefix))
                {
                    var torrentId = item.Key.Substring(_configPrefix.Length);
                    result[torrentId] = item.Value;
                }
            }

            return result;
        }

        private HttpRequestBuilder BuildRequest(HttpMethod method, string endpoint, PutioSettings settings)
        {
            var requestBuilder = new HttpRequestBuilder("https://api.put.io/v2")
            {
                LogResponseContent = true
            };
            requestBuilder.Method = method;
            requestBuilder.Resource(endpoint);
            requestBuilder.SetHeader("Authorization", "Bearer " + settings.OAuthToken);
            return requestBuilder;
        }

        private HttpResponse<TResult> Execute<TResult>(HttpRequestBuilder requestBuilder)
            where TResult : new()
        {
            var request = requestBuilder.Build();
            request.LogResponseContent = true;

            try
            {
                if (requestBuilder.Method == HttpMethod.Post)
                {
                    return _httpClient.Post<TResult>(request);
                }
                else
                {
                    return _httpClient.Get<TResult>(request);
                }
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new DownloadClientAuthenticationException("Invalid credentials. Check your OAuthToken");
                }

                throw new DownloadClientException("Failed to connect to put.io API", ex);
            }
            catch (Exception ex)
            {
                throw new DownloadClientException("Failed to connect to put.io API", ex);
            }
        }
    }
}
