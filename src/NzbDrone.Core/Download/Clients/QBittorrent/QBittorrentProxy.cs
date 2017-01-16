using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    // API https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-Documentation

    public interface IQBittorrentProxy
    {
        int GetVersion(QBittorrentSettings settings);
        QBittorrentPreferences GetConfig(QBittorrentSettings settings);
        List<QBittorrentTorrent> GetTorrents(QBittorrentSettings settings);

        void AddTorrentFromUrl(string torrentUrl, QBittorrentSettings settings);
        void AddTorrentFromFile(string fileName, Byte[] fileContent, QBittorrentSettings settings);

        void RemoveTorrent(string hash, Boolean removeData, QBittorrentSettings settings);
        void SetTorrentLabel(string hash, string label, QBittorrentSettings settings);
        void MoveTorrentToTopInQueue(string hash, QBittorrentSettings settings);
    }

    public class QBittorrentProxy : IQBittorrentProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private readonly ICached<Dictionary<string, string>> _authCookieCache;

        public QBittorrentProxy(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _authCookieCache = cacheManager.GetCache<Dictionary<string, string>>(GetType(), "authCookies");
        }

        public int GetVersion(QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/version/api");
            var response = ProcessRequest<int>(request, settings);

            return response;
        }

        public QBittorrentPreferences GetConfig(QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/query/preferences");
            var response = ProcessRequest<QBittorrentPreferences>(request, settings);

            return response;
        }

        public List<QBittorrentTorrent> GetTorrents(QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/query/torrents")
                                                .AddQueryParam("label", settings.TvCategory)
                                                .AddQueryParam("category", settings.TvCategory);

            var response = ProcessRequest<List<QBittorrentTorrent>>(request, settings);

            return response;
        }

        public void AddTorrentFromUrl(string torrentUrl, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/command/download")
                                                .Post()
                                                .AddFormParameter("urls", torrentUrl);

            ProcessRequest<object>(request, settings);
        }

        public void AddTorrentFromFile(string fileName, Byte[] fileContent, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/command/upload")
                                                .Post()
                                                .AddFormUpload("torrents", fileName, fileContent);

            ProcessRequest<object>(request, settings);
        }

        public void RemoveTorrent(string hash, Boolean removeData, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource(removeData ? "/command/deletePerm" : "/command/delete")
                                                .Post()
                                                .AddFormParameter("hashes", hash);

            ProcessRequest<object>(request, settings);
        }

        public void SetTorrentLabel(string hash, string label, QBittorrentSettings settings)
        {
            var setCategoryRequest = BuildRequest(settings).Resource("/command/setCategory")
                                                        .Post()
                                                        .AddFormParameter("hashes", hash)
                                                        .AddFormParameter("category", label);
            try
            {
                ProcessRequest<object>(setCategoryRequest, settings);
            }
            catch(DownloadClientException ex)
            {
                // if setCategory fails due to method not being found, then try older setLabel command for qbittorent < v.3.3.5
                if (ex.InnerException is HttpException && (ex.InnerException as HttpException).Response.StatusCode == HttpStatusCode.NotFound)
                {
                    var setLabelRequest = BuildRequest(settings).Resource("/command/setLabel")
                                                                .Post()
                                                                .AddFormParameter("hashes", hash)
                                                                .AddFormParameter("label", label);
                    ProcessRequest<object>(setLabelRequest, settings);
                }
            }
        }

        public void MoveTorrentToTopInQueue(string hash, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/command/topPrio")
                                                .Post()
                                                .AddFormParameter("hashes", hash);

            try
            {
                var response = ProcessRequest<object>(request, settings);
            }
            catch (DownloadClientException ex)
            {
                // qBittorrent rejects all Prio commands with 403: Forbidden if Options -> BitTorrent -> Torrent Queueing is not enabled
#warning FIXME: so wouldn't the reauthenticate logic trigger on Forbidden?
                if (ex.InnerException is HttpException && (ex.InnerException as HttpException).Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return;
                }

                throw;
            }

        }

        private HttpRequestBuilder BuildRequest(QBittorrentSettings settings)
        {
            var requestBuilder = new HttpRequestBuilder(settings.UseSsl, settings.Host, settings.Port);
            requestBuilder.LogResponseContent = true;
            requestBuilder.NetworkCredential = new NetworkCredential(settings.Username, settings.Password);

            return requestBuilder;
        }

        private TResult ProcessRequest<TResult>(HttpRequestBuilder requestBuilder, QBittorrentSettings settings)
            where TResult : new()
        {
            AuthenticateClient(requestBuilder, settings);

            var request = requestBuilder.Build();

            HttpResponse response;
            try
            {
                response = _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.Debug("Authentication required, logging in.");

                    AuthenticateClient(requestBuilder, settings, true);

                    request = requestBuilder.Build();

                    response = _httpClient.Execute(request);
                }
                else
                {
                    throw new DownloadClientException("Failed to connect to qBitTorrent, check your settings.", ex);
                }
            }
            catch (WebException ex)
            {
                throw new DownloadClientException("Failed to connect to qBitTorrent, please check your settings.", ex);
            }

            return Json.Deserialize<TResult>(response.Content);
        }

        private void AuthenticateClient(HttpRequestBuilder requestBuilder, QBittorrentSettings settings, bool reauthenticate = false)
        {
            if (settings.Username.IsNullOrWhiteSpace() || settings.Password.IsNullOrWhiteSpace())
            {
                return;
            }

            var authKey = string.Format("{0}:{1}", requestBuilder.BaseUrl, settings.Password);

            var cookies = _authCookieCache.Find(authKey);

            if (cookies == null || reauthenticate)
            {
                _authCookieCache.Remove(authKey);

                var authLoginRequest = BuildRequest(settings).Resource("/login")
                                                            .Post()
                                                            .AddFormParameter("username", settings.Username ?? string.Empty)
                                                            .AddFormParameter("password", settings.Password ?? string.Empty)
                                                            .Build();

                HttpResponse response;
                try
                {
                    response = _httpClient.Execute(authLoginRequest);
                }
                catch (HttpException ex)
                {
                    _logger.Debug("qbitTorrent authentication failed.");
                    if (ex.Response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new DownloadClientAuthenticationException("Failed to authenticate with qbitTorrent.", ex);
                    }

                    throw new DownloadClientException("Failed to connect to qBitTorrent, please check your settings.", ex);
                }
                catch (WebException ex)
                {
                    throw new DownloadClientException("Failed to connect to qBitTorrent, please check your settings.", ex);
                }

                if (response.Content != "Ok.") // returns "Fails." on bad login
                {
                    _logger.Debug("qbitTorrent authentication failed.");
                    throw new DownloadClientAuthenticationException("Failed to authenticate with qbitTorrent.");
                }

                _logger.Debug("qbitTorrent authentication succeeded.");

                cookies = response.GetCookies();

                _authCookieCache.Set(authKey, cookies);
            }

            requestBuilder.SetCookies(cookies);
        }
    }
}
