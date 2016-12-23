using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public class TorrentRssParser : RssParser
    {
        // Parse various seeder/leecher/peers formats in the description element to determine number of seeders.
        public bool ParseSeedersInDescription { get; set; }

        // Use the specified element name to determine the size
        public string SizeElementName { get; set; }

        public TorrentRssParser()
        {
            PreferredEnclosureMimeType = "application/x-bittorrent";
        }

        public IEnumerable<XElement> GetItems(IndexerResponse indexerResponse)
        {
            var document = LoadXmlDocument(indexerResponse);
            var items = GetItems(document);

            return items;
        }

        protected override ReleaseInfo CreateNewReleaseInfo()
        {
            return new TorrentInfo();
        }

        protected override ReleaseInfo ProcessItem(XElement item, ReleaseInfo releaseInfo)
        {
            var result = base.ProcessItem(item, releaseInfo) as TorrentInfo;

            result.InfoHash = GetInfoHash(item);
            result.MagnetUrl = GetMagnetUrl(item);
            result.Seeders = GetSeeders(item);
            result.Peers = GetPeers(item);

            return result;
        }

        protected virtual string GetInfoHash(XElement item)
        {
            var magnetUrl = GetMagnetUrl(item);
            if (magnetUrl.IsNotNullOrWhiteSpace())
            {
                try
                {
                    var magnetLink = new MonoTorrent.MagnetLink(magnetUrl);
                    return magnetLink.InfoHash.ToHex();
                }
                catch
                {
                }
            }

            return null;
        }

        protected virtual string GetMagnetUrl(XElement item)
        {
            var downloadUrl = GetDownloadUrl(item);
            if (downloadUrl.IsNotNullOrWhiteSpace() && downloadUrl.StartsWith("magnet:"))
            {
                return downloadUrl;
            }

            return null;
        }

        protected virtual int? GetSeeders(XElement item)
        {
            if (ParseSeedersInDescription && item.Element("description") != null)
            {
                var matchSeeders = ParseSeedersRegex.Match(item.Element("description").Value);

                if (matchSeeders.Success)
                {
                    return int.Parse(matchSeeders.Groups["value"].Value);
                }

                var matchPeers = ParsePeersRegex.Match(item.Element("description").Value);
                var matchLeechers = ParseLeechersRegex.Match(item.Element("description").Value);

                if (matchPeers.Success && matchLeechers.Success)
                {
                    return int.Parse(matchPeers.Groups["value"].Value) - int.Parse(matchLeechers.Groups["value"].Value);
                }
            }

            return null;
        }

        protected virtual int? GetPeers(XElement item)
        {
            if (ParseSeedersInDescription && item.Element("description") != null)
            {
                var matchPeers = ParsePeersRegex.Match(item.Element("description").Value);

                if (matchPeers.Success)
                {
                    return int.Parse(matchPeers.Groups["value"].Value);
                }

                var matchSeeders = ParseSeedersRegex.Match(item.Element("description").Value);
                var matchLeechers = ParseLeechersRegex.Match(item.Element("description").Value);

                if (matchSeeders.Success && matchLeechers.Success)
                {
                    return int.Parse(matchSeeders.Groups["value"].Value) + int.Parse(matchLeechers.Groups["value"].Value);
                }
            }

            return null;
        }

        protected override long GetSize(XElement item)
        {
            var size = base.GetSize(item);
            if (size == 0 && SizeElementName.IsNotNullOrWhiteSpace())
            {
                if (item.Element(SizeElementName) != null)
                {
                    size = ParseSize(item.Element(SizeElementName).Value, true);
                }
            }

            return size;
        }

        private static readonly Regex ParseSeedersRegex = new Regex(@"(Seeder)s?:\s+(?<value>\d+)|(?<value>\d+)\s+(seeder)s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ParseLeechersRegex = new Regex(@"(Leecher)s?:\s+(?<value>\d+)|(?<value>\d+)\s+(leecher)s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ParsePeersRegex = new Regex(@"(Peer)s?:\s+(?<value>\d+)|(?<value>\d+)\s+(peer)s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
