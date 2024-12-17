using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.Porla.Models;

namespace NzbDrone.Core.Download.Clients.Porla
{
    #nullable enable
    public interface IPorlaProxy
    {
        // sys

        PorlaSysVersions GetSysVersion(PorlaSettings settings);     // sys.versions

        // fs
            // fs.space

        // sessions

        ReadOnlyCollection<PorlaSession> ListSessions(PorlaSettings settings);      // sessions.list
        void PauseSessions(PorlaSettings settings);                                 // sessions.pause
        void ResumeSessions(PorlaSettings settings);                                // sessions.resume
        PorlaSessionSettings GetSessionSettings(PorlaSettings settings);           // sessions.settings.list

        // presets

        ReadOnlyDictionary<string, PorlaPreset> ListPresets(PorlaSettings settings);    // presets.list

        // torrents

        PorlaTorrent AddMagnetTorrent(PorlaSettings settings, string uri, IList<string>? tags = null);             // torrents.add
        PorlaTorrent AddTorrentFile(PorlaSettings settings, byte[] fileContent, IList<string>? tags = null);       // torrents.add
        void RemoveTorrent(PorlaSettings settings, bool removeData, PorlaTorrent[] porlaTorrents);        // torrents.remove

        // torrents.move

        void PauseTorrent(PorlaSettings settings, PorlaTorrent porlaTorrent);     // torrents.pause
        void ResumeTorrent(PorlaSettings settings, PorlaTorrent porlaTorrent);    // torrents.resume
        ReadOnlyCollection<PorlaTorrentDetail> ListTorrents(PorlaSettings settings, int page = 0, int size = int.MaxValue);      // torrents.list

        // torrents.recheck
        // torrents.files.list
        // torrents.metadata.list
        // torrents.trackers.list

        // torrents.peers
            // torrents.peer.add
            // torrents.peer.list

        // torrents.properties
            // torrents.properties.get
            // torrents.properties.set
    }

    public class PorlaProxy : IPorlaProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public PorlaProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private T ProcessRequest<T>(PorlaSettings settings, string method, params object?[] parameters)
        {
            var baseUrl = HttpRequestBuilder.BuildBaseUrl(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase);

            var jwt = settings.InfinteJWT ??= string.Empty;
            var apiurl = settings.ApiUrl ??= string.Empty;

            var requestBuilder = new JsonRpcRequestBuilder(baseUrl, method, true, parameters)
                .Resource(apiurl)
                .SetHeader("Authorization", $"Bearer {jwt}");
            requestBuilder.LogResponseContent = true;

            var httpRequest = requestBuilder.Build();
            _logger.Debug(httpRequest.ToString());
            HttpResponse response;

            // TODO: catch and throw auth exceptions like in Qbit
            try
            {
                response = _httpClient.Execute(httpRequest);
            }
            catch (HttpRequestException ex)
            {
                throw new DownloadClientException("Unable to connect to Porla, please check your settings", ex);
            }
            catch (HttpException ex)
            {
                throw new DownloadClientException("Unable to connect to Porla, please check your settings", ex);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.TrustFailure)
                {
                    throw new DownloadClientUnavailableException("Unable to connect to Porla, certificate validation failed.", ex);
                }

                throw new DownloadClientUnavailableException("Unable to connect to Porla, please check your settings", ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Unkown Connection Error");
                throw new DownloadClientException("Unable to connect to Porla, Unkown error", ex);
            }

            var result = Json.Deserialize<JsonRpcResponse<T>>(response.Content);

            if (result.Error != null)
            {
                throw new DownloadClientException("Error response received from Porla: {0}", result.Error.ToString());
            }

            return result.Result;
        }

        private void LogSupposedToBeNothing(string method, string something)
        {
            if (!string.IsNullOrEmpty(something))
            {
                _logger.Warn($"method: {method} was not expected to return: {something}");
            }
        }

        // sys

        public PorlaSysVersions GetSysVersion(PorlaSettings settings)
        {
            return ProcessRequest<PorlaSysVersions>(settings, "sys.versions");
        }

        // fs

        // session

        public ReadOnlyCollection<PorlaSession> ListSessions(PorlaSettings settings)
        {
            var sessions = ProcessRequest<ResponsePorlaSessionList>(settings, "sessions.list");
            return sessions.Sessions;
        }

        public void PauseSessions(PorlaSettings settings)
        {
            var empty = ProcessRequest<string>(settings, "sessions.pause");
            LogSupposedToBeNothing("PauseSessions", empty);
        }

        public void ResumeSessions(PorlaSettings settings)
        {
            var empty = ProcessRequest<string>(settings, "sessions.resume");
            LogSupposedToBeNothing("ResumeSessions", empty);
        }

        public PorlaSessionSettings GetSessionSettings(PorlaSettings settings)
        {
            var resp = ProcessRequest<ResponsePorlaSessionSettingsList>(settings, "sessions.settings.list");
            return resp.Settings;
        }

        // presets

        public ReadOnlyDictionary<string, PorlaPreset> ListPresets(PorlaSettings settings)
        {
            var presets = ProcessRequest<ResponsePorlaPresetsList>(settings, "presets.list");
            return presets.Presets;
        }

        // torrents

        public PorlaTorrent AddMagnetTorrent(PorlaSettings settings, string uri, IList<string>? tags = null)
        {
            var dir = string.IsNullOrWhiteSpace(settings.TvDirectory) ? null : settings.TvDirectory;
            var category = string.IsNullOrWhiteSpace(settings.Category) ? "" : settings.Category;
            var preset = string.IsNullOrWhiteSpace(settings.Preset) ? "" : settings.Preset;
            var torrent = ProcessRequest<PorlaTorrent>(settings, "torrents.add", "preset", preset, "tags", tags, "category", category, "magnet_uri", uri, "save_path", dir);
            return torrent;
        }

        public PorlaTorrent AddTorrentFile(PorlaSettings settings, byte[] fileContent, IList<string>? tags = null)
        {
            var dir = string.IsNullOrWhiteSpace(settings.TvDirectory) ? null : settings.TvDirectory;
            var category = string.IsNullOrWhiteSpace(settings.Category) ? "" : settings.Category;
            var preset = string.IsNullOrWhiteSpace(settings.Preset) ? "" : settings.Preset;
            var torrent = ProcessRequest<PorlaTorrent>(settings, "torrents.add", "preset", preset, "tags", tags, "category", category, "ti", fileContent.ToBase64(), "save_path", dir);
            return torrent;
        }

        public void RemoveTorrent(PorlaSettings settings, bool removeData, PorlaTorrent[] porlaTorrents)
        {
            var empty = ProcessRequest<string>(settings, "torrents.remove", "info_hashes", porlaTorrents.SelectMany(porlaTorrent => porlaTorrent.AsParameters()).ToArray(), "remove_data", removeData);
            LogSupposedToBeNothing("RemoveTorrent", empty);
        }

        public void PauseTorrent(PorlaSettings settings, PorlaTorrent porlaTorrent)
        {
            var empty = ProcessRequest<string>(settings, "torrents.pause", porlaTorrent.AsQualifiedParameters());
            LogSupposedToBeNothing("PauseTorrent", empty);
        }

        public void ResumeTorrent(PorlaSettings settings, PorlaTorrent porlaTorrent)
        {
            var empty = ProcessRequest<string>(settings, "torrents.resume", porlaTorrent.AsQualifiedParameters());
            LogSupposedToBeNothing("ResumeTorrent", empty);
        }

        public ReadOnlyCollection<PorlaTorrentDetail> ListTorrents(PorlaSettings settings, int page = 0, int size = int.MaxValue)
        {
            // cheating with the int.MaxValue. Should do proper Pagination :P
            var resp = ProcessRequest<ResponsePorlaTorrentList>(settings, "torrents.list", "filters", new { category = settings.Category ?? "" }, "page", page, "size", size);
            return resp.Torrents;
        }
    }
}
