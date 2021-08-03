using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MonoTorrent;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public class TorrentRssParser : RssParser
    {
        // Use to sum/calculate Peers as Leechers+Seeders
        public bool CalculatePeersAsSum { get; set; } = false;

        // Use the specified element name to determine the Infohash
        public string InfoHashElementName { get; set; }

        // Parse various seeder/leecher/peers formats in the description element to determine number of seeders.
        public bool ParseSeedersInDescription { get; set; }

        // Use the specified element name to determine the Peers
        public string PeersElementName { get; set; }

        // Use the specified element name to determine the Seeds
        public string SeedsElementName { get; set; }

        // Use the specified element name to determine the Size
        public string SizeElementName { get; set; }

        // Use the specified element name to determine the Magnet link
        public string MagnetElementName { get; set; }

        public TorrentRssParser()
        {
            PreferredEnclosureMimeTypes = TorrentEnclosureMimeTypes;
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

            if (CalculatePeersAsSum)
            {
                result.Peers = GetPeers(item) + result.Seeders;
            }
            else
            {
                result.Peers = GetPeers(item);
            }

            return result;
        }

        protected virtual string GetInfoHash(XElement item)
        {
            if (InfoHashElementName.IsNotNullOrWhiteSpace())
            {
                return item.FindDecendants(InfoHashElementName).FirstOrDefault().Value;
            }

            var magnetUrl = GetMagnetUrl(item);

            if (magnetUrl.IsNotNullOrWhiteSpace())
            {
                try
                {
                    return MagnetLink.Parse(magnetUrl).InfoHash.ToHex();
                }
                catch
                {
                }
            }

            return null;
        }

        protected virtual string GetMagnetUrl(XElement item)
        {
            if (MagnetElementName.IsNotNullOrWhiteSpace())
            {
                var magnetURL = item.FindDecendants(MagnetElementName).FirstOrDefault().Value;
                if (magnetURL.IsNotNullOrWhiteSpace() && magnetURL.StartsWith("magnet:"))
                {
                    return magnetURL;
                }
            }
            else
            {
                var downloadUrl = GetDownloadUrl(item);
                if (downloadUrl.IsNotNullOrWhiteSpace() && downloadUrl.StartsWith("magnet:"))
                {
                    return downloadUrl;
                }
            }

            return null;
        }

        protected virtual int? GetSeeders(XElement item)
        {
            // safe to always use the element if it's present (and valid)
            // fall back to description if ParseSeedersInDescription is enabled

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

            var seeds = item.FindDecendants(SeedsElementName).SingleOrDefault();
            if (seeds != null)
            {
                return (int)seeds;
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

            if (PeersElementName.IsNotNullOrWhiteSpace())
            {
                var itempeers = item.FindDecendants(PeersElementName).SingleOrDefault();
                return int.Parse(itempeers.Value);
            }

            return null;
        }

        protected override long GetSize(XElement item)
        {
            var size = base.GetSize(item);
            if (size == 0 && SizeElementName.IsNotNullOrWhiteSpace())
            {
                var itemsize = item.FindDecendants(SizeElementName).SingleOrDefault();
                if (itemsize != null)
                {
                    size = ParseSize(itemsize.Value, true);
                }
            }

            return size;
        }

        private static readonly Regex ParseSeedersRegex = new Regex(@"(Seeder)s?:\s+(?<value>\d+)|(?<value>\d+)\s+(seeder)s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ParseLeechersRegex = new Regex(@"(Leecher)s?:\s+(?<value>\d+)|(?<value>\d+)\s+(leecher)s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ParsePeersRegex = new Regex(@"(Peer)s?:\s+(?<value>\d+)|(?<value>\d+)\s+(peer)s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
