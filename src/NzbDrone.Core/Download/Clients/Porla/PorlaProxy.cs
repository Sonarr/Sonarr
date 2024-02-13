using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.Porla.Models;

namespace NzbDrone.Core.Download.Clients.Porla
{
    public interface IPorlaProxy
    {
        //sys
        PorlaSysVersions GetSysVersion(PorlaSettings settings);
        //fs
            //fs.space
        //sessions
        Session[] ListSessions(PorlaSettings settings);                     //sessions.list
        void PauseSessions(PorlaSettings settings);                         //sessions.pause
        void ResumeSessions(PorlaSettings settings);                        //sessions.pause
        PorlaSessionsSettings GetSessionSettings(PorlaSettings settings);   //sessions.settings.list
        //presets
        PorlaPresets ListPresets(PorlaSettings settings);     //presets.list
        //torrents
        PorlaTorrent AddMagnetTorrent(PorlaSettings settings, string uri);                  //torrents.add
        PorlaTorrent AddTorrentFile(PorlaSettings settings, byte[] fileContent);            //torrents.add
        void RemoveTorrent(PorlaSettings settings, bool RemoveData, PorlaTorrent[] pts);    //torrents.remove
        void MoveTorrent(PorlaSettings settings, PorlaMoveSettings MoveSettings, PorlaTorrent pt); //torrents.move
        void PauseTorrent(PorlaSettings settings, PorlaTorrent pt);      //torrents.pause
        void ResumeTorrent(PorlaSettings settings, PorlaTorrent pt);     //torrents.resume
        PorlaTorrentFiles ListTorrentsFiles(PorlaSettings settings, PorlaTorrent pt);    //torrents.files.list
        PorlaTorrentMetadata ListTorrentsMetadata(PorlaSettings settings);    //torrents.metadata.list
        //torrents.peers
        void AddPeer(PorlaSettings settings, PorlaPeers peers, PorlaTorrent pt);    //torrents.peer.add
        PorlaPeerDetail ListPeers(PorlaSettings settings, PorlaTorrent pt);        //torrents.peer.list
        //torrents.properties
            //torrents.properties.get
            //torrents.properties.set
        //torrents.recheck
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
            baseUrl = HttpUri.CombinePath(baseUrl, "api");

            string JWT = settings.InfiniteJWT ??= String.Empty
            
            if (IsNullOrEmpty(JWT)) {
                _logger.Warn("Porla: We don't implemenet alternate JWT methods (yet)")
                //add logic here
            } else {
                _logger.Notice("Porla: Setting Infinte JWT")
            }

            var requestBuilder = new JsonRpcRequestBuilder(baseUrl, method, parameters);
            requestBuilder.LogResponseContent = true;
            requestBuilder.Header.Add("Authorization", $"Bearer {JWT}")
            //requestBuilder.Headers.Add("Content-Type", "application/json"); //should be set in JRPC builder

            var httpRequest = requestBuilder.Build();
            HttpResponse response;

            try
            {
                response = _httpClient.Execute(httpRequest);
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

            var result = Json.Deserialize<JsonRpcResponse<T>>(response.Content);

            if (result.Error != null)
            {
                throw new DownloadClientException("Error response received from Porla: {0}", result.Error.ToString());
            }

            return result.Result;
        }

        private void LogSupposedToBeNothing(string method, string? something) {
            if (IsNotNullOrEmpty(empty)) {
                _logger.Warn($"Porla: method: {method} was not expected to return: {something}")
            }
        }

        //sys

        PorlaSysVersions GetSysVersion(PorlaSettings settings)
        {
            return ProcessRequest<PorlaSysVersions>(settings, "sys.versions")
        }

        // fs

        // session

        public void ListSessions(PorlaSettings settings)
        {
            var empty = ProcessRequest<string>(settings, "sessions.list")
            LogSupposedToBeNothing(empty)
        }

        public void PauseSessions(PorlaSettings settings)
        {
            var empty = ProcessRequest<string>(settings, "sessions.pause")
            LogSupposedToBeNothing(empty)
        }

        public void ResumeSessions(PorlaSettings settings)
        {
            var empty = ProcessRequest<string>(settings, "sessions.resume")
            LogSupposedToBeNothing(empty)
        }

        public PorlaSessionsSettings GetSessionSettingsList(PorlaSettings settings)
        {
            return ProcessRequest<PorlaSessionsSettings>(settings, "sessions.settings.list")
        }

        // presets

        public PorlaPresets ListPresets(PorlaSettings settings)
        {
            return ProcessRequest<PorlaPresets>(settings, "presets.list")
        }

        // torrents

        public PorlaTorrent AddMagnetTorrent(PorlaSettings settings, string uri)
        {
            return ProcessRequest<PorlaTorrent>(settings, "torrents.add")
        }

        public PorlaTorrent AddTorrentFile(PorlaSettings settings, byte[] fileContent)
        {
            return ProcessRequest<PorlaTorrent>(settings, "torrents.add")
        }

        public void RemoveTorrent(PorlaSettings settings, bool RemoveData, PorlaTorrent[] pts)
        {
            var empty = ProcessRequest<string>(settings, "torrents.remove", "info_hashes", pts.SelectMany(pt => pt.AsParam()).ToArray(), "remove_data", RemoveData)
            LogSupposedToBeNothing(empty)
        }

        public void MoveTorrent(PorlaSettings settings, PorlaMoveSettings MoveSettings, PorlaTorrent pt)
        {
            var empty = ProcessRequest<string>(settings, "torrents.move", PorlaMoveSettings.MakeParams(pt)) 
            LogSupposedToBeNothing(empty)
        }

        public void PauseTorrent(PorlaSettings settings, PorlaTorrent pt)
        {
            var empty = ProcessRequest<string>(settings, "torrents.pause", pt.AsParams())
            LogSupposedToBeNothing(empty)
        }

        public void ResumeTorrent(PorlaSettings settings, PorlaTorrent pt)
        {
            var empty = ProcessRequest<string>(settings, "torrents.resume", pt.AsParams())
            LogSupposedToBeNothing(empty)
        }

        public PorlaTorrentFiles ListTorrentsFiles(PorlaSettings settings, PorlaTorrent pt)
        {
            return ProcessRequest<PorlaTorrentFiles>(settings, "torrents.files.list", pt.AsParams())
        }

        public PorlaTorrentMetadata ListTorrentsMetadata(PorlaSettings settings)
        {
            return ProcessRequest<PorlaTorrentFiles>(settings, "torrents.metadata.list")
        }

        //torrents.peers

        public void AddPeer(PorlaSettings settings, PorlaPeers peers, PorlaTorrent pt)
        {
            var empty = ProcessRequest<string>(settings, "torrents.peer.add", PorlaPeers.MakeParams(pt))
            LogSupposedToBeNothing(empty)
        }

        public PorlaPeerDetail ListPeers(PorlaSettings settings, PorlaTorrent pt)
        {
            return ProcessRequest<PorlaPeerDetail>(settings, "torrents.peer.list", pt.AsParams())
        }
    }
}
