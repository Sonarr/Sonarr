using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using CookComputing.XmlRpc;
using NLog;

namespace NzbDrone.Core.Download.Clients.Aria2
{
    public interface IAria2Proxy
    {
        string GetVersion(Aria2Settings settings);
        string AddMagnet(Aria2Settings settings, string magnet);
        string AddTorrent(Aria2Settings settings, byte[] torrent);
        bool RemoveTorrent(Aria2Settings settings, string gid);
        bool RemoveCompletedTorrent(Aria2Settings settings, string gid);
        Dictionary<string, string> GetGlobals(Aria2Settings settings);
        List<Aria2Status> GetTorrents(Aria2Settings settings);
        Aria2Status GetFromGID(Aria2Settings settings, string gid);
    }

    public interface IAria2 : IXmlRpcProxy
    {
        [XmlRpcMethod("aria2.getVersion")]
        Aria2Version GetVersion(string token);

        [XmlRpcMethod("aria2.addUri")]
        string AddUri(string token, string[] uri);

        [XmlRpcMethod("aria2.addTorrent")]
        string AddTorrent(string token, byte[] torrent);

        [XmlRpcMethod("aria2.forceRemove")]
        string Remove(string token, string gid);

        [XmlRpcMethod("aria2.removeDownloadResult")]
        string RemoveResult(string token, string gid);

        [XmlRpcMethod("aria2.tellStatus")]
        Aria2Status GetFromGid(string token, string gid);

        [XmlRpcMethod("aria2.getGlobalOption")]
        XmlRpcStruct GetGlobalOption(string token);

        [XmlRpcMethod("aria2.tellActive")]
        Aria2Status[] GetActive(string token);

        [XmlRpcMethod("aria2.tellWaiting")]
        Aria2Status[] GetWaiting(string token, int offset, int num);

        [XmlRpcMethod("aria2.tellStopped")]
        Aria2Status[] GetStopped(string token, int offset, int num);
    }

    public class Aria2Proxy : IAria2Proxy
    {
        private readonly Logger _logger;

        public Aria2Proxy(Logger logger)
        {
            _logger = logger;
        }

        private string GetToken(Aria2Settings settings)
        {
            return $"token:{settings?.SecretToken}";
        }

        private string GetURL(Aria2Settings settings)
        {
            return $"http{(settings.UseSsl ? "s" : "")}://{settings.Host}:{settings.Port}{settings.RpcPath}";
        }

        public string GetVersion(Aria2Settings settings)
        {
            _logger.Trace("> aria2.getVersion");

            var client = BuildClient(settings);
            var version = ExecuteRequest(() => client.GetVersion(GetToken(settings)));

            _logger.Trace("< aria2.getVersion");

            return version.Version;
        }

        public Aria2Status GetFromGID(Aria2Settings settings, string gid)
        {
            _logger.Trace("> aria2.tellStatus");

            var client = BuildClient(settings);
            var found = ExecuteRequest(() => client.GetFromGid(GetToken(settings), gid));

            _logger.Trace("< aria2.tellStatus");

            return found;
        }

        public List<Aria2Status> GetTorrents(Aria2Settings settings)
        {
            _logger.Trace("> aria2.tellActive");

            var client = BuildClient(settings);

            var active = ExecuteRequest(() => client.GetActive(GetToken(settings)));

            _logger.Trace("< aria2.tellActive");

            _logger.Trace("> aria2.tellWaiting");

            var waiting = ExecuteRequest(() => client.GetWaiting(GetToken(settings), 0, 10 * 1024));

            _logger.Trace("< aria2.tellWaiting");

            _logger.Trace("> aria2.tellStopped");

            var stopped = ExecuteRequest(() => client.GetStopped(GetToken(settings), 0, 10 * 1024));

            _logger.Trace("< aria2.tellStopped");

            var items = new List<Aria2Status>();

            items.AddRange(active);
            items.AddRange(waiting);
            items.AddRange(stopped);

            return items;
        }

        public Dictionary<string, string> GetGlobals(Aria2Settings settings)
        {
            _logger.Trace("> aria2.getGlobalOption");

            var client = BuildClient(settings);
            var options = ExecuteRequest(() => client.GetGlobalOption(GetToken(settings)));

            _logger.Trace("< aria2.getGlobalOption");

            var ret = new Dictionary<string, string>();

            foreach (DictionaryEntry option in options)
            {
                ret.Add(option.Key.ToString(), option.Value?.ToString());
            }

            return ret;
        }

        public string AddMagnet(Aria2Settings settings, string magnet)
        {
            _logger.Trace("> aria2.addUri");

            var client = BuildClient(settings);
            var gid = ExecuteRequest(() => client.AddUri(GetToken(settings), new[] { magnet }));

            _logger.Trace("< aria2.addUri");

            return gid;
        }

        public string AddTorrent(Aria2Settings settings, byte[] torrent)
        {
            _logger.Trace("> aria2.addTorrent");

            var client = BuildClient(settings);
            var gid = ExecuteRequest(() => client.AddTorrent(GetToken(settings), torrent));

            _logger.Trace("< aria2.addTorrent");

            return gid;
        }

        public bool RemoveTorrent(Aria2Settings settings, string gid)
        {
            _logger.Trace("> aria2.forceRemove");

            var client = BuildClient(settings);
            var gidres = ExecuteRequest(() => client.Remove(GetToken(settings), gid));

            _logger.Trace("< aria2.forceRemove");

            return gid == gidres;
        }

        public bool RemoveCompletedTorrent(Aria2Settings settings, string gid)
        {
            _logger.Trace("> aria2.removeDownloadResult");

            var client = BuildClient(settings);
            var result = ExecuteRequest(() => client.RemoveResult(GetToken(settings), gid));

            _logger.Trace("< aria2.removeDownloadResult");

            return result == "OK";
        }

        private IAria2 BuildClient(Aria2Settings settings)
        {
            var client = XmlRpcProxyGen.Create<IAria2>();
            client.Url = GetURL(settings);

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
                throw new DownloadClientException("Unable to connect to aria2, please check your settings", ex);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.TrustFailure)
                {
                    throw new DownloadClientUnavailableException("Unable to connect to aria2, certificate validation failed.", ex);
                }

                throw new DownloadClientUnavailableException("Unable to connect to aria2, please check your settings", ex);
            }
        }
    }
}
