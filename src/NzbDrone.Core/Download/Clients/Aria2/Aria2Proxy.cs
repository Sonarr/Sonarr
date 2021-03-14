using System;
using System.Net;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NLog;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Serializer;
using CookComputing.XmlRpc;
using System.Collections;

namespace NzbDrone.Core.Download.Clients.Aria2
{
    public interface IAria2Proxy
    {
        string GetVersion(Aria2Settings settings);

        string AddMagnet(Aria2Settings settings, string magnet);
        string AddTorrent(Aria2Settings settings, byte[] torrent);
        bool RemoveTorrent(Aria2Settings settings, string gid);
        Dictionary<string, string> GetGlobals(Aria2Settings settings);
        Aria2Status[] GetStatuses(Aria2Settings settings);
        Aria2Status GetFromGID(Aria2Settings settings, string gid);
    }

    public interface IAria2 : IXmlRpcProxy
    {
        [XmlRpcMethod("aria2.getVersion")]
        Aria2Version GetVersion(string token);

        [XmlRpcMethod("aria2.addUri")]
        string AddUri(string token, string[] uri);

        [XmlRpcMethod("aria2.addTorrent")]
        string AddTorrent(string token, string base64Torrent);

        [XmlRpcMethod("aria2.remove")]
        string Remove(string token, string gid);

        [XmlRpcMethod("aria2.tellStatus")]
        Aria2Status GetFromGid(string token, string gid);

        [XmlRpcMethod("aria2.getGlobalOption")]
        XmlRpcStruct GetGlobalOption(string token);

        [XmlRpcMethod("aria2.tellActive")]
        Aria2Status[] GetActives(string token);

        [XmlRpcMethod("aria2.tellWaiting")]
        Aria2Status[] GetWaitings(string token, int offset, int num);

        [XmlRpcMethod("aria2.tellStopped")]
        Aria2Status[] GetStoppeds(string token, int offset, int num); 
    }

    public class Aria2Proxy : IAria2Proxy
    {
        private readonly Logger _logger;

        public Aria2Proxy(Logger logger)
        {
            _logger = logger;
        }

        public string GetVersion(Aria2Settings settings)
        {
            _logger.Debug("> aria2.getVersion");

            var client = BuildClient(settings);
            var version = ExecuteRequest(() => client.GetVersion(settings.RPCToken));

            _logger.Debug("< aria2.getVersion");

            return version.version;
        }

        public Aria2Status GetFromGID(Aria2Settings settings, string gid)
        {
            _logger.Debug("> aria2.tellStatus");

            var client = BuildClient(settings);
            var found = ExecuteRequest(() => client.GetFromGid(settings.RPCToken, gid));

            _logger.Debug("< aria2.tellStatus");

            return found;
        }


        public Aria2Status[] GetStatuses(Aria2Settings settings)
        {
            _logger.Debug("> aria2.tellActive");

            var client = BuildClient(settings);

            var actives = ExecuteRequest(() => client.GetActives(settings.RPCToken));

            _logger.Debug("< aria2.tellActive");

            _logger.Debug("> aria2.tellWaiting");

            var waitings = ExecuteRequest(() => client.GetWaitings(settings.RPCToken, 1, 10*1024));

            _logger.Debug("< aria2.tellWaiting");

            _logger.Debug("> aria2.tellStopped");

            var stoppeds = ExecuteRequest(() => client.GetStoppeds(settings.RPCToken, 1, 10*1024));

            _logger.Debug("< aria2.tellStopped");

            var ret = new List<Aria2Status>();

            ret.AddRange(actives);
            ret.AddRange(waitings);
            ret.AddRange(stoppeds);
            
            return ret.ToArray();
        }

        public Dictionary<string, string> GetGlobals(Aria2Settings settings)
        {
            _logger.Debug("> aria2.getGlobalOption");

            var client = BuildClient(settings);
            var options = ExecuteRequest(() => client.GetGlobalOption(settings.RPCToken));

            _logger.Debug("< aria2.getGlobalOption");

            var ret = new Dictionary<string, string>();

            foreach(DictionaryEntry option in options)
            {                
                ret.Add(option.Key.ToString(), option.Value?.ToString());
            }

            return ret;
        }

        public string AddMagnet(Aria2Settings settings, string magnet)
        {
            _logger.Debug("> aria2.addUri");

            var client = BuildClient(settings);
            var gid = ExecuteRequest(() => client.AddUri(settings.RPCToken, new[] { magnet }));

            _logger.Debug("< aria2.addUri");

            return gid;
        }

        public string AddTorrent(Aria2Settings settings, byte[] torrent)
        {
            _logger.Debug("> aria2.addTorrent");

            var client = BuildClient(settings);
            var gid = ExecuteRequest(() => client.AddTorrent(settings.RPCToken, Convert.ToBase64String(torrent)));

            _logger.Debug("< aria2.addTorrent");

            return gid;
        }

        public bool RemoveTorrent(Aria2Settings settings, string gid)
        {
            _logger.Debug("> aria2.remove");

            var client = BuildClient(settings);
            var gidres = ExecuteRequest(() => client.Remove(settings.RPCToken, gid));

            _logger.Debug("< aria2.remove");

            return gid == gidres;
        }

        private IAria2 BuildClient(Aria2Settings settings)
        {
            var client = XmlRpcProxyGen.Create<IAria2>();
            client.Url = settings.URL;
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