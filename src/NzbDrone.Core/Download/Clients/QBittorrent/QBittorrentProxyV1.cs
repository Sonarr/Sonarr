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

    public class QBittorrentProxyV1 : IQBittorrentProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private readonly ICached<Dictionary<string, string>> _authCookieCache;

        public QBittorrentProxyV1(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _authCookieCache = cacheManager.GetCache<Dictionary<string, string>>(GetType(), "authCookies");
        }

        public bool IsApiSupported(QBittorrentSettings settings)
        {
            // We can do the api test without having to authenticate since v4.1 will return 404 on the request.
            var request = BuildRequest(settings).Resource("/version/api");
            request.SuppressHttpError = true;

            try
            {
                var response = _httpClient.Execute(request.Build());

                // Version request will return 404 if it doesn't exist.
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return true;
                }

                if (response.HasHttpError)
                {
                    throw new DownloadClientException("Failed to connect to qBittorrent, check your settings.", new HttpException(response));
                }

                return true;
            }
            catch (WebException ex)
            {
                throw new DownloadClientException("Failed to connect to qBittorrent, check your settings.", ex);
            }
        }

        public Version GetApiVersion(QBittorrentSettings settings)
        {
            // Version request does not require authentication and will return 404 if it doesn't exist.
            var request = BuildRequest(settings).Resource("/version/api");
            var response = Version.Parse("1." + ProcessRequest(request, settings));

            return response;
        }

        public string GetVersion(QBittorrentSettings settings)
        {
            // Version request does not require authentication.
            var request = BuildRequest(settings).Resource("/version/qbittorrent");
            var response = ProcessRequest(request, settings).TrimStart('v');

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
            var request = BuildRequest(settings).Resource("/query/torrents");
            if (settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                request.AddQueryParam("label", settings.TvCategory);
                request.AddQueryParam("category", settings.TvCategory);
            }

            var response = ProcessRequest<List<QBittorrentTorrent>>(request, settings);

            return response;
        }

        public bool IsTorrentLoaded(string hash, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource($"/query/propertiesGeneral/{hash}");
            request.LogHttpError = false;

            try
            {
                ProcessRequest(request, settings);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public QBittorrentTorrentProperties GetTorrentProperties(string hash, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource($"/query/propertiesGeneral/{hash}");
            var response = ProcessRequest<QBittorrentTorrentProperties>(request, settings);

            return response;
        }

        public List<QBittorrentTorrentFile> GetTorrentFiles(string hash, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource($"/query/propertiesFiles/{hash}");
            var response = ProcessRequest<List<QBittorrentTorrentFile>>(request, settings);

            return response;
        }

        public void AddTorrentFromUrl(string torrentUrl, TorrentSeedConfiguration seedConfiguration, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/command/download")
                                                .Post()
                                                .AddFormParameter("urls", torrentUrl);

            if (settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                request.AddFormParameter("category", settings.TvCategory);
            }

            // Note: ForceStart is handled by separate api call
            if ((QBittorrentState)settings.InitialState == QBittorrentState.Start)
            {
                request.AddFormParameter("paused", false);
            }
            else if ((QBittorrentState)settings.InitialState == QBittorrentState.Pause)
            {
                request.AddFormParameter("paused", true);
            }

            var result = ProcessRequest(request, settings);

            // Note: Older qbit versions returned nothing, so we can't do != "Ok." here.
            if (result == "Fails.")
            {
                throw new DownloadClientException("Download client failed to add torrent by url");
            }
        }

        public void AddTorrentFromFile(string fileName, byte[] fileContent, TorrentSeedConfiguration seedConfiguration, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/command/upload")
                                                .Post()
                                                .AddFormUpload("torrents", fileName, fileContent);

            if (settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                request.AddFormParameter("category", settings.TvCategory);
            }

            // Note: ForceStart is handled by separate api call
            if ((QBittorrentState)settings.InitialState == QBittorrentState.Start)
            {
                request.AddFormParameter("paused", false);
            }
            else if ((QBittorrentState)settings.InitialState == QBittorrentState.Pause)
            {
                request.AddFormParameter("paused", true);
            }

            var result = ProcessRequest(request, settings);

            // Note: Current qbit versions return nothing, so we can't do != "Ok." here.
            if (result == "Fails.")
            {
                throw new DownloadClientException("Download client failed to add torrent");
            }
        }

        public void RemoveTorrent(string hash, bool removeData, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource(removeData ? "/command/deletePerm" : "/command/delete")
                                                    .Post()
                                                    .AddFormParameter("hashes", hash);

            ProcessRequest(request, settings);
        }

        public void SetTorrentLabel(string hash, string label, QBittorrentSettings settings)
        {
            var setCategoryRequest = BuildRequest(settings).Resource("/command/setCategory")
                                                        .Post()
                                                        .AddFormParameter("hashes", hash)
                                                        .AddFormParameter("category", label);
            try
            {
                ProcessRequest(setCategoryRequest, settings);
            }
            catch (DownloadClientException ex)
            {
                // if setCategory fails due to method not being found, then try older setLabel command for qBittorrent < v.3.3.5
                if (ex.InnerException is HttpException && (ex.InnerException as HttpException).Response.StatusCode == HttpStatusCode.NotFound)
                {
                    var setLabelRequest = BuildRequest(settings).Resource("/command/setLabel")
                                                                .Post()
                                                                .AddFormParameter("hashes", hash)
                                                                .AddFormParameter("label", label);

                    ProcessRequest(setLabelRequest, settings);
                }
            }
        }

        public void AddLabel(string label, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/command/addCategory")
                                                .Post()
                                                .AddFormParameter("category", label);
            ProcessRequest(request, settings);
        }

        public Dictionary<string, QBittorrentLabel> GetLabels(QBittorrentSettings settings)
        {
            throw new NotSupportedException("qBittorrent api v1 does not support getting all torrent categories");
        }

        public void SetTorrentSeedingConfiguration(string hash, TorrentSeedConfiguration seedConfiguration, QBittorrentSettings settings)
        {
            // Not supported on api v1
        }

        public void MoveTorrentToTopInQueue(string hash, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/command/topPrio")
                                                    .Post()
                                                    .AddFormParameter("hashes", hash);
            try
            {
                ProcessRequest(request, settings);
            }
            catch (DownloadClientException ex)
            {
                // qBittorrent rejects all Prio commands with 403: Forbidden if Options -> BitTorrent -> Torrent Queueing is not enabled
                if (ex.InnerException is HttpException && (ex.InnerException as HttpException).Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return;
                }

                throw;
            }
        }

        public void PauseTorrent(string hash, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/command/pause")
                                                 .Post()
                                                 .AddFormParameter("hash", hash);
            ProcessRequest(request, settings);
        }

        public void ResumeTorrent(string hash, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/command/resume")
                                                .Post()
                                                .AddFormParameter("hash", hash);
            ProcessRequest(request, settings);
        }

        public void SetForceStart(string hash, bool enabled, QBittorrentSettings settings)
        {
            var request = BuildRequest(settings).Resource("/command/setForceStart")
                                                .Post()
                                                .AddFormParameter("hashes", hash)
                                                .AddFormParameter("value", enabled ? "true" : "false");
            ProcessRequest(request, settings);
        }

        private HttpRequestBuilder BuildRequest(QBittorrentSettings settings)
        {
            var requestBuilder = new HttpRequestBuilder(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase)
            {
                LogResponseContent = true,
                NetworkCredential = new NetworkCredential(settings.Username, settings.Password)
            };
            return requestBuilder;
        }

        private TResult ProcessRequest<TResult>(HttpRequestBuilder requestBuilder, QBittorrentSettings settings)
            where TResult : new()
        {
            var responseContent = ProcessRequest(requestBuilder, settings);

            return Json.Deserialize<TResult>(responseContent);
        }

        private string ProcessRequest(HttpRequestBuilder requestBuilder, QBittorrentSettings settings)
        {
            AuthenticateClient(requestBuilder, settings);

            var request = requestBuilder.Build();
            request.LogResponseContent = true;
            request.SuppressHttpErrorStatusCodes = new[] { HttpStatusCode.Forbidden };

            HttpResponse response;
            try
            {
                response = _httpClient.Execute(request);

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.Debug("Authentication required, logging in.");

                    AuthenticateClient(requestBuilder, settings, true);

                    request = requestBuilder.Build();

                    response = _httpClient.Execute(request);
                }
            }
            catch (HttpException ex)
            {
                throw new DownloadClientException("Failed to connect to qBittorrent, check your settings.", ex);
            }
            catch (WebException ex)
            {
                throw new DownloadClientException("Failed to connect to qBittorrent, please check your settings.", ex);
            }

            return response.Content;
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
                        throw new DownloadClientAuthenticationException("Failed to authenticate with qBittorrent.", ex);
                    }

                    throw new DownloadClientException("Failed to connect to qBittorrent, please check your settings.", ex);
                }
                catch (WebException ex)
                {
                    throw new DownloadClientUnavailableException("Failed to connect to qBittorrent, please check your settings.", ex);
                }

                if (response.Content != "Ok.")
                {
                    // returns "Fails." on bad login
                    _logger.Debug("qbitTorrent authentication failed.");
                    throw new DownloadClientAuthenticationException("Failed to authenticate with qBittorrent.");
                }

                _logger.Debug("qBittorrent authentication succeeded.");

                cookies = response.GetCookies();

                _authCookieCache.Set(authKey, cookies);
            }

            requestBuilder.SetCookies(cookies);
        }
    }
}
