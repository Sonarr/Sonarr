using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Extensions;

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

    public class Aria2Proxy : IAria2Proxy
    {
        private readonly IHttpClient _httpClient;

        public Aria2Proxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string GetVersion(Aria2Settings settings)
        {
            var response = ExecuteRequest(settings, "aria2.getVersion", GetToken(settings));

            var element = response.XPathSelectElement("./methodResponse/params/param/value");

            var version = new Aria2Version(element);

            return version.Version;
        }

        public Aria2Status GetFromGID(Aria2Settings settings, string gid)
        {
            var response = ExecuteRequest(settings, "aria2.tellStatus", GetToken(settings), gid);

            var element = response.XPathSelectElement("./methodResponse/params/param/value");

            return new Aria2Status(element);
        }

        private List<Aria2Status> GetTorrentsMethod(Aria2Settings settings, string method, params object[] args)
        {
            var allArgs = new List<object> { GetToken(settings) };
            if (args.Any())
            {
                allArgs.AddRange(args);
            }

            var response = ExecuteRequest(settings, method, allArgs.ToArray());

            var element = response.XPathSelectElement("./methodResponse/params/param/value/array/data");

            var torrents = element?.Elements()
                               .Select(x => new Aria2Status(x))
                               .ToList()
                           ?? new List<Aria2Status>();
            return torrents;
        }

        public List<Aria2Status> GetTorrents(Aria2Settings settings)
        {
            var active = GetTorrentsMethod(settings, "aria2.tellActive");

            var waiting = GetTorrentsMethod(settings, "aria2.tellWaiting", 0, 10 * 1024);

            var stopped = GetTorrentsMethod(settings, "aria2.tellStopped", 0, 10 * 1024);

            var items = new List<Aria2Status>();

            items.AddRange(active);
            items.AddRange(waiting);
            items.AddRange(stopped);

            return items;
        }

        public Dictionary<string, string> GetGlobals(Aria2Settings settings)
        {
            var response = ExecuteRequest(settings, "aria2.getGlobalOption", GetToken(settings));

            var element = response.XPathSelectElement("./methodResponse/params/param/value");

            var result = new Aria2Dict(element);

            return result.Dict;
        }

        public string AddMagnet(Aria2Settings settings, string magnet)
        {
            var response = ExecuteRequest(settings, "aria2.addUri", GetToken(settings), new List<string> { magnet });

            var gid = response.GetStringResponse();

            return gid;
        }

        public string AddTorrent(Aria2Settings settings, byte[] torrent)
        {
            var response = ExecuteRequest(settings, "aria2.addTorrent", GetToken(settings), torrent);

            var gid = response.GetStringResponse();

            return gid;
        }

        public bool RemoveTorrent(Aria2Settings settings, string gid)
        {
            var response = ExecuteRequest(settings, "aria2.forceRemove", GetToken(settings), gid);

            var gidres = response.GetStringResponse();

            return gid == gidres;
        }

        public bool RemoveCompletedTorrent(Aria2Settings settings, string gid)
        {
            var response = ExecuteRequest(settings, "aria2.removeDownloadResult", GetToken(settings), gid);

            var result = response.GetStringResponse();

            return result == "OK";
        }

        private string GetToken(Aria2Settings settings)
        {
            return $"token:{settings?.SecretToken}";
        }

        private XDocument ExecuteRequest(Aria2Settings settings, string methodName, params object[] args)
        {
            var requestBuilder = new XmlRpcRequestBuilder(settings.UseSsl, settings.Host, settings.Port, settings.RpcPath)
            {
                LogResponseContent = true,
            };

            var request = requestBuilder.Call(methodName, args).Build();

            var response = _httpClient.Execute(request);

            var doc = XDocument.Parse(response.Content);

            var faultElement = doc.XPathSelectElement("./methodResponse/fault");

            if (faultElement != null)
            {
                var fault = new Aria2Fault(faultElement);

                throw new DownloadClientException($"Aria2 returned error code {fault.FaultCode}: {fault.FaultString}");
            }

            return doc;
        }
    }
}
