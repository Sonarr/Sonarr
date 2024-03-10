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
        PorlaSessionsSettings GetSessionSettings(PorlaSettings settings);           // sessions.settings.list

        // presets

        ReadOnlyCollection<PorlaPreset> ListPresets(PorlaSettings settings);    // presets.list

        // torrents

        PorlaTorrent AddMagnetTorrent(PorlaSettings settings, string uri, IList<string> tags = null);             // torrents.add
        PorlaTorrent AddTorrentFile(PorlaSettings settings, byte[] fileContent, IList<string> tags = null);       // torrents.add
        void RemoveTorrent(PorlaSettings settings, bool removeData, PorlaTorrent[] pts);        // torrents.remove

        // torrents.move

        void PauseTorrent(PorlaSettings settings, PorlaTorrent pt);     // torrents.pause
        void ResumeTorrent(PorlaSettings settings, PorlaTorrent pt);    // torrents.resume
        ReadOnlyCollection<PorlaTorrentDetail> ListTorrents(PorlaSettings settings, long page = 0, long size = long.MaxValue);      // torrents.list

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

        private T ProcessRequest<T>(PorlaSettings settings, string method, params object[] parameters)
        {
            var baseUrl = HttpRequestBuilder.BuildBaseUrl(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase);

            var jwt = settings.InfinteJWT ??= string.Empty;
            var apiurl = settings.ApiUrl ??= string.Empty;

            // this block will run a lot, don't want to be too noisy in the logs
            if (string.IsNullOrEmpty(jwt))
            {
                // _logger.Warn("Porla: We don't implemenet alternate JWT methods (yet)")
                // add logic here
            }
            else
            {
                // _logger.Notice("Porla: Setting Infinte JWT")
            }

            var requestBuilder = new JsonRpcRequestBuilder(baseUrl, method, true, parameters)
                .Resource(apiurl)
                .SetHeader("Authorization", $"Bearer {jwt}");
            requestBuilder.LogResponseContent = true;

            var httpRequest = requestBuilder.Build();
            _logger.Info($"Pain: {httpRequest.ToString()}");
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
                _logger.Warn($"Porla: method: {method} was not expected to return: {something}");
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

        public PorlaSessionsSettings GetSessionSettings(PorlaSettings settings)
        {
            return ProcessRequest<PorlaSessionsSettings>(settings, "sessions.settings.list");
        }

        // presets

        public ReadOnlyCollection<PorlaPreset> ListPresets(PorlaSettings settings)
        {
            var presets = ProcessRequest<ResponsePorlaPresetsList>(settings, "presets.list");
            return presets.Presets;
        }

        // torrents

        public PorlaTorrent AddMagnetTorrent(PorlaSettings settings, string uri, IList<string> tags = null)
        {
            var dir = string.IsNullOrWhiteSpace(settings.TvDirectory) ? null : settings.TvDirectory;
            return ProcessRequest<PorlaTorrent>(settings, "torrents.add", "preset", settings.Preset, "tags", tags, "magnet_uri", uri, "save_path", dir);
        }

        public PorlaTorrent AddTorrentFile(PorlaSettings settings, byte[] fileContent, IList<string> tags = null)
        {
            _logger.Info($"Whute: {settings.TvDirectory}");
            var dir = string.IsNullOrWhiteSpace(settings.TvDirectory) ? null : settings.TvDirectory;
            _logger.Info($"Whut: {dir}");

            return ProcessRequest<PorlaTorrent>(settings, "torrents.add", "preset", settings.Preset, "tags", tags, "ti", fileContent.ToBase64(), "save_path", dir);
        }

        public void RemoveTorrent(PorlaSettings settings, bool removeData, PorlaTorrent[] pts)
        {
            var empty = ProcessRequest<string>(settings, "torrents.remove", "info_hashes", pts.SelectMany(pt => pt.AsParam()).ToArray(), "remove_data", removeData);
            LogSupposedToBeNothing("RemoveTorrent", empty);
        }

        public void PauseTorrent(PorlaSettings settings, PorlaTorrent pt)
        {
            var empty = ProcessRequest<string>(settings, "torrents.pause", pt.AsParams());
            LogSupposedToBeNothing("PauseTorrent", empty);
        }

        public void ResumeTorrent(PorlaSettings settings, PorlaTorrent pt)
        {
            var empty = ProcessRequest<string>(settings, "torrents.resume", pt.AsParams());
            LogSupposedToBeNothing("ResumeTorrent", empty);
        }

        public ReadOnlyCollection<PorlaTorrentDetail> ListTorrents(PorlaSettings settings, long page = 0, long size = long.MaxValue)
        {
            // cheating with the Int64.MaxValue  :P
            var resp = ProcessRequest<ResponsePorlaTorrentList>(settings, "torrents.list", "filters", new { category = settings.Category ?? "" }, "page", page, "size", size);
            return resp.Torrents;
        }
    }
}
