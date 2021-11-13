using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Extensions;

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

    public class RTorrentProxy : IRTorrentProxy
    {
        private readonly IHttpClient _httpClient;

        public RTorrentProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string GetVersion(RTorrentSettings settings)
        {
            var document = ExecuteRequest(settings, "system.client_version");

            return document.Descendants("string").FirstOrDefault()?.Value ?? "0.0.0";
        }

        public List<RTorrentTorrent> GetTorrents(RTorrentSettings settings)
        {
            var document = ExecuteRequest(settings,
                "d.multicall2",
                "",
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
                "d.complete=", //long
                "d.timestamp.finished="); // long (unix timestamp)

            var torrents = document.XPathSelectElement("./methodResponse/params/param/value/array/data")
                               ?.Elements()
                               .Select(x => new RTorrentTorrent(x))
                               .ToList()
                           ?? new List<RTorrentTorrent>();

            return torrents;
        }

        public void AddTorrentFromUrl(string torrentUrl, string label, RTorrentPriority priority, string directory, RTorrentSettings settings)
        {
            var args = new List<object> { "", torrentUrl };
            args.AddRange(GetCommands(label, priority, directory));

            XDocument response;

            if (settings.AddStopped)
            {
                response = ExecuteRequest(settings, "load.normal", args.ToArray());
            }
            else
            {
                response = ExecuteRequest(settings, "load.start", args.ToArray());
            }

            if (response.GetIntResponse() != 0)
            {
                throw new DownloadClientException("Could not add torrent: {0}.", torrentUrl);
            }
        }

        public void AddTorrentFromFile(string fileName, byte[] fileContent, string label, RTorrentPriority priority, string directory, RTorrentSettings settings)
        {
            var args = new List<object> { "", fileContent };
            args.AddRange(GetCommands(label, priority, directory));

            XDocument response;

            if (settings.AddStopped)
            {
                response = ExecuteRequest(settings, "load.raw", args.ToArray());
            }
            else
            {
                response = ExecuteRequest(settings, "load.raw_start", args.ToArray());
            }

            if (response.GetIntResponse() != 0)
            {
                throw new DownloadClientException("Could not add torrent: {0}.", fileName);
            }
        }

        public void SetTorrentLabel(string hash, string label, RTorrentSettings settings)
        {
            var response = ExecuteRequest(settings, "d.custom1.set", hash, label);

            if (response.GetStringResponse() != label)
            {
                throw new DownloadClientException("Could not set label to {1} for torrent: {0}.", hash, label);
            }
        }

        public void PushTorrentUniqueView(string hash, string view, RTorrentSettings settings)
        {
            var response = ExecuteRequest(settings, "d.views.push_back_unique", hash, view);

            if (response.GetIntResponse() != 0)
            {
                throw new DownloadClientException("Could not push unique view {0} for torrent: {1}.", view, hash);
            }
        }

        public void RemoveTorrent(string hash, RTorrentSettings settings)
        {
            var response = ExecuteRequest(settings, "d.erase", hash);

            if (response.GetIntResponse() != 0)
            {
                throw new DownloadClientException("Could not remove torrent: {0}.", hash);
            }
        }

        public bool HasHashTorrent(string hash, RTorrentSettings settings)
        {
            try
            {
                var response = ExecuteRequest(settings, "d.name", hash);
                var name = response.GetStringResponse();

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

        private XDocument ExecuteRequest(RTorrentSettings settings, string methodName, params object[] args)
        {
            var requestBuilder = new XmlRpcRequestBuilder(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase)
            {
                LogResponseContent = true,
            };

            if (!settings.Username.IsNullOrWhiteSpace())
            {
                requestBuilder.NetworkCredential = new NetworkCredential(settings.Username, settings.Password);
            }

            var request = requestBuilder.Call(methodName, args).Build();

            var response = _httpClient.Execute(request);

            var doc = XDocument.Parse(response.Content);

            var faultElement = doc.XPathSelectElement("./methodResponse/fault");

            if (faultElement != null)
            {
                var fault = new RTorrentFault(faultElement);

                throw new DownloadClientException($"rTorrent returned error code {fault.FaultCode}: {fault.FaultString}");
            }

            return doc;
        }
    }
}
