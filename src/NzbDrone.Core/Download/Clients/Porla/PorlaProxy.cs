using System;
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
        PorlaFsSpace GetFsSpace(PorlaSettings settings, string path);                   //fs.space
        void PauseSession(PorlaSettings settings);                                      //session.pause
        PorlaSessionSettingsList GetSessionSettingsList(PorlaSettings settings);    //
        PorlaAddTorrent AddMagnetTorrent(PorlaSettings settings, string Uri);           //torrents.add
        PorlaAddTorrent AddTorrentFile(PorlaSettings settings, string Base64Data);      //torrents.add
        PorlaTorrentDetail[] GetTorrent(PorlaSettings settings, PorlaGetTorrent get);   //torrents.files.list
        void MoveTorrent(PorlaSettings settings, PorlaGetTorrent get)   //torrents.move
        IReadOnlyDictionary<string, object> GetConfig(PorlaSettings settings);      
        string AddTorrentFile(PorlaSettings settings, byte[] fileContent);
        void AddTorrentUri(PorlaSettings settings, string torrentUrl);
        void RemoveTorrent(PorlaSettings settings, string downloadId);
        void RemoveTorrentAndData(PorlaSettings settings, string downloadId);
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

            var requestBuilder = new JsonRpcRequestBuilder(baseUrl, method, parameters);
            requestBuilder.LogResponseContent = true;
            requestBuilder.NetworkCredential = new BasicNetworkCredential(settings.Username, settings.Password);
            requestBuilder.Headers.Add("Accept-Encoding", "gzip,deflate");

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