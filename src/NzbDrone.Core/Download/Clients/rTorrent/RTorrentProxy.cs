using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using CookComputing.XmlRpc;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Download.Clients.RTorrent
{
    public interface IRTorrentProxy
    {
        string GetVersion(RTorrentSettings settings);
        List<RTorrentTorrent> GetTorrents(RTorrentSettings settings);

        void AddTorrentFromUrl(string torrentUrl, string label, RTorrentPriority priority, string directory, RTorrentSettings settings);
        void AddTorrentFromFile(string fileName, byte[] fileContent, string label, RTorrentPriority priority, string directory, RTorrentSettings settings);
        void RemoveTorrent(string hash, RTorrentSettings settings);
        void SetTorrentLabel(string hash, string label, RTorrentSettings settings);
        bool HasHashTorrent(string hash, RTorrentSettings settings);
        void PushTorrentUniqueView(string hash, string view, RTorrentSettings settings);
    }

    public interface IRTorrent : IXmlRpcProxy
    {
        [XmlRpcMethod("d.multicall2")]
        object[] TorrentMulticall(params string[] parameters);

        [XmlRpcMethod("load.normal")]
        int LoadNormal(string target, string data, params string[] commands);

        [XmlRpcMethod("load.start")]
        int LoadStart(string target, string data, params string[] commands);

        [XmlRpcMethod("load.raw")]
        int LoadRaw(string target, byte[] data, params string[] commands);

        [XmlRpcMethod("load.raw_start")]
        int LoadRawStart(string target, byte[] data, params string[] commands);

        [XmlRpcMethod("d.erase")]
        int Remove(string hash);

        [XmlRpcMethod("d.name")]
        string GetName(string hash);

        [XmlRpcMethod("d.custom1.set")]
        string SetLabel(string hash, string label);

        [XmlRpcMethod("d.views.push_back_unique")]
        int PushUniqueView(string hash, string view);

        [XmlRpcMethod("system.client_version")]
        string GetVersion();
    }

    public class RTorrentProxy : IRTorrentProxy
    {
        private readonly Logger _logger;

        public RTorrentProxy(Logger logger)
        {
            _logger = logger;
        }

        public string GetVersion(RTorrentSettings settings)
        {
            _logger.Debug("Executing remote method: system.client_version");

            var client = BuildClient(settings);
            var version = ExecuteRequest(() => client.GetVersion());

            return version;
        }

        public List<RTorrentTorrent> GetTorrents(RTorrentSettings settings)
        {
            _logger.Debug("Executing remote method: d.multicall2");

            var client = BuildClient(settings);
            var ret = ExecuteRequest(() => client.TorrentMulticall("",
                    "",
                    "d.name=", // string
                    "d.hash=", // string
                    "d.base_path=", // string
                    "d.custom1=", // string (label)
                    "d.size_bytes=", // long
                    "d.left_bytes=", // long
                    "d.down.rate=", // long (in bytes / s)
                    "d.ratio=", // long
                    "d.is_open=", // long
                    "d.is_active=", // long
                    "d.complete=", // long
                    "d.timestamp.finished=")); // long (unix timestamp)

            var items = new List<RTorrentTorrent>();

            foreach (object[] torrent in ret)
            {
                var labelDecoded = System.Web.HttpUtility.UrlDecode((string)torrent[3]);

                var item = new RTorrentTorrent();
                item.Name = (string)torrent[0];
                item.Hash = (string)torrent[1];
                item.Path = (string)torrent[2];
                item.Category = labelDecoded;
                item.TotalSize = (long)torrent[4];
                item.RemainingSize = (long)torrent[5];
                item.DownRate = (long)torrent[6];
                item.Ratio = (long)torrent[7];
                item.IsOpen = Convert.ToBoolean((long)torrent[8]);
                item.IsActive = Convert.ToBoolean((long)torrent[9]);
                item.IsFinished = Convert.ToBoolean((long)torrent[10]);
                item.FinishedTime = (long)torrent[11];

                items.Add(item);
            }

            return items;
        }

        public void AddTorrentFromUrl(string torrentUrl, string label, RTorrentPriority priority, string directory, RTorrentSettings settings)
        {
            var client = BuildClient(settings);
            var response = ExecuteRequest(() =>
            {
                if (settings.AddStopped)
                {
                    _logger.Debug("Executing remote method: load.normal");
                    return client.LoadNormal("", torrentUrl, GetCommands(label, priority, directory));
                }
                else
                {
                    _logger.Debug("Executing remote method: load.start");
                    return client.LoadStart("", torrentUrl, GetCommands(label, priority, directory));
                }
            });

            if (response != 0)
            {
                throw new DownloadClientException("Could not add torrent: {0}.", torrentUrl);
            }
        }

        public void AddTorrentFromFile(string fileName, byte[] fileContent, string label, RTorrentPriority priority, string directory, RTorrentSettings settings)
        {
            var client = BuildClient(settings);
            var response = ExecuteRequest(() =>
            {
                if (settings.AddStopped)
                {
                    _logger.Debug("Executing remote method: load.raw");
                    return client.LoadRaw("", fileContent, GetCommands(label, priority, directory));
                }
                else
                {
                    _logger.Debug("Executing remote method: load.raw_start");
                    return client.LoadRawStart("", fileContent, GetCommands(label, priority, directory));
                }
            });

            if (response != 0)
            {
                throw new DownloadClientException("Could not add torrent: {0}.", fileName);
            }
        }

        public void SetTorrentLabel(string hash, string label, RTorrentSettings settings)
        {
            _logger.Debug("Executing remote method: d.custom1.set");

            var client = BuildClient(settings);
            var response = ExecuteRequest(() => client.SetLabel(hash, label));

            if (response != label)
            {
                throw new DownloadClientException("Could not set label to {1} for torrent: {0}.", hash, label);
            }
        }

        public void PushTorrentUniqueView(string hash, string view, RTorrentSettings settings)
        {
            _logger.Debug("Executing remote method: d.views.push_back_unique");

            var client = BuildClient(settings);
            var response = ExecuteRequest(() => client.PushUniqueView(hash, view));
            if (response != 0)
            {
                throw new DownloadClientException("Could not push unique view {0} for torrent: {1}.", view, hash);
            }
        }

        public void RemoveTorrent(string hash, RTorrentSettings settings)
        {
            _logger.Debug("Executing remote method: d.erase");

            var client = BuildClient(settings);
            var response = ExecuteRequest(() => client.Remove(hash));

            if (response != 0)
            {
                throw new DownloadClientException("Could not remove torrent: {0}.", hash);
            }
        }

        public bool HasHashTorrent(string hash, RTorrentSettings settings)
        {
            _logger.Debug("Executing remote method: d.name");

            var client = BuildClient(settings);

            try
            {
                var name = ExecuteRequest(() => client.GetName(hash));

                if (name.IsNullOrWhiteSpace())
                {
                    return false;
                }

                var metaTorrent = name == (hash + ".meta");

                return !metaTorrent;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string[] GetCommands(string label, RTorrentPriority priority, string directory)
        {
            var result = new List<string>();

            if (label.IsNotNullOrWhiteSpace())
            {
                result.Add("d.custom1.set=" + label);
            }

            if (priority != RTorrentPriority.Normal)
            {
                result.Add("d.priority.set=" + (int)priority);
            }

            if (directory.IsNotNullOrWhiteSpace())
            {
                result.Add("d.directory.set=" + directory);
            }

            return result.ToArray();
        }

        private IRTorrent BuildClient(RTorrentSettings settings)
        {
            var client = XmlRpcProxyGen.Create<IRTorrent>();

            client.Url = string.Format(@"{0}://{1}:{2}/{3}",
                                    settings.UseSsl ? "https" : "http",
                                    settings.Host,
                                    settings.Port,
                                    settings.UrlBase);

            client.EnableCompression = true;

            if (!settings.Username.IsNullOrWhiteSpace())
            {
                client.Credentials = new NetworkCredential(settings.Username, settings.Password);
            }

            return client;
        }

        private T ExecuteRequest<T>(Func<T> task)
        {
            try
            {
                return task();
            }
            catch (XmlRpcServerException ex)
            {
                throw new DownloadClientException("Unable to connect to rTorrent, please check your settings", ex);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.TrustFailure)
                {
                    throw new DownloadClientUnavailableException("Unable to connect to rTorrent, certificate validation failed.", ex);
                }

                throw new DownloadClientUnavailableException("Unable to connect to rTorrent, please check your settings", ex);
            }
        }
    }
}
